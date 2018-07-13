using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RestSharp;
using UniprotDistributedServer.Models;
using Newtonsoft.Json;

namespace UniprotDistributedServer.Controllers
{
    [Route("master")]
    ///This method can catch data from SQL
    public class ValuesController : Controller
    {
        private IConfiguration _configuration;

        public ValuesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("available")]
        //Checking if the master is available
        public bool Availability()
        {
            return true;
        }

        // GET api/values
        [HttpGet]
        [Route("get")]
        public IEnumerable<string> Get(string sql)
        {
            BaseDataAccess DataBase = new BaseDataAccess(_configuration.GetConnectionString("DefaultConnection"));

            List<DbParameter> parameterList = new List<DbParameter>();
            List<string> Result = new List<string>();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            using (DbDataReader dataReader = DataBase.ExecuteDataReader(sql, parameterList))
            {
                stopwatch.Stop();
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        Result.Add(dataReader[0].ToString());
                    }
                }

                //foreach(DataRow row in data.Tables[0].Rows)
                //{
                //    Console.WriteLine(row[0]);
                //}
            }

            return Result;
            //return new string[] { sql, "value1", "value2" };
        }

        [HttpGet]
        [Route("info")]
        //Info controller for checking the status
        public string Info()
        {
            if (Startup.taskList.Count > 0)
            {
                Models.Task task = Startup.taskList[0];
                if (task != null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        status = task.Status,
                        time = DateTime.Now,

                        s_current = task.s_current,
                        s_total = task.s_total,
                        s_status = task.s_status,
                        splitDone = task.splitDone,
                        s_time = (task.s_end.Subtract(task.s_start)).ToString(),

                        b_current = task.b_current,
                        b_total = task.b_total,
                        b_status = task.b_status,
                        broadcastDone = task.broadcastDone,
                        b_time = (task.b_end.Subtract(task.b_start)).ToString(),

                        blk_current = task.blk_current,
                        blk_total = task.blk_total,
                        blk_status = task.blk_status,
                        bulkDone = task.bulkDone,
                        blk_time = (task.blk_end.Subtract(task.blk_start)).ToString(),

                        details = task.details
                    });
                }
            }

            return JsonConvert.SerializeObject(new
            {
                status = "no tasks running",
                time = DateTime.Now
            });
        }

        [HttpGet]
        [Route("check_bulk_status")]
        public async Task<List<string>> Bulk_info()
        {
            //Checking all slaves bulk status
            List<string> slaveInfo = new List<string>();
            foreach (Servers server in Program.Servers)
            {
                HttpClient client = new HttpClient();

                try
                {
                    HttpResponseMessage response = await client.GetAsync(server.api_call + "/slave/check_bulk");
                    if (response.IsSuccessStatusCode)
                    {
                        Stream receiveStream = await response.Content.ReadAsStreamAsync();
                        StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                        string result = readStream.ReadToEnd();
                        slaveInfo.Add(server.api_call + ": " + result);
                    }
                    else slaveInfo.Add(server.api_call + ": Slave Not Running");
                }
                catch (Exception)
                {
                    slaveInfo.Add(server.api_call + ": Slave Not Running");
                }
            }

            return slaveInfo;
        }

        [HttpGet]
        [Route("start_bulk")]
        //Start bulk on manually
        public async Task<string> Start_Bulk(string slave)
        {
            HttpClient client = new HttpClient();

            try
            {
                HttpResponseMessage response = await client.GetAsync(slave + "/slave/start_bulk");
                if (response.IsSuccessStatusCode)
                {
                    Stream receiveStream = await response.Content.ReadAsStreamAsync();
                    StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                    string result = readStream.ReadToEnd();
                    return (slave + ": " + result);
                }
                else return (slave + ": Error, Slave Not Running. Activate the slave and then Try activating the bulk load manually with /master/start_bulk?slave={host}:{port}");
            }
            catch (Exception)
            {
                return (slave + ": Error, Slave Not Running. Activate the slave and then Try activating the bulk load manually with /master/start_bulk?slave={host}:{port}");
            }
        }

        [HttpGet]
        [Route("check_slaves")]
        public async Task<List<string>> CheckSlaves()
        {
            //Checking if all slaves are running
            List<string> slaveInfo = new List<string>();
            foreach (Servers server in Program.Servers)
            {
                HttpClient client = new HttpClient();

                try
                {
                    HttpResponseMessage response = await client.GetAsync(server.api_call + "/slave/check_wd");
                    if (response.IsSuccessStatusCode)
                    {
                        Stream receiveStream = await response.Content.ReadAsStreamAsync();
                        StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                        string result = readStream.ReadToEnd();
                        slaveInfo.Add(server.api_call + ": Running, working directory exists: " + result);
                    }
                    else slaveInfo.Add(server.api_call + ": Not Running");
                }
                catch (Exception)
                {
                    slaveInfo.Add(server.api_call + ": Not Running");
                }
            }

            return slaveInfo;
        }
        
        [HttpGet]
        [Route("load")]
        //Loading controller
        public async Task<string> Load(string path)
        {
            //Check number 1 --> Correct load query
            if (path == null)
            {
                return JsonConvert.SerializeObject(new
                {
                    status = "Please provide the source file in path variable.",
                    success = false,
                    time = DateTime.Now
                });
            }
            string sourceFile = path;

            //Check number 2 --> Is the load already running
            if (Startup.taskList.Count >= 1)
            {
                return JsonConvert.SerializeObject(new
                {
                    status = "Load is already running.",
                    success = false,
                    time = DateTime.Now
                });
            }

            //Check number 3 --> Are all slaves running
            List<string> slaveInfo = new List<string>();
            foreach (Servers server in Program.Servers)
            {
                HttpClient client = new HttpClient();

                try
                {
                    HttpResponseMessage response = await client.GetAsync(server.api_call + "/slave/available");
                    if (response.IsSuccessStatusCode)
                    {
                        Stream receiveStream = await response.Content.ReadAsStreamAsync();
                        StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                        string result = readStream.ReadToEnd();
                        if (result.Equals("true")) continue;
                    }
                    else return JsonConvert.SerializeObject(new
                    {
                        status = "Not all slaves are running. Check slaves first.",
                        success = false,
                        time = DateTime.Now
                    });
                }
                catch (Exception)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        status = "Not all slaves are running. Check slaves first.",
                        success = false,
                        time = DateTime.Now
                    });
                } 
            }

            //Check number 3 --> Check if the file exists
            if (Int32.Parse(ShellHelper.Bash("test -e " + path + " && echo 1 || echo 0")) == 0)
            {
                return JsonConvert.SerializeObject(new
                {
                    status = "File does not exist. Please enter correct existing file name.",
                    success = false,
                    time = DateTime.Now
                });
            } else
            {
                string workingDirectory = String.Join('/', sourceFile.Split('/').Take(sourceFile.Split('/').Length - 1)) + '/';

                //return ShellHelper.Bash("test -e " + path + " && echo 1 || echo 0");
                //return "Working Directory: " + workingDirectory + "\nSource file: " + sourceFile;

                Models.Task task = new Models.Task();
                task.Thread = new Thread(() => Loader(task, sourceFile, workingDirectory));
                task.Thread.Start();
                Startup.taskList.Add(task);

                return JsonConvert.SerializeObject(new
                {
                    status = "started",
                    success = true,
                    time = DateTime.Now
                });
            }
        }

        //Thread function that checks split info
        public void checkSplit(string workingDirectory, string sourceFile, Models.Task task)
        {
            string sourceSize = ShellHelper.Bash("du -h -k " + sourceFile).Split('\t')[0];
            int sourceSizeGB = Int32.Parse(sourceSize)/1024;

            string folderSize = ShellHelper.Bash("du -sh -k " + workingDirectory + "Run/").Split('\t')[0];
            int folderSizeGB = Int32.Parse(folderSize)/1024;

            while (!task.splitDone)
            {
                task.s_status = "Splitting file into pieces. Current size: " + folderSizeGB + "KB of " + sourceSizeGB + "KB";
                task.s_current = folderSizeGB;
                task.s_total = sourceSizeGB;
                Thread.Sleep(2000);

                sourceSize = ShellHelper.Bash("du -h -k " + sourceFile).Split('\t')[0];
                sourceSizeGB = Int32.Parse(sourceSize) / 1024;

                folderSize = ShellHelper.Bash("du -sh -k " + workingDirectory + "Run/").Split('\t')[0];
                folderSizeGB = Int32.Parse(folderSize) / 1024;
            }
        }

        /// <summary>
        /// Process for Loading the data
        /// </summary>
        private async void Loader(Models.Task task, string sourceFile, string workingDirectory)
        {
            Stopwatch stopwatch = new Stopwatch();
            List<string> TimeStatistics = new List<string>();
            List<int> values = Program.values;

            task.splitDone = false;
            task.broadcastDone = false;
            task.bulkDone = false;

            string name = sourceFile.Split('/')[sourceFile.Split('/').Length - 1].Split('.')[0];

            #region Delete 'log.txt' if exists
            if (System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + "log" + name + ".txt"))
            {
                System.IO.File.Delete(AppDomain.CurrentDomain.BaseDirectory + "log" + name + ".txt");
            }
            #endregion

            #region Delete 'time_log.txt' if exists
            if (System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + "time_log" + name + ".txt"))
            {
                System.IO.File.Delete(AppDomain.CurrentDomain.BaseDirectory + "time_log" + name + ".txt");
            }
            #endregion

            using (StreamWriter sw = System.IO.File.CreateText(AppDomain.CurrentDomain.BaseDirectory + "log" + name + ".txt"))
            {
                sw.WriteLine(DateTime.Now + " ::: Mater Load Started");
            }

            TimeStatistics.Add("\n\nNew measure: \n");
            stopwatch.Start();

            #region Split the file into pieces
            task.Status = "split";
            task.s_start = DateTime.Now;

            //Thread for checking split status
            Thread split_checker = new Thread(() => checkSplit(workingDirectory, sourceFile, task));
            split_checker.Start();

            //Setting and executing the SPLIT command to execute
            //Everything is splited into pieces inside of sourcefile directory ~ workingdirectory/Run/
            if (Int32.Parse(ShellHelper.Bash("test -e " + workingDirectory + "Run/ && echo 1 || echo 0")) == 1)
            {
                ShellHelper.Bash("rm -r " + workingDirectory + "Run/");
            }
            ShellHelper.Bash("echo tijan99 | mkdir " + workingDirectory + "Run/");
            string splitBash = "echo tijan99 | split -l 100000 --additional-suffix=.csv " + sourceFile + " " + workingDirectory + "Run/";
            task.details = ShellHelper.Bash(splitBash);

            //Aborting split thread work after it's done
            task.s_status = "Done";
            task.splitDone = true;
            task.s_end = DateTime.Now;

            TimeStatistics.Add(DateTime.Now + ": Splitting the file into 100 000 line ones: " + stopwatch.Elapsed);
            stopwatch.Restart();
            #endregion

            #region Broadcasting the files
            task.Status = "broadcast";
            task.b_start = DateTime.Now;
            //Reading the new files one by one and doing stuff depending on MASTER/SLAVE
            //Now it reads all the files from ~ workingdirectory/Run/
            string[] files = Directory.GetFiles(workingDirectory + "Run/");

            int counter = 1;
            foreach (string file in files)
            {
                Random r = new Random();
                int randomNumber = r.Next(0, values.Count);
                task.b_status = "Broadcasting file " + counter + " of " + files.Length;
                task.b_current = counter;
                task.b_total = files.Length;

                //The values[randomNumber] is allways a number between 0 (first server from configuration table) and max (last server from configuration table)
                //The number will allways be in that scope so that is not a problem!
                //Now we just send the file to the adress from the Program.Servers list (the value[randomNumber] will determine which one from the table is the destination! 

                using (StreamWriter sw = System.IO.File.AppendText(AppDomain.CurrentDomain.BaseDirectory + "log" + name + ".txt"))
                {
                    List<string> result = await Sender2(task, Program.Servers[values[randomNumber]].api_call, "/slave/recieve", file.Split('/')[file.Split('/').Length - 1], counter, file);
                    task.details = string.Join('\n', result);
                    foreach (string line in result)
                    {
                        //Logging the output
                        sw.WriteLine(line);
                    }
                }
                counter++;
            }
            

            TimeStatistics.Add(DateTime.Now + ": Broadcasting the Files: " + stopwatch.Elapsed);
            stopwatch.Restart();
            task.broadcastDone = true;
            task.b_status = "Done";
            task.b_end = DateTime.Now;

            #endregion

            #region Deleting the /Run folder
            //With one single bash line
            ShellHelper.Bash("rm -r " + workingDirectory + "Run/");
            #endregion

            


            #region Bulk Insert Activation
            //Activating the bulk insert
            task.Status = "bulk";
            task.blk_start = DateTime.Now;
            List<string> slaveInfo = new List<string>();
            foreach (Servers server in Program.Servers)
            {
                HttpClient client = new HttpClient();

                try
                {
                    HttpResponseMessage response = await client.GetAsync(server.api_call + "/slave/start_bulk");
                    if (response.IsSuccessStatusCode)
                    {
                        Stream receiveStream = await response.Content.ReadAsStreamAsync();
                        StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                        string result = readStream.ReadToEnd();
                        slaveInfo.Add(server.api_call + " :" + result + ". Check it with /master/check_bulk_status");
                    }
                    else slaveInfo.Add(server.api_call + " - Error, Slave Not Running, Try activating it manually with /master/start_bulk?slave={host}:{port}");
                }
                catch (Exception)
                {
                    slaveInfo.Add(server.api_call + " - - Error, Slave Not Running, Try activating it manually with /master/start_bulk?slave={host}:{port}");
                }
            }

            //Constantly repeat check! (Every 2 secs)
            while (!task.bulkDone)
            {
                int sumcurrent = 0;
                int sumtotal = 0;
                string status = "";
                bool done = true;
                string details = "";

                foreach(Servers server in Program.Servers)
                {
                    using (var client = new HttpClient())
                    {
                        try
                        {
                            HttpResponseMessage response = await client.GetAsync(server.api_call + "/slave/check_bulk");
                            if (response.IsSuccessStatusCode)
                            {
                                Stream receiveStream = await response.Content.ReadAsStreamAsync();
                                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                                string result = readStream.ReadToEnd();
                                var res = JsonConvert.DeserializeObject<BulkModel>(result);
                                status += res.status + "/";
                                sumcurrent += res.current;
                                sumtotal += res.total;
                                done = done && res.done;
                                details += "Server n: " + res.details;
                            }
                            status = "Check if the slave is running";
                        }
                        catch (Exception)
                        {
                            status = "Check if the slave is running";
                        }
                    }
                }

                task.blk_current = sumcurrent;
                task.blk_total = sumtotal;
                task.blk_status = "Bulk importing " + sumcurrent + " of " + sumtotal;
                task.bulkDone = done;
                task.details = details;
                task.blk_end = DateTime.Now;
                Thread.Sleep(2000);
            }

            //Kill all tasks on servers (after 10 secs)
            Thread.Sleep(10000);
            foreach (Servers server in Program.Servers)
            {
                using (var client = new HttpClient())
                {
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(server.api_call + "/slave/kill_task");
                        if (response.IsSuccessStatusCode)
                        {
                            Stream receiveStream = await response.Content.ReadAsStreamAsync();
                            StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                            string result = readStream.ReadToEnd();
                            task.details = result;
                        }
                        task.details = "Check if the slave is running";
                    }
                    catch (Exception)
                    {
                        task.details = "Check if the slave is running";
                    }
                }
            }

            //Bulk done
            task.blk_status = "Done";

            TimeStatistics.Add(DateTime.Now + ": Activating the bulk insertions: " + stopwatch.Elapsed);
            stopwatch.Stop();
            #endregion

            #region Log the Time stats
            task.Status = "time";
            //Writing time stats to log file
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "time_log" + name + ".txt", true))
            {
                foreach (string line in TimeStatistics)
                {
                    file.WriteLine(line);
                }
            }
            #endregion

            task.Status = "finished";
            Thread.Sleep(60000);

            //60 seconds the task is still active so the user can see the "Load finished" information before the task is killed.
            Startup.taskList.Remove(task);
        }

        //Thread method for sending file -> restsharp
        private List<string> Sender(Models.Task task, string slave, string controller, string fileName, int id, string path)
        {
            List<string> log = new List<string>();

            log.Add(id + ": -----------------------------------------------------------NEW SENDER");
            log.Add(DateTime.Now + ": START");
            log.Add(DateTime.Now + ": Setting up the request");
            log.Add(DateTime.Now + ": Filename: " + fileName);
            log.Add(DateTime.Now + ": Path: " + path);
            log.Add(DateTime.Now + ": Slave: " + slave);
            log.Add(DateTime.Now + ": Controller: " + controller);

            // Setting the client and request
            var client = new RestClient(slave);
            var request = new RestRequest(controller, Method.POST);
            //request.AddParameter("name", fileName); // adds to POST or URL querystring based on Method
            //request.AddUrlSegment("id", id); // replaces matching token in request.Resource

            // easily add HTTP Headers
            request.AddHeader("file-name", fileName);

            // add files to upload (works with compatible verbs)
            request.AddFile(fileName, path);
            log.Add(DateTime.Now + ": Request: " + request.ToString());

            // execute the request
            log.Add(DateTime.Now + ": EXECUTION");

            IRestResponse response = client.Execute(request);
            var content = response.Content;
             // raw content as string

            log.Add(DateTime.Now + ": Response: " + content);
            log.Add(DateTime.Now + ": DONE\n");

            return log;
        }

        //Thread method for sending file -> httpclient
        private async Task<List<string>> Sender2(Models.Task task, string slave, string controller, string fileName, int id, string path)
        {
            List<string> log = new List<string>();

            log.Add(id + ": -----------------------------------------------------------NEW SENDER");
            log.Add(DateTime.Now + ": START");
            log.Add(DateTime.Now + ": Setting up the request");
            log.Add(DateTime.Now + ": Filename: " + fileName);
            log.Add(DateTime.Now + ": Path: " + path);
            log.Add(DateTime.Now + ": Slave: " + slave);
            log.Add(DateTime.Now + ": Controller: " + controller);

            string result = "No result";

            // Setting the client and request
            using (var client = new HttpClient())
            {
                using (var content =
                    new MultipartFormDataContent())
                {
                    var stream = new FileStream(path, FileMode.Open);
                    content.Add(new StreamContent(new FileStream(path, FileMode.Open)));
                    content.Headers.Add("file-name", fileName);

                    using (
                       var message =
                           await client.PostAsync(slave + controller, content))
                    {
                        var input = await message.Content.ReadAsStringAsync();

                        result = !string.IsNullOrWhiteSpace(input) ? input : null;
                    }
                }
            }

            // execute the request
            log.Add(DateTime.Now + ": EXECUTION");
            log.Add(DateTime.Now + ": Response: " + result);
            log.Add(DateTime.Now + ": DONE\n");

            return log;
        }

        #region TEST
        [HttpGet]
        [Route("test")]
        public string testing()
        {
            Thread t = new Thread(() => tester(null));
            t.Start();
            return "Zaprimljeno";
        }

        private void tester(Models.Task task)
        {
            //Koja se ne spaja na bazu ni nipt
            task.Status = "Prvi dio";
           
            //status = "Started";
            Thread.Sleep(5000);
            task.Status = "Drugi dio";
            //status = "Running";
            Thread.Sleep(5000);
            task.Status = "Treći";
            //status = "Lelelelele";
            Thread.Sleep(5000);
            task.Status = "Četvrtiiii";
            //status = "Skoro gotovo";
            Thread.Sleep(5000);
            //status = "Gotovo jebote život!";
            task.Status = "Gotovo";
            Thread.Sleep(10000);
            Startup.taskList.Remove(task);
        }
        #endregion

    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace UniprotDistributedServer.Controllers
{
    [Route("api")]
    ///This method can catch data from SQL
    public class ValuesController : Controller
    {
        private IConfiguration _configuration;

        public ValuesController(IConfiguration configuration)
        {
            _configuration = configuration;
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
        public string Info(string guid)
        {
            if (Startup.taskList.Count > 0)
            {
                Models.Task task = Startup.taskList[0];
                if (task != null)
                {
                    return task.Status;
                }
            }
            
            return DateTime.Now + ": No tasks running.";
        }
        
        [HttpGet]
        [Route("load")]
        public string Load(string path)
        {
            if (path == null) return "";

            if (Startup.taskList.Count >= 1)
            {
                return DateTime.Now + ": Load already running.\n" + Startup.taskList[0].Status;
            }

            string sourceFile = path;

            //Check if the file exists
            if (ShellHelper.Bash("test -e " + path + " && echo 1 || echo 0").Equals("0"))
            {
                return DateTime.Now + ": File does not exist";
            } else
            {
                string workingDirectory = String.Join('/', sourceFile.Split('/').Take(sourceFile.Split('/').Length - 1)) + '/';

                return "Working Directory: " + workingDirectory + "\nSource file: " + sourceFile;

                Models.Task task = new Models.Task();
                //task.Thread = new Thread(() => Loader(task, sourceFile, workingDirectory));
                //task.Thread.Start();
                Startup.taskList.Add(task);

                return task.Status;
            }
        }

        /// <summary>
        /// Process for Loading the data
        /// </summary>
        private void Loader(Models.Task task, string sourceFile, string workingDirectory)
        {
            Stopwatch stopwatch = new Stopwatch();
            List<string> TimeStatistics = new List<string>();
            int[] values = { '1', '1', '1', '1', '1', '2', '2' };

            TimeStatistics.Add("\n\nNew measure: \n");
            stopwatch.Start();

            #region Split the file into pieces
            //Setting and executing the SPLIT command to execute
            //status = "Splitting into pieces";
            string splitBash = "split -l 100000 --additional-suffix=.csv " + sourceFile + " " + workingDirectory;
            ShellHelper.Bash(splitBash);

            TimeStatistics.Add("Splitting the file into 100 000 line ones: " + stopwatch.Elapsed);
            stopwatch.Restart();
            #endregion

            #region Broadcasting the files
            //Reading the new files one by one and doing stuff depending on MASTER/SLAVE
            //status = "Broadcasting the files";
            string[] files = Directory.GetFiles(workingDirectory);

            ShellHelper.Bash("mkdir " + workingDirectory + "Run/");

            foreach (string file in files)
            {
                Random r = new Random();
                int randomNumber = r.Next(0, values.Length);

                if (values[randomNumber] == 1)
                {
                    //Copy file to Run/ directory
                    ShellHelper.Bash("cp " + file + " " + workingDirectory + "Run/");

                }
                else if (values[randomNumber] == 2)
                {
                    //Else send a HTTP POST request
                    //Upload("proteinreader.bioinfo.pbf.hr/api/load", param, stream, Bytes);
                }
            }

            TimeStatistics.Add("Broadcasting the Files: " + stopwatch.Elapsed);
            stopwatch.Stop();
            #endregion

            #region Bulk load
            //status = "Bulk load";
            //Run bulk load
            #endregion

            #region Log the Time stats
            //Writing time stats to log file
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(workingDirectory + "log.txt", true))
            {
                foreach (string line in TimeStatistics)
                {
                    file.WriteLine(line);
                }
            }
            #endregion

            //status = "Load finished";
        }


        //private async Task<System.IO.Stream> Upload(string actionUrl, string paramString, Stream paramFileStream, byte[] paramFileBytes)
        //{
        //    HttpContent stringContent = new StringContent(paramString);
        //    HttpContent fileStreamContent = new StreamContent(paramFileStream);
        //    HttpContent bytesContent = new ByteArrayContent(paramFileBytes);
        //    var client = new HttpClient();
        //    using (var formData = new MultipartFormDataContent())
        //    {
        //        formData.Add(stringContent, "param1", "param1");
        //        formData.Add(fileStreamContent, "file1", "file1");
        //        formData.Add(bytesContent, "file2", "file2");
        //        var response = await client.PostAsync(actionUrl, formData);
        //        if (!response.IsSuccessStatusCode)
        //        {
        //            return null;
        //        }
        //        return await response.Content.ReadAsStreamAsync();
        //    }
        //}

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

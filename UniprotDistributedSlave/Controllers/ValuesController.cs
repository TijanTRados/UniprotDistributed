using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Web;
using RestSharp;
using RestSharp.Extensions;
using System.Threading;

namespace UniprotDistributedSlave.Controllers
{
    [Route("slave")]
    public class ValuesController : Controller
    {
        //Method for checking if the server is available
        [HttpGet]
        [Route("available")]
        public bool Get()
        {
            return true;
        }

        [HttpGet]
        [Route("check_wd")]
        public bool CheckWorkingDirectory()
        {
            string directory = Program.myWorkingDirectory;
            Console.WriteLine(directory);

            if (Directory.Exists(directory))
            {
                return true;
            }
            else return false;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "slave1";
        }

        // POST api/recieve
        //Method that saves the file from the request body to folder - copy with the correct name!
        [HttpPost]
        [Route("recieve")]
        public string Copy()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                try
                {
                    //Skipping first 4 lines
                    List<string> fileLines = reader.ReadToEnd().Split('\n').ToList();

                    ////TRY TO MANAGE START OF EACH LINE
                    fileLines = fileLines.Select(x => "0\t" + x).ToList();

                    fileLines.RemoveRange(0, 3);
                    fileLines.RemoveRange(fileLines.Count - 3, 2);

                    System.IO.File.WriteAllText(Program.myWorkingDirectory + Request.Headers["file-name"], string.Join('\n', fileLines));
                    return $"Saved to " + Program.myWorkingDirectory + Request.Headers["file-name"];
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An exception occured while saving " + Request.Headers["file-name"] + ". Original message: " + ex.Message);
                    return "An exception occured while saving " + Request.Headers["file-name"] + ". Original message: " + ex.Message;
                }
                
            }
        }

        [HttpGet]
        [Route("start_bulk")]
        public string Bulk()
        {
            //Checking if the bulk is already active
            if (Program.taskList.Count >= 1)
            {
                return DateTime.Now + ": Load already running.\n" + Program.taskList[0].Status;
            }

            //Checking if working directory exists
            if (!CheckWorkingDirectory())
            {
                return "Working directory does not exists.";
            }

            //Else start the bulk insertion
            else
            {
                try
                {
                    Models.Task task = new Models.Task();
                    task.Thread = new Thread(() => Bulkloader(task));
                    task.Thread.Start();
                    Program.taskList.Add(task);
                    return "Bulk load successfully started";
                }
                catch (Exception)
                {
                    return "Bulk load start failed.";
                }
                
            }
        }

        //Method for bulkloading
        public void Bulkloader(Models.Task task)
        {
            string[] files = Directory.GetFiles(Program.myWorkingDirectory);
            int counter = 0;

            if (files.Length == 0) return;
            else
            {
                task.Status = "Bulk importing " + counter + " of " + files.Length;

                Console.Write("Bulk file " + counter + " of " + files.Length + "\n");
                foreach (string file in files)
                {
                    //Bulk insert it
                    Console.Write(ShellHelper.Bash("/opt/mssql-tools/bin/bcp " + Program.myMainTable +
                        " in " + file +
                        " -S " + Program.myApiCall +
                        " -U " + Program.username +
                        " -P " + Program.password +
                        " -d " + Program.myDatabaseName +
                        " -c " +
                        " -t '\\t' -r '\\n'"
                       ).ToString() + "\n");
                    //Remove it
                    ShellHelper.Bash("echo tijan99 | sudo rm " + file);

                    //Count
                    task.bulkcount++;
                    counter++;
                }

                Console.Write("Bulk successfull loaded: " + counter + " files.");

                task.Status = "Bulk finished.";
                Thread.Sleep(60000);
                Program.taskList.Remove(task);
            }
            
        }

        [HttpGet]
        [Route("check_bulk")]
        public string Check_bulk()
        {
            if (Program.taskList.Count > 0)
            {
                return Program.taskList[0].Status;
            }
            else return "Bulk not running";
            
        }
    }
}

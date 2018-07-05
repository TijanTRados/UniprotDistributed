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
        public string Post()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                try
                {
                    //Skipping first 4 lines
                    List<string> fileLines = reader.ReadToEnd().Split('\n').ToList();
                    fileLines.RemoveRange(0, 4);
                    fileLines.RemoveRange(fileLines.Count - 2, 2);

                    System.IO.File.WriteAllText(Program.myWorkingDirectory + Request.Headers["file-name"], string.Join('\n', fileLines));

                    Console.WriteLine($"Saved to " + Program.myWorkingDirectory + Request.Headers["file-name"]);
                    return $"Saved to " + Program.myWorkingDirectory + Request.Headers["file-name"];
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An exception occured while saving " + Request.Headers["file-name"] + ". Check it manually.");
                    return "An exception occured while saving " + Request.Headers["file-name"] + ". Check it manually.";
                }
                
            }
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

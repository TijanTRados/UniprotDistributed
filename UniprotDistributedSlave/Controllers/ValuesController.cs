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
            if (Directory.Exists("~/Distributed/UniprotDistributed/UniprotDistributedSlave/bin/Debug/netcoreapp2.0/slaves/Slave" + Program.mySlaveId+ "/wd/"))
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
                var body = reader.ReadToEnd();

                System.IO.File.WriteAllText(Program.myWorkingDirectory + Request.Headers["file-name"], body);

                Console.WriteLine($"Saved to " + Program.myWorkingDirectory + Request.Headers["file-name"]);
                return $"Saved to " + Program.myWorkingDirectory + Request.Headers["file-name"];
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

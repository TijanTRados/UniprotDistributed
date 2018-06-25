using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "slave1";
        }

        // POST api/values
        [HttpPost]
        [Route("recieve")]
        public string Post()
        {
            Console.WriteLine("I have been called!");

            var client = new RestClient("http://storage.bioinfo.pbf.hr:7000");
            var request = new RestRequest("/slave/recieve", Method.POST);

            client.DownloadData(request).SaveAs("/home/users/tijan/test/slave1/");

            Console.WriteLine("I have just recieved and saved a file!");

            return "I have just recieved a file!!";
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

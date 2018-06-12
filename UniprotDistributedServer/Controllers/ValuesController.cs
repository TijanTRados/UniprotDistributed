using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace UniprotDistributedServer.Controllers
{
    [Route("api/get")]
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
        [Route("api/get")]
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
    }

    [Route("api/post")]
    public class PutController : Controller
    {
        [HttpPost]
        public string Post([FromBody]string sql)
        {
            return "I've put something new";
        }
    }

    [Route("api/info")]
    public class ConfigController : Controller
    {
        [HttpGet]
        public string Get()
        {
            return "Info";
        }
    }
}

using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
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
        public string Load()
        {
            //Read config file

            //Configuration is set up on DefaultConnection string in this case
            BaseDataAccess DataBase = new BaseDataAccess("Data Source=storage.bioinfo.pbf.hr,8758;Initial Catalog=prot;Integrated Security=False;User Id=tijan;Password=tijan99;MultipleActiveResultSets=True");

            DataSet ConfigData = DataBase.ExecuteFillDataSet("select * from configuration c join configuration_servers s on c.configuration_id = s.configuration_id", null);

            //Setting up the SPLIT command to execute
            string splitBash = "split -l 100000 --additional-suffix=.csv " + ConfigData.Tables[0].Select("is_master = 1")[0]["load_source_file"] + " " + ConfigData.Tables[0].Select("is_master = 1")[0]["load_mediation_directory"];
            var escapedArgs = splitBash.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }
    }
}

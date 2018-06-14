using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
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
        public string[] Load()
        {
            Stopwatch stopwatch = new Stopwatch();
            List<string> TimeStatistics = new List<string>();
            int[] values = { '1', '1', '1', '1', '1', '2', '2' };

            TimeStatistics.Add("New measure: \n");
            stopwatch.Start();

            #region Read Configuration File
            //Read config file
            //Configuration is set up on DefaultConnection string in this case
            BaseDataAccess DataBase = new BaseDataAccess("Data Source=storage.bioinfo.pbf.hr,8758;Initial Catalog=prot;Integrated Security=False;User Id=tijan;Password=tijan99;MultipleActiveResultSets=True");
            string MediationDirectory, SourceFile;

            using(DataSet ConfigData = DataBase.ExecuteFillDataSet("select * from configuration c join configuration_servers s on c.configuration_id = s.configuration_id", null))
            {
                MediationDirectory = ConfigData.Tables[0].Select("is_master = 1")[0]["load_mediation_directory"].ToString();
                SourceFile = ConfigData.Tables[0].Select("is_master = 1")[0]["load_source_file"].ToString();
            }

            TimeStatistics.Add("Loading the configuration: " + stopwatch.Elapsed);
            stopwatch.Restart();
            #endregion

            #region Split the file into pieces
            //Setting and executing the SPLIT command to execute
            string splitBash = "split -l 100000 --additional-suffix=.csv " + SourceFile + " " + MediationDirectory;
            ShellHelper.Bash(splitBash);

            TimeStatistics.Add("Splitting the file into 100 000 line ones: " + stopwatch.Elapsed);
            stopwatch.Restart();
            #endregion

            #region Broadcasting the files
            //Reading the new files one by one and doing stuff depending on MASTER/SLAVE
            string[] files = Directory.GetFiles(MediationDirectory);

            ShellHelper.Bash("mkdir " + MediationDirectory + "Run/");

            foreach (string file in files)
            {
                Random r = new Random();
                int randomNumber = r.Next(0, values.Length);

                if (values[randomNumber] == 1)
                {
                    //Copy file to Run/ directory
                    ShellHelper.Bash("cp " + file + " " + MediationDirectory + "Run/");

                } else if (values[randomNumber] == 2)
                { 
                    //Else send a HTTP POST request
                    //Upload("proteinreader.bioinfo.pbf.hr/api/load", param, stream, Bytes);
                }
            }

            TimeStatistics.Add("Broadcasting the Files: " + stopwatch.Elapsed);
            stopwatch.Stop();
            #endregion

            #region Bulk load
            //Run bulk load
            #endregion

            #region Log the Time stats
            //Writing time stats to log file
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(MediationDirectory + "log.txt", true))
            {
                foreach (string line in TimeStatistics)
                {
                    file.WriteLine(line);
                }
            }
            #endregion

            return files;
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
    }
}

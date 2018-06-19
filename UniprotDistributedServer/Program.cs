using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace UniprotDistributedServer
{
    public class Program
    {

        public static void Main(string[] args)
        {

            Init();
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();

        public static void Init()
        {
            #region Read Configuration File
            //status = "Reading configuration files";
            //Read config file
            //Configuration is set up on DefaultConnection string in this case
            BaseDataAccess DataBase = new BaseDataAccess("Data Source=storage.bioinfo.pbf.hr,8758;Initial Catalog=configuration;Integrated Security=False;User Id=tijan;Password=tijan99;MultipleActiveResultSets=True");
            string MediationDirectory, SourceFile;

            using (DataSet ConfigData = DataBase.ExecuteFillDataSet("select * from slaves", null))
            {
                MediationDirectory = ConfigData.Tables[0].Select("is_master = 1")[0]["load_mediation_directory"].ToString();
                SourceFile = ConfigData.Tables[0].Select("is_master = 1")[0]["load_source_file"].ToString();
            }

            #endregion
        }
    }
}

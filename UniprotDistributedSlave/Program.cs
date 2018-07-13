using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UniprotDistributedSlave.Models;

namespace UniprotDistributedSlave
{
    public class Program
    {
        public static List<Servers> Servers;
        public static List<Models.Task> taskList = new List<Models.Task>();

        public static string Me;
        public static Int16 mySlaveId;
        public static string myDatabaseConnectionString;
        public static string myApiCall;
        public static Int16 myApiPort;
        public static byte myServerLevel;
        public static string myWorkingDirectory;
        public static string myMainTable;
        public static string username;
        public static string password;
        public static string myDatabaseName;
        public static string myDbCall;

        public static void Main(string[] args)
        {
            Init();

            //Finding my configuration from the configuration list via argument (http://{host}:{port})

            Me = args[1];

            foreach(string arg in args)
            {
                Console.WriteLine(arg);
            }

            var filtered = from server in Servers
                           where server.api_call == Me
                           select server;
            mySlaveId = (filtered.ToList())[0].slave_id;
            myDatabaseConnectionString = (filtered.ToList())[0].database_connection_string;
            myApiCall = (filtered.ToList())[0].api_call;
            myApiPort = (filtered.ToList())[0].api_port;
            myServerLevel = (filtered.ToList())[0].server_level;
            myWorkingDirectory = AppDomain.CurrentDomain.BaseDirectory + "wd/";
            myMainTable = (filtered.ToList())[0].main_table;
            myDbCall = (filtered.ToList())[0].db_call;

            //Building the configuration
            var configuration = new ConfigurationBuilder()
            .AddCommandLine(args)
            .Build();

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseConfiguration(configuration)
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();

            //BuildWebHost(args).Run();
        }

        //If not the configuration from command line, this one sets it to default
        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("http://localhost:5000")
                .Build();

        //Getting the configuration table data from master server)
        public static void Init()
        {
            Servers = new List<Models.Servers>();
            List<byte> levels = new List<byte>();

            #region Read Configuration File
            //Read config file
            //Configuration is set up on DefaultConnection string in this case
            BaseDataAccess DataBase = new BaseDataAccess("Data Source=localhost,8758;Initial Catalog=configuration;Integrated Security=False;User Id=tijan;Password=tijan99;MultipleActiveResultSets=True");

            using (DataSet ConfigData = DataBase.ExecuteFillDataSet("select slave_id, concat(server_name, ',', db_port) as db_call, concat('Data Source=localhost,', db_port, ';Initial Catalog=', database_name, ';Integrated Security=False;User Id=', username, ';Password=', password, ';MultipleActiveResultSets=True') as database_connection_string, concat('http://', server_name, ':', api_port) as api_call, api_port, server_level, main_table, database_name, username, password  FROM slaves WHERE is_using = 'true';", null))
            {
                foreach (DataRow row in ConfigData.Tables[0].Rows)
                {
                    Servers.Add(new Models.Servers
                    {
                        slave_id = Convert.ToInt16(row["slave_id"]), //smallint to int
                        database_connection_string = row["database_connection_string"].ToString(), //string to string
                        api_call = row["api_call"].ToString(), //string to string
                        api_port = Convert.ToInt16(row["api_port"]), //smallint to int
                        server_level = (byte)row["server_level"], //tinyint to int
                        working_directory = AppDomain.CurrentDomain.BaseDirectory + "wd/",
                        main_table = row["main_table"].ToString(), //string to string,
                        username = row["username"].ToString(),
                        password = row["password"].ToString(),
                        database_name = row["database_name"].ToString(),
                        db_call = row["db_call"].ToString()
                    });

                    levels.Add((byte)row["server_level"]);
                }
            }
            #endregion
        }
    }


}

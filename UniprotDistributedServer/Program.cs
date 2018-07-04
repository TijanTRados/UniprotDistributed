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
using UniprotDistributedServer.Models;

namespace UniprotDistributedServer
{
    public class Program
    {
        public static List<Servers> Servers;
        public static List<int> values;

        public static void Main(string[] args)
        {
            Init();
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("http://localhost:5000")
                .Build();

        public static void Init()
        {
            Servers = new List<Models.Servers>();
            List<byte> levels = new List<byte>();

            #region Read Configuration File
            //Read config file
            //Configuration is set up on DefaultConnection string in this case
            BaseDataAccess DataBase = new BaseDataAccess("Data Source=localhost,8758;Initial Catalog=configuration;Integrated Security=False;User Id=tijan;Password=tijan99;MultipleActiveResultSets=True");

            using (DataSet ConfigData = DataBase.ExecuteFillDataSet("select slave_id, concat('Data Source=localhost,', db_port, ';Initial Catalog=', database_name, ';Integrated Security=False;User Id=', username, ';Password=', password, ';MultipleActiveResultSets=True') as database_connection_string, concat('http://', server_name, ':', api_port) as api_call, api_port, server_level, main_table  FROM slaves WHERE is_using = 'true';", null))
            {
                foreach(DataRow row in ConfigData.Tables[0].Rows)
                {
                    Servers.Add(new Models.Servers
                    {
                        slave_id = Convert.ToInt16(row["slave_id"]), //smallint to int
                        database_connection_string = row["database_connection_string"].ToString(), //string to string
                        api_call = row["api_call"].ToString(), //string to string
                        api_port = Convert.ToInt16(row["api_port"]), //smallint to int
                        server_level = (byte)row["server_level"], //tinyint to int
                        main_table = row["main_table"].ToString() //string to string
                    });

                    levels.Add((byte)row["server_level"]);
                }
            }

            //Initialize the values array
            int counter = 0;
            values = new List<int>();

            foreach(byte level in levels)
            {
                for (byte i = 0; i<level; i++)
                {
                    values.Add(counter);
                }
                counter++;
            }

            //Checkup
            Console.Write("Values Array: " + string.Join(",", values));
            #endregion
        }
    }
}

using Starter.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace Starter
{
    class Program
    {
        public static List<Servers> Servers;

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Please enter master port for the main server to work with.\nUsing: dotnet Starter.dll [port for master]");
                return;
            }

            string port = args[0];

            Init();
            MakeScript(port);

            Console.WriteLine("Successfully made files '"+ Properties.Resources.runscript +"' and '"+ Properties.Resources.killscript +"'. Start the system with ./" + Properties.Resources.runscript + "\nTo kill the processes and free ports run ./" + Properties.Resources.killscript);
        }

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
                foreach (DataRow row in ConfigData.Tables[0].Rows)
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
            #endregion
        }

        public static void MakeScript(string port)
        {
            string path = Properties.Resources.runscript;
            string path2 = Properties.Resources.killscript;

            try
            {

                // Delete the file if it exists.
                if (File.Exists(path))
                {
                    // Note that no lock is put on the
                    // file and the possibility exists
                    // that another process could do
                    // something with it between
                    // the calls to Exists and Delete.
                    File.Delete(path);
                }

                // Delete the file if it exists.
                if (File.Exists(path2))
                {
                    // Note that no lock is put on the
                    // file and the possibility exists
                    // that another process could do
                    // something with it between
                    // the calls to Exists and Delete.
                    File.Delete(path2);
                }

                string script = "#Kill all processes that hold up ports user for the system\n" +
                        "fuser -k -n tcp " + port + "\n";

                foreach(Servers server in Servers)
                {
                    script += "fuser -k -n tcp " + server.api_port + "\n";
                }

                // Create the file for killing the processes
                using (FileStream fs = File.Create(path2))
                {
                    Byte[] info = new UTF8Encoding(true).GetBytes(script);
                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);

                    ShellHelper.Bash("sudo chmod -R a+r+w+x " + path2);
                }


                script += "\n#Build and publish\n" +
                        "dotnet restore ~/Distributed/UniprotDistributed\n" +
                        "dotnet build ~/Distributed/UniprotDistributed\n" +
                        "dotnet publish ~/Distributed/UniprotDistributed\n" +
                        "\n#Remove and make directories and copy the stuff to each slave and make log and wd folders\n" +
                        "rm -r ~/Distributed/UniprotDistributed/UniprotDistributedSlave/bin/Debug/netcoreapp2.0/slaves; mkdir ~/Distributed/UniprotDistributed/UniprotDistributedSlave/bin/Debug/netcoreapp2.0/slaves/\n";

                foreach (Servers server in Servers)
                {
                    script +=
                        "\nmkdir ~/Distributed/UniprotDistributed/UniprotDistributedSlave/bin/Debug/netcoreapp2.0/slaves/Slave" + server.slave_id + "\n" +
                        "cp -r ~/Distributed/UniprotDistributed/UniprotDistributedSlave/bin/Debug/netcoreapp2.0/publish ~/Distributed/UniprotDistributed/UniprotDistributedSlave/bin/Debug/netcoreapp2.0/slaves/Slave" + server.slave_id + "\n" +
                        "mkdir ~/Distributed/UniprotDistributed/UniprotDistributedSlave/bin/Debug/netcoreapp2.0/slaves/Slave" + server.slave_id + "/publish/logs\n" +
                        "mkdir ~/Distributed/UniprotDistributed/UniprotDistributedSlave/bin/Debug/netcoreapp2.0/slaves/Slave" + server.slave_id + "/publish/wd\n";
                }

                script += "\n#Run the thing\n" +
                    "dotnet ~/Distributed/UniprotDistributed/UniprotDistributedServer/bin/Debug/netcoreapp2.0/publish/UniprotDistributedServer.dll &\n";

                foreach(Servers server in Servers)
                {
                    script += "dotnet ~/Distributed/UniprotDistributed/UniprotDistributedSlave/bin/Debug/netcoreapp2.0/slaves/Slave" + server.slave_id + "/publish/UniprotDistributedSlave.dll --urls "+ server.api_call + " >> ~/Distributed/UniprotDistributed/UniprotDistributedSlave/bin/Debug/netcoreapp2.0/slaves/Slave" + server.slave_id + "/publish/logs/log.txt &\n";
                }

                // Create the file.
                using (FileStream fs = File.Create(path))
                {
                    Byte[] info = new UTF8Encoding(true).GetBytes(script);
                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);

                    ShellHelper.Bash("sudo chmod -R a+r+w+x " + path);
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine("Could not create the script. Original error: " + ex.ToString());
            }
        }
    }
}

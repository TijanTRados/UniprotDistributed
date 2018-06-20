using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniprotDistributedServer.Models
{
    public class Servers
    {
        public int slave_id { get; set; }
        public string database_connection_string { get; set; } 
        public string api_call { get; set; }
        public int api_port { get; set; }
        public int server_level { get; set; }
        public string working_directory { get; set; }
        public string main_table { get; set; }
    }
}

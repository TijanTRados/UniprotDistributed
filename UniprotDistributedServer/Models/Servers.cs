using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniprotDistributedServer.Models
{
    public class Servers
    {
        public int slave_id { get; set; }
        public string connection_string { get; set; } 
        public int server_level { get; set; }
        public string working_directory { get; set; }
    }
}

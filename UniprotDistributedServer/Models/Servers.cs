using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniprotDistributedServer.Models
{
    public class Servers
    {
        public string connection_string { get; set; } 
        public int value { get; set; }
        public string working_directory { get; set; }

    }
}

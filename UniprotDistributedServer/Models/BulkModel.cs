using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniprotDistributedServer.Models
{
    public class BulkModel
    {
        public int current { get; set; }
        public int total { get; set; }
        public string status { get; set; }
        public bool done { get; set; }
        public string details { get; set; }
    }
}

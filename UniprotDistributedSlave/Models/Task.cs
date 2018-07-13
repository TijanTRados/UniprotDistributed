using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UniprotDistributedSlave.Models
{
    public class Task
    {
        public DateTime StartTime { get; set; }
        private string _status;

        public int current { get; set; }
        public int total { get; set; }
        public Thread Thread { get; set; }
        public bool done { get; set; }
        public string details { get; set; }

        public Task()
        {
            StartTime = DateTime.Now;
            Status = "started";
            current = 0;
            total = 0;
            done = false;
        }

        public string Status
        {
            get { return _status; } set { _status = value; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UniprotDistributedSlave.Models
{
    public class Task
    {
        public int guid { get; set; }
        public DateTime StartTime { get; set; }
        private string _status;
        public int bulkcount;
        public Thread Thread { get; set; }

        public Task()
        {
            StartTime = DateTime.Now;
            Status = "Bulk started";
            bulkcount = 0;
        }

        public string Status
        {
            get { return _status; } set { _status = DateTime.Now + ": " + value; }
        }
    }
}

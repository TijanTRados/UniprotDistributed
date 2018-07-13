using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UniprotDistributedServer.Models
{
    public class Task
    {
        public DateTime StartTime { get; set; }
        private string _status;
        public Thread Thread { get; set; }

        public bool splitDone { get; set; }
        public int s_current { get; set; }
        public int s_total { get; set; }
        public string s_status;
        public DateTime s_start { get; set; }
        public DateTime s_end { get; set; }

        public bool broadcastDone { get; set; }
        public int b_current { get; set; }
        public int b_total { get; set; }
        public string b_status;
        public DateTime b_start { get; set; }
        public DateTime b_end { get; set; }

        public bool bulkDone { get; set; }
        public int blk_current { get; set; }
        public int blk_total { get; set; }
        public string blk_status;
        public DateTime blk_start { get; set; }
        public DateTime blk_end { get; set; }

        public string details { get; set; }

        public Task()
        {
            StartTime = DateTime.Now;
            Status = "start";
            splitDone = false;
            broadcastDone = false;
            bulkDone = false;
        }

        public string Status
        {
            get { return _status; } set { _status = value; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Starter.Models
{
    public class Task
    {
        public DateTime StartTime { get; set; }
        private string _status;
        public Thread Thread { get; set; }

        public Task()
        {
            StartTime = DateTime.Now;
            Status = "Load started";
        }

        public string Status
        {
            get { return _status; } set { _status = DateTime.Now + ": " + value; }
        }
    }
}

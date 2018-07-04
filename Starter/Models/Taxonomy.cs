using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Starter.Models
{
    public class Taxonomy
    {
        public int tax_id { get; set; }
        public string name { get; set; }
        public int parrent_id { get; set; }
        public int division_id { get; set; }
        public int rank { get; set; }
    }
}

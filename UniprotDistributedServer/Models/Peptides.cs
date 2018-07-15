using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniprotDistributedServer.Models
{
    public class Peptides
    {
        public int id { get; set; }
        public float mass { get; set; }
        public string peptide { get; set; }
        public string acc { get; set; }
        public string protein { get; set; }
        public string taxonomy { get; set; }
        public string division { get; set; }
    }
}

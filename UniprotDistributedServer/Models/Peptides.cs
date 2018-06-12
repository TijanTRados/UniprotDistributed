using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniprotDistributedServer.Models
{
    public class Peptides
    {
        public float mass { get; set; }
        public string peptide { get; set; }
        public string acc { get; set; }
        public int tax_id { get; set; }
        public int div_id { get; set; }
    }
}

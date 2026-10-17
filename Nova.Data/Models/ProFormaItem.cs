using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Data.Models
{
    public class ProFormaItem
    {
        public int Id { get; set; }

        public int ProFormaId { get; set; }
        public int PartId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public ProForma ProForma { get; set; } = null!;
        public Part Part { get; set; } = null!;
    }
}

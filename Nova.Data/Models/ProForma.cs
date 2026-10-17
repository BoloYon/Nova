using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Data.Models
{
    public class ProForma
    {
        public int Id { get; set; }

        public string ProformaNumber { get; set; } = "";
        
        public int CustomerId { get; set; }
        public Company Customer { get; set; } = null!;
        public DateTime Date { get; set; }
        public string Terms { get; set; } = "";
        public string ShippingAddress { get; set; } = "";
        public string Notes { get; set; } = "";
        public List<ProFormaItem> Items { get; set; } = new();
    }
}

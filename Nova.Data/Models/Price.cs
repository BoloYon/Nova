using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Data.Models
{
    public class Price
    {
        public int Id { get; set; }

        public PartCompany PartCompany { get; set; } = null!;
        public int PartCompanyId { get; set; }

        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
    }
}

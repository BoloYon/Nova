using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Data.Models
{
    //Only holds company specific information (regardless of parts)
    public class Company
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";
        public string CompanyNumber { get; set; } = "";
        public string ContactName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        public string Notes { get; set; } = "";
        public string Terms { get; set; } = "";

        public bool IsVendor { get; set; }
        public bool IsCustomer { get; set; }
    }
}

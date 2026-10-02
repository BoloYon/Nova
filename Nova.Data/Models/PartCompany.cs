using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Data.Models
{
    //Acts as a connection from the part to a company
    public class PartCompany
    {
        public int Id { get; set; }

        public int PartId { get; set; }
        public int CompanyId { get; set; }
        public string RelationshipType { get; set; } = "";
        public string CompanyPartNumber { get; set; } = "";

        public Part Part { get; set; } = null!;
        public Company Company { get; set; } = null!;

    }
}

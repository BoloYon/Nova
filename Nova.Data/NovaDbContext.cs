using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nova.Data.Models;

namespace Nova.Data
{
    public class NovaDbContext : DbContext
    {
        public DbSet<Part> Parts { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<PartCompany> PartCompanies { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Nova");

            Directory.CreateDirectory(folderPath);

            string databasePath = Path.Combine(folderPath, "nova.db");

            optionsBuilder.UseSqlite($"Data Source={databasePath}");
        }
    }
}

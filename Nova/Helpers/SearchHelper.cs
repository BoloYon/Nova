using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using Nova.Data;
using Nova.Data.Migrations;
using Nova.Data.Models;
using Nova.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Helpers
{
    internal class SearchHelper
    {
        //Allows easy filtering
        public enum CompanyFilter
        {
            All,
            Customer,
            Vendor
        }

        //MAIN FUNCTION:

        //Filters and populates a list
        public static void LoadSearchList<T>(ListView list, IEnumerable<T> items, String text, CompanyFilter filter = CompanyFilter.All, bool hideOnEmpty = true)
        {
            DbSet<Part>? parts = items as DbSet<Part>;
            DbSet<Company>? companies = items as DbSet<Company>;

            if (string.IsNullOrWhiteSpace(text) && hideOnEmpty)
            {
                return;
            }

            text = text.ToLower().Trim();

            list.Items.Clear();

            //Company filtered searches
            if (companies != null)
            {
                //Create an empty company list
                List<Company> filteredCompanies = new List<Company>();

                foreach (Company company in companies.OrderBy(c => c.Name))
                {
                    bool hasName = company.Name.ToLower().Trim().Contains(text);
                    bool hasContact = company.ContactName.ToLower().Trim().Contains(text);
                    bool hasCompanyNumber = company.CompanyNumber.ToLower().Trim().Contains(text);

                    if (hasName || hasContact || hasCompanyNumber)
                    {
                        //Both customer or vendor
                        if (filter == CompanyFilter.All && (company.IsCustomer || company.IsVendor))
                        {
                            filteredCompanies.Add(company);
                        }
                        //Customer only
                        else if (filter == CompanyFilter.Customer && company.IsCustomer)
                        {
                            filteredCompanies.Add(company);
                        }
                        //Vendor only
                        else if (filter == CompanyFilter.Vendor && company.IsVendor)
                        {
                            filteredCompanies.Add(company);
                        }
                    }
                }

                LoadSelectionList(list, filteredCompanies);

            }

            //Part filtered searches
            else if (parts != null)
            {
                //Create an empt part list
                List<Part> filteredParts = new List<Part>();

                foreach (Part part in parts.OrderBy(p => p.PartName))
                {
                    bool hasName = part.PartName.ToLower().Trim().Contains(text);
                    bool hasPartNumber = part.PrimaryPartNumber.ToLower().Trim().Contains(text);
                    
                    if (hasName || hasPartNumber)
                    {
                        filteredParts.Add(part);
                    }
                }

                LoadSelectionList(list, filteredParts);
            }
        }


        //HELPER FUNCTIONS:

        //Loads items into their respective lists
        private static void LoadSelectionList(ListView list, List<Part> parts)
        {

            //Loop through and add parts + tag
            foreach (Part part in parts)
            {
                ListViewItem item = new ListViewItem();

                item.Content = $"{part.PrimaryPartNumber} - {part.PartName}";
                item.Tag = part;

                list.Items.Add(item);
            }
        }
        private static void LoadSelectionList(ListView list, List<Company> companies)
        {

            //Loop through and add companies + tag
            foreach (Company company in companies)
            {
                ListViewItem item = new ListViewItem();

                if (!string.IsNullOrWhiteSpace(company.ContactName))
                {
                    item.Content = $"{company.Name} - {company.ContactName}";
                }
                else
                {
                    item.Content = $"{company.Name}";
                }
                
                item.Tag = company;

                list.Items.Add(item);
            }
        }

    }
}

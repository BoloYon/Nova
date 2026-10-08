using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Nova.Data;
using Nova.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Helpers
{
    internal static class LinkHelper
    {
        private enum Command
        {
            Save,
            Update
        }

        public enum PageType
        {
            Company,
            Part
        }

        //Finds an existing part company using part and company Ids
        public static PartCompany? FindExistingLink(Part part, Company company)
        {
            using NovaDbContext context = new NovaDbContext();

            foreach (PartCompany pc in context.PartCompanies)
            {
                if (pc.CompanyId == company.Id && pc.PartId == part.Id)
                {
                    return pc;
                }
            }

            return null;
        }

        //Creates a new PartCompany link
        public static async Task<PartCompany?> CreateNewLink(XamlRoot xaml, String number, int cId, int pId, String relation)
        {
            //Init and declare PC properties
            PartCompany pc = new PartCompany();

            pc.CompanyPartNumber = number;
            pc.CompanyId = cId;
            pc.PartId = pId;
            pc.RelationshipType = relation;

            //Create dialog and error message
            ContentDialog dialog = DialogHelper.CreateDialog("TEMP", "", "Okay", xaml);
            TextBlock msg = new TextBlock();

            dialog.Content = msg;
            msg.Text = "";

            //Attempt a save
            if (await TryToSaveOrUpdate(Command.Save, pc))
            {
                dialog.Title = "SUCCESS";
                msg.Text = "Part ↔ Company link was saved.";

                await dialog.ShowAsync();
                return pc;
            }
            else
            {
                dialog.Title = "ERROR";
                msg.Text = "Part ↔ Company link was not saved.";

                await dialog.ShowAsync();
                return null;
            }

        }

        public static async Task<bool> UpdateLink(XamlRoot xaml, String number, PartCompany pc, String relation, PageType page)
        {
            pc.CompanyPartNumber = number;

            //Change relation to both if relation is different
            if (pc.RelationshipType != relation && page == PageType.Company)
            {
                pc.RelationshipType = "Both";
            }
            else
            {
                pc.RelationshipType = relation;
            }

            //Create dialog and error message
            ContentDialog dialog = DialogHelper.CreateDialog("TEMP", "", "Okay", xaml);
            TextBlock msg = new TextBlock();

            dialog.Content = msg;
            msg.Text = "";

            //Attempt a save
            if (await TryToSaveOrUpdate(Command.Update, pc))
            {
                dialog.Title = "SUCCESS";
                msg.Text = "Part ↔ Company link has been updated.";

                await dialog.ShowAsync();
                return true;
            }
            else
            {
                dialog.Title = "ERROR";
                msg.Text = "Part ↔ company link has not been updated.";

                await dialog.ShowAsync();
                return false;
            }


        }


        //HELPER FUNCTIONS:

        //Either adds or updates the PC database using a cmd
        private static async Task<bool> TryToSaveOrUpdate(Command cmd, PartCompany pc)
        {
            using NovaDbContext context = new NovaDbContext();

            //Determind command
            if (cmd == Command.Save)
            {
                context.Add(pc);
            }
            else if (cmd == Command.Update)
            {
                context.Update(pc);
            }

            //Attempt to save changes
            try
            {
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }

        }
    }
}

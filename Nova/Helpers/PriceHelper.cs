using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nova.Data.Models;
using Nova.Data;
using Microsoft.EntityFrameworkCore;
using Windows.UI.Notifications;

namespace Nova.Helpers
{
    public static class PriceHelper
    {
        private class PriceResult
        {
            public string FormattedDate { get; set; } = "";
            public string FormattedPrice { get; set; } = "";
            public DateTime Date { get; set; }
            public Decimal Amount { get; set; }
        }

        //EVENT HANDLERS:

        //Deletes the price entry and item in the list
        private static async void DeletePrice_Click(object sender, RoutedEventArgs e, ListView list, TextBlock latestPrice, ListViewItem item, int pcId, XamlRoot xaml)
        {
            ContentDialog dialog = DialogHelper.CreateDialog("TEMPORARY", "", "Okay", xaml);
            TextBlock msg = new TextBlock();
            dialog.Content = msg;

            msg.Text = "";

            Button deleteButton = (Button)sender;

            Price? p = deleteButton.Tag as Price;

            //Safety check
            if (p == null)
            {
                dialog.Title = "ERROR";
                msg.Text = "Delete price failed. Please try again.";
                await dialog.ShowAsync();
                return;
            }

            //Attempt to delete price
            if (await DeletePriceEntry(p))
            {
                dialog.Title = "SUCCESS";
                msg.Text = "Price was deleted.";
                list.Items.Remove(item);
                ReloadPriceHistory(list, latestPrice, pcId, xaml);
            }
            else
            {
                dialog.Title = "ERROR";
                msg.Text = "Price was not deleted.";
            }

            await dialog.ShowAsync();

        }

        //MAIN FUNCTIONS:

        //Adds a price to the list and to the database 
        public static async Task CreatePriceEntry(ListView list, TextBlock latestPrice, int pcId, XamlRoot xaml)
        {
            ContentDialog dialog = DialogHelper.CreateDialog("TEMPORARY", "", "Okay", xaml);
            TextBlock msg = new TextBlock();
            dialog.Content = msg;

            msg.Text = "";

            PriceResult? pr = await CreateAndValidatePriceDialog(xaml);

            //If Validation failed
            if (pr == null)
            {
                dialog.Title = "ERROR";
                msg.Text = "Validation failed. Please double check your inputs.";
                await dialog.ShowAsync();
                return;
            }

            Price? price = await CreateandSaveNewPrice(pr, pcId);

            //If Price was not created
            if (price == null)
            {
                dialog.Title = "ERROR";
                msg.Text = "There was an error while saving price. Please try again.";
                await dialog.ShowAsync();
                return;
            }

            ReloadPriceHistory(list, latestPrice, pcId, xaml);

            dialog.Title = "SUCCESS";
            msg.Text = "New price entry successfully created.";
            await dialog.ShowAsync();
        }

        //Populates a list with all the prices in order by date
        public static void ReloadPriceHistory(ListView list, TextBlock latestPrice, int pcId, XamlRoot xaml)
        {
            list.Items.Clear();
            using NovaDbContext context = new NovaDbContext();

            //Search each price with pcId
            foreach (Price price in context.Prices.OrderByDescending(p => p.Date))
            {
                if (price.PartCompanyId == pcId)
                {
                    PriceResult pr = new PriceResult();
                    pr.FormattedDate = price.Date.ToString("MM/yyyy");
                    pr.FormattedPrice = price.Amount.ToString("C");

                    MakePriceItem(pr, price, list, latestPrice, pcId, xaml);
                    latestPrice.Text = GetLatestPrice(pcId);
                } 
            }
        }



        //HELPER FUNCTIONS:

        //Fetches the latest price and returns the formatted version
        private static string GetLatestPrice(int id)
        {
            using NovaDbContext context = new NovaDbContext();

            foreach (Price p in context.Prices.OrderByDescending(p => p.Date))
            {
                if (id == p.PartCompanyId)
                {
                    return p.Amount.ToString("C");
                }
            }

            return "$0.00";
        }

        //Makes a price dialog then validates and attempts to correct price and date inputs. Returns corrected price results or null if attempt failed.
        private static async Task<PriceResult?> CreateAndValidatePriceDialog(XamlRoot xaml)
        {
            DialogHelper.PriceDialogParts dParts =  DialogHelper.CreatePriceDialog(xaml);
            PriceResult? pr = new PriceResult();

            //Add to the click logic event
            dParts.Dialog.PrimaryButtonClick += (dialogSender, dialogArgs) =>
            {
                //Init errorMessage
                string errorMessage = "";

                // Check price first so date errors take priority
                if (TryParsePriceEntry(dParts.PriceTextBox.Text, out decimal parsedPrice))
                {
                    pr.Amount = parsedPrice;
                    pr.FormattedPrice = FormatPrice(parsedPrice);
                }
                else
                {
                    errorMessage = "Invalid price.";
                    dialogArgs.Cancel = true;
                }

                //Check date
                if (TryParseDateEntry(dParts.DateTextBox.Text, out DateTime parsedDate))
                {
                    pr.Date = parsedDate;
                    pr.FormattedDate = FormatDate(parsedDate);
                }
                else
                {
                    errorMessage = "Invalid date.";
                    dialogArgs.Cancel = true;
                }

                dParts.ErrorText.Text = errorMessage;
                
            };

            ContentDialogResult result = await dParts.Dialog.ShowAsync();

            //If button was "Add"
            if (result == ContentDialogResult.Primary && pr != null)
            {
                return pr;
            }

            return null;
        }

        //Adds a ListViewItem with price text and a delete button
        private static void MakePriceItem(PriceResult pr, Price price, ListView list, TextBlock latestPrice, int pcId, XamlRoot xaml)
        {
            ListViewItem item = new ListViewItem();
            item.Tag = price; //For edits on prices

            //Create the grid structure for the item
            Grid grid = new Grid();

            ColumnDefinition textColumn = new ColumnDefinition();
            textColumn.Width = new GridLength(1, GridUnitType.Star);

            ColumnDefinition deleteColumn = new ColumnDefinition();
            deleteColumn.Width = GridLength.Auto;

            grid.ColumnDefinitions.Add(textColumn);
            grid.ColumnDefinitions.Add(deleteColumn);

            TextBlock priceText = new TextBlock();
            Button deleteButton = new Button();

            Grid.SetColumn(priceText, 0);
            Grid.SetColumn(deleteButton, 1);

            grid.Children.Add(priceText);
            grid.Children.Add(deleteButton);

            //Add the grid to the button contents
            item.Content = grid;

            //Create the price string
            priceText.Text = $"{pr.FormattedDate} - {pr.FormattedPrice}";

            deleteButton.Tag = price;
            deleteButton.Content = "×";

            //Add delete event handler
            deleteButton.Click += (sender, e) => DeletePrice_Click(sender, e, list, latestPrice, item, pcId, xaml);

            list.Items.Add(item);

        }
        
        //Creates a new price object
        private static async Task<Price?> CreateandSaveNewPrice(PriceResult pr, int pcId)
        {
            using NovaDbContext context = new NovaDbContext();

            Price p = new Price();
            p.Amount = pr.Amount;
            p.Date = pr.Date;
            p.PartCompanyId = pcId;

            context.Prices.Add(p);

            try
            {
                await context.SaveChangesAsync();
                return p;
            }
            catch (DbUpdateException)
            {
                return null;
            }

        }
        
        //Checks if date is valid
        private static bool TryParseDateEntry(string date, out DateTime parsedDate)
        {
            return DateTime.TryParseExact(date,
                "MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out parsedDate);
        }

        //Checks if price is valid
        private static bool TryParsePriceEntry(string price, out decimal parsedPrice)
        {
            return decimal.TryParse(price,
               System.Globalization.NumberStyles.Currency,
               System.Globalization.CultureInfo.CurrentCulture,
               out parsedPrice);
        }

        //Returns formatted price
        private static string FormatPrice(decimal price)
        {
            return price.ToString("C");
        }

        //Returns formatted date
        private static string FormatDate(DateTime date)
        {
            return date.ToString("MM/yyyy");
        }

        //Deletes a price entry from the database
        private static async Task<bool> DeletePriceEntry(Price p)
        {
            using NovaDbContext context = new NovaDbContext();

            context.Prices.Remove(p);

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

using ABI.Windows.ApplicationModel.Activation;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Nova.Data;
using Nova.Data.Models;
using Nova.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Nova.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PartsPage : Page
    {
        private Part? _selectedPart = null;
        private Company? _selectedCompany = null;

        public PartsPage()
        {
            InitializeComponent();
            LoadCompanies();
        }

        private void CompanyList_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            //Grab the list
            ListView CompanyList = (ListView)sender;

            //Get the selected item
            ListViewItem? selectedCompany = CompanyList.SelectedItem as ListViewItem;

            //Safety Check
            if (selectedCompany == null)
            {
                return;
            }

            //Change the Selected Company
            SelectedCompanyText.Text = $"Selected Company: {selectedCompany.Content}";

            //Grab the company obj from the tag
            _selectedCompany = selectedCompany.Tag as Company;

        }

        private async void NewPart_Click(object sender, RoutedEventArgs args)
        {
            if (await DialogHelper.ConfirmClearAsync(this.XamlRoot, "part"))
            {
                ClearAllFields();
            }
        }

        //Makes a new part object
        private async void SavePart_Click(object sender, RoutedEventArgs args)
        {
            //Create the part obj
            Part p = new Part();

            //Create dialog. Title and message will change depending on success/ error
            ContentDialog saveDialog = DialogHelper.CreateDialog("Temporary", "", "Okay", this.XamlRoot);

            TextBlock message = new TextBlock();
            message.Text = "";

            saveDialog.Content = message;

            //Allows for else if checks on decimals' out
            decimal shipWeight = 0;
            decimal cubicFeet = 0;

            //Validate each field
            saveDialog.Title = "ERROR";

            if (string.IsNullOrWhiteSpace(PartNameText.Text))
            {
                message.Text = "Part name cannot be empty.";
            }
            else if (string.IsNullOrWhiteSpace(PrimaryPartNumberText.Text))
            {
                message.Text = "Primary part number cannot be empty.";
            }
            else if (string.IsNullOrWhiteSpace(UnitTypeText.Text))
            {
                message.Text = "Unit type cannot be empty.";
            }
            else if (!decimal.TryParse(ShipWeightText.Text, out shipWeight))
            {
                message.Text = "Ship weight must be a valid number.";
            }
            else if (!decimal.TryParse(CubicFeetText.Text, out cubicFeet))
            {
                message.Text = "Cubic feet must be a valid number.";
            }

            //Error occured if message is not empty
            if (message.Text != "")
            {
                await saveDialog.ShowAsync();
                return;
            }
            
            //Initialize all part properties
            p.PartName = PartNameText.Text;
            p.PrimaryPartNumber = PrimaryPartNumberText.Text.Trim();
            p.Description = DescriptionText.Text.Trim();
            p.UnitType = UnitTypeText.Text.Trim();
            p.ShipWeight = shipWeight;
            p.CubicFeet = cubicFeet;
            p.DrawingOnFile = DrawingOnFileCheckBox.IsChecked == true;
            p.MoldAvailable = MoldAvailableCheckBox.IsChecked == true;
            p.Notes = NotesText.Text.Trim();

            //Create a context  and add the new part obj
            using NovaDbContext context = new NovaDbContext();

            context.Parts.Add(p);

            //Attempt to save
            if (await TrySaveToDB(context, saveDialog, message, $"{p.PartName}"))
            {
                _selectedPart = p;
            }

            
            
        }

        private async void AddPriceEntry_Click(object sender, RoutedEventArgs args)
        {

            //Make a new dialog object
            ContentDialog addPriceDialog = DialogHelper.CreateDialog("Add Price Entry", "Add", "Cancel", this.XamlRoot);
            

            //Make the actual content for the dialog
            StackPanel dialogContent = new StackPanel();
            dialogContent.Spacing = 8;

            TextBlock dateLabel = new TextBlock();
            dateLabel.Text = "Date";

            TextBox dateTextBox = new TextBox();
            dateTextBox.PlaceholderText = "MM/YYYY";

            TextBlock priceLabel = new TextBlock();
            priceLabel.Text = "Price";

            TextBox priceTextBox = new TextBox();
            priceTextBox.PlaceholderText = "0.00";

            TextBlock errorText = new TextBlock();
            errorText.Text = "";
            errorText.Foreground = new SolidColorBrush(Colors.Red);

            dialogContent.Children.Add(dateLabel);
            dialogContent.Children.Add(dateTextBox);
            dialogContent.Children.Add(priceLabel);
            dialogContent.Children.Add(priceTextBox);
            dialogContent.Children.Add(errorText);


            //Add content to the dialog
            addPriceDialog.Content = dialogContent;

            //Save formatted date and time
            string formattedDate = "";
            string formattedPrice = "";

            //Add to the click logic event
            addPriceDialog.PrimaryButtonClick += (dialogSender, dialogArgs) =>
            {
                //Init errorMessage
                string errorMessage = "";

                // Check price first so date errors take priority
                if (TryParsePriceEntry(priceTextBox.Text, out decimal parsedPrice))
                {
                    formattedPrice = FormatPrice(parsedPrice);
                }
                else
                {
                    errorMessage = "Invalid price.";
                    dialogArgs.Cancel = true;
                }

                //Check date
                if (TryParseDateEntry(dateTextBox.Text, out DateTime parsedDate))
                {
                    formattedDate = FormatDate(parsedDate);
                }
                else
                {
                    errorMessage = "Invalid date.";
                    dialogArgs.Cancel = true;
                }
            
                errorText.Text = errorMessage;

                return;
;
            };

            ContentDialogResult result = await addPriceDialog.ShowAsync();

            //If button was "Add"
            if (result == ContentDialogResult.Primary)
            {
                //Create a new price entry
                ListViewItem newPriceEntry = new ListViewItem();

                newPriceEntry.Content = $"{formattedDate} - {formattedPrice}";

                //Add it to the list
                PriceHistoryList.Items.Insert(0, newPriceEntry);
                LatestCostText.Text = formattedPrice;

            }
        }

        private async void SaveRelation_Click(object sender, RoutedEventArgs e)
        {
            //Make a part company obj
            PartCompany pc = new PartCompany();

            //Create Dialog
            ContentDialog relationDialog = DialogHelper.CreateDialog("TEMPORARY", "", "Okay", this.XamlRoot);

            //Create message
            TextBlock message = new TextBlock();
            message.Text = "";

            relationDialog.Content = message;

            //Safety check
            if (_selectedPart == null)
            {
                message.Text = "You must select a part before adding a relationship.";

            }
            else if (_selectedCompany == null)
            {
                message.Text = "You must select a company before adding a relationship.";
            }
            else if (RelationshipTypeComboBox.SelectedItem == null)
            {
                message.Text = "A relationship must be selected.";
            }

            //Show error dialog
            if (message.Text != "")
            {
                relationDialog.Title = "ERROR";
                await relationDialog.ShowAsync();
                return;
            }

            //Get relationship type from combo box
            ComboBoxItem selectedRelation = (ComboBoxItem)RelationshipTypeComboBox.SelectedItem!;
            string relation = selectedRelation.Content.ToString()!;

            //Update customer & vendor bools
            if (!UpdateCompanyBooleans(relation))
            {
                relationDialog.Title = "ERROR";
                message.Text = "Company update failed. Please try again.";
                await relationDialog.ShowAsync();

                return;
            }



            //Initialize pc properties
            pc.PartId = _selectedPart!.Id;
            pc.CompanyId = _selectedCompany!.Id;
            pc.CompanyPartNumber = CompanyPartNumberText.Text.Trim();
            pc.RelationshipType = relation;

            using NovaDbContext context = new NovaDbContext();
            context.PartCompanies.Add(pc);

            await TrySaveToDB(context, relationDialog, message, $"{_selectedPart.PartName} ↔ {_selectedCompany.Name} relationship");
            
        }


        //HELPERS

        //Changes the customer and vendor booleans in a company obj
        private bool UpdateCompanyBooleans(String relation)
        {

            //Safety check
            if (_selectedCompany == null)
            {
                return false;
            }
            else if (relation == null)
            {
                return false;
            }

            //Update relation(s)
            if (relation == "Customer")
            {
                _selectedCompany.IsCustomer = true;
            }
            else if (relation == "Vendor")
            {
                _selectedCompany.IsVendor = true;
            }
            else if (relation == "Both")
            {
                _selectedCompany.IsCustomer = true;
                _selectedCompany.IsVendor = true;
            }

            using NovaDbContext context = new NovaDbContext();
            context.Companies.Update(_selectedCompany);

            try
            {
                context.SaveChanges();
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }

        //Attempts to save an obj to the database
        private async Task<bool> TrySaveToDB(NovaDbContext context, ContentDialog dialog, TextBlock msg, string name)
        {
            try
            {
                context.SaveChanges();
                dialog.Title = "SUCCESS";
                msg.Text = $"'{name}' was saved.";

                await dialog.ShowAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                dialog.Title = "ERROR";
                msg.Text = $"'{name}' was not saved.";

                await dialog.ShowAsync();
                return false;
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

        //Clears every input field on the parts page
        private void ClearAllFields()
        {
            //Clear left side
            PartNameText.Text = "";
            PrimaryPartNumberText.Text = "";
            DescriptionText.Text = "";
            UnitTypeText.Text = "";
            ShipWeightText.Text = "";
            CubicFeetText.Text = "";
            DrawingOnFileCheckBox.IsChecked = false;
            MoldAvailableCheckBox.IsChecked = false;
            NotesText.Text = "";

            //Clear right side
            CompanySearchBar.Text = "";
            CompanyList.Items.Clear();
            SelectedCompanyText.Text = "Selected Company";
            CompanyPartNumberText.Text = "";
            LatestCostText.Text = "";
            PriceHistoryList.Items.Clear();
            RelationshipTypeComboBox.SelectedItem = null;

            //Clear selected part and company
            _selectedPart = null;
            _selectedCompany = null;
            
        }

        //Populates the company list
        private void LoadCompanies()
        {
            using NovaDbContext context = new NovaDbContext();

            //Clear the list
            CompanyList.Items.Clear();

            foreach (Company company in context.Companies)
            {
                ListViewItem item = new ListViewItem();

                item.Content = $"{company.CompanyNumber} - {company.Name}";

                //Save the company object as a tag for later referencing
                item.Tag = company;

                CompanyList.Items.Add(item);
            }
        }
    }
}

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
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Nova.Views
{
    public sealed partial class PartsPage : Page
    {
        private Part? _selectedPart = null;
        private Company? _selectedCompany = null;
        private PartCompany? _selectedPartCompany = null;

        public PartsPage()
        {
            InitializeComponent();
            LoadCompanies();
        }

        private void PartSearchBar_TextChanged(object sender, TextChangedEventArgs args)
        {
            using NovaDbContext context = new NovaDbContext();

            SearchHelper.LoadSearchList(PartSearchResults, context.Parts, PartSearchBar.Text);
        }
        private void CompanySearchBar_TextChanged(object sender, TextChangedEventArgs args)
        {
            using NovaDbContext context = new NovaDbContext();
            SearchHelper.LoadSearchList(CompanyList, context.Companies, CompanySearchBar.Text, hideOnEmpty: false);
        }
        private void PartSearchResult_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            ListView searchList = (ListView)sender;

            ListViewItem? item = searchList.SelectedItem as ListViewItem;

            if (item == null)
            {
                return;
            }

            _selectedPart = (Part)item.Tag;

            PopulatePartFields();

            //Populate relationship fields once both a part and company are selected
            if (_selectedCompany != null && _selectedPart != null)
            {
                PopulateCompanyFields();
            }
        }

        private void CompanyList_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            //Grab the list
            ListView CompanyList = (ListView)sender;

            //Get the selected item
            ListViewItem? selectedCompany = CompanyList.SelectedItem as ListViewItem;

            //Safety Check
            if (selectedCompany == null || selectedCompany.Tag as Company == null)
            {
                return;
            }

            //Grab the company obj from the tag
            _selectedCompany = selectedCompany.Tag as Company;

            //Change the Selected Company
            if (!string.IsNullOrWhiteSpace(_selectedCompany!.ContactName))
            {
                SelectedCompanyText.Text = $"{_selectedCompany.Name} - {_selectedCompany.ContactName}";
            }
            else
            {
                SelectedCompanyText.Text = _selectedCompany.Name;
            }

            //Populate relationship fields once both a part and company are selected
            if (_selectedCompany != null && _selectedPart != null)
            {
                PopulateCompanyFields();
            }

        }

        private async void NewPart_Click(object sender, RoutedEventArgs args)
        {
            if (await DialogHelper.ConfirmClearAsync(this.XamlRoot, "part"))
            {
                ClearAllFields();
            }
        }

        private async void SavePart_Click(object sender, RoutedEventArgs args)
        {

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

            //Determine if creation or update
            if (_selectedPart == null)
            {
                await CreatePartEntry(saveDialog, message, shipWeight, cubicFeet);
            }
            else
            {
                await UpdatePartEntry(saveDialog, message, shipWeight, cubicFeet);
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
            //Create Dialog
            ContentDialog relationDialog = DialogHelper.CreateDialog("TEMPORARY", "", "Okay", this.XamlRoot);

            //Create message
            TextBlock message = new TextBlock();
            message.Text = "";

            relationDialog.Content = message;

            //Safety check
            if (_selectedPart == null || _selectedCompany == null)
            {
                message.Text = "Part and Company must BOTH be selected before saving a relationship.";

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

            PartCompany? pc = LinkHelper.FindExistingLink(_selectedPart!, _selectedCompany!);
            bool success;

            //Create new link
            if (pc == null)
            {
                pc = await LinkHelper.CreateNewLink(this.XamlRoot, CompanyPartNumberText.Text, _selectedCompany!.Id, _selectedPart!.Id, relation);
                success = (pc != null);
            }
            //Edit existing link
            else
            {
                success = await LinkHelper.UpdateLink(this.XamlRoot, CompanyPartNumberText.Text, pc, relation, LinkHelper.PageType.Part);
            }

            if (success)
            {
                _selectedPartCompany = pc;
            }
            
        }


        //HELPERS

        //Populates the search bar result list using filtered text
        private void SearchParts()
        {
            PartSearchResults.Items.Clear();

            using NovaDbContext context = new NovaDbContext();

            String searchStr = PartSearchBar.Text.Trim().ToLower();

            //Show nothing on empty search
            if (searchStr == "")
            {
                return;
            }

            //Search each part for partname and part number
            foreach (Part part in context.Parts)
            {
                String partName = part.PartName.ToLower();
                String partNumber = part.PrimaryPartNumber.ToLower(); //(PPN's can contain letters)

                if (partName.Contains(searchStr) || partNumber.Contains(searchStr))
                {
                    ListViewItem item = new ListViewItem();

                    item.Content = $"{part.PrimaryPartNumber} - {part.PartName}";

                    //Save part obj
                    item.Tag = part;

                    PartSearchResults.Items.Add(item);
                }
            }
        }

        //Populates all the left fields on the parts page
        private void PopulatePartFields()
        {
            if (_selectedPart == null)
            {
                return;
            }

            PartNameText.Text = _selectedPart.PartName;
            PrimaryPartNumberText.Text = _selectedPart.PrimaryPartNumber;
            UnitTypeText.Text = _selectedPart.UnitType;
            ShipWeightText.Text = _selectedPart.ShipWeight.ToString();
            CubicFeetText.Text = _selectedPart.CubicFeet.ToString();
            DrawingOnFileCheckBox.IsChecked = _selectedPart.DrawingOnFile;
            MoldAvailableCheckBox.IsChecked = _selectedPart.MoldAvailable;
            DescriptionText.Text = _selectedPart.Description;
            NotesText.Text = _selectedPart.Notes;

        }

        //Populates all the fields on the right side of the parts page
        private void PopulateCompanyFields()
        {
            //Look for an existing connection
            PartCompany? pc = LinkHelper.FindExistingLink(_selectedPart!, _selectedCompany!);

            //New connection, no fields to populate
            if (pc == null)
            {
                RelationshipTypeComboBox.SelectedItem = null;
                CompanyPartNumberText.Text = "";
                LatestCostText.Text = "$0.00";
                PriceHistoryList.Items.Clear();
                _selectedPartCompany = null;
                return;
            }

            //Loop through each item to find the matching combo box item
            foreach (ComboBoxItem item in RelationshipTypeComboBox.Items)
            {
                if (item.Content.ToString() == pc.RelationshipType)
                {
                    RelationshipTypeComboBox.SelectedItem = item;
                }
            }

            CompanyPartNumberText.Text = pc.CompanyPartNumber;
            _selectedPartCompany = pc;
        }


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

        //Creates an entire part entry
        private async Task CreatePartEntry(ContentDialog saveDialog, TextBlock message, decimal shipWeight, decimal cubicFeet)
        {
            //Create the part obj
            Part p = new Part();

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

        //Updates an existing part entry
        private async Task UpdatePartEntry(ContentDialog saveDialog, TextBlock message, decimal shipWeight, decimal cubicFeet)
        {
            Part p = _selectedPart!;

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

            try
            {
                context.Parts.Update(p);
                context.SaveChanges();

                saveDialog.Title = "SUCCESS";
                message.Text = $"'{p.PartName}' was updated successfully.";
            }
            catch (DbUpdateException)
            {
                saveDialog.Title = "ERROR";
                message.Text = $"'{p.PartName}' was not updated.";
            }

            await saveDialog.ShowAsync();
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
            PartSearchBar.Text = "";
            PartSearchResults.Items.Clear();

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
            LoadCompanies();

            SelectedCompanyText.Text = "No Company Selected";
            CompanyPartNumberText.Text = "";
            LatestCostText.Text = "";
            PriceHistoryList.Items.Clear();
            RelationshipTypeComboBox.SelectedItem = null;

            //Clear selected part and company
            _selectedPart = null;
            _selectedCompany = null;
            _selectedPartCompany = null;

        }

        //Populates the company list
        private void LoadCompanies()
        {
            using NovaDbContext context = new NovaDbContext();

            SearchHelper.LoadSearchList(CompanyList, context.Companies, "", hideOnEmpty: false);

        }

    }
}

using Microsoft.EntityFrameworkCore;
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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Nova.Views
{
    
    public sealed partial class CustomerPage : Page
    {
        Company? _selectedCompany = null;
        Part? _selectedPart = null;
        PartCompany? _selectedPartCompany = null;

        public CustomerPage()
        {
            InitializeComponent();
            LoadParts();
        }

        private void CustomerSearchBar_TextChanged(object sender, TextChangedEventArgs args)
        {
            using NovaDbContext context = new NovaDbContext();
            SearchHelper.LoadSearchList(CustomerSearchResults, context.Companies, CustomerSearchBar.Text, SearchHelper.CompanyFilter.Customer);
        }

        private void CustomerSearchResult_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            ListView list = (ListView)sender;
            ListViewItem? item = list.SelectedItem as ListViewItem;

            if (item == null || item.Tag as Company == null)
            {
                return;
            }

            _selectedCompany = (Company)item.Tag;

            PopulateCompanyFields();

            if (_selectedCompany != null && _selectedPart != null)
            {
                PopulatePartField();
            }

        }

        private void CustomerPartSearchBar_TextChanged(object sender, TextChangedEventArgs args)
        {
            using NovaDbContext context = new NovaDbContext();
            SearchHelper.LoadSearchList(CustomerPartList, context.Parts, CustomerPartSearchBar.Text, hideOnEmpty: false);
        }

        private async void SaveCustomerPartNumber_Click(object sender, RoutedEventArgs args)
        {
            ContentDialog d = DialogHelper.CreateDialog("TEMPORARY", "", "Okay", this.XamlRoot);
            TextBlock msg = new TextBlock();
            d.Content = msg;

            //Safety
            if (_selectedPart == null || _selectedCompany == null)
            {
                d.Title = "ERROR";
                msg.Text = "Customer and Part must BOTH be selected.";
                await d.ShowAsync();
                return;
            }

            PartCompany? pc = LinkHelper.FindExistingLink(_selectedPart, _selectedCompany);
            bool success;

            //Delegate Tasks
            if (pc == null)
            {
                pc = await LinkHelper.CreateNewLink(this.XamlRoot, CustomerPartNumberText.Text, _selectedCompany.Id, _selectedPart.Id, "Customer");

                success = (pc != null);
            }
            else
            {
                success = await LinkHelper.UpdateLink(this.XamlRoot, CustomerPartNumberText.Text, pc, "Customer", LinkHelper.PageType.Company);
            }
            
            //Update selected on save success
            if (success)
            {
                _selectedPartCompany = pc;
            }

        }

        private void AddCustomerQuote_Click(object sender, RoutedEventArgs args)
        {

        }

        private void CustomerPartList_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            ListView list = (ListView)sender;
            ListViewItem? item = list.SelectedItem as ListViewItem;

            if (item == null || item.Tag as Part == null)
            {
                return;
            }

            _selectedPart = item.Tag as Part;

            SelectedCustomerPartText.Text = $"{_selectedPart!.PrimaryPartNumber} - {_selectedPart!.PartName}";

            if (_selectedCompany != null && _selectedPart != null)
            {
                PopulatePartField();
            }

        }

        private async void SaveCustomer_Click(object sender, RoutedEventArgs e)
        {
            //Create the company obj
            Company? c = _selectedCompany;

            //Create context
            using NovaDbContext context = new NovaDbContext();

            bool isNewCustomer = (c == null);

            //Create dialog Title and message will change depending on success/ error
            ContentDialog saveDialog = DialogHelper.CreateDialog("Temporary", "", "Okay", this.XamlRoot);

            TextBlock message = new TextBlock();
            message.Text = "";
            String updatedOrSaved = "";

            saveDialog.Content = message;

            //Validate name field

            if (string.IsNullOrWhiteSpace(CustomerNameText.Text))
            {
                message.Text = "Customer's name cannot be empty.";
            }

            //Error occured if message is not empty
            if (message.Text != "")
            {
                saveDialog.Title = "ERROR";
                await saveDialog.ShowAsync();
                return;
            }

            //Create new customer if new
            if (isNewCustomer)
            {
                c = new Company();
            }

            c!.Name = CustomerNameText.Text.Trim();
            c.ContactName = ContactNameText.Text.Trim();
            c.Phone = CustomerPhoneText.Text.Trim();
            c.Email = CustomerEmailText.Text.Trim();
            c.Address = CustomerAddressText.Text.Trim();
            c.Notes = CustomerNotesText.Text.Trim();
            c.IsCustomer = true;

            //Distinguish between add/ update
            if (isNewCustomer)
            {
                context.Companies.Add(c);
                updatedOrSaved = "saved";
            }
            else
            {
                context.Companies.Update(c);
                updatedOrSaved = "updated";
            }

            try
            {
                context.SaveChanges();
                saveDialog.Title = "SUCCESS";
                message.Text = $"'{c.Name}' was {updatedOrSaved}.";
                _selectedCompany = c;
            }
            catch (DbUpdateException)
            {
                saveDialog.Title = "ERROR";
                message.Text = $"'{c.Name}' was not {updatedOrSaved}.";
            }

            await saveDialog.ShowAsync();
        }

        private async void NewCustomer_Click(object sender, RoutedEventArgs args)
        {
            if (await DialogHelper.ConfirmClearAsync(this.XamlRoot, "customer")) {
                ClearAllFields();
            }
        }


        //HELPERS:

        //Fills all customer fields with saved data
        private void PopulateCompanyFields()
        {
            Company c = _selectedCompany!;

            CustomerNameText.Text = c.Name;
            ContactNameText.Text = c.ContactName;
            CustomerPhoneText.Text = c.Phone;
            CustomerEmailText.Text = c.Email;
            CustomerAddressText.Text = c.Address;
            CustomerNotesText.Text = c.Notes;
        }

        //Fills the customer part number box
        private void PopulatePartField()
        {
            PartCompany? pc = LinkHelper.FindExistingLink(_selectedPart!, _selectedCompany!);

            if (pc == null)
            {
                CustomerPartNumberText.Text = "";
                _selectedPartCompany = null;
                return;
            }
            else
            {
                CustomerPartNumberText.Text = pc.CompanyPartNumber;
                _selectedPartCompany = pc;
            }

        }

        //Resets all the fields on the customer page
        private void ClearAllFields()
        {
            CustomerSearchBar.Text = "";
            CustomerSearchResults.Items.Clear();

            CustomerNameText.Text = "";
            ContactNameText.Text = "";
            CustomerPhoneText.Text = "";
            CustomerEmailText.Text = "";
            CustomerAddressText.Text = "";
            CustomerNotesText.Text = "";

            CustomerPartSearchBar.Text = "";
            CustomerPartList.Items.Clear();
            LoadParts();

            SelectedCustomerPartText.Text = "No Selected Part";
            CustomerPartNumberText.Text = "";
            LatestCustomerQuoteText.Text = "$0.00";
            CustomerQuoteHistoryList.Items.Clear();

            _selectedCompany = null;
            _selectedPart = null;
            _selectedPartCompany = null;
        }

        //Populates part list upon opening and clearing page
        private void LoadParts()
        {
            using NovaDbContext context = new NovaDbContext();
            SearchHelper.LoadSearchList(CustomerPartList, context.Parts, "", hideOnEmpty: false);
        }
    }
}

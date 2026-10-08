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
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Nova.Views
{
    public sealed partial class VendorsPage : Page
    {
        private Company? _selectedCompany = null;
        private Part? _selectedPart = null;
        private PartCompany? _selectedPartCompany = null;

        public VendorsPage()
        {
            InitializeComponent();
            LoadParts();
        }

        //EVENT HANDLERS:
        private async void NewVendor_Click(object sender, RoutedEventArgs args)
        {
            if (await DialogHelper.ConfirmClearAsync(this.XamlRoot, "vendor"))
            {
                ClearAllFields();
            }

            return;
        }

        private void AddVendorCost_Click(object sender, RoutedEventArgs args)
        {

        }

        private void VendorPartList_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            ListView list = (ListView)sender;

            ListViewItem? item = list.SelectedItem as ListViewItem;

            if (item == null || item.Tag as Part == null)
            {
                return;
            }

            _selectedPart = (Part)item.Tag;

            if (_selectedCompany != null && _selectedPart != null)
            {
                PopulatePartFields();
            }
            
        }

        private async void SaveVendor_Click(object sender, RoutedEventArgs args)
        {
            //Create dialog. Title and message will change depending on success/ error
            ContentDialog saveDialog = DialogHelper.CreateDialog("Temporary", "", "Okay", this.XamlRoot);

            TextBlock message = new TextBlock();
            message.Text = "";

            saveDialog.Content = message;

            //Validate each field
            saveDialog.Title = "ERROR";

            if (string.IsNullOrWhiteSpace(VendorNameText.Text))
            {
                message.Text = "Vendor's name cannot be empty.";
            }
            else if (string.IsNullOrWhiteSpace(VendorNumberText.Text))
            {
                message.Text = "Vendor's number cannot be empty.";
            }

            //Error occured if message is not empty
            if (message.Text != "")
            {
                await saveDialog.ShowAsync();
                return;
            }

            Company? c = _selectedCompany!;
            bool isNewCompany = false;

            if (c == null)
            {
                c = new Company();
                isNewCompany = true;
            }

            c.Name = VendorNameText.Text.Trim();
            c.CompanyNumber = VendorNumberText.Text.Trim();
            c.ContactName = ContactNameText.Text.Trim();
            c.Phone = VendorPhoneText.Text.Trim();
            c.Email = VendorEmailText.Text.Trim();
            c.Terms = VendorTermsText.Text.Trim();
            c.Address = VendorAddressText.Text.Trim();
            c.Notes = VendorNotesText.Text.Trim();

            //Tag as vendor
            c.IsVendor = true;

            using NovaDbContext context = new NovaDbContext();

            //New company -> Add
            if (isNewCompany)
            {
                context.Companies.Add(c);
            }

            //Existing company -> Update
            else
            {
                context.Companies.Update(c);
            }

            try
            {
                context.SaveChanges();

                saveDialog.Title = "SUCCESS";

                //Picks between saved vs updated
                if (isNewCompany)
                {
                    message.Text = $"'{c.Name}' was saved.";
                }
                else
                {
                    message.Text = $"'{c.Name}' was updated.";
                }

                _selectedCompany = c;

            }
            catch (DbUpdateException)
            {
                saveDialog.Title = "ERROR";

                if (isNewCompany)
                {
                    message.Text = $"'{c.Name}' was not saved.";
                }
                else
                {
                    message.Text = $"'{c.Name}' was not updated.";
                }
            }

            await saveDialog.ShowAsync();

        }

        private void VendorSearchBar_TextChanged(object sender, TextChangedEventArgs args)
        {
            using NovaDbContext context = new NovaDbContext();
            SearchHelper.LoadSearchList(VendorSearchResults, context.Companies, VendorSearchBar.Text, SearchHelper.CompanyFilter.Vendor);
        }

        private void VendorPartSearchBar_TextChanged(object sender, TextChangedEventArgs args)
        {
            using NovaDbContext context = new NovaDbContext();
            SearchHelper.LoadSearchList(VendorPartList, context.Parts, VendorPartSearchBar.Text, hideOnEmpty: false);
        }

        private void VendorSearchResult_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            ListView list = (ListView)sender;

            ListViewItem? item = list.SelectedItem as ListViewItem;

            if (item == null || item.Tag as Company == null)
            {
                return;
            }

            _selectedCompany = (Company)item.Tag;

            PopulateVendorFields();

            if (_selectedCompany != null && _selectedPart != null)
            {
                PopulatePartFields();
            }
        }

        private async void SaveVendorPartNumber_Click(object sender, RoutedEventArgs args)
        {
            ContentDialog d = DialogHelper.CreateDialog("TEMPORARY", "", "Okay", this.XamlRoot);
            TextBlock msg = new TextBlock();
            d.Content = msg;

            if (_selectedCompany == null || _selectedPart == null)
            {
                d.Title = "ERROR";
                msg.Text = "Vendor and Part must BOTH be selected.";
                await d.ShowAsync();

                return;
            }

            PartCompany? pc = _selectedPartCompany;
            bool success;

            //Create a new link
            if (pc == null)
            {
                pc = await LinkHelper.CreateNewLink(this.XamlRoot, VendorPartNumberText.Text, _selectedCompany.Id, _selectedPart.Id, "Vendor");

                success = (pc != null);
            }
            //Update existing link
            else
            {
                success = await LinkHelper.UpdateLink(this.XamlRoot, VendorPartNumberText.Text, pc, "Vendor", LinkHelper.PageType.Company);
            }

            if (success)
            {
                _selectedPartCompany = pc;
            }

        }

        //HELPER FUNCTIONS:
        
        //Resets all fields on vendors page
        private void ClearAllFields()
        {
            VendorSearchBar.Text = "";
            VendorSearchResults.Items.Clear();

            VendorNameText.Text = "";
            VendorNumberText.Text = "";
            ContactNameText.Text = "";
            VendorPhoneText.Text = "";
            VendorEmailText.Text = "";
            VendorTermsText.Text = "";
            VendorAddressText.Text = "";
            VendorNotesText.Text = "";

            VendorPartSearchBar.Text = "";
            VendorPartList.Items.Clear();
            LoadParts();

            SelectedVendorPartText.Text = "No Part Selected";
            VendorPartNumberText.Text = "";
            LatestVendorCostText.Text = "$0.00";
            VendorPriceHistoryList.Items.Clear();

            _selectedCompany = null;
            _selectedPart = null;
            _selectedPartCompany = null;
        }

        private void PopulateVendorFields()
        {
            //Grab company
            Company c = _selectedCompany!;

            //Populate fields
            VendorNameText.Text = c.Name;
            VendorNumberText.Text = c.CompanyNumber;
            ContactNameText.Text = c.ContactName;
            VendorPhoneText.Text = c.Phone;
            VendorEmailText.Text = c.Email;
            VendorTermsText.Text = c.Terms;
            VendorAddressText.Text = c.Address;
            VendorNotesText.Text = c.Notes;
        }
        private void PopulatePartFields()
        {
            //Company and Part are never null when this function is called

            SelectedVendorPartText.Text = $"{_selectedPart!.PrimaryPartNumber} - {_selectedPart!.PartName}";

            PartCompany? pc = LinkHelper.FindExistingLink(_selectedPart!, _selectedCompany!);

            //If null, it's a new link
            if (pc == null)
            {
                VendorPartNumberText.Text = "";
                LatestVendorCostText.Text = "$0.00";
                VendorPriceHistoryList.Items.Clear();
                _selectedPartCompany = null;

                return;
            }

            VendorPartNumberText.Text = pc.CompanyPartNumber;
            _selectedPartCompany = pc;
        }

        //Loads all parts upon loading vendor page
        private void LoadParts()
        {
            using NovaDbContext context = new NovaDbContext();
            SearchHelper.LoadSearchList(VendorPartList, context.Parts, "", hideOnEmpty: false);
        }

    }
}

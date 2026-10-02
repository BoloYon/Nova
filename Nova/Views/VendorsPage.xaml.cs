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
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Nova.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VendorsPage : Page
    {
        public VendorsPage()
        {
            InitializeComponent();
        }

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

        }

        private async void SaveVendor_Click(object sender, RoutedEventArgs args)
        {
            //Create the company obj
            Company c = new Company();

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

            context.Companies.Add(c);

            try
            {
                context.SaveChanges();
                saveDialog.Title = "SUCCESS";
                message.Text = $"'{c.Name}' was saved.";
            }
            catch (DbUpdateException)
            {
                saveDialog.Title = "ERROR";
                message.Text = $"'{c.Name}' was not saved.";
            }

            await saveDialog.ShowAsync();
        }

        private void ClearAllFields()
        {
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

            SelectedVendorPartText.Text = "Selected Part";
            VendorPartNumberText.Text = "";
            LatestVendorCostText.Text = "$0.00";
            VendorPriceHistoryList.Items.Clear();
        }

    }
}

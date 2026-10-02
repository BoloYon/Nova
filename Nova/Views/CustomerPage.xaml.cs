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
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Nova.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CustomerPage : Page
    {
        public CustomerPage()
        {
            InitializeComponent();
        }

        private void AddCustomerQuote_Click(object sender, RoutedEventArgs args)
        {

        }

        private void CustomerPartList_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {

        }

        private async void SaveCustomer_Click(object sender, RoutedEventArgs e)
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

            if (string.IsNullOrWhiteSpace(CustomerNameText.Text))
            {
                message.Text = "Customer's name cannot be empty.";
            }

            //Error occured if message is not empty
            if (message.Text != "")
            {
                await saveDialog.ShowAsync();
                return;
            }

            c.Name = CustomerNameText.Text.Trim();
            c.ContactName = ContactNameText.Text.Trim();
            c.Phone = CustomerPhoneText.Text.Trim();
            c.Email = CustomerEmailText.Text.Trim();
            c.Address = CustomerAddressText.Text.Trim();
            c.Notes = CustomerNotesText.Text.Trim();

            c.IsCustomer = true;

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

        private async void NewCustomer_Click(object sender, RoutedEventArgs args)
        {
            if (await DialogHelper.ConfirmClearAsync(this.XamlRoot, "customer")) {
                ClearAllFields();
            }
        }

        private void ClearAllFields()
        {
            CustomerNameText.Text = "";
            ContactNameText.Text = "";
            CustomerPhoneText.Text = "";
            CustomerEmailText.Text = "";
            CustomerAddressText.Text = "";
            CustomerNotesText.Text = "";

            CustomerPartSearchBar.Text = "";
            CustomerPartList.Items.Clear();

            SelectedCustomerPartText.Text = "Selected Part";
            CustomerPartNumberText.Text = "";
            LatestCustomerQuoteText.Text = "$0.00";
            CustomerQuoteHistoryList.Items.Clear();
        }
    }
}

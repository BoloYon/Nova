using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Nova.Helpers
{
    public static class DialogHelper
    {
        //Makes a confirmation dialog for clearing and 
        public static async Task<bool> ConfirmClearAsync(XamlRoot xamlroot, string itemName)
        {
            //Make a confirmation Dialog
            ContentDialog confirmDialog = DialogHelper.CreateDialog("WARNING", "Yes", "No", xamlroot);

            TextBlock warningText = new TextBlock();
            warningText.Text = $"Are you sure you want to clear ALL fields for this {itemName}?";

            confirmDialog.Content = warningText;

            //Return true on primary button click
            ContentDialogResult result = await confirmDialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }

        //Makes a dialog
        public static ContentDialog CreateDialog(string title, string primaryText, string closeText, XamlRoot xamlroot)
        {
            ContentDialog d = new ContentDialog();
            d.Title = title;
            d.PrimaryButtonText = primaryText;
            d.CloseButtonText = closeText;
            d.XamlRoot = xamlroot;

            return d;
        }

    }
}

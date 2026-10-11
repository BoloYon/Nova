using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Helpers
{
    public static class DialogHelper
    {
        public class PriceDialogParts
        {
            public ContentDialog Dialog { get; set; } = null!;
            public TextBox DateTextBox { get; set; } = null!;
            public TextBox PriceTextBox { get; set; } = null!;
            public TextBlock ErrorText { get; set; } = null!;
        }

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

        //Creates a special price entry dialog
        public static PriceDialogParts CreatePriceDialog(XamlRoot xaml)
        {
            //Make a new dialog object
            ContentDialog addPriceDialog = DialogHelper.CreateDialog("Add Price Entry", "Add", "Cancel", xaml);

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

            PriceDialogParts newPD = new PriceDialogParts();
            newPD.Dialog = addPriceDialog;
            newPD.DateTextBox = dateTextBox;
            newPD.PriceTextBox = priceTextBox;
            newPD.ErrorText = errorText;

            return newPD;
        }

    }
}

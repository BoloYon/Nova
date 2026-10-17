using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
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

        public class ProFormaPartDialogParts
        {
            public ContentDialog Dialog { get; set; } = null!;
            public TextBox PartSearchBar { get; set; } = null!;
            public ListView PartSearchResults { get; set; } = null!;
            public TextBlock SelectedPartText { get; set; } = null!;
            public TextBox QuantityTextBox { get; set; } = null!;
            public TextBox UnitPriceTextBox { get; set; } = null!;
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
            d.DefaultButton = ContentDialogButton.Primary;
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

        public static ProFormaPartDialogParts CreateProFormaPartDialog(XamlRoot xaml)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Add Part",
                PrimaryButtonText = "Add Part",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = xaml
            };


            StackPanel mainPanel = new StackPanel
            {
                Spacing = 12,
                MinWidth = 420
            };


            //SEARCH LABEL
            TextBlock searchLabel = new TextBlock
            {
                Text = "Search Parts",
                FontSize = 14,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            };


            //SEARCH BAR
            TextBox searchBar = new TextBox
            {
                PlaceholderText = "Search by part number or name...",
                HorizontalAlignment = HorizontalAlignment.Stretch
            };


            //SEARCH RESULTS
            ListView searchResults = new ListView
            {
                MaxHeight = 160,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };


            //DIVIDER
            Rectangle divider = new Rectangle
            {
                Height = 1,
                Fill = new SolidColorBrush(
                    ColorHelper.FromArgb(255, 68, 58, 83)
                ),
                Margin = new Thickness(0, 4, 0, 4)
            };


            //SELECTED PART LABEL
            TextBlock selectedLabel = new TextBlock
            {
                Text = "Selected Part",
                FontSize = 14,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = new SolidColorBrush(
                    ColorHelper.FromArgb(255, 181, 181, 190)
                )
            };


            //SELECTED PART DISPLAY
            TextBlock selectedPartText = new TextBlock
            {
                Text = "No Part Selected",
                FontSize = 16,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Colors.White),
                TextWrapping = TextWrapping.Wrap
            };


            Border selectedPartBorder = new Border
            {
                Background = new SolidColorBrush(
                    ColorHelper.FromArgb(255, 41, 39, 48)
                ),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12),
                Child = selectedPartText
            };


            //QUANTITY + UNIT PRICE
            Grid valueGrid = new Grid
            {
                ColumnSpacing = 12
            };

            valueGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Star)
            });

            valueGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Star)
            });


            //QUANTITY
            StackPanel quantityPanel = new StackPanel
            {
                Spacing = 4
            };

            TextBlock quantityLabel = new TextBlock
            {
                Text = "Quantity *",
                FontSize = 14
            };

            TextBox quantityTextBox = new TextBox
            {
                PlaceholderText = "1"
            };

            quantityPanel.Children.Add(quantityLabel);
            quantityPanel.Children.Add(quantityTextBox);


            //UNIT PRICE
            StackPanel pricePanel = new StackPanel
            {
                Spacing = 4
            };

            TextBlock priceLabel = new TextBlock
            {
                Text = "Unit Price *",
                FontSize = 14
            };

            TextBox unitPriceTextBox = new TextBox
            {
                PlaceholderText = "0.00"
            };

            pricePanel.Children.Add(priceLabel);
            pricePanel.Children.Add(unitPriceTextBox);


            Grid.SetColumn(quantityPanel, 0);
            Grid.SetColumn(pricePanel, 1);

            valueGrid.Children.Add(quantityPanel);
            valueGrid.Children.Add(pricePanel);


            //ERROR MESSAGE
            TextBlock errorText = new TextBlock
            {
                Text = "",
                Foreground = new SolidColorBrush(Colors.Red),
                TextWrapping = TextWrapping.Wrap
            };


            //ADD EVERYTHING
            mainPanel.Children.Add(searchLabel);
            mainPanel.Children.Add(searchBar);
            mainPanel.Children.Add(searchResults);

            mainPanel.Children.Add(divider);

            mainPanel.Children.Add(selectedLabel);
            mainPanel.Children.Add(selectedPartBorder);

            mainPanel.Children.Add(valueGrid);

            mainPanel.Children.Add(errorText);


            dialog.Content = mainPanel;


            return new ProFormaPartDialogParts
            {
                Dialog = dialog,
                PartSearchBar = searchBar,
                PartSearchResults = searchResults,
                SelectedPartText = selectedPartText,
                QuantityTextBox = quantityTextBox,
                UnitPriceTextBox = unitPriceTextBox,
                ErrorText = errorText
            };
        }

    }
}
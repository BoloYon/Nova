using Microsoft.EntityFrameworkCore;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Nova.Data;
using Nova.Data.Models;
using Nova.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Nova.Views
{
    public sealed partial class ProFormaPage : Page
    {

        private Company? _selectedCustomer = null;
        private ProForma? _selectedProForma = null;
        private Part? _temporaryPart = null;

        public ProFormaPage()
        {
            InitializeComponent();
        }

        private void ProFormaSearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            using NovaDbContext context = new NovaDbContext();
            SearchHelper.LoadSearchList(ProFormaSearchResults, context.Companies, ProFormaSearchBar.Text, SearchHelper.CompanyFilter.Customer);
        }

        private void ProFormaSearchBar_FocusDisengaged(Control sender, FocusDisengagedEventArgs args)
        {
            ProFormaSearchResults.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }

        private void ProFormaSearchBar_FocusEngaged(Control sender, FocusEngagedEventArgs args)
        {
            ProFormaSearchResults.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        }

        private async void SaveProFormaButton_Click(object sender, RoutedEventArgs e)
        {
            Company? customer = _selectedCustomer;

            ContentDialog dialog = DialogHelper.CreateDialog("TEMP", "", "Okay", this.XamlRoot);
            TextBlock msg = new TextBlock();
            msg.Text = "";

            dialog.Content = msg;

            //Validations
            if (string.IsNullOrWhiteSpace(ProFormaNumberText.Text))
            {
                msg.Text = "Pro forma number cannot be empty.";
            }
            else if (ProFormaDatePicker.Date == null)
            {
                msg.Text = "You must choose a date.";
            }
            else if (string.IsNullOrWhiteSpace(ProFormaTermsText.Text))
            {
                msg.Text = "Terms cannot be empty.";
            }
            else if (customer == null)
            {
                msg.Text = "A customer must be selected.";
            }
            else if (string.IsNullOrWhiteSpace(ShipToText.Text))
            {
                msg.Text = "\"Ship To\" address cannot be empty";
            }
            else if (ProFormaItemsList.Items.Count == 0)
            {
                msg.Text = "Please add at least one part under \"Pro Forma Items\"";
            }

            if (msg.Text != "")
            {
                dialog.Title = "ERROR";
                await dialog.ShowAsync();
                return;
            }

            using NovaDbContext context = new NovaDbContext();
            ProForma? pf = _selectedProForma;
            string updatedOrSaved = "";


            if (pf == null)
            {
                pf = new ProForma();
                context.Add(pf);
                updatedOrSaved = "saved";
            }
            else
            {
                context.Update(pf);
                updatedOrSaved = "updated";
            }

            pf.ProformaNumber = ProFormaNumberText.Text.Trim();
            pf.Date = ProFormaDatePicker.Date!.Value.DateTime;
            pf.Terms = ProFormaTermsText.Text.Trim();
            pf.Notes = ProFormaNotesText.Text.Trim();

            pf.CustomerId = customer!.Id;
            pf.Customer = customer;
            pf.ShippingAddress = ShipToText.Text.Trim();

            foreach (ListViewItem item in ProFormaItemsList.Items)
            {
                if (!pf.Items.Contains((ProFormaItem)item.Tag))
                {
                    pf.Items.Add((ProFormaItem)item.Tag);
                }
            }

            try
            {
                await context.SaveChangesAsync();

                _selectedProForma = pf;

                dialog.Title = "SUCCESS";
                msg.Text = $"Pro Forma was {updatedOrSaved}.";
            }
            catch (DbUpdateException)
            {
                dialog.Title = "ERROR";
                msg.Text = $"Pro Forma could not be {updatedOrSaved}.";
            }

            await dialog.ShowAsync();

        }

        private async void AddPartButton_Click(object sender, RoutedEventArgs e)
        {
            DialogHelper.ProFormaPartDialogParts dialogParts = DialogHelper.CreateProFormaPartDialog(this.XamlRoot);
            ContentDialog dialog = dialogParts.Dialog;

            NovaDbContext context = new NovaDbContext();

            //Add event handlers to dialog searchbar
            dialogParts.PartSearchBar.TextChanged += (sender, e) => PartSearchBar_TextChanged(sender, e, dialogParts.PartSearchResults);
            dialogParts.PartSearchBar.LostFocus += (sender, e) => PartSearchBar_LostFocus(sender, e, dialogParts.PartSearchResults);
            dialogParts.PartSearchBar.GotFocus += (sender, e) => PartSearchBar_GotFocus(sender, e, dialogParts.PartSearchResults, dialogParts.PartSearchBar);

            //Get selection changed event
            dialogParts.PartSearchResults.SelectionChanged += (sender, e) => PartSearchResults_SelectionChanged(sender, e, dialogParts);

            //Add function to the primary button
            dialogParts.Dialog.PrimaryButtonClick += (sender, e) => Dialog_PrimaryButtonClick(sender, e, dialogParts);




            await dialog.ShowAsync();
            _temporaryPart = null; //Always reset part after adding or canceling
        }


        private async void Dialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args, DialogHelper.ProFormaPartDialogParts dialogParts)
        {
            dialogParts.ErrorText.Text = "";

            bool isValidPrice = PriceHelper.TryParsePriceEntry(dialogParts.UnitPriceTextBox.Text, out decimal parsedPrice);
            bool isValidQuantity = int.TryParse(dialogParts.QuantityTextBox.Text, out int result);

            int quantity;

            //Preserves placeholder if empty. (Before validation to preserve error order)
            if ((!isValidQuantity || result <= 0) && dialogParts.QuantityTextBox.Text != "")
            {
                quantity = -1; //Just so quantity can be assigned. Never actually reaches create pf item.
                dialogParts.ErrorText.Text = "You must enter a valid price";
            }
            else if (dialogParts.QuantityTextBox.Text == "")
            {
                quantity = 1;
            }
            else
            {
                quantity = result;
            }

            //Validation
            if (_temporaryPart == null)
            {
                dialogParts.ErrorText.Text = "A part must be selected.";
            }
            else if (!isValidPrice)
            {
                dialogParts.ErrorText.Text = "You must enter a valid price";
            }


            if (dialogParts.ErrorText.Text != "")
            {
                args.Cancel = true;
                return;
            }

            if (!CreateProFormaItem(_temporaryPart!, quantity, parsedPrice))
            {
                dialogParts.ErrorText.Text = "Pro Forma item already exists";
                args.Cancel = true;
                return;
            }

        }

        private void PartSearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e, DialogHelper.ProFormaPartDialogParts dialogParts)
        {
            ListView list = (ListView)sender;

            ListViewItem? item = list.SelectedItem as ListViewItem;

            if (item == null || item.Tag as Part == null)
            {
                return;
            }

            _temporaryPart = item.Tag as Part;

            dialogParts.SelectedPartText.Text = $"{_temporaryPart!.PrimaryPartNumber} - {_temporaryPart.PartName}";

            //Automatically fill the Unit Price IF _selectedCompany != null
            string price = GetUnitPrice();

            dialogParts.UnitPriceTextBox.Text = price;
        }

        private void PartSearchBar_TextChanged(object sender, TextChangedEventArgs e, ListView list)
        {
            TextBox searchBar = (TextBox)sender;

            NovaDbContext context = new NovaDbContext();
            SearchHelper.LoadSearchList(list, context.Parts, searchBar.Text, hideOnEmpty: false);
        }

        private void PartSearchBar_LostFocus(object sender, RoutedEventArgs e, ListView list)
        {
            list.Visibility = Visibility.Collapsed;
        }

        private void PartSearchBar_GotFocus(object sender, RoutedEventArgs e, ListView list, TextBox searchBar)
        {
            list.Visibility = Visibility.Visible;
            NovaDbContext context = new NovaDbContext();
            SearchHelper.LoadSearchList(list, context.Parts, searchBar.Text, hideOnEmpty: false);
        }

        private void RemoveProFormaItem_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void EditProFormaItem_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }


        //HELPER FUNCTIONS:
        private string GetUnitPrice()
        {
            if (_selectedCustomer == null || _temporaryPart == null)
            {
                return "";
            }

            PartCompany? pc = LinkHelper.FindExistingLink(_temporaryPart, _selectedCustomer);

            if (pc == null)
            {
                return "";
            }

            string latestPrice = PriceHelper.GetLatestPrice(pc.Id);

            //Preserves placeholder text
            if (latestPrice == "$0.00")
            {
                return "";
            }

            return latestPrice;
        }

        //Creates the actual proformaitem AND pfItem list item unless it already exists 
        private bool CreateProFormaItem(Part part, int quantity, decimal unitPrice)
        {
            ProFormaItem proFormaItem = new ProFormaItem();

            proFormaItem.PartId = part.Id;
            proFormaItem.Part = part;
            proFormaItem.Quantity = quantity;
            proFormaItem.UnitPrice = unitPrice;

            //Check if part id is already in the pfi list
            foreach (ListViewItem itm in ProFormaItemsList.Items)
            {
                int? itmPartId = (itm.Tag as ProFormaItem)?.PartId;
                if(itmPartId == part.Id)
                {
                    return false;
                }
            }

            ListViewItem item = new ListViewItem();

            item.Tag = proFormaItem;
            item.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            item.Padding = new Thickness(0);


            Border rowBorder = new Border
            {
                BorderBrush = new SolidColorBrush(
                    ColorHelper.FromArgb(255, 58, 55, 68)
                ),

                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(10, 8, 10, 8)
            };


            Grid rowGrid = new Grid
            {
                ColumnSpacing = 0
            };


            //PART NUMBER
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(1.2, GridUnitType.Star)
            });

            //DIVIDER
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(18)
            });

            //DESCRIPTION
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(2, GridUnitType.Star)
            });

            //DIVIDER
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(18)
            });

            //QUANTITY
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(0.7, GridUnitType.Star)
            });

            //DIVIDER
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(18)
            });

            //UNIT PRICE
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(0.9, GridUnitType.Star)
            });

            //DIVIDER
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(18)
            });

            //TOTAL
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(0.9, GridUnitType.Star)
            });

            //DIVIDER
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(18)
            });

            //ACTIONS
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(90)
            });


            //PART NUMBER
            TextBlock partNumberText = new TextBlock
            {
                Text = part.PrimaryPartNumber,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Margin = new Thickness(4, 0, 8, 0)
            };

            Grid.SetColumn(partNumberText, 0);


            //DESCRIPTION
            TextBlock descriptionText = new TextBlock
            {
                Text = part.Description,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,

                // Extra space on the right intentionally makes
                // the ellipsis appear before it gets too close to Qty.
                Margin = new Thickness(8, 0, 20, 0)
            };

            Grid.SetColumn(descriptionText, 2);


            //QUANTITY
            TextBlock quantityText = new TextBlock
            {
                Text = quantity.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Grid.SetColumn(quantityText, 4);


            //UNIT PRICE
            TextBlock unitPriceText = new TextBlock
            {
                Text = unitPrice.ToString("C"),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Grid.SetColumn(unitPriceText, 6);


            //TOTAL
            TextBlock totalText = new TextBlock
            {
                Text = (quantity * unitPrice).ToString("C"),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            };

            Grid.SetColumn(totalText, 8);


            //DIVIDERS
            for (int column = 1; column <= 9; column += 2)
            {
                Rectangle divider = new Rectangle
                {
                    Width = 1,
                    Fill = new SolidColorBrush(
                        ColorHelper.FromArgb(255, 66, 62, 76)
                    ),

                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Margin = new Thickness(0, 2, 0, 2)
                };

                Grid.SetColumn(divider, column);
                rowGrid.Children.Add(divider);
            }


            //ACTION BUTTONS
            StackPanel actionPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };


            Button editButton = new Button
            {
                Content = new SymbolIcon(Symbol.Edit),
                Background = new SolidColorBrush(Colors.Transparent),
                BorderThickness = new Thickness(0),
                Padding = new Thickness(8),
                Tag = item
            };

            ToolTipService.SetToolTip(editButton, "Edit item");


            Button removeButton = new Button
            {
                Content = new SymbolIcon(Symbol.Delete),
                Background = new SolidColorBrush(Colors.Transparent),
                BorderThickness = new Thickness(0),
                Padding = new Thickness(8),
                Tag = item
            };

            ToolTipService.SetToolTip(removeButton, "Remove item");


            editButton.Click += EditProFormaItem_Click;
            removeButton.Click += RemoveProFormaItem_Click;


            actionPanel.Children.Add(editButton);
            actionPanel.Children.Add(removeButton);

            Grid.SetColumn(actionPanel, 10);


            rowGrid.Children.Add(partNumberText);
            rowGrid.Children.Add(descriptionText);
            rowGrid.Children.Add(quantityText);
            rowGrid.Children.Add(unitPriceText);
            rowGrid.Children.Add(totalText);
            rowGrid.Children.Add(actionPanel);


            rowBorder.Child = rowGrid;
            item.Content = rowBorder;

            ProFormaItemsList.Items.Add(item);

            return true;
        }
    }
}

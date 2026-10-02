using Nova.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Nova
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        //Constructor
        
        public MainWindow()
        {
            InitializeComponent();
        }

        //State Variables
        string? prevItem = null;

        //Event Handlers

        private void MainNavigation_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {

            string? clickedItem = args.InvokedItemContainer?.Tag?.ToString();

            //Safety Check
            if (clickedItem == null) 
            {
                return;
            }

            //Avoid refreshing the same page from the side-bar
            if (prevItem != clickedItem) {

                if (clickedItem == "Home")
                {
                    ContentFrame.Navigate(typeof(HomePage));
                }

                else if (clickedItem == "Customer")
                {
                    ContentFrame.Navigate(typeof(CustomerPage));
                }

                else if (clickedItem == "Vendors")
                {
                    ContentFrame.Navigate(typeof(VendorsPage));
                }

                else if (clickedItem == "Parts")
                {
                    ContentFrame.Navigate(typeof(PartsPage));
                }


            }

            prevItem = clickedItem;
        }

        //Helper Functions
    }

}

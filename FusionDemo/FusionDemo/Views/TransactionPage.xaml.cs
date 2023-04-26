using FusionDemo.Models;
using FusionDemo.ViewModels;
using FusionDemo.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FusionDemo.Views
{
    public partial class TransactionPage : ContentPage
    {
        public TransactionPage()
        {
            InitializeComponent();
            Shell.Current.Navigated += Current_Navigated;
        }

        private async void Current_Navigated(object sender, ShellNavigatedEventArgs e)
        {
            if (e.Current.Location.OriginalString == $"//{nameof(TransactionPage)}")
            {
                await (BindingContext as BaseViewModel).OnNavigatedTo();
            }
        }
    }
}
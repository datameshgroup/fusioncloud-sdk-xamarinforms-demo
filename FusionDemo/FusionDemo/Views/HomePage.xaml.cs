using FusionDemo.ViewModels;
using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FusionDemo.Views
{
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
            Shell.Current.Navigated += Current_Navigated;
        }

        private async void Current_Navigated(object sender, ShellNavigatedEventArgs e)
        {
            if (e.Current.Location.OriginalString == $"//{nameof(HomePage)}")
            {
                await (BindingContext as HomeViewModel).OnNavigatedTo();
            }
        }
    }
}
using FusionDemo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Plugin.DateTimePicker;

namespace FusionDemo.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OtherFieldsPage : ContentPage
    {
        public OtherFieldsPage()
        {
            InitializeComponent();
            PickUpDateTimePicker.MinimumDate = DateTime.Today.AddDays(-7);
            PickUpDateTimePicker.MaximumDate = DateTime.Today.AddDays(1);

            DestinationDateTimePicker.MinimumDate = DateTime.Today.AddDays(-7);
            DestinationDateTimePicker.MaximumDate = DateTime.Today.AddDays(1);

            DateTime defaultDateTime = DateTime.Now;

            PickUpDateTimePicker.SelectedDate = defaultDateTime.AddDays(-1);
            PickUpDateTimePicker.SelectedHour = defaultDateTime.Hour;
            PickUpDateTimePicker.SelectedMinute = defaultDateTime.Minute;            

            DestinationDateTimePicker.SelectedDate = defaultDateTime;
            DestinationDateTimePicker.SelectedHour = defaultDateTime.Hour;
            DestinationDateTimePicker.SelectedMinute = defaultDateTime.Minute;
        }
    }
}
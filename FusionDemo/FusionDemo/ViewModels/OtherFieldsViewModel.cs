using FusionDemo.Models;
using FusionDemo.Views;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FusionDemo.ViewModels
{
    public class OtherFieldsViewModel : BaseViewModel
    {
        public Command SaveCommand { get; }

        public OtherFieldsViewModel()
        {
            Title = "Other Fields";
            SaveCommand = new Command(OnSaveClicked);
        }

        private async void OnSaveClicked(object obj)
        {
            // Need to recreate the Payment if payment settings have changed
            Settings.CreatePayment();

            // Navigate 
            await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
        }

        public override Task OnNavigatedTo()
        {
            return Task.CompletedTask;
        }

        #region Properties

        public string OperatorID
        {
            get => Settings.OperatorID;
            set
            {
                Settings.OperatorID = value;
                OnPropertyChanged(nameof(OperatorID));
            }
        }

        public string ShiftNumber
        {
            get => Settings.ShiftNumber;
            set
            {
                Settings.ShiftNumber = value;
                OnPropertyChanged(nameof(ShiftNumber));
            }
        }

        public string TransactionID
        {
            get => Settings.TransactionID;
            set
            {
                Settings.TransactionID = value;
                OnPropertyChanged(nameof(TransactionID));
            }
        }

        public string SaleTransactionTimeStamp
        {
            get => Settings.SaleTransactionTimeStamp;
            set
            {
                if (DateTime.TryParse(value, out var date))
                {
                    Settings.SaleTransactionTimeStamp = value;
                    OnPropertyChanged(nameof(SaleTransactionTimeStamp));
                }                
            }
        }

        public String DeviceID
        {
            get => Settings.DeviceID;
            set
            {
                Settings.DeviceID = value;
                OnPropertyChanged(nameof(DeviceID));
            }
        }

        public String BusinessID
        {
            get => Settings.BusinessID;
            set
            {
                Settings.BusinessID = value;
                OnPropertyChanged(nameof(BusinessID));
            }
        }

        public String RegisteredIdentifier
        {
            get => Settings.RegisteredIdentifier;
            set
            {
                Settings.RegisteredIdentifier = value;
                OnPropertyChanged(nameof(RegisteredIdentifier));
            }
        }

        public String SiteID
        {
            get => Settings.SiteID;
            set
            {
                Settings.SiteID = value;
                OnPropertyChanged(nameof(SiteID));
            }
        }

        public bool IsWheelchairEnabled
        {
            get => Settings.IsWheelchairEnabled;
            set
            {
                Settings.IsWheelchairEnabled = value;
                OnPropertyChanged(nameof(IsWheelchairEnabled));
            }
        }

        public decimal TotalDistanceTravelled
        {
            get => Settings.TotalDistanceTravelled;
            set
            {
                Settings.TotalDistanceTravelled = value;
                OnPropertyChanged(nameof(TotalDistanceTravelled));
            }
        }

        public String PickUpStopName
        {
            get => Settings.PickUpStopName;
            set
            {
                Settings.PickUpStopName = value;
                OnPropertyChanged(nameof(PickUpStopName));
            }
        }

        public String PickUpLatitude
        {
            get => Settings.PickUpLatitude;
            set
            {
                Settings.PickUpLatitude = value;
                OnPropertyChanged(nameof(PickUpLatitude));
            }
        }

        public String PickUpLongitude
        {
            get => Settings.PickUpLongitude;
            set
            {
                Settings.PickUpLongitude = value;
                OnPropertyChanged(nameof(PickUpLongitude));
            }
        }

        public String PickUpTimeStamp
        {
            get => Settings.PickUpTimeStamp;
            set
            {
                Settings.PickUpTimeStamp = value;
                OnPropertyChanged(nameof(PickUpTimeStamp));
            }
        }

        public String DestinationStopName
        {
            get => Settings.DestinationStopName;
            set
            {
                Settings.DestinationStopName = value;
                OnPropertyChanged(nameof(DestinationStopName));
            }
        }

        public String DestinationLatitude
        {
            get => Settings.DestinationLatitude;
            set
            {
                Settings.DestinationLatitude = value;
                OnPropertyChanged(nameof(DestinationLatitude));
            }
        }

        public String DestinationLongitude
        {
            get => Settings.DestinationLongitude;
            set
            {
                Settings.DestinationLongitude = value;
                OnPropertyChanged(nameof(DestinationLongitude));
            }
        }

        public String DestinationTimeStamp
        {
            get => Settings.DestinationTimeStamp;
            set
            {              
                Settings.DestinationTimeStamp = value;
                OnPropertyChanged(nameof(DestinationTimeStamp));
            }
        }

        public DateTime MaxDateTime
        {
            get => DateTime.Now;
        }

        public DateTime MinDateTime
        {
            get => DateTime.MinValue;
        }

        #endregion
    }
}

using DataMeshGroup.Fusion.Model.Transit;
using DataMeshGroup.Fusion.Model;
using FusionDemo.Models;
using FusionDemo.Views;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using Xamarin.Essentials;
using System.Numerics;
using static System.Net.Mime.MediaTypeNames;
using System.Transactions;
using FusionDemo.Util;
using FusionSaleItem = DataMeshGroup.Fusion.Model.SaleItem;
using SaleItem = FusionDemo.Models.SaleItem;

namespace FusionDemo.ViewModels
{
    public class OtherFieldsViewModel : BaseViewModel
    {
        private PaymentRequest currentPaymentRequest = null;

        public Command SaveCommand { get; }      

        public OtherFieldsViewModel()
        {
            Title = "Other Fields";
            SaveCommand = new Command(OnSaveClicked);                       

            PopulatePayment();
        }

        public void PopulatePayment()
        {
            currentPaymentRequest = Settings.Payment.Request;
            
            OperatorID = Settings.OperatorID;
            ShiftNumber = Settings.ShiftNumber;

            SaleData saleData = currentPaymentRequest.SaleData;            
            TransactionID = saleData.SaleTransactionID.TransactionID;
            SaleTransactionTimeStampDate = saleData.SaleTransactionID.TimeStamp.Date;
            SaleTransactionTimeStampTimeSpan = saleData.SaleTransactionID.TimeStamp.TimeOfDay;
            DeviceID = saleData.SaleTerminalData.DeviceID;
            BusinessID = saleData.SponsoredMerchant.BusinessID;
            RegisteredIdentifier = saleData.SponsoredMerchant.RegisteredIdentifier;
            SiteID = saleData.SponsoredMerchant.SiteID;

            TransitData transitData = currentPaymentRequest.ExtensionData.TransitData;
            IsWheelchairEnabled = transitData.IsWheelchairEnabled;
            TotalDistanceTravelled = transitData.Trip.TotalDistanceTravelled;

            PickUpStopName = transitData.Trip.Pickup.StopName;
            PickUpLatitude = transitData.Trip.Pickup.Latitude;
            PickUpLongitude  = transitData.Trip.Pickup.Longitude;

            MinimumTripDateTime = DateTime.Today.AddDays(-7);
            MaximumTripDateTime = DateTime.Today.AddDays(1);

            PickUpDate = transitData.Trip.Pickup.Timestamp.Date;
            PickUpTimeSpan = transitData.Trip.Pickup.Timestamp.TimeOfDay;
            
            DestinationStopName = transitData.Trip.Destination.StopName;
            DestinationLatitude = transitData.Trip.Destination.Latitude;
            DestinationLongitude = transitData.Trip.Destination.Longitude;
            DestinationDate = transitData.Trip.Destination.Timestamp.Date;
            DestinationTimeSpan = transitData.Trip.Destination.Timestamp.TimeOfDay;            
        }        

        private async void OnSaveClicked(object obj)
        {
            if ((DateTime.Compare(PickUpDateTime, DestinationDateTime) < 0))
            {                
                // Need to recreate the Payment if payment settings have changed
                Settings.CreatePayment(CreatePaymentRequest());

                // Navigate 
                await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
            }
        }       

        public override Task OnNavigatedTo()
        {
            return Task.CompletedTask;
        }

        private PaymentRequest CreatePaymentRequest()
        {
            // Construct payment request
            PaymentRequest paymentRequest = new PaymentRequest()
            {
                SaleData = new SaleData()
                {
                    OperatorID = this.OperatorID,
                    ShiftNumber = this.ShiftNumber,
                    SaleTransactionID = new TransactionIdentification()
                    {
                        TransactionID = this.TransactionID,
                        TimeStamp = this.SaleTransactionTimeStamp
                    },
                    SaleTerminalData = new SaleTerminalData(false)
                    {
                        DeviceID = this.DeviceID
                    },
                    SponsoredMerchant = new SponsoredMerchant()
                    {
                        BusinessID = this.BusinessID,
                        RegisteredIdentifier = this.RegisteredIdentifier,
                        SiteID = this.SiteID
                    }
                },
                ExtensionData = new ExtensionData()
                {
                    TransitData = new TransitData()
                    {
                        IsWheelchairEnabled = this.IsWheelchairEnabled,
                        Trip = new Trip()
                        {
                            TotalDistanceTravelled = this.TotalDistanceTravelled,
                            Pickup = new Stop()
                            {
                                StopIndex = 0,
                                StopName = this.PickUpStopName,
                                Latitude = this.PickUpLatitude,
                                Longitude = this.PickUpLongitude,
                                Timestamp = this.PickUpDateTime
                            },
                            Destination = new Stop()
                            {
                                StopIndex = 1,
                                StopName = this.DestinationStopName,
                                Latitude = this.DestinationLatitude,
                                Longitude = this.DestinationLongitude,
                                Timestamp = this.DestinationDateTime
                            }
                        }
                    }
                },
                PaymentTransaction = currentPaymentRequest.PaymentTransaction
            };            

            return paymentRequest;
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

        string transactionID = "0347d00e-5d13-4043-b92b-6bf32381ab16";
        public string TransactionID
        {
            get => transactionID;
            set
            {
                transactionID = value;
                OnPropertyChanged(nameof(TransactionID));
            }
        }

        public DateTime SaleTransactionTimeStamp
        {
            get
            {
                return saleTransactionTimeStampDate.Date.AddHours(saleTransactionTimeStampTimeSpan.Hours).AddMinutes(saleTransactionTimeStampTimeSpan.Minutes);
            }
        }

        DateTime saleTransactionTimeStampDate = DateTime.Today;
        public DateTime SaleTransactionTimeStampDate
        {
            get => saleTransactionTimeStampDate;
            set
            {
                saleTransactionTimeStampDate = value;
                OnPropertyChanged(nameof(SaleTransactionTimeStampDate));
            }
        }

        TimeSpan saleTransactionTimeStampTimeSpan = DateTime.Now.TimeOfDay;
        public TimeSpan SaleTransactionTimeStampTimeSpan
        {
            get => saleTransactionTimeStampTimeSpan;
            set
            {
                saleTransactionTimeStampTimeSpan = value;
                OnPropertyChanged(nameof(SaleTransactionTimeStampTimeSpan));
            }
        }

        string deviceID = "58df5074-0f6d-41be-9b4f-bf3de3197ddd";
        public string DeviceID
        {
            get => deviceID;
            set
            {
                deviceID = value;
                OnPropertyChanged(nameof(DeviceID));
            }
        }

        string businessID = "50110219460";
        public string BusinessID
        {
            get => businessID;
            set
            {
                businessID = value;
                OnPropertyChanged(nameof(BusinessID));
            }
        }

        string registeredIdentifier = "TestClient";
        public string RegisteredIdentifier
        {
            get => registeredIdentifier;
            set
            {
                registeredIdentifier = value;
                OnPropertyChanged(nameof(RegisteredIdentifier));
            }
        }

        string siteID = "719428ed-8c98-4a1a-8b4f-853bbaa0a154";
        public string SiteID
        {
            get => siteID;
            set
            {
                siteID = value;
                OnPropertyChanged(nameof(SiteID));
            }
        }

        bool isWheelchairEnabled = true;
        public bool IsWheelchairEnabled
        {
            get => isWheelchairEnabled;
            set
            {
                isWheelchairEnabled = value;
                OnPropertyChanged(nameof(IsWheelchairEnabled));
            }
        }

        decimal? totalDistanceTravelled = 29.4M;
        public decimal? TotalDistanceTravelled
        {
            get => totalDistanceTravelled;
            set
            {
                totalDistanceTravelled = value;
                OnPropertyChanged(nameof(TotalDistanceTravelled));
            }
        }

        string pickUpStopName = "Richmond";
        public string PickUpStopName
        {
            get => pickUpStopName;
            set
            {
                pickUpStopName = value;
                OnPropertyChanged(nameof(PickUpStopName));
            }
        }

        string pickUpLatitude = "-37.82274517047244";
        public string PickUpLatitude
        {
            get => pickUpLatitude;
            set
            {
                pickUpLatitude = value;
                OnPropertyChanged(nameof(PickUpLatitude));
            }
        }

        string pickUpLongitude = "144.98394642094434";
        public string PickUpLongitude
        {
            get => pickUpLongitude;
            set
            {
                pickUpLongitude = value;
                OnPropertyChanged(nameof(PickUpLongitude));
            }
        }

        public DateTime MinimumTripDateTime { get; private set; }

        public DateTime MaximumTripDateTime { get; private set; }
        
        public DateTime PickUpDateTime
        {
            get
            {
                return pickUpDate.Date.AddHours(pickUpTimeSpan.Hours).AddMinutes(pickUpTimeSpan.Minutes);
            }
        }

        DateTime pickUpDate = DateTime.Today;
        public DateTime PickUpDate
        {
            get => pickUpDate;
            set
            {
                pickUpDate = value;
                OnPropertyChanged(nameof(PickUpDate));
            }
        }

        TimeSpan pickUpTimeSpan = DateTime.Now.TimeOfDay;
        public TimeSpan PickUpTimeSpan
        {
            get => pickUpTimeSpan;
            set
            {
                pickUpTimeSpan = value;
                OnPropertyChanged(nameof(PickUpTimeSpan));
            }
        }        

        string destinationStopName = "Beaumaris";
        public string DestinationStopName
        {
            get => destinationStopName;
            set
            {
                destinationStopName = value;
                OnPropertyChanged(nameof(DestinationStopName));
            }
        }

        string destinationLatitude = "-37.988864997462048";
        public string DestinationLatitude
        {
            get => destinationLatitude;
            set
            {
                destinationLatitude = value;
                OnPropertyChanged(nameof(DestinationLatitude));
            }
        }

        string destinationLongitude = "145.04484379736329";
        public string DestinationLongitude
        {
            get => destinationLongitude;
            set
            {
                destinationLongitude = value;
                OnPropertyChanged(nameof(DestinationLongitude));
            }
        }

        public DateTime DestinationDateTime
        {
            get
            {
                return destinationDate.Date.AddHours(destinationTimeSpan.Hours).AddMinutes(destinationTimeSpan.Minutes);

            }
        }

        DateTime destinationDate = DateTime.Today;
        public DateTime DestinationDate
        {
            get => destinationDate;
            set
            {
                destinationDate = value;
                OnPropertyChanged(nameof(DestinationDate));
            }
        }

        TimeSpan destinationTimeSpan = DateTime.Now.TimeOfDay;
        public TimeSpan DestinationTimeSpan
        {
            get => destinationTimeSpan;
            set
            {
                destinationTimeSpan = value;
                OnPropertyChanged(nameof(DestinationTimeSpan));
            }
        }        
                
        #endregion
    }
}

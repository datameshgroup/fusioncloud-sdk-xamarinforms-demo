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
            Settings.CreatePayment(CreatePaymentRequest());

            // Navigate 
            await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
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
                        TimeStamp = DateTime.Parse(this.SaleTransactionTimeStamp)
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
                                Timestamp = DateTime.Parse(this.PickUpTimeStamp)
                            },
                            Destination = new Stop()
                            {
                                StopIndex = 1,
                                StopName = this.DestinationStopName,
                                Latitude = this.DestinationLatitude,
                                Longitude = this.DestinationLongitude,
                                Timestamp = DateTime.Parse(this.DestinationTimeStamp)
                            }
                        }
                    }
                }
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

        string saleTransactionTimeStamp = DateTime.Now.ToString();
        public string SaleTransactionTimeStamp
        {
            get => saleTransactionTimeStamp;
            set
            {
                if (DateTime.TryParse(value, out var date))
                {
                    saleTransactionTimeStamp = value;
                    OnPropertyChanged(nameof(SaleTransactionTimeStamp));
                }                
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

        decimal totalDistanceTravelled = 29.4M;
        public decimal TotalDistanceTravelled
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

        string pickUpTimeStamp = "2023-04-06T03:00:15+0000";
        public string PickUpTimeStamp
        {
            get => pickUpTimeStamp;
            set
            {
                pickUpTimeStamp = value;
                OnPropertyChanged(nameof(PickUpTimeStamp));
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

        string destinationTimeStamp = "2023-04-06T03:39:30+0000";
        public string DestinationTimeStamp
        {
            get => destinationTimeStamp;
            set
            {              
                destinationTimeStamp = value;
                OnPropertyChanged(nameof(DestinationTimeStamp));
            }
        }  

        #endregion
    }
}

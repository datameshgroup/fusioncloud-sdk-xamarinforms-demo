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
        private bool updateAddSaleItemBtn = true;

        private SaleItem currentSelectedItem = null;

        private UnitOfMeasure? unitOfMeasure = null;
        public Command SaveCommand { get; }
        public Command SelectSaleItemCommand { get; }
        public Command DeleteSaleItemCommand { get; }
        public Command AddSaleItemCommand { get; }        

        public OtherFieldsViewModel()
        {
            Title = "Other Fields";
            SaveCommand = new Command(OnSaveClicked);
            SelectSaleItemCommand = new Command<object>(OnSelectSaleItem);
            DeleteSaleItemCommand = new Command(OnDeleteSaleItem);
            AddSaleItemCommand = new Command(OnAddSaleItem);                        

            unitOfMeasureList = new List<string>();
            unitOfMeasureList.AddRange(Enum.GetNames(typeof(UnitOfMeasure)));

            PopulatePayment();
        }

        public void PopulatePayment()
        {
            PaymentRequest paymentRequest = Settings.Payment.Request;
            OperatorID = Settings.OperatorID;
            ShiftNumber = Settings.ShiftNumber;

            SaleData saleData = paymentRequest.SaleData;            
            TransactionID = saleData.SaleTransactionID.TransactionID;
            SaleTransactionTimeStamp = saleData.SaleTransactionID.TimeStamp.ToString();
            DeviceID = saleData.SaleTerminalData.DeviceID;
            BusinessID = saleData.SponsoredMerchant.BusinessID;
            RegisteredIdentifier = saleData.SponsoredMerchant.RegisteredIdentifier;
            SiteID = saleData.SponsoredMerchant.SiteID;

            TransitData transitData = paymentRequest.ExtensionData.TransitData;
            IsWheelchairEnabled = transitData.IsWheelchairEnabled;
            TotalDistanceTravelled = transitData.Trip.TotalDistanceTravelled;

            PickUpStopName = transitData.Trip.Pickup.StopName;
            PickUpLatitude = transitData.Trip.Pickup.Latitude;
            PickUpLongitude  = transitData.Trip.Pickup.Longitude;
            PickUpDate = transitData.Trip.Pickup.Timestamp.Date;
            PickUpHour = transitData.Trip.Pickup.Timestamp.Hour;
            PickUpMinute = transitData.Trip.Pickup.Timestamp.Minute;

            DestinationStopName = transitData.Trip.Destination.StopName;
            DestinationLatitude = transitData.Trip.Destination.Latitude;
            DestinationLongitude = transitData.Trip.Destination.Longitude;
            DestinationDate = transitData.Trip.Destination.Timestamp.Date;
            DestinationHour = transitData.Trip.Destination.Timestamp.Hour;
            DestinationMinute = transitData.Trip.Destination.Timestamp.Minute;

            SaleItemsList = new List<SaleItem>();
            if (Settings.Payment.Request.PaymentTransaction.SaleItem != null)
            {
                foreach (FusionSaleItem saleItem in Settings.Payment.Request.PaymentTransaction.SaleItem)
                {
                    SaleItemsList.Add(new SaleItem() {
                        ItemID = saleItem.ItemID,
                        ProductCode = saleItem.ProductCode,
                        ProductLabel = saleItem.ProductLabel,
                        UnitOfMeasure = saleItem.UnitOfMeasure,
                        UnitPrice = saleItem.UnitPrice,
                        Quantity = saleItem.Quantity,
                        ItemAmount = saleItem.ItemAmount,
                        Tags = saleItem.Tags
                    });
                }
            }
            IsSaleItemListVisible = (saleItemsList != null) && (saleItemsList.Count > 0);
        }

        private async void OnSaveClicked(object obj)
        {
            if ((DateTime.Compare(PickUpDateTime, DestinationDateTime) < 0) && (saleItemsList != null) && (saleItemsList.Count > 0))
            {                
                // Need to recreate the Payment if payment settings have changed
                Settings.CreatePayment(CreatePaymentRequest());

                // Navigate 
                await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
            }
        }

        private void OnSelectSaleItem(object item)
        {
            currentSelectedItem = (item as SaleItem);
            IsDeleteSaleItemEnabled = (currentSelectedItem != null);
        }

        private void OnDeleteSaleItem()
        {
            if (currentSelectedItem != null)
            {
                List<SaleItem> siList = new List<SaleItem>();
                if (saleItemsList != null)
                {                    
                    foreach (SaleItem si in saleItemsList)
                    {
                        if ((si.ItemID != currentSelectedItem.ItemID) || (si.ProductCode != currentSelectedItem.ProductCode) || (si.ProductLabel != currentSelectedItem.ProductLabel))
                        {
                            siList.Add(si);
                        }
                    }
                    SaleItemsList = siList;
                }
                IsSaleItemListVisible = (siList.Count > 0);
                currentSelectedItem = null;
                IsDeleteSaleItemEnabled = false;                
            }
        }        

        private void OnAddSaleItem()
        {
            if (!IsAddSaleItemEnabled)
                return;
                        
            List<SaleItem> siList = new List<SaleItem>();
            if (saleItemsList != null)            
            {
                siList.AddRange(saleItemsList);
            }

            List<string> tags = new List<string>();
            tags.Add(Tags);

            siList.Add(new SaleItem()
            {
                ItemID = (int)ItemID,
                ProductCode = ProductCode,
                ProductLabel = ProductLabel,
                UnitOfMeasure = (UnitOfMeasure)unitOfMeasure,
                UnitPrice = (decimal)UnitPrice,
                Quantity = (decimal)Quantity,
                ItemAmount = (decimal)ItemAmount,
                Tags = tags
            });

            IsDeleteSaleItemEnabled = false;
            
            SaleItemsList = siList;

            IsSaleItemListVisible = (siList.Count > 0);

            updateAddSaleItemBtn = false; 

            ItemID = null;
            ProductCode = null;
            ProductLabel = null;
            UnitOfMeasureSelectedIndex = -1;
            UnitPrice = null;
            Quantity = null;
            ItemAmount = null;
            Tags = null;
            IsAddSaleItemEnabled = false;

            updateAddSaleItemBtn = true;
        }

        private void CheckIfCanAddSaleItem()
        {
            if (updateAddSaleItemBtn) 
            { 
                IsAddSaleItemEnabled = (ItemID != null) && (ProductCode != null) && (ProductLabel != null) && (unitOfMeasure != null) && (UnitPrice != null) && (Quantity != null) && (ItemAmount != null);
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
                PaymentTransaction = new PaymentTransaction()
            };           

            foreach(SaleItem saleItem in saleItemsList)
            {
                    paymentRequest.AddSaleItem(itemID: saleItem.ItemID,
                        productCode: saleItem.ProductCode, 
                        productLabel: saleItem.ProductLabel, 
                        unitOfMeasure: saleItem.UnitOfMeasure, 
                        unitPrice:saleItem.UnitPrice, 
                        quantity: saleItem.Quantity, 
                        itemAmount: saleItem.ItemAmount, 
                        tags: saleItem.Tags);
            }

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

        public DateTime PickUpDateTime
        {
            get
            {
                return pickUpDate.Date.AddHours(pickUpHour).AddMinutes(pickUpMinute);
            }
        }

        DateTime pickUpDate;
        public DateTime PickUpDate
        {
            get => pickUpDate;
            set
            {
                pickUpDate = value;
                OnPropertyChanged(nameof(PickUpDate));
            }
        }

        int pickUpHour;
        public int PickUpHour
        {
            get => pickUpHour;
            set
            {
                pickUpHour = value;
                OnPropertyChanged(nameof(PickUpHour));
            }
        }

        int pickUpMinute;
        public int PickUpMinute
        {
            get => pickUpMinute;
            set
            {
                pickUpMinute = value;
                OnPropertyChanged(nameof(PickUpMinute));
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
                return destinationDate.Date.AddHours(destinationHour).AddMinutes(destinationMinute);

            }
        }

        DateTime destinationDate;
        public DateTime DestinationDate
        {
            get => destinationDate;
            set
            {
                destinationDate = value;
                OnPropertyChanged(nameof(DestinationDate));
            }
        }

        int destinationHour;
        public int DestinationHour
        {
            get => destinationHour;
            set
            {
                destinationHour = value;
                OnPropertyChanged(nameof(DestinationHour));
            }
        }

        int destinationMinute;
        public int DestinationMinute
        {
            get => destinationMinute;
            set
            {
                destinationMinute = value;
                OnPropertyChanged(nameof(DestinationMinute));
            }
        }

        private List<SaleItem> saleItemsList;

        public List<SaleItem> SaleItemsList
        {
            get => saleItemsList;
            set { 
                saleItemsList = value;
                OnPropertyChanged(nameof(SaleItemsList));
            }
        }

        bool isDeleteSaleItemEnabled = false;
        public bool IsDeleteSaleItemEnabled
        {
            get => isDeleteSaleItemEnabled;
            set
            {
                isDeleteSaleItemEnabled = value;
                OnPropertyChanged(nameof(IsDeleteSaleItemEnabled));
            }
        }

        bool isSaleItemListVisible = false;
        public bool IsSaleItemListVisible
        {
            get => isSaleItemListVisible;
            set
            {
                isSaleItemListVisible = value;
                if (value)
                {
                    SaleItemListVisibility = "True";
                    NoSaleItemTextVisibility = "False";
                }
                else
                {
                    SaleItemListVisibility = "False";
                    NoSaleItemTextVisibility = "True";
                }
                OnPropertyChanged(nameof(IsSaleItemListVisible));
            }
        }

        string noSaleItemTextVisibility = "True";
        public string NoSaleItemTextVisibility
        {
            get => noSaleItemTextVisibility;
            set
            {
                noSaleItemTextVisibility = value;
                OnPropertyChanged(nameof(NoSaleItemTextVisibility));
            }
        }

        string saleItemListVisibility = "False";
        public string SaleItemListVisibility
        {
            get => saleItemListVisibility;
            set
            {
                saleItemListVisibility = value;
                OnPropertyChanged(nameof(SaleItemListVisibility));
            }
        }

        bool isAddSaleItemEnabled = false;
        public bool IsAddSaleItemEnabled
        {
            get => isAddSaleItemEnabled;
            set
            {
                isAddSaleItemEnabled = value;
                OnPropertyChanged(nameof(IsAddSaleItemEnabled));
            }
        }

        #region Sale Item Fields 

        int? itemID = null;
        public int? ItemID
        {
            get => itemID;
            set
            {
                itemID = value;
                CheckIfCanAddSaleItem();
                OnPropertyChanged(nameof(ItemID));
            }
        }

        string productCode = null;
        public string ProductCode
        {
            get => productCode;
            set
            {
                productCode = value;
                CheckIfCanAddSaleItem();
                OnPropertyChanged(nameof(ProductCode));
            }
        }

        string productLabel = null;
        public string ProductLabel
        {
            get => productLabel;
            set
            {
                productLabel = value;
                CheckIfCanAddSaleItem();
                OnPropertyChanged(nameof(ProductLabel));
            }
        }

        List<string> unitOfMeasureList;
        public List<string> UnitOfMeasureList
        {
            get => unitOfMeasureList;
            set
            {
                unitOfMeasureList = value;
                OnPropertyChanged(nameof(UnitOfMeasureList));
            }
        }

        int unitOfMeasureSelectedIndex = -1;
        public int UnitOfMeasureSelectedIndex
        {
            get => unitOfMeasureSelectedIndex;
            set
            {                
                unitOfMeasureSelectedIndex = value;
                unitOfMeasure = null;
                if ((value >= 0) && (value < unitOfMeasureList.Count) && Enum.TryParse(unitOfMeasureList[value], out UnitOfMeasure uom))
                {
                    unitOfMeasure = uom;
                }                
                CheckIfCanAddSaleItem();
                OnPropertyChanged(nameof(UnitOfMeasureSelectedIndex));
            }
        }

        decimal? unitPrice = null;
        public decimal? UnitPrice
        {
            get => unitPrice;
            set
            {
                unitPrice = value;
                CheckIfCanAddSaleItem();
                OnPropertyChanged(nameof(UnitPrice));
            }
        }

        decimal? quantity = null;
        public decimal? Quantity
        {
            get => quantity;
            set
            {
                quantity = value;
                CheckIfCanAddSaleItem();
                OnPropertyChanged(nameof(Quantity));
            }
        }

        decimal? itemAmount = null;
        public decimal? ItemAmount
        {
            get => itemAmount;
            set
            {
                itemAmount = value;
                CheckIfCanAddSaleItem();
                OnPropertyChanged(nameof(ItemAmount));
            }
        }

        string tags = null;
        public string Tags
        {
            get => tags;
            set
            {
                tags = value;
                CheckIfCanAddSaleItem();
                OnPropertyChanged(nameof(Tags));
            }
        }

        #endregion Sale Item Fields

        #endregion
    }
}

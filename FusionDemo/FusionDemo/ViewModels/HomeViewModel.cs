using FusionDemo.Models;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using System.Collections.Generic;
using DataMeshGroup.Fusion.Model;
using System.Dynamic;
using FusionDemo.Views;
using DataMeshGroup.Fusion.Model.Transit;

namespace FusionDemo.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        public HomeViewModel()
        {
            Title = "Payments";
            DoPurchaseCommand = new Command(async () => await DoPurchase());
            DoRefundCommand = new Command(async () => await DoRefund());
            NavigateSettingsCommand = new Command(async () => await Shell.Current.GoToAsync($"//{nameof(SettingsPage)}"));
        }

        public ICommand NavigateSettingsCommand { get; }
        public ICommand DoPurchaseCommand { get; }
        public ICommand DoRefundCommand { get; }


        private PaymentRequest CreatePaymentRequest()
        {
            // Construct payment request
            PaymentRequest paymentRequest = new PaymentRequest()
            {
                PaymentData = new PaymentData()
                {
                    PaymentType = paymentType
                },
                SaleData = new SaleData()
                {
                    OperatorID = "4452",
                    ShiftNumber = "2023-04-06_01",
                    SaleTransactionID = new TransactionIdentification()
                    {
                        TransactionID = "0347d00e-5d13-4043-b92b-6bf32381ab16",
                        TimeStamp = DateTime.UtcNow
                    },
                    SaleTerminalData = new SaleTerminalData(false)
                    {
                        DeviceID = "58df5074-0f6d-41be-9b4f-bf3de3197ddd"
                    },
                    SponsoredMerchant = new SponsoredMerchant()
                    {
                        BusinessID = "50110219460",
                        RegisteredIdentifier = "TestClient",
                        SiteID = "719428ed-8c98-4a1a-8b4f-853bbaa0a154"
                    }
                },
                PaymentTransaction = new PaymentTransaction()
                {
                    AmountsReq = new AmountsReq()
                    {
                        Currency = CurrencySymbol.AUD,
                        RequestedAmount = requestedAmount,
                        TipAmount = tipAmount,
                        CashBackAmount = cashBackAmount
                    },
                    SaleItem = new List<SaleItem>()
                    {
                        new SaleItem()
                        {
                            ItemID = 0,
                            ProductCode = "MeteredFare",
                            ProductLabel = "TRF 1 SINGLE",
                            UnitOfMeasure = UnitOfMeasure.Kilometre,
                            UnitPrice = 2M,
                            Quantity = 8M,
                            ItemAmount = requestedAmount / 2
                        },
                        new SaleItem()
                        {
                            ItemID = 1,
                            ProductCode = "NSWGovLevy",
                            ProductLabel = "NSW GOV LEVY",
                            UnitOfMeasure = UnitOfMeasure.Other,
                            UnitPrice = 8M,
                            Quantity = 1M,
                            ItemAmount = requestedAmount / 4,
                            Tags = new List<string>()
                            {
                                "subtotal"
                            }
                        },
                        new SaleItem()
                        {
                            ItemID = 2,
                            ProductCode = "LateNightFee",
                            ProductLabel = "Late Night Fee",
                            UnitOfMeasure = UnitOfMeasure.Other,
                            UnitPrice = 2.1M,
                            Quantity = 1M,
                            ItemAmount = requestedAmount / 4,
                            Tags = new List<string>()
                            {
                                "extra"
                            }
                        }
                    }
                },
                ExtensionData = new ExtensionData()
                {
                    TransitData = new TransitData()
                    {
                        IsWheelchairEnabled = true,
                        Trip = new Trip()
                        {
                            TotalDistanceTravelled = 29.4M,
                            Pickup = new Stop()
                            {
                                StopIndex = 0,
                                StopName = "Richmond",
                                Latitude = "-37.82274517047244",
                                Longitude = "144.98394642094434",
                                Timestamp = DateTime.Parse("2023-04-06T03:00:15+0000")
                            },
                            Destination = new Stop()
                            {
                                StopIndex = 1,
                                StopName = "Beaumaris",
                                Latitude = "-37.988864997462048",
                                Longitude = "145.04484379736329",
                                Timestamp = DateTime.Parse("2023-04-06T03:39:30+0000")
                            }
                        }
                    }
                }
            };           

            if(!String.IsNullOrEmpty(ProductCode))
            {
                paymentRequest.AddSaleItem(productCode: ProductCode, productLabel: ProductCode, itemAmount: 0);
            }            

            return paymentRequest;
        }

        private async Task DoPurchase()
        {
            PaymentType = PaymentType.Normal;
            Settings.Instance.Payment.Request = CreatePaymentRequest();
            await Shell.Current.GoToAsync($"//{nameof(PaymentPage)}");
        }

        private async Task DoRefund()
        {
            PaymentType = PaymentType.Refund;
            Settings.Instance.Payment.Request = CreatePaymentRequest();
            await Shell.Current.GoToAsync($"//{nameof(PaymentPage)}");
        }

        public async override Task OnNavigatedTo()
        {
            Settings s = Settings.Instance;
            if (string.IsNullOrEmpty(s.SaleID) || string.IsNullOrEmpty(s.POIID) || string.IsNullOrEmpty(s.KEK))
            {
                await Shell.Current.GoToAsync($"//{nameof(SettingsPage)}");
            }
        }

        #region Properties

        PaymentType paymentType = PaymentType.Normal;
        public PaymentType PaymentType
        {
            get { return paymentType; }
            set { SetProperty(ref paymentType, value); }
        }

        decimal requestedAmount = 10.42M;
        public decimal RequestedAmount
        {
            get { return requestedAmount; }
            set { SetProperty(ref requestedAmount, value); }
        }


        decimal? tipAmount = null;
        public decimal? TipAmount
        {
            get { return tipAmount; }
            set { SetProperty(ref tipAmount, value); }
        }


        decimal? cashBackAmount = null;
        public decimal? CashBackAmount
        {
            get { return cashBackAmount; }
            set { SetProperty(ref cashBackAmount, value); }
        }

        String productCode = null;
        public String ProductCode
        {
            get { return productCode; }
            set { SetProperty(ref productCode, value); }
        }

        #endregion

    }
}
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
            NavigateOtherFieldsCommand = new Command(async () => await Shell.Current.GoToAsync($"//{nameof(OtherFieldsPage)}"));
        }

        public ICommand NavigateSettingsCommand { get; }
        public ICommand NavigateOtherFieldsCommand { get; }
        public ICommand DoPurchaseCommand { get; }
        public ICommand DoRefundCommand { get; }

        private void UpdatePaymentRequest()
        {
            // Construct payment request
            PaymentRequest paymentRequest = new PaymentRequest()
            {
                PaymentData = new PaymentData()
                {
                    PaymentType = paymentType
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
                }                                
            };           

            if(!String.IsNullOrEmpty(ProductCode))
            {
                paymentRequest.AddSaleItem(productCode: ProductCode, productLabel: ProductCode, itemAmount: 0);
            }

            Settings.Instance.Payment.Request.PaymentData = paymentRequest.PaymentData;
            Settings.Instance.Payment.Request.PaymentTransaction = paymentRequest.PaymentTransaction;
        }

        private async Task DoPurchase()
        {
            PaymentType = PaymentType.Normal;
            UpdatePaymentRequest();
            await Shell.Current.GoToAsync($"//{nameof(PaymentPage)}");
        }

        private async Task DoRefund()
        {
            PaymentType = PaymentType.Refund;
            UpdatePaymentRequest();
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
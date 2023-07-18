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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FusionDemo.Util;

namespace FusionDemo.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        public HomeViewModel()
        {
            Title = "Payments";
            DoPurchaseCommand = new Command(async () => await DoPurchase());
            DoRefundCommand = new Command(async () => await DoRefund());
            DoGetTotalsCommand = new Command(async () => await DoGetTotals());
            NavigateSettingsCommand = new Command(async () => await Shell.Current.GoToAsync($"//{nameof(SettingsPage)}"));
            NavigateOtherFieldsCommand = new Command(async () => await Shell.Current.GoToAsync($"//{nameof(OtherFieldsPage)}"));
            NavigateSaleItemsCommand = new Command(async () => await Shell.Current.GoToAsync($"//{nameof(SaleItemsPage)}"));
        }

        public ICommand NavigateSettingsCommand { get; }
        public ICommand NavigateOtherFieldsCommand { get; }

        public ICommand NavigateSaleItemsCommand { get; }
        public ICommand DoPurchaseCommand { get; }
        public ICommand DoRefundCommand { get; }
        public ICommand DoGetTotalsCommand { get; }

        private void UpdatePaymentRequest()
        {
            // Construct payment request
            // 
            PaymentRequest paymentRequest = Settings.Instance.Payment.Request;
            paymentRequest.PaymentData = new PaymentData()
            {
                PaymentType = paymentType
            };
            paymentRequest.PaymentTransaction.AmountsReq = new AmountsReq()
            {
                Currency = CurrencySymbol.AUD,
                RequestedAmount = requestedAmount,
                TipAmount = tipAmount,
                CashBackAmount = cashBackAmount
            };
            if (paymentRequest.PaymentTransaction.SaleItem != null)
            {
                paymentRequest.PaymentTransaction.SaleItem.Remove(paymentRequest.PaymentTransaction.SaleItem.Find(si => (si.AdditionalProductInfo == "DataMesh Test Case ID"))); //remove any previously added test case product code.
            }
            if (!String.IsNullOrEmpty(ProductCode))
            {
                paymentRequest.AddSaleItem(productCode: ProductCode, productLabel: ProductCode, itemAmount: 0, additionalProductInfo:"DataMesh Test Case ID");
            }
        }

        private void CreateGetTotals()
        {
            // Construct gettotal request
            GetTotalsRequest getTotalsRequest = new GetTotalsRequest()
            {
                TotalDetails = new List<TotalDetail> { TotalDetail.EndOfShift },
                TotalFilter = new TotalFilter()
                {
                    OperatorID = Settings.OperatorID,
                    ShiftNumber = Settings.ShiftNumber
                }
            };      
            
            Settings.Instance.GetTotals.Request = getTotalsRequest;
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

        private async Task DoGetTotals()
        {
            CreateGetTotals();
            await Shell.Current.GoToAsync($"//{nameof(TransactionPage)}");
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

        decimal requestedAmount = 33.1M;
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
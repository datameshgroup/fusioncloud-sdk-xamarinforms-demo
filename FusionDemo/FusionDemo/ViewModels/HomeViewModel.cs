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
            PaymentRequest paymentRequest = new PaymentRequest(
                transactionID: Guid.NewGuid().ToString(),
                requestedAmount: RequestedAmount,
                paymentType: PaymentType);

            paymentRequest.PaymentTransaction.AmountsReq.TipAmount = TipAmount;
            paymentRequest.PaymentTransaction.AmountsReq.CashBackAmount = CashBackAmount;

            // Create sale item
            SaleItem parentItem = paymentRequest.AddSaleItem(
                productCode: "XXVH776",
                productLabel: "Big Kahuna Burger",
                itemAmount: RequestedAmount,
                category: "food",
                subCategory: "mains"
                );
            // Sale item modifiers
            paymentRequest.AddSaleItem(
                    productCode: "XXVH776-0",
                    productLabel: "Extra pineapple",
                    parentItemID: parentItem.ItemID,
                   itemAmount: 0,
                   category: "food",
                   subCategory: "mains"
                   );
            paymentRequest.AddSaleItem(
                productCode: "XXVH776-1",
               productLabel: "Side of fries",
               parentItemID: parentItem.ItemID,
               itemAmount: 0,
               category: "food",
               subCategory: "sides"
               );

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
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
    public class SaleItemsViewModel : BaseViewModel
    {
        private PaymentRequest currentPaymentRequest = null;

        private bool updateAddSaleItemBtn = true;

        private SaleItem currentSelectedItem = null;

        private UnitOfMeasure? unitOfMeasure = null;

        public Command SaveCommand { get; }
        public Command SelectSaleItemCommand { get; }
        public Command DeleteSaleItemCommand { get; }
        public Command AddSaleItemCommand { get; }        

        public SaleItemsViewModel()
        {
            Title = "Sale Items";
            SaveCommand = new Command(OnSaveClicked);
            SelectSaleItemCommand = new Command<object>(OnSelectSaleItem);
            DeleteSaleItemCommand = new Command(OnDeleteSaleItem);
            AddSaleItemCommand = new Command(OnAddSaleItem);

            unitOfMeasureList = new List<string>();
            unitOfMeasureList.AddRange(Enum.GetNames(typeof(UnitOfMeasure)));

            PopulateSaleItems();
        }

        public void PopulateSaleItems()
        {
            currentPaymentRequest = Settings.Payment.Request;
            
            SaleItemsList = new List<SaleItem>();
            if (currentPaymentRequest.PaymentTransaction.SaleItem != null)
            {
                foreach (FusionSaleItem saleItem in currentPaymentRequest.PaymentTransaction.SaleItem)
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
            if ((saleItemsList != null) && (saleItemsList.Count > 0))
            {                
                // Need to recreate the Payment if payment settings have changed
                Settings.CreatePayment(CreatePaymentRequest());

                DisplayAddSaleItemFields = false;

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
            if(currentPaymentRequest.PaymentTransaction == null)
            {
                currentPaymentRequest.PaymentTransaction = new PaymentTransaction();
            }

            currentPaymentRequest.PaymentTransaction.SaleItem = new List<FusionSaleItem>();

            foreach (SaleItem saleItem in saleItemsList)
            {
                currentPaymentRequest.AddSaleItem(itemID: saleItem.ItemID,
                        productCode: saleItem.ProductCode, 
                        productLabel: saleItem.ProductLabel, 
                        unitOfMeasure: saleItem.UnitOfMeasure, 
                        unitPrice:saleItem.UnitPrice, 
                        quantity: saleItem.Quantity, 
                        itemAmount: saleItem.ItemAmount, 
                        tags: saleItem.Tags);
            }
            
            return currentPaymentRequest;
        }

        #region Properties     

        bool displayAddSaleItemFields = false;
        public bool DisplayAddSaleItemFields
        {
            get => displayAddSaleItemFields;
            set
            {
                displayAddSaleItemFields = value;
                if (value)
                {
                    AddSaleItemFieldsVisibility = "True";                    
                }
                else
                {
                    AddSaleItemFieldsVisibility = "False";                    
                }
                OnPropertyChanged(nameof(DisplayAddSaleItemFields));
            }
        }

        string addSaleItemFieldsVisibility = "False";
        public string AddSaleItemFieldsVisibility
        {
            get => addSaleItemFieldsVisibility;
            set
            {
                addSaleItemFieldsVisibility = value;
                OnPropertyChanged(nameof(AddSaleItemFieldsVisibility));
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

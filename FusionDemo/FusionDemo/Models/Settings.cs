using DataMeshGroup.Fusion;
using DataMeshGroup.Fusion.Model;
using DataMeshGroup.Fusion.Model.Transit;
using FusionDemo.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

namespace FusionDemo.Models
{
    public sealed class Settings
    {
        private static readonly Lazy<Settings> lazy = new Lazy<Settings>(() => new Settings());

        public static Settings Instance { get { return lazy.Value; } }

        private static DefaultSettings defaultSettings = new DefaultDevelopmentSettings();

        private Settings()
        {
            //EnablePaymentDialogOnSuccess = true;
            //EnablePaymentDialogOnFailure = true;
            //PaymentDialogOnSuccessTimeoutMSecs = 5000;
            //PaymentDialogOnFailureTimeoutMSecs = 15000;
            //MinimumLogEventLevel = Serilog.Events.LogEventLevel.Debug;
            //ErrorHandlingTimeoutMSecs = 90000;
            //ErrorHandlingRequestTimeMSecs = 10000;
            //TransactionProcessingTimeoutMSecs = 330000;
            //TransactionResponseTimeoutMSecs = 60000;
            //LoginProcessingTimeoutMSecs = 15000;

            CreateFusionClient();
            CreatePayment();
            GetTotals = new GetTotals();
        }

        bool defaultLoadDefaultProductionSetting = false;
        public bool LoadDefaultProductionSetting
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(LoadDefaultProductionSetting), out object value))
                {
                    return (bool)value;
                }
                return defaultLoadDefaultProductionSetting;
            }
            set
            {
                Application.Current.Properties[nameof(LoadDefaultProductionSetting)] = value;
            }
        }

        public bool UseTestEnvironment
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(UseTestEnvironment), out object value))
                {
                    return (bool)value;
                }
                return defaultSettings.UseTestEnvironment;
            }
            set
            {
                Application.Current.Properties[nameof(UseTestEnvironment)] = value;
            }
        }

        public string ProviderIdentification
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(ProviderIdentification), out object value))
                {
                    return (string)value;
                }
                return defaultSettings.ProviderIdentification;
            }
            set
            {
                Application.Current.Properties[nameof(ProviderIdentification)] = value;
            }
        }

        public string ApplicationName
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(ApplicationName), out object value))
                {
                    return (string)value;
                }
                return defaultSettings.ApplicationName;
            }
            set
            {
                Application.Current.Properties[nameof(ApplicationName)] = value;
            }
        }

        public string POSSoftwareVersion
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(POSSoftwareVersion), out object value))
                {
                    return (string)value;
                }
                return defaultSettings.SoftwareVersion;
            }
            set
            {
                Application.Current.Properties[nameof(POSSoftwareVersion)] = value;
            }
        }

        public string CertificationCode
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(CertificationCode), out object value))
                {
                    return (string)value;
                }
                return defaultSettings.CertificationCode;
            }
            set
            {
                Application.Current.Properties[nameof(CertificationCode)] = value;
            }
        }

        public string CustomNexoURL
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(CustomNexoURL), out object value))
                {
                    return (string)value;
                }
                return defaultSettings.CustomNexoURL;
            }
            set
            {
                Application.Current.Properties[nameof(CustomNexoURL)] = value;
            }
        }        

        public string SaleID
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(SaleID), out object value))
                {
                    return (string)value;
                }
                return "";
            }
            set
            {
                Application.Current.Properties[nameof(SaleID)] = value;
            }
        }

        public string POIID
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(POIID), out object value))
                {
                    return (string)value;
                }
                return "";
            }
            set
            {
                Application.Current.Properties[nameof(POIID)] = value;
            }
        }

        public string KEK
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(KEK), out object value))
                {
                    return (string)value;
                }
                return defaultSettings.KEK;
            }
            set
            {
                Application.Current.Properties[nameof(KEK)] = value;
            }
        }

        public bool DisplayOtherFields
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(DisplayOtherFields), out object value))
                {
                    return (bool)value;
                }
                return true;
            }
            set
            {
                Application.Current.Properties[nameof(DisplayOtherFields)] = value;
            }
        }
                    
        public string OperatorID
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(OperatorID), out object value))
                {
                    return (string)value;
                }
                return "4452";
            }
            set
            {
                Application.Current.Properties[nameof(OperatorID)] = value;
            }
        }

        public string ShiftNumber
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(ShiftNumber), out object value))
                {
                    return (string)value;
                }
                return Guid.NewGuid().ToString();
            }
            set
            {
                Application.Current.Properties[nameof(ShiftNumber)] = value;
            }
        }

        private void FusionClient_OnLog(object sender, LogEventArgs e)
        {
            Logger.Instance.Log(e.Exception, e.LogLevel.ToString(), e.Data);
        }

        // Shared FusionClient instance
        public IFusionClient FusionClient { get; internal set; }
        public void CreateFusionClient()
        {
            if (FusionClient != null)
            {
                FusionClient.OnLog -= FusionClient_OnLog;
            }

            FusionClient = new DataMeshGroup.Fusion.FusionClient(useTestEnvironment: UseTestEnvironment)
            {
                SaleID = SaleID,
                POIID = POIID,
                KEK = KEK,                
                CustomURL = CustomNexoURL,
                LogLevel = DataMeshGroup.Fusion.LogLevel.Trace,
                LoginRequest = new LoginRequest()
                {
                    SaleSoftware = new SaleSoftware()
                    {
                        ProviderIdentification = ProviderIdentification,
                        ApplicationName = ApplicationName,
                        SoftwareVersion = POSSoftwareVersion,
                        CertificationCode = CertificationCode
                    },
                    SaleTerminalData = new SaleTerminalData()
                    {
                        SaleCapabilities = new List<SaleCapability>()
                            {
                                SaleCapability.CashierStatus,
                                SaleCapability.CashierError,
                                SaleCapability.CashierInput,
                                SaleCapability.CustomerAssistance
                            }
                    }
                }
            };
            FusionClient.URL = string.IsNullOrWhiteSpace(CustomNexoURL) ? FusionClient.URL : UnifyURL.Custom;
            FusionClient.OnLog += FusionClient_OnLog;
        }

        // Shared Payment instance
        public Payment Payment { get; private set; }

        public void CreatePayment()
        {
            Payment = new Payment();
            Payment.Request = CreatePaymentRequest();
        }

        public void CreatePayment(PaymentRequest request)
        {
            Payment = new Payment();
            Payment.Request = request;
        }

        private PaymentRequest CreatePaymentRequest()
        {
            DateTime defaultDateTime = DateTime.Now;

            DateTime pickUpDate = defaultDateTime.AddMinutes(-45);            

            DateTime destinationDate = defaultDateTime;            

            // Construct payment request
            PaymentRequest paymentRequest = new PaymentRequest()
            {
                SaleData = new SaleData()
                {
                    OperatorID = this.OperatorID,
                    ShiftNumber = this.ShiftNumber,
                    SaleTransactionID = new TransactionIdentification()
                    {
                        TransactionID = "0347d00e-5d13-4043-b92b-6bf32381ab16",
                        TimeStamp = defaultDateTime
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
                                Timestamp = pickUpDate
                            },
                            Destination = new Stop()
                            {
                                StopIndex = 1,
                                StopName = "Beaumaris",
                                Latitude = "-37.988864997462048",
                                Longitude = "145.04484379736329",
                                Timestamp = destinationDate
                            }
                        }
                    }
                },
                PaymentTransaction = new PaymentTransaction()                
            };           
            
            foreach (SaleItem saleItem in defaultSaleItemsList)
            {
                paymentRequest.AddSaleItem(itemID: saleItem.ItemID,
                    productCode: saleItem.ProductCode,
                    productLabel: saleItem.ProductLabel,
                    unitOfMeasure: saleItem.UnitOfMeasure,
                    unitPrice: saleItem.UnitPrice,
                    quantity: saleItem.Quantity,
                    itemAmount: saleItem.ItemAmount,
                    tags: saleItem.Tags);
            }            

            return paymentRequest;
        }

        public GetTotals GetTotals { get; set; }

        private List<SaleItem> defaultSaleItemsList = new List<SaleItem>()
        {
            new SaleItem()
            {
                ItemID = 0,
                ProductCode = "MeteredFare",
                ProductLabel = "TRF 1 SINGLE",
                UnitOfMeasure = UnitOfMeasure.Kilometre,
                UnitPrice = 2.5M,
                Quantity = 8,
                ItemAmount = 20
            },
            new SaleItem()
            {
                ItemID = 1,
                ProductCode = "SAGovLevy",
                ProductLabel = "SA GOV LEVY",
                UnitOfMeasure = UnitOfMeasure.Other,
                UnitPrice = 8,
                Quantity = 1,
                ItemAmount = 8,
                Tags = new List<string>()
                {
                    "subtotal"
                }
            },
            /* - for NSW
            new SaleItem()
            {
                ItemID = 1,
                ProductCode = "NSWGovLevy",
                ProductLabel = "NSW GOV LEVY",
                UnitOfMeasure = UnitOfMeasure.Other,
                UnitPrice = 8,
                Quantity = 1,
                ItemAmount = 8,
                Tags = new List<string>()
                {
                    "extra"
                }
            },*/
            new SaleItem()
            {
                ItemID = 2,
                ProductCode = "LateNightFee",
                ProductLabel = "Late Night Fee",
                UnitOfMeasure = UnitOfMeasure.Other,
                UnitPrice = 2.10M,
                Quantity = 1,
                ItemAmount = 2.10M,
                Tags = new List<string>()
                {
                    "extra"
                }
            },
            new SaleItem()
            {
                ItemID = 51003,
                ProductCode = "M5 South-West Mwy West, Parramatta Road, Windsor Road, Sunnyholt Road, Great Western High Way, Wester Distributor, Cahill Expressway, Easter Distributor, Airport Road, Anzac Parade",
                ProductLabel = "M5 South-West Mwy West, Parramatta Road, Windsor Road, Sunnyholt Road, Great Western High Way, Wester Distributor, Cahill Expressway, Easter Distributor, Airport Road, Anzac Parade ",
                UnitOfMeasure = UnitOfMeasure.Other,
                UnitPrice = 3.0M,
                Quantity = 1,
                ItemAmount = 3.0M,
                Tags = new List<string>()
                {
                    "extra"
                }
            }
        };

        public List<SaleItem> DefaultSaleItemsList
        {
            get => defaultSaleItemsList;            
        }
    }
}
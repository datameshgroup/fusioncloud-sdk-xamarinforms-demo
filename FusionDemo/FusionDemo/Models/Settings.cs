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
        }

        public bool UseTestEnvironment
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(UseTestEnvironment), out object value))
                {
                    return (bool)value;
                }
                return true;
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
                return "Company A";
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
                return "POS Retail";
            }
            set
            {
                Application.Current.Properties[nameof(ApplicationName)] = value;
            }
        }

        public string SoftwareVersion => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() ?? "1.0.0";

        public string CertificationCode
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(CertificationCode), out object value))
                {
                    return (string)value;
                }
                return "98cf9dfc-0db7-4a92-8b8cb66d4d2d7169";
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
                return "wss://cloudposintegration.io/nexodev";
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
                return "44DACB2A22A4A752ADC1BBFFE6CEFB589451E0FFD83F8B21";
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
                return false;
            }
            set
            {
                Application.Current.Properties[nameof(DisplayOtherFields)] = value;
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
                LogLevel = DataMeshGroup.Fusion.LogLevel.Trace,
                LoginRequest = new LoginRequest()
                {
                    SaleSoftware = new SaleSoftware()
                    {
                        ProviderIdentification = ProviderIdentification,
                        ApplicationName = ApplicationName,
                        SoftwareVersion = SoftwareVersion,
                        CertificationCode = CertificationCode
                    },
                    SaleTerminalData = new SaleTerminalData()
                    {
                        SaleCapabilities = new List<SaleCapability>()
                            {
                                SaleCapability.CashierStatus,
                                SaleCapability.CashierError,
                                SaleCapability.CashierInput,
                                SaleCapability.CustomerAssistance,
                                SaleCapability.PrinterReceipt
                            }
                    }
                }
            };            

            FusionClient.OnLog += FusionClient_OnLog;
        }

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

        // Shared Payment instance
        public Payment Payment { get; private set; }

        private PaymentRequest CreatePaymentRequest()
        {
            // Construct payment request
            PaymentRequest paymentRequest = new PaymentRequest()
            {
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

            return paymentRequest;
        }
    }
}
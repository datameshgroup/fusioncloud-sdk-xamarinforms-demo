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

        public bool OtherSettings
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(OtherSettings), out object value))
                {
                    return (bool)value;
                }
                return true;
            }
            set
            {
                Application.Current.Properties[nameof(OtherSettings)] = value;
            }
        }

        #region Payment Fields

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
                return "2023-04-06_01";
            }
            set
            {
                Application.Current.Properties[nameof(ShiftNumber)] = value;
            }
        }

        public string TransactionID
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(TransactionID), out object value))
                {
                    return (string)value;
                }
                return "0347d00e-5d13-4043-b92b-6bf32381ab16";
            }
            set
            {
                Application.Current.Properties[nameof(TransactionID)] = value;
            }
        }

        public string SaleTransactionTimeStamp
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(SaleTransactionTimeStamp), out object value))
                {
                    return (string)value;
                }
                return DateTime.Now.ToString();
            }
            set
            {
                Application.Current.Properties[nameof(SaleTransactionTimeStamp)] = value;
            }
        }

        public String DeviceID
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(DeviceID), out object value))
                {
                    return (string)value;
                }
                return "58df5074-0f6d-41be-9b4f-bf3de3197ddd";
            }
            set
            {
                Application.Current.Properties[nameof(DeviceID)] = value;
            }
        }

        public String BusinessID
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(BusinessID), out object value))
                {
                    return (string)value;
                }
                return "50110219460";
            }
            set
            {
                Application.Current.Properties[nameof(BusinessID)] = value;
            }
        }

        public String RegisteredIdentifier
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(RegisteredIdentifier), out object value))
                {
                    return (string)value;
                }
                return "TestClient";
            }
            set
            {
                Application.Current.Properties[nameof(RegisteredIdentifier)] = value;
            }
        }

        public String SiteID
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(SiteID), out object value))
                {
                    return (string)value;
                }
                return "719428ed-8c98-4a1a-8b4f-853bbaa0a154";
            }
            set
            {
                Application.Current.Properties[nameof(SiteID)] = value;
            }
        }

        public bool IsWheelchairEnabled
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(IsWheelchairEnabled), out object value))
                {
                    return (bool)value;
                }
                return true;
            }
            set
            {
                Application.Current.Properties[nameof(IsWheelchairEnabled)] = value;
            }
        }
        public decimal TotalDistanceTravelled
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(TotalDistanceTravelled), out object value))
                {
                    return (decimal)value;
                }
                return 29.4M;
            }
            set
            {
                Application.Current.Properties[nameof(TotalDistanceTravelled)] = value;
            }
        }

        public String PickUpStopName
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(PickUpStopName), out object value))
                {
                    return (string)value;
                }
                return "Richmond";
            }
            set
            {
                Application.Current.Properties[nameof(PickUpStopName)] = value;
            }
        }

        public String PickUpLatitude
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(PickUpLatitude), out object value))
                {
                    return (string)value;
                }
                return "-37.82274517047244";
            }
            set
            {
                Application.Current.Properties[nameof(PickUpLatitude)] = value;
            }
        }

        public String PickUpLongitude
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(PickUpLongitude), out object value))
                {
                    return (string)value;
                }
                return "144.98394642094434";
            }
            set
            {
                Application.Current.Properties[nameof(PickUpLongitude)] = value;
            }
        }

        public String PickUpTimeStamp
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(PickUpTimeStamp), out object value))
                {
                    return (string)value;
                }
                return "2023-04-06T03:00:15+0000";
            }
            set
            {
                Application.Current.Properties[nameof(PickUpTimeStamp)] = value;
            }
        }

        public String DestinationStopName
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(DestinationStopName), out object value))
                {
                    return (string)value;
                }
                return "Beaumaris";
            }
            set
            {
                Application.Current.Properties[nameof(DestinationStopName)] = value;
            }
        }

        public String DestinationLatitude
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(DestinationLatitude), out object value))
                {
                    return (string)value;
                }
                return "-37.988864997462048";
            }
            set
            {
                Application.Current.Properties[nameof(DestinationLatitude)] = value;
            }
        }

        public String DestinationLongitude
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(DestinationLongitude), out object value))
                {
                    return (string)value;
                }
                return "145.04484379736329";
            }
            set
            {
                Application.Current.Properties[nameof(DestinationLongitude)] = value;
            }
        }

        public String DestinationTimeStamp
        {
            get
            {
                if (Application.Current.Properties.TryGetValue(nameof(DestinationTimeStamp), out object value))
                {
                    return (string)value;
                }
                return "2023-04-06T03:39:30+0000";
            }
            set
            {
                Application.Current.Properties[nameof(DestinationTimeStamp)] = value;
            }
        }


        #endregion


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

        // Shared Payment instance
        public Payment Payment { get; private set; }

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
    }
}
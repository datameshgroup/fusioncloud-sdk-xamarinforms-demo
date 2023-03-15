using DataMeshGroup.Fusion;
using DataMeshGroup.Fusion.Model;
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
            Payment = new Payment();
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

        // Shared Payment instance
        public Payment Payment { get; set; }
    }

}
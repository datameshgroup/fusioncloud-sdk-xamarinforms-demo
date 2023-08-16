using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace FusionDemo.Models
{
    public class DefaultSettings
    {
        public string KEK { protected set; get; }
        public string ProviderIdentification { protected set; get; }
        public string ApplicationName { protected set; get; }
        public string CertificationCode { protected set; get; }
        public string SoftwareVersion { protected set; get; }
        public bool UseTestEnvironment { protected set; get; }
        public string CustomNexoURL { protected set; get; }
    }

    public class DefaultProductionSettings : DefaultSettings
    {
        public DefaultProductionSettings() {
            KEK = "";
            ProviderIdentification = "";
            ApplicationName = "";
            CertificationCode = "";
            SoftwareVersion = "";
            UseTestEnvironment = false;
            CustomNexoURL = "";
        }        
    }

    public class DefaultDevelopmentSettings : DefaultSettings
    {
        public DefaultDevelopmentSettings()
        {
            KEK = "44DACB2A22A4A752ADC1BBFFE6CEFB589451E0FFD83F8B21";
            ProviderIdentification = "Company A";
            ApplicationName = "POS Retail";
            CertificationCode = "98cf9dfc-0db7-4a92-8b8cb66d4d2d7169";
            SoftwareVersion = AppInfo.Version.ToString();
            UseTestEnvironment = true;
            CustomNexoURL = "wss://cloudposintegration.io/nexodev";
        }        
    }
}

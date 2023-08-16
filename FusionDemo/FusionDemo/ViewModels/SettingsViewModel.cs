using FusionDemo.Models;
using FusionDemo.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace FusionDemo.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public Command SaveCommand { get; }

        public Command LoadDefaultProductionSettingCommand { get; }

        public SettingsViewModel()
        {
            Title = "Settings";
            SaveCommand = new Command(OnSaveClicked);
            LoadDefaultProductionSettingCommand = new Command(OnLoadDefaultProductionSettingToggled);
        }

        private async void OnSaveClicked(object obj)
        {
            // Need to recreate the FusionClient if settings have changed
            Settings.CreateFusionClient();

            // Navigate 
            await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
        }

        private void OnLoadDefaultProductionSettingToggled(object obj)
        {
            ToggledEventArgs arg = (obj as ToggledEventArgs);
            if (arg != null)
            {                
                OverrideSettings(arg.Value);
            }            
        }

        private void OverrideSettings(bool isProduction)
        {
            LoadDefaultProductionSetting = isProduction;
            SaleID = "";
            POIID = "";
            DefaultSettings defaultSettings;
            if(isProduction){
                defaultSettings = new DefaultProductionSettings();                
            }
            else {
                defaultSettings = new DefaultDevelopmentSettings();
            }
            KEK = defaultSettings.KEK;
            ProviderIdentification = defaultSettings.ProviderIdentification;
            ApplicationName = defaultSettings.ApplicationName;
            POSSoftwareVersion = defaultSettings.SoftwareVersion;
            CertificationCode = defaultSettings.CertificationCode;
            CustomNexoURL = defaultSettings.CustomNexoURL;
            UseTestEnvironment = defaultSettings.UseTestEnvironment;
        }

        public override Task OnNavigatedTo()
        {
            return Task.CompletedTask;
        }

        #region Properties

        public bool LoadDefaultProductionSetting
        {
            get => Settings.LoadDefaultProductionSetting;
            set
            {
                Settings.LoadDefaultProductionSetting = value;
                OnPropertyChanged(nameof(LoadDefaultProductionSetting));
            }
        }

        public bool UseTestEnvironment
        {
            get => Settings.UseTestEnvironment;
            set
            {
                Settings.UseTestEnvironment = value;
                OnPropertyChanged(nameof(UseTestEnvironment));
            }
        }

        public string ProviderIdentification
        {
            get => Settings.ProviderIdentification;
            set
            {
                Settings.ProviderIdentification = value;
                OnPropertyChanged(nameof(ProviderIdentification));
            }
        }

        public string ApplicationName
        {
            get => Settings.ApplicationName;
            set
            {
                Settings.ApplicationName = value;
                OnPropertyChanged(nameof(ApplicationName));
            }
        }

        public string POSSoftwareVersion
        {
            get => Settings.POSSoftwareVersion;
            set
            {
                Settings.POSSoftwareVersion = value;
                OnPropertyChanged(nameof(POSSoftwareVersion));
            }
        }

        public string CertificationCode
        {
            get => Settings.CertificationCode;
            set
            {
                Settings.CertificationCode = value;
                OnPropertyChanged(nameof(CertificationCode));
            }
        }

        public string CustomNexoURL
        {
            get => Settings.CustomNexoURL;
            set
            {
                Settings.CustomNexoURL = value;
                OnPropertyChanged(nameof(CustomNexoURL));
            }
        }


        public string SaleID
        {
            get => Settings.SaleID;
            set
            {
                Settings.SaleID = value;
                OnPropertyChanged(nameof(SaleID));
            }
        }


        public string POIID
        {
            get => Settings.POIID;
            set
            {
                Settings.POIID = value;
                OnPropertyChanged(nameof(POIID));
            }
        }

        public string KEK
        {
            get => Settings.KEK;
            set
            {
                Settings.KEK = value;
                OnPropertyChanged(nameof(KEK));
            }
        }

        public string AppVersionNumber
        {
            get => AppInfo.Version.ToString();
        }

        #endregion
    }
}

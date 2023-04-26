using DataMeshGroup.Fusion;
using DataMeshGroup.Fusion.Model;
using FusionDemo.Models;
using FusionDemo.Util;
using FusionDemo.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FusionDemo.ViewModels
{
    public class TransactionViewModel : BaseViewModel
    {
        /// <summary>
        /// Current messagePayload for UI
        /// </summary>
        private MessagePayload currentMessagePayload;


        /// <summary>
        /// Track the saleToPOIRequest in progress. Used for cancel and error handling.
        /// TODO - can we merge this with currentMessagePayload...
        /// </summary>
        private SaleToPOIMessage currentSaleToPOIRequest;

        /// <summary>
        /// Text representation of the transaction type in progress
        /// </summary>
        private string transactionType;

        private Logger log;

        public Command OkCommand { get; }        

        public TransactionViewModel()
        {
            log = Logger.Instance;

            currentMessagePayload = null;
            currentSaleToPOIRequest = null; // TODO: Need to handle transaction in progress after crash           

            OkCommand = new Command(OnOkTapped);            
        }

        private async void OnOkTapped(object obj)
        {
            await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
        }        

        public async override Task OnNavigatedTo()
        {
            GetTotalsRequest getTotalsRequest = this.Settings.GetTotals.Request;
                   
            // Reset UI
            DialogCaption = "";
            DialogDisplayLine = "";
            DialogDisplayText = "";
            DialogTitle = "";

            DialogTitleTextColor = Color.Black;
            DialogTitleBackgroundColor = Color.Transparent;

            DialogEnableOkButton = false;
            DialogEnableCancelButton = false;

            // Send get totals request 
            await SendGetTotalsRequest(fusionClient: this.Settings.FusionClient, getTotalsRequest);            
        }

        /// <summary>
        /// Perform a get totals request 
        /// </summary>
        /// <param name="fusionClient"></param>
        /// <param name="getTotalsRequest"></param>
        /// <returns></returns>
        private async Task<GetTotalsResponse> SendGetTotalsRequest(IFusionClient fusionClient, GetTotalsRequest getTotalsRequest)
        {
            int transactionProcessingTimeoutMSecs = 330000;
            int transactionResponseTimeoutMSecs = 60000;

            //this is unique to every payment request
            string serviceId = Guid.NewGuid().ToString();

            // Overall processing timeout. Defaults to 5m30s.
            using CancellationTokenSource overallTimeoutCTS = new CancellationTokenSource(TimeSpan.FromMilliseconds(transactionProcessingTimeoutMSecs));
            // Process getTotals 
            GetTotalsResponse getTotalsResponse = (GetTotalsResponse)getTotalsRequest.CreateDefaultResponseMessagePayload(new Response(ErrorCondition.Cancel, "User cancel"));
            try
            {
                TriggerMessagePayloadEvent(serviceId, getTotalsRequest);

                // TODO: Save currentSaleToPOIRequest application state for use in error recovery
                currentSaleToPOIRequest = await fusionClient.SendAsync(getTotalsRequest, serviceId, true, overallTimeoutCTS.Token);

                // Wait for next response...
                bool waitingForResponse = true;
                do
                {
                    // Per-message timeout token. Defaults to 60s and is reset every time a response is recevied 
                    using CancellationTokenSource perMessageTimeoutCTS = new CancellationTokenSource(TimeSpan.FromMilliseconds(transactionResponseTimeoutMSecs));

                    // Merged cancellation token
                    using CancellationTokenSource timeoutCTS = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken[] { overallTimeoutCTS.Token, perMessageTimeoutCTS.Token });

                    // Request to RecvAsync() will either result in a response from the host, or an exception (timeout, network error etc)
                    bool resetPerMessageTimeout = false;
                    while (!resetPerMessageTimeout)
                    {
                        resetPerMessageTimeout = true;
                        MessagePayload messagePayload = await fusionClient.RecvAsync(timeoutCTS.Token);
                        switch (messagePayload)
                        {
                            case GetTotalsResponse r:
                                getTotalsResponse = r;
                                waitingForResponse = false;
                                break;
                            case null:
                                // unhandled message response for some reason...
                                break;
                            default:
                                TriggerMessagePayloadEvent(serviceId, messagePayload);
                                break;
                        }
                    }
                }
                while (waitingForResponse);
            }
            catch (FusionException fe)
            {
                getTotalsResponse = (GetTotalsResponse)getTotalsRequest.CreateDefaultResponseMessagePayload(new Response(ErrorCondition.UnreachableHost, fe.Message));
            }

            TriggerMessagePayloadEvent(serviceId, getTotalsResponse);
            return getTotalsResponse;
        }        

        /// <summary>
        /// Function which is triggered on send/recv of Fusion message payload (both real and simulated). 
        /// Only really used to remove custom UI logic from SendGetTotalsRequest
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="messagePayload"></param>
        /// <returns></returns>
        private void TriggerMessagePayloadEvent(string sessionId, MessagePayload messagePayload)
        {
            int transactionDialogOnSuccessTimeoutMSecs = 5000;
            int transactionDialogOnFailureTimeoutMSecs = 15000;

            switch (messagePayload)
            {                
                // At this point the only time we will see a login request is when the POS manually sends a request. We should be careful here however, 
                // as a future version may include auto logins here
                case LoginRequest r:
                    transactionType = "LOGIN";
                    currentMessagePayload = messagePayload;
                    ClearAutoHideTimer();
                    ShowTransactionDialog(transactionType, "LOGIN IN PROGRESS", null, null, DialogType.Normal, false, false);
                    break;

                // At this point the only time we will see a login response is when the POS manually sends a request. We should be careful here however, 
                // as a future version may include auto logins here
                case LoginResponse r:
                    log.Verbose($"Login response. Success={r?.Response?.Success}");

                    // currentSaleToPOIRequest?.MessageHeader.MessageCategory is set to "Login" above in "case LoginRequest".
                    // "LoginRequest" is only received when the merchant initiates a *manual* login, which allows us to use it
                    // here to validate if this login was due to an autologin by the lib, or a manual login 
                    //
                    // If the library starts returning LoginRequest for auto-login, then we will need to handle this differently here.
                    if (currentMessagePayload.MessageCategory != MessageCategory.Login)
                    {
                        log.Verbose($"Ignore login response and manual login was not in progress");
                        break;
                    }

                    currentMessagePayload = null;
                    TransactionDialogResponse transactionDialogResponse = new TransactionDialogResponse(r.Response, TransactionDialogResponse.TransactionType.Login);

                    log.Verbose($"Display login result dialog. ResultTitle={transactionDialogResponse.TransactionResultTitle}, ErrorTitle={transactionDialogResponse.ErrorTitle}, ErrorText={transactionDialogResponse.ErrorText}");
                    ShowTransactionDialog(
                            TransactionDialogResponse.TransactionTypeToDisplayText(TransactionDialogResponse.TransactionType.Login),
                            transactionDialogResponse.TransactionResultTitle,
                            transactionDialogResponse.ErrorTitle,
                            transactionDialogResponse.ErrorText,
                            transactionDialogResponse.Success ? DialogType.Success : DialogType.Error,
                            true,
                            false); ;

                    SetAutoHideTimer(transactionDialogResponse.Success ? transactionDialogOnSuccessTimeoutMSecs : transactionDialogOnFailureTimeoutMSecs);

                    break;


                case GetTotalsRequest r:
                    transactionType = TransactionDialogResponse.TransactionTypeToDisplayText(TransactionDialogResponse.TransactionType.GetTotals);
                    currentMessagePayload = messagePayload;
                    ClearAutoHideTimer();
                    ShowTransactionDialog(transactionType, "TRANSACTION IN PROGRESS", null, null, DialogType.Normal, false, true);
                    break;

                case GetTotalsResponse r:
                    log.Verbose($"Transaction response. Success={r?.Response?.Success}");
                    currentMessagePayload = null;

                    transactionDialogResponse = new TransactionDialogResponse(r);
                    log.Verbose($"Display transaction result dialog. TransactionType={transactionDialogResponse.CurrentTransactionType}, TransactionResultTitle={transactionDialogResponse.TransactionResultTitle}, ErrorTitle={transactionDialogResponse.ErrorTitle}, ErrorText={transactionDialogResponse.ErrorText}");

                    ShowTransactionDialog(
                            TransactionDialogResponse.TransactionTypeToDisplayText(TransactionDialogResponse.TransactionType.GetTotals),
                            transactionDialogResponse.TransactionResultTitle,
                            transactionDialogResponse.ErrorTitle,
                            transactionDialogResponse.ErrorText,
                            "",
                            transactionDialogResponse.Success ? DialogType.Success : DialogType.Error,
                            true,
                            false);

                    SetAutoHideTimer(transactionDialogResponse.Success ? transactionDialogOnSuccessTimeoutMSecs : transactionDialogOnFailureTimeoutMSecs);
                    
                    break;
            }
        }        

        #region TransactionDialogUI

        public void SetAutoHideTimer(int milliseconds)
        {
            // TODO
            // autoHideTimer.Enabled = false;
            // autoHideTimer.Interval = milliseconds;
            // autoHideTimer.Enabled = true;
        }

        public void ClearAutoHideTimer()
        {
            // TODO
            // autoHideTimer.Enabled = false;
        }

        private static void SetLabel(Label label, string content)
        {
            label.Text = content;
            label.IsVisible = !string.IsNullOrEmpty(content);
        }


        #endregion

        public enum DialogType { Normal, Success, Error };

        private void ShowTransactionDialog(string caption, string title, string displayLine1, string displayText, DialogType dialogType, bool enableOk, bool enableCancel)
        {
            ShowTransactionDialog(caption, title, displayLine1, displayText, string.Empty, dialogType, enableOk, enableCancel);
        }

        private void ShowTransactionDialog(string caption, string title, string displayLine1, string displayText, string displayAddtionalText, DialogType dialogType, bool enableOk, bool enableCancel)
        {
            // Set caption
            DialogCaption = caption;

            // Set title
            Color foreground, background;
            DialogTitle = title;
            switch (dialogType)
            {
                case DialogType.Error:
                    background = Color.FromRgb(0xFF, 0x4E, 0x4E); // red
                    foreground = Color.White;

                    AnimationRepeatCount = 1;
                    AnimationName = "fail.txt";

                    break;
                case DialogType.Success:
                    background = Color.FromRgb(0x01, 0xFF, 0x96); // green
                    foreground = Color.White;

                    AnimationRepeatCount = 1;
                    AnimationName = "success.txt";

                    break;
                case DialogType.Normal:
                default:
                    background = Color.Transparent;
                    foreground = Color.Black;

                    AnimationRepeatCount = 999;
                    AnimationName = "potato.txt";
                    break;
            }

            DialogTitleTextColor = foreground;
            DialogTitleBackgroundColor = background;

            DialogDisplayLine = displayLine1;
            DialogDisplayText = displayText;
            DialogDisplayAdditionalText = displayAddtionalText;

            DialogEnableOkButton = enableOk;
            DialogEnableCancelButton = enableCancel;
        }

        #region Properties
        private string dialogCaption;
        public string DialogCaption
        {
            get => dialogCaption;
            set
            {
                SetProperty(ref dialogCaption, value);
            }
        }


        private string dialogTitle;
        public string DialogTitle
        {
            get => dialogTitle;
            set
            {
                SetProperty(ref dialogTitle, value);
            }
        }

        private Color dialogTitleBackgroundColor;
        public Color DialogTitleBackgroundColor
        {
            get => dialogTitleBackgroundColor;
            set
            {
                SetProperty(ref dialogTitleBackgroundColor, value);
            }
        }

        private Color dialogTitleTextColor;
        public Color DialogTitleTextColor
        {
            get => dialogTitleTextColor;
            set
            {
                SetProperty(ref dialogTitleTextColor, value);
            }
        }


        private string dialogDisplayLine;
        public string DialogDisplayLine
        {
            get => dialogDisplayLine;
            set
            {
                SetProperty(ref dialogDisplayLine, value);
            }
        }

        private string dialogDisplayText;
        public string DialogDisplayText
        {
            get => dialogDisplayText;
            set
            {
                SetProperty(ref dialogDisplayText, value);
            }
        }

        private string dialogDisplayAdditionalText;
        public string DialogDisplayAdditionalText
        {
            get => dialogDisplayAdditionalText;
            set
            {
                SetProperty(ref dialogDisplayAdditionalText, value);
            }
        }

        private bool dialogEnableOkButton;
        public bool DialogEnableOkButton
        {
            get => dialogEnableOkButton;
            set
            {
                SetProperty(ref dialogEnableOkButton, value);
            }
        }

        private bool dialogEnableCancelButton;
        public bool DialogEnableCancelButton
        {
            get => dialogEnableCancelButton;
            set
            {
                SetProperty(ref dialogEnableCancelButton, value);
            }
        }


        private string animationName;
        public string AnimationName
        {
            get => string.IsNullOrEmpty(animationName) ? "potato.txt" : animationName;
            set
            {
                SetProperty(ref animationName, value);
            }
        }

        private int animationRepeatCount;
        public int AnimationRepeatCount
        {
            get => animationRepeatCount == 0 ? 99 : animationRepeatCount;
            set
            {
                SetProperty(ref animationRepeatCount, value);
            }
        }
        #endregion
    }
}
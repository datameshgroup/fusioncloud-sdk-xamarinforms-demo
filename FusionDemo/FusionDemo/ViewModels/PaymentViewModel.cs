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
    public class PaymentViewModel : BaseViewModel
    {
        /// <summary>
        /// Current messagePayload for UI
        /// </summary>
        private MessagePayload currentMessagePayload;

        /// <summary>
        /// Determine if payment amount was paid partially
        /// </summary>
        private bool isPartiallyPaid;

        /// <summary>
        /// Current unpaid payment balance after payment amount was partially paid (isPartiallyPaid = true)
        /// </summary>
        private decimal? currentUnpaidBalance;

        /// <summary>
        /// Track the saleToPOIRequest in progress. Used for cancel and error handling.
        /// TODO - can we merge this with currentMessagePayload...
        /// </summary>
        private SaleToPOIMessage currentSaleToPOIRequest;

        /// <summary>
        /// Text representation of the payment type in progress
        /// </summary>
        private string paymentType;

        private Logger log;

        public Command OkCommand { get; }
        public Command CancelCommand { get; }

        public PaymentViewModel()
        {
            log = Logger.Instance;

            currentMessagePayload = null;
            currentSaleToPOIRequest = null; // TODO: Need to handle payment in progress after crash

            isPartiallyPaid = false;
            currentUnpaidBalance = null;            

            OkCommand = new Command(OnOkTapped);
            CancelCommand = new Command(OnCancelTapped);
        }

        private async void OnOkTapped(object obj)
        {
            await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
        }

        private async void OnCancelTapped(object obj)
        {
            // Cancel button was pressed... we need to send an abort request
            log.Information("Abort request triggered via user cancel key press");

            // This shouldn't happen. If the payment has completed by this dialog is still visible 
            // something has gone very wrong! Just attempt to close the dialog and allow the POS 
            // to take back control
            if (currentSaleToPOIRequest?.MessageHeader == null)
            {
                log.Error("Receive OnKeyPress without currentSaleToPOIRequest to abort. Closing dialog...");
                await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
                return;
            }

            AbortRequest abortRequest = new AbortRequest()
            {
                MessageReference = new MessageReference()
                {
                    SaleID = currentSaleToPOIRequest.MessageHeader.SaleID,
                    POIID = currentSaleToPOIRequest.MessageHeader.POIID,
                    ServiceID = currentSaleToPOIRequest.MessageHeader.ServiceID,
                    MessageCategory = currentSaleToPOIRequest.MessageHeader.MessageCategory
                },
                AbortReason = "User cancelled"
            };

            await Settings.Instance.FusionClient.SendAsync(abortRequest);
        }


        public async override Task OnNavigatedTo()
        {
            currentUnpaidBalance = null;
            isPartiallyPaid = false;            

            PaymentRequest paymentRequest = this.Settings.Payment.Request;

            bool processPayment = false;
            if (paymentRequest?.PaymentTransaction?.AmountsReq?.RequestedAmount != null)
            {
                currentUnpaidBalance = (decimal)paymentRequest?.PaymentTransaction?.AmountsReq?.RequestedAmount;
                processPayment = currentUnpaidBalance > 0;
            }

            decimal actualAmountPaid = 0;
            decimal? nextPaymentAmount;
            decimal? partialAuthorizedAmount;
            while (processPayment)
            {
                processPayment = false;
                // Send payment request
                PaymentResponse paymentResponse = await ProcessPayment(paymentRequest);

                //Check response if purchase amount has been paid in full.  If not, verify the amounts
                nextPaymentAmount = CalcuteUnpaidAmount(paymentResponse, out partialAuthorizedAmount);

                //check if payment was fully or partially paid
                isPartiallyPaid = (nextPaymentAmount != null) && (nextPaymentAmount > 0);
                
                //if payment was partially paid, initiate another payment for the remaining amount
                if (isPartiallyPaid)
                {                                             
                    //need to send another payment amount to handle the balance
                    processPayment = true;

                    //total amount already paid
                    actualAmountPaid += (decimal) partialAuthorizedAmount;
                    paymentRequest.PaymentTransaction.AmountsReq.PaidAmount = actualAmountPaid;

                    //update the RequestedAmount with the remaining unpaid amount
                    paymentRequest.PaymentTransaction.AmountsReq.RequestedAmount = currentUnpaidBalance = (decimal)nextPaymentAmount;                    
                }                
            }
        }

        /// <summary>
        /// Processeses the actual payment
        /// </summary>
        /// <param name="paymentRequest"></param>
        /// <returns></returns>
        private async Task<PaymentResponse> ProcessPayment(PaymentRequest paymentRequest)
        {            
            // Reset UI
            DialogCaption = "";
            DialogDisplayLine = "";
            DialogDisplayText = "";
            DialogTitle = "";

            DialogTitleTextColor = Color.Black;
            DialogTitleBackgroundColor = Color.Transparent;

            DialogEnableOkButton = false;
            DialogEnableCancelButton = false;

            // Send payment request 
            return await SendPaymentRequest(fusionClient: this.Settings.FusionClient, paymentRequest);            
        }

        /// <summary>
        /// Perform a payment request 
        /// </summary>
        /// <param name="paymentRequest"></param>
        /// <returns></returns>
        private async Task<PaymentResponse> SendPaymentRequest(IFusionClient fusionClient, PaymentRequest paymentRequest)
        {
            int transactionProcessingTimeoutMSecs = 330000;
            int transactionResponseTimeoutMSecs = 60000;

            //this is unique to every payment request
            string serviceId = Guid.NewGuid().ToString();


            // Overall processing timeout. Defaults to 5m30s.
            using CancellationTokenSource overallTimeoutCTS = new CancellationTokenSource(TimeSpan.FromMilliseconds(transactionProcessingTimeoutMSecs));
            // Process payment 
            PaymentResponse paymentResponse = (PaymentResponse)paymentRequest.CreateDefaultResponseMessagePayload(new Response(ErrorCondition.Cancel, "User cancel"));
            try
            {
                TriggerMessagePayloadEvent(serviceId, paymentRequest);

                // TODO: Save currentSaleToPOIRequest application state for use in error recovery
                currentSaleToPOIRequest = await fusionClient.SendAsync(paymentRequest, serviceId, true, overallTimeoutCTS.Token);

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
                            case PaymentResponse r:
                                paymentResponse = r;
                                waitingForResponse = false;
                                break;
                            case EventNotification r when r.EventToNotify == EventToNotify.Reject && (r.EventDetails?.Contains("Transaction already completed") ?? false):
                                resetPerMessageTimeout = false;
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
                if (fe.ErrorRecoveryRequired)
                {
                    paymentResponse = await PerformErrorRecovery(fusionClient, serviceId, paymentResponse, fe.ErrorReason, overallTimeoutCTS.Token);
                }
                else
                {
                    paymentResponse = (PaymentResponse)paymentRequest.CreateDefaultResponseMessagePayload(new Response(ErrorCondition.UnreachableHost, fe.Message));
                }
            }
            catch (Exception ex)
            {
                paymentResponse = (PaymentResponse)paymentRequest.CreateDefaultResponseMessagePayload(new Response(ErrorCondition.UnreachableHost, ex.Message));
            }

            TriggerMessagePayloadEvent(serviceId, paymentResponse);
            return paymentResponse;
        }

        private async Task<PaymentResponse> PerformErrorRecovery(IFusionClient fusionClient, string sessionId, PaymentResponse paymentResponse, string abortReason, CancellationToken cancellationToken)
        {
            int errorHandlingRequestTimeMSecs = 10000;
            int errorHandlingTimeoutMSecs = 90000;


            log.Information($"Error recovery - starting error recovery. AbortReason={abortReason}, ServiceID={sessionId}");

            // Notify UI that we are recovering the payment 
            DisplayRequest displayRequest = new DisplayRequest();
            displayRequest.SetCashierDisplayAsPlainText("");
            TriggerMessagePayloadEvent(sessionId, displayRequest);

            // Start timeout timers
            TimeSpan requestDelay = TimeSpan.FromMilliseconds(errorHandlingRequestTimeMSecs);

            // Error recovery timeout
            using (CancellationTokenSource timeoutTCS = new CancellationTokenSource(TimeSpan.FromMilliseconds(errorHandlingTimeoutMSecs)))
            {


                // Start error recovery
                bool abortRequestSent = false;
                bool errorRecoveryInProgress = true;
                while (errorRecoveryInProgress)
                {
                    TransactionStatusRequest transactionStatusRequest = new TransactionStatusRequest()
                    {
                        MessageReference = new MessageReference()
                        {
                            MessageCategory = MessageCategory.Payment,
                            POIID = fusionClient.POIID,
                            SaleID = fusionClient.SaleID,
                            ServiceID = sessionId
                        }
                    };

                    try
                    {
                        // Send an abort request before the first TransactionStatusRequest
                        if (!abortRequestSent)
                        {
                            log.Information("Error recovery - sending AbortRequest");

                            AbortRequest abortRequest = new AbortRequest()
                            {
                                MessageReference = new MessageReference()
                                {
                                    MessageCategory = MessageCategory.Payment,
                                    POIID = fusionClient.POIID,
                                    SaleID = fusionClient.SaleID,
                                    ServiceID = sessionId
                                },

                                AbortReason = abortReason,
                            };
                            _ = await fusionClient.SendAsync(abortRequest);

                            abortRequestSent = true;
                        }

                        // Send TransactionStatusRequest, and wait for TransactionStatusResponse 
                        log.Information("Error recovery - sending TransactionStatusRequest");
                        TriggerMessagePayloadEvent(sessionId, transactionStatusRequest);
                        TransactionStatusResponse r = await fusionClient.SendRecvAsync<TransactionStatusResponse>(transactionStatusRequest, timeoutTCS.Token);
                        TriggerMessagePayloadEvent(sessionId, r);

                        // If the response to our TransactionStatus request is "Success", we have a PaymentResponse to check
                        if (r.Response.Result == Result.Success)
                        {
                            paymentResponse = r.RepeatedMessageResponse.RepeatedResponseMessageBody.PaymentResponse;
                            errorRecoveryInProgress = false;
                        }
                        // else if the transaction is still in progress, and we haven't reached our timeout
                        else if (r.Response.ErrorCondition == ErrorCondition.InProgress)
                        {
                            displayRequest.SetCashierDisplayAsPlainText("PAYMENT RECOVERY PLEASE WAIT");
                            TriggerMessagePayloadEvent(sessionId, displayRequest);
                        }
                        // otherwise, fail
                        else
                        {
                            paymentResponse.Response = r.Response;
                            errorRecoveryInProgress = false;
                        }
                    }
                    catch (DataMeshGroup.Fusion.NetworkException e)
                    {
                        log.Error(e, "Network exception during error recovery");

                        displayRequest.SetCashierDisplayAsPlainText("WAITING FOR CONNECTION");
                        TriggerMessagePayloadEvent(sessionId, displayRequest);
                    }
                    catch (DataMeshGroup.Fusion.TimeoutException e)
                    {
                        log.Error(e, "Timeout exception during error recovery");

                        displayRequest.SetCashierDisplayAsPlainText("TIMEOUT WAITING FOR HOST...");
                        TriggerMessagePayloadEvent(sessionId, displayRequest);
                    }
                    catch (TaskCanceledException tce)
                    {
                        log.Error(tce, "Timeout during error recovery");

                        paymentResponse.Response = new Response(ErrorCondition.UnreachableHost, "SYSTEM TIMEOUT - CHECK PINPAD FOR RESULT");
                        errorRecoveryInProgress = false;
                    }
                    catch (Exception e)
                    {
                        log.Error(e, "Unhandled exception during error recovery");

                        displayRequest.SetCashierDisplayAsPlainText("WAITING FOR CONNECTION...");
                        TriggerMessagePayloadEvent(sessionId, displayRequest);
                    }


                    try
                    {
                        if (errorRecoveryInProgress)
                        {
                            await Task.Delay(requestDelay); // TODO: THIS NEEDS TO BE IN TRY/CATCH
                        }
                    }
                    catch (TaskCanceledException tce)
                    {
                        log.Error(tce, "Timeout during error recovery");

                        paymentResponse.Response = new Response(ErrorCondition.UnreachableHost, "SYSTEM TIMEOUT - CHECK PINPAD FOR RESULT");
                        errorRecoveryInProgress = false;
                    }
                }
            }

            // return result
            return paymentResponse;
        }

        /// <summary>
        /// Function which is triggered on send/recv of Fusion message payload (both real and simulated). 
        /// Only really used to remove custom UI logic from SendPaymentRequest and PerformErrorRecovery
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="messagePayload"></param>
        /// <returns></returns>
        private void TriggerMessagePayloadEvent(string sessionId, MessagePayload messagePayload)
        {
            int paymentDialogOnSuccessTimeoutMSecs = 5000;
            int paymentDialogOnFailureTimeoutMSecs = 15000;

            switch (messagePayload)
            {
                case TransactionStatusRequest transactionStatusRequest:
                    currentMessagePayload = null;
                    ClearAutoHideTimer();
                    ShowPaymentDialog(paymentType, "PAYMENT IN PROGRESS", "CHECKING STATUS PLEASE WAIT", null, DialogType.Normal, false, false);
                    break;

                case TransactionStatusResponse r:
                    log.Verbose($"Status response. Success={r?.Response?.Success}");
                    currentMessagePayload = null;

                    PaymentDialogResponse paymentDialogResponse = new PaymentDialogResponse(r?.RepeatedMessageResponse?.RepeatedResponseMessageBody?.PaymentResponse);

                    // Payment dialog display logic
                    log.Verbose($"Display payment result dialog. PaymentType={paymentDialogResponse.PaymentType}, PaymentResultTitle={paymentDialogResponse.PaymentResultTitle}, ErrorTitle={paymentDialogResponse.ErrorTitle}, ErrorText={paymentDialogResponse.ErrorText}");

                    ShowPaymentDialog(
                            paymentDialogResponse.PaymentType,
                            paymentDialogResponse.PaymentResultTitle,
                            paymentDialogResponse.ErrorTitle,
                            paymentDialogResponse.ErrorText,
                            GetAdditionalText(paymentDialogResponse.Success),
                            paymentDialogResponse.Success ? DialogType.Success : DialogType.Error,
                            true,
                            false);

                    SetAutoHideTimer(paymentDialogResponse.Success ? paymentDialogOnSuccessTimeoutMSecs : paymentDialogOnFailureTimeoutMSecs);
                    break;


                // At this point the only time we will see a login request is when the POS manually sends a request. We should be careful here however, 
                // as a future version may include auto logins here
                case LoginRequest r:
                    paymentType = "LOGIN";
                    currentMessagePayload = messagePayload;
                    ClearAutoHideTimer();
                    ShowPaymentDialog(paymentType, "LOGIN IN PROGRESS", null, null, DialogType.Normal, false, false);
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
                    paymentDialogResponse = new PaymentDialogResponse(r.Response, "LOGIN");

                    log.Verbose($"Display login result dialog. PaymentType={paymentDialogResponse.PaymentType}, PaymentResultTitle={paymentDialogResponse.PaymentResultTitle}, ErrorTitle={paymentDialogResponse.ErrorTitle}, ErrorText={paymentDialogResponse.ErrorText}");
                    ShowPaymentDialog(
                            paymentDialogResponse.PaymentType,
                            paymentDialogResponse.PaymentResultTitle,
                            paymentDialogResponse.ErrorTitle,
                            paymentDialogResponse.ErrorText,
                            paymentDialogResponse.Success ? DialogType.Success : DialogType.Error,
                            true,
                            false);

                    SetAutoHideTimer(paymentDialogResponse.Success ? paymentDialogOnSuccessTimeoutMSecs : paymentDialogOnFailureTimeoutMSecs);

                    break;


                case PaymentRequest r:
                    paymentType = PaymentDialogResponse.PaymentTypeToDisplayText(r?.PaymentData?.PaymentType);
                    currentMessagePayload = messagePayload;
                    ClearAutoHideTimer();
                    ShowPaymentDialog(paymentType, "PAYMENT IN PROGRESS", null, null, DialogType.Normal, false, true);
                    break;

                case PaymentResponse r:
                    log.Verbose($"Payment response. Success={r?.Response?.Success}");
                    currentMessagePayload = null;

                    paymentDialogResponse = new PaymentDialogResponse(r);
                    log.Verbose($"Display payment result dialog. PaymentType={paymentDialogResponse.PaymentType}, PaymentResultTitle={paymentDialogResponse.PaymentResultTitle}, ErrorTitle={paymentDialogResponse.ErrorTitle}, ErrorText={paymentDialogResponse.ErrorText}");

                    ShowPaymentDialog(
                            paymentDialogResponse.PaymentType,
                            paymentDialogResponse.PaymentResultTitle,
                            paymentDialogResponse.ErrorTitle,
                            paymentDialogResponse.ErrorText,
                            GetAdditionalText(paymentDialogResponse.Success),
                            paymentDialogResponse.Success ? DialogType.Success : DialogType.Error,
                            true,
                            false);

                    SetAutoHideTimer(paymentDialogResponse.Success ? paymentDialogOnSuccessTimeoutMSecs : paymentDialogOnFailureTimeoutMSecs);
                    
                    break;
                case DisplayRequest r:
                    ShowPaymentDialog(paymentType, "PAYMENT IN PROGRESS", r.GetCashierDisplayAsPlainText()?.ToUpper(System.Globalization.CultureInfo.InvariantCulture), "", DialogType.Normal, false, true);
                    break;
            }
        }

        private decimal? CalcuteUnpaidAmount(PaymentResponse paymentResponse, out decimal? partialAuthorizedAmount)
        {
            decimal? paymentBalance = 0;
            partialAuthorizedAmount = null;
            if ((paymentResponse?.PaymentResult?.PaymentType == PaymentType.Normal) && (paymentResponse?.Response != null))
            {                
                Response response = paymentResponse.Response;
                //check if payment was successful or not
                if (response.Result != Result.Success) {

                    paymentBalance = null;
                    //check if payment was partially paid
                    if (response?.Result == Result.Partial)
                    {
                        partialAuthorizedAmount = paymentResponse?.PaymentResult?.AmountsResp?.PartialAuthorizedAmount;
                        decimal? requestedAmount = paymentResponse?.PaymentResult?.AmountsResp?.RequestedAmount;
                        if ((partialAuthorizedAmount != null) && (requestedAmount != null) && (partialAuthorizedAmount < requestedAmount))
                        {
                            //get the unpaid balance
                            paymentBalance = requestedAmount - partialAuthorizedAmount;
                        }
                    }
                }
            }           
            return paymentBalance;
        }

        private String GetAdditionalText(bool isPaymentSuccessful)
        {
            String additionalText = string.Empty;
            if (!isPaymentSuccessful && isPartiallyPaid)
            {
                //used to display unpaid amount in the screen
                additionalText = "Unpaid Balance = $" + currentUnpaidBalance;

                //reset values so they don't get processed again.
                currentUnpaidBalance = null;
                isPartiallyPaid = false;
            }

            return additionalText;
        }

        #region PaymentDialogUI

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

        private void ShowPaymentDialog(string caption, string title, string displayLine1, string displayText, DialogType dialogType, bool enableOk, bool enableCancel)
        {
            ShowPaymentDialog(caption, title, displayLine1, displayText, string.Empty, dialogType, enableOk, enableCancel);
        }

        private void ShowPaymentDialog(string caption, string title, string displayLine1, string displayText, string displayAddtionalText, DialogType dialogType, bool enableOk, bool enableCancel)
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
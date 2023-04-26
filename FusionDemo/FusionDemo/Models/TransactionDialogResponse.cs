using DataMeshGroup.Fusion.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionDemo.Models
{
    /// <summary>
    /// Helper class for util functions which help present a get totals response UI
    /// </summary>
    public class TransactionDialogResponse
    {
        public enum TransactionType { GetTotals, Login } //to do: add other transactions
        /// <summary>
        /// Construct a TransactionDialogResponse based on a getTotalsResponse response
        /// </summary>
        /// <param name="getTotalsResponse"></param>
        public TransactionDialogResponse(GetTotalsResponse getTotalsResponse)
        {
            CurrentTransactionType = TransactionType.GetTotals;

            GetTotalsResponse = getTotalsResponse;

            ErrorTitle = Success ? null : ErrorConditionToDisplayText(Response?.ErrorCondition);
            ErrorText = Success ? null : Response?.AdditionalResponse;            
        }

        public TransactionDialogResponse(Response response, TransactionType transactionType)
        {
            CurrentTransactionType = transactionType;

            Response = response;

            ErrorTitle = Success ? null : ErrorConditionToDisplayText(Response?.ErrorCondition);
            ErrorText = Success ? null : Response?.AdditionalResponse;
        }

        public TransactionType CurrentTransactionType { get; private set; }
        
        Response response = null;
        public Response Response
        {
            get => response ?? GetTotalsResponse?.Response ?? null;
            set => response = value;
        }

        public GetTotalsResponse GetTotalsResponse { get; internal set; }

        public bool Success => Response?.Success == true;

        public string ErrorTitle { get; set; }
        public string ErrorText { get; set; }

        /// <summary>
        /// Returns the title which should display at the top of the payment result dialog. 
        /// Depending on the result, will return: "PAYMENT APPROVED", "PAYMENT DECLINED", "LOGIN APPROVED", "LOGIN DECLINED"
        /// </summary>
        public string TransactionResultTitle
        {
            get
            {
                return TransactionTypeToDisplayText(CurrentTransactionType) + " " + (Response?.Success == true ? "APPROVED" : "DECLINED");
            }
        }        

        public static string TransactionTypeToDisplayText(TransactionType transactionType)
        {
            switch(transactionType)
            {
                case TransactionType.GetTotals:
                    return "GET TOTALS";
                case TransactionType.Login:
                    return "LOGIN";
                default:
                    return "UNKNOWN";
            }
        }
        private static string ErrorConditionToDisplayText(ErrorCondition? errorCondition)
        {
            switch (errorCondition)
            {
                case ErrorCondition.Aborted:
                    return "ABORTED";
                case ErrorCondition.Busy:
                    return "BUSY";
                case ErrorCondition.Cancel:
                    return "CANCEL";
                case ErrorCondition.DeviceOut:
                    return "DEVICE OUT";
                case ErrorCondition.InProgress:
                    return "IN PROGRESS";
                case ErrorCondition.InsertedCard:
                    return "INSERTED CARD";
                case ErrorCondition.InvalidCard:
                    return "INVALID CARD";
                case ErrorCondition.LoggedOut:
                    return "LOGGED OUT";
                case ErrorCondition.MessageFormat:
                    return "MESSAGE FORMAT";
                case ErrorCondition.NotAllowed:
                    return "NOT ALLOWED";
                case ErrorCondition.NotFound:
                    return "NOT FOUND";
                case ErrorCondition.PaymentRestriction:
                    return "PAYMENT RESTRICTION";
                case ErrorCondition.Refusal:
                    return "REFUSAL";
                case ErrorCondition.UnavailableDevice:
                    return "UNAVAILABLE DEVICE";
                case ErrorCondition.UnavailableService:
                    return "UNAVAILABLE SERVICE";
                case ErrorCondition.Unknown:
                    return "UNKNOWN";
                case ErrorCondition.UnreachableHost:
                    return "UNREACHABLE HOST";
                case ErrorCondition.WrongPIN:
                    return "WRONG PIN";
                case null:
                    return "UNKNOWN";
                default:
                    return errorCondition.Value.ToString();
            }

        }
    }
}

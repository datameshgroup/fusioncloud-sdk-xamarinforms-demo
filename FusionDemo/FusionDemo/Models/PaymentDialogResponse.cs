using DataMeshGroup.Fusion.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionDemo.Models
{
    /// <summary>
    /// Helper class for util functions which help present a payment response UI
    /// </summary>
    public class PaymentDialogResponse
    {
        /// <summary>
        /// Construct a PaymentDialogResponse based on a payment response
        /// </summary>
        /// <param name="paymentResponse"></param>
        public PaymentDialogResponse(PaymentResponse paymentResponse)
        {
            PaymentResponse = paymentResponse;

            ErrorTitle = Success ? null : ErrorConditionToDisplayText(Response?.ErrorCondition);
            ErrorText = Success ? null : Response?.AdditionalResponse;
            PaymentType = PaymentTypeToDisplayText(paymentResponse?.PaymentResult?.PaymentType ?? DataMeshGroup.Fusion.Model.PaymentType.Unknown);
        }

        /// <summary>
        /// Construct a PaymentDialogResponse based on a different response type (e.g. Login, CardAcquisition etc)
        /// </summary>
        /// <param name="paymentResponse"></param>
        public PaymentDialogResponse(Response response, string paymentType)
        {
            Response = response;

            ErrorTitle = Success ? null : ErrorConditionToDisplayText(Response?.ErrorCondition);
            ErrorText = Success ? null : Response?.AdditionalResponse;
            PaymentType = paymentType;
        }

        Response response = null;
        public Response Response
        {
            get => response ?? PaymentResponse?.Response ?? null;
            set => response = value;
        }

        public PaymentResponse PaymentResponse { get; internal set; }

        public bool Success => Response?.Success == true;


        public string PaymentType { get; set; }
        public string ErrorTitle { get; set; }
        public string ErrorText { get; set; }

        /// <summary>
        /// Returns the title which should display at the top of the payment result dialog. 
        /// Depending on the result, will return: "PAYMENT APPROVED", "PAYMENT DECLINED", "LOGIN APPROVED", "LOGIN DECLINED"
        /// </summary>
        public string PaymentResultTitle
        {
            get
            {
                return (PaymentType?.ToUpper() == "LOGIN" ? "LOGIN" : "PAYMENT") + " " + (Response?.Success == true ? "APPROVED" : "DECLINED");
            }
        }

        public static string PaymentTypeToDisplayText(PaymentType? paymentType)
        {
            switch (paymentType)
            {
                case DataMeshGroup.Fusion.Model.PaymentType.CashAdvance:
                    return "CASH ADVANCE";
                case DataMeshGroup.Fusion.Model.PaymentType.Refund:
                    return "REFUND";
                case DataMeshGroup.Fusion.Model.PaymentType.Unknown:
                    return "RECOVERY";
                case DataMeshGroup.Fusion.Model.PaymentType.Normal:
                case null:
                default:
                    return "PURCHASE";
            }
        }

        public static string ErrorConditionToDisplayText(ErrorCondition? errorCondition)
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

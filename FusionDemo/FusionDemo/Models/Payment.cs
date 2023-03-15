using System;
using System.Collections.Generic;
using System.Text;
using DataMeshGroup.Fusion.Model;

namespace FusionDemo.Models
{
    public class Payment
    {
        public PaymentRequest Request { get; set; }
        public PaymentResponse Response { get; set; }
    }
}

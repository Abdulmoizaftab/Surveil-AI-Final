//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SurveilAI.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class V_CashView
    {
        public string deviceid { get; set; }
        public System.DateTime postingdatetime { get; set; }
        public int denomination { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<long> QuantityAmount { get; set; }
        public Nullable<int> Replenish { get; set; }
        public Nullable<long> ReplenishAmount { get; set; }
        public Nullable<int> Dispense { get; set; }
        public Nullable<long> DispenseAmount { get; set; }
        public Nullable<int> Reject { get; set; }
        public Nullable<long> RejectAmount { get; set; }
    }
}
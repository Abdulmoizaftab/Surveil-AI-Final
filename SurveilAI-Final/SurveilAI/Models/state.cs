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
    
    public partial class state
    {
        public string deviceid { get; set; }
        public Nullable<int> devicestate { get; set; }
        public Nullable<int> eventid { get; set; }
        public Nullable<System.DateTime> timestamp { get; set; }
        public bool connected { get; set; }
        public Nullable<System.DateTime> LastConTime { get; set; }
        public Nullable<bool> Network { get; set; }
        public Nullable<System.DateTime> NetworkConTime { get; set; }
    }
}

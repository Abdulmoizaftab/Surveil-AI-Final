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
    
    public partial class eventstore
    {
        public string deviceid { get; set; }
        public System.DateTime timestamp { get; set; }
        public int messageno { get; set; }
        public int eventno { get; set; }
        public int eventid { get; set; }
        public string orgmessage { get; set; }
        public Nullable<System.DateTime> servertimestamp { get; set; }
    }
}

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
    
    public partial class problemdata
    {
        public int remnumber { get; set; }
        public int repnumber { get; set; }
        public string remuser { get; set; }
        public string remextuser { get; set; }
        public System.DateTime remtime { get; set; }
        public string remark { get; set; }
    
        public virtual problem problem { get; set; }
        public virtual problem problem1 { get; set; }
    }
}

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
    
    public partial class cashstock
    {
        public string deviceid { get; set; }
        public int containerid { get; set; }
        public int containeritemno { get; set; }
        public int stockcounter { get; set; }
        public System.DateTime postingdatetime { get; set; }
        public int quantity { get; set; }
        public int cashout { get; set; }
        public int cashin { get; set; }
        public int reject { get; set; }
        public Nullable<int> turnovercashout { get; set; }
        public Nullable<int> turnovercashin { get; set; }
        public Nullable<int> turnoverreject { get; set; }
    
        public virtual cashcontitem cashcontitem { get; set; }
        public virtual cashcontitem cashcontitem1 { get; set; }
    }
}
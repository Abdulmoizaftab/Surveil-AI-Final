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
    
    public partial class usersession
    {
        public string domain { get; set; }
        public string extuserid { get; set; }
        public string desktop { get; set; }
        public string ipaddress { get; set; }
        public Nullable<int> port { get; set; }
        public Nullable<System.DateTime> logintime { get; set; }
        public Nullable<System.DateTime> lastactiontime { get; set; }
        public string terminalserver { get; set; }
        public Nullable<int> sessionflag { get; set; }
        public string username { get; set; }
        public string sessionid { get; set; }
    
        public virtual extuser extuser { get; set; }
    }
}
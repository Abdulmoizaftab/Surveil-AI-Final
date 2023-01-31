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
    
    public partial class extuser
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public extuser()
        {
            this.usersessions = new HashSet<usersession>();
        }
    
        public string domain { get; set; }
        public string extuserid { get; set; }
        public string userid { get; set; }
        public int isuser { get; set; }
        public string password { get; set; }
        public int amethod { get; set; }
        public int admin { get; set; }
        public int locked { get; set; }
        public Nullable<System.DateTime> lockeduntil { get; set; }
        public int failedlogins { get; set; }
        public Nullable<System.DateTime> pwdexpires { get; set; }
        public Nullable<System.DateTime> pwdchanged { get; set; }
        public int policyid { get; set; }
        public Nullable<System.DateTime> validfrom { get; set; }
        public Nullable<System.DateTime> validto { get; set; }
        public string contactid { get; set; }
        public Nullable<System.DateTime> lastlogontime { get; set; }
        public int showonlogin { get; set; }
        public string usergroup { get; set; }
        public Nullable<int> failedtries { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<usersession> usersessions { get; set; }
    }
}
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
    
    public partial class userprofile
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public userprofile()
        {
            this.userprofilefuncs = new HashSet<userprofilefunc>();
            this.userprofilefuncs1 = new HashSet<userprofilefunc>();
            this.users_ = new HashSet<users_>();
            this.users_1 = new HashSet<users_>();
        }
    
        public string userprofid { get; set; }
        public string funcmask { get; set; }
        public Nullable<int> statebeepmask { get; set; }
        public Nullable<int> allfolderagent { get; set; }
        public Nullable<int> reversefolder { get; set; }
        public Nullable<int> allfolderserver { get; set; }
        public Nullable<int> allfiletypes { get; set; }
        public Nullable<int> filetypes { get; set; }
        public string normalfiletypes { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<userprofilefunc> userprofilefuncs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<userprofilefunc> userprofilefuncs1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<users_> users_ { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<users_> users_1 { get; set; }
    }
}
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
    
    public partial class passwordpolicy
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public passwordpolicy()
        {
            this.extuser_ = new HashSet<extuser_>();
            this.extuser_1 = new HashSet<extuser_>();
        }
    
        public int policyid { get; set; }
        public string description { get; set; }
        public int minlength { get; set; }
        public int maxlength { get; set; }
        public Nullable<int> mincapital { get; set; }
        public int mindigits { get; set; }
        public int maxattempts { get; set; }
        public int minage { get; set; }
        public int maxage { get; set; }
        public int historysize { get; set; }
        public int changefirst { get; set; }
        public int lockduration { get; set; }
        public int changehint { get; set; }
        public int maxlogouttime { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<extuser_> extuser_ { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<extuser_> extuser_1 { get; set; }
    }
}

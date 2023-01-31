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
    
    public partial class eventbase
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public eventbase()
        {
            this.eventconversions = new HashSet<eventconversion>();
            this.eventconversions1 = new HashSet<eventconversion>();
        }
    
        public int eventno { get; set; }
        public int textno { get; set; }
        public int texttype { get; set; }
        public int setbit { get; set; }
        public int unsetbit { get; set; }
        public int componentid { get; set; }
        public int compsetbit { get; set; }
        public int compunsetbit { get; set; }
        public int target { get; set; }
        public int forwarddesktop { get; set; }
        public int forwardrule { get; set; }
        public Nullable<int> eventgroupid { get; set; }
        public Nullable<int> confidential { get; set; }
        public string confidentialmask { get; set; }
        public Nullable<int> masktype { get; set; }
    
        public virtual component component { get; set; }
        public virtual component component1 { get; set; }
        public virtual message0001 message0001 { get; set; }
        public virtual message0001 message00011 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<eventconversion> eventconversions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<eventconversion> eventconversions1 { get; set; }
    }
}
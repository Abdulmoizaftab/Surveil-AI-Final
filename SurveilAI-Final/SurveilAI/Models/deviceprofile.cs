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
    
    public partial class deviceprofile
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public deviceprofile()
        {
            this.componentmaps = new HashSet<componentmap>();
            this.componentmaps1 = new HashSet<componentmap>();
            this.device_ = new HashSet<device_>();
            this.device_1 = new HashSet<device_>();
            this.device_2 = new HashSet<device_>();
            this.device_3 = new HashSet<device_>();
            this.device_4 = new HashSet<device_>();
            this.device_5 = new HashSet<device_>();
            this.device_6 = new HashSet<device_>();
            this.device_7 = new HashSet<device_>();
            this.eventconversions = new HashSet<eventconversion>();
            this.eventconversions1 = new HashSet<eventconversion>();
            this.eventconversions2 = new HashSet<eventconversion>();
            this.eventconversions3 = new HashSet<eventconversion>();
        }
    
        public int devicetype { get; set; }
        public string devicedescr { get; set; }
        public string manuf { get; set; }
        public string devclass { get; set; }
        public string devfunc { get; set; }
        public Nullable<int> gatewaytype { get; set; }
        public Nullable<int> iconid { get; set; }
        public Nullable<int> versionid { get; set; }
        public Nullable<int> eodkeygroupid { get; set; }
        public Nullable<int> calcstatsstate { get; set; }
        public Nullable<int> inventoryid { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<componentmap> componentmaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<componentmap> componentmaps1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<device_> device_ { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<device_> device_1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<device_> device_2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<device_> device_3 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<device_> device_4 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<device_> device_5 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<device_> device_6 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<device_> device_7 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<eventconversion> eventconversions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<eventconversion> eventconversions1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<eventconversion> eventconversions2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<eventconversion> eventconversions3 { get; set; }
    }
}

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
    
    public partial class component
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public component()
        {
            this.componentmaps = new HashSet<componentmap>();
            this.componentmaps1 = new HashSet<componentmap>();
            this.eventbases = new HashSet<eventbase>();
            this.eventbases1 = new HashSet<eventbase>();
        }
    
        public int componentid { get; set; }
        public Nullable<int> textno { get; set; }
        public Nullable<int> texttype { get; set; }
        public int active { get; set; }
        public Nullable<bool> ForMonitoring { get; set; }
    
        public virtual message0001 message0001 { get; set; }
        public virtual message0001 message00011 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<componentmap> componentmaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<componentmap> componentmaps1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<eventbase> eventbases { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<eventbase> eventbases1 { get; set; }
    }
}
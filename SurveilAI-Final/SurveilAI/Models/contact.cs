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
    using System.ComponentModel.DataAnnotations.Schema;
    using System.ComponentModel.DataAnnotations;

    public partial class contact
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public contact()
        {
            this.extusers = new HashSet<extuser>();
            this.extuser_ = new HashSet<extuser_>();
        }
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        [Required(ErrorMessage = "Contact ID Required")]
        public string contactid { get; set; }
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        [Required(ErrorMessage = "Last Name Required")]
        public string lname { get; set; }
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        [Required(ErrorMessage = "First Name Required")]
        public string fname { get; set; }
        public string company { get; set; }
        //[RegularExpression(@"^[0-9]{11}$", ErrorMessage = "Please enter a valid Phone Number.")]
        public string phone { get; set; }
        public string fax { get; set; }
        [Required(ErrorMessage = "This Field is required")]
        //[RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", ErrorMessage = "Please enter a valid Email Address.")]
        public string e_mail { get; set; }
        public string cfunction { get; set; }
        public string language { get; set; }
        //[RegularExpression(@"^[0-9]{11}$", ErrorMessage = "Please enter a valid Mobile Number.")]
        public string mobile { get; set; }
        public Nullable<int> voice { get; set; }
        public Nullable<int> voiceid { get; set; }
        public string pin { get; set; }
        public string mailtemplate { get; set; }
        public string smsmailaddress { get; set; }
        public string hierlevel { get; set; }

        [NotMapped]
        public string ContactidOld { get; set; }
        public string check { get; set; }

        public List<contact> data = new List<contact>();

        public virtual hierarchy_ hierarchy_ { get; set; }
        public virtual hierarchy_ hierarchy_1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<extuser> extusers { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<extuser_> extuser_ { get; set; }
    }
}
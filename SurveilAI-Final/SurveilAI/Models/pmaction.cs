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
    
    public partial class pmaction
    {
        public string pmid { get; set; }
        public string pmuserid { get; set; }
        public int step { get; set; }
        public int deactivated { get; set; }
        public int onlyonerror { get; set; }
        public int type { get; set; }
        public string jobid { get; set; }
        public string contactid { get; set; }
        public string adress { get; set; }
        public Nullable<int> classification { get; set; }
        public string reporttopic { get; set; }
        public string mt_subject { get; set; }
        public string mt_message { get; set; }
        public string command { get; set; }
        public string vardata { get; set; }
    }
}

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

    public partial class job
    {
        public string jobid { get; set; }
        public int commandno { get; set; }
        public int type { get; set; }
        public string command { get; set; }
        public string vardata { get; set; }
        public Nullable<int> delivery { get; set; }
        public string address { get; set; }
        public string mt_subject { get; set; }
        public string mt_message { get; set; }
        public string contactid { get; set; }
        public string resultfile { get; set; }

        public virtual jobschedule jobschedule { get; set; }
        public virtual jobschedule jobschedule1 { get; set; }
        public virtual jobschedule jobschedule2 { get; set; }
        public virtual jobschedule jobschedule3 { get; set; }
        public List<job> Jobs = new List<job>();
        public List<jobresult> JRData = new List<jobresult>();
    }
}

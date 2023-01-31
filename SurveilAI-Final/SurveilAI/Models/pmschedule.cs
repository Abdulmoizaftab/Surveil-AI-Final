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
    using System.ComponentModel.DataAnnotations;

    public partial class pmschedule
    {
        public string check { get; set; }
        [Required(ErrorMessage = "Enter Rule |ame"), MaxLength(30)]
        public string pmid { get; set; }
        public string pmuserid { get; set; }
        public string description { get; set; }
        public Nullable<int> active { get; set; }
        public int actioncount { get; set; }
        public string startit { get; set; }
        public string stopit { get; set; }
        public int dayofweek { get; set; }
        public string condition { get; set; }
        public int triggertype { get; set; }
        public Nullable<int> statebitmask { get; set; }
        public Nullable<int> setreset { get; set; }
        public Nullable<int> compid { get; set; }
        public Nullable<int> compstbmask { get; set; }
        public Nullable<int> compsetreset { get; set; }
        public Nullable<int> leveltriggered { get; set; }
        public Nullable<int> problemevent { get; set; }
        public Nullable<int> classification { get; set; }
        public Nullable<int> noevents { get; set; }
        public Nullable<int> nodevents { get; set; }
        public Nullable<int> delay { get; set; }
        public string calname { get; set; }
        public string caluserid { get; set; }
        public Nullable<int> timetoescalate { get; set; }
        public Nullable<int> escalation { get; set; }
        public string problempmid { get; set; }
        public string problempmuserid { get; set; }
        public Nullable<int> timetype { get; set; }
        public int econsameevent { get; set; }
        public int echascorrelcond { get; set; }
        public int echascancelcond { get; set; }
        public Nullable<int> eccanceltrigger { get; set; }
        public Nullable<int> eccheckonlyonce { get; set; }
        public string hierlevel { get; set; }
        public Nullable<int> eccheckbeforefire { get; set; }
        public int resultneeded { get; set; }
        public int journalneeded { get; set; }
        public string execcalname { get; set; }
        public Nullable<int> execcaltype { get; set; }

        public List<pmschedule> data = new List<pmschedule>();
        public List<Hierarchy> Hierar = new List<Hierarchy>();
        public List<string> devices = new List<string>();
        public List<string> action = new List<string>();
        public List<Data2> state = new List<Data2>();
        public List<message0001> events = new List<message0001>();
        public string eventno { get; set; } //pmevents
        public string[] Actdata { get; set; } //pmaction

        public List<message0001> Sendevents = new List<message0001>();

    }

    public class Data2
    {
        public string Color { get; set; }
        public string Messagetext { get; set; }
        public int Bit { get; set; }
    }

    public class Data3
    {
        public int textno { get; set; }
    }
}

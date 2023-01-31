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

    public partial class Archive
    {
        public string Archive1 { get; set; }
        public Nullable<int> NoOfDays { get; set; }
        public Nullable<decimal> Size { get; set; }
        public bool ArchivePolicy { get; set; }
        public bool PurgePolicy { get; set; }
        public bool ByDays { get; set; }
        public bool BySize { get; set; }
        public bool IsActive { get; set; }
        public Nullable<System.DateTime> ArchivedTill { get; set; }
        public Nullable<System.DateTime> JobStarted { get; set; }
        public Nullable<System.DateTime> JobEnded { get; set; }
        public List<TableArchive> tableArchives { get; set; }
        public List<Archive> Archives { get; set; }

        public string Msg { get; set; }
    }

    public class TableArchive
    {
        public string name { get; set; }
        public string rows { get; set; }
        public string reserved { get; set; }
        public string data { get; set; }
        public string index_size { get; set; }
        public string unused { get; set; }
    }
}

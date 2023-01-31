using System;
using System.Collections.Generic;

namespace SurveilAI.DataContext
{
    internal class FullCalendar
    {
        public FullCalendar()
        {
            HijriDates = new List<LunarCalendar>();
        }
        public string Description { get; set; }
        public string Subject { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string ThemeColor { get; set; }
        public bool isFullDay { get; set; }
        public List<LunarCalendar> HijriDates { get; set; }
    }

    public class LunarCalendar
    {
        public string LunarDate { get; set; }
        public Nullable<System.DateTime> SolarDate { get; set; }
    }
}
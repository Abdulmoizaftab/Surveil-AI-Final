using SurveilAI.Models;
using System.Collections.Generic;

namespace SurveilAI.DataContext
{
    public class ResourceViewModel
    {
        public string devices { get; set; }
        public List<string> devicesheir = new List<string>();
        public List<Hierarchy> Hierar = new List<Hierarchy>();

    }
}
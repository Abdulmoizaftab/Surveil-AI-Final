using SurveilAI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SurveilAI.DataContext
{
    public  class  TypeCameras 
    {

        public string DeviceID { get; set; }
        public string DeviceType { get; set; }
        public int CamId { get; set; }
        public int CHierLevel { get; set; }
        public string CamName { get; set; }
        public string CamType { get; set; }

        public int ACid { get; set; }
        public Nullable<int> ACHierLevel { get; set; }
        public string Cam1 { get; set; }
        public string Cam2 { get; set; }
        public string Cam3 { get; set; }
        public string Cam4 { get; set; }
        public string Cam5 { get; set; }
        public string Cam6 { get; set; }
        public string Cam7 { get; set; }
        public string Cam8 { get; set; }
        public string Cam9 { get; set; }
        public string Cam10 { get; set; }
        public string Cam11 { get; set; }
        public string Cam12 { get; set; }
        public string Cam13 { get; set; }
        public string Cam14 { get; set; }
        public string Cam15 { get; set; }
        public string Cam16 { get; set; }
        public string Cam1Type { get; set; }
        public string Cam2Type { get; set; }
        public string Cam3Type { get; set; }
        public string Cam4Type { get; set; }
        public string Cam5Type { get; set; }
        public string Cam6Type { get; set; }
        public string Cam7Type { get; set; }
        public string Cam8Type { get; set; }
        public string Cam9Type { get; set; }
        public string Cam10Type { get; set; }
        public string Cam11Type { get; set; }
        public string Cam12Type { get; set; }
        public string Cam13Type { get; set; }
        public string Cam14Type { get; set; }
        public string Cam15Type { get; set; }
        public string Cam16Type { get; set; }
        public Nullable<System.DateTime> Date { get; set; }

        public virtual Device Device { get; set; }


    }
}
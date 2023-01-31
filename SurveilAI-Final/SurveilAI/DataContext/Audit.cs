using SurveilAI.Models;
using System;

namespace SurveilAI.DataContext
{
    public class Audit
    {
        #region private_varaibles
        private SurveilAIEntities db = new SurveilAIEntities();
        #endregion

        #region publicfunction
        public void Log(pvjournal obj)
        {
            string result;
            try
            {
                var output2 = db.Database.ExecuteSqlCommand("INSERT INTO [dbo].[pvjournal]([serverdatetime],[issuertype],[issuer],[pvuser],[device],[command],[adminaction],[errorcode],[cmdstat],[vardata],[desktopuser],[Operation_Type],[newvalue],[oldvalue],[functionid])Values(GETDATE(),'" + obj.issuertype + "','" + obj.issuer + "','" + obj.pvuser + "','" + obj.device + "','" + obj.command + "','" + obj.adminaction + "','" + obj.errorcode + "','" + obj.cmdstat + "','" + obj.vardata + "','" + obj.desktopuser + "','" + obj.Operation_Type + "','" + obj.newvalue + "','" + obj.oldvalue + "','" + obj.functionid + "');");

                if (output2 == 1)
                {
                    result = "Transaction has been logged";
                }
                else
                {
                    result = "Error in logging";
                }
            }
            catch (Exception ex)
            {
                result = "Error in logging " + ex.Message;
            }
        }
        #endregion
        public class EmailUtil
        {
            public string EmailAddress { get; set; }
            public string Host { get; set; }
            public string Port { get; set; }
            public string Password { get; set; }
        }
    }
}

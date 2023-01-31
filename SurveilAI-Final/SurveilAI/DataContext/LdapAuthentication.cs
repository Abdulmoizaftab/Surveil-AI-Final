using NLog;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Web;

namespace SurveilAI.DataContext
{
    public class LdapAuthentication
    {
        private String _path;
        private String _filterAttribute;


        ILogger userlog = LogManager.GetLogger("user");
        ILogger activitylog = LogManager.GetLogger("activity");
        ILogger errorlog = LogManager.GetLogger("error");

        public LdapAuthentication(String path)
        {
            _path = path;
        }

        public bool IsAuthenticated(String domain, String username, String pwd)
        {
            String domainAndUsername = domain + @"\" + username;
            DirectoryEntry entry = new DirectoryEntry(_path, domainAndUsername, pwd);

            try
            {
                Object obj = entry.NativeObject;

                DirectorySearcher search = new DirectorySearcher(entry);

                search.Filter = "(SAMAccountName=" + username + ")";
                search.PropertiesToLoad.Add("cn");
                SearchResult result = search.FindOne();

                if (null == result)
                {
                    return false;
                }

                _path = result.Path;
                _filterAttribute = (String)result.Properties["cn"][0];
            }
            catch (Exception ex)
            {
                errorlog.Error("Error: " + ex);
                return false;
            }

            return true;
        }

    }
}
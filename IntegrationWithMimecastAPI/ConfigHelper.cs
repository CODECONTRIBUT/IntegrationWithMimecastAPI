using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace IntegrationWithMimecastAPI
{
    public class ConfigHelper
    {
        public static string GetMimecastHeldEmailListUri()
        {
            return ConfigurationManager.AppSettings["MimecastUri"];
        }

        public static int GetMimecastHeldEmailsInterval()
        {
            return int.Parse(ConfigurationManager.AppSettings["MimecastInterval"].Trim());
        }

        public static string GetMimecastBaseUrl()
        {
            return ConfigurationManager.AppSettings["MimecastBaseUrl"];
        }

        public static string GetMimecastAccessKey()
        {
            return ConfigurationManager.AppSettings["MimecastAccessKey"];
        }
        public static string GetMimecastSecretKey()
        {
            return ConfigurationManager.AppSettings["MimecastSecretKey"];
        }
        public static string GetMimecastApplicationId()
        {
            return ConfigurationManager.AppSettings["MimecastAppId"];
        }
        public static string GetMimecastApplicationKey()
        {
            return ConfigurationManager.AppSettings["MimecastAppKey"];
        }
        public static int GetMimecastHeldEmailsPageSize()
        {
            return int.Parse(ConfigurationManager.AppSettings["MimecastPageSize"].Trim());
        }
    }
}

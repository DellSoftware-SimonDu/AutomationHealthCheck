using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Api
{
    public class Host
    {
        public static string Name
        {
            get { return IsTestWebsite ? "test.spotlightessentials.com" : "www.spotlightessentials.com"; }
        }

        public static bool IsTestWebsite
        {
            get
            {
                return string.Equals(
                    ConfigurationManager.AppSettings["HostingEnvironment"],
                    "test", StringComparison.OrdinalIgnoreCase);
            }
        }

        public static string ApiHost
        {
            get
            {
                switch (ConfigurationManager.AppSettings["HostingEnvironment"])
                {
                    case "Development":
                        return "http://localhost:60189";
                    case "Test":
                        return "https://testapi.spotlightessentials.com";
                    case "Production":
                    default:
                        return "https://api.spotlightessentials.com";
                }
            }
        }
    }
}
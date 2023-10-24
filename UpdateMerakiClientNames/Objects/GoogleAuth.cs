using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateMerakiClientNames.Objects
{
    public class GoogleAuth
    {
        public string? CustomerID { get; set; }
        public string? AppName { get; set; }
        public string? APIKey { get; set; }
        public string ClientSecretJsonFile { get; set; } = "";
    }
}

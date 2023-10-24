using System.Collections.Generic;

namespace UpdateMerakiClientNames.Objects.Meraki
{
    public class clientRequestBody
    {
        public string devicePolicy { get; set; } = "normal";
        public string groupPolicyId { get; set; } = null;
        public List<client> clients { get; set; }


        public clientRequestBody() { }
    }
}

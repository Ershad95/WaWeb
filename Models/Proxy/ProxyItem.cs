using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaWeb.Models.Proxy
{
    public class ProxyItem
    {
        //public string _id { get; set; }
        public string ip { get; set; }
        //public string anonymityLevel { get; set; }
        //public string asn { get; set; }
        //public string city { get; set; }
        //public string country { get; set; }
        //public string created_at { get; set; }
        //public string google { get; set; }
        //public string isp { get; set; }
        //public int lastChecked { get; set; }
        //public double latency { get; set; }
        //public string org { get; set; }
        public string port { get; set; }
        public List<string> protocols { get; set; }
        //public string region { get; set; }
        //public string responseTime { get; set; }
        public int speed { get; set; }
        //public string updated_at { get; set; }
        //public string workingPercent { get; set; }
    }
}

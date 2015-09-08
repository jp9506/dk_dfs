using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Cache;
using System.IO;

namespace dk
{
    public class Browser
    {
        public static string[] Browse(string URL)
        {
            string url = URL + (URL.Contains('?') ? "&" : "?") + "myvarticks=" + DateTime.Now.Ticks;
            WebRequest HttpWebReq = WebRequest.Create(url);
            HttpWebReq.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
            WebResponse HttpWebResponse = HttpWebReq.GetResponse();
            Stream ResponseStream = HttpWebResponse.GetResponseStream();
            StreamReader sr = new StreamReader(ResponseStream);
            List<string> res = new List<string>();
            while (!sr.EndOfStream)
            {
                res.Add(sr.ReadLine());
            }
            return res.ToArray();
        }
    }
}

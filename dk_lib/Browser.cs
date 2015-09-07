using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace dk
{
    public class Browser
    {
        public static string[] Browse(string URL)
        {
            WebRequest HttpWebReq = WebRequest.Create(URL);
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

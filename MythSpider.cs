using System;
using System.Net;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace pinkspider
{
	public class MythSpider
	{
		HttpWebRequest request;
		private List<string> NameLists;
		//private Dictionary<string,int> WebLists;
		private List<string> BrowserLists;
		public string GetStatics(string tagName)
		{

			try
			{
                StreamReader sr = GetRequestCount("http://index.iqiyi.com/q/?name=" + tagName);
                if (sr == null)
                    return string.Empty;

				string ret = string.Empty;
				for(;;){
					string t = sr.ReadLine();
					if(t == null){
						break;
					}
					//var videoIndexStat
					if(t.Contains("var videoIndexStat")){
						var sp = t.Split('=');
						if(sp.Length > 1){
							//Console.WriteLine(sp[1]);
							ret = sp[1];
							ret = ret.Replace(" ","");
							ret = ret.Replace(";","");
							break;
						}
					}
				}
				sr.Close();
				return ret;
			}catch{
				Console.WriteLine ("Error on Tag:" + tagName);
				return string.Empty;
			}
		}

		private string GetTitleCore(string html)
		{
			StreamReader sr = GetRequestCount (html);

            if (sr == null)
                return string.Empty;

			const string pattern = @"(?<=title\>).*(?=</title)";
			Regex r = new Regex (pattern, RegexOptions.IgnoreCase); //新建正则模式
			string ret = string.Empty;
			for (;;) {
				string t = sr.ReadLine ();
				if (t == null) {
					break;
				} else {
					MatchCollection m = r.Matches (t); //获得匹配结果
					if (m.Count > 0) {
						ret = m [0].ToString ();
						ret = ret.Replace ("<title>", "");
						ret = ret.Replace ("</title>", "");
						break;
					}

				}
			}
			//request.Abort ();
			//response.Close ();
			sr.Close ();
			return ret;
		}
		private StreamReader GetRequestCount(string html,int times = 10){
			StreamReader sr = null;
			for (int i = 0; i < times; i++) {
				if (sr != null)
					break;
				try {
					if(request != null){
						request.Abort();
						//request.GetResponse().Close();
					}
					request = (HttpWebRequest)HttpWebRequest.Create (html);    //创建一个请求示例
					request.AllowAutoRedirect = true;
					request.Timeout = 1000;
					HttpWebResponse response = (HttpWebResponse)request.GetResponse ();//获取响应，即发送请求
					var responseStream = response.GetResponseStream ();
					sr = new StreamReader (responseStream, Encoding.UTF8);
					//request.Abort();
				} catch {
					sr = null;
					Console.WriteLine ("Reconnecting:" + html);
				}
			}
			return sr;
		}

		private List<string> GetLinksCore(string html)
		{
			StreamReader sr = GetRequestCount (html);
            if (sr == null)
                return null;
			const string pattern = @"http://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?";
			Regex r = new Regex (pattern, RegexOptions.IgnoreCase); //新建正则模式
			MatchCollection m = r.Matches (sr.ReadToEnd()); //获得匹配结果
			List<string> links = new List<string>(); 

			for (int i = 0; i < m.Count; i++) {
				string s = m [i].ToString ();
				if(s.Contains(".html")){	//perhaps add ?list
                    if (!s.Contains("list.iqiyi"))
                    {
                        if (!BrowserLists.Contains(s))
                        {
                            BrowserLists.Add(s);
                            links.Add(s); //提取出结果
                        }
                    }
				}
			}
			sr.Close ();
			return links;
		}

        private void WriteStatics(string url)
        {
            string title = GetTitleCore(url);
            if (!title.Equals(string.Empty))
            {
                var realtitles = title.Split('-');
                if (realtitles.Length > 0)
                {
                    string realtitle = realtitles[0];
                    if (!NameLists.Contains(realtitle))
                    {
                        //Console.WriteLine (realtitle);
                        NameLists.Add(realtitle);
                        string ret = GetStatics(realtitle);
                        if (!ret.Equals(string.Empty))
                        {
                            Console.WriteLine("hit:" + realtitle);
                            FileStream fs = new FileStream("out.txt", FileMode.Append);
                            StreamWriter sw = new StreamWriter(fs);
                            sw.WriteLine(ret);
                            sw.Close();
                            fs.Close();
                        }
                    }
                }

            }
        }
		public void StartLoop(string html){
			//string[] tmp = { html };
            List<string> superlist = new List<string>();
            superlist.Add(html);
            for (; ; )
            {
                if (superlist.Count == 0)
                    break;
                var s = superlist[0];
                superlist.AddRange(GetLinksCore(s));
                WriteStatics(s);
                superlist.RemoveAt(0);
            }
		}
		public MythSpider ()
		{
			request = null;
			NameLists = new List<string> ();
			//WebLists = new Dictionary<string, int> ();
			BrowserLists = new List<string> ();
		}
	}
}


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
        private HttpWebRequest request;
        private HashSet<string> NameLists;
        private List<string> superlist;
        private int mindex;
        public string GetStatics(string tagName)
        {

            try
            {
                StreamReader sr = GetRequestCount("http://index.iqiyi.com/q/?name=" + tagName);
                if (sr == null)
                    return string.Empty;

                string ret = string.Empty;
                for (; ; )
                {
                    string t = sr.ReadLine();
                    if (t == null)
                    {
                        break;
                    }
                    //var videoIndexStat
                    if (t.Contains("var videoIndexStat"))
                    {
                        var sp = t.Split('=');
                        if (sp.Length > 1)
                        {
                            //Console.WriteLine(sp[1]);
                            ret = sp[1];
                            ret = ret.Replace(" ", "");
                            ret = ret.Replace(";", "");
                            break;
                        }
                    }
                }
                sr.Close();
                return ret;
            }
            catch
            {
                Console.WriteLine("Error on Tag:" + tagName);
                return string.Empty;
            }
        }

        public List<string> GetList()
        {
            return superlist;
        }

        private string GetTitleCore(string src)
        {
            string ret = string.Empty;

            const string pattern = @"(?<=title\>).*(?=</title)";
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase); //新建正则模式
            MatchCollection m = r.Matches(src); //获得匹配结果
            if (m.Count > 0)
            {
                ret = m[0].ToString();
                ret = ret.Replace("<title>", "");
                ret = ret.Replace("</title>", "");
            }
            return ret;
        }
        private StreamReader GetRequestCount(string html, int times = 5)
        {
            StreamReader sr = null;
            for (int i = 0; i < times; i++)
            {
                if (sr != null)
                    break;
                try
                {
                    if (request != null)
                    {
                        request.Abort();
                        //request.GetResponse().Close();
                    }
                    request = (HttpWebRequest)HttpWebRequest.Create(html);    //创建一个请求示例
                    request.AllowAutoRedirect = true;
                    request.Timeout = 1000 / 2;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();//获取响应，即发送请求
                    var responseStream = response.GetResponseStream();
                    sr = new StreamReader(responseStream, Encoding.UTF8);
                    //request.Abort();
                }
                catch
                {
                    sr = null;
                    //Console.WriteLine ("Reconnecting:" + html);
                }
            }
            return sr;
        }

        private List<string> GetLinksCore(string html)
        {
            List<string> links = new List<string>();
            try
            {
                StreamReader sr = GetRequestCount(html);
                if (sr == null)
                {
                    Console.WriteLine("thread {0}:ReadFailed,{1}" ,mindex,html);
                    return links;
                }

                const string pattern = @"http://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?";
                string str = sr.ReadToEnd();
                WriteStatics(str);
                Regex r = new Regex(pattern, RegexOptions.IgnoreCase); //新建正则模式
                MatchCollection m = r.Matches(str); //获得匹配结果

                for (int i = 0; i < m.Count; i++)
                {
                    string s = m[i].ToString();
                    if (s.Contains(".html"))
                    {	//perhaps add ?list
                        if (!s.Contains("list.iqiyi"))
                        {
                            //video_name
                            var splits = Regex.Split(s, ".html");
                            if (splits.Length > 0)
                            {
                                s = splits[0];
                                s = s + ".html";
                            }
                            if (!Global.BrowserLists.Contains(s))
                            {
                                Global.BrowserLists.Add(s);
                                links.Add(s); //提取出结果
                            }
                        }
                    }
                }
                sr.Close();
            }
            catch (Exception ee)
            {
                Console.WriteLine("thread {0}:{1}", mindex, ee.Message);
            }
            return links;
        }

        private void SaveBackground(List<string> list)
        {
            Console.WriteLine("Saving Background,{0},elements", list.Count);
            var t = DateTime.Now;
            StreamWriter sw = new StreamWriter("history.log",false,Encoding.UTF8);
            foreach (string s in list)
            {
                sw.WriteLine(s);
            }
            sw.Close();
        }
        private void WriteStatics(string src)
        {
            string title = GetTitleCore(src);
            if (!title.Equals(string.Empty))
            {
                var realtitles = title.Split('-');
                if (realtitles.Length > 0)
                {
                    string realtitle = realtitles[0];
                    var sptitle = Regex.Split(realtitle, "&nbsp");
                    if (sptitle.Length > 0)
                        realtitle = sptitle[0];
                    if (!Global.NameLists.Contains(realtitle))
                    {
                        Global.NameLists.Add(realtitle);
                        string ret = GetStatics(realtitle);
                        if (!ret.Equals(string.Empty))
                        {
                            Console.WriteLine("thread {0}:hit,{1}",mindex,realtitle);
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
        public void StartLoop(string html)
        {
            string[] tmp = { html };
            StartLoop(tmp,0);
        }
        public void StartLoop(string[] history,int index)
        {
            mindex = index;
            superlist = new List<string>(history);
            //int t = superlist.Count;
            for (; ; )
            {
                if (superlist.Count == 0)
                    break;
                var s = superlist[0];
                superlist.AddRange(GetLinksCore(s));
                WriteStatics(s);
                superlist.RemoveAt(0);
                //if (superlist.Count - t > 500)
                //{
                //    t = superlist.Count;
                //    SaveBackground(superlist);
                //}
            }
        }
        public MythSpider()
        {
            request = null;
            NameLists = new HashSet<string>();
        }
    }
}


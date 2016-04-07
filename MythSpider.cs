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
        private Queue<string> superlist;
        private int mindex;
        public string GetStatics(string tagName)
        {

            try
            {

                MythRequestHelper requesthelper = new MythRequestHelper("http://index.iqiyi.com/q/?name=" + tagName);
                requesthelper.Connect();
                StreamReader sr = requesthelper.GetStream();
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
                requesthelper.Close();
                return ret;
            }
            catch
            {
                Console.WriteLine("Error on Tag:" + tagName);
                return string.Empty;
            }
        }

        public string[] GetList()
        {
            return superlist.ToArray(); ;
        }

        private string GetTitleCore(string src)
        {
            string ret = string.Empty;
            /*
            const string pattern = @"(?<=title\>).*(?=</title)";
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase); //新建正则模式
            MatchCollection m = r.Matches(src); //获得匹配结果
            foreach(Match match in m)
            {
                ret = match.ToString();
                ret = ret.Replace("<title>", "");
                ret = ret.Replace("</title>", "");
            }
             */
            if (src.Contains("<title>"))
            {
                ret = src.Replace("<title>", "");
                ret = ret.Replace("</title>", "");
            }
            return ret.Trim();
        }
        private bool ReadTitle(StreamReader sr)
        {

            for (; ; )
            {
                string str = sr.ReadLine();
                if (str == null)
                    break;
                else
                {
                    string title = GetTitleCore(str);
                    if (!title.Equals(string.Empty))
                    {
                        WriteStatics(title);
                        return true;
                    }

                }
            }
            return false;
        }
        private void GetLinksCore(string html)
        {
            //List<string> links = new List<string>();
           // if (Global.Contains(html))
            //{
            //    return links;
            //}
            try
            {
                //Global.Add(html);
                MythRequestHelper requesthelper = new MythRequestHelper(html);
                requesthelper.Connect();
                StreamReader sr = requesthelper.GetStream();
                if (sr == null)
                {
                    Console.WriteLine("thread {0}:ReadFailed,{1}" ,mindex,html);
                    return;
                }

                const string pattern = @"http://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*.html)";
                if (ReadTitle(sr))
                {
                    string str = sr.ReadToEnd();
                    Regex r = new Regex(pattern, RegexOptions.IgnoreCase); //新建正则模式
                    MatchCollection m = r.Matches(str); //获得匹配结果
                    foreach (Match match in m)
                    {
                        string s = match.ToString();
                        if (!s.Contains("list.iqiyi"))
                        {
                            if (!Global.Contains(s))
                            {
                                Global.Add(s);
                                superlist.Enqueue(s); //提取出结果
                            }
                        }
                    }
                    requesthelper.Close();
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine("thread {0}:{1},{2}", mindex, ee.Message,html);
            }
            return;
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
        private bool WriteStatics(string title)
        {
            //string title = GetTitleCore(src);
            if (!title.Equals(string.Empty))
            {
                var realtitles = title.Split('-');
                if (realtitles.Length > 0)
                {
                    string realtitle = realtitles[0];
                    var sptitle = Regex.Split(realtitle, "&nbsp");
                    if (sptitle.Length > 0)
                        realtitle = sptitle[0];
                    if (!Global.Contains(realtitle))
                    {
                        Global.Add(realtitle);
                        string ret = GetStatics(realtitle);
                        if (!ret.Equals(string.Empty))
                        {
                            Console.WriteLine("thread {0}:hit,{1}",mindex,realtitle);
                            FileStream fs = new FileStream("out.txt", FileMode.Append);
                            StreamWriter sw = new StreamWriter(fs);
                            sw.WriteLine(ret);
                            sw.Close();
                            fs.Close();
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("thread {0}:Ignore,{1}", mindex, realtitle);
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;

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
            superlist = new Queue<string>(history);
            for (; ; )
            {
                if (superlist.Count == 0)
                    break;
                var s = superlist.Dequeue();
                GetLinksCore(s);
            }
        }
        public MythSpider()
        {
        }
    }
}


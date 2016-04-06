using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
namespace pinkspider
{
    public class MythSpiderPool
    {
        private Timer mtimer;
        private HttpWebRequest request;
        private List<Thread> mthread;
        private List<MythSpider> mspider;
        private int mthreadnum;
        private string[] mstrs;
        private void SingleStep(object args)
        {
            int index = (int)args;
            List<string> tmpstr = new List<string>();
            for (int i = index; i < mstrs.Length; i++)
            {
                if (i % mthreadnum == 0)
                {
                    tmpstr.Add(mstrs[i]);
                }
            }
            mspider[index].StartLoop(tmpstr.ToArray());
        }
        private void timer_Callback(object sender)
        {
            List<string> alllist = new List<string>();
            foreach (MythSpider spider in mspider)
            {
                alllist.AddRange(spider.GetList());
            }
            Console.WriteLine("Saving Background,{0} elements", alllist.Count);
            StreamWriter sw = new StreamWriter("history.log", false, Encoding.UTF8);
            foreach (string s in alllist)
            {
                sw.WriteLine(s);
            }
            sw.Close();
            Console.WriteLine("Saving Background,Done");
        }
        private void MythSpiderPoolCore(string[] history,int threadnum)
        {
            request = null;
            mstrs = history;
            mthreadnum = threadnum;
            mthread = new List<Thread>();
            mspider = new List<MythSpider>();
            mtimer = new Timer(new TimerCallback(timer_Callback), null, 60000, 60000);
            
            for (int i = 0; i < threadnum; i++)
            {
                Thread th = new Thread(new ParameterizedThreadStart(SingleStep));
                MythSpider spider = new MythSpider();
                mthread.Add(th);
                mspider.Add(spider);
            }
        }

        public MythSpiderPool(string url, int threadnum)
        {
            //string[] tmpstr = { url };
            string[] tmpstr = GetLinksCore(url).ToArray();
            MythSpiderPoolCore(tmpstr, threadnum);
        }

        public MythSpiderPool(string[] history, int threadnum)
        {
            MythSpiderPoolCore(history, threadnum);
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
                    Console.WriteLine("ReadFailed:" + html);
                    return links;
                }

                const string pattern = @"http://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?";
                string str = sr.ReadToEnd();
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
                Console.WriteLine(ee.Message);
            }
            return links;
        }

        public void StartLoop()
        {
            if (mthreadnum == 1)
            {
                mspider[0].StartLoop(mstrs);
            }
            else
            {
                //mthread[0].Start(0);
                if (mthreadnum > 1)
                {
                    for (int i = 1; i < mthreadnum; i++)
                    {
                        mthread[i].Start(i);
                    }
                    mthread[0].Start(0);
                }
            }
        }
    }
}

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
        private List<MythSpider> mspider;
        private int mthreadnum;
        private string[] mstrs;
        private void SingleStep(int index)
        {
            List<string> tmpstr = new List<string>();
            for (int i = index; i < mstrs.Length; i++)
            {
                if (i % mthreadnum == 0)
                {
                    tmpstr.Add(mstrs[i]);
                }
            }
            mspider[index].StartLoop(tmpstr.ToArray(), index);
        }

        private void timer_Callback(object sender)
        {
            long allistlen = 0;
            foreach (MythSpider spider in mspider)
                allistlen += spider.len();
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine("Saving Background,{0} elements", allistlen);
            StreamWriter sw = new StreamWriter("history.log", false, Encoding.UTF8);
            foreach (MythSpider spider in mspider)
                foreach (string s in spider.GetList())
                    sw.WriteLine(s);
            sw.Close();
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine("Saving Background,Done");
        }

        private void MythSpiderPoolCore(string[] history, int threadnum)
        {
            mstrs = history;
            mthreadnum = threadnum;
            mspider = new List<MythSpider>();
            mtimer = new Timer(new TimerCallback(timer_Callback), null, 60000, 60000);

            for (int i = 0; i < threadnum; i++)
            {
                MythSpider spider = new MythSpider();
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

        #region fistrequest
        private List<string> GetLinksCore(string html)
        {
            List<string> links = new List<string>();
            try
            {

                MythRequestHelper requesthelper = new MythRequestHelper(html);
                requesthelper.Connect();
                StreamReader sr = requesthelper.GetStream();
                if (sr == null)
                {
                    Console.WriteLine("ReadFailed:" + html);
                    return links;
                }

                const string pattern = @"http://www.iqiyi.com/(.*).html";
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
                            links.Add(s); //提取出结果
                        }
                    }
                }
                requesthelper.Close();
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.Message);
            }
            return links;
        }
        #endregion
        public void StartLoop()
        {
            System.Threading.Tasks.Parallel.For(0, mthreadnum,
                (i) => { SingleStep(i); });
        }
    }
}

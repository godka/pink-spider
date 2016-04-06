using System;
namespace pinkspider
{
	class MainClass
	{
        public static void Main(string[] args)
        {
            MythSpider spider = new MythSpider();
            if (args.Length > 0)
            {
                var strs = System.IO.File.ReadAllLines(args[0], System.Text.Encoding.UTF8);
                spider.StartLoop(strs);
            }
            else
            {
                spider.StartLoop("http://www.iqiyi.com");
            }
            //Console.ReadKey ();
        }

	}
}

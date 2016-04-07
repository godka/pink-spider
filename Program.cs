using System;
namespace pinkspider
{
	class MainClass
	{
        public const int threadnum = 1;
        public static void Main(string[] args)
        {
            MythSpiderPool spider;// = new MythSpiderPool();
            if (args.Length > 0)
            {
                var strs = System.IO.File.ReadAllLines(args[0], System.Text.Encoding.UTF8);
                Console.WriteLine("Read Files success");
                spider = new MythSpiderPool(strs, threadnum);
            }
            else
            {
                spider = new MythSpiderPool("http://www.iqiyi.com", threadnum);
            }
            spider.StartLoop();
        }

	}
}

using System;
namespace pinkspider
{
	class MainClass
	{
        public static void Main(string[] args)
        {
            MythSpiderPool spider;// = new MythSpiderPool();
            if (args.Length > 0)
            {
                var strs = System.IO.File.ReadAllLines(args[0], System.Text.Encoding.UTF8);
                spider = new MythSpiderPool(strs, 3);
            }
            else
            {
                spider = new MythSpiderPool("http://www.iqiyi.com", 3);
            }
            spider.StartLoop();
            //Console.ReadKey ();
        }

	}
}

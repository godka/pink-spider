using System;
namespace pinkspider
{
	class MainClass
	{
        public const int threadnum = 4;
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
                spider = new MythSpiderPool("http://www.iqiyi.com/v_19rrkz8rv8.html", threadnum);
            }
            spider.StartLoop();
        }

	}
}

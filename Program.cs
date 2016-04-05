using System;
namespace pinkspider
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			MythSpider spider = new MythSpider ();
			spider.StartLoop ("http://www.iqiyi.com");
			//Console.ReadKey ();
		}

	}
}

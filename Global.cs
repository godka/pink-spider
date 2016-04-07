using System;
using System.Collections.Generic;
using System.Text;

namespace pinkspider
{
    public static class Global
    {
        private static HashSet<string> BrowserLists = new HashSet<string>();
        //public static HashSet<string> NameLists = new HashSet<string>();
        public static bool Contains(string item)
        {
            return BrowserLists.Contains(item);
        }
        public static void Add(string item)
        {
            BrowserLists.Add(item);
        }
    }
}

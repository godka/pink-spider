using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace pinkspider
{
    public class MythRequestHelper
    {
        StreamReader _stream;
        HttpWebRequest request;
        string _url;
        int _timeout;
        int _times;
        public MythRequestHelper(string url, int timeout = 1000, int reconnect_count = 5)
        {
            _stream = null;
            request = null;
            _timeout = timeout;
            _url = url;
            _times = reconnect_count;
        }
        public void Connect()
        {
            for (int i = 0; i < _times; i++)
            {
                if (_stream != null)
                    break;
                try
                {
                    request = (HttpWebRequest)HttpWebRequest.Create(_url);    //创建一个请求示例
                    request.AllowAutoRedirect = true;
                    request.Timeout = _timeout;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();//获取响应，即发送请求
                    var responseStream = response.GetResponseStream();
                    _stream = new StreamReader(responseStream, Encoding.UTF8);
                    //request.Abort();
                }
                catch
                {
                    _stream = null;
                    //Console.WriteLine ("Reconnecting:" + html);
                }
            }
        }
        public StreamReader GetStream()
        {
            return _stream;
        }
        public void Close()
        {
            _stream.Close();
            request.GetResponse().Close();
            request.Abort();
        }
    }
}

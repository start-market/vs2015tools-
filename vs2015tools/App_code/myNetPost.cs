using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.IO;

class myNetPost
{
    public myNetPost()
    {
    }
    public static string Get(string surl)
    {
        WebClient wc = new WebClient();
        Stream st = wc.OpenRead(surl);
        StreamReader sr = new StreamReader(st, Encoding.GetEncoding("utf-8"));
        string res = sr.ReadToEnd();
        sr.Close();
        st.Close();
        res = System.Web.HttpUtility.UrlDecode(res, Encoding.GetEncoding("utf-8"));
        return res;
    }
    public static string PostFile(string surl, string sfilename)
    {
        WebHeaderCollection PostHeader = new WebHeaderCollection();
        PostHeader["Accept-Encoding"] = " gzip, deflate";
        PostHeader["Cache-Control"] = " no-cache";
        PostHeader["Accept-Language"] = " zh-CN";

        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(surl);
        req.Headers = PostHeader;
        req.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 2.0.50727)";
        req.Method = "POST";
        req.ContentType = "application/octet-stream";

        Encoding encode = Encoding.GetEncoding("utf-8");

        FileStream file = new FileStream(sfilename, FileMode.Open);
        MemoryStream stream = new MemoryStream();
        int b1 = 0;
        while ((b1 = file.ReadByte()) != -1)
        {
            stream.WriteByte(((byte)b1)); 
        }

        byte[] bytes = stream.ToArray();
        //如果你的postData中有中文的话，会出现问题，你需要先把postData转换成byte，然后计算长度。
        req.ContentLength = bytes.Length;
        Stream outStream = req.GetRequestStream();
        outStream.Write(bytes, 0, bytes.Length);
        outStream.Close();
        stream.Close();
        file.Close();
        //接收HTTP做出的响应
        HttpWebResponse rep = (HttpWebResponse)req.GetResponse();

        Stream receiveStream = null;
        receiveStream = rep.GetResponseStream();


        // 建立一个流读取器，可以设置流编码，不设置则默认为UTF-8
        StreamReader reader = new StreamReader(receiveStream, encode);
        string result = reader.ReadToEnd();
        rep.Close();
        reader.Close();
        // 读取流字符串内容
        return result;
    }
}

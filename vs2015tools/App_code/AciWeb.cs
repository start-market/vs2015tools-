using System;
using System.Data;
using System.Collections;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

/// <summary>
/// AciWeb 主要处理 Response 等网页特有的代码
/// </summary>
public class AciWeb
{
    public AciWeb()
	{
        
	}
    ///   <summary>
    ///   返回继承其他URL参数
    ///   </summary>
    ///   <param name="text">不需要继承的参数名 必须使用 | | 隔开 例如：|id|page|</param>
    public static string url_Inherit(string text)
    {
        string stemp = "";
        string sUrl = HttpContext.Current.Request.Url.ToString();
        if (sUrl.IndexOf('?') > -1)
        {
            string ss = sUrl.Split('?')[1];
            ArrayList als = new ArrayList(ss.Split('&'));
            int it = 0;
            for (int i = 0; i < als.Count; i++)
            {
                if (als[i].ToString().IndexOf('=') > -1)
                {
                    if (text.IndexOf("|" + als[i].ToString().Split('=')[0] + "|") < 0)
                    {
                        it++;
                        if (it != 1)
                        {
                            stemp += "&";
                        }
                        stemp += als[i].ToString().Split('=')[0] + "=" + HttpContext.Current.Server.UrlEncode(als[i].ToString().Split('=')[1]);
                    }
                }
            }
        }
        return stemp;
    }
    ///   <summary>
    ///   返回继承其他字符串型URL参数
    ///   </summary>
    ///   <param name="url">字符串型url</param>
    ///   <param name="text">不需要继承的参数名 必须使用 | | 隔开 例如：|id|page|</param>
    public static string url_InheritByStr(string url, string text)
    {
        string stemp = "";
        if (url.IndexOf('?') > -1)
        {
            string ss = url.Split('?')[1];
            ArrayList als = new ArrayList(ss.Split('&'));
            int it = 0;
            for (int i = 0; i < als.Count; i++)
            {
                if (als[i].ToString().IndexOf('=') > -1)
                {
                    if (text.IndexOf("|" + als[i].ToString().Split('=')[0] + "|") < 0)
                    {
                        it++;
                        if (it != 1)
                        {
                            stemp += "&";
                        }
                        stemp += als[i].ToString().Split('=')[0] + "=" + HttpContext.Current.Server.UrlEncode(als[i].ToString().Split('=')[1]);
                    }
                }
            }
        }
        return stemp;
    }
    ///   <summary>
    ///   返回当前页面的名称
    ///   </summary>
    public static string url_getPageName()
    {
        string[] strs = HttpContext.Current.Request.FilePath.Split('/');
        return strs[strs.Length - 1];
    }
    ///   <summary>
    ///   对字符串型URL 使用的 Request
    ///   </summary>
    ///   <param name="url">字符串型url</param>
    ///   <param name="key">用返回值的参数名</param>
    public static string Request_Str(string url, string key)
    {
        string parastr, tempstr = "";
        string[] para;
        int pos;
        pos = url.IndexOf("?");
        parastr = url.Substring(pos + 1);
        para = parastr.Split('&');
        tempstr = "";
        for (int i = 0; i < para.Length; i++)
        {
            tempstr = para[i];
            if (tempstr != "")
            {
                if (tempstr.IndexOf('=') > -1)
                {
                    pos = tempstr.IndexOf("=");
                    if (tempstr.Substring(0, pos) == key)
                    {
                        return tempstr.Substring(pos + 1).Trim();
                    }
                }
            }
        }
        return "";
    }
    ///   <summary>
    ///   对字符串型URL 先 UrlDecode 再使用的 Request
    ///   </summary>
    ///   <param name="url">字符串型url</param>
    ///   <param name="key">用返回值的参数名</param>
    public static string Request_StrDecode(string url, string key)
    {
        url = HttpContext.Current.Server.UrlDecode(url);
        return Request_Str(url, key);
    }
    /// <summary>
    /// 弹出JavaScript小窗口 并刷新当前页
    /// </summary>
    /// <param name="message">窗口信息</param>
    public static void Alert(string message)
    {
        #region
        string js = "<Script language='JavaScript'>alert('" + message + "');";
        js += "location.href='" + HttpContext.Current.Request.Url.ToString() + "';</Script>";
        HttpContext.Current.Response.Write(js);
        HttpContext.Current.Response.End();
        #endregion
    }
    /// <summary>
    /// 弹出JavaScript小窗口
    /// </summary>
    /// <param name="message">窗口信息</param>
    /// <param name="value">是否返回历史记录 -1/1</param>
    public static void Alert(string message, int value)
    {
        #region
        string js = "<Script language='JavaScript'>alert('" + message + "');";
        js += "history.go(" + value + ");</Script>";
        HttpContext.Current.Response.Write(js);
        HttpContext.Current.Response.End();
        #endregion
    }
    /// <summary>
    /// 弹出JavaScript小窗口
    /// </summary>
    /// <param name="message">窗口信息</param>
    /// <param name="url">要跳到的页面地址</param>
    public static void Alert(string message, string url)
    {
        #region
        string js = "<Script language='JavaScript'>alert('" + message + "');";
        js += "location.href='" + url + "';</Script>";
        HttpContext.Current.Response.Write(js);
        HttpContext.Current.Response.End();
        #endregion
    }
    /// <summary>
    /// 弹出JavaScript小窗口
    /// </summary>
    /// <param name="message">窗口信息</param>
    /// <param name="url">要跳到的页面地址</param>
    public static void AlertTop(string message, string url)
    {
        #region
        string js = "<Script language='JavaScript'>alert('" + message + "');";
        js += "top.location.href='" + url + "';</Script>";
        HttpContext.Current.Response.Write(js);
        HttpContext.Current.Response.End();
        #endregion
    }

    /// <summary>
    /// 返回虚拟路径
    /// </summary>
    /// <param name="spath">路径</param>
    public static string getMapPath(string path)
    {
        return HttpContext.Current.Server.MapPath(path);
    }


    ///   <summary>
    ///   将URL地址编码
    ///   </summary>
    public static string myEncodeUrl(string text)
    {
        string sreturn = text;
        sreturn = sreturn.Replace("?", "~*@");
        sreturn = sreturn.Replace("&", "~@@");
        sreturn = sreturn.Replace("%", "~5@");
        sreturn = sreturn.Replace("=", "~9@");
        sreturn = sreturn.Replace("+", "~0@");
        sreturn = sreturn.Replace("/", "~7@");
        sreturn = sreturn.Replace("\\", "~6@");
        sreturn = sreturn.Replace("\"", "~4@");
        sreturn = sreturn.Replace("<", "~8@");
        sreturn = sreturn.Replace(">", "~3@");
        sreturn = sreturn.Replace("#", "~2@");
        sreturn = sreturn.Replace("'", "~-@");
        return sreturn;
    }
    ///   <summary>
    ///   将URL地址解码
    ///   </summary>
    public static string myDecodeUrl(string text)
    {
        string sreturn = text;
        sreturn = sreturn.Replace("~*@", "?");
        sreturn = sreturn.Replace("~@@", "&");
        sreturn = sreturn.Replace("~5@", "%");
        sreturn = sreturn.Replace("~9@", "=");
        sreturn = sreturn.Replace("~0@", "+");
        sreturn = sreturn.Replace("~7@", "/");
        sreturn = sreturn.Replace("~6@", "\\");
        sreturn = sreturn.Replace("~4@", "\"");
        sreturn = sreturn.Replace("~8@", "<");
        sreturn = sreturn.Replace("~3@", ">");
        sreturn = sreturn.Replace("~2@", "#");
        sreturn = sreturn.Replace("~-@", "'");
        return sreturn;
    }



    /// <summary>
    /// 路径转换
    /// </summary>
    /// <param name="spath">不论是不是虚拟路径都可以执行</param>
    public static string MapPath(string spath)
    {
        if (spath.IndexOf(':') > -1)
        {
            return spath;
        }
        else
        {
            return HttpContext.Current.Server.MapPath(spath);
        }
    }
}

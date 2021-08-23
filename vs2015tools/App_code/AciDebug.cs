using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

class AciDebug
{
    public AciDebug()
    {
        //
        // TODO: 在此处添加构造函数逻辑
        //
    }
    /// <summary>
    /// 输出调试日志
    /// </summary>
    /// <param name="svalue">输出的内容</param>
    public static void Debug(string svalue)
    {
        Debug(svalue, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    }
    /// <summary>
    /// 输出调试日志
    /// </summary>
    /// <param name="svalue">输出的内容</param>
    public static void Debug(string svalue, string stime)
    {
        System.IO.StreamWriter wr = new System.IO.StreamWriter(Application.StartupPath + "/debug.txt", true, Encoding.UTF8);
        wr.Write(stime + " " + svalue + "\r\n");
        wr.Close();
    }

    /// <summary>
    /// 输出调试日志
    /// </summary>
    /// <param name="svalue">输出的内容</param>
    public static void Worry(string svalue)
    {
        Worry(svalue, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    }
    /// <summary>
    /// 输出调试日志
    /// </summary>
    /// <param name="svalue">输出的内容</param>
    public static void Worry(string svalue, string stime)
    {
        System.IO.StreamWriter wr = new System.IO.StreamWriter(Application.StartupPath + "/worry.txt", true, Encoding.UTF8);
        wr.Write(stime + " " + svalue + "\r\n");
        wr.Close();
    }
}

using System;
using System.Collections;
using System.Security.Cryptography;
using System.Web.Security;
using System.IO;
using System.Text;
using System.Data;

/// <summary>
/// AciSf 主要用于安全加密方面
/// </summary>
public class AciSf
{
	public AciSf()
	{
		//
		// TODO: 在此处添加构造函数逻辑
		//
	}
    ///   <summary>
    ///   替换非法的字符串
    ///   </summary>
    ///   <param name="str">需要去除的字符串</param>
    public static string ReplaceStr(string str)
    {
        if (str != null)
        {
            str = str.Trim();
            str = str.Replace("'", "''");
            str = str.Replace("&lt;", "");
            str = str.Replace("&gt;", "");
            return str;
        }
        else
        {
            return "";
        }
    }
    ///   <summary>
    ///   替换非法的字符串
    ///   </summary>
    ///   <param name="str">需要去除的字符串</param>
    ///   <param name="space">是否替换文字中的空格 true 为替换 false为不替换</param>
    public static string ReplaceStr(string str, bool space)
    {
        str = str.Trim();
        str = str.Replace("'", "''");
        if (space)
        {
            str = str.Replace(" ", "");
        }
        str = str.Replace("&lt;", "");
        str = str.Replace("&gt;", "");
        return str;
    }
    ///   <summary>
    ///   替换非法的字符串
    ///   </summary>
    ///   <param name="str">需要去除的字符串</param>
    public static string ReplaceMy(string str)
    {
        return ReplaceStr(str);
    }
    ///   <summary>
    ///   替换非法的字符串,在sql中为数字型变量 错误 100 为参数是 null  错误 101 为参数是 不是数字  错误 102 为参数是 ""
    ///   </summary>
    ///   <param name="str">需要去除的字符串</param>
    public static string Test_Int(string str)
    {
        try
        {
            System.Convert.ToInt32(str);
        }
        catch
        {
            AciWeb.Alert("发生参数错误");
        }
        return str;
    }

    ///   <summary>
    ///   ASP.NET加密方式
    ///   </summary>
    ///   <param name="str">需要加密的字符串</param>
    ///   <param name="format">用什么方式加密，有两种方式 SHA1 和 MD5</param>
    public static string Pwd_Format(string str, string format)
    {
        string returnstr = "";
        if (format == "SHA1") returnstr = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(str, "SHA1");
        if (format == "MD5") returnstr = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(str, "MD5");
        return returnstr;
    }
    /// <summary>
    /// 将try出来message转换格式去除 无法输出的符号
    /// </summary>
    /// <param name="message">try出来message</param>
    public static string try_EncodeMessage(string message)
    {
        return message.Replace("'", "").Replace("\"", "").Replace("\\", "\\\\").Replace("\r\n", "");
    }
    ///   <summary>
    ///   加密字符串
    ///   </summary>
    ///   <param name="text">需要加密的字符串</param>
    ///   <param name="pass">加密密码 必须是6位数字</param>
    ///   <param name="imode">是否需要当URL传递而转换字符串, 0为不需要 1为base64 2为需要用16位数字</param>
    public static string EnCode(string text, string pass, int imode)
    {
        byte[] bt = (new System.Text.UnicodeEncoding()).GetBytes(text);
        PasswordDeriveBytes pdb = new PasswordDeriveBytes(pass, null);
        byte[] key = pdb.GetBytes(24);
        byte[] iv = pdb.GetBytes(8);
        MemoryStream ms = new MemoryStream();
        TripleDESCryptoServiceProvider tdesc = new TripleDESCryptoServiceProvider();
        CryptoStream cs = new CryptoStream(ms, tdesc.CreateEncryptor(key, iv), CryptoStreamMode.Write);
        cs.Write(bt, 0, bt.Length);
        cs.FlushFinalBlock();

        if (imode == 0)
        {
            return System.Convert.ToBase64String(ms.ToArray());
        }
        else if (imode == 1)
        {
            return AciWeb.myEncodeUrl(System.Convert.ToBase64String(ms.ToArray()));
        }
        else
        {
            return toHex(ms.ToArray());
        }
    }
    ///   <summary>
    ///   解密字符串
    ///   </summary>
    ///   <param name="text">需要解密的字符串</param>
    ///   <param name="pass">解密密码 必须是6位数字</param>
    ///   <param name="imode">是否需要当URL传递而转换字符串, 0为不需要  1为base64  2为需要用16位数字</param>
    public static string DeCode(string text, string pass, int imode)
    {
        try
        {
            if (imode == 1)
            {
                //text = System.Web.HttpContext.Current.Server.UrlDecode(text);
                text = AciWeb.myDecodeUrl(text);
            }
            byte[] bt = null;
            if (imode != 0 && imode != 1)
            {
                bt = toBytesByHex(text);
            }
            else
            {
                bt = System.Convert.FromBase64String(text);
            }
           
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(pass, null);
            byte[] key = pdb.GetBytes(24);
            byte[] iv = pdb.GetBytes(8);
            MemoryStream ms = new MemoryStream();
            TripleDESCryptoServiceProvider tdesc = new TripleDESCryptoServiceProvider();
            CryptoStream cs = new CryptoStream(ms, tdesc.CreateDecryptor(key, iv), CryptoStreamMode.Write);
            cs.Write(bt, 0, bt.Length);
            cs.FlushFinalBlock();
            return (new System.Text.UnicodeEncoding()).GetString(ms.ToArray());
        }
        catch (Exception ex)
        {
            return "";
        }
    }


    /// <summary>
    /// DES加密
    /// </summary>
    /// <param name="str">要加密字符串</param>
    /// <returns>返回加密后字符串</returns>
    public static string EnCode_DES(String str, string pass)
    {
        DES des = new DESCryptoServiceProvider();
        des.Key = Encoding.UTF8.GetBytes(pass);
        des.IV = Encoding.UTF8.GetBytes(pass);

        byte[] bytes = Encoding.UTF8.GetBytes(str);
        byte[] resultBytes = des.CreateEncryptor().TransformFinalBlock(bytes, 0, bytes.Length);

        return toHex(resultBytes);
    }
    /// <summary>
    /// DAES加密
    /// </summary>
    /// <param name="str">要加密字符串</param>
    /// <returns>返回加密后字符串</returns>
    public static string DeCode_DES(string str, string pass)
    {
        DES des = new DESCryptoServiceProvider();
        des.Key = Encoding.UTF8.GetBytes(pass);
        des.IV = Encoding.UTF8.GetBytes(pass);

        byte[] bytes = toBytesByHex(str);
        byte[] resultBytes = des.CreateDecryptor().TransformFinalBlock(bytes, 0, bytes.Length);

        return System.Text.UTF8Encoding.UTF8.GetString(resultBytes);
    }
    /// <summary>
    /// 将 bytes 转成 16 进制 字符串
    /// </summary>
    /// <param name="bytes">要转的 bytes</param>
    /// <returns></returns>
    public static string toHex(byte[] bytes)
    {
        string HEX = "0123456789ABCDEF";
        string s = "";
        for (int i = 0; i < bytes.Length; i++)
        {
            byte b = bytes[i];
            s += HEX[(b >> 4) & 0x0f];
            s += HEX[b & 0x0f];
        }
        return s;
    }
    /// <summary>
    /// 将 16 进制 字符串 转成 bytes
    /// </summary>
    /// <param name="hexString">要转的 16 进制 字符串</param>
    /// <returns></returns>
    public static byte[] toBytesByHex(string hexString)
    {
        int len = hexString.Length / 2;
        byte[] result = new byte[len];
        for (int i = 0; i < len; i++)
        {
            result[i] = System.Convert.ToByte(hexString.Substring(2 * i, 2), 16);
        }
        return result;
    }

    /// <summary>
    /// 将 ArrayList 返回成 <> 符号隔开的字符串 
    /// </summary>
    /// <param name="arrstr">要转换的 string[]</param>
    public static string toPostRow(string[] arrstr)
    {
        string s = "";
        for (int i = 0; i < arrstr.Length; i++)
        {
            if (s != "")
            {
                s += "<c>";
            }
            if (arrstr[i] == "")
            {
                s += "_";
            }
            else
            {
                s += arrstr[i].Replace("<c>", "&lt;c&gt;");
            }
        }
        return s;
    }
    /// <summary>
    /// 将 DataTable 转换为 PostString  用 <tablehead> 隔开头与数据
    /// </summary>
    /// <param name="dt">要转换的 DataTable</param>
    public static string toPostTableString(DataTable dt)
    {
        return toPostTableHeadString(dt) + "<tablehead>" + toPostTableValueString(dt).Replace("<tablehead>", "&lt;tablehead&gt;");
    }
    /// <summary>
    /// 将 DataTable 转换为 PostString 仅数据结构
    /// </summary>
    /// <param name="dt">要转换的 DataTable</param>
    public static string toPostTableHeadString(DataTable dt)
    {
        string shead = "";
        for (int i = 0; i < dt.Columns.Count; i++)
        {
            if (shead != "")
            {
                shead += "<h>";
            }
            shead += dt.Columns[i].ColumnName.ToLower();
            if (AciCvt.toSampleType(dt.Columns[i].DataType) == 0)
            {
                shead += ",0";
            }
        }
        return dt.TableName + "<table>" + shead;
    }
    /// <summary>
    /// 将 DataTable 转换为 PostString 仅数据结构
    /// </summary>
    /// <param name="dt">要转换的 DataTable</param>
    public static string toPostTableValueString(DataTable dt)
    {
        string svalue = "";
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            DataRow dr = dt.Rows[i];
            if (svalue != "")
            {
                svalue += "<r>";
            }

            string sr = "";
            for (int j = 0; j < dt.Columns.Count; j++)
            {
                if (sr != "")
                {
                    sr += "<c>";
                }
                if (dr[j].ToString().Trim() == "")
                {
                    sr += "_";
                }
                else
                {
                    sr += dr[j].ToString().Replace("<c>", "&lt;c&gt;");
                }
            }
            svalue += sr;
        }
        return svalue;
    }
}
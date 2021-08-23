using System;
using System.Collections.Generic;
using System.Text;

public class AciMath
{
    /// <summary>
    /// 数字补 0 程序
    /// </summary>
    /// <param name="it">数字</param>
    /// <param name="iwei">需要保持的位数</param>
    /// <returns></returns>
    public static string setNumberLength(int it, int iwei)
    {
        string st = it.ToString();
        int iw = st.Length;
        if (iw < iwei)
        {
            for (int i = 0; i < iwei - iw; i++)
            {
                st = "0" + st;
            }
        }

        return st;
    }
    /// <summary>
    /// 数字补 0 程序
    /// </summary>
    /// <param name="it">数字</param>
    /// <param name="iwei">需要保持的位数</param>
    /// <returns></returns>
    public static string setNumberLength(string it, int iwei)
    {
        return setNumberLength(ToInt(it), iwei);
    }
    /// <summary>
    /// 数字补 0 程序
    /// </summary>
    /// <param name="it">数字</param>
    /// <param name="iwei">需要保持的位数</param>
    /// <returns></returns>
    public static string setNumberLength(object it, int iwei)
    {
        return setNumberLength(ToInt(it), iwei);
    }
    ///   <summary>
    ///   返回数字类型，如果转换的为非数字就返回 0 
    ///   </summary>
    ///   <param name="str">需要去转数字的字符串</param>
    public static int ToInt(string str)
    {
        int it = 0;
        try
        {
            it = Convert.ToInt32(str);
        }
        catch
        {

        }
        return it;
    }
    ///   <summary>
    ///   返回数字类型，如果转换的为非数字就返回 0 
    ///   </summary>
    ///   <param name="str">需要去转数字的字符串</param>
    public static int ToInt(object str)
    {
        int it = 0;
        try
        {
            it = Convert.ToInt32(str);
        }
        catch
        {

        }
        return it;
    }
}

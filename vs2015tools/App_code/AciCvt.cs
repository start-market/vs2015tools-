using System;
using System.Collections;
using System.IO;
using System.Text;

/// <summary>
/// AciCvt 的摘要说明
/// </summary>
public class AciCvt
{
    ///   <summary>
    ///   用于各种数据转换
    ///   </summary>
    public AciCvt()
    {
        //
        // TODO: 在此处添加构造函数逻辑
        //
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
    public static short ToInt16(string str)
    {
        short it = 0;
        try
        {
            it = Convert.ToInt16(str);
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
    public static long ToInt64(string str)
    {
        long it = 0;
        try
        {
            it = Convert.ToInt64(str);
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
    ///   <summary>
    ///   返回数字类型，如果转换的为非数字就返回 0 
    ///   </summary>
    ///   <param name="str">需要去转数字的字符串</param>
    public static short ToInt16(object str)
    {
        short it = 0;
        try
        {
            it = Convert.ToInt16(str);
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
    public static long ToInt64(object str)
    {
        long it = 0;
        try
        {
            it = Convert.ToInt64(str);
        }
        catch
        {

        }
        return it;
    }
    ///   <summary>
    ///   返回Double类型，如果转换的为非数字就返回 0 
    ///   </summary>
    ///   <param name="str">需要去转数字的字符串</param>
    public static double ToDouble(string str)
    {
        double it = 0;
        try
        {
            it = Convert.ToDouble(str);
        }
        catch
        {

        }
        return it;
    }
    ///   <summary>
    ///   返回Double类型，如果转换的为非数字就返回 0 
    ///   </summary>
    ///   <param name="str">需要去转数字的字符串</param>
    public static double ToDouble(object str)
    {
        double it = 0;
        try
        {
            it = Convert.ToDouble(str);
        }
        catch
        {

        }
        return it;
    }
    ///   <summary>
    ///   返回float类型，如果转换的为非数字就返回 0 
    ///   </summary>
    ///   <param name="str">需要去转数字的字符串</param>
    public static float toFloat(object str)
    {
        float it = 0;
        try
        {
            it = Convert.ToSingle(str);
        }
        catch
        {

        }
        return it;
    }
    ///   <summary>
    ///   返回 DateTime 类型的 object，如果转换失败返回 null
    ///   </summary>
    ///   <param name="str">需要转换的字符串</param>
    public static object ToDateTime(string str)
    {
        try
        {
            return Convert.ToDateTime(str);
        }
        catch
        {
            return null;
        }
    }
    /// <summary>
    /// 将 ArrayList 返回成指定符号隔开的字符串
    /// </summary>
    /// <param name="al">要转换的 ArrayList</param>
    /// <param name="scode">用来隔开的字符串</param>
    public static string Array_to_Str(ArrayList al, string scode)
    {
        string s = "";
        for (int i = 0; i < al.Count; i++)
        {
            s += al[i];
            if (i != al.Count - 1)
            {
                s += scode;
            }
        }
        return s;
    }
    /// <summary>
    /// 将 ArrayList 返回成指定符号隔开的字符串
    /// </summary>
    /// <param name="arrstr">要转换的 string[]</param>
    /// <param name="scode">用来隔开的字符串 当为string 时 返回 'xx','xx'</param>
    public static string Array_to_Str(string[] arrstr, string scode)
    {
        string s = "";
        for (int i = 0; i < arrstr.Length; i++)
        {
            if (scode == "string")
            {
                s += "'" + arrstr[i] + "'";
            }
            else
            {
                s += arrstr[i];
            }
            if (i != arrstr.Length - 1)
            {
                if (scode == "string")
                {
                    s += ",";
                }
                else
                {
                    s += scode;
                }
            }
        }
        return s;
    }

    /// <summary>
    /// 将 ArrayList 返回成指定符号隔开的字符串
    /// </summary>
    /// <param name="al">要转换的 ArrayList</param>
    /// <param name="scode">用来隔开的字符串</param>
    public static string Array_to_Str(int[] arrstr, string scode)
    {
        string s = "";
        for (int i = 0; i < arrstr.Length; i++)
        {
            if (scode == "string")
            {
                s += "'" + arrstr[i] + "'";
            }
            else
            {
                s += arrstr[i];
            }
            if (i != arrstr.Length - 1)
            {
                if (scode == "string")
                {
                    s += ",";
                }
                else
                {
                    s += scode;
                }
            }
        }
        return s;
    }
    /// <summary>
    /// 将 ArrayList 返回成指定符号隔开的字符串
    /// </summary>
    /// <param name="al">要转换的 ArrayList</param>
    /// <param name="scode">用来隔开的字符串</param>
    public static string Array_to_Str(double[] arrstr, string scode)
    {
        string s = "";
        for (int i = 0; i < arrstr.Length; i++)
        {
            if (scode == "string")
            {
                s += "'" + arrstr[i] + "'";
            }
            else
            {
                s += arrstr[i];
            }
            if (i != arrstr.Length - 1)
            {
                if (scode == "string")
                {
                    s += ",";
                }
                else
                {
                    s += scode;
                }
            }
        }
        return s;
    }
    /// <summary>
    /// 将 string[] 转换成 int[]
    /// </summary>
    /// <param name="al">要转换的 ArrayList</param>
    public static int[] toIntArray(string[] sarr)
    {
        int[] it = null;
        if (sarr != null)
        {
            it = new int[sarr.Length];
            for (int i = 0; i < sarr.Length; i++)
            {
                it[i] = ToInt(sarr[i]);
            }
        }
        return it;
    }
    /// <summary>
    /// 取 int[] 中的最大值 无返回 0
    /// </summary>
    /// <param name="it">要转换的 int[]</param>
    public static int toIntArrayMax(int[] it)
    {
        int imax = 0;
        if (it != null)
        {
            if (it.Length > 0)
            {
                imax = it[0];
                for (int i = 1; i < it.Length; i++)
                {
                    if (it[i] > imax)
                    {
                        imax = it[i];
                    }
                }
            }
        }
        return imax;
    }

    /// <summary>
    /// 二维 转 一维 int[it][] 转 int[]
    /// </summary>
    /// <param name="it">要转换的 int[,]</param>
    /// <param name="iw">维数</param>
    public static int[] toIntArray(int[,] it, int iw)
    {
        int[] inew = null;
        if (it != null)
        {
            inew = new int[it.GetLength(1)];
            for (int i = 0; i < it.GetLength(1); i++)
            {
                inew[i] = it[iw, i];
            }
        }
        return inew;
    }

    /// <summary>
    /// 转换成标准的日期格式 2011-2-22 "yyyy-MM-dd"
    /// </summary>
    /// <param name="str">要转换的 str</param>
    public static string toTimeString(string str)
    {
        string s = "";
        try
        {
            if (str != "")
            {
                DateTime dt = Convert.ToDateTime(str);
                s = dt.ToString("yyyy-MM-dd");
                if (s == "1899-12-30" || s == "1900-1-1")
                {
                    s = "";
                }
            }
        }
        catch
        {

        }
        return s;
    }
    /// <summary>
    /// 从一个数组中随机取出  n 个数据 组成新的数组
    /// </summary>
    /// <param name="arr">要转换的 str</param>
    /// <param name="num">数量</param>
    public static ArrayList getRamArr(string[] arr, int num)
    {
        string[] arrOld = new string[arr.Length];

        for (int i = 0; i < arr.Length; i++)
        {
            arrOld[i] = arr[i];
        }

        ArrayList al = new ArrayList();

        Random random = new Random();
        for (int i = 0; i < num; i++)
        {
            //判断如果列表还有可以取出的项,以防下标越界
            if (arrOld.Length > 0)
            {
                //在列表中产生一个随机索引
                int arrIndex = random.Next(0, arrOld.Length);
                //将此随机索引的对应的列表元素值复制出来
                al.Add(arrOld[arrIndex]);
                //然后删掉此索引的列表项
                ArrayRemoveAt(ref arrOld, arrIndex);
            }
            else
            {
                break;
            }
        }

        return al;
    }
    /// <summary>
    /// 从一个数组中删除一个元素
    /// </summary>
    /// <param name="arr">要转换的 str</param>
    /// <param name="index">数量</param>
    public static void ArrayRemoveAt(ref string[] arr, int index)
    {
        if (index >= 0)
        {
            string[] arrNew = new string[arr.Length - 1];
            for (int i = 0; i < arr.Length; i++)
            {
                if (i < index)
                {
                    arrNew[i] = arr[i];
                }
                else if (i > index)
                {
                    arrNew[i - 1] = arr[i];
                }
            }
            arr = arrNew;
        }
    }
    /// <summary>
    /// 将 string[] 中的每一个字符都 小写化 需要传入 ref
    /// </summary>
    /// <param name="strs">要转换的 string[] </param>
    public static void toLower(ref string[] strs)
    {
        for (int i = 0; i < strs.Length; i++)
        {
            strs[i] = strs[i].ToLower();
        }
    }

    /// <summary> 
    /// 将 Stream 转成 byte[] 
    /// </summary>
    /// <param name="strs">要转换的 Stream </param>
    public byte[] StreamToBytes(Stream stream)
    {
        byte[] bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);

        // 设置当前流的位置为流的开始 
        stream.Seek(0, SeekOrigin.Begin);
        return bytes;
    }

    /// <summary> 
    /// 将 byte[] 转成 Stream 
    /// </summary>
    /// <param name="strs">要转换的 bytes </param>
    public static Stream BytesToStream(byte[] bytes)
    {
        Stream stream = new MemoryStream(bytes);
        return stream;
    }

    /// <summary>
    /// 转换成简易模式 的 0 数字  1 字符 
    /// </summary>
    /// <param name="type">要转换的 Type</param>
    public static int toSampleType(Type type)
    {
        int i = 0;
        switch (type.Name.ToLower())
        {
            case "char":
                i = 1;
                break;
            case "datetime":
                i = 1;
                break;
            case "decimal":
                break;
            case "double":
                break;
            case "int16":
                break;
            case "int32":
                break;
            case "int64":
                break;
            case "sbyte":
                i = 1;
                break;
            case "single":
                break;
            case "string":
                i = 1;
                break;
            case "uint16":
                break;
            case "timespan":
                i = 1;
                break;
            case "uint32":
                break;
            case "uint64":
                break;
        }
        return i;
    }

    /// <summary>
    /// 将 ArrayList 返回 string[]
    /// </summary>
    /// <param name="al">要转换的 ArrayList</param>
    public static string[] ArrayList_to_StringArr(ArrayList al)
    {
        string[] str = new string[al.Count];
        for (int i = 0; i < al.Count; i++)
        {
            str[i] = al[i].ToString();
        }
        return str;
    }

    /// <summary>
    /// 测试字符串是否为日期 如果是返回 yyyy-MM-dd 的短日期格式 不是日期返回 ""
    /// </summary>
    /// <param name="str">要转换的 str</param>
    public static string tryToTimeShort(string str)
    {
        string s = "";
        try
        {
            DateTime dt = Convert.ToDateTime(str);
            s = dt.ToString("yyyy-MM-dd");
        }
        catch
        {

        }
        return s;
    }
    /// <summary>
    /// 测试字符串是否为日期 如果是返回 yyyy-MM-dd 的短日期格式 不是日期返回 str 字符串本身
    /// </summary>
    /// <param name="str">要转换的 str</param>
    public static string tryTimeShort(string str)
    {
        string s = str;
        try
        {
            DateTime dt = Convert.ToDateTime(str);
            s = dt.ToString("yyyy-MM-dd");
        }
        catch
        {

        }
        return s;
    }
    /// <summary>
    /// 测试字符串是否为日期 如果是返回 yyyy-MM-dd yyyy-MM-dd HH:mm:ss 的短日期格式 不是日期返回 "" 字符串
    /// </summary>
    /// <param name="str">要转换的 str</param>
    public static string tryTimeFull(string str)
    {
        string s = str;
        try
        {
            DateTime dt = Convert.ToDateTime(str);
            s = dt.ToString("yyyy-MM-dd HH:mm:ss");
        }
        catch
        {

        }
        return s;
    }
    /// <summary>
    /// 在 "名,名" "值,值" 键值对 数组中 取值
    /// </summary>
    /// <param name="arrName">"名,名,名"</param>
    /// <param name="arrValue">"值,值,值"</param>
    /// <param name="value">值</param>
    /// <param name="scode">分隔符 通常为 ,</param>
    /// <returns>返回值</returns>
    public static string getArrayValue(string sName, string sValue, string value, string scode)
    {
        string s = "";
        string[] arrName = sName.Split(new string[] { scode }, StringSplitOptions.RemoveEmptyEntries);
        string[] arrValue = sValue.Split(new string[] { scode }, StringSplitOptions.RemoveEmptyEntries);

        int it = Array.IndexOf<string>(arrValue, value);
        if (it > -1)
        {
            s = arrName[it];
        }

        return s;
    }
    /// <summary>
    /// 剔除数组中的重复字符串项目
    /// </summary>
    /// <param name="strs">要剔除的数组</param>
    /// <returns></returns>
    public static ArrayList replaceRepeat(string[] strs)
    {
        ArrayList al = new ArrayList();
        for (int i = 0; i < strs.Length; i++)
        {
            if (al.IndexOf(strs[i]) < 0)
            {
                al.Add(strs[i]);
            }
        }
        return al;
    }
    /// <summary>
    /// 剔除数组中的重复字符串项目
    /// </summary>
    /// <param name="strs">要剔除的数组</param>
    /// <returns></returns>
    public static ArrayList replaceRepeat(ArrayList strs)
    {
        ArrayList al = new ArrayList();
        for (int i = 0; i < strs.Count; i++)
        {
            if (al.IndexOf(strs[i]) < 0)
            {
                al.Add(strs[i]);
            }
        }
        return al;
    }
    /// <summary>
    /// 将 Stream 转成 byte[]
    /// </summary>
    /// <param name="stream">要转换的流</param>
    /// <returns></returns>
    public static byte[] ToBytes(Stream stream)
    {
        byte[] bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);
        // 设置当前流的位置为流的开始
        stream.Seek(0, SeekOrigin.Begin);
        return bytes;
    }


}

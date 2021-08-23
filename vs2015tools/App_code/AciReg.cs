using System;
using System.Collections;
using System.Text.RegularExpressions;

/// <summary>
/// AciReg 主要用于验证
/// </summary>
public class AciReg
{
	public AciReg()
	{
		//
		// TODO: 在此处添加构造函数逻辑
		//
	}
    ///   <summary>
    ///   正则表达验证是否正确 正确返回true  错误返回false
    ///   </summary>
    ///   <param name="text">需要验证的字符</param>
    ///   <param name="sReg">正则表达式</param>
    public static bool Reg_Str(string text, string sReg)
    {
        Regex reg = new Regex(sReg);
        return reg.IsMatch(text);
    }
    ///   <summary>
    ///   判断是否是 整数
    ///   </summary>
    ///   <param name="str">需要去转数字的字符串</param>
    public static bool isInteger(string str)
    {
        return Reg_Str(str, "^-?\\d+$");
    }
    ///用正则表达式 提取HTML代码中文字的C#函数     
    ///   <summary>   
    ///   去除HTML标记   
    ///   </summary>   
    ///   <param name="strHtml">包括HTML的源码</param>   
    ///   <returns>已经去除后的文字</returns>
    public static string cuteHTML(string strHtml)
    {
        //删除脚本 
        strHtml = strHtml.Replace("\r\n", "");
        strHtml = Regex.Replace(strHtml, @"<script.*?</script>", "", RegexOptions.IgnoreCase);
        strHtml = Regex.Replace(strHtml, @"<style.*?</style>", "", RegexOptions.IgnoreCase);
        strHtml = Regex.Replace(strHtml, @"<.*?>", "", RegexOptions.IgnoreCase);
        //删除HTML 
        strHtml = Regex.Replace(strHtml, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase);
        strHtml = Regex.Replace(strHtml, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);
        strHtml = Regex.Replace(strHtml, @"-->", "", RegexOptions.IgnoreCase);
        strHtml = Regex.Replace(strHtml, @"<!--.*", "", RegexOptions.IgnoreCase);
        strHtml = Regex.Replace(strHtml, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);
        strHtml = Regex.Replace(strHtml, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);
        strHtml = Regex.Replace(strHtml, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);
        strHtml = Regex.Replace(strHtml, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);
        strHtml = Regex.Replace(strHtml, @"&(nbsp|#160);", "", RegexOptions.IgnoreCase);
        strHtml = Regex.Replace(strHtml, @"&(iexcl|#161);", "\xa1", RegexOptions.IgnoreCase);
        strHtml = Regex.Replace(strHtml, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);
        strHtml = Regex.Replace(strHtml, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);
        strHtml = Regex.Replace(strHtml, @"&(copy|#169);", "\xa9", RegexOptions.IgnoreCase);
        strHtml = Regex.Replace(strHtml, @"&#(\d+);", "", RegexOptions.IgnoreCase);
        strHtml = strHtml.Replace("<", "");
        strHtml = strHtml.Replace(">", "");
        strHtml = strHtml.Replace("\r\n", "");
        return strHtml;
    }
    /// <summary>
    /// 验证是否是日期
    /// </summary>
    /// <param name="strDate"></param>
    /// <returns></returns>
    public static bool IsValidDate(string strDate)
    {
        String REGEXP_ISVALIDDATE = @"^((\d{2}(([02468][048])|([13579][26]))[\-\/\s]?((((0?[13578])|(1[02]))[\-\/\s]?((0?[1-9])|([1-2][0-9])|(3[01])))|(((0?[469])|(11))[\-\/\s]?((0?[1-9])|([1-2][0-9])|(30)))|(0?2[\-\/\s]?((0?[1-9])|([1-2][0-9])))))|(\d{2}(([02468][1235679])|([13579][01345789]))[\-\/\s]?((((0?[13578])|(1[02]))[\-\/\s]?((0?[1-9])|([1-2][0-9])|(3[01])))|(((0?[469])|(11))[\-\/\s]?((0?[1-9])|([1-2][0-9])|(30)))|(0?2[\-\/\s]?((0?[1-9])|(1[0-9])|(2[0-8]))))))(\s(((0?[0-9])|([1-2][0-3]))\:([0-5]?[0-9])((\s)|(\:([0-5]?[0-9])))))?$";
        return (new Regex(REGEXP_ISVALIDDATE)).IsMatch(strDate);
    }

}

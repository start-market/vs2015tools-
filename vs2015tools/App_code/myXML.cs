using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml;
using System.IO;
using System.Text;

/// <summary>
/// myXML 的摘要说明
/// </summary>
public class myXML
{
    /// <summary>根节点</summary>
    public XmlNode xnRoot = null;
    /// <summary>xml文档</summary>
    public XmlDocument xml = null;
    public XmlReaderSettings xmlSetting;
    /// <summary>xml文档字符串格式</summary>
    public string xmlString = "";

    /// <summary>
    /// myXML 的摘要说明
    /// </summary>
    /// <param name="xmlurl">xml 文件的地址  也可以接受 .htm 格式的 xml文件</param>
	public myXML(string xmlurl)
	{
        //try
        //{
            xml = new XmlDocument();
            xmlSetting = new XmlReaderSettings();
            xmlSetting.IgnoreComments = true;
            xmlSetting.IgnoreWhitespace = true;
            XmlReader xnRd = null;

            if (Path.GetExtension(xmlurl).ToLower() == ".htm")
            {
                StreamReader sr = new StreamReader(HttpContext.Current.Server.MapPath(xmlurl));
                xmlString = sr.ReadToEnd();
                xmlString = setXmlCode(xmlString);

                xnRd = XmlReader.Create(new MemoryStream(Encoding.UTF8.GetBytes(xmlString)), xmlSetting);
                xml.Load(xnRd);
                sr.Close();
                sr.Dispose();
            }
            else
            {
                xnRd = XmlReader.Create(xmlurl, xmlSetting);
                xml.Load(xnRd);
            }

            if (xml.ChildNodes.Count > 1)
            {
                xnRoot = xml.ChildNodes[1];
            }
            else
            {
                xnRoot = xml.ChildNodes[0];
            }

            xnRd.Close();
        //}
        //catch (Exception ex)
        //{
        //    AciWeb.Alert(ex.Message);
        //}
	}
    public string setXmlCode(string scode)
    {
        string s = "";
        string[] strs = scode.Split(new char[] { '\"' }, StringSplitOptions.None);

        for (int i = 0; i < strs.Length; i++)
        {
            if (i % 2 == 1)
            {
                s += "\"";
                strs[i] = strs[i].Replace("&", "&amp;");
                strs[i] = strs[i].Replace("<", "&lt;");
                strs[i] = strs[i].Replace(">", "&gt;");
                strs[i] = strs[i].Replace("'", "&apos;");
                strs[i] = strs[i].Replace("\"", "&quot;");

                s += strs[i];
                s += "\"";
            }
            else
            {
                s += strs[i];
            }
        }
        return s;
    }
    /// <summary>
    /// 获取某一节点 返回 XmlNode
    /// </summary>
    /// <param name="spath">起始节点路径 例如："setCuteData[0]/DataTable[1]" 就会返回 根目录下 setCuteData 节点的第一个中的 DataTable 节点的第二个节点</param>
    public XmlNode getNodeByPath(string spath)
    {
        XmlNode xn = xnRoot;
        string s = spath;
        string[] str = s.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < str.Length; i++)
        {
            try
            {
                bool ok = false;
                string[] ss = str[i].Split('[');
                XmlNodeList xnl = xn.ChildNodes;
                int it = 0;
                for (int j = 0; j < xnl.Count; j++)
                {
                    if (xnl[j].Name == ss[0])
                    {
                        if (it == AciCvt.ToInt(ss[1].Replace("]", "")))
                        {
                            xn = xnl[j];
                            ok = true;
                        }
                        it++;
                    }
                }
                //节点找不到时返回 null
                if (!ok)
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
        if (xn == xnRoot)
        {
            xn = null;
        }
        return xn;
    }
    /// <summary>
    /// 获取某一节点列表  返回 XmlNodeList
    /// </summary>
    /// <param name="spath">起始节点路径 例如："setCuteData[0]/DataTable[1]" 就会返回 根目录下 setCuteData 节点的第一个中的 DataTable 节点的第二个节点</param>
    /// <param name="sname">指定路径下的同名的节点列表</param>
    public XmlNode[] getNodeArrayByPath(string spath, string sname)
    {
        XmlNodeList xnl = getNodeByPath(spath).ChildNodes;
        int it = 0;
        for (int i = 0; i < xnl.Count; i++)
        {
            if (xnl[i].Name == sname)
            {
                it++;
            }
        }
        XmlNode[] xns = new XmlNode[it];
        it = 0;
        for (int i = 0; i < xnl.Count; i++)
        {
            if (xnl[i].Name == sname)
            {
                xns[it] = xnl[i];
                it++;
            }
        }
        return xns;
    }
    /// <summary>
    /// 从根节点出发 获取sname节点名字第一个的值
    /// </summary>
    /// <param name="sname">节点名称</param>
    public string getNodeValueByName(string sname)
    {
        return xml.GetElementsByTagName(sname)[0].InnerText;
    }
    /// <summary>
    /// 从根节点出发 获取sname节点名字第一个的sAttribute属性的值
    /// </summary>
    /// <param name="sname">节点名称</param>
    /// <param name="sAttribute">属性名称</param>
    public string getNodeValueByName(string sname, string sAttribute)
    {
        return xml.GetElementsByTagName(sname)[0].Attributes[sAttribute].Value;
    }
    /// <summary>
    /// 从指定路径节点出发 获取sname节点名字第一个的值
    /// </summary>
    /// <param name="spath">节点路径 例如："setCuteData[0]/DataTable[1]" 就会返回 根目录下 setCuteData 节点的第一个中的 DataTable 节点的第二个节点 的 InnerText</param>
    public string getNodeValueByPath(string spath)
    {
        try
        {
            return getNodeByPath(spath).InnerText;
        }
        catch
        {

        }
        return "";
    }
    /// <summary>
    /// 从指定路径节点出发 获取sname节点名字第一个的sAttribute属性的值
    /// </summary>
    /// <param name="spath">起始节点路径 例如："setCuteData[0]/DataTable[1]" 就会返回 根目录下 setCuteData 节点的第一个中的 DataTable 节点的第二个节点</param>
    /// <param name="sAttribute">属性名称</param>
    public string getNodeValueByPath(string spath, string sAttribute)
    {
        try
        {
            return getNodeByPath(spath).Attributes[sAttribute].Value;
        }
        catch
        {

        }
        return "";
    }
}

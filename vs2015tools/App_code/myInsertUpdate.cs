using System;
using System.Data;
using System.Collections;
using System.Web;

/// <summary>
/// myInsertUpdate 的摘要说明
/// </summary>
public class myInsertUpdate
{
    /// <summary>insert时的列</summary>
    public ArrayList list_add;
    /// <summary>insert时的值</summary>
    public ArrayList value_add;
    /// <summary>update时的列</summary>
    public ArrayList list_edit;
    /// <summary>update时的值</summary>
    public ArrayList value_edit;
    /// <summary>判断是插入还是更新的条件 已经有where 第一个组不需要 and </summary>
    public string sWhere;
    /// <summary>同步的表</summary>
    public string sTable;
    /// <summary>关键列 DataBind 后 返回的就是该值 一般为 id</summary>
    public string sID;
    /// <summary>数据连接类</summary>
    public myDataConn myDC;
    /// <summary>是否是添加</summary>
    public bool bAdd;
    /// <summary>是否是更新</summary>
    public bool bEdit;
    /// <summary>
    /// 同步类
    /// </summary>
    /// <param name="mydc"></param>
    /// <param name="table">同步的表</param>
    /// <param name="id">通常为 id</param>
    /// <param name="where">判断是插入还是更新的条件 已经有where 第一个组不需要 and </param>
    public myInsertUpdate(myDataConn mydc, string table, string id, string where)
	{
        sTable = table;
        sWhere = where;
        sID = id;
        myDC = mydc;
        Clear();
	}
    public void Clear()
    {
        list_add = new ArrayList();
        value_add = new ArrayList();
        list_edit = new ArrayList();
        value_edit = new ArrayList();
    }
    /// <summary>
    /// 插入同步列
    /// </summary>
    /// <param name="slist">列名 逗号后接类型   列明,0 数字 3为浮点数字</param>
    /// <param name="svalues">所有时的值</param>
    public void Both(string slist, object svalue)
    {
        Both(slist, svalue.ToString());
    }
    /// <summary>
    /// 插入同步列
    /// </summary>
    /// <param name="slist">列名 逗号后接类型   列明,0 数字 3为浮点数字</param>
    /// <param name="svalue">所有时的值</param>
    public void Both(string slist, string svalue)
    {
        list_add.Add(slist);
        value_add.Add(svalue);

        list_edit.Add(slist);
        value_edit.Add(svalue);
    }
    /// <summary>
    /// 插入同步列
    /// </summary>
    /// <param name="slist">列名 逗号后接类型   列明,0 数字 3为浮点数字</param>
    /// <param name="sadd">insert时的值</param>
    /// <param name="seidt">update时的值</param>
    public void Both(string slist, object sadd, object seidt)
    {
        Both(slist, sadd.ToString(), seidt.ToString());
    }
    /// <summary>
    /// 插入同步列
    /// </summary>
    /// <param name="slist">列名 逗号后接类型   列明,0 数字 3为浮点数字</param>
    /// <param name="sadd">insert时的值</param>
    /// <param name="seidt">update时的值</param>
    public void Both(string slist, string sadd, string seidt)
    {
        list_add.Add(slist);
        value_add.Add(sadd);
        
        list_edit.Add(slist);
        value_edit.Add(seidt);
    }
    /// <summary>
    /// 插入仅insert时 的列
    /// </summary>
    /// <param name="slist">列名 逗号后接类型   列明,0 数字 3为浮点数字</param>
    /// <param name="svalue">insert时的值</param>
    public void Add(string slist, object svalue)
    {
        list_add.Add(slist);
        value_add.Add(svalue.ToString());
    }
    /// <summary>
    /// 插入仅insert时 的列
    /// </summary>
    /// <param name="slist">列名 逗号后接类型   列明,0 数字 3为浮点数字</param>
    /// <param name="svalue">insert时的值</param>
    public void Add(string slist, string svalue)
    {
        list_add.Add(slist);
        value_add.Add(svalue);
    }
    /// <summary>
    /// 插入仅update时 的列
    /// </summary>
    /// <param name="slist">列名 逗号后接类型   列明,0 数字 3为浮点数字</param>
    /// <param name="svalue">update时的值</param>
    public void Edit(string slist, object svalue)
    {
        list_edit.Add(slist);
        value_edit.Add(svalue.ToString());
    }
    /// <summary>
    /// 插入仅update时 的列
    /// </summary>
    /// <param name="slist">列名 逗号后接类型   列明,0 数字 3为浮点数字</param>
    /// <param name="svalue">update时的值</param>
    public void Edit(string slist, string svalue)
    {
        list_edit.Add(slist);
        value_edit.Add(svalue);
    }
    /// <summary>开始执行同步 需要 myDC.open() 和 myDC.close()</summary>
    /// <param name="skey">关键列名称 一般为 id  返回的就是该值</param>
    /// <returns></returns>
    public string DataBind()
    {
        return DataBind(sID);
    }
    /// <summary>开始执行同步 需要 myDC.open() 和 myDC.close()</summary>
    /// <param name="skey">关键列名称 一般为 id  返回的就是该值</param>
    /// <returns></returns>
    public string DataBind(object oID)
    {
        string where = "";
        if (sWhere != "")
        {
            where = " where " + sWhere;
        }
        string sid = myDC.Fc_sqlReturnString("select " + sID + " from " + sTable + " " + where + "");
        if (sid == "")
        {
            bAdd = true;
            bEdit = false;
            if (list_add.Count != 0)
            {
                sid = myDC.sqlInsert(sTable, list_add, value_add, oID);
            }
        }
        else
        {
            bAdd = false;
            bEdit = true;
            if (list_edit.Count != 0)
            {
                myDC.sqlUpdate(sTable, list_edit, value_edit, sWhere);
            }
        }
        return sid;
    }
    /// <summary>
    /// 返回： "总共：" + iAll + "条 更新：" + (iAll - iAdd) + " 新增：" + iAdd + ""
    /// </summary>
    /// <param name="s1">总共：</param>
    /// <param name="iAll">总数</param>
    /// <param name="iAdd">新增</param>
    /// <returns></returns>
    public static string getWord(int iAll, int iAdd)
    {
        return getWord("总共：", "更新：", "新增：", iAll, iAdd);
    }
    /// <summary>
    /// 返回： "总共：" + iAll + "条 更新：" + (iAll - iAdd) + " 新增：" + iAdd + ""
    /// </summary>
    /// <param name="s1">总共：</param>
    /// <param name="iAll">总数</param>
    /// <param name="iAdd">新增</param>
    /// <returns></returns>
    public static string getWord(string s1, int iAll, int iAdd)
    {
        return getWord(s1, "更新：", "新增：", iAll, iAdd);
    }
    /// <summary>
    /// 返回： "总共：" + iAll + "条 更新：" + (iAll - iAdd) + " 新增：" + iAdd + ""
    /// </summary>
    /// <param name="s1">总共：</param>
    /// <param name="s2">更新：</param>
    /// <param name="s3">新增：</param>
    /// <param name="iAll">总数</param>
    /// <param name="iAdd">新增</param>
    /// <returns></returns>
    public static string getWord(string s1, string s2, string s3, int iAll, int iAdd)
    {
        return s1 + "" + iAll + "条 " + s2 + "" + (iAll - iAdd) + " " + s3 + "" + iAdd + "";
    }
}

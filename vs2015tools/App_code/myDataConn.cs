using System;
using System.Collections;
using System.Text;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Data;

public class myDataConn
{
    public SqlConnection connSql;
    public SqlCommand cmdSql;
    public SqlDataReader rsSql;
    public SqlDataAdapter odaSql;

    public OleDbConnection connOleDb;
    public OleDbDataAdapter odaOleDb;
    public OleDbCommand cmdOleDb;
    public OleDbDataReader rsOleDb;

    ///   <summary>数据库模式 oledb sql</summary>
    public string connMode;
    ///   <summary>
    ///   数据库操作类 sql
    ///   </summary>
    ///   <param name="sMode">连接方式 支持 "sql"和 "oledb"  远程使用 "post"</param>
    ///   <param name="connUrl">连接的路径：当为NULL时 为web.config 中设置的默认值。 如 oledb： "../App_Data/success.mdb" 如 sql： "server=localhost;database=success;uid=sa;pwd='' post:http://www.a.com/dbPost/"</param>
    public myDataConn(string sMode, object connUrl)
    {
        Init_myDC(sMode, connUrl);
    }

    void Init_myDC(string sMode, object connUrl)
    {
        if (sMode.ToLower() == "sql")
        {
            connMode = "sql";
            connSql = new SqlConnection(connUrl.ToString());
        }
        else if (sMode.ToLower() == "oledb")
        {
            connMode = "oledb";
            connOleDb = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + connUrl.ToString() + "");
        }
    }

    ///   <summary>
    ///   将SQL语句填充，返回一个 DataTable
    ///   </summary>
    ///   <param name="sql">sql语句</param>
    ///   <param name="name">返回TABLE的名称</param>
    public DataTable Fc_OdaDataTable(string sql, string name)
    {

        return Fc_OdaDataTable(sql, name, false);
    }
    ///   <summary>
    ///   将SQL语句填充，返回一个 DataTable
    ///   </summary>
    ///   <param name="sql">sql语句</param>
    ///   <param name="name">返回TABLE的名称</param>
    public DataTable Fc_OdaDataTable(string sql, string name, Boolean bOrder)
    {
        DataTable dt = new DataTable(name);
        try
        {
            switch (connMode)
            {
                case "oledb":
                    //if (bOrder) AciDT.CreateOrder(1, ref dt);
                    odaOleDb = new OleDbDataAdapter(sql, connOleDb);
                    //if (bSqlType) odaOleDb.SelectCommand.CommandType = CommandType.StoredProcedure;
                    odaOleDb.Fill(dt);
                    break;
                case "sql":
                    //if (bOrder) AciDT.CreateOrder(1, ref dt);
                    odaSql = new SqlDataAdapter(sql, connSql);
                    //if (bSqlType) odaSql.SelectCommand.CommandType = CommandType.StoredProcedure;
                    odaSql.Fill(dt);
                    break;
                case "post":

                    break;
            }
        }
        catch (Exception ex)
        {
            AciDebug.Debug(sql + " " + ex.Message);
        }
        return dt;
    }
    ///   <summary>
    ///   将SQL语句填充，且可选择条数，返回一个 DataTable
    ///   </summary>
    ///   <param name="sql">sql语句</param>
    ///   <param name="iStart">读取的数据的起始条数位置</param>
    ///   <param name="iNum">读取的数据的条数</param>
    ///   <param name="name">返回TABLE的名称</param>
    public DataTable Fc_OdaDataTable(string sql, string name, int iStart, int iNum)
    {
        return Fc_OdaDataTable(sql, name, iStart, iNum, false);
    }
    ///   <summary>
    ///   将SQL语句填充，且可选择条数，返回一个 DataTable
    ///   </summary>
    ///   <param name="sql">sql语句</param>
    ///   <param name="iStart">读取的数据的起始条数位置</param>
    ///   <param name="iNum">读取的数据的条数</param>
    ///   <param name="name">返回TABLE的名称</param>
    public DataTable Fc_OdaDataTable(string sql, string name, int iStart, int iNum, Boolean bOrder)
    {
        DataTable dt = new DataTable(name);
        try
        {
            switch (connMode)
            {
                case "oledb":
                    //if (bOrder) AciDT.CreateOrder(1, ref dt);
                    odaOleDb = new OleDbDataAdapter(sql, connOleDb);
                    //if (bSqlType) odaOleDb.SelectCommand.CommandType = CommandType.StoredProcedure;
                    odaOleDb.Fill(iStart, iNum, dt);
                    break;
                case "sql":
                    //if (bOrder) AciDT.CreateOrder(1, ref dt);
                    odaSql = new SqlDataAdapter(sql, connSql);
                    //if (bSqlType) odaSql.SelectCommand.CommandType = CommandType.StoredProcedure;
                    odaSql.Fill(iStart, iNum, dt);
                    break;
                case "post":

                    break;
            }
        }
        catch (Exception ex)
        {
            AciDebug.Debug(sql + " " + ex.Message);
        }
        return dt;
    }

    ///   <summary>
    ///   执行一段SQL 返回第一行的一个值 无时返回 ""  需要 myDC.open() 支持， 要关闭 myDC.close()
    ///   </summary>
    ///   <param name="sql">要执行的SQL语句</param>
    public string Fc_sqlReturnString(string sql)
    {
        try
        {
            string str = "";
            switch (connMode)
            {
                case "oledb":
                    cmdOleDb = new OleDbCommand(sql, connOleDb);
                    rsOleDb = cmdOleDb.ExecuteReader();
                    if (rsOleDb.Read())
                    {
                        str = rsOleDb[0].ToString();
                    }
                    rsOleDb.Close();
                    return str;
                    break;
                case "sql":
                    cmdSql = new SqlCommand(sql, connSql);
                    rsSql = cmdSql.ExecuteReader();
                    if (rsSql.Read())
                    {
                        str = rsSql[0].ToString();
                    }
                    rsSql.Close();
                    return str;
                    break;
                case "post":

                    break;
            }
        }
        catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show(sql + " " + ex.Message);
        }
        return "";
    }

    ///   <summary>
    ///   执行一段SQL 需要 myDC.open() 支持， 要关闭 myDC.close()
    ///   </summary>
    ///   <param name="sql">要执行的SQL语句</param>
    public string sqlExecute(string sql)
    {
        try
        {
            switch (connMode)
            {
                case "oledb":
                    cmdOleDb = new OleDbCommand(sql, connOleDb);
                    cmdOleDb.ExecuteNonQuery();
                    break;
                case "sql":
                    cmdSql = new SqlCommand(sql, connSql);
                    cmdSql.ExecuteNonQuery();
                    break;
                case "post":
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show(sql + " " + ex.Message);
        }
        return "";
    }

    public void open()
    {
        try
        {
            openNoTry();
        }
        catch (Exception ex)
        {
            AciDebug.Debug(ex.Message);
        }
    }
    public void openNoTry()
    {
        switch (connMode)
        {
            case "oledb":
                if (connOleDb.State == ConnectionState.Closed) connOleDb.Open();
                break;
            case "sql":
                if (connSql.State == ConnectionState.Closed) connSql.Open();
                break;
        }
    }
    public void close()
    {
        try
        {
            closeNoTry();
        }
        catch (Exception ex)
        {
            AciDebug.Debug(ex.Message);
        }
    }
    public void closeNoTry()
    {
        switch (connMode)
        {
            case "oledb":
                if (connOleDb.State == ConnectionState.Open) connOleDb.Close();
                break;
            case "sql":
                if (connSql.State == ConnectionState.Open) connSql.Close();
                break;
        }
    }
    ///   <summary>
    ///   插入数据 需要 myDC.open() 支持， 要关闭 myDC.close() 如oMaxID不为空时返回插入的ID值 否则返回 sql 语句
    ///   </summary>
    ///   <param name="sFrom">要插入内容的表</param>
    ///   <param name="sList">需要插入的数据列名  0数字 1字符 2组合语句 3为浮点数字 5日期字段 -1 时id取最大加一</param>
    ///   <param name="sValue">需要插入的数据劣数值</param>
    ///   <param name="oMaxID">不需要时请输入 null 需要自动递增max ID 列时 输入列名</param>
    public string sqlInsert(string sFrom, ArrayList sList, ArrayList sValue, object oMaxID)
    {
        return sqlInsert(sFrom, AciCvt.ArrayList_to_StringArr(sList), AciCvt.ArrayList_to_StringArr(sValue), oMaxID);
    }
    ///   <summary>
    ///   插入数据 需要 myDC.open() 支持， 要关闭 myDC.close() 如oMaxID不为空时返回插入的ID值 否则返回 sql 语句
    ///   </summary>
    ///   <param name="sFrom">要插入内容的表</param>
    ///   <param name="sList">需要插入的数据列名  0数字 1字符 2组合语句 3为浮点数字 5日期字段 -1 时id取最大加一</param>
    ///   <param name="sValue">需要插入的数据劣数值</param>
    public string sqlInsert(string sFrom, string[] sList, string[] sValue)
    {
        return sqlInsert(sFrom, sList, sValue, null);
    }
    ///   <summary>
    ///   插入数据 需要 myDC.open() 支持， 要关闭 myDC.close() 如oMaxID不为空时返回插入的ID值 否则返回 sql 语句
    ///   </summary>
    ///   <param name="sFrom">要插入内容的表</param>
    ///   <param name="sList">需要插入的数据列名  0数字 1字符 2组合语句 3为浮点数字 5日期字段 -1 时id取最大加一</param>
    ///   <param name="sValue">需要插入的数据劣数值</param>
    ///   <param name="oMaxID">默认为 null 需要自动递增max ID 列时 输入列名</param>
    public string sqlInsert(string sFrom, string[] sList, string[] sValue, object oMaxID)
    {
        string sql = "insert into " + sFrom + "(";
        long iMax = 0;
        for (int i = 0; i < sList.Length; i++)
        {
            if (sList[i] != "")
            {
                string[] arrValue = sList[i].Split(',');
                sql += " " + arrValue[0];
                sql += ",";
            }
        }
        if (sql != "")
        {
            sql = sql.Substring(0, sql.Length - 1);
        }

        if (oMaxID != null)
        {
            sql += "," + oMaxID;
        }

        sql += ")values(";
        for (int i = 0; i < sValue.Length; i++)
        {
            if (sList[i] != "")
            {
                string[] arrValue = sList[i].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (arrValue.Length >= 2)
                {
                    if (arrValue[1] == "0")
                    {
                        if (sValue[i].ToLower().Trim() == "true")
                        {
                            sql += "1";
                        }
                        else
                        {
                            sql += AciCvt.ToInt64(sValue[i]);
                        }
                    }
                    else if (arrValue[1] == "3")
                    {
                        sql += AciCvt.ToDouble(sValue[i]);
                    }
                    else if (arrValue[1] == "1")
                    {
                        sql += "'" + AciSf.ReplaceStr(sValue[i]) + "'";
                    }
                    else if (arrValue[1] == "5")
                    {
                        string stime = AciCvt.tryTimeFull(AciSf.ReplaceStr(sValue[i]));
                        if (stime == "")
                        {
                            sql += "null";
                        }
                        else
                        {
                            sql += "'" + stime + "'";
                        }
                    }
                    else if (arrValue[1] == "-1")
                    {
                        sql += (AciCvt.ToInt64(Fc_sqlReturnString("select max(id) from " + sFrom + "")) + 1).ToString();
                    }
                    else
                    {
                        sql += sValue[i];
                    }
                }
                else
                {
                    sql += "'" + AciSf.ReplaceStr(sValue[i]) + "'";
                }
                sql += ",";
            }
        }
        if (sql != "")
        {
            sql = sql.Substring(0, sql.Length - 1);
        }
        if (oMaxID != null)
        {
            //自制自动编号
            iMax = AciCvt.ToInt64(Fc_sqlReturnString("select max(" + oMaxID + ") from " + sFrom)) + 1;
            sql += "," + iMax.ToString();
        }
        sql += ")";

        //HttpContext.Current.Response.Write(sql);
        //HttpContext.Current.Response.End();

        try
        {
            switch (connMode)
            {
                case "oledb":
                    cmdOleDb = new OleDbCommand(sql, connOleDb);
                    cmdOleDb.ExecuteNonQuery();
                    break;
                case "sql":
                    cmdSql = new SqlCommand(sql, connSql);
                    cmdSql.ExecuteNonQuery();
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show(sql + " " + ex.Message);
        }

        if (oMaxID != null)
        {
            return iMax.ToString();
        }
        else
        {
            return sql;
        }
    }

    ///   <summary>
    ///   更新数据 需要 myDC.open() 支持， 要关闭 myDC.close()
    ///   </summary>
    ///   <param name="sFrom">要更新内容的表</param>
    ///   <param name="sList">需要更新的数据列名  0数字 1字符 2组合语句 3为浮点数字 5日期字段</param>
    ///   <param name="sValue">需要更新的数据劣数值</param>
    ///   <param name="sWhere">需要更新的条件 已经有where 了，不需要时为 ""</param>
    public string sqlUpdate(string sFrom, ArrayList sList, ArrayList sValue, string sWhere)
    {
        return sqlUpdate(sFrom, AciCvt.ArrayList_to_StringArr(sList), AciCvt.ArrayList_to_StringArr(sValue), sWhere);
    }
    ///   <summary>
    ///   更新数据 需要 myDC.open() 支持， 要关闭 myDC.close()
    ///   </summary>
    ///   <param name="sFrom">要更新内容的表</param>
    ///   <param name="sList">需要更新的数据列名  0数字 1字符 2组合语句 3为浮点数字 5日期字段</param>
    ///   <param name="sValue">需要更新的数据劣数值</param>
    ///   <param name="sWhere">需要更新的条件 已经有where 了，不需要时为 ""</param>
    public string sqlUpdate(string sFrom, string[] sList, string[] sValue, string sWhere)
    {
        string sql = "update " + sFrom + " set ";

        for (int i = 0; i < sList.Length; i++)
        {
            if (sList[i] != "")
            {
                string[] arrValue = sList[i].Split(',');
                if (arrValue.Length >= 2)
                {
                    if (arrValue[1] == "0")
                    {
                        if (sValue[i].ToLower().Trim() == "true")
                        {
                            sql += " " + arrValue[0] + "=1";
                        }
                        else
                        {
                            sql += " " + arrValue[0] + "=" + AciCvt.ToInt64(sValue[i]);
                        }
                    }
                    else if (arrValue[1] == "3")
                    {
                        sql += " " + arrValue[0] + "=" + AciCvt.ToDouble(sValue[i]);
                    }
                    else if (arrValue[1] == "1")
                    {
                        sql += " " + arrValue[0] + "='" + AciSf.ReplaceStr(sValue[i]) + "'";
                    }
                    else if (arrValue[1] == "5")
                    {
                        string stime = AciCvt.tryTimeFull(AciSf.ReplaceStr(sValue[i]));
                        if (stime == "")
                        {
                            sql += " " + arrValue[0] + "=null";
                        }
                        else
                        {
                            sql += " " + arrValue[0] + "='" + AciSf.ReplaceStr(sValue[i]) + "'";
                        }
                    }
                    else
                    {
                        sql += " " + arrValue[0] + "" + sValue[i] + "";
                    }
                }
                else
                {
                    sql += " " + arrValue[0] + "='" + AciSf.ReplaceStr(sValue[i]) + "'";
                }
                sql += ",";
            }
        }
        if (sql != "")
        {
            sql = sql.Substring(0, sql.Length - 1);
        }
        if (sWhere != "")
        {
            sql += " where " + sWhere;
        }

        //HttpContext.Current.Response.Write(sql);
        //HttpContext.Current.Response.End();

        try
        {
            switch (connMode)
            {
                case "oledb":
                    cmdOleDb = new OleDbCommand(sql, connOleDb);
                    cmdOleDb.ExecuteNonQuery();
                    break;
                case "sql":
                    cmdSql = new SqlCommand(sql, connSql);
                    cmdSql.ExecuteNonQuery();
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show(sql + " " + ex.Message);
        }
        return sql;
    }
    ///   <summary>
    ///   得到全部的字段名字 需要 myDC.open() 支持， 要关闭 myDC.close()
    ///   </summary>
    ///   <param name="sTable">表名</param>
    public ArrayList getDatalistString(string sTable)
    {
        string sql = "select top 1 * from " + sTable + "";
        try
        {
            ArrayList al = new ArrayList(); ;
            switch (connMode)
            {
                case "oledb":
                    cmdOleDb = new OleDbCommand(sql, connOleDb);
                    rsOleDb = cmdOleDb.ExecuteReader();
                    for (int i = 0; i < rsOleDb.FieldCount; i++)
                    {
                        al.Add(rsOleDb.GetName(i).ToLower());
                    }
                    rsOleDb.Close();
                    return al;
                    break;
                case "sql":
                    cmdSql = new SqlCommand(sql, connSql);
                    rsSql = cmdSql.ExecuteReader();
                    for (int i = 0; i < rsSql.FieldCount; i++)
                    {
                        al.Add(rsSql.GetName(i).ToLower());
                    }
                    rsSql.Close();
                    return al;
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show(sql + " " + ex.Message);
        }
        return null;
    }
}

using System;
using System.Data;
using System.Collections;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

/// <summary>
/// AFPBasePage 的摘要说明
/// </summary>
public class AFPBasePage
{
    myDataConn myDC = null;
    public string sAgency = "";
    public string sYear = "";
    public string sBox = "";
    public string sKeyid = "";
    public string sSQL = "";

    /// <summary>是否去除B面 true不去除 false去除</summary>
    public bool isB = true;
    /// <summary>额外的信息 如：去除00001A.jpg页 " and pagename not like'00001A.jpg'" </summary>
    public string sSqlOutMore = "";

    public string sSql = "";

    public AFPBasePage(myDataConn mydc, string agency, string year, string box, string keyid)
    {
        setDefault(mydc, agency, year, box, keyid);
    }

    void setDefault(myDataConn mydc, string agency, string year, string box, string keyid)
    {
        myDC = mydc;
        sAgency = agency;
        sYear = year;
        sBox = box;
        sKeyid = keyid;

        if (sAgency != "")
        {
            sSql += " and agency='" + sAgency + "'";
        }
        if (sYear != "")
        {
            sSql += " and iyear='" + sYear + "'";
        }
        if (sBox != "")
        {
            sSql += " and box='" + sBox + "'";
        }
        if (sKeyid != "")
        {
            sSql += " and keyid='" + sKeyid + "'";
        }
    }

    public DataTable getPageData()
    {
        DataTable dtPage = new DataTable();
        dtPage.Columns.Add("sgroup");
        dtPage.Columns.Add("pagename");
        dtPage.Columns.Add("pagenum");
        dtPage.Columns.Add("list_j");
        dtPage.Columns.Add("list_rq");
        dtPage.Columns.Add("list_rq2");
        dtPage.Columns.Add("list_yyz");
        dtPage.Columns.Add("isRotate");
        dtPage.Columns.Add("list_XTBJ");
        dtPage.Columns.Add("scut");

        string sSqlOutB = "";
        if (!isB)
        {
            sSqlOutB = " and pagename not like'%B.jpg%'";
        }

        int iPage = 0;
        string sPage = "";
        sSQL = "select sgroup,pagename,pagenum,list_j,list_rq,list_rq2,list_yyz,isRotate,list_XTBJ,scut from AFP_Archive_Page where 1=1 " + sSql + sSqlOutB + sSqlOutMore
            + " and (sort<>'00' or sort is null) order by pagename asc";
        DataTable dt = myDC.Fc_OdaDataTable(sSQL, "");
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            DataRow dr = dt.Rows[i];
            string pagenum = dr["pagenum"].ToString();
            //4-1,5
            if (pagenum != "")
            {
                //sPage = iPage + "_" + dr["pagenum"].ToString();
                sPage = pagenum;
                string[] arr = sPage.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length > 0)
                {
                    int iTemp = -1;
                    for (int j = 0; j < arr.Length; j++)
                    {
                        string[] arrOne = arr[j].Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                        if (arrOne != null)
                        {
                            int iXu = AciCvt.ToInt(arrOne[0]);
                            if (iXu > iTemp)
                            {
                                iTemp = iXu;
                            }
                        }
                    }
                    iPage = iTemp;
                }
            }
            else
            {
                iPage++;
                sPage = iPage.ToString();
            }

            DataRow drPage = dtPage.NewRow();
            drPage["sgroup"] = dr["sgroup"].ToString();
            drPage["pagename"] = dr["pagename"].ToString();
            drPage["list_rq"] = dr["list_rq"].ToString();
            drPage["list_rq2"] = dr["list_rq2"].ToString();
            drPage["list_j"] = dr["list_j"].ToString();
            drPage["list_yyz"] = dr["list_yyz"].ToString();
            drPage["isRotate"] = dr["isRotate"].ToString();
            drPage["list_XTBJ"] = dr["list_XTBJ"].ToString();
            drPage["scut"] = dr["scut"].ToString();
            drPage["pagenum"] = sPage;
            dtPage.Rows.Add(drPage);
        }

        return dtPage;
    }
}

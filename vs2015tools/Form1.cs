using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;


public partial class Form1 : Form
{
    ImageXiuTuTools tools;
    PictureBox picMain;

    PictureBox picFile;

    myDataConn myDC = null;
    string sPathOld = "";
    string sPathNew = "";
    //领取方式：领取新盒 领取page表
    string sGetType = "";
    //领取page表时的sql语句
    string sSqlPage = "";

    //当前选中项目where
    string sWhereTable = "";

    DataTable dtKeyidAll = null;
    DataRow drKeyidNow = null;
    DataTable dtNowKeyidPage = null;
    DataTable dtPageShow = null;
    int iPageAll = 0;
    int iPageNow = 0;
    int iKeyidNow = 0;
    int iRotate = 0;

    string sPathFileOld = "";
    string sPathFileNew = "";

    string sUser = "";

    public Form1()
    {
        InitializeComponent();
        pl_main.KeyUp += new KeyEventHandler(xiutu_KeyUp);
        this.Click += Form1_Click;
        pl_main.Click += Form1_Click;

        picMain = new PictureBox();
        tools = new ImageXiuTuTools(this, pl_main, picMain, picBG, lbSave, PL_box);
        pl_main.Controls.Add(picMain);

        picFile = new PictureBox();
        pl_file.Controls.Add(picFile);

        myXML xml = new myXML(Application.StartupPath + "/config.xml");
        string mode = xml.getNodeByPath("item[0]").Attributes["mode"].Value;
        string conn = xml.getNodeByPath("item[0]").Attributes["conn"].Value;
        sPathOld = xml.getNodeByPath("item[1]").Attributes["path"].Value;
        sPathNew = xml.getNodeByPath("item[2]").Attributes["path"].Value;
        sGetType = xml.getNodeByPath("item[3]").Attributes["type"].Value;
        sSqlPage = xml.getNodeByPath("item[3]").Attributes["sql"].Value;
        myDC = new myDataConn(mode, conn);
        myDC.open();

        #region 账号验证
        string[] arrEnvi = System.Environment.GetCommandLineArgs();
        if (arrEnvi.Length == 2)
        {
            sUser = arrEnvi[1];
        }
        else
        {
            sUser = "admin";//zuojing admin
        }
        string susername = myDC.Fc_sqlReturnString("select user_name from agi_sys_user_username where user_username='" + sUser + "'");
        if (susername != "")
        {
            //信息录入
            lb_user.Text = "检查员：" + susername;
        }
        else
        {
            MessageBox.Show("账号【" + sUser + "】,在人事档案系统中不存在，请联系管理员！");
            System.Environment.Exit(0);
        }
        #endregion

        init();

        this.FormClosing += Form1_FormClosing;
        this.Shown += Form1_Shown;
    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (myDC != null)
        {
            myDC.close();
        }
    }

    public void init()
    {
        DataTable dttable = new DataTable();
        dttable.Columns.Add("name");
        dttable.Columns.Add("remark");
        DataRow dr = dttable.NewRow();
        dr[0] = "-100";
        dr[1] = "全部";
        dttable.Rows.Add(dr);
        DataTable dttemp = myDC.Fc_OdaDataTable("select a.name as name,b.table_remark as remark from AFP_Sort_Table a left join sys_table_record b on a.name=b.table_name where name<>'data' order by a.id", "table");
        for (int i = 0; i < dttemp.Rows.Count; i++)
        {
            dr = dttable.NewRow();
            dr[0] = dttemp.Rows[i]["name"].ToString();
            dr[1] = dttemp.Rows[i]["remark"].ToString();
            dttable.Rows.Add(dr);
        }
        cmbTable.DataSource = dttable;
        cmbTable.DisplayMember = "remark";
        cmbTable.ValueMember = "name";
    }

    //需要读取数据库转换角度
    public void toRotate(int iR, int type)
    {
        iRotate += iR;
        if (iRotate == 360 || iRotate == 270)
        {
            iRotate = 0;
        }
        if (iRotate == -180)
        {
            iRotate = 180;
        }

        if (type == 0)
        {
            tools.getRotate(iR, sPathFileOld);
        }
        else if (type == 1)
        {
            tools.getRotateYT(iR, sPathFileOld);
        }
    }
    public void toSave()
    {
        tools.Save(sPathFileNew);
    }
    public void toSaveData()
    {
        //保存旋转角度
        if (dtNowKeyidPage != null && dtNowKeyidPage.Rows.Count > 0)
        {
            string swhere = "";
            if (lbSave.Text == "需要保存")
            {
                swhere = ",list_XTBJ=20";
            }
            if (list_error.Text == "错误")
            {
                swhere += ",list_error=1";
                dtNowKeyidPage.Rows[iPageNow - 1]["list_error"] = "1";
            }
            else
            {
                swhere += ",list_error=0";
                dtNowKeyidPage.Rows[iPageNow - 1]["list_error"] = "0";
            }
            myDC.sqlExecute("update AFP_Archive_Page set isRotate=" + iRotate + swhere + ",finishCheck=2 where id=" + dtNowKeyidPage.Rows[iPageNow - 1]["id"]);
            //AciDebug.Debug("update AFP_Archive_Page set isRotate=" + iRotate + swhere + ",finishCheck=2 where id=" + dtNowKeyidPage.Rows[iPageNow - 1]["id"]);
            toSave();
        }
    }
    public void toPage(int page)
    {
        if (dtNowKeyidPage != null)
        {
            if (page <= 0)
            {
                page = 1;
            }
            else if (page > dtNowKeyidPage.Rows.Count)
            {
                page = dtNowKeyidPage.Rows.Count;
            }
            iPageNow = page;
            loadPageInfo();
            pl_main.Focus();
        }
    }
    public void toNext()
    {
        if (dtNowKeyidPage != null && dtNowKeyidPage.Rows.Count > 0)
        {
            toSaveData();
            if (iPageNow + 1 <= dtNowKeyidPage.Rows.Count)
            {
                iPageNow++;
            }
            else
            {
                MessageBox.Show("已经到该条码最后一页了！");
                iPageNow = 1;
            }
            loadPageInfo();
        }
    }
    public void toPrev()
    {
        toSaveData();
        if (dtNowKeyidPage != null)
        {
            if (iPageNow - 1 >= 1)
            {
                iPageNow--;
            }
            else
            {
                iPageNow = 1;
            }

            loadPageInfo();
        }
    }
    public void clearKeyid()
    {
        iPageAll = 0;
        iPageNow = 0;
        drKeyidNow = null;
        dtNowKeyidPage = null;

        lb_pageall.Text = "";
        tb_nowpage.Text = "";
    }
    public void clearPage()
    {
        sPathFileOld = "";
        sPathFileNew = "";

        iRotate = 0;
        lb_keyidInfo.Text = "";
    }
    public void loadPageInfo()
    {
        clearPage();

        DataRow drNowPage = dtNowKeyidPage.Rows[iPageNow - 1];
        string sfilename = @"\" + drNowPage["agency"] + @"\" + drNowPage["iyear"] + @"\" + drNowPage["box"] + @"\" + drNowPage["keyid"] + @"\" + drNowPage["pagename"];
        sPathFileOld = sPathOld + sfilename;
        sPathFileNew = sPathNew + sfilename;
        tb_nowpage.Text = iPageNow + "";
        lbPage.Text = dtPageShow.Rows[iPageNow - 1]["pagenum"].ToString();
        string list_error_str = drNowPage["list_error"].ToString();
        switch (list_error_str)
        {
            case "":
                pl_MSG.Visible = false;
                list_error.Text = "正确";
                break;
            case "0":
                pl_MSG.Visible = false;
                list_error.Text = "正确";
                break;
            case "1":
                pl_MSG.Visible = true;
                list_error.Text = "错误";
                break;
            default:
                break;
        }

        tools.setBitmap(sPathFileNew, false);
        setBitmapFile(sPathFileOld);

        iRotate = AciCvt.ToInt(drNowPage["isRotate"].ToString());

        lb_keyidInfo.Text = "盒号：" + drNowPage["box"] + "　　条形码：" + drNowPage["keyid"] + "　　页码：" + drNowPage["pagename"];
    }
    public void InitNewKeyid(string sql)
    {
        dtNowKeyidPage = myDC.Fc_OdaDataTable(sql, "page");
        if (dtNowKeyidPage.Rows.Count > 0)
        {
            iPageAll = dtNowKeyidPage.Rows.Count;
            lb_pageall.Text = iPageAll + "";
            lb_keyidInfo.Text = "盒号：" + dtNowKeyidPage.Rows[0]["box"] + "　　条形码：" + dtNowKeyidPage.Rows[0]["keyid"];
            iPageNow = 1;
            loadPageInfo();
        }
        else
        {
            lb_keyidInfo.Text = "没有可修的图片";
        }
    }
    public void getNewKeyid()
    {
        if (dtKeyidAll.Rows.Count > 0)
        {
            drKeyidNow = dtKeyidAll.Rows[iKeyidNow - 1];
            AFPBasePage AFPBP = new AFPBasePage(myDC, drKeyidNow["agency"].ToString(), drKeyidNow["iyear"].ToString(), drKeyidNow["box"].ToString(), drKeyidNow["keyid"].ToString());
            AFPBP.sSqlOutMore = " and sgroup not in('AAAAA','BBBBB')";
            dtPageShow = AFPBP.getPageData();

            string sql = "select * from AFP_Archive_Page where box='" + drKeyidNow["box"] + "' and keyid='" + drKeyidNow["keyid"] + "' and sgroup not in('AAAAA','BBBBB') and sort<>'00' and sort is not null order by pagename asc";
            InitNewKeyid(sql);
            btnGet.Text = "完成此卷";
        }
        else
        {
            clearKeyid();
            MessageBox.Show("没有可领取的条码");
        }
    }

    private void btnSelect_Click(object sender, EventArgs e)
    {
        if (cmbTable.SelectedIndex > -1)
        {
            lb_project.Text = cmbTable.Text;
            string stablename = cmbTable.SelectedValue.ToString();
            sWhereTable = "";
            if (cmbTable.Text != "全部")
            {
                sWhereTable = " and list_table='" + stablename + "'";
            }

            if (sGetType == "领取新盒")
            {
                iKeyidNow = 0;
                //dtKeyidAll = myDC.Fc_OdaDataTable("select * from AFP_work where istate=6 and box like'CF%' and list_xtjcbj=1 " + sWhereTable + " order by id", "keyid");
                //dtKeyidAll = myDC.Fc_OdaDataTable("select * from AFP_work where  box like 'JS%' and keyid in (select keyid from data_JS_jaq where lock2='10003') " + sWhereTable + " order by id", "keyid");
                //dtKeyidAll = myDC.Fc_OdaDataTable("select * from AFP_work where istate=6 and box like 'JS%' and keyid in (select keyid from data_JS_jaq where list_sczt='健在' or (list_sczt='死亡' and list_swzt='死亡5年以内')) " + sWhereTable + " order by id", "keyid");
                //dtKeyidAll = myDC.Fc_OdaDataTable("select * from AFP_work where istate=6 and keyid in ('01178','00371') " + sWhereTable + " order by id", "keyid");
                //dtKeyidAll = myDC.Fc_OdaDataTable("select * from AFP_work where istate=6 and box like 'BQ%' and list_LSBJ=10 " + sWhereTable + " order by id", "keyid");
                //dtKeyidAll = myDC.Fc_OdaDataTable("select * from AFP_work where istate=6 and keyid in( 'BF00160','BF00005','BF00074','JA0065','JA0134','JA0020','JA0141','JA0107','BF00075','JA0175') order by keyid", "keyid");
                dtKeyidAll = myDC.Fc_OdaDataTable("select * from AFP_work where istate=6 and box like 'hy%' and list_xtjcbj=1 " + sWhereTable + " order by keyid", "keyid");
                //dtKeyidAll = myDC.Fc_OdaDataTable("select * from AFP_work where istate=6 and  keyid in ('JA0113','JA0175') " + sWhereTable + " order by keyid", "keyid");

                //dtKeyidAll = myDC.Fc_OdaDataTable("select * from AFP_work where istate=6 and box like 'hy%' and list_xtjcbj=2 " + sWhereTable + " order by keyid", "keyid");
                dtKeyidAll = myDC.Fc_OdaDataTable("select * from AFP_work where istate>=6 and box like 'cf%' and keyid in ('01276','01278','01820','01870','01279','01507','01673','01275','00778','00651','00341','00337') " + sWhereTable + " order by keyid", "keyid");
                if (dtKeyidAll.Rows.Count > 0)
                {
                    lb_juanall.Text = dtKeyidAll.Rows.Count + "";
                }
            }
            else
            {
                label9.Visible = false;
                lb_juanall.Visible = false;
                btnGet.Visible = false;

                if (sSqlPage != "")
                {
                    InitNewKeyid(sSqlPage);
                }
                else
                {
                    MessageBox.Show("方式是领取page表时，配置表中sql不能为空");
                }
            }
        }
        else
        {
            MessageBox.Show("请先选择项目");
        }
    }
    private void btnGet_Click(object sender, EventArgs e)
    {
        if (btnGet.Text == "领取一卷")
        {
            if (lb_project.Text != "项目")
            {
                iKeyidNow++;

                lb_juanall.Text = iKeyidNow + " / " + dtKeyidAll.Rows.Count;
                getNewKeyid();

                pl_main.Focus();
            }
            else
            {
                MessageBox.Show("请先选择项目");
            }
        }
        else if (btnGet.Text == "完成此卷")
        {
            string pagename = myDC.Fc_sqlReturnString("select pagename from AFP_Archive_Page where box='" + drKeyidNow["box"] + "' and keyid='" + drKeyidNow["keyid"]
                + "' and sgroup not in('AAAAA','BBBBB') and (sort<>'00' or sort is null) and (finishCheck<>2 or finishCheck is null) order by pagename");
            if (pagename == "")
            {
                if (MessageBox.Show("确认要完成这个条码吗？", "", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    btnGet.Enabled = false;
                    //更改检索完成状态 插入日志
                    myDC.sqlExecute("update AFP_work set istate=8,person10='" + sUser + "' where id=" + drKeyidNow["id"]);
                    myInsertUpdate myIU = new myInsertUpdate(myDC, "AFP_worklog", "id", "workid=" + drKeyidNow["id"] + " and imode=505");
                    myIU.Add("workid,0", drKeyidNow["id"]);
                    myIU.Add("imode,0", "505");
                    myIU.Add("suser,1", sUser);
                    myIU.Add("time_create,5", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    myIU.Both("time_edit,5", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    myIU.DataBind();
                    AciDebug.Debug("workid=" + drKeyidNow["id"] + " and imode='505'---" + myIU.sID);

                    clearKeyid();
                    clearPage();
                    btnGet.Text = "领取一卷";
                    btnGet.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("该份页" + pagename + "未完成");
            }
        }
    }
    private void btnPre_Click(object sender, EventArgs e)
    {
        toPrev();
        pl_main.Focus();
    }
    private void btnNext_Click(object sender, EventArgs e)
    {
        toNext();
        pl_main.Focus();
    }
    private void tb_nowpage_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)//Enter
        {
            int pagetemp = AciCvt.ToInt(tb_nowpage.Text.Trim());
            toPage(pagetemp);
        }
    }
    void xiutu_KeyUp(object sender, KeyEventArgs e)
    {
        //↑↓
        //MessageBox.Show(e.KeyValue + "");
        if (e.KeyValue == 83 && e.Modifiers.CompareTo(Keys.Control) == 0)//Ctrl + S 保存图像
        {
            toSave();
        }
        else if (e.KeyValue == 89)//Y 使用原图
        {
            tools.getAvgBmp(sPathFileOld);
            iRotate = 0;
            pl_main.Focus();
        }
        else if (e.KeyValue == 81)//Q 左旋转90
        {
            toRotate(-90, 0);
        }
        else if (e.KeyValue == 87)//W 翻转180
        {
            toRotate(180, 0);
        }
        else if (e.KeyValue == 69)//E 右旋转90
        {
            toRotate(90, 0);
        }
        else if (e.KeyValue == 66)//B 获取底色
        {
            tools.getAutoRGB();
        }
        else if (e.KeyValue == 72)//H 补页码
        {
            picMain.Cursor = Cursors.IBeam;
            tools.pageNumber = lbPage.Text;
        }
        else if (e.KeyValue == 90 && e.Modifiers.CompareTo(Keys.Control) == 0)//Ctrl + Z 后退一步
        {
            tools.toBack();
            picMain.Cursor = Cursors.Arrow;
        }
        else if (e.KeyValue == 78)//N 错误标记
        {
            if (list_error.Text == "错误")
            {
                list_error.Text = "正确";
                pl_MSG.Visible = false;
            }
            else if (list_error.Text == "正确")
            {
                list_error.Text = "错误";
                pl_MSG.Visible = true;
            }
        }
        else if (e.KeyValue == 86)//V 吸取底色
        {
            tools.getMyRGB();
        }
        else if (e.KeyValue == 83)//S 补色
        {
            int iZ = AciCvt.ToInt(tb_z.Text);
            if (iZ < 5) iZ = 5;
            tools.beginZ(iZ, false);
        }
        else if (e.KeyValue == 221)//] 放大印章
        {
            if (tools.bBeginZ)
            {
                int iZ = AciCvt.ToInt(tb_z.Text);
                iZ += 5;
                tb_z.Text = iZ.ToString();
                tools.beginZ(iZ, true);
            }
        }
        else if (e.KeyValue == 219)//[ 放大印章
        {
            if (tools.bBeginZ)
            {
                int iZ = AciCvt.ToInt(tb_z.Text);
                iZ -= 5;
                if (iZ < 5) iZ = 5;
                tb_z.Text = iZ.ToString();
                tools.beginZ(iZ, true);
            }
        }
        else if (e.KeyValue == 65)//A 左右切割
        {
            picMain.Cursor = Cursors.VSplit;
        }
        else if (e.KeyValue == 68)//D 上下切割
        {
            picMain.Cursor = Cursors.HSplit;
        }
        else if (e.KeyValue == 90)//Z 上一页
        {
            toPrev();
        }
        else if (e.KeyValue == 88)//X 下一页
        {
            toNext();
        }
    }

    private void button1_Click(object sender, EventArgs e)
    {
        pl_main.Focus();
    }
    private void Form1_Click(object sender, EventArgs e)
    {
        pl_main.Focus();
    }
    private void Form1_Shown(object sender, EventArgs e)
    {
        pl_main.Focus();
    }

    private void button2_Click(object sender, EventArgs e)
    {
        tools.getAvgBmp(sPathFileOld);
    }
    private void button8_Click(object sender, EventArgs e)
    {
        tools.CloseAll_gn();
        Bitmap bmp = (Bitmap)System.Drawing.Image.FromFile(sPathFileOld);
        tools.setBitmap(bmp, true);
    }

    private void button3_Click(object sender, EventArgs e)
    {
        tools.getCutDong(sPathFileOld);
    }

    private void btn180_Click(object sender, EventArgs e)
    {
        toRotate(180, 0);
    }

    private void btnLeft90_Click(object sender, EventArgs e)
    {
        toRotate(-90, 0);
    }

    private void btnRight90_Click(object sender, EventArgs e)
    {
        toRotate(90, 0);
    }

    private void button9_Click(object sender, EventArgs e)
    {
        toRotate(180, 1);
    }

    private void button11_Click(object sender, EventArgs e)
    {
        toRotate(-90, 1);
    }

    private void button10_Click(object sender, EventArgs e)
    {
        toRotate(90, 1);
    }

    private void BtnSave_Click(object sender, EventArgs e)
    {
        toSave();
    }

    /// <summary>
    /// 设置右侧原图
    /// </summary>
    /// <param name="path"></param>
    public void setBitmapFile(string path)
    {
        byte[] bytes = tools.getImageBytes(path);
        MemoryStream ms = new MemoryStream(bytes);
        picFile.SizeMode = PictureBoxSizeMode.Normal;
        Bitmap bmp = (Bitmap)Image.FromStream(ms);
        if (bmp.Height >= bmp.Width)
        {
            float pp = (float)bmp.Width / (float)bmp.Height;
            picFile.Height = pl_file.Height;
            picFile.Width = (int)Math.Round((float)picFile.Height * pp);
            picFile.Location = new Point((pl_file.Width - picFile.Width) / 2, 0);
        }
        else if (bmp.Width > bmp.Height)
        {
            float pp = (float)bmp.Height / (float)bmp.Width;
            picFile.Width = pl_file.Height;
            picFile.Height = (int)Math.Round((float)picFile.Width * pp);
            picFile.Location = new Point(0, (pl_file.Height - picFile.Height) / 2);
        }
        picFile.Image = bmp.GetThumbnailImage(picFile.Width, picFile.Height, null, IntPtr.Zero);
        ms.Dispose();
        ms.Close();
    }
}
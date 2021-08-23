using System;
using System.Collections;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Data;

public unsafe class OcrResult
{
    public saveImage imgSave;
    public Bitmap bmp;
    public myDataConn myDC = null;

    int iFrames = 0;

    /// <summary>四项点</summary>
    public resultDot[] arrResultDot = null;


    bool isAll = false;

    /// <summary>OCR 结果文字</summary>
    public string[] arrName = null;
    /// <summary>OCR 结果文字与匹配点数</summary>
    public int[] arrMin = null;
    /// <summary>比对字的所有点总和</summary>
    public double[] arrAll = null;
    /// <summary>相同且重合的点 占据 比对字的所有点总和 的比例</summary>
    public float[] arrPer = null;
    /// <summary>相同且重合的点 占据 比对字的所有点总和 的比例</summary>
    public Rect.OcrMode[] arrMode = null;

    /// <summary>调试文字比较 </summary>
    public string sDebugWord = "";

    /// <summary>中心点 X</summary>
    public int pCenter_X = 0;
    /// <summary>中心点 Y</summary>
    public int pCenter_Y = 0;
    /// <summary>调用的主结构</summary>
    public myOcrWord ocrWord;

    public Rect.OcrMode ocrMode;

    public int iCheckIndex = 0;

    public OcrResult(myDataConn mydc, saveImage imgsave, int iframes)
    {
        iFrames = iframes;
        myDC = mydc;
        imgSave = imgsave;
    }
    int[] sBidui = null;

    /// <summary>区域信息字符集</summary>
    public string sRectString = "";

    /// <summary>是否去除线段验证 默认为 true 识别率低时可以使用 false 全部识别</summary>
    public bool isCheckLine = true;

    /// <summary>开始识别</summary>
    public void Ocr(DataSet ds, Rect rect, myOcrWord ocrword, int icount, string sprocess, string swhere)
    {
        sRectString = rect.t + "," + rect.r + "," + rect.b + "," + rect.l;
        ocrWord = ocrword;
        ocrMode = rect.iOcrMode;

        
        //int[,] iCut = null;

        DataTable dt = ds.Tables[iFrames + ""];

        DataRow[] drs = null;
        if (ocrMode == Rect.OcrMode.Chinese)
        {
            drs = dt.Select("imode=0 and dot_0_count>=" + (arrResultDot[0].iCount - 100) + " and dot_0_count<=" + (arrResultDot[0].iCount + 100) + " " + swhere + "");
            //iCut = iCut_5;
        }
        else if (ocrMode == Rect.OcrMode.Int)
        {
            drs = dt.Select("imode=1 " + swhere);
            //iCut = iCut_4;
        }
        else if (ocrMode == Rect.OcrMode.English)
        {
            drs = dt.Select("imode=2 " + swhere);
            //iCut = iCut_4;
        }
        else if (ocrMode == Rect.OcrMode.Code)
        {
            drs = dt.Select("imode=3 " + swhere);
            //iCut = iCut_4;
        }
        else if (ocrMode == Rect.OcrMode.Bushou)
        {
            drs = dt.Select("imode=4 " + swhere);
            //iCut = iCut_4;
        }
        else if (ocrMode == Rect.OcrMode.CodeUp)
        {
            drs = dt.Select("imode=5 " + swhere);
            //iCut = iCut_4;
        }
        else if (ocrMode == Rect.OcrMode.CodeDown)
        {
            drs = dt.Select("imode=6 " + swhere);
            //iCut = iCut_4;
        }
        else if (ocrMode == Rect.OcrMode.Int_English)
        {
            drs = dt.Select("imode in(1,2) " + swhere);
            //iCut = iCut_4;
        }
        else if (ocrMode == Rect.OcrMode.Code_English)
        {
            drs = dt.Select("imode in(2,3) " + swhere);
            //iCut = iCut_4;
        }
        else if (ocrMode == Rect.OcrMode.Code_Int_English)
        {
            drs = dt.Select("imode in(1,2,3,5,6) " + swhere);
            //iCut = iCut_4;
        }
        else if (ocrMode == Rect.OcrMode.Code_Int_English_Bushou)
        {
            drs = dt.Select("imode in(1,2,3,4,5,6) " + swhere);
            //iCut = iCut_4;
        }


        //MessageBox.Show(drs.Length + " " + swhere);
        //int ic = iCut.GetUpperBound(0) + 1;

        arrName = new string[5];
        arrMin = new int[5];
        arrAll = new double[5];
        arrPer = new float[5];
        arrMode = new Rect.OcrMode[5];

        for (int i = 0; i < arrMin.Length; i++)
        {
            arrMin[i] = -1;
        }
        //MessageBox.Show("imode=0 and dot_0_count>=" + (arrResultDot[0].iCount - 100) + " and dot_0_count<=" + (arrResultDot[0].iCount + 100) + " " + drs.Length);

        float width_per = (float)rect.width / (float)rect.height;
        float height_per = 1;

        iCheckIndex = 0;
        for (int k = 0; k < drs.Length; k++)
        {
            DataRow dr = drs[k];
            
            string smode = dr["imode"].ToString();
            
            float thisWidth = 1;
            float thisHeight = 1;
            if (ocrMode == Rect.OcrMode.Code_Int_English_Bushou || ocrMode == Rect.OcrMode.Code_Int_English || ocrMode == Rect.OcrMode.Int || ocrMode == Rect.OcrMode.Int_English)
            {
                thisWidth = (float)Convert.ToDouble(dr["width"]);
                thisHeight = (float)Convert.ToDouble(dr["height"]);

                float icc = 0.2f;
                if (dr["name"].ToString().Substring(0, 1) == "7")
                {
                    icc = 0.15f;
                }

                if (Math.Abs(width_per - thisWidth) >= icc)
                {
                    if (width_per < 5 && thisWidth < 5)
                    {
                        continue;
                    }
                }
            }
            //if (ocrMode == Rect.OcrMode.Chinese)
            //{
            //    MessageBox.Show(width_per + " " + thisWidth + " " + height_per + " " + thisHeight + " ");
            //}
            string name = dr["name"].ToString();
            
            
            resultDot[] arr_dot = new resultDot[5];
            for (int i = 0; i < arrResultDot.Length; i++)
            {
                arr_dot[i] = new resultDot();
                arr_dot[i].sString = new StringBuilder(dr["dot_" + i + "_str"].ToString());
                arr_dot[i].iCount = Convert.ToInt32(dr["dot_" + i + "_count"]);
                arr_dot[i].X = Convert.ToInt32(dr["dot_" + i + "_x"]);
                arr_dot[i].Y = Convert.ToInt32(dr["dot_" + i + "_y"]);
                if (i != 0)
                {
                    arr_dot[i].X2 = Convert.ToInt32(dr["dot_" + i + "_x2"]);
                    arr_dot[i].Y2 = Convert.ToInt32(dr["dot_" + i + "_y2"]);
                    arr_dot[i].X3 = Convert.ToInt32(dr["dot_" + i + "_x3"]);
                    arr_dot[i].Y3 = Convert.ToInt32(dr["dot_" + i + "_y3"]);
                }
                arr_dot[i].W = Convert.ToInt32(dr["dot_" + i + "_w"]);
                arr_dot[i].H = Convert.ToInt32(dr["dot_" + i + "_h"]);
                if (i == 0)
                {
                    arr_dot[i].lineX_30_str = new StringBuilder(dr["lineX_30_str"].ToString());
                    arr_dot[i].lineX_50_str = new StringBuilder(dr["lineX_50_str"].ToString());
                    arr_dot[i].lineX_90_str = new StringBuilder(dr["lineX_90_str"].ToString());
                    arr_dot[i].lineY_30_str = new StringBuilder(dr["lineY_30_str"].ToString());
                    arr_dot[i].lineY_50_str = new StringBuilder(dr["lineY_50_str"].ToString());
                    arr_dot[i].lineY_90_str = new StringBuilder(dr["lineY_90_str"].ToString());
                }
            }

            if (name.Length == 2 && (smode == "2" || smode == "0" || smode == "1"))
            {
                name = name[0].ToString();
            }

            resultDot dot_all = arr_dot[0];
            resultDot dot_now = arrResultDot[0];
            dot_all.ReInit_Line();

            //检查线段
            bool checkOK = false;
            bool isShow = false;

            if (sDebugWord != "")
            {
                if (sDebugWord.IndexOf(name) > -1)// && ocrMode == Rect.OcrMode.Chinese
                {
                    isShow = true;
                    //样本图片中心点
                    createBmp(dot_all.sString, dot_all.W, new Rectangle(0, 0, dot_all.W, dot_all.H), Application.StartupPath + "/test/11" + name + "_中心点.jpg", dot_all.X, dot_all.Y);
                    //当前图片中心点
                    createBmp(dot_now.sString, dot_now.W, new Rectangle(0, 0, dot_now.W, dot_now.H), Application.StartupPath + "/test/11当前_中心点.jpg", dot_now.X, dot_now.Y);

                    //MessageBox.Show(dot_now.DebugLine() + "\r\n \r\n" + dot_all.DebugLine());
                }
            }

            if (ocrMode == Rect.OcrMode.Chinese && isCheckLine)
            {
                checkOK = dot_all.checkLineX_90(dot_now.AL_lineX_90, 0.15f); if (!checkOK) continue; if (isShow) MessageBox.Show("X90_1");
                checkOK = dot_all.checkLineY_90(dot_now.AL_lineY_90, 0.15f); if (!checkOK) continue; if (isShow) MessageBox.Show("Y90_1");
                checkOK = dot_now.checkLineX_90(dot_all.AL_lineX_90, 0.15f); if (!checkOK) continue; if (isShow) MessageBox.Show("X90_2");
                checkOK = dot_now.checkLineY_90(dot_all.AL_lineY_90, 0.15f); if (!checkOK) continue; if (isShow) MessageBox.Show("Y90_2");

                checkOK = dot_all.checkLineX_50(dot_now.AL_lineX_50, 0.15f); if (!checkOK) continue; if (isShow) MessageBox.Show("X50_1");
                checkOK = dot_all.checkLineY_50(dot_now.AL_lineY_50, 0.15f); if (!checkOK) continue; if (isShow) MessageBox.Show("Y50_1");
                checkOK = dot_now.checkLineX_50(dot_all.AL_lineX_50, 0.15f); if (!checkOK) continue; if (isShow) MessageBox.Show("X50_2");
                checkOK = dot_now.checkLineY_50(dot_all.AL_lineY_50, 0.15f); if (!checkOK) continue; if (isShow) MessageBox.Show("Y50_2");
            }


            int iC_all = 0;
            float iLikePer_all = 0;

            for (int f = 1; f < arrResultDot.Length; f++)
            {
                Rectangle rectframe = new Rectangle();
                if (f == 1)
                {
                    rectframe = new Rectangle(0, 0, arr_dot[f].W, arr_dot[f].H);
                }
                else if (f == 2)
                {
                    rectframe = new Rectangle(dot_all.X, 0, arr_dot[f].W, arr_dot[f].H);
                }
                else if (f == 3)
                {
                    rectframe = new Rectangle(0, dot_all.Y, arr_dot[f].W, arr_dot[f].H);
                }
                else if (f == 4)
                {
                    rectframe = new Rectangle(dot_all.X, dot_all.Y, arr_dot[f].W, arr_dot[f].H);
                }

                int iC_min = 0;
                float iLikePer_max = 0;

                ArrayList AL_point = new ArrayList();

                resultLike rLike = null;

                for (int q = 0; q <= 3; q++)
                {
                    int iCha_x = 0;
                    int iCha_y = 0;

                    if (q == 1)
                    {
                        iCha_x = arrResultDot[f].X - arr_dot[f].X;
                        iCha_y = arrResultDot[f].Y - arr_dot[f].Y;
                    }
                    else if (q == 2)
                    {
                        iCha_x = arrResultDot[f].X2 - arr_dot[f].X2;
                        iCha_y = arrResultDot[f].Y2 - arr_dot[f].Y2;
                    }
                    else if (q == 3)
                    {
                        iCha_x = arrResultDot[f].X3 - arr_dot[f].X3;
                        iCha_y = arrResultDot[f].Y3 - arr_dot[f].Y3;
                    }
                    //偏移设置
                    if (f == 2 || f == 4)
                    {
                        iCha_x += arr_dot[f].W - arrResultDot[f].W;
                    }
                    if (f == 3 || f == 4)
                    {
                        iCha_y += arr_dot[f].H - arrResultDot[f].H;
                    }

                    bool isExsit = false;
                    for (int n = 0; n < AL_point.Count; n++)
                    {
                        Point point = (Point)AL_point[n];
                        if (point.X == iCha_x && point.Y == iCha_y)
                        {
                            isExsit = true;
                            break;
                        }
                    }

                    StringBuilder dot_frame = null;

                    //移动的中心点不存在时
                    if (!isExsit)
                    {
                        AL_point.Add(new Point(iCha_x, iCha_y));

                        dot_frame = new StringBuilder();
                        for (int j = 0; j < dot_all.H; j++)
                        {
                            for (int i = 0; i < dot_all.W; i++)
                            {
                                int ix = i + iCha_x;
                                int iy = j + iCha_y;

                                if (iy >= 0 && iy < dot_now.H && ix >= 0 && ix < dot_now.W)
                                {
                                    if (dot_now.sString[iy * dot_now.W + ix] == '1')
                                    {
                                        dot_frame.Append('1');
                                    }
                                    else
                                    {
                                        dot_frame.Append('0');
                                    }
                                }
                                else
                                {
                                    dot_frame.Append('0');
                                }
                            }
                        }

                        //AciDebug.Debug(dot_all.sString.ToString());
                        //AciDebug.Debug(dot_frame.ToString());

                        //createBmp(dot_frame, dot_all.W, dot_all.H, Application.StartupPath + "/test/move_" + iCha_x + "_" + iCha_y + ".jpg");
                        //MessageBox.Show("a");


                        sBidui = new int[dot_all.W * dot_all.H];

                        rLike = new resultLike();
                        checkLike(dot_frame, dot_all.sString, ref rLike, dot_all.W, dot_all.H, rectframe, 0);
                        checkLike(dot_all.sString, dot_frame, ref rLike, dot_all.W, dot_all.H, rectframe, 1);
                        if (rLike.iAll_dot != 0) rLike.iLikePer = rLike.iLike / rLike.iAll_dot;

                        if (q == 0)
                        {
                            iC_min = rLike.iC_all;
                            iLikePer_max = rLike.iLikePer;
                        }
                        else
                        {
                            if (rLike.iC_all < iC_min) iC_min = rLike.iC_all;
                            if (rLike.iLikePer > iLikePer_max) iLikePer_max = rLike.iLikePer;
                        }


                        //if (name == "亩" && ocrMode == Rect.OcrMode.Chinese)
                        //{
                        //    //MessageBox.Show(dot_frame.ToString() + " " + rectframe.Width + " " + rectframe.Height);
                        //    //MessageBox.Show(arr_dot[f].sString.Length + "  " + arr_dot[f].W + " " + arr_dot[f].H);
                        //    //MessageBox.Show(rectframe.X + " " + rectframe.Y + " " + rectframe.Width + " " + rectframe.Height);



                        //    //当前图片
                        //    //createBmp(dot_frame, dot_all.W, new Rectangle(0, 0, dot_all.W, dot_all.H), Application.StartupPath + "/test/00当前_" + f + "_" + q + ".jpg", 0, 0);
                        //    //样本图片
                        //    //createBmp(dot_all.sString, dot_all.W, new Rectangle(0, 0, dot_all.W, dot_all.H), Application.StartupPath + "/test/00样本_" + f + "_" + q + ".jpg", 0, 0);

                        //    if (q == 0)
                        //    {
                        //        //当前图片 移动
                        //        createBmp(dot_frame, dot_all.W, rectframe, Application.StartupPath + "/test/00当前移动_" + f + "_" + q + ".jpg", arr_dot[f].X, arr_dot[f].Y);
                        //        //当前图片 原始
                        //        createBmp(arrResultDot[f].sString, arrResultDot[f].W, new Rectangle(0, 0, arrResultDot[f].W, arrResultDot[f].H), Application.StartupPath + "/test/00当前原始_" + f + "_" + q + ".jpg", arrResultDot[f].X, arrResultDot[f].Y);
                        //        //比对样本
                        //        createBmp(arr_dot[f].sString, rectframe.Width, new Rectangle(0, 0, rectframe.Width, rectframe.Height), Application.StartupPath + "/test/00" + name + "_" + f + "_" + q + ".jpg", arrResultDot[f].X, arrResultDot[f].Y);
                        //    }
                        //    else
                        //    {
                        //        //当前图片 移动
                        //        createBmp(dot_frame, dot_all.W, rectframe, Application.StartupPath + "/test/00当前移动_" + f + "_" + q + ".jpg", arr_dot[f].X2, arr_dot[f].Y2);
                        //        //当前图片 原始
                        //        createBmp(arrResultDot[f].sString, arrResultDot[f].W, new Rectangle(0, 0, arrResultDot[f].W, arrResultDot[f].H), Application.StartupPath + "/test/00当前原始_" + f + "_" + q + ".jpg", arrResultDot[f].X2, arrResultDot[f].Y2);
                        //        //比对样本
                        //        createBmp(arr_dot[f].sString, rectframe.Width, new Rectangle(0, 0, rectframe.Width, rectframe.Height), Application.StartupPath + "/test/00" + name + "_" + f + "_" + q + ".jpg", arrResultDot[f].X2, arrResultDot[f].Y2);
                        //    }

                        //    //MessageBox.Show(arrResultDot[f].X3 + " " + arr_dot[f].X3 + " " + arrResultDot[f].Y3 + " " + arr_dot[f].Y3);

                        //    MessageBox.Show("【" + f + "-" + q + "】【当前：" + rLike.iC_all + "】【同级最小：" + iC_min + "】【总计：" + iC_all + "】【" + iCha_x + "】【" + iCha_y + "】");
                        //}
                    }

                    if (rLike.iC_all == 0)
                    {
                        //是 0 直接不用跑了
                        iC_all += iC_min;
                        iLikePer_all += iLikePer_max;
                        break;
                    }
                    else
                    {
                        if (q == 3)
                        {
                            iC_all += iC_min;
                            iLikePer_all += iLikePer_max;
                        }
                    }
                }
            }

            iC_all /= 4;
            iLikePer_all /= 4;
            

            for (int i = 0; i < arrMin.Length; i++)
            {
                if (arrMin[i] == -1)
                {
                    arrName[i] = name;
                    arrMin[i] = iC_all;
                    arrAll[i] = arr_dot[0].iCount;
                    arrPer[i] = (float)Math.Round(iLikePer_all, 3);
                    arrMode[i] = (Rect.OcrMode)AciCvt.ToInt(dr["imode"]);

                    break;
                }
                else if (arrMin[i] > iC_all)
                {
                    for (int j = arrMin.Length - 1; j > i; j--)
                    {
                        arrName[j] = arrName[j - 1];
                        arrMin[j] = arrMin[j - 1];
                        arrAll[j] = arrAll[j - 1];
                        arrPer[j] = arrPer[j - 1];
                        arrMode[j] = arrMode[j - 1];
                    }
                    arrName[i] = name;
                    arrMin[i] = iC_all;
                    arrAll[i] = arr_dot[0].iCount;
                    arrPer[i] = (float)Math.Round(iLikePer_all, 3);
                    arrMode[i] = (Rect.OcrMode)AciCvt.ToInt(dr["imode"]);

                    break;
                }

            }

            //if (name == "耳右" && icount == 34)
            //{
            //    MessageBox.Show(DebugResult());
            //}

            iCheckIndex++;
            //AciDebug.Debug(arrAll[0] + "");
            //AciDebug.Debug(name + " " + iC_all);
        }

        sResultWord = arrName[0];

        if (ocrWord.form != null)
        {
            string sdebug = "【" + icount + "】【" + arrResultDot[0].iCount + "】结果：(" + DebugResult() + ") " + sprocess;
            //AciDebug.Debug(sdebug);
            //ocrWord.form.BeginInvoke(ocrWord.form.IK_lb, new Object[] { sdebug });
        }
    }
    public void createBitmap(string name)
    {
        createBmp(arrResultDot[0].sString, arrResultDot[0].W, new Rectangle(0, 0, arrResultDot[0].W, arrResultDot[0].H), Application.StartupPath + "/test/" + name + ".jpg", arrResultDot[0].X, arrResultDot[0].Y);
    }
    public void createBmp(StringBuilder sdot, int dotWidth, Rectangle rectframe, string path, int iC_x, int iC_y)
    {
        Bitmap bitmap = new Bitmap(rectframe.Width, rectframe.Height);

        BitmapData bitData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        uint bmp_ic = (uint)(bitData.Stride - bitData.Width * 3);
        byte* bmp_p = (byte*)(bitData.Scan0.ToPointer());

        uint bmp_w = (uint)bitData.Width;
        uint bmp_h = (uint)bitData.Height;

        //一行的像素点
        uint bmp_iWidth = bmp_w * 3 + bmp_ic;

        for (uint y = 0; y < bmp_h; y++)
        {
            for (uint x = 0; x < bmp_w; x++)
            {
                char c = sdot[(int)((y + rectframe.Y) * dotWidth + (x + rectframe.X))];
                if (c == '0')
                {
                    uint ib = (uint)(y * bmp_iWidth + x * 3);
                    bmp_p[ib] = bmp_p[ib + 1] = bmp_p[ib + 2] = 255;
                }
                else if (c == '2')
                {
                    uint ib = (uint)(y * bmp_iWidth + x * 3);
                    bmp_p[ib] = 0;
                    bmp_p[ib + 1] = 255;
                    bmp_p[ib + 2] = 0;
                }
                else if (c == '3')
                {
                    uint ib = (uint)(y * bmp_iWidth + x * 3);
                    bmp_p[ib] = 0;
                    bmp_p[ib + 1] = 0;
                    bmp_p[ib + 2] = 255;
                }
            }
        }

        if (iC_x >= 0 && iC_x < bmp_w && iC_y >= 0 && iC_y < bmp_h)
        {
            uint ibb = (uint)(iC_y * bmp_iWidth + iC_x * 3);
            if (bmp_p[ibb] == 0)
            {
                bmp_p[ibb] = 255;
                bmp_p[ibb + 1] = 255;
                bmp_p[ibb + 2] = 255;
            }
            else
            {
                bmp_p[ibb] = 0;
                bmp_p[ibb + 1] = 0;
                bmp_p[ibb + 2] = 0;
            }
        }

        bitmap.UnlockBits(bitData);
        imgSave.Save(bitmap, path);
    }
    /// <summary>
    /// 获取所有结果    字,最小值,相似度,总点数;字,最小值,相似度,总点数
    /// </summary>
    /// <returns></returns>
    public string getResultString()
    {
        string s = "";
        for (int i = 0; i < arrMin.Length; i++)
        {
            if (s != "")
            {
                s += ";";
            }
            s += arrName[i] + "," + arrMin[i] + "," + arrPer[i] + "," + arrAll[i] + "";
        }
        return s;
    }
    /// <summary>
    /// 获取所有结果 相近的 结果 用 【】 标注
    /// </summary>
    /// <returns></returns>
    public string DebugResult()
    {
        string s = "";
        for (int i = 0; i < arrMin.Length; i++)
        {
            string name = arrName[i];
            if (i == 0)
            {
                if ((float)arrMin[1] / (float)arrMin[0] < 2 && arrMin[1] > -1)
                {
                    name = "【" + name + "】";
                }
            }
            else if (i < arrMin.Length - 1)
            {
                if ((float)arrMin[i] / (float)arrMin[0] < 2 && arrMin[1] > -1)
                {
                    name = "【" + name + "】";
                }
            }
            s += name + "(" + arrMin[i] + "," + arrPer[i] + "," + arrAll[i] + ") ";
        }
        return s;
    }
    /// <summary>结果文字</summary>
    public string sResultWord = "";
    /// <summary>
    /// 获取 结果 有相似的用 【直,宣,宜】 否则返回直接的文字
    /// </summary>
    /// <returns></returns>
    public string getResult()
    {
        //直接返回结果
        //if (sResultWord != "") return sResultWord;
        if (arrMin[0] == -1) return "　";

        string s = "";
        //if (ocrMode == Rect.OcrMode.Chinese)
        //{
        for (int i = 0; i < arrMin.Length; i++)
        {
            //if (arrMin[0] > 20)
            //{
            if (i == 0)
            {
                if ((float)arrMin[1] / (float)arrMin[0] < 2 && arrMin[1] > -1 && (arrMin[1] - arrMin[0]) < 300)
                {
                    s += "" + arrName[0] + "";
                }
            }
            else if (i < arrMin.Length - 1)
            {
                if ((float)arrMin[i] / (float)arrMin[0] < 2 && arrMin[i] > -1 && (arrMin[i] - arrMin[0]) < 300)
                {
                    s += "," + arrName[i] + "";
                }
            }
            //}
        }
        if (s == "")
        {
            s = arrName[0];
        }
        else
        {
            s = "【" + s + "】";
        }
            
        return s;
    }

    
    /// <summary>网格化分析 多因子 分析</summary>
    public void get_iCut(byte* bmp_p, uint bmp_w, uint bmp_h, uint bmp_iWidth, int ithisW, int ithisH)
    {
        arrResultDot = new resultDot[5];
        set_iCut0(ref arrResultDot[0], 0, bmp_p, 0, 0, bmp_w, bmp_h, bmp_iWidth);
        arrResultDot[0].ReInit_Line();

        set_iCut1234(ref arrResultDot[1], 1, bmp_p, 0, 0, (uint)arrResultDot[0].X, (uint)arrResultDot[0].Y, bmp_iWidth);
        set_iCut1234(ref arrResultDot[2], 2, bmp_p, (uint)arrResultDot[0].X, 0, bmp_w, (uint)arrResultDot[0].Y, bmp_iWidth);
        set_iCut1234(ref arrResultDot[3], 3, bmp_p, 0, (uint)arrResultDot[0].Y, (uint)arrResultDot[0].X, bmp_h, bmp_iWidth);
        set_iCut1234(ref arrResultDot[4], 4, bmp_p, (uint)arrResultDot[0].X, (uint)arrResultDot[0].Y, bmp_w, bmp_h, bmp_iWidth);
    }
    public void set_iCut0(ref resultDot rDot, int index, byte* bmp_p, uint iX, uint iY, uint iW, uint iH, uint bmp_iWidth)
    {
        rDot = new resultDot();
        float ix = 0;
        float iy = 0;

        bool isShow = false;

        ArrayList AL_ClearH = new ArrayList();
        ArrayList AL_ClearV = new ArrayList();
        SmallLineX_Only(ref AL_ClearH, bmp_p, iW, iH, bmp_iWidth);
        SmallLineY_Only(ref AL_ClearV, bmp_p, iW, iH, bmp_iWidth);

        for (uint y = iY; y < iH; y++)
        {
            for (uint x = iX; x < iW; x++)
            {
                uint ib = y * bmp_iWidth + x * 3;
                if (bmp_p[ib] == 0)
                {
                    rDot.iCount++;
                    rDot.sString.Append('1');

                    ix += (float)x;
                    iy += (float)y;
                }
                else
                {
                    rDot.sString.Append('0');
                }
            }
        }
       
        for (int i = 0; i < AL_ClearV.Count; i++)
        {
            ArrayList AL_Now = (ArrayList)AL_ClearV[i];
            for (int j = 0; j < AL_Now.Count; j++)
            {
                Rect rect = (Rect)AL_Now[j];
                if (!rect.isHB)
                {
                    float iperLong = (float)rect.height / (float)iH;
                    float iper = (float)rect.l / (float)iW;
                    if (iperLong >= 0.9)
                    {
                        rDot.lineY_90_count++;
                        rDot.lineY_90_str.Append(iperLong + "," + iper + ";" + rect.l + "," + rect.t + "," + rect.width + "," + rect.height + "|");
                        if (isShow) rect.Draw(bmp_p, bmp_iWidth, 0, 0, 255);
                    }
                    else if (iperLong >= 0.5)
                    {
                        rDot.lineY_50_count++;
                        rDot.lineY_50_str.Append(iperLong + "," + iper + ";" + rect.l + "," + rect.t + "," + rect.width + "," + rect.height + "|");

                        if (isShow) rect.Draw(bmp_p, bmp_iWidth, 0, 255, 0);
                    }
                    else if (iperLong >= 0.3)
                    {
                        rDot.lineY_30_count++;
                        rDot.lineY_30_str.Append(iperLong + "," + iper + ";" + rect.l + "," + rect.t + "," + rect.width + "," + rect.height + "|");

                        if (isShow) rect.Draw(bmp_p, bmp_iWidth, 255, 0, 0);
                    }
                    //rect.Draw(bmp_p, bmp_iWidth);
                    //MessageBox.Show(rect.DebugString());
                }
            }
        }
        for (int i = 0; i < AL_ClearH.Count; i++)
        {
            ArrayList AL_Now = (ArrayList)AL_ClearH[i];
            for (int j = 0; j < AL_Now.Count; j++)
            {
                Rect rect = (Rect)AL_Now[j];
                if (!rect.isHB)
                {
                    float iperLong = (float)rect.width / (float)iW;
                    float iper = (float)rect.t / (float)iH;
                    if (iperLong >= 0.9)
                    {
                        rDot.lineX_90_count++;
                        rDot.lineX_90_str.Append(iperLong + "," + iper + ";" + rect.l + "," + rect.t + "," + rect.width + "," + rect.height + "|");
                        if (isShow) rect.Draw(bmp_p, bmp_iWidth, 0, 0, 255);
                    }
                    else if (iperLong >= 0.5)
                    {
                        rDot.lineX_50_count++;
                        rDot.lineX_50_str.Append(iperLong + "," + iper + ";" + rect.l + "," + rect.t + "," + rect.width + "," + rect.height + "|");

                        if (isShow) rect.Draw(bmp_p, bmp_iWidth, 0, 255, 0);
                    }
                    else if (iperLong >= 0.3)
                    {
                        rDot.lineX_30_count++;
                        rDot.lineX_30_str.Append(iperLong + "," + iper + ";" + rect.l + "," + rect.t + "," + rect.width + "," + rect.height + "|");

                        if (isShow) rect.Draw(bmp_p, bmp_iWidth, 255, 0, 0);
                    }
                    //rect.Draw(bmp_p, bmp_iWidth);
                    //MessageBox.Show(rect.DebugString());
                }
            }
        }

        rDot.X2 = (int)Math.Round(ix / (float)rDot.iCount - iX);
        rDot.Y2 = (int)Math.Round(iy / (float)rDot.iCount - iY);
        rDot.X = rDot.X2;
        rDot.Y = rDot.Y2;

        if (isShow) DrawDot(bmp_p, rDot.X2, rDot.Y2, bmp_iWidth, 2);

        //MessageBox.Show(rDot.X + " " + rDot.Y);

        rDot.W = (int)(iW - iX);
        rDot.H = (int)(iH - iY);


        //imgSave.SaveNoDispose(bmp, Application.StartupPath + "/test/" + index + ".jpg");

        //createBmp(rDot.sString, rDot.W, new Rectangle(0, 0, rDot.W, rDot.H), Application.StartupPath + "/test/" + index + ".jpg", rDot.X2, rDot.Y2);
    }

    void DrawDot(byte* bmp_p, int iX, int iY, uint bmp_iWidth, int N)
    {
        for (int y = iY - N; y <= iY + N; y++)
        {
            for (int x = iX - N; x <= iX + N; x++)
            {
                uint ib = (uint)y * bmp_iWidth + (uint)x * 3;
                bmp_p[ib] = 0;
                bmp_p[ib + 1] = 0;
                bmp_p[ib + 2] = 255;
            }
        }
    }

    public void set_iCut1234(ref resultDot rDot, int index, byte* bmp_p, uint iX, uint iY, uint iW, uint iH, uint bmp_iWidth)
    {
        rDot = new resultDot();

        float ix = 0;
        float iy = 0;

        float ix2 = 0;
        float iy2 = 0;

        float ix3 = 0;
        float iy3 = 0;
        float icount2 = 0;
        
        float x_helf = (float)iW/2;
        float y_helf = (float)iH/2;
        float icount3 = 0;

        for (uint y = iY; y < iH; y++)
        {
            for (uint x = iX; x < iW; x++)
            {
                uint ib = y * bmp_iWidth + x * 3;
                if (bmp_p[ib] == 0)
                {
                    rDot.iCount++;
                    rDot.sString.Append('1');

                    ix += (float)x;
                    iy += (float)y;
                }
                else
                {
                    rDot.sString.Append('0');
                }


                if (bmp_p[ib] == 0)
                {
                    if (index == 1 && (x > iW - 3 || y > iH - 3))
                    {
                        //左上
                    }
                    else if (index == 2 && (x < iX + 3 || y > iH - 3))
                    {
                        //右上
                    }
                    else if (index == 3 && (x > iW - 3 || y < iY + 3))
                    {
                        //左下
                    }
                    else if (index == 4 && (x < iX + 3 || y < iY + 3))
                    {
                        //右下
                    }
                    else
                    {
                        //9格权重
                        float idot = getDot_3_3_frame(bmp_p, (int)x, (int)y, iW, iH, bmp_iWidth, 1);
                        //离中心点权重
                        //float xx = x_helf - Math.Abs((float)x - x_helf);
                        //float yy = y_helf - Math.Abs((float)y - y_helf);

                        //idot += (xx + yy) / 4;

                        ix2 += (float)x * idot;
                        iy2 += (float)y * idot;
                        icount2 += idot;
                    }

                    if (index == 1 && (x > iW - 6 || y > iH - 6))
                    {
                        //左上
                    }
                    else if (index == 2 && (x < iX + 6 || y > iH - 6))
                    {
                        //右上
                    }
                    else if (index == 3 && (x > iW - 6 || y < iY + 6))
                    {
                        //左下
                    }
                    else if (index == 4 && (x < iX + 6 || y < iY + 6))
                    {
                        //右下
                    }
                    else
                    {
                        //9格权重
                        float idot = getDot_3_3_frame(bmp_p, (int)x, (int)y, iW, iH, bmp_iWidth, 1);
                        //离中心点权重
                        //float xx = x_helf - Math.Abs((float)x - x_helf);
                        //float yy = y_helf - Math.Abs((float)y - y_helf);

                        //idot += (xx + yy) / 4;

                        ix3 += (float)x * idot;
                        iy3 += (float)y * idot;
                        icount3 += idot;
                    }
                }
            }
        }

        //MessageBox.Show(rDot.sString.Length + "");

        if (rDot.iCount == 0)
        {
            rDot.X = (int)(iX + iW / 4);
            rDot.Y = (int)(iY + iH / 4);
        }
        else
        {
            rDot.X = (int)Math.Round(ix / (float)rDot.iCount - iX);
            rDot.Y = (int)Math.Round(iy / (float)rDot.iCount - iY);
        }

        if (icount2 == 0)
        {
            rDot.X2 = (int)(iX + iW / 4);
            rDot.Y2 = (int)(iY + iH / 4);
        }
        else
        {
            rDot.X2 = (int)Math.Round(ix2 / icount2 - iX);
            rDot.Y2 = (int)Math.Round(iy2 / icount2 - iY);
        }

        if (icount3 == 0)
        {
            rDot.X3 = (int)(iX + iW / 4);
            rDot.Y3 = (int)(iY + iH / 4);
        }
        else
        {
            rDot.X3 = (int)Math.Round(ix3 / icount3 - iX);
            rDot.Y3 = (int)Math.Round(iy3 / icount3 - iY);
        }

        rDot.W = (int)(iW - iX);
        rDot.H = (int)(iH - iY);


        //createBmp(rDot.sString, rDot.W, new Rectangle(0, 0, rDot.W, rDot.H), Application.StartupPath + "/test/" + index + ".jpg", rDot.X2, rDot.Y2);
    }
    float getDot_3_3_frame(byte* bmp_p, int iX, int iY, uint iW, uint iH, uint bmp_iWidth, int N)
    {
        float iDot = 0;
        for (int y = iY - N; y <= iY + N; y++)
        {
            for (int x = iX - N; x <= iX + N; x++)
            {
                if (x >= 0 && x < iW && y >= 0 && y < iH)
                {
                    uint ib = (uint)(y * bmp_iWidth + x * 3);
                    if (bmp_p[ib] == 0)
                    {
                        iDot++;
                    }
                }
            }
        }
        return iDot;
    }
    
    /// <summary>插入训练样本</summary>
    public void createWords(string sWord, Rect.OcrMode ocrMode, float width, float height)
    {
        string stable = "words";
        //ArrayList AL_db = myDC.getDatalistString(stable);

        string swhere = "";
        string sinsert = "";
        string supdate = "";
        if (ocrMode == Rect.OcrMode.Chinese_ts)
        {
            string[] arr = sWord.Split('_');
            swhere = "name='" + arr[0] + "' and imode=0 and its=" + arr[1] + "";
            supdate = "imode=0, its=" + arr[1] + "";
            sinsert = "('" + arr[0] + "', 0, " + arr[1] + ")";
        }
        else if (ocrMode == Rect.OcrMode.Int_ts)
        {
            string[] arr = sWord.Split('_');
            swhere = "name='" + arr[0] + "' and imode=1 and its=" + arr[1] + "";
            supdate = "imode=1, its=" + arr[1] + "";
            sinsert = "('" + arr[0] + "', 1, " + arr[1] + ")";
        }
        else
        {
            swhere = "name='" + sWord + "' and imode=" + (int)ocrMode + " and its=0";
            supdate = "imode=" + (int)ocrMode + ", its=0";
            sinsert = "('" + sWord + "', " + (int)ocrMode + ", 0)";
        }



        string words = sWord;
        if ("ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(sWord) > -1)
        {
            words = sWord + sWord;
        }

        if (myDC.Fc_sqlReturnString("select id from " + stable + " where " + swhere + "") == "")
        {
            myDC.sqlExecute("insert into " + stable + "(name, imode, its) values" + sinsert + "");
        }

        string sql = "update " + stable + " set ";

        string s = "";
        if (ocrMode != Rect.OcrMode.Chinese)
        {
            s += "width=" + width + ",";
            s += "height=" + height + ",";
        }

        s += "lineX_30_count=" + arrResultDot[0].lineX_30_count + ",";
        s += "lineX_30_str='" + arrResultDot[0].lineX_30_str + "',";
        s += "lineX_50_count=" + arrResultDot[0].lineX_50_count + ",";
        s += "lineX_50_str='" + arrResultDot[0].lineX_50_str + "',";
        s += "lineX_90_count=" + arrResultDot[0].lineX_90_count + ",";
        s += "lineX_90_str='" + arrResultDot[0].lineX_90_str + "',";

        s += "lineY_30_count=" + arrResultDot[0].lineY_30_count + ",";
        s += "lineY_30_str='" + arrResultDot[0].lineY_30_str + "',";
        s += "lineY_50_count=" + arrResultDot[0].lineY_50_count + ",";
        s += "lineY_50_str='" + arrResultDot[0].lineY_50_str + "',";
        s += "lineY_90_count=" + arrResultDot[0].lineY_90_count + ",";
        s += "lineY_90_str='" + arrResultDot[0].lineY_90_str + "',";


        for (int i = 0; i < arrResultDot.Length; i++)
        {
            s += "dot_" + i + "_str='" + arrResultDot[i].sString + "',";
            s += "dot_" + i + "_count='" + arrResultDot[i].iCount + "',";
            s += "dot_" + i + "_x=" + arrResultDot[i].X + ",";
            s += "dot_" + i + "_y=" + arrResultDot[i].Y + ",";
            s += "dot_" + i + "_x2=" + arrResultDot[i].X2 + ",";
            s += "dot_" + i + "_y2=" + arrResultDot[i].Y2 + ",";
            if (i != 0)
            {
                s += "dot_" + i + "_x3=" + arrResultDot[i].X3 + ",";
                s += "dot_" + i + "_y3=" + arrResultDot[i].Y3 + ",";
            }
            s += "dot_" + i + "_w=" + arrResultDot[i].W + ",";
            s += "dot_" + i + "_h=" + arrResultDot[i].H + ",";
        }
        s += supdate;

        sql += s + " where " + swhere + "";
        //AciDebug.Debug(sql);
        myDC.sqlExecute(sql);
    }

    public void SaveBmp(string name)
    {
        bmp.Save(Application.StartupPath + "/test/" + name + ".gif", ImageFormat.Gif);
        //imgSave.SaveNoDispose(bmp, );
    }

    public class resultLike
    {
        public int iC_all = 0;
        public float iLike = 0;
        public float iAll_dot = 0;
        public float iLikePer = 0;
    }
    void checkLike(StringBuilder dot_all_1, StringBuilder dot_all_2, ref resultLike rLike, int iW, int iH, Rectangle rectframe, int iAgain)
    {
        int itimes = iH;
        if (iW > iH)
        {
            itimes = iW;
        }
        int iC_all = 0;
        for (int i = 0; i < dot_all_1.Length; i++)
        {
            int x = i % iW;
            int y = (i - x) / iW;

            if (x >= rectframe.X && x < rectframe.X + rectframe.Width && y >= rectframe.Y && y < rectframe.Y + rectframe.Height)
            {
                if (iAgain == 0 || sBidui[i] == 0)
                {
                    char cc = dot_all_1[i];
                    if (cc != '0')
                    {
                        rLike.iAll_dot++;
                        //MessageBox.Show(i + " " + x + " " + y + " " + iW + " " + iH);
                        int thisIC = -1;
                        if (dot_all_2[y * iW + x] != '0')
                        {
                            //第一个点相和 距离为 0
                            thisIC = 0;
                            rLike.iLike++;

                            if (iAgain == 0)
                            {
                                sBidui[y * iW + x] = 1;
                            }
                        }
                        else
                        {
                            //向外散10层
                            for (int n = 1; n <= itimes / 5; n++)
                            {
                                //向上向下侧
                                int[] iNN = new int[] { -1, 1 };
                                bool isOK = false;
                                for (int e = 0; e < iNN.Length; e++)
                                {
                                    //MessageBox.Show(x + "-" + (y + n * iNN[e]));
                                    int iyy = y + n * iNN[e];
                                    if (iyy >= 0 && iyy < iH)
                                    {
                                        if (dot_all_2[iyy * iW + x] != '0')
                                        {
                                            //向左第n个点相和 距离为 n
                                            thisIC = n;
                                            if (iAgain == 0) sBidui[iyy * iW + x] = 1;
                                            isOK = true;
                                            break;
                                        }
                                        //向左向上向下
                                        for (int ii = 1; ii <= n - 1; ii++)
                                        {
                                            if (x - ii >= 0 && dot_all_2[iyy * iW + (x - ii)] != '0')
                                            {
                                                //MessageBox.Show((x - ii) + "-" + (y + n * iNN[e]));
                                                //向左第n个点想 且向上 ii 个点相和 距离为 n + ii
                                                thisIC = n + ii;
                                                if (iAgain == 0) sBidui[iyy * iW + (x - ii)] = 1;
                                                isOK = true;
                                                break;
                                            }
                                            else if (x + ii < iW && dot_all_2[iyy * iW + (x + ii)] != '0')
                                            {
                                                //MessageBox.Show((x + ii) + "-" + (y + n * iNN[e]));
                                                //向左第n个点想 且向上 ii 个点相和 距离为 n + ii
                                                thisIC = n + ii;
                                                if (iAgain == 0) sBidui[iyy * iW + (x + ii)] = 1;
                                                isOK = true;
                                                break;
                                            }
                                        }
                                        if (isOK) break;
                                    }
                                }
                                if (isOK) break;

                                //向左向右侧
                                isOK = false;
                                for (int e = 0; e < iNN.Length; e++)
                                {
                                    //MessageBox.Show((x + n * iNN[e]) + "-" + y);
                                    int ixx = x + n * iNN[e];
                                    if (ixx >= 0 && ixx < iW)
                                    {
                                        if (dot_all_2[y * iW + ixx] != '0')
                                        {
                                            //向左第n个点相和 距离为 n
                                            thisIC = n;
                                            if (iAgain == 0) sBidui[y * iW + ixx] = 1;
                                            isOK = true;
                                            break;
                                        }
                                        //向左向上向下
                                        for (int ii = 1; ii <= n; ii++)
                                        {
                                            //MessageBox.Show((x + n * iNN[e]) + "-" + (y - ii));
                                            //MessageBox.Show((x + n * iNN[e]) + "-" + (y + ii));
                                            if (y - ii >= 0 && dot_all_2[(y - ii) * iW + ixx] != '0')
                                            {
                                                //向左第n个点想 且向上 ii 个点相和 距离为 n + ii
                                                thisIC = n + ii;
                                                if (iAgain == 0) sBidui[(y - ii) * iW + ixx] = 1;
                                                isOK = true;
                                                break;
                                            }
                                            else if (y + ii < iH && dot_all_2[(y + ii) * iW + ixx] != '0')
                                            {
                                                //向左第n个点想 且向上 ii 个点相和 距离为 n + ii
                                                thisIC = n + ii;
                                                if (iAgain == 0) sBidui[(y + ii) * iW + ixx] = 1;
                                                isOK = true;
                                                break;
                                            }
                                        }
                                        if (isOK) break;
                                    }
                                }
                                if (isOK) break;
                            }
                        }
                        if (thisIC == -1)
                        {
                            iC_all = itimes / 5 * 2;
                            iC_all = iC_all * iC_all * iC_all;
                        }
                        else if (thisIC != 0)
                        {
                            if (thisIC == 1)
                            {
                                rLike.iLike++;
                                //dot_all_1[y * iW + x] = '3';
                            }
                            else if (thisIC < 3)
                            {
                                rLike.iLike++;
                                //dot_all_1[y * iW + x] = '3';
                            }
                            else
                            {
                                iC_all += thisIC * thisIC;
                                //dot_all_1[y * iW + x] = '2';
                            }
                        }
                    }
                }
            }
        }

        rLike.iC_all += iC_all;
    }

    public class resultDot
    {
        /// <summary>四项点 黑点个数</summary>
        public int iCount = 0;
        /// <summary>四项点 全部字符串</summary>
        public StringBuilder sString = null;
        /// <summary>四项点 中心点 X</summary>
        public int X = 0;
        /// <summary>四项点 中心点 Y</summary>
        public int Y = 0;
        /// <summary>四项点 权重中心点 X</summary>
        public int X2 = 0;
        /// <summary>四项点 权重中心点 Y</summary>
        public int Y2 = 0;
        /// <summary>四项点 权重中心点 X</summary>
        public int X3 = 0;
        /// <summary>四项点 权重中心点 Y</summary>
        public int Y3 = 0;
        /// <summary>四项点 中心点 W</summary>
        public int W = 0;
        /// <summary>四项点 中心点 H</summary>
        public int H = 0;

        /// <summary>横向直线 大于 30% 的线</summary>
        public int lineX_30_count = 0;
        /// <summary>横向直线 大于 30% 的线 的 Rect </summary>
        public StringBuilder lineX_30_str = new StringBuilder();
        /// <summary>横向直线 大于 50% 的线</summary>
        public int lineX_50_count = 0;
        /// <summary>横向直线 大于 50% 的线 的 Rect </summary>
        public StringBuilder lineX_50_str = new StringBuilder();
        /// <summary>横向直线 大于 90% 的线</summary>
        public int lineX_90_count = 0;
        /// <summary>横向直线 大于 90% 的线 的 Rect </summary>
        public StringBuilder lineX_90_str = new StringBuilder();

        /// <summary>纵向直线 大于 30% 的线</summary>
        public int lineY_30_count = 0;
        /// <summary>纵向直线 大于 30% 的线 的 Rect </summary>
        public StringBuilder lineY_30_str = new StringBuilder();
        /// <summary>纵向直线 大于 50% 的线</summary>
        public int lineY_50_count = 0;
        /// <summary>纵向直线 大于 50% 的线 的 Rect </summary>
        public StringBuilder lineY_50_str = new StringBuilder();
        /// <summary>纵向直线 大于 90% 的线</summary>
        public int lineY_90_count = 0;
        /// <summary>纵向直线 大于 90% 的线 的 Rect </summary>
        public StringBuilder lineY_90_str = new StringBuilder();

        public resultDot()
        {
            sString = new StringBuilder();
            iCount = 0;
            X = 0;
            Y = 0;
            W = 0;
            H = 0;
        }

        public string DebugLine()
        {
            string s = "x90：" + lineX_90_count + " " + lineX_90_str + " \r\n"
                    + "y90：" + lineY_90_count + " " + lineY_90_str + " \r\n"
                    + "x50：" + lineX_50_count + " " + lineX_50_str + " \r\n"
                    + "y50：" + lineY_50_count + " " + lineY_50_str + " \r\n"
                    + "x30：" + lineX_30_count + " " + lineX_30_str + " \r\n"
                    + "y30：" + lineY_30_count + " " + lineY_30_str + " \r\n";

            return s;
        }

        /// <summary>横向直线 大于 30% 的线 的 Rect </summary>
        public ArrayList AL_lineX_30 = new ArrayList();
        /// <summary>横向直线 大于 50% 的线 的 Rect </summary>
        public ArrayList AL_lineX_50 = new ArrayList();
        /// <summary>横向直线 大于 90% 的线 的 Rect </summary>
        public ArrayList AL_lineX_90 = new ArrayList();
        /// <summary>纵向直线 大于 30% 的线 的 Rect </summary>
        public ArrayList AL_lineY_30 = new ArrayList();
        /// <summary>纵向直线 大于 50% 的线 的 Rect </summary>
        public ArrayList AL_lineY_50 = new ArrayList();
        /// <summary>纵向直线 大于 90% 的线 的 Rect </summary>
        public ArrayList AL_lineY_90 = new ArrayList();

        public void ReInit_Line()
        {
            AL_lineX_30 = new ArrayList();
            AL_lineX_50 = new ArrayList();
            AL_lineX_90 = new ArrayList();
            AL_lineY_30 = new ArrayList();
            AL_lineY_50 = new ArrayList();
            AL_lineY_90 = new ArrayList();
            string[] arrLine90 = lineX_90_str.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < arrLine90.Length; i++)
            {
                resultLine line = new resultLine(arrLine90[i]);
                AL_lineX_90.Add(line);
            }
            string[] arrLine50 = lineX_50_str.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < arrLine50.Length; i++)
            {
                resultLine line = new resultLine(arrLine50[i]);
                AL_lineX_50.Add(line);
            }
            string[] arrLine30 = lineX_30_str.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < arrLine30.Length; i++)
            {
                resultLine line = new resultLine(arrLine30[i]);
                AL_lineX_30.Add(line);
            }
            arrLine90 = lineY_90_str.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < arrLine90.Length; i++)
            {
                resultLine line = new resultLine(arrLine90[i]);
                AL_lineY_90.Add(line);
            }
            arrLine50 = lineY_50_str.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < arrLine50.Length; i++)
            {
                resultLine line = new resultLine(arrLine50[i]);
                AL_lineY_50.Add(line);
            }
            arrLine30 = lineY_30_str.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < arrLine30.Length; i++)
            {
                resultLine line = new resultLine(arrLine30[i]);
                AL_lineY_30.Add(line);
            }
        }
        public bool checkLineX_90(ArrayList AL_line, float iCha)
        {
            bool checkOK = true;
            for (int i = 0; i < AL_line.Count; i++)
            {
                resultLine line = (resultLine)AL_line[i];
                bool isOK = false;
                for (int j = 0; j < AL_lineX_90.Count; j++)
                {
                    resultLine l = (resultLine)AL_lineX_90[j];
                    if (Math.Abs(line.iPostion - l.iPostion) <= iCha)
                    {
                        isOK = true;
                        break;
                    }
                }
                if (!isOK)
                {
                    for (int j = 0; j < AL_lineX_50.Count; j++)
                    {
                        resultLine l = (resultLine)AL_lineX_50[j];
                        if (Math.Abs(line.iPostion - l.iPostion) <= iCha)
                        {
                            isOK = true;
                            break;
                        }
                    }
                }
                if (!isOK)
                {
                    float iLength = 0;
                    for (int j = 0; j < AL_lineX_30.Count; j++)
                    {
                        resultLine l = (resultLine)AL_lineX_30[j];
                        if (Math.Abs(line.iPostion - l.iPostion) <= 1)
                        {
                            iLength += l.ilength;
                            if (iLength >= 0.5)
                            {
                                isOK = true;
                                break;
                            }
                        }
                    }
                }
                checkOK = isOK;
                if (!checkOK) break;
            }
            return checkOK;
        }
        public bool checkLineY_90(ArrayList AL_line, float iCha)
        {
            bool checkOK = true;
            for (int i = 0; i < AL_line.Count; i++)
            {
                resultLine line = (resultLine)AL_line[i];
                bool isOK = false;
                for (int j = 0; j < AL_lineY_90.Count; j++)
                {
                    resultLine l = (resultLine)AL_lineY_90[j];
                    if (Math.Abs(line.iPostion - l.iPostion) <= iCha)
                    {
                        isOK = true;
                        break;
                    }
                }
                if (!isOK)
                {
                    for (int j = 0; j < AL_lineY_50.Count; j++)
                    {
                        resultLine l = (resultLine)AL_lineY_50[j];
                        if (Math.Abs(line.iPostion - l.iPostion) <= iCha)
                        {
                            isOK = true;
                            break;
                        }
                    }
                }
                if (!isOK)
                {
                    float iLength = 0;
                    for (int j = 0; j < AL_lineY_30.Count; j++)
                    {
                        resultLine l = (resultLine)AL_lineY_30[j];
                        if (Math.Abs(line.iPostion - l.iPostion) <= 1)
                        {
                            iLength += l.ilength;
                            if (iLength >= 0.5)
                            {
                                isOK = true;
                                break;
                            }
                        }
                    }
                }
                checkOK = isOK;
                if (!checkOK) break;
            }
            return checkOK;
        }
        public bool checkLineX_50(ArrayList AL_line, float iCha)
        {
            bool checkOK = true;
            for (int i = 0; i < AL_line.Count; i++)
            {
                resultLine line = (resultLine)AL_line[i];
                if (line.Height == 2 && line.ilength >= 0.6)
                {
                    bool isOK = false;
                    for (int j = 0; j < AL_lineX_50.Count; j++)
                    {
                        resultLine l = (resultLine)AL_lineX_50[j];
                        if (Math.Abs(line.iPostion - l.iPostion) <= iCha)
                        {
                            isOK = true;
                            break;
                        }
                    }
                    if (!isOK)
                    {
                        for (int j = 0; j < AL_lineX_30.Count; j++)
                        {
                            resultLine l = (resultLine)AL_lineX_30[j];
                            if (Math.Abs(line.iPostion - l.iPostion) <= iCha)
                            {
                                isOK = true;
                                break;
                            }
                        }
                    }
                    if (!isOK)
                    {
                        for (int j = 0; j < AL_lineX_90.Count; j++)
                        {
                            resultLine l = (resultLine)AL_lineX_90[j];
                            if (Math.Abs(line.iPostion - l.iPostion) <= iCha)
                            {
                                isOK = true;
                                break;
                            }
                        }
                    }
                    checkOK = isOK;
                    if (!checkOK) break;
                }
            }
            return checkOK;
        }
        public bool checkLineY_50(ArrayList AL_line, float iCha)
        {
            bool checkOK = true;
            for (int i = 0; i < AL_line.Count; i++)
            {
                resultLine line = (resultLine)AL_line[i];
                if (line.Width == 2 && line.ilength >= 0.6)
                {
                    bool isOK = false;
                    for (int j = 0; j < AL_lineY_50.Count; j++)
                    {
                        resultLine l = (resultLine)AL_lineY_50[j];
                        if (Math.Abs(line.iPostion - l.iPostion) <= iCha)
                        {
                            isOK = true;
                            break;
                        }
                    }
                    if (!isOK)
                    {
                        for (int j = 0; j < AL_lineY_30.Count; j++)
                        {
                            resultLine l = (resultLine)AL_lineY_30[j];
                            if (Math.Abs(line.iPostion - l.iPostion) <= iCha)
                            {
                                isOK = true;
                                break;
                            }
                        }
                    }
                    if (!isOK)
                    {
                        for (int j = 0; j < AL_lineY_90.Count; j++)
                        {
                            resultLine l = (resultLine)AL_lineY_90[j];
                            if (Math.Abs(line.iPostion - l.iPostion) <= iCha)
                            {
                                isOK = true;
                                break;
                            }
                        }
                    }
                    checkOK = isOK;
                    if (!checkOK) break;
                }
            }
            return checkOK;
        }
    }
    public class resultLine
    {
        /// <summary>线段长度</summary>
        public float ilength = 0;
        /// <summary>线段位置</summary>
        public float iPostion = 0;
        /// <summary>宽</summary>
        public int Width = 0;
        /// <summary>高</summary>
        public int Height = 0;

        public resultLine(string str)
        {
            string[] arr = str.Split(';');
            string[] arr1 = arr[0].Split(',');
            string[] arr2 = arr[1].Split(',');
            ilength = (float)Convert.ToDouble(arr1[0]);
            iPostion = (float)Convert.ToDouble(arr1[1]);

            Width = Convert.ToInt32(arr2[2]);
            Height = Convert.ToInt32(arr2[3]);
        }
    }

    /// <summary>
    /// 获取并合并所有的 大于 50% 的 横向直线
    /// </summary>
    /// <param name="p"></param>
    /// <param name="bmp_w"></param>
    /// <param name="bmp_h"></param>
    /// <param name="bmp_iWidth"></param>
    void SmallLineX_Only(ref ArrayList AL_ClearH, byte* p, uint bmp_w, uint bmp_h, uint bmp_iWidth)
    {
        for (uint y = 0; y < bmp_h; y++)
        {
            ArrayList AL_Now = new ArrayList();
            //该行的所有线段
            Rect rect = null;
            for (uint x = 0; x < bmp_w; x++)
            {
                myDot dot = new myDot(x, y, bmp_iWidth, p);
                if (dot.R == 0)
                {
                    if (rect == null)
                    {
                        //建立新线段
                        rect = new Rect(y, x, y, x);
                    }
                    //加入到线段
                    rect.Add(dot);
                    rect.r = x;
                    rect.width = Math.Abs((int)rect.l - (int)rect.r) + 1;
                    rect.height = 1;
                }
                else
                {
                    //遇到白点断开

                    if (rect != null)
                    {
                        //MessageBox.Show("a" + y + " " + rect.l + " " + rect.width);
                        if (rect.width > bmp_h / 4)
                        {
                            AL_Now.Add(rect);
                            //MessageBox.Show("b" + y + " " + rect.l + " " + rect.width);
                        }
                        rect = null;
                    }
                }
            }
            if (rect != null && rect.width > bmp_h / 4)
            {
                AL_Now.Add(rect);
                // MessageBox.Show(y + " " + rect.width);
            }

            AL_ClearH.Add(AL_Now);
        }

        for (int i = 0; i < AL_ClearH.Count; i++)
        {
            if (i != 0)
            {
                ArrayList AL_Prev = (ArrayList)AL_ClearH[i - 1];
                ArrayList AL_Now = (ArrayList)AL_ClearH[i];
                ArrayList AL_hb = new ArrayList();

                for (int j = 0; j < AL_Now.Count; j++)
                {
                    Rect rect = (Rect)AL_Now[j];
                    for (int k = 0; k < AL_Prev.Count + AL_hb.Count; k++)
                    {
                        Rect r = null;
                        if (k < AL_Prev.Count)
                        {
                            r = (Rect)AL_Prev[k];
                        }
                        else
                        {
                            r = (Rect)AL_hb[k - AL_Prev.Count];
                        }

                        //相聚 1 格相交的线，如有合并 下侧应该相和
                        if (!r.isHB && (rect.t - r.b == 1 || rect.b == r.b))
                        {
                            if (!r.isHB)
                            {
                                //上一个在里面 且相交部分大与 50% 
                                if ((rect.l <= r.l && r.l <= rect.r)
                                    && (rect.l <= r.r && r.r <= rect.r)
                                    )
                                {
                                    r.isHB = true;
                                    rect.AddRect(r);
                                    AL_hb.Add(rect);
                                    break;
                                }
                            }
                            if (!r.isHB)
                            {
                                //上一个在右边 且相交部分大与 50% 
                                int prevLeft = (int)rect.r - (int)r.l;
                                //float iall = (float)(rect.width + r.width) / 2;
                                if (prevLeft > 0 && rect.r <= r.r)
                                {
                                    r.isHB = true;
                                    rect.AddRect(r);
                                    AL_hb.Add(rect);
                                    break;
                                }
                            }
                            if (!r.isHB)
                            {
                                //上一个在左边 且相交部分大与 50% 
                                int prevRight = (int)r.r - (int)rect.l;
                                //float iall = (float)(rect.width + r.width) / 2;
                                if (prevRight > 0 && r.l <= rect.l)
                                {
                                    r.isHB = true;
                                    rect.AddRect(r);
                                    AL_hb.Add(rect);
                                    break;
                                }
                            }
                            // && (float)prevLeft / iall > 0.5
                        }

                    }
                }
            }
        }
    }
    /// <summary>
    /// 获取并合并所有的 大于 50% 的 纵向直线
    /// </summary>
    /// <param name="p"></param>
    /// <param name="bmp_w"></param>
    /// <param name="bmp_h"></param>
    /// <param name="bmp_iWidth"></param>
    void SmallLineY_Only(ref ArrayList AL_ClearV, byte* p, uint bmp_w, uint bmp_h, uint bmp_iWidth)
    {
        for (uint x = 0; x < bmp_w; x++)
        {
            ArrayList AL_Now = new ArrayList();
            //该行的所有线段
            Rect rect = null;
            for (uint y = 0; y < bmp_h; y++)
            {
                myDot dot = new myDot(x, y, bmp_iWidth, p);
                if (dot.R == 0)
                {
                    if (rect == null)
                    {
                        //建立新线段
                        rect = new Rect(y, x, y, x);
                    }
                    //加入到线段
                    rect.Add(dot);
                    rect.b = y;
                    rect.height = Math.Abs((int)rect.t - (int)rect.b) + 1;
                    rect.width = 1;
                }
                else
                {
                    //遇到白点断开
                    if (rect != null)
                    {
                        //MessageBox.Show("a" + y + " " + rect.l + " " + rect.width);
                        if (rect.height > bmp_h / 4)
                        {
                            AL_Now.Add(rect);
                            //MessageBox.Show("b" + y + " " + rect.l + " " + rect.width);
                        }
                        rect = null;
                    }
                }
            }

            if (rect != null && rect.height > bmp_h / 4)
            {
                AL_Now.Add(rect);
                // MessageBox.Show(y + " " + rect.width);
            }

            AL_ClearV.Add(AL_Now);
        }

        for (int i = 0; i < AL_ClearV.Count; i++)
        {
            if (i != 0)
            {
                ArrayList AL_Prev = (ArrayList)AL_ClearV[i - 1];
                ArrayList AL_Now = (ArrayList)AL_ClearV[i];
                ArrayList AL_hb = new ArrayList();

                for (int j = 0; j < AL_Now.Count; j++)
                {
                    Rect rect = (Rect)AL_Now[j];
                    for (int k = 0; k < AL_Prev.Count + AL_hb.Count; k++)
                    {
                        Rect r = null;
                        if (k < AL_Prev.Count)
                        {
                            r = (Rect)AL_Prev[k];
                        }
                        else
                        {
                            r = (Rect)AL_hb[k - AL_Prev.Count];
                        }
                        //相聚 1 格相交的线，如有合并 右侧应该相和
                        if (!r.isHB && (rect.l - r.r == 1 || rect.r == r.r))
                        {
                            if (!r.isHB)
                            {
                                //上一个在里面 且相交部分大与 50% 
                                if ((rect.t <= r.t && r.t <= rect.b)
                                    && (rect.t <= r.b && r.b <= rect.b)
                                    )
                                {
                                    r.isHB = true;
                                    rect.AddRect(r);
                                    AL_hb.Add(rect);
                                    break;
                                }
                            }
                            if (!r.isHB)
                            {
                                //上一个在右边 且相交部分大与 50% 
                                int prevLeft = (int)rect.b - (int)r.t;
                                //float iall = (float)(rect.height + r.height) / 2;
                                if (prevLeft > 0 && rect.b <= r.b)
                                {
                                    r.isHB = true;
                                    rect.AddRect(r);
                                    AL_hb.Add(rect);
                                    break;
                                }
                            }
                            if (!r.isHB)
                            {
                                //上一个在左边 且相交部分大与 50% 
                                int prevRight = (int)r.b - (int)rect.t;
                                //float iall = (float)(rect.height + r.height) / 2;
                                if (prevRight > 0 && r.t <= rect.t)
                                {
                                    r.isHB = true;
                                    rect.AddRect(r);
                                    AL_hb.Add(rect);
                                    break;
                                }
                            }
                            // && (float)prevLeft / iall > 0.5
                        }
                    }
                }
            }
        }
    }
}

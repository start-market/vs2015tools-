using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;


public class intBmpCheckTableGAJ
{
    public AciBmpInt.dotRGB RGB = null;
    /// <summary>
    /// 灰度计算的有效范围 默认为 80
    /// </summary>
    public bool show = false;
    public int greyAreaX = 0;
    public int greyAreaY = 0;

    public List<HoughHbClass> AL_hb_H_Long;
    public List<HoughHbClass> AL_hb_H;
    public List<HoughHbClass> AL_hb_V;
    public List<HoughHbClass> AL_H;
    public List<HoughHbClass> AL_V;
    public HoughHbClass hb_V_first;
    public HoughHbClass hb_V_last;
    public HoughHbClass hb_V_last2;
    public HoughHbClass hb_H_first;
    public HoughHbClass hb_H_last;
    saveImage imgSave = new saveImage();
    public int iSmall = 1500;
    /// <summary>
    /// 左上角像素比
    /// </summary>
    public float idotFirstHV = 0;

    public intBmpCheckTableGAJ()
    {

    }

    public Bitmap setImage(FileInfo file, bool isTwo)
    {
        Bitmap bmp = (Bitmap)System.Drawing.Image.FromFile(file.FullName);
        return setImage(ref bmp, file, isTwo);
    }

    public List<Rect> setSmall(ref Bitmap bmp, FileInfo file)
    {
        List<Rect> rectBlack = new List<Rect>();
        int ismall = 300;
        Bitmap bitmap = new Bitmap(ismall, (int)(ismall / (double)bmp.Width * (double)bmp.Height));
        Graphics g = Graphics.FromImage(bitmap);
        g.DrawImage(bmp, 0, 0, bitmap.Width, bitmap.Height);
        g.Dispose();
        int[,] intBmp = AciBmpInt.getIntBmpByBitmap(ref bitmap);

        if (show) AciBmpInt.DrawIntBmpPng(file.FullName.Replace(".jpg", "_临时_00-1_生产小图.png"), intBmp);

        int[] pixelNum = new int[256];
        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                pixelNum[intBmp[x, y]]++;
            }
        }
        uint ik = AciBmpInt.getOstu(pixelNum);
        uint ik2 = AciBmpInt.getShang(pixelNum);
        if (ik2 > ik)
        {
            ik = ik2;
        }

        AciBmpInt.toTwo(ref intBmp, ik, bitmap.Width, bitmap.Height);


        List<Rect> arrRect = AciBmpInt.cutArea(ref intBmp, file, bitmap.Width, bitmap.Height);
        for (int i = 0; i < arrRect.Count; i++)
        {
            Rect rect = arrRect[i];
            rect.calculateWH();
            rect.S = rect.width * rect.height;
            if ((float)rect.iNum / (float)rect.S > 0.3f && rect.iNum > 30 && rect.width > 5 && rect.height > 5)
            {
                if (show) rect.Draw(ref intBmp);
                rectBlack.Add(rect);
            }
        }
        if (show) AciBmpInt.DrawIntBmpPng(file.FullName.Replace(".jpg", "_临时_00-2_小图二值化.png"), intBmp);

        bitmap.Dispose();
        return rectBlack;
    }

    public void Clear()
    {
        RGB = null;
    }
    public Bitmap setImage(ref Bitmap bmp, FileInfo file, bool isTwo)
    {
        Clear();
        List<Rect> rectBlack = setSmall(ref bmp, file);

        iSmall = 1500;
        float ipw = 2200f / (float)bmp.Width;
        iSmall = (int)Math.Round((float)iSmall / ipw);
        Bitmap bitmap = new Bitmap(iSmall, (int)(iSmall / (double)bmp.Width * (double)bmp.Height));


        Graphics g = Graphics.FromImage(bitmap);
        g.DrawImage(bmp, 0, 0, bitmap.Width, bitmap.Height);
        g.Dispose();

        int iw = bitmap.Width;
        int ih = bitmap.Height;

        greyAreaX = 1;
        greyAreaY = 1;

        int[,] intBmp = AciBmpInt.getIntBmpByBitmap(ref bitmap);
        bitmap.Dispose();

        //if (show) AciBmpInt.DrawIntBmpPng(file.FullName.Replace(".jpg", "_临时_01_缩略图.png"), intBmp);

        //去除黑色色块
        float ipwh = 300 / (float)iSmall;
        for (int i = 0; i < rectBlack.Count; i++)
        {
            Rect rect = rectBlack[i];
            int irectL = (int)Math.Round((float)rect.l / ipwh);
            int irectW = (int)Math.Round((float)rect.width / ipwh);
            int irectT = (int)Math.Round((float)rect.t / ipwh);
            int irectH = (int)Math.Round((float)rect.height / ipwh);
            for (int y = irectT; y < irectT + irectH; y++)
            {
                for (int x = irectL; x < irectL + irectW; x++)
                {
                    if (x < iw && y < ih)
                    {
                        if (intBmp[x, y] > 150)
                        {
                            intBmp[x, y] = 0;
                        }
                    }
                }
            }
        }

        if (isTwo)
        {
            int[] pixelNum2;

            pixelNum2 = new int[256];

            for (int y = 0; y < ih; y++)
            {
                for (int x = 0; x < iw; x++)
                {
                    pixelNum2[intBmp[x, y]]++;
                }
            }
            uint ikk = AciBmpInt.getOstu(pixelNum2);
            uint ikk2 = AciBmpInt.getShang(pixelNum2);
            if (ikk < ikk2)
            {
                ikk = ikk2;
            }

            for (uint y = 0; y < ih; y++)
            {
                for (uint x = 0; x < iw; x++)
                {
                    if (intBmp[x, y] < ikk)
                    {
                        intBmp[x, y] = 0;
                    }
                }
            }
        }

        int[,] outBmp = AciBmpInt.getFrameHV_1(intBmp, false);

        if (show) AciBmpInt.DrawIntBmpPng(file.FullName.Replace(".jpg", "_临时_01_提取轮廓.png"), outBmp);

        List<Rect> al_rectH = AciBmpInt.cutAreaLineH(outBmp, file, 50, greyAreaX, greyAreaY, false);
        List<Rect> al_rectV = AciBmpInt.cutAreaLineV(outBmp, file, 50, greyAreaX, greyAreaY, false);

        outBmp = new int[iw, ih];
        for (int i = 0; i < al_rectH.Count; i++)
        {
            for (int j = 0; j < al_rectH[i].Al_dot.Count; j++)
            {
                myDot dot = al_rectH[i].Al_dot[j];
                outBmp[dot.x, dot.y] = intBmp[dot.x, dot.y];
                //outBmp[dot.x, dot.y] = 255;
            }
        }
        for (int i = 0; i < al_rectV.Count; i++)
        {
            for (int j = 0; j < al_rectV[i].Al_dot.Count; j++)
            {
                myDot dot = al_rectV[i].Al_dot[j];
                outBmp[dot.x, dot.y] = intBmp[dot.x, dot.y];
                //outBmp[dot.x, dot.y] = 255;
            }
        }
        if (show) AciBmpInt.DrawIntBmpPng(file.FullName.Replace(".jpg", "_临时_02_去除小区域.png"), outBmp);

        List<HoughHbClass> AL_move = new List<HoughHbClass>();

        //合并直线H
        AL_hb_H_Long = AciBmpInt.getAL_hb_H(outBmp, file, 0, 0, -1, al_rectH, 50);
        for (int i = 0; i < AL_hb_H_Long.Count; i++)
        {
            HoughHbClass hb = AL_hb_H_Long[i];
            hb.setLength01();
            float lpp = (float)hb.iLength / (float)iw;
            if (hb.ib < 30 && lpp > 0.50f)
            {

            }
            else
            {
                AL_move.Add(hb);
            }
        }
        for (int i = 0; i < AL_move.Count; i++)
        {
            AL_hb_H_Long.Remove(AL_move[i]);
        }

        if (show) AciBmpInt.DrawIntBmpPng(file.FullName.Replace(".jpg", "_临时_02-1_彩杠.png"), DrawFrameLong(iw, ih));

        AL_hb_H = AciBmpInt.getAL_hb_H(outBmp, file, 0, 0, -1, al_rectH, 10);
        AL_hb_V = AciBmpInt.getAL_hb_V(outBmp, file, 0, 0, -1, al_rectV, 15);

        AL_hb_H.Sort(new myHbSorterH());
        AL_hb_V.Sort(new myHbSorterV());

        if (show)
        {
            AciBmpInt.DrawIntBmpPng(file.FullName.Replace(".jpg", "_临时_03_合并直线.png"), AciBmpInt.getIntBmp_hb(AL_hb_H, AL_hb_V, false, iw, ih, false, ""));
        }

        ////////////////////////////////////////////////////
        ////查找 水平 H 长直线 
        ////////////////////////////////////////////////////
        AL_H = new List<HoughHbClass>();
        for (int i = 0; i < AL_hb_H.Count; i++)
        {
            HoughHbClass hb = AL_hb_H[i];
            hb.setLength01();
            hb.setLength255(10f, 80);
            hb.setLengthLong(hb.arrLength255, 25);
            if (hb.iLength255 > iw / 2)
            {
                hb.isok = true;
                AL_H.Add(hb);
            }
        }
        ////////////////////////////////////////////////////
        ////查找 垂直 V 长直线 
        ////////////////////////////////////////////////////
        AL_V = new List<HoughHbClass>();
        for (int i = 0; i < AL_hb_V.Count; i++)
        {
            HoughHbClass hb = AL_hb_V[i];
            hb.setLength01();
            hb.setLength255(10f, 80);
            hb.setLengthLong(hb.arrLength255, 25);
            if (hb.iLength > ih / 5 * 2)
            {
                hb.isok = true;
                AL_V.Add(hb);
            }
        }

        //if (show)
        //{
        //    AciBmpInt.DrawIntBmpPng(file.FullName.Replace(".jpg", "_临时_04_计算255后.png"), AciBmpInt.getIntBmp_hb(AL_hb_H, AL_hb_V, false, iw, ih, false, "255"));
        //    //AciBmpInt.DrawIntBmpPng(file.FullName.Replace(".jpg", "_临时_05_计算long后.png"), AciBmpInt.getIntBmp_hb(AL_hb_H, AL_hb_V, false, iw, ih, false, "long"));
        //}


        ////////////////////////////////////////////////////
        ////开始寻找表格轮廓  查找 垂直 V 轮廓线 左侧第一根 满足相交50%
        ////////////////////////////////////////////////////
        hb_V_first = null;
        AL_move = new List<HoughHbClass>();
        for (int i = 0; i < AL_V.Count; i++)
        {
            float inum = 0;
            HoughHbClass V = (HoughHbClass)AL_V[i];
            if (iw - V.ixy > 1150)
            {
                if (V.iLength255 > ih / 5 * 2)
                {
                    for (int j = 0; j < AL_H.Count; j++)
                    {
                        HoughHbClass H = (HoughHbClass)AL_H[j];
                        if (Math.Abs(V.ixy - H.ib) <= 25
                            || H.arrLengthLong[V.ixy] > 0)
                        {
                            inum++;
                        }
                    }
                    float ipp = inum / (float)AL_H.Count;
                    if (ipp > 0.5f)
                    {
                        hb_V_first = V;
                        break;
                    }
                }
            }
            AL_move.Add(V);
        }
        //左侧1200里面没有左侧第一根 取1100左侧第一根
        if (hb_V_first == null)
        {
            AL_move = new List<HoughHbClass>();
            for (int i = 0; i < AL_V.Count; i++)
            {
                HoughHbClass V = (HoughHbClass)AL_V[i];
                if (iw - V.ixy > 1150 && V.ixy >= 80)
                {
                    if (hb_V_first == null)
                    {
                        hb_V_first = V;
                        V.isok = true;
                        AL_move.Remove(V);
                        break;
                    }
                    else
                    {
                        AL_move.Add(V);
                    }
                }
            }
        }
        for (int i = 0; i < AL_move.Count; i++)
        {
            AL_move[i].isok = false;
            AL_V.Remove(AL_move[i]);
        }
        hb_V_last = null;
        //左侧第一根必须找到
        if (hb_V_first != null)
        {
            ////////////////////////////////////////////////////
            ////开始寻找表格轮廓  查找 垂直 V 轮廓线 右侧第一根 满足相交50%
            ////////////////////////////////////////////////////
            AL_move = new List<HoughHbClass>();
            for (int i = AL_V.Count - 1; i >= 0; i--)
            {
                float inum = 0;
                HoughHbClass V = (HoughHbClass)AL_V[i];
                if (V.ixy - hb_V_first.ixy > 1110)
                {
                    if (V.iLength255 > ih / 5 * 2)
                    {
                        uint iv = V.ixy;
                        for (int j = 0; j < AL_H.Count; j++)
                        {
                            HoughHbClass H = (HoughHbClass)AL_H[j];
                            if (Math.Abs(V.ixy - H.ie) <= 25
                                || H.arrLengthLong[V.ixy] > 0)
                            {
                                inum++;
                            }
                        }
                        float ipp = inum / (float)AL_H.Count;
                        if (ipp > 0.5f)
                        {
                            hb_V_last = V;
                            break;
                        }
                    }
                }
                AL_move.Add(V);
            }
            //离左侧第一个少于1100的 向右侧找
            if (hb_V_last == null)
            {
                AL_move = new List<HoughHbClass>();
                for (int i = 0; i < AL_V.Count; i++)
                {
                    HoughHbClass V = (HoughHbClass)AL_V[i];
                    if (V.ixy - hb_V_first.ixy > 1110)
                    {
                        if (hb_V_last == null)
                        {
                            hb_V_last = V;
                            V.isok = true;
                            AL_move.Remove(V);
                        }
                        else
                        {
                            AL_move.Add(V);
                        }
                    }
                }
            }
            //实在没有就创建一根
            if (hb_V_last == null)
            {
                AL_move = new List<HoughHbClass>();

                HoughHbClass V = new HoughHbClass(ih);
                uint ixy = hb_V_first.ixy + 1218;
                if (ixy >= iw) ixy = (uint)iw - 2;

                V.add(ixy, hb_V_first.rect, "V");
                V.isok = true;
                V.setLength255(10f, 80);
                V.setLength01();
                V.setLengthLong(V.arrLength255, 25);
                AL_hb_V.Add(V);
                AL_V.Add(V);
                hb_V_last = V;

                AL_V.Sort(new myHbSorterV());
                AL_hb_V.Sort(new myHbSorterV());
            }
            for (int i = 0; i < AL_move.Count; i++)
            {
                AL_move[i].isok = false;
                AL_V.Remove(AL_move[i]);
            }
            ////////////////////////////////////////////////////
            ////开始寻找表格轮廓  查找 垂直 V 轮廓线 右侧第二根 满足相交50%
            ////////////////////////////////////////////////////
            hb_V_last2 = null;
            if (hb_V_last != null)
            {
                uint ilast2_b = hb_V_last.ixy - 250;
                uint ilast2_e = hb_V_last.ixy - 120;
                for (int i = AL_V.Count - 1; i >= 0; i--)
                {
                    HoughHbClass V = (HoughHbClass)AL_V[i];
                    if (V.ixy > ilast2_b && V.ixy < ilast2_e)
                    {
                        hb_V_last2 = V;
                        break;
                    }
                }
            }

            //AL_H.Sort(new myHbSorterH());
            //AL_hb_V.Sort(new myHbSorterV());
            ////////////////////////////////////////////////////
            ////开始寻找表格轮廓  查找 水平 H 轮廓线 上方第一根 满足相交50% 必须是粗3线
            ////////////////////////////////////////////////////
            hb_H_first = null;
            if (hb_V_first != null && hb_V_last != null)
            {
                //去除彩杠影响
                AL_move = new List<HoughHbClass>();
                for (int i = 0; i < AL_hb_H_Long.Count; i++)
                {
                    HoughHbClass hb = (HoughHbClass)AL_hb_H_Long[i];
                    int index = 0;
                    int inums = 0;
                    for (int n = 0; n < hb.arrLength.Length; n++)
                    {
                        if (n < hb_V_first.ixy || n > hb_V_last.ixy)
                        {
                            if (hb.arrLength[n] > 0)
                            {
                                index++;
                                inums += hb.arrLength[n];
                            }
                        }
                    }
                    if (index > 0)
                    {
                        int iavg = inums / index;

                        for (int h = 0; h < AL_hb_H.Count; h++)
                        {
                            HoughHbClass H = (HoughHbClass)AL_hb_H[h];
                            if (hb.ib2 <= H.ixy && H.ixy <= hb.ie2 + 10)
                            {
                                for (int n = 0; n < H.arrLength.Length; n++)
                                {
                                    int icc = H.arrLength[n] - iavg;
                                    if (icc < 0) icc = 0;
                                    H.arrLength[n] = icc;
                                }
                            }
                        }
                    }
                }
                AL_H.Clear();
                //重新计算AL_hb_H
                for (int i = 0; i < AL_hb_H.Count; i++)
                {
                    HoughHbClass hb = (HoughHbClass)AL_hb_H[i];
                    hb.setLength255(10, 80);
                    int ilength255 = hb.getLength255ByBE((int)hb_V_first.ixy, (int)hb_V_last.ixy);
                    uint ilength = hb_V_last.ixy - hb_V_first.ixy;
                    if (ilength255 > ilength / 5 * 2)
                    {
                        hb.isok = true;
                        AL_H.Add(hb);
                    }
                }



                if (show)
                {
                    AciBmpInt.DrawIntBmpPng(file.FullName.Replace(".jpg", "_临时_04-2_去除彩杠后.png"), AciBmpInt.getIntBmp_hb(AL_hb_H, AL_hb_V, false, iw, ih, false, ""));
                    AciBmpInt.DrawIntBmpPng(file.FullName.Replace(".jpg", "_临时_04-3_去除彩杠后255.png"), AciBmpInt.getIntBmp_hb(AL_hb_H, AL_hb_V, false, iw, ih, false, "255"));
                    AciBmpInt.DrawIntBmpPng(file.FullName.Replace(".jpg", "_临时_05_去除彩杠H.png"), DrawFrame(iw, ih));
                }

                AL_move = new List<HoughHbClass>();
                for (int i = 0; i < AL_H.Count; i++)
                {
                    float inum = 0;
                    HoughHbClass H = (HoughHbClass)AL_H[i];
                    if (H.iLength255 > iw / 5 * 2)
                    {
                        int ibb = (int)hb_V_first.ixy + 240;
                        int iee = (int)hb_V_first.ixy + 800;
                        if (H.getLength255ByBE(ibb, iee) > (iee - ibb) / 2)
                        {
                            for (int j = 0; j < AL_V.Count; j++)
                            {
                                HoughHbClass V = (HoughHbClass)AL_V[j];
                                if (Math.Abs(H.ixy - V.ib) <= 25
                                    || V.arrLengthLong[H.ixy] > 0)
                                {
                                    float ff = (float)V.getLength255ByBE((int)H.ixy, (int)H.ixy + 80) / 80f;

                                    if (V == hb_V_first || V == hb_V_last)
                                    {
                                        inum += ff * 2;
                                    }
                                    else
                                    {
                                        inum += ff;
                                    }
                                }
                            }
                            float ipp = inum / (float)AL_V.Count;
                            if (ipp >= 0.5f)
                            {
                                hb_H_first = H;
                                break;
                            }
                        }
                    }
                    H.isok = false;
                    AL_move.Add(H);
                }
                for (int i = 0; i < AL_move.Count; i++)
                {
                    AL_H.Remove(AL_move[i]);
                }
            }
            ////////////////////////////////////////////////////
            ////开始寻找表格轮廓  查找 水平 H 轮廓线 下方第一根 满足相交50%
            ////////////////////////////////////////////////////
            hb_H_last = null;
            AL_move = new List<HoughHbClass>();
            for (int i = AL_H.Count - 1; i >= 0; i--)
            {
                float inum = 0;
                HoughHbClass H = (HoughHbClass)AL_H[i];
                if (H.iLength255 > iw / 5 * 2)
                {
                    for (int j = 0; j < AL_V.Count; j++)
                    {
                        HoughHbClass V = (HoughHbClass)AL_V[j];
                        if (Math.Abs(H.ixy - V.ib) <= 25
                            || V.arrLengthLong[H.ixy] > 0)
                        {
                            float ff = (float)V.getLength255ByBE((int)H.ixy - 80, (int)H.ixy) / 80f;

                            if (V == hb_V_first || V == hb_V_last)
                            {
                                inum += ff * 2;
                            }
                            else
                            {
                                inum += ff;
                            }
                        }
                    }
                    float ipp = inum / (float)AL_V.Count;
                    if (ipp >= 0.5f)
                    {
                        hb_H_last = H;
                        break;
                    }
                }
                H.isok = false;
                AL_move.Add(H);
            }
            for (int i = 0; i < AL_move.Count; i++)
            {
                AL_H.Remove(AL_move[i]);
            }

            if (show)
            {
                AciBmpInt.DrawIntBmpPng(file.FullName.Replace(".jpg", "_临时_06_外框.png"), DrawFrame(iw, ih));
            }

            ////////////////////////////////////////////////////
            ////去除多余H 横线中存在的多余的线段
            ////////////////////////////////////////////////////
            AL_move = new List<HoughHbClass>();
            for (int i = 1; i < AL_H.Count; i++)
            {
                HoughHbClass hb = (HoughHbClass)AL_H[i];

                HoughHbClass hb_prev = null;
                for (int j = i - 1; j >= 0; j--)
                {
                    HoughHbClass hb_ls = (HoughHbClass)AL_H[j];
                    hb_prev = hb_ls;
                    break;
                }
                if (hb_prev != null)
                {
                    if (hb.ixy - hb_prev.ixy < 60
                        || (hb.ixy - hb_prev.ixy > 100 && hb.ixy - hb_prev.ixy < 120))
                    {
                        //中间存在多余的线
                        if (hb != hb_H_last)
                        {
                            hb.isok = false;
                            AL_move.Add(hb);
                        }
                        else
                        {
                            hb_prev.isok = false;
                            AL_move.Add(hb_prev);
                        }
                    }
                }
            }
            for (int i = 0; i < AL_move.Count; i++)
            {
                AL_H.Remove(AL_move[i]);
            }

            if (show)
            {
                AciBmpInt.DrawIntBmpPng(file.FullName.Replace(".jpg", "_临时_07_去除多余.png"), DrawFrame(iw, ih));
            }


            if (hb_H_first != null && hb_H_last != null)
            {
                ////////////////////////////////////////////////////
                ////补充H 横线中存在的较短的线 进行补充
                ////////////////////////////////////////////////////
                int iJu = 80;
                for (int i = 1; i < AL_H.Count; i++)
                {
                    HoughHbClass hb = (HoughHbClass)AL_H[i];
                    if (hb.ixy >= hb_H_first.ixy && hb.ixy <= hb_H_last.ixy)
                    {
                        HoughHbClass hb_prev = (HoughHbClass)AL_H[i - 1];
                        int indexPrev = AL_hb_H.IndexOf(hb_prev);
                        int indexNow = AL_hb_H.IndexOf(hb);

                        int icha = (int)Math.Round((float)(hb.ixy - hb_prev.ixy) / (float)iJu) - 1;
                        if (icha > 0)
                        {
                            //中间存在跳线
                            for (int c = 0; c < icha; c++)
                            {
                                //上一个
                                int ixyNow = (int)hb_prev.ixy;
                                //下一个
                                int ixyNext = ixyNow + iJu;
                                int imin = -1;
                                int iminIndex = -1;
                                HoughHbClass hbls = null;
                                for (int j = indexPrev + 1; j < indexNow; j++)
                                {
                                    hbls = AL_hb_H[j];
                                    //上下方位
                                    if (ixyNext - 10 < hbls.ixy && hbls.ixy < ixyNext + 10)
                                    {
                                        int thismin = Math.Abs((int)hbls.ixy - ixyNext);
                                        if (thismin < imin || imin == -1)
                                        {
                                            imin = thismin;
                                            iminIndex = j;
                                        }
                                    }
                                }
                                if (((int)hb_H_last.ixy - ixyNext) < iJu / 2)
                                {
                                    break;
                                }
                                if (iminIndex > -1)
                                {
                                    //存在最小时，确定该条
                                    hbls = AL_hb_H[iminIndex];
                                    AL_H.Insert(AL_H.IndexOf(hb), hbls);
                                    hb_prev = hbls;
                                }
                                else
                                {
                                    //不存在时创建一条新的
                                    hbls = new HoughHbClass(iw);
                                    hbls.add((uint)ixyNext, hb_H_first.rect, "H");
                                    hbls.isok = true;
                                    hbls.setLength255(10f, 80);
                                    hbls.setLength01();
                                    hbls.isok = true;
                                    AL_H.Insert(AL_H.IndexOf(hb), hbls);
                                    hb_prev = hbls;
                                }
                            }
                        }
                    }
                }
            }
        }
        AL_H.Sort(new myHbSorterH());
        AL_hb_H.Sort(new myHbSorterH());


        if (hb_H_first != null && hb_V_first != null)
        {
            int tow = 120;
            int toh = 60;
            int[,] intBmp1 = new int[tow, toh];
            for (uint y = hb_H_first.ixy + 10; y < hb_H_first.ixy + 10 + toh; y++)
            {
                for (uint x = hb_V_first.ixy + 30; x < hb_V_first.ixy + 30 + tow; x++)
                {
                    intBmp1[x - (int)hb_V_first.ixy - 30, y - (int)hb_H_first.ixy - 10] = intBmp[x, y];
                }
            }
            if (show) AciBmpInt.DrawIntBmpPng(file.FullName.Replace(".jpg", "_临时_10-1_左上.png"), intBmp1);
            int[] pixelNum = new int[256];
            for (int x = 0; x < tow; x++)
            {
                for (int y = 0; y < toh; y++)
                {
                    pixelNum[intBmp1[x, y]]++;
                }
            }
            uint ik = AciBmpInt.getOstu(pixelNum);
            uint ik2 = AciBmpInt.getShang(pixelNum);
            if (ik2 > ik)
            {
                ik = ik2;
            }

            AciBmpInt.toTwo01(ref intBmp1, ik, tow, toh);

            float idot = 0;
            for (int x = 0; x < tow; x++)
            {
                for (int y = 0; y < toh; y++)
                {
                    if (intBmp1[x, y] > 0)
                    {
                        idot++;
                    }
                }
            }
            idotFirstHV = idot / (float)(tow * toh);
            if (show) AciBmpInt.DrawIntBmpPng(file.FullName.Replace(".jpg", "_临时_10-2_【" + idotFirstHV + "】.png"), intBmp1);

        }

        if (show)
        {
            AciBmpInt.DrawIntBmpPng(file.FullName.Replace(".jpg", "_临时_09_【" + AL_H.Count + "," + AL_V.Count + "】.png"), DrawFrame(iw, ih));
        }

        return bmp;
    }
    int[,] DrawFrameLong(int iw, int ih)
    {
        int[,] outBmp = new int[iw, ih];

        for (int i = 0; i < AL_hb_H_Long.Count; i++)
        {
            HoughHbClass hb = (HoughHbClass)AL_hb_H_Long[i];
            for (int x = hb.ib; x < hb.ie; x++)
            {
                outBmp[x, (int)hb.ixy] = 255;
            }
        }
        return outBmp;
    }
    int[,] DrawFrame(int iw, int ih)
    {
        int[,] outBmp = new int[iw, ih];

        for (int i = 0; i < AL_H.Count; i++)
        {
            HoughHbClass hb = (HoughHbClass)AL_H[i];
            for (int x = 0; x < iw; x++)
            {
                //if (hb.arrLength255[x] > 0)
                //{
                outBmp[x, (int)hb.ixy] = -1;
                //}
            }
        }
        for (int i = 0; i < AL_V.Count; i++)
        {
            HoughHbClass hb = (HoughHbClass)AL_V[i];
            for (int y = 0; y < ih; y++)
            {
                if (hb.arrLength255[y] > 0)
                {
                    outBmp[(int)hb.ixy, y] = -1;
                }
            }
        }
        if (hb_V_first != null)
        {
            for (int y = 0; y < ih; y++)
            {
                if (hb_V_first.arrLength255[y] > 0)
                {
                    outBmp[(int)hb_V_first.ixy, y] = -2;
                }
            }
        }
        if (hb_V_last != null)
        {
            for (int y = 0; y < ih; y++)
            {
                if (hb_V_last.arrLength255[y] > 0)
                {
                    outBmp[(int)hb_V_last.ixy, y] = -2;
                }
            }
        }
        if (hb_V_last2 != null)
        {
            for (int y = 0; y < ih; y++)
            {
                if (hb_V_last2.arrLength255[y] > 0)
                {
                    outBmp[(int)hb_V_last2.ixy, y] = -2;
                }
            }
        }
        if (hb_H_first != null)
        {
            for (int x = 0; x < iw; x++)
            {
                //if (hb_H_first.arrLength255[x] > 0)
                //{
                outBmp[x, (int)hb_H_first.ixy] = -2;
                //}
            }
        }
        if (hb_H_last != null)
        {
            for (int x = 0; x < iw; x++)
            {
                //if (hb_H_last.arrLength255[x] > 0)
                //{
                outBmp[x, (int)hb_H_last.ixy] = -2;
                //}
            }
        }
        return outBmp;
    }

    public void saveBmpFrame(Bitmap bmp, int x, int y, int width, int height, int ixc, int iyc, int imode, string savepath)
    {
        Bitmap img = new Bitmap(width, height);
        Graphics g = Graphics.FromImage(img);
        g.DrawImage(bmp, new Rectangle(0, 0, width, height), new Rectangle(x, y, width, height), GraphicsUnit.Pixel);
        g.Dispose();
        if (show)
        {
            imgSave.Save(img, savepath.Replace(".jpg", "【" + ixc + "," + iyc + "," + imode + "】.jpg"));
        }
        else
        {
            imgSave.Save(img, savepath);
        }
    }

    public class myHbSorterV : IComparer<HoughHbClass>
    {
        public int Compare(HoughHbClass x, HoughHbClass y)
        {
            if (x == null && y == null)
            {
                return 0;
            }
            if (x == null)
            {
                return -1;
            }
            if (y == null)
            {
                return 1;
            }

            //依名稱排序    
            return x.ixy.CompareTo(y.ixy);//遞增    
            //return yInfo.ixy.CompareTo(xInfo.ixy);//遞減    

            //依修改日期排序    
            //return xInfo.LastWriteTime.CompareTo(yInfo.LastWriteTime);//遞增    
            //return yInfo.LastWriteTime.CompareTo(xInfo.LastWriteTime);//遞減    
        }
    }
    public class myHbSorterH : IComparer<HoughHbClass>
    {
        public int Compare(HoughHbClass x, HoughHbClass y)
        {
            if (x == null && y == null)
            {
                return 0;
            }
            if (x == null)
            {
                return -1;
            }
            if (y == null)
            {
                return 1;
            }


            //依名稱排序    
            return x.ixy.CompareTo(y.ixy);//遞增    
            //return yInfo.ixy.CompareTo(xInfo.ixy);//遞減    

            //依修改日期排序    
            //return xInfo.LastWriteTime.CompareTo(yInfo.LastWriteTime);//遞增    
            //return yInfo.LastWriteTime.CompareTo(xInfo.LastWriteTime);//遞減    
        }
    }
    public class myRectSorterV : IComparer<Rect>
    {
        public int Compare(Rect x, Rect y)
        {
            if (x == null && y == null)
            {
                return 0;
            }
            if (x == null)
            {
                return -1;
            }
            if (y == null)
            {
                return 1;
            }

            //依名稱排序    
            return x.l.CompareTo(y.l);//遞增    
            //return yInfo.l.CompareTo(xInfo.l);//遞減    

            //依修改日期排序    
            //return xInfo.LastWriteTime.CompareTo(yInfo.LastWriteTime);//遞增    
            //return yInfo.LastWriteTime.CompareTo(xInfo.LastWriteTime);//遞減    
        }
    }

    public class myRectSorterH : IComparer<Rect>
    {
        public int Compare(Rect x, Rect y)
        {
            if (x == null && y == null)
            {
                return 0;
            }
            if (x == null)
            {
                return -1;
            }
            if (y == null)
            {
                return 1;
            }


            //依名稱排序    
            //return xInfo.t.CompareTo(yInfo.t);//遞增    
            return y.t.CompareTo(x.t);//遞減    

            //依修改日期排序    
            //return xInfo.LastWriteTime.CompareTo(yInfo.LastWriteTime);//遞增    
            //return yInfo.LastWriteTime.CompareTo(xInfo.LastWriteTime);//遞減    
        }
    }


}

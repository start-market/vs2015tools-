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

public unsafe class intBmpAvgRGB
{
    public AciBmpInt.dotRGB RGB = null;
    public bool show = false;
    public saveImage imgSave = new saveImage();

    public intBmpAvgRGB()
    {
    }


    public Bitmap setImage(FileInfo file)
    {
        Bitmap bmp = (Bitmap)System.Drawing.Image.FromFile(file.FullName);
        return setImage(ref bmp, file, null);
    }

    public void Clear()
    {
        RGB = null;
    }
    public Bitmap setImage(ref Bitmap bmp, FileInfo file, AciBmpInt.dotRGB rgb)
    {
        
        Clear();
        BitmapData bitData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        int ic = bitData.Stride - bitData.Width * 3;
        byte* p = (byte*)(bitData.Scan0.ToPointer());
        int iWidth = bmp.Width * 3 + ic;
        float SS = (float)(bmp.Width * bmp.Height);

        if (rgb == null)
        {
            RGB = AciBmpInt.getAvgGrey(p, iWidth, bmp.Width, bmp.Height);
        }
        else
        {
            RGB = rgb;
        }

        int iw = bmp.Width;
        int ih = bmp.Height;

        int icutNum = iw / 43;

        
        int igrey = 0;
        int[] pixelNum = new int[256];
        int igreyAvg = ((int)RGB.B + (int)RGB.R + (int)RGB.G) / 3;

        if (igreyAvg >= 175)
        {
            int[,] intBmp = new int[iw, ih];
            //白图 降维
            for (int y = 0; y < ih; y++)
            {
                for (int x = 0; x < iw; x++)
                {
                    int ib = y * iWidth + x * 3;
                    int B = (int)p[ib];
                    int G = (int)p[ib + 1];
                    int R = (int)p[ib + 2];

                    intBmp[x, y] = 255 - (B + G + R) / 3;

                    B = B + 255 - (int)RGB.B;
                    G = G + 255 - (int)RGB.G;
                    R = R + 255 - (int)RGB.R;
                    if (B > 255) B = 255;
                    if (G > 255) G = 255;
                    if (R > 255) R = 255;

                    p[ib] = (byte)B;
                    p[ib + 1] = (byte)G;
                    p[ib + 2] = (byte)R;

                    igrey = (B + G + R) / 3;

                    pixelNum[igrey]++;
                }
            }
            //DateTime dt1 = DateTime.Now;
            int[,] outBmpHV = AciBmpInt.getFrameHV_1(intBmp, false, 3, 10);


            //DateTime dt2 = DateTime.Now;
            //TimeSpan ts = new TimeSpan();
            //ts = dt2 - dt1;
            //AciDebug.Debug(ts.TotalMilliseconds + "");

            //int[,] intBmp1 = AciBmpInt.getFrameHV_1(intBmp, false, 1);
            int[,] intBmp1 = outBmpHV;
            if (show) AciBmpInt.DrawIntBmpPng(file.FullName.Replace(".jpg", ".png"), intBmp1);

            
            int iLR = 10;
            List<Rect> al_rectH1 = cutAreaLineH(intBmp1, file, icutNum, 0, iw / iLR, 2);
            int[,] outBmp = new int[iw, ih];
            for (int i = 0; i < al_rectH1.Count; i++)
            {
                for (int j = 0; j < al_rectH1[i].Al_dot.Count; j++)
                {
                    myDot dot = al_rectH1[i].Al_dot[j];
                    outBmp[dot.x, dot.y] = intBmp1[dot.x, dot.y];
                }
            }
            List<Rect> al_rectH2 = cutAreaLineH(intBmp1, file, icutNum, iw - iw / iLR, iw, 2);
            for (int i = 0; i < al_rectH2.Count; i++)
            {
                for (int j = 0; j < al_rectH2[i].Al_dot.Count; j++)
                {
                    myDot dot = al_rectH2[i].Al_dot[j];
                    outBmp[dot.x, dot.y] = intBmp1[dot.x, dot.y];
                }
            }
            //归一化
            int tow = 1000;
            int toh = 0;
            float pp = 0f;
            if (iw > tow)
            {
                pp = (float)tow / (float)iw;
                toh = (int)Math.Round((float)ih * pp);
                outBmp = AciBmpInt.intBmpScaleSmall(outBmp, tow, toh, new Rectangle(0, 0, tow, toh), new Rectangle(0, 0, iw, ih), true);
            }
            else
            {
                tow = iw;
                toh = ih;
            }
            //细化
            AciBmpInt.toSmallLine(ref outBmp, "", false);

            if (show) AciBmpInt.DrawIntBmpPng(file.FullName.Replace(".jpg", "_临时_02.png"), outBmp);

            
            //两头直线
            List<HoughHB> AL_hb_L = houghR_H(outBmp, "left", tow / iLR, 8);
            List<HoughHB> AL_hb_R = houghR_H(outBmp, "right", tow / iLR, 8);

            //合并线
            List<houghLR> AL_LR = new List<houghLR>();
            for (int i = 0; i < AL_hb_L.Count; i++)
            {
                HoughHB houghL = AL_hb_L[i];
                for (int j = 0; j < AL_hb_R.Count; j++)
                {
                    HoughHB houghR = AL_hb_R[j];
                    if (Math.Abs(houghL.b3 - houghR.b3) <= 12)
                    {
                        if (Math.Abs(houghL.b - houghR.b) <= 24 && Math.Abs(houghL.b2 - houghR.b2) <= 24)
                        {
                            houghLR LR = new houghLR();
                            LR.pointB = houghL.pointB;
                            LR.pointE = houghR.pointE;
                            LR.b = houghL.b;
                            LR.b2 = houghR.b2;
                            LR.b3 = (houghL.b3 + houghR.b3) / 2;
                            LR.ik = (float)((float)(LR.b - LR.b2) / (float)(0 - tow));
                            if (pp != 0) LR.setPP(pp);

                            AL_LR.Add(LR);
                        }
                    }
                }
            }

            uint ik = AciBmpInt.getOstu(pixelNum);
            uint ik2 = AciBmpInt.getShang(pixelNum);
            if (ik < ik2)
            {
                ik = ik2;
            }
            if (ik > 240)
            {
                ik = 240;
            }
            AciBmpInt.greyRGB toRGB = AciBmpInt.toTwoAddRGB(p, ik, (uint)iWidth, (uint)iw, (uint)ih, RGB, outBmpHV);

            int inn = 2;
            if (iw > 2000)
            {
                inn = 3;
            }

            //直接处理线
            for (int i = 0; i < AL_LR.Count; i++)
            {
                houghLR LR = AL_LR[i];
                for (int y = 0; y < ih; y++)
                {
                    for (int x = 0; x < iw; x++)
                    {
                        int iy = (int)Math.Round(LR.ik * (float)x + LR.b);
                        if (iy == y)
                        {
                            //AciDebug.Debug(x + " " + y);
                            int B = 0;
                            int G = 0;
                            int R = 0;
                            int it = 0;
                            for (int n = -inn - 1; n < -inn; n++)
                            {
                                int toy = y - n;
                                if (toy >= 0 && toy < ih)
                                {
                                    int ib = (y - n) * iWidth + x * 3;
                                    B += p[ib];
                                    G += p[ib + 1];
                                    R += p[ib + 2];
                                    it++;
                                }
                            }
                            for (int n = inn + 1; n < inn + 2; n++)
                            {
                                int toy = y - n;
                                if (toy >= 0 && toy < ih)
                                {
                                    int ib = (y - n) * iWidth + x * 3;
                                    B += p[ib];
                                    G += p[ib + 1];
                                    R += p[ib + 2];
                                    it++;
                                }
                            }
                            B /= it;
                            G /= it;
                            R /= it;
                            int greys = (B + G + R) / 3;
                            
                            for (int n = -inn; n <= inn; n++)
                            {
                                int toy = y - n;
                                if (toy >= 0 && toy < ih)
                                {
                                    int ib = (y - n) * iWidth + x * 3;
                                    if (greys <= 228)
                                    {
                                        p[ib] = (byte)B;
                                        p[ib + 1] = (byte)G;
                                        p[ib + 2] = (byte)R;
                                    }
                                    else
                                    {
                                        p[ib] = (byte)toRGB.B;
                                        p[ib + 1] = (byte)toRGB.G;
                                        p[ib + 2] = (byte)toRGB.R;
                                    }
                                    //p[ib] = 0;
                                }
                            }

                            //for (int n = -inn; n <= inn; n++)
                            //{
                            //    int toy = y - n;
                            //    if (toy >= 0 && toy < ih)
                            //    {
                            //        int ib = (y - n) * iWidth + x * 3;
                            //        p[ib] = (byte)toRGB.B;
                            //        p[ib + 1] = (byte)toRGB.G;
                            //        p[ib + 2] = (byte)toRGB.R;
                            //        //p[ib] = 0;
                            //    }
                            //}
                        }
                    }
                }
            }
        }
        else
        {
            AciBmpInt.toTwoRGB(p, 240, (uint)iWidth, (uint)iw, (uint)ih, RGB, false);
        }
        

        bmp.UnlockBits(bitData);
        return bmp;
    }

    
    public class houghLR
    {
        public PointF pointB;
        public PointF pointE;
        public float b;
        public float b2;
        public float b3;
        public float ik;

        public houghLR()
        {

        }
        public void setPP(float pp)
        {
            b /= pp;
            b2 /= pp;
            b3 /= pp;
            pointB = new PointF(pointB.X / pp, pointB.Y / pp);
            pointE = new PointF(pointE.X / pp, pointE.Y / pp);
        }
    }

    unsafe static List<HoughHB> houghR_H(int[,] intBmp, string smode, int iLR, float ihb)
    {
        int w = intBmp.GetLength(0);
        int h = intBmp.GetLength(1);

        int irMax = 30;
        int irStep = 1;//0.小数开始
        int rho_max = (int)Math.Floor(Math.Sqrt(w * w + h * h)) + 1; //由原图数组坐标算出ρ最大值，并取整数部分加1
        //此值作为ρ，θ坐标系ρ最大值
        HoughStruct[,] accarray = new HoughStruct[rho_max, 1800]; //定义ρ，θ坐标系的数组，初值为0。
        //θ的最大值，180度

        double[] Theta = new double[1800];
        //定义θ数组，确定θ取值范围
        for (int k = 0; k < irMax * 10; k += irStep)
        {
            Theta[k] = Math.PI / 180 * k / 10;
        }
        for (int k = (180 - irMax) * 10; k < 180 * 10; k += irStep)
        {
            Theta[k] = Math.PI / 180 * k / 10;
        }

        List<Point> AL_hough = new List<Point>();

        int icutNum_X = iLR * 3 / 5;

        int ib = 0;
        int ie = iLR;
        if (smode == "right")
        {
            ib = w - iLR;
            ie = w;
        }

        for (int y = 0; y < h; y++)
        {
            for (int x = ib; x < ie; x++)
            {
                int cc = intBmp[x, y];
                if (cc > 0)
                {
                    for (int k = 0; k < irMax * 10; k += irStep)
                    {
                        setHoughClass((float)x, (float)y, ref accarray, k, rho_max, Theta, ref AL_hough, icutNum_X);
                    }
                    for (int k = (180 - irMax) * 10; k < 180 * 10; k += irStep)
                    {
                        setHoughClass((float)x, (float)y, ref accarray, k, rho_max, Theta, ref AL_hough, icutNum_X);
                    }
                }
            }
        }

        List<HoughClass> AL_houghClass = new List<HoughClass>();
        for (int i = 0; i < AL_hough.Count; i++)
        {
            HoughStruct hough = accarray[AL_hough[i].X, AL_hough[i].Y];
            HoughClass houghClass = new HoughClass();
            houghClass.pointB = hough.pointB;
            houghClass.pointE = hough.pointE;
            houghClass.iNum = hough.iNum;
            houghClass.r = hough.ik;

            houghClass.pointB.Y = houghClass.pointB.Y / (float)hough.bNum;
            houghClass.pointE.Y = houghClass.pointE.Y / (float)hough.eNum;

            //houghClass.k = (float)Math.Tan((float)(Math.PI/180) * hough.ik);
            //houghClass.b = (float)houghClass.pointB.Y - (float)houghClass.pointB.X * houghClass.k;


            houghClass.k = (float)((float)(houghClass.pointB.Y - houghClass.pointE.Y) / (float)(houghClass.pointB.X - houghClass.pointE.X));
            houghClass.b = (float)houghClass.pointB.Y - (float)houghClass.pointB.X * houghClass.k;
            houghClass.b2 = houghClass.k * (w - 1) + houghClass.b;
            houghClass.b3 = houghClass.k * (w / 2) + houghClass.b;
            
            houghClass.r = (float)Math.Atan(houghClass.k) * (float)(180 / Math.PI);
            AL_houghClass.Add(houghClass);
        }

        List<HoughHB> AL_houghHB = new List<HoughHB>();
        for (int i = 0; i < AL_houghClass.Count; i++)
        {
            HoughClass hough = AL_houghClass[i];
            bool bAdd = false;
            for (int j = 0; j < AL_houghHB.Count; j++)
            {
                HoughHB houghHB = AL_houghHB[j];
                if (smode == "right")
                {
                    if (Math.Abs(houghHB.b2 - hough.b2) <= ihb)
                    {
                        bAdd = true;
                        houghHB.Add(hough);
                    }
                }
                else
                {
                    if (Math.Abs(houghHB.b - hough.b) <= ihb)
                    {
                        bAdd = true;
                        houghHB.Add(hough);
                    }
                }
            }
            if (!bAdd)
            {
                AL_houghHB.Add(new HoughHB(hough));
            }
        }

        bool isHb = true;
        while (isHb)
        {
            isHb = false;
            for (int i = 0; i < AL_houghHB.Count; i++)
            {
                HoughHB hough = AL_houghHB[i];

                for (int j = 0; j < AL_houghHB.Count; j++)
                {
                    HoughHB houghHB = AL_houghHB[j];
                    if (hough != houghHB)
                    {
                        if (smode == "right")
                        {
                            if (Math.Abs(houghHB.b2 - hough.b2) <= ihb)
                            {
                                houghHB.Add(hough);
                                AL_houghHB.Remove(hough);
                                isHb = true;
                                break;
                            }
                        }
                        else
                        {
                            if (Math.Abs(houghHB.b - hough.b) <= ihb)
                            {
                                houghHB.Add(hough);
                                AL_houghHB.Remove(hough);
                                isHb = true;
                                break;
                            }
                        }
                    }
                }
                if (isHb) break;
            }
        }

        List<HoughHB> AL_hb = new List<HoughHB>();
        for (int i = 0; i < AL_houghHB.Count; i++)
        {
            HoughHB hough = AL_houghHB[i];
            if (smode == "right")
            {
                if (hough.pointE.X > w - 30)
                {
                    AL_hb.Add(hough);
                }
            }
            else
            {
                if (hough.pointB.X < 30)
                {
                    AL_hb.Add(hough);
                }
            }
        }

        return AL_hb;
    }

    public static void setHoughClass(float x, float y, ref HoughStruct[,] accarray, int k, int rho_max, double[] Theta, ref List<Point> AL_hough, int icutNum)
    {
        //将θ值代入hough变换方程，求ρ值
        double rho = (y * Math.Cos(Theta[k])) + (x * Math.Sin(Theta[k]));
        //将ρ值与ρ最大值的和的一半作为ρ的坐标值（数组坐标），这样做是为了防止ρ值出现负数
        int rho_int = (int)Math.Round(rho / 2 + rho_max / 2);
        //在ρθ坐标（数组）中标识点，即计数累加

        accarray[rho_int, k].iNum++;
        //accarray[rho_int, k].point += x + "," + y + "_";

        if (accarray[rho_int, k].iNum <= 1)
        {
            accarray[rho_int, k].pointB.X = x;
            accarray[rho_int, k].pointB.Y = y;
            accarray[rho_int, k].pointE = accarray[rho_int, k].pointB;
            accarray[rho_int, k].bNum = accarray[rho_int, k].eNum = 1;
        }
        else
        {
            if (accarray[rho_int, k].pointB.X > x)
            {
                accarray[rho_int, k].pointB.X = x;
                accarray[rho_int, k].pointB.Y = y;
                accarray[rho_int, k].bNum = 1;
            }
            else if (accarray[rho_int, k].pointB.X == x)
            {
                accarray[rho_int, k].pointB.Y += y;
                accarray[rho_int, k].bNum++;
            }

            if (accarray[rho_int, k].pointE.X < x)
            {
                accarray[rho_int, k].pointE.X = x;
                accarray[rho_int, k].pointE.Y = y;
                accarray[rho_int, k].eNum = 1;
            }
            else if (accarray[rho_int, k].pointE.X == x)
            {
                accarray[rho_int, k].pointE.Y += y;
                accarray[rho_int, k].eNum++;
            }
        }

        if (accarray[rho_int, k].iNum > icutNum)
        {
            if (!accarray[rho_int, k].isok)
            {
                accarray[rho_int, k].ik = (float)k / 10;
                accarray[rho_int, k].isok = true;
                AL_hough.Add(new Point(rho_int, k));
            }
        }
    }

    public class HoughHB
    {
        public PointF pointB;
        public PointF pointE;
        public int iNum;
        
        public float k;
        public float b;
        public float b2;
        public float b3;
        public float r;
        public float kAll;
        public float bAll;
        public float b2All;
        public float b3All;
        public float rAll;
        public float iall = 0;
        public float ball = 0;
        public float eall = 0;
        public float bYall = 0;
        public float eYall = 0;

        public HoughHB(HoughClass hough)
        {
            pointB = hough.pointB;
            pointE = hough.pointE;
            iNum = hough.iNum;
            r = rAll = hough.r;
            k = kAll = hough.k;
            b = bAll = hough.b;
            b2 = b2All = hough.b2;
            b3 = b3All = hough.b3;
            bYall = hough.pointB.Y;
            eYall = hough.pointE.Y;
            iall = ball = eall = 1;
        }
        public void Add(HoughClass hough)
        {
            rAll += hough.r;
            kAll += hough.k;
            bAll += hough.b;
            b2All += hough.b2;
            b3All += hough.b3;
            iall++;

            r = rAll / iall;
            k = kAll / iall;
            b = bAll / iall;
            b2 = b2All / iall;
            b3 = b3All / iall;

            if (pointB.X > hough.pointB.X)
            {
                pointB.X = hough.pointB.X;
                bYall = hough.pointB.Y;
                ball = 1;
            }
            else if (pointB.X == hough.pointB.X)
            {
                bYall += hough.pointB.Y;
                ball ++;
            }

            if (pointE.X < hough.pointE.X)
            {
                pointE.X = hough.pointE.X;
                eYall = hough.pointE.Y;
                eall = 1;
            }
            else if (pointE.X == hough.pointE.X)
            {
                eYall += hough.pointE.Y;
                eall ++;
            }
            pointB.Y = bYall / ball;
            pointE.Y = eYall / eall;

            //k = (float)((float)(pointB.Y - pointE.Y) / (float)(pointB.X - pointE.X));
            //b = (float)pointB.Y - (float)pointB.X * k;
            //r = (float)Math.Atan(k) * (float)(180 / Math.PI);
        }
        public void Add(HoughHB hough)
        {
            rAll += hough.r;
            kAll += hough.k;
            bAll += hough.b;
            b2All += hough.b2;
            b3All += hough.b3;
            iall++;

            r = rAll / iall;
            k = kAll / iall;
            b = bAll / iall;
            b2 = b2All / iall;
            b3 = b3All / iall;

            if (pointB.X > hough.pointB.X)
            {
                pointB.X = hough.pointB.X;
                bYall = hough.pointB.Y;
                ball = 1;
            }
            else if (pointB.X == hough.pointB.X)
            {
                bYall += hough.pointB.Y;
                ball++;
            }

            if (pointE.X < hough.pointE.X)
            {
                pointE.X = hough.pointE.X;
                eYall = hough.pointE.Y;
                eall = 1;
            }
            else if (pointE.X == hough.pointE.X)
            {
                eYall += hough.pointE.Y;
                eall++;
            }
            pointB.Y = bYall / ball;
            pointE.Y = eYall / eall;

            //k = (float)((float)(pointB.Y - pointE.Y) / (float)(pointB.X - pointE.X));
            //b = (float)pointB.Y - (float)pointB.X * k;
            //r = (float)Math.Atan(k) * (float)(180 / Math.PI);
        }
    }
    public class HoughClass
    {
        public PointF pointB;
        public PointF pointE;
        public int iNum;
        public float k;
        public float b;
        public float b2;
        public float b3;
        public float r;
    }

    public struct HoughStruct
    {
        public PointF pointB;
        public PointF pointE;
        //public string point;
        public int iNum;
        public int bNum;
        public int eNum;
        public float ik;
        public bool isok;
    }
    /// <summary>
    /// 联通区域获取直线 H
    /// </summary>
    /// <param name="intBmp"></param>
    /// <param name="file"></param>
    /// <param name="inum"></param>
    /// <param name="greyAreaX"></param>
    /// <param name="greyAreaY"></param>
    /// <param name="imaxY">Y方向最大可取范围 只算一边</param>
    /// <returns></returns>
    public static List<Rect> cutAreaLineH(int[,] intBmp, FileInfo file, int inum, int ib, int ie, int imaxY)
    {
        int iw = intBmp.GetLength(0);
        int ih = intBmp.GetLength(1);
        List<Rect> arrRect = new List<Rect>();
        List<myDot> Al_sp = null;
        myDot point = null;

        Rect rect = null;
        int dot = 0;

        int[,] arrRect_P = new int[iw, ih];
        for (int y = 0; y < ih; y++)
        {
            for (int x = ib; x < ie; x++)
            {
                if (intBmp[x, y] > 0 && arrRect_P[x, y] == 0)
                {
                    //基点入锥
                    Al_sp = new List<myDot>();
                    arrRect_P[x, y] = 1;
                    Al_sp.Add(new myDot((uint)x, (uint)y));

                    rect = new Rect();
                    rect.l = (uint)x;
                    rect.t = (uint)y;
                    rect.r = (uint)x + 1;
                    rect.b = (uint)y + 1;

                    while (Al_sp.Count != 0)
                    {
                        //取第一个点计算8方向
                        point = (myDot)Al_sp[0];
                        for (int irow = -1; irow <= 1; irow++)
                        {
                            for (int icell = -1; icell <= 1; icell++)
                            {
                                int thisx = (int)point.x + icell;
                                int thisy = (int)point.y + irow;
                                //不超出边界
                                if (thisx >= ib && thisx < ie && thisy > 0 && thisy < ih - 1 && Math.Abs(thisy - y) <= imaxY)
                                {
                                    int grey = intBmp[thisx, thisy];
                                    if (grey > 0 && arrRect_P[thisx, thisy] == 0)
                                    {
                                        //新点入锥
                                        arrRect_P[thisx, thisy] = 1;
                                        myDot dott = new myDot((uint)thisx, (uint)thisy);

                                        Al_sp.Add(dott);
                                        rect.Add(dott);
                                        //重新设置矩形大小
                                        if (thisx < rect.l)
                                        {
                                            rect.l = (uint)thisx;
                                        }
                                        else if (thisx > rect.r)
                                        {
                                            rect.r = (uint)thisx;
                                        }
                                        if (thisy < rect.t)
                                        {
                                            rect.t = (uint)thisy;
                                        }
                                        else if (thisy > rect.b)
                                        {
                                            rect.b = (uint)thisy;
                                        }
                                    }
                                }
                            }
                        }
                        //出锥
                        Al_sp.RemoveAt(0);
                    }
                    if (rect.iNum >= inum)
                    {
                        arrRect.Add(rect);
                    }
                }

            }
        }

        return arrRect;
    }
}

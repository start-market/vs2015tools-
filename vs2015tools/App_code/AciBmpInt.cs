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

public unsafe class AciBmpInt
{
    public AciBmpInt()
    {

    }

    public class dotRGB
    {
        public byte R = 0;
        public byte G = 0;
        public byte B = 0;
        public dotRGB(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }
    }
    public class greyRGB
    {
        public uint R = 0;
        public uint G = 0;
        public uint B = 0;
        public uint iC = 0;
        public greyRGB()
        {

        }
        public void add(uint r, uint g, uint b)
        {
            R += r;
            G += g;
            B += b;
            iC++;
        }
        public dotRGB getRGB()
        {
            return new dotRGB((byte)(R / iC), (byte)(G / iC), (byte)(B / iC));
        }
    }
    /// <summary>
    /// ��ͼƬ���Ե�һ��ָ���� �����С��
    /// </summary>
    /// <param name="iw">�����</param>
    /// <param name="ih">�����</param>
    /// <param name="bmp">ͼƬ</param>
    /// <returns></returns>
    public static Bitmap getBitmapByRect(float iw, float ih, Bitmap bmp)
    {
        float bmpPP = (float)bmp.Width / (float)bmp.Height;
        float pp = iw / ih;
        float ip = 0;


        int iwidth = 0;
        int iheight = 0;

        if (bmpPP > pp)
        {
            //��ģʽ
            iwidth = (int)Math.Round(iw);
            ip = (float)bmp.Width / iwidth;
            iheight = (int)Math.Round((float)bmp.Height / ip);
        }
        else
        {
            //��ģʽ
            iheight = (int)Math.Round(ih);
            ip = (float)bmp.Height / iheight;
            iwidth = (int)Math.Round((float)bmp.Width / ip);
        }

        Bitmap bitmap = new Bitmap(iwidth, iheight);
        Graphics g = Graphics.FromImage(bitmap);
        g.DrawImage(bmp, 0, 0, iwidth, iheight);
        g.Dispose();

        return bitmap;
    }
    public static unsafe dotRGB getAvgGrey(Bitmap bmp)
    {
        BitmapData bitData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        int ic = bitData.Stride - bitData.Width * 3;
        byte* p = (byte*)(bitData.Scan0.ToPointer());
        int iWidth = bmp.Width * 3 + ic;

        dotRGB rgb = getAvgGrey(p, iWidth, bmp.Width, bmp.Height);
        bmp.UnlockBits(bitData);

        return rgb;
    }

    public static unsafe dotRGB getAvgGrey(byte* p, int iWidth, int iw, int ih)
    {
        greyRGB[] arrDot = new greyRGB[256];
        for (int i = 0; i < arrDot.Length; i++)
        {
            arrDot[i] = new greyRGB();
        }

        for (int y = 0; y < ih; y++)
        {
            for (int x = 0; x < iw; x++)
            {
                //��ʼ��
                int ib = y * iWidth + x * 3;
                uint B = p[ib];
                uint G = p[ib + 1];
                uint R = p[ib + 2];
                uint igrey = (R + G + B) / 3;
                if (igrey > 20)
                {
                    arrDot[igrey].add(R, G, B);
                }
            }
        }
        uint imax = 0;
        int imaxit = 0;
        for (int i = 0; i < arrDot.Length; i++)
        {
            if (arrDot[i].iC > imax)
            {
                imax = arrDot[i].iC;
                imaxit = i;
            }
        }

        return arrDot[imaxit].getRGB();
    }
    /// <summary>
    /// ��ֵ��
    /// </summary>
    /// <param name="intBmp"></param>
    /// <param name="ik"></param>
    /// <param name="w"></param>
    /// <param name="h"></param>
    public static void toTwo(ref int[,] intBmp, uint ik, int w, int h)
    {
        for (uint y = 0; y < h; y++)
        {
            for (uint x = 0; x < w; x++)
            {
                if (intBmp[x, y] <= ik)
                {
                    intBmp[x, y] = 0;
                }
            }
        }
    }
    /// <summary>
    /// ��ֵ�� ��ǿ��ɫ�Ĳ��� 
    /// </summary>
    /// <param name="intBmp"></param>
    /// <param name="ik"></param>
    /// <param name="w"></param>
    /// <param name="h"></param>
    /// <param name="outBmp">��Ե����ɫ</param>
    public static greyRGB toTwoAddRGB(byte* p, uint ik, uint iWidth, uint iw, uint ih, dotRGB RGB, int[,] outBmp)
    {
        int toB = (int)Math.Round((float)RGB.B * 1.1f); if (toB > 255) toB = 255;
        int toG = (int)Math.Round((float)RGB.G * 1.1f); if (toG > 255) toG = 255;
        int toR = (int)Math.Round((float)RGB.R * 1.1f); if (toR > 255) toR = 255;

        for (uint y = 0; y < ih; y++)
        {
            for (uint x = 0; x < iw; x++)
            {
                uint ib = y * iWidth + x * 3;
                int B = (int)p[ib];
                int G = (int)p[ib + 1];
                int R = (int)p[ib + 2];
                int igrey = (B + G + R) / 3;
                bool isBG = false;

                if (igrey > ik)
                {
                    if (outBmp[x, y] > 0)
                    {
                        isBG = false;
                    }
                    else
                    {
                        isBG = true;
                    }
                }
                else
                {
                    isBG = false;
                }

                if (isBG)
                {
                    p[ib] = (byte)toB;
                    p[ib + 1] = (byte)toG;
                    p[ib + 2] = (byte)toR;
                }
                else
                {
                    B -= (255 - toB);
                    G -= (255 - toG);
                    R -= (255 - toR);
                    //B -= (255 - (int)Math.Round((float)RGB.B * 0.95f));
                    //G -= (255 - (int)Math.Round((float)RGB.G * 0.95f));
                    //R -= (255 - (int)Math.Round((float)RGB.R * 0.95f));
                    if (B < 0) B = 0;
                    if (G < 0) G = 0;
                    if (R < 0) R = 0;
                    p[ib] = (byte)B;
                    p[ib + 1] = (byte)G;
                    p[ib + 2] = (byte)R;
                }
            }
        }
        greyRGB rgb = new greyRGB();
        rgb.R = (uint)toR;
        rgb.G = (uint)toG;
        rgb.B = (uint)toB;
        return rgb;
        //for (uint y = 0; y < ih; y++)
        //{
        //    for (uint x = 0; x < iw; x++)
        //    {
        //        uint ib = y * iWidth + x * 3;
        //        int B = (int)p[ib];
        //        int G = (int)p[ib + 1];
        //        int R = (int)p[ib + 2];
        //        int igrey = (B + G + R) / 3;
        //        if (igrey > ik
        //            && R - (B + G) /2 <= 10
        //            && G - (R + B) / 2 <= 20
        //            && B - (R + G) / 2 <= 20)
        //        {
        //            p[ib] = (byte)toB;
        //            p[ib + 1] = (byte)toG;
        //            p[ib + 2] = (byte)toR;
        //        }
        //        else 
        //        {
        //            if (igrey <= ik)
        //            {
        //                B -= (255 - (int)Math.Round((float)RGB.B * 0.88f));
        //                G -= (255 - (int)Math.Round((float)RGB.G * 0.88f));
        //                R -= (255 - (int)Math.Round((float)RGB.R * 0.88f));
        //                if (B < 0) B = 0;
        //                if (G < 0) G = 0;
        //                if (R < 0) R = 0;

        //                p[ib] = (byte)B;
        //                p[ib + 1] = (byte)G;
        //                p[ib + 2] = (byte)R;
        //            }
        //            else
        //            {
        //                B -= (255 - toB);
        //                G -= (255 - toG);
        //                R -= (255 - toR);
        //                if (B < 0) B = 0;
        //                if (G < 0) G = 0;
        //                if (R < 0) R = 0;
        //                p[ib] = (byte)B;
        //                p[ib + 1] = (byte)G;
        //                p[ib + 2] = (byte)R;
        //            }
        //        }
        //    }
        //}
    }
    /// <summary>
    /// ��ֵ��
    /// </summary>
    /// <param name="intBmp"></param>
    /// <param name="ik"></param>
    /// <param name="w"></param>
    /// <param name="h"></param>
    public static void toTwo(byte* p, uint ik, uint iWidth, uint iw, uint ih)
    {
        for (uint y = 0; y < ih; y++)
        {
            for (uint x = 0; x < iw; x++)
            {
                uint ib = y * iWidth + x * 3;
                int B = (int)p[ib];
                int G = (int)p[ib + 1];
                int R = (int)p[ib + 2];
                int igrey = (B + G + R) / 3;
                if (igrey > ik)
                {
                    p[ib] = 255;
                    p[ib + 1] = 255;
                    p[ib + 2] = 255;
                }
            }
        }
    }
    /// <summary>
    /// ��ֵ�� RGB
    /// </summary>
    /// <param name="intBmp"></param>
    /// <param name="ik"></param>
    /// <param name="w"></param>
    /// <param name="h"></param>
    /// <param name="isAdd">�Ƿ�RGB����ȥ</param>
    public static void toTwoRGB(byte* p, uint ik, uint iWidth, uint iw, uint ih, AciBmpInt.dotRGB RGB, bool isAdd)
    {
        int iGrey = ((int)RGB.B + (int)RGB.R + (int)RGB.G) / 3;
        int igreyAvg = 255 - iGrey;
        int igreyAvg2 = 255 - (255 - iGrey) / 2;

        for (uint y = 0; y < ih; y++)
        {
            for (uint x = 0; x < iw; x++)
            {
                uint ib = y * iWidth + x * 3;
                int B = (int)p[ib];
                int G = (int)p[ib + 1];
                int R = (int)p[ib + 2];
                int igrey = (B + G + R) / 3;
                if (igrey + igreyAvg > ik)
                {
                    if (igrey < igreyAvg2)
                    {
                        int iB = B + 255 - (int)RGB.B;
                        int iG = G + 255 - (int)RGB.G;
                        int iR = R + 255 - (int)RGB.R;
                        if (iB > 255) iB = 255;
                        if (iG > 255) iG = 255;
                        if (iR > 255) iR = 255;

                        if (iR - (iB + iG) / 2 <= 0)
                        {
                            p[ib] = RGB.B;
                            p[ib + 1] = RGB.G;
                            p[ib + 2] = RGB.R;
                        }
                        else if (iR - (iB + iG) / 2 > 10)
                        {
                            B = (int)Math.Round((float)B * 0.8f);
                            G = (int)Math.Round((float)G * 0.8f);
                            R = (int)Math.Round((float)R * 0.8f);
                            if (B < 0) B = 0;
                            if (G < 0) G = 0;
                            if (R < 0) R = 0;

                            p[ib] = (byte)B;
                            p[ib + 1] = (byte)G;
                            p[ib + 2] = (byte)R;
                        }
                    }
                }
                else if (isAdd)
                {
                    B -= (255 - RGB.B);
                    G -= (255 - RGB.G);
                    R -= (255 - RGB.R);
                    if (B < 0) B = 0;
                    if (G < 0) G = 0;
                    if (R < 0) R = 0;
                    p[ib] = (byte)B;
                    p[ib + 1] = (byte)G;
                    p[ib + 2] = (byte)R;
                }
            }
        }
    }
    /// <summary>
    /// ��ֵ�� �ڰ׻�
    /// </summary>
    /// <param name="intBmp"></param>
    /// <param name="ik"></param>
    /// <param name="w"></param>
    /// <param name="h"></param>
    public static void toTwo01(ref int[,] intBmp, uint ik, int w, int h)
    {
        for (uint y = 0; y < h; y++)
        {
            for (uint x = 0; x < w; x++)
            {
                if (intBmp[x, y] <= ik)
                {
                    intBmp[x, y] = 0;
                }
                else
                {
                    intBmp[x, y] = 255;
                }
            }
        }
    }

    /// <summary>
    /// �ҶȻ�
    /// </summary>
    /// <param name="intBmp"></param>
    /// <param name="ik"></param>
    /// <param name="w"></param>
    /// <param name="h"></param>
    public static Bitmap toGreyBmp(ref Bitmap bmp)
    {
        uint iw = (uint)bmp.Width;
        uint ih = (uint)bmp.Height;

        BitmapData bitData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        int ic = bitData.Stride - bitData.Width * 3;
        byte* p = (byte*)(bitData.Scan0.ToPointer());
        uint iWidth = iw * 3 + (uint)ic;


        for (uint y = 0; y < ih; y++)
        {
            for (uint x = 0; x < iw; x++)
            {
                uint ib = y * iWidth + x * 3;
                int B = (int)p[ib];
                int G = (int)p[ib + 1];
                int R = (int)p[ib + 2];
                int igrey = (B + G + R) / 3;

                p[ib] = p[ib + 1] = p[ib + 2] = (byte)igrey;
            }
        }
        bmp.UnlockBits(bitData);
        return bmp;
    }
    /// <summary>
    /// ��ȡ�ڲ�ʵ����Ч���� �� Rectangle
    /// </summary>
    /// <param name="intBmp">����intͼ</param>
    /// <param name="ik">��ֵ����ֵ</param>
    /// <returns></returns>
    public static Rectangle getRect(ref int[,] intBmp, int ik)
    {
        Rectangle rect = new Rectangle();
        int iw = intBmp.GetLength(0);
        int ih = intBmp.GetLength(1);
        int l = 0;
        int r = 0;
        int t = 0;
        int b = 0;
        for (int y = 0; y < ih; y++)
        {
            for (int x = 0; x < iw; x++)
            {
                if (intBmp[x, y] > ik)
                {
                    if (t == 0) t = y;
                    break;
                }
            }
        }
        for (int y = ih - 1; y > 0; y--)
        {
            for (int x = 0; x < iw; x++)
            {
                if (intBmp[x, y] > ik)
                {
                    if (b == 0) b = y;
                    break;
                }
            }
        }
        for (int x = 0; x < iw; x++)
        {
            for (int y = 0; y < ih; y++)
            {
                if (intBmp[x, y] > ik)
                {
                    if (l == 0) l = x;
                    break;
                }
            }
        }
        for (int x = iw - 1; x > 0; x--)
        {
            for (int y = 0; y < ih; y++)
            {
                if (intBmp[x, y] > ik)
                {
                    if (r == 0) r = x;
                    break;
                }
            }
        }
        rect.X = l;
        rect.Y = t;

        rect.Width = (r - l) + 1;
        rect.Height = (b - t) + 1;
        return rect;
    }


    /// <summary>
    /// ��ȡ���� ���� List Point 
    /// </summary>
    public static unsafe List<Point> getFrameP(int[,] intBmp, int cc)
    {
        //��ȡ����
        List<Point> AL_point = new List<Point>();
        //int[,] outBmp = new int[intBmp.GetLength(0), intBmp.GetLength(1)];
        for (int j = 1; j < intBmp.GetLength(1) - 1; j++)
        {
            for (int i = 1; i < intBmp.GetLength(0) - 1; i++)
            {
                int _5 = intBmp[i, j];
                if (_5 > cc)
                {
                    int _1 = intBmp[i - 1, j - 1];
                    int _2 = intBmp[i, j - 1];
                    int _3 = intBmp[i + 1, j - 1];
                    int _4 = intBmp[i - 1, j];

                    int _6 = intBmp[i + 1, j];
                    int _7 = intBmp[i - 1, j + 1];
                    int _8 = intBmp[i, j + 1];
                    int _9 = intBmp[i + 1, j + 1];


                    if (_1 >= cc && _2 >= cc && _3 >= cc
                        && _3 >= cc && _5 >= cc && _6 >= cc
                        && _7 >= cc && _8 >= cc && _9 >= cc)
                    {

                    }
                    else
                    {
                        //outBmp[i, j] = intBmp[i, j];
                        //outBmp[i, j] = 254;
                        AL_point.Add(new Point(i, j));
                    }
                }
            }
        }
        return AL_point;
    }

    /// <summary>
    /// ��ȡ���� ���� һ��ȫ�µ�  outBmp
    /// </summary>
    /// <param name="intBmp"></param>
    /// <param name="cc">��Ե��Сֵ 1 - 10 ֮��</param>
    /// <returns></returns>
    public static int[,] getFrame(int[,] intBmp, int cc)
    {
        int iw = intBmp.GetLength(0);
        int ih = intBmp.GetLength(1);
        //��ȡ����
        //List<Point> AL_point = new List<Point>();
        int[,] outBmp = new int[iw, ih];
        for (int j = 1; j < ih - 1; j++)
        {
            for (int i = 1; i < iw - 1; i++)
            {
                int _5 = intBmp[i, j];
                if (_5 > cc)
                {
                    int _1 = intBmp[i - 1, j - 1];
                    int _2 = intBmp[i, j - 1];
                    int _3 = intBmp[i + 1, j - 1];
                    int _4 = intBmp[i - 1, j];

                    int _6 = intBmp[i + 1, j];
                    int _7 = intBmp[i - 1, j + 1];
                    int _8 = intBmp[i, j + 1];
                    int _9 = intBmp[i + 1, j + 1];


                    if (_1 >= cc && _2 >= cc && _3 >= cc
                        && _3 >= cc && _5 >= cc && _6 >= cc
                        && _7 >= cc && _8 >= cc && _9 >= cc)
                    {

                    }
                    else
                    {
                        //outBmp[i, j] = intBmp[i, j];
                        outBmp[i, j] = 255;
                        //AL_point.Add(new Point(i, j));
                    }
                }
            }
        }
        return outBmp;
    }
    /// <summary>
    /// ��ȡ���� ���� һ��ȫ�µ�  outBmp
    /// </summary>
    /// <param name="intBmp"></param>
    /// <param name="cc">��Ե��Сֵ 1 - 10 ֮��</param>
    /// <returns></returns>
    public static int[,] getFrameHV(int[,] intBmp, int cc, List<HoughHbClass> AL_hb_H, List<HoughHbClass> AL_hb_V)
    {
        int iw = intBmp.GetLength(0);
        int ih = intBmp.GetLength(1);
        //��ȡ����
        //List<Point> AL_point = new List<Point>();
        int[,] outBmp = new int[iw, ih];
        if (AL_hb_H != null)
        {
            for (int h = 0; h < AL_hb_H.Count; h++)
            {
                HoughHbClass hb = AL_hb_H[h];
                for (int j = hb.iB; j < hb.iE; j++)
                {
                    if (j > 0 && j < ih)
                    {
                        for (int i = 1; i < iw - 1; i++)
                        {
                            int _5 = intBmp[i, j];
                            if (_5 > cc)
                            {
                                int _1 = intBmp[i - 1, j - 1];
                                int _2 = intBmp[i, j - 1];
                                int _3 = intBmp[i + 1, j - 1];
                                int _4 = intBmp[i - 1, j];

                                int _6 = intBmp[i + 1, j];
                                int _7 = intBmp[i - 1, j + 1];
                                int _8 = intBmp[i, j + 1];
                                int _9 = intBmp[i + 1, j + 1];


                                if (_1 >= cc && _2 >= cc && _3 >= cc
                                    && _3 >= cc && _5 >= cc && _6 >= cc
                                    && _7 >= cc && _8 >= cc && _9 >= cc)
                                {

                                }
                                else
                                {
                                    //outBmp[i, j] = intBmp[i, j];
                                    outBmp[i, j] = 255;
                                    //AL_point.Add(new Point(i, j));
                                }
                            }
                        }
                    }
                }
            }
        }
        if (AL_hb_V != null)
        {
            for (int v = 0; v < AL_hb_V.Count; v++)
            {
                HoughHbClass hb = AL_hb_V[v];
                for (int j = 1; j < ih - 1; j++)
                {
                    for (int i = hb.iB; i < hb.iE; i++)
                    {
                        if (i > 0 && i < iw)
                        {
                            int _5 = intBmp[i, j];
                            if (_5 > cc)
                            {
                                int _1 = intBmp[i - 1, j - 1];
                                int _2 = intBmp[i, j - 1];
                                int _3 = intBmp[i + 1, j - 1];
                                int _4 = intBmp[i - 1, j];

                                int _6 = intBmp[i + 1, j];
                                int _7 = intBmp[i - 1, j + 1];
                                int _8 = intBmp[i, j + 1];
                                int _9 = intBmp[i + 1, j + 1];


                                if (_1 >= cc && _2 >= cc && _3 >= cc
                                    && _3 >= cc && _5 >= cc && _6 >= cc
                                    && _7 >= cc && _8 >= cc && _9 >= cc)
                                {

                                }
                                else
                                {
                                    //outBmp[i, j] = intBmp[i, j];
                                    outBmp[i, j] = 255;
                                    //AL_point.Add(new Point(i, j));
                                }
                            }
                        }
                    }
                }
            }
        }
        return outBmp;
    }
    /// <summary>
    /// ���� ��ȡ�����Ҷ�ͼ �� H ���� V �����ֱ�� �м����
    /// </summary>
    public static int[,] getFrameHV_0(int[,] intBmp)
    {
        int iw = intBmp.GetLength(0);
        int ih = intBmp.GetLength(1);
        //��ȡ����
        int[,] outBmp = new int[iw, ih];
        for (int j = 1; j < ih - 1; j++)
        {
            for (int i = 1; i < iw - 1; i++)
            {
                int _1 = intBmp[i - 1, j - 1];
                int _2 = intBmp[i, j - 1];
                int _3 = intBmp[i + 1, j - 1];
                int _4 = intBmp[i - 1, j];
                int _5 = intBmp[i, j];
                int _6 = intBmp[i + 1, j];
                int _7 = intBmp[i - 1, j + 1];
                int _8 = intBmp[i, j + 1];
                int _9 = intBmp[i + 1, j + 1];
                
                int cc = 1;
                if ((_1 - _4 >= cc && _2 - _5 >= cc && _3 - _6 >= cc)
                    || (_7 - _4 >= cc && _8 - _5 >= cc && _9 - _6 >= cc)
                    || (_1 - _2 >= cc && _4 - _5 >= cc && _7 - _8 >= cc)
                    || (_3 - _2 >= cc && _6 - _5 >= cc && _9 - _8 >= cc))
                {
                    outBmp[i, j] = 255;
                }
            }
        }
        return outBmp;
    }
    /// <summary>
    /// ���� ��ȡ�����Ҷ�ͼ �� H ���� ��ֱ�� �м����
    /// </summary>
    public static int[,] getFrameH_0(int[,] intBmp)
    {
        return getFrameHV_1(intBmp, true);
    }
    /// <summary>
    /// ���� ��ȡ�����Ҷ�ͼ �� H ���� ��ֱ�� �м����
    /// </summary>
    public static int[,] getFrameH_0(int[,] intBmp, bool isTwo)
    {
        int iw = intBmp.GetLength(0);
        int ih = intBmp.GetLength(1);
        //��ȡ����
        int[,] outBmp = new int[iw, ih];
        for (int j = 1; j < ih - 1; j++)
        {
            for (int i = 1; i < iw - 1; i++)
            {
                int _1 = intBmp[i - 1, j - 1];
                int _2 = intBmp[i, j - 1];
                int _3 = intBmp[i + 1, j - 1];
                int _4 = intBmp[i - 1, j];
                int _5 = intBmp[i, j];
                int _6 = intBmp[i + 1, j];
                int _7 = intBmp[i - 1, j + 1];
                int _8 = intBmp[i, j + 1];
                int _9 = intBmp[i + 1, j + 1];

                int cc = 1;
                if ((_1 - _4 >= cc && _2 - _5 >= cc && _3 - _6 >= cc)
                    || (_7 - _4 >= cc && _8 - _5 >= cc && _9 - _6 >= cc))
                {
                    if (isTwo)
                    {
                        outBmp[i, j] = 255;
                    }
                    else
                    {
                        outBmp[i, j] = _5;
                    }
                }
            }
        }
        return outBmp;
    }
    /// <summary>
    /// ʵ�� ��ȡ�����Ҷ�ͼ �� H ���� V �����ֱ�� �м�ʵ��
    /// </summary>
    public static int[,] getFrameHV_1(int[,] intBmp)
    {
        return getFrameHV_1(intBmp, true);
    }
    /// <summary>
    /// ʵ�� ��ȡ�����Ҷ�ͼ �� H ���� V �����ֱ�� �м�ʵ��
    /// </summary>
    public static int[,] getFrameHV_1(int[,] intBmp, bool isTwo)
    {
        return getFrameHV_1(intBmp, isTwo, 1);
    }
    /// <summary>
    /// ʵ�� ��ȡ�����Ҷ�ͼ �� H ���� V �����ֱ�� �м�ʵ��
    /// </summary>
    /// <param name="intBmp"></param>
    /// <param name="isTwo"></param>
    /// <param name="cc">����������ֵ1-20</param>
    /// <returns></returns>
    public static int[,] getFrameHV_1(int[,] intBmp, bool isTwo, int cc)
    {
        int iw = intBmp.GetLength(0);
        int ih = intBmp.GetLength(1);
        //��ȡ����
        int[,] outBmp = new int[iw, ih];
        for (int j = 1; j < ih - 1; j++)
        {
            for (int i = 1; i < iw - 1; i++)
            {
                int _1 = intBmp[i - 1, j - 1];
                int _2 = intBmp[i, j - 1];
                int _3 = intBmp[i + 1, j - 1];
                int _4 = intBmp[i - 1, j];
                int _5 = intBmp[i, j];
                int _6 = intBmp[i + 1, j];
                int _7 = intBmp[i - 1, j + 1];
                int _8 = intBmp[i, j + 1];
                int _9 = intBmp[i + 1, j + 1];

                if ((_4 - _1 >= cc && _5 - _2 >= cc && _6 - _3 >= cc)
                    || (_4 - _7 >= cc && _5 - _8 >= cc && _6 - _9 >= cc)
                    || (_2 - _1 >= cc && _5 - _4 >= cc && _8 - _7 >= cc)
                    || (_2 - _3 >= cc && _5 - _6 >= cc && _8 - _9 >= cc))
                {
                    if (isTwo)
                    {
                        outBmp[i, j] = 255;
                    }
                    else
                    {
                        outBmp[i, j] = _5;
                    }
                }
            }
        }
        return outBmp;
    }
    /// <summary>
    /// ʵ�� ��ȡ�����Ҷ�ͼ �� H ���� V �����ֱ�� �м�ʵ��
    /// </summary>
    /// <param name="intBmp"></param>
    /// <param name="isTwo"></param>
    /// <param name="ccMin">��̬����������ֵ��Сֵ 1</param>
    /// <param name="ccMax">��̬����������ֵ���ֵ 10</param>
    /// <returns></returns>
    public static int[,] getFrameHV_1(int[,] intBmp, bool isTwo, int ccMin, int ccMax)
    {
        int iw = intBmp.GetLength(0);
        int iw2 = iw / 2;
        int ih = intBmp.GetLength(1);
        int ih2 = ih / 2;
        double iww = (double)(iw2 * iw2);
        double ihh = (double)(ih2 * ih2);

        int ccAvg = (ccMin + ccMax) / 2;
        //��ȡ����
        int[,] outBmp = new int[iw, ih];
        for (int j = 1; j < ih - 1; j++)
        {
            for (int i = 1; i < iw - 1; i++)
            {
                int _1 = intBmp[i - 1, j - 1];
                int _2 = intBmp[i, j - 1];
                int _3 = intBmp[i + 1, j - 1];
                int _4 = intBmp[i - 1, j];
                int _5 = intBmp[i, j];
                int _6 = intBmp[i + 1, j];
                int _7 = intBmp[i - 1, j + 1];
                int _8 = intBmp[i, j + 1];
                int _9 = intBmp[i + 1, j + 1];

                double fx = i <= iw2 ? (double)(iw2 - i) : (double)(i - iw2);
                double fy = j <= ih2 ? (double)(ih2 - j) : (double)(j - ih2);
                fx = (fx * fx) / iww;
                fy = (fy * fy) / ihh;
                //fx /= (double)iw2;
                //fy /= (double)ih2;

                int cc = (int)Math.Ceiling(((fx + fy) / 2f * (double)ccMax));
                if (cc < ccMin) cc = ccMin;
                //AciDebug.Debug(i + " " + j + " " + cc + " " + fx + " " + fy + " " + (fx + fy) / 2);
                

                if ((_4 - _1 >= cc && _5 - _2 >= cc && _6 - _3 >= cc)
                    || (_4 - _7 >= cc && _5 - _8 >= cc && _6 - _9 >= cc)
                    || (_2 - _1 >= cc && _5 - _4 >= cc && _8 - _7 >= cc)
                    || (_2 - _3 >= cc && _5 - _6 >= cc && _8 - _9 >= cc))
                {
                    if (isTwo)
                    {
                        outBmp[i, j] = 255;
                    }
                    else
                    {
                        outBmp[i, j] = _5;
                    }
                }
            }
        }
        return outBmp;
    }
    /// <summary>
    /// ���  int[] ��ͼƬ  Bitmap û�йر�
    /// </summary>
    /// <param name="filepath"></param>
    public static int[,] getIntBmpByBitmap(string filepath)
    {
        Bitmap bmp = (Bitmap)Image.FromFile(filepath);
        return getIntBmpByBitmap(ref bmp);
    }
    /// <summary>
    /// ���  int[] ��ͼƬ  Bitmap û�йر�
    /// </summary>
    /// <param name="filepath"></param>
    public static int[,] getIntBmpByBitmap(ref Bitmap bmp)
    {
        int[,] intBmp = new int[bmp.Width, bmp.Height];
        BitmapData bitData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        uint ic = (uint)(bitData.Stride - bitData.Width * 3);
        byte* p = (byte*)(bitData.Scan0.ToPointer());
        uint iWidth = (uint)bmp.Width * 3 + ic;
        
        for (int y = 0; y < bmp.Height; y++)
        {
            for (int x = 0; x < bmp.Width; x++)
            {
                uint ib = (uint)y * iWidth + (uint)x * 3;
                //int igrey = 255 - (int)(0.11 * (double)p[ib + 2] + 0.59 * (double)p[ib + 1] + 0.3 * (double)p[ib]);
                int igrey = 255 - (int)(p[ib] + p[ib + 1] + p[ib + 2]) / 3;
                if (igrey > 0)
                {
                    intBmp[x, y] = igrey;
                }
            }
        }
        bmp.UnlockBits(bitData);
        return intBmp;
    }
    /// <summary>
    /// ����  int[] ��ͼƬ ����� ƽ��RGB  Bitmap û�йر�
    /// </summary>
    /// <param name="filepath"></param>
    public static dotRGB getIntBmpCutRGB_ByBitmap(ref Bitmap bmp, ref int[,] intBmp, AciBmpInt.dotRGB rgb)
    {
        BitmapData bitData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        int ic = bitData.Stride - bitData.Width * 3;
        byte* p = (byte*)(bitData.Scan0.ToPointer());
        int iWidth = bmp.Width * 3 + ic;

        if (rgb == null)
        {
            rgb = getAvgGrey(p, iWidth, bmp.Width, bmp.Height);
        }
        for (int y = 0; y < bmp.Height; y++)
        {
            for (int x = 0; x < bmp.Width; x++)
            {
                //��ʼ��
                int ib = y * iWidth + x * 3;
                int B = p[ib];
                int G = p[ib + 1];
                int R = p[ib + 2];

                B += 255 - (int)rgb.B;
                G += 255 - (int)rgb.G;
                R += 255 - (int)rgb.R;
                if (B > 255f) B = 255;
                if (G > 255f) G = 255;
                if (R > 255f) R = 255;

                int igrey = (R + G + B) / 3;

                if (igrey > 0)
                {
                    intBmp[x, y] = 255 - igrey;
                }
            }
        }
        bmp.UnlockBits(bitData);


        return rgb;
    }
    /// <summary>
    /// ���  int[] ��ͼƬ 
    /// </summary>
    /// <param name="filepath"></param>
    /// <param name="imode">0 ��y ��x  1 ��x ��y</param>
    public static int[] getIntArrayByBitmap(string filepath, int imode)
    {
        Bitmap bmp = (Bitmap)Image.FromFile(filepath);
        return getIntArrayByBitmap(ref bmp, imode);
    }
    /// <summary>
    /// ���  int[] ��ͼƬ 
    /// </summary>
    /// <param name="filepath"></param>
    /// <param name="imode">0 ��y ��x  1 ��x ��y</param>
    public static int[] getIntArrayByBitmap(ref Bitmap bmp, int imode)
    {
        BitmapData bitData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        uint ic = (uint)(bitData.Stride - bitData.Width * 3);
        byte* p = (byte*)(bitData.Scan0.ToPointer());
        uint iWidth = (uint)bmp.Width * 3 + ic;

        int[] arr = new int[bmp.Width * bmp.Height];
        for (int y = 0; y < bmp.Height; y++)
        {
            for (int x = 0; x < bmp.Width; x++)
            {
                uint ib = (uint)y * iWidth + (uint)x * 3;
                if (p[ib] > 0)
                {
                    if (imode == 0)
                    {
                        arr[y * bmp.Width + x] = 255;
                    }
                    else
                    {
                        arr[x * bmp.Height + y] = 255;
                    }
                }
            }
        }
        bmp.UnlockBits(bitData);
        bmp.Dispose();
        return arr;
    }

    /// <summary>
    /// ���  int[] ��ͼƬ 
    /// </summary>
    /// <param name="filepath"></param>
    /// <param name="imode">0 ��y ��x  1 ��x ��y</param>
    public static int[] getBmpIntByArray(int[,] intBmp, int imode)
    {
        int iw = intBmp.GetLength(0);
        int ih = intBmp.GetLength(1);

        int[] arr = new int[iw * ih];
        for (int y = 0; y < ih; y++)
        {
            for (int x = 0; x < iw; x++)
            {
                if (intBmp[x, y] > 0)
                {
                    if (imode == 0)
                    {
                        arr[y * iw + x] = 255;
                    }
                    else
                    {
                        arr[x * ih + y] = 255;
                    }
                }
            }
        }
        return arr;
    }
    /// <summary>
    /// ��� intBmp ��ͼƬ 
    /// </summary>
    /// <param name="savepath"></param>
    /// <param name="intBmp"></param>
    public static void DrawIntBmpPng_Blank(string savepath, int[,] intBmp)
    {
        int w = intBmp.GetLength(0);
        int h = intBmp.GetLength(1);
        Bitmap bmp = new Bitmap(w, h);
        Graphics g = Graphics.FromImage(bmp);
        g.Clear(Color.Black);
        g.Dispose();


        BitmapData bitData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        uint ic = (uint)(bitData.Stride - bitData.Width * 3);
        byte* p = (byte*)(bitData.Scan0.ToPointer());
        uint iWidth = (uint)w * 3 + ic;

        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                uint ib = (uint)j * iWidth + (uint)i * 3;
                if (intBmp[i, j] > 0)
                {
                    p[ib] = p[ib + 1] = p[ib + 2] = (byte)intBmp[i, j];
                }
            }
        }
        bmp.UnlockBits(bitData);
        bmp.Save(savepath, ImageFormat.Png);
        bmp.Dispose();
    }
    /// <summary>
    /// ��� intBmp ��ͼƬ 
    /// </summary>
    /// <param name="savepath"></param>
    /// <param name="intBmp"></param>
    public static void DrawIntBmpPng(string savepath, int[,] intBmp)
    {
        int w = intBmp.GetLength(0);
        int h = intBmp.GetLength(1);
        Bitmap bmp = new Bitmap(w, h);
        Graphics g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        g.Dispose();


        BitmapData bitData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        uint ic = (uint)(bitData.Stride - bitData.Width * 3);
        byte* p = (byte*)(bitData.Scan0.ToPointer());
        uint iWidth = (uint)w * 3 + ic;

        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                uint ib = (uint)j * iWidth + (uint)i * 3;
                if (intBmp[i, j] > 0)
                {
                    p[ib] = p[ib + 1] = p[ib + 2] = (byte)(255 - intBmp[i, j]);
                }
                else if (intBmp[i, j] == -1)
                {
                    p[ib] = 0;
                    p[ib + 1] = 0;
                    p[ib + 2] = 255;
                }
                else if (intBmp[i, j] == -2)
                {
                    p[ib] = 255;
                    p[ib + 1] = 0;
                    p[ib + 2] = 0;
                }
                else if (intBmp[i, j] == -3)
                {
                    p[ib] = 0;
                    p[ib + 1] = 255;
                    p[ib + 2] = 0;
                }
            }
        }
        bmp.UnlockBits(bitData);
        bmp.Save(savepath, ImageFormat.Png);
        bmp.Dispose();
    }

    /// <summary>
    /// ��� Bitmap ��ͼƬ  δ�ر�
    /// </summary>
    /// <param name="savepath"></param>
    /// <param name="intBmp"></param>
    public static Bitmap DrawIntBmpPng(int[,] intBmp)
    {
        int w = intBmp.GetLength(0);
        int h = intBmp.GetLength(1);
        Bitmap bmp = new Bitmap(w, h);
        Graphics g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        g.Dispose();

        BitmapData bitData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        uint ic = (uint)(bitData.Stride - bitData.Width * 3);
        byte* p = (byte*)(bitData.Scan0.ToPointer());
        uint iWidth = (uint)w * 3 + ic;

        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                uint ib = (uint)j * iWidth + (uint)i * 3;
                if (intBmp[i, j] > 0)
                {
                    p[ib] = p[ib + 1] = p[ib + 2] = (byte)(255 - (byte)intBmp[i, j]);
                }
            }
        }
        bmp.UnlockBits(bitData);
        return bmp;
    }
    /// <summary>
    /// ����ֱ�ߣ���Ҫ����debug����
    /// </summary>
    /// <param name="AL_hb_H"></param>
    /// <param name="AL_hb_V"></param>
    /// <param name="isHb_ok">�Ƿ����� HB.isok ��ֱ��</param>
    /// <param name="iw"></param>
    /// <param name="ih"></param>
    /// <param name="isTwo">�Ƿ��ֵ��</param>
    /// <param name="mode">�� ΪarrLength 255 Ϊ arrLength255 long Ϊ arrLengthLong </param>
    /// <returns></returns>
    public static int[,] getIntBmp_hb(List<HoughHbClass> AL_hb_H, List<HoughHbClass> AL_hb_V, bool isHb_ok, int iw, int ih, bool isTwo, string smode)
    {
        int[,] testBmp = new int[iw, ih];
        if (AL_hb_H != null)
        {
            for (int i = 0; i < AL_hb_H.Count; i++)
            {
                HoughHbClass hb = (HoughHbClass)AL_hb_H[i];
                if (hb.isok || !isHb_ok)
                {
                    for (int x = 0; x < hb.arrLength.Length; x++)
                    {
                        if (isTwo)
                        {
                            if (hb.arrLength[x] > 0)
                            {
                                testBmp[x, (int)hb.ixy] = 255;
                            }
                        }
                        else
                        {
                            int ic = 0;
                            int inum = 0;
                            if (smode == "")
                            {
                                inum = hb.arrLength[x];
                            }
                            else if (smode == "long")
                            {
                                inum = hb.arrLengthLong[x];
                            }
                            else
                            {
                                inum = hb.arrLength255[x];
                            }
                            if (inum > 0)
                            {
                                int index = 0;
                                while (inum > 0)
                                {
                                   
                                    if (index % 2 == 1)
                                    {
                                        ic = -1;
                                    }
                                    else
                                    {
                                        ic = 1;
                                    }
                                    ic *= (int)Math.Ceiling((float)index / 2f);

                                    if (inum > 255)
                                    {
                                        testBmp[x, (int)hb.ixy + ic] = 255;
                                    }
                                    else
                                    {
                                        testBmp[x, (int)hb.ixy + ic] = inum;
                                    }

                                    inum -= 255;
                                    index++;
                                }
                            }
                        }
                    }
                }
            }
        }
        if (AL_hb_V != null)
        {
            for (int i = 0; i < AL_hb_V.Count; i++)
            {
                HoughHbClass hb = (HoughHbClass)AL_hb_V[i];
                if (hb.isok || !isHb_ok)
                {
                    for (int y = 0; y < hb.arrLength.Length; y++)
                    {
                        if (isTwo)
                        {
                            if (hb.arrLength[y] > 0)
                            {
                                testBmp[(int)hb.ixy, y] = 255;
                            }
                        }
                        else
                        {
                            int ic = 0;
                            int inum = 0;
                            if (smode == "")
                            {
                                inum = hb.arrLength[y];
                            }
                            else if (smode == "long")
                            {
                                inum = hb.arrLengthLong[y];
                            }
                            else
                            {
                                inum = hb.arrLength255[y];
                            }
                            if (inum > 0)
                            {
                                int index = 0;
                                while (inum > 0)
                                {
                                    if (index % 2 == 1)
                                    {
                                        ic = -1;
                                    }
                                    else
                                    {
                                        ic = 1;
                                    }
                                    ic *= (int)Math.Ceiling((float)index / 2f);

                                    if (inum > 255)
                                    {
                                        testBmp[(int)hb.ixy + ic, y] = 255;
                                    }
                                    else
                                    {
                                        testBmp[(int)hb.ixy + ic, y] = inum;
                                    }

                                    inum -= 255;
                                    index++;
                                }
                            }
                        }
                    }
                }
            }
        }
        return testBmp;
    }
    /// <summary>
    ///  ��ȡ����HV�������� ������ ָ�����ȵ�ֱ�� H
    /// </summary>
    /// <param name="outBmp"></param>
    /// <param name="file"></param>
    /// <param name="ib">ֱ�����¸����ľ��뷶Χ Ӱ�� iB</param>
    /// <param name="ie">ֱ�����¸����ľ��뷶Χ Ӱ�� iE</param>
    /// <param name="ilength">ȷ������Ҫ���ֱ�߳��� Ϊ -1 ʱȫ��ȡ�������㳤�ȷ�������Ҫ��</param>
    /// <param name="inum">��ȡ��ͨ�����ǵ���Ч������� ��С�ܵ���</param>
    /// <param name="iJu">�����Զ��ֱ����ϲ� 5  25 ��</param>
    /// <returns></returns>
    public static List<HoughHbClass> getAL_hb_H(int[,] outBmp, FileInfo file, int ib, int ie, int ilength, int inum, int iJu)
    {
        List<Rect> al_rectH = cutAreaLineH(outBmp, file, inum, null, false);
        return getAL_hb_H(outBmp, file, ib, ie, ilength, al_rectH, iJu);
    }
    /// <summary>
    /// ��ȡ����HV�������� ������ ָ�����ȵ�ֱ�� H
    /// </summary>
    /// <param name="outBmp"></param>
    /// <param name="file"></param>
    /// <param name="ib">ֱ�����¸����ľ��뷶Χ Ӱ�� iB</param>
    /// <param name="ie">ֱ�����¸����ľ��뷶Χ Ӱ�� iE</param>
    /// <param name="ilength">ȷ������Ҫ���ֱ�߳��� Ϊ -1 ʱȫ��ȡ�������㳤�ȷ�������Ҫ��</param>
    /// <param name="al_rectH">��ȡ��ͨ�����ǵ���Ч�������</param>
    /// <param name="iJu">�����Զ��ֱ����ϲ� 5  25 ��</param>
    /// <returns></returns>
    public static List<HoughHbClass> getAL_hb_H(int[,] outBmp, FileInfo file, int ib, int ie, int ilength, List<Rect> al_rectH, int iJu)
    {
        int iw = outBmp.GetLength(0);
        int ih = outBmp.GetLength(1);
        List<HoughHbClass> AL_hb_H = null;

        //�ϲ�ֱ��
        AL_hb_H = new List<HoughHbClass>();
        for (int i = 0; i < al_rectH.Count; i++)
        {
            Rect rect = (Rect)al_rectH[i];
            rect.calculateWH();
            uint y = rect.t;
            bool isin = false;
            for (int j = 0; j < AL_hb_H.Count; j++)
            {
                HoughHbClass hb = (HoughHbClass)AL_hb_H[j];
                int icc = (int)Math.Abs((int)hb.ixy - (int)y);
                if (icc <= iJu)
                {
                    hb.add(y, rect, "H");
                    isin = true;
                    break;
                }
            }
            if (!isin)
            {
                HoughHbClass hb = new HoughHbClass(iw);
                hb.add(y, rect, "H");
                AL_hb_H.Add(hb);
            }
        }
        if (ilength >= 0)
        {
            for (int j = 0; j < AL_hb_H.Count; j++)
            {
                HoughHbClass hb = (HoughHbClass)AL_hb_H[j];
                if (hb.ib > 10 || hb.ie < iw - 10) continue;
                hb.setLength01();
                if (hb.iLength > ilength)
                {
                    hb.isok = true;
                    hb.iB = (int)hb.ixy + ib;
                    if (hb.iB < 0) hb.iB = 0;
                    hb.iE = (int)hb.ixy + ie;
                    if (hb.iE >= ih) hb.iE = (int)ih - 1;
                }
            }
        }
        return AL_hb_H;
    }
    /// <summary>
    ///  ��ȡ����HV�������� ������ ָ�����ȵ�ֱ�� V
    /// </summary>
    /// <param name="outBmp"></param>
    /// <param name="file"></param>
    /// <param name="ib">ֱ�����¸����ľ��뷶Χ Ӱ�� iB</param>
    /// <param name="ie">ֱ�����¸����ľ��뷶Χ Ӱ�� iE</param>
    /// <param name="ilength">ȷ������Ҫ���ֱ�߳��� Ϊ -1 ʱȫ��ȡ�������㳤�ȷ�������Ҫ��</param>
    /// <param name="inum">��ȡ��ͨ�����ǵ���Ч������� ��С�ܵ���</param>
    /// <param name="iJu">�����Զ��ֱ����ϲ� 5  25 ��</param>
    /// <returns></returns>
    public static List<HoughHbClass> getAL_hb_V(int[,] outBmp, FileInfo file, int ib, int ie, int ilength, int inum, int iJu)
    {
        List<Rect> al_rectV = cutAreaLineV(outBmp, file, inum, null, false);
        return getAL_hb_V(outBmp, file, ib, ie, ilength, al_rectV, iJu);
    }
    /// <summary>
    ///  ��ȡ����HV�������� ������ ָ�����ȵ�ֱ�� V
    /// </summary>
    /// <param name="outBmp"></param>
    /// <param name="file"></param>
    /// <param name="ib">ֱ�����¸����ľ��뷶Χ Ӱ�� iB</param>
    /// <param name="ie">ֱ�����¸����ľ��뷶Χ Ӱ�� iE</param>
    /// <param name="ilength">ȷ������Ҫ���ֱ�߳��� Ϊ -1 ʱȫ��ȡ�������㳤�ȷ�������Ҫ��</param>
    /// <param name="al_rectH">��ȡ��ͨ�����ǵ���Ч�������</param>
    /// <param name="iJu">�����Զ��ֱ����ϲ� 5  25 ��</param>
    /// <returns></returns>
    public static List<HoughHbClass> getAL_hb_V(int[,] outBmp, FileInfo file, int ib, int ie, int ilength, List<Rect> al_rectV, int iJu)
    {
        List<HoughHbClass> AL_hb_V = null;
        int iw = outBmp.GetLength(0);
        int ih = outBmp.GetLength(1);
        
        //�ϲ�ֱ��
        AL_hb_V = new List<HoughHbClass>();
        for (int i = 0; i < al_rectV.Count; i++)
        {
            Rect rect = al_rectV[i];
            rect.calculateWH();
            uint x = rect.l;
            bool isin = false;
            for (int j = 0; j < AL_hb_V.Count; j++)
            {
                HoughHbClass hb = (HoughHbClass)AL_hb_V[j];
                int icc = (int)Math.Abs((int)hb.ixy - (int)x);
                if (icc <= iJu)
                {
                    hb.add(x, rect, "V");
                    isin = true;
                    break;
                }
            }

            if (!isin)
            {
                HoughHbClass hb = new HoughHbClass(ih);
                hb.add(x, rect, "V");
                AL_hb_V.Add(hb);

                //AciDebug.Debug("����:" + x + " " + rect.t + " " + rect.height);
            }
        }
        if (ilength >= 0)
        {
            for (int j = 0; j < AL_hb_V.Count; j++)
            {
                HoughHbClass hb = (HoughHbClass)AL_hb_V[j];
                if (hb.ib > 10 || hb.ie < ih - 10) continue;
                hb.setLength01();
                if (hb.iLength > ilength)
                {
                    hb.isok = true;
                    hb.iB = (int)hb.ixy - 8;
                    if (hb.iB < 0) hb.iB = 0;
                    hb.iE = (int)hb.ixy + 9;
                    if (hb.iE >= iw) hb.iE = (int)iw - 1;
                }
            }
        }
        return AL_hb_V;
    }
   
    /// <summary>
    /// ��ͨ�����ȡֱ�� H
    /// </summary>
    /// <param name="intBmp"></param>
    /// <param name="file"></param>
    /// <param name="inum"></param>
    /// <param name="AL_hb_H"></param>
    /// <param name="isMove"></param>
    /// <returns></returns>
    public static List<Rect> cutAreaLineH(int[,] intBmp, FileInfo file, int inum, List<HoughHbClass> AL_hb_H, bool isMove)
    {
        int iw = intBmp.GetLength(0);
        int ih = intBmp.GetLength(1);
        List<Rect> arrRect = new List<Rect>();
        if (AL_hb_H == null)
        {
            AL_hb_H = new List<HoughHbClass>();
            HoughHbClass hb = new HoughHbClass(0);
            AL_hb_H.Add(hb);

            hb.isok = true;
            hb.iB = 0;
            hb.iE = ih;
        }
        for (int iH = 0; iH < AL_hb_H.Count; iH++)
        {
            HoughHbClass hb = (HoughHbClass)AL_hb_H[iH];
            if (hb.isok)
            {
                Rect rect = null;
                int dot = 0;

                int[,] arrRect_P = new int[iw, ih];
                
                for (int y = hb.iB; y < hb.iE; y++)
                {
                    for (int x = 0; x < iw; x++)
                    {
                        if (intBmp[x, y] > 0 && arrRect_P[x, y] == 0)
                        {
                            arrRect_P[x, y] = 1;
                            rect = new Rect();
                            rect.l = (uint)x;
                            rect.t = (uint)y;
                            rect.r = (uint)x + 1;
                            rect.b = (uint)y + 1;
                            rect.Add(new myDot((uint)x, (uint)y));
                            int icc = 0;

                            //ȡ��һ�������8����
                            for (int icell = 1; icell <= iw - x; icell++)
                            {
                                int thisx = x + icell;
                                int thisy = y + icc;
                                //�������߽�
                                if (thisx < iw && thisy < hb.iE)
                                {
                                    dot = intBmp[thisx, thisy];
                                    if (dot == 0)
                                    {
                                        if (isMove)
                                        {
                                            bool isok = false;

                                            if (Math.Abs(thisy - 1 - y) <= 1)
                                            {
                                                if (thisy - 1 >= hb.iB)
                                                {
                                                    if (intBmp[thisx - 1, thisy - 1] > 0 && arrRect_P[thisx - 1, thisy - 1] == 0)
                                                    {
                                                        icc--;
                                                        isok = true;
                                                    }
                                                }
                                            }
                                            else if (Math.Abs(thisy + 1 - y) <= 1)
                                            {
                                                if (thisy + 1 < hb.iE)
                                                {
                                                    if (intBmp[thisx - 1, thisy + 1] > 0 && arrRect_P[thisx - 1, thisy + 1] == 0)
                                                    {
                                                        icc++;
                                                        isok = true;
                                                    }
                                                }
                                            }
                                            if (isok)
                                            {
                                                icell--;
                                                thisx = x + icell;
                                                thisy = y + icc;
                                                dot = intBmp[thisx, thisy];
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    if (dot > 0 && arrRect_P[thisx, thisy] == 0)
                                    {
                                        //�µ���׶
                                        arrRect_P[thisx, thisy] = 1;
                                        myDot dott = new myDot((uint)thisx, (uint)thisy);
                                        dott.iR = (byte)dot;
                                        rect.Add(dott);
                                        //�������þ��δ�С
                                        if (thisx < rect.l)
                                        {
                                            rect.l = (uint)thisx;
                                        }
                                        else if (thisx > rect.r)
                                        {
                                            rect.r = (uint)thisx;
                                        }
                                        rect.t = (uint)thisy;
                                        rect.b = (uint)thisy;
                                    }
                                }
                            }
                            if (rect.iNum >= inum)
                            {
                                arrRect.Add(rect);
                            }
                        }

                    }
                }
            }
        }
        return arrRect;
    }
    /// <summary>
    /// ��ͨ����ָ��㷨 
    /// </summary>
    public static unsafe List<Rect> cutArea(ref int[,] intBmp, FileInfo file, int iw, int ih)
    {
        return cutArea(ref intBmp, file, 0, 0, iw, ih);
    }
    /// <summary>
    /// ��ͨ����ָ��㷨 
    /// </summary>
    public static unsafe List<Rect> cutArea(ref int[,] intBmp, FileInfo file, int ix, int iy, int iw, int ih)
    {
        int w = intBmp.GetLength(0);
        int h = intBmp.GetLength(1);

        List<Rect> arrRect = new List<Rect>();
        Rect rect = null;
        myDot point;
        int[,] arrRect_P = new int[ix + iw, iy + ih];

        //��ͨ׶
        List<myDot> Al_sp = null;

        for (int y = iy; y < iy + ih; y++)
        {
            for (int x = ix; x < ix + iw; x++)
            {
                if (x >= ix && x < ix + iw - 1 && y >= iy && y < iy + ih - 1)
                {
                    int igrey = intBmp[x, y];
                    if (igrey > 0 && arrRect_P[x, y] == 0)
                    {
                        //������׶
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
                            //ȡ��һ�������8����
                            point = (myDot)Al_sp[0];
                            for (int irow = -1; irow <= 1; irow++)
                            {
                                for (int icell = -1; icell <= 1; icell++)
                                {
                                    int thisx = (int)point.x + icell;
                                    int thisy = (int)point.y + irow;
                                    //�������߽�
                                    if (thisx >= ix && thisx < ix + iw - 1 && thisy >= iy && thisy < iy + ih - 1)
                                    {
                                        int grey = intBmp[thisx, thisy];
                                        if (grey > 0 && arrRect_P[thisx, thisy] == 0)
                                        {
                                            //�µ���׶
                                            arrRect_P[thisx, thisy] = 1;
                                            myDot dott = new myDot((uint)thisx, (uint)thisy);

                                            Al_sp.Add(dott);
                                            rect.Add(dott);
                                            //�������þ��δ�С
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
                            //��׶
                            Al_sp.RemoveAt(0);
                        }
                        arrRect.Add(rect);
                    }
                }
            }
        }
        return arrRect;
    }
    /// <summary>
    /// ��ͨ�����ȡֱ�� H
    /// </summary>
    /// <param name="intBmp"></param>
    /// <param name="file"></param>
    /// <param name="inum"></param>
    /// <param name="greyAreaX"></param>
    /// <param name="greyAreaY"></param>
    /// <param name="imaxY">Y��������ȡ��Χ ֻ��һ��</param>
    /// <returns></returns>
    public static List<Rect> cutAreaLineH(int[,] intBmp, FileInfo file, int inum, int greyAreaX, int greyAreaY, int imaxY)
    {
        int iw = intBmp.GetLength(0);
        int ih = intBmp.GetLength(1);
        List<Rect> arrRect = new List<Rect>();
        List<myDot> Al_sp = null;
        myDot point = null;

        Rect rect = null;
        int dot = 0;

        int[,] arrRect_P = new int[iw, ih];
        for (int y = greyAreaY; y < ih - greyAreaY; y++)
        {
            for (int x = greyAreaX; x < iw - greyAreaX; x++)
            {
                if (intBmp[x, y] > 0 && arrRect_P[x, y] == 0)
                {
                    //������׶
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
                        //ȡ��һ�������8����
                        point = (myDot)Al_sp[0];
                        for (int irow = -1; irow <= 1; irow++)
                        {
                            for (int icell = -1; icell <= 1; icell++)
                            {
                                int thisx = (int)point.x + icell;
                                int thisy = (int)point.y + irow;
                                //�������߽�
                                if (thisx > 0 && thisx < iw - 1 && thisy > 0 && thisy < ih - 1 && Math.Abs(thisy - y) <= imaxY)
                                {
                                    int grey = intBmp[thisx, thisy];
                                    if (grey > 0 && arrRect_P[thisx, thisy] == 0)
                                    {
                                        //�µ���׶
                                        arrRect_P[thisx, thisy] = 1;
                                        myDot dott = new myDot((uint)thisx, (uint)thisy);

                                        Al_sp.Add(dott);
                                        rect.Add(dott);
                                        //�������þ��δ�С
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
                        //��׶
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

    /// <summary>
    /// ��ͨ�����ȡֱ�� H
    /// </summary>
    /// <param name="intBmp"></param>
    /// <param name="file"></param>
    /// <param name="inum"></param>
    /// <param name="AL_hb_H"></param>
    /// <param name="isMove"></param>
    /// <returns></returns>
    public static List<Rect> cutAreaLineH(int[,] intBmp, FileInfo file, int inum, int greyAreaX, int greyAreaY, bool isMove)
    {
        int iw = intBmp.GetLength(0);
        int ih = intBmp.GetLength(1);
        List<Rect> arrRect = new List<Rect>();

        Rect rect = null;
        int dot = 0;

        int[,] arrRect_P = new int[iw, ih];
        for (int y = greyAreaY; y < ih - greyAreaY; y++)
        {
            for (int x = greyAreaX; x < iw - greyAreaX; x++)
            {
                if (intBmp[x, y] > 0 && arrRect_P[x, y] == 0)
                {
                    arrRect_P[x, y] = 1;
                    rect = new Rect();
                    rect.l = (uint)x;
                    rect.t = (uint)y;
                    rect.r = (uint)x + 1;
                    rect.b = (uint)y + 1;
                    rect.Add(new myDot((uint)x, (uint)y));
                    int icc = 0;

                    //ȡ��һ�������8����
                    for (int icell = 1; icell <= iw - x; icell++)
                    {
                        int thisx = x + icell;
                        int thisy = y + icc;
                        //�������߽�
                        if (thisx < iw - greyAreaX)
                        {
                            dot = intBmp[thisx, thisy];
                            if (dot == 0)
                            {
                                if (isMove)
                                {
                                    bool isok = false;

                                    if (Math.Abs(thisy - 1 - y) <= 1)
                                    {
                                        if (thisy - 1 >= 0)
                                        {
                                            if (intBmp[thisx - 1, thisy - 1] > 0 && arrRect_P[thisx - 1, thisy - 1] == 0)
                                            {
                                                icc--;
                                                isok = true;
                                            }
                                        }
                                    }
                                    if (!isok && Math.Abs(thisy + 1 - y) <= 1)
                                    {
                                        if (thisy + 1 < ih - greyAreaY)
                                        {
                                            if (intBmp[thisx - 1, thisy + 1] > 0 && arrRect_P[thisx - 1, thisy + 1] == 0)
                                            {
                                                icc++;
                                                isok = true;
                                            }
                                        }
                                    }
                                    if (isok)
                                    {
                                        icell--;
                                        thisx = x + icell;
                                        thisy = y + icc;
                                        dot = intBmp[thisx, thisy];
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                            if (dot > 0 && arrRect_P[thisx, thisy] == 0)
                            {
                                //�µ���׶
                                arrRect_P[thisx, thisy] = 1;
                                myDot dott = new myDot((uint)thisx, (uint)thisy);
                                dott.iR = (byte)dot;
                                rect.Add(dott);
                                //�������þ��δ�С
                                if (thisx < rect.l)
                                {
                                    rect.l = (uint)thisx;
                                }
                                else if (thisx > rect.r)
                                {
                                    rect.r = (uint)thisx;
                                }
                                rect.t = (uint)thisy;
                                rect.b = (uint)thisy;
                            }
                        }
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
    /// <summary>
    /// ��ͨ�����ȡֱ�� V
    /// </summary>
    /// <param name="intBmp"></param>
    /// <param name="file"></param>
    /// <param name="inum"></param>
    /// <param name="AL_hb_V"></param>
    /// <param name="isMove"></param>
    /// <returns></returns>
    public static List<Rect> cutAreaLineV(int[,] intBmp, FileInfo file, int inum, List<HoughHbClass> AL_hb_V, bool isMove)
    {
        int iw = intBmp.GetLength(0);
        int ih = intBmp.GetLength(1);
        List<Rect> arrRect = new List<Rect>();
        if (AL_hb_V == null)
        {
            AL_hb_V = new List<HoughHbClass>();
            HoughHbClass hb = new HoughHbClass(0);
            AL_hb_V.Add(hb);

            hb.isok = true;
            hb.iB = 0;
            hb.iE = iw;
        }
        for (int iV = 0; iV < AL_hb_V.Count; iV++)
        {
            HoughHbClass hb = (HoughHbClass)AL_hb_V[iV];
            if (hb.isok)
            {
                Rect rect = null;
                int dot = 0;

                int[,] arrRect_P = new int[iw, ih];

                for (int x = hb.iB; x < hb.iE; x++)
                {
                    for (int y = 0; y < ih; y++)
                    {
                        if (intBmp[x, y] > 0 && arrRect_P[x, y] == 0)
                        {
                            arrRect_P[x, y] = 1;
                            rect = new Rect();
                            rect.l = (uint)x;
                            rect.t = (uint)y;
                            rect.r = (uint)x + 1;
                            rect.b = (uint)y + 1;
                            rect.Add(new myDot((uint)x, (uint)y));
                            int icc = 0;

                            //ȡ��һ�������8����
                            for (int irow = 1; irow <= ih - y; irow++)
                            {
                                int thisx = x + icc;
                                int thisy = y + irow;
                                //�������߽�
                                if (thisy < ih && thisx < hb.iE)
                                {
                                    dot = intBmp[thisx, thisy];
                                    if (dot == 0)
                                    {
                                        if (isMove)
                                        {
                                            bool isok = false;

                                            if (Math.Abs(thisx - 1 - x) <= 1)
                                            {
                                                if (thisx - 1 >= hb.iB)
                                                {
                                                    if (intBmp[thisx - 1, thisy - 1] > 0 && arrRect_P[thisx - 1, thisy - 1] == 0)
                                                    {
                                                        icc--;
                                                        isok = true;
                                                    }
                                                }
                                            }
                                            if (!isok && Math.Abs(thisx - 1 - x) <= 1)
                                            {
                                                if (thisx + 1 < hb.iE)
                                                {
                                                    if (intBmp[thisx - 1, thisy + 1] > 0 && arrRect_P[thisx - 1, thisy + 1] == 0)
                                                    {
                                                        icc++;
                                                        isok = true;
                                                    }
                                                }
                                            }
                                            if (isok)
                                            {
                                                irow--;
                                                thisx = x + icc;
                                                thisy = y + irow;
                                                dot = intBmp[thisx, thisy];
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    if (dot > 0 && arrRect_P[thisx, thisy] == 0)
                                    {
                                        //�µ���׶
                                        arrRect_P[thisx, thisy] = 1;
                                        myDot dott = new myDot((uint)thisx, (uint)thisy);
                                        dott.iR = (byte)dot;
                                        rect.Add(dott);
                                        //�������þ��δ�С
                                        rect.l = (uint)thisx;
                                        rect.r = (uint)thisx;
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
                            if (rect.iNum >= inum)
                            {
                                arrRect.Add(rect);
                            }

                        }

                    }
                }
            }
        }
        return arrRect;
    }

    /// <summary>
    /// ��ͨ�����ȡֱ�� V
    /// </summary>
    /// <param name="intBmp"></param>
    /// <param name="file"></param>
    /// <param name="inum"></param>
    /// <param name="AL_hb_V"></param>
    /// <param name="isMove"></param>
    /// <returns></returns>
    public static List<Rect> cutAreaLineV(int[,] intBmp, FileInfo file, int inum, int greyAreaX, int greyAreaY, bool isMove)
    {
        int iw = intBmp.GetLength(0);
        int ih = intBmp.GetLength(1);
        List<Rect> arrRect = new List<Rect>();
        
        Rect rect = null;
        int dot = 0;

        int[,] arrRect_P = new int[iw, ih];

        
        for (int x = greyAreaX; x < iw - greyAreaX; x++)
        {
            for (int y = greyAreaY; y < ih - greyAreaY; y++)
            {
                if (intBmp[x, y] > 0 && arrRect_P[x, y] == 0)
                {
                    arrRect_P[x, y] = 1;
                    rect = new Rect();
                    rect.l = (uint)x;
                    rect.t = (uint)y;
                    rect.r = (uint)x + 1;
                    rect.b = (uint)y + 1;
                    rect.Add(new myDot((uint)x, (uint)y));
                    int icc = 0;

                    //ȡ��һ�������8����
                    for (int irow = 1; irow <= ih - y; irow++)
                    {
                        int thisx = x + icc;
                        int thisy = y + irow;
                        //�������߽�
                        if (thisy < ih - greyAreaY)
                        {
                            dot = intBmp[thisx, thisy];
                            if (dot == 0)
                            {
                                if (isMove)
                                {
                                    bool isok = false;

                                    if (Math.Abs(thisx - 1 - x) <= 1)
                                    {
                                        if (thisx - 1 >= 0)
                                        {
                                            if (intBmp[thisx - 1, thisy - 1] > 0 && arrRect_P[thisx - 1, thisy - 1] == 0)
                                            {
                                                icc--;
                                                isok = true;
                                            }
                                        }
                                    }
                                    else if (Math.Abs(thisx - 1 - x) <= 1)
                                    {
                                        if (thisx + 1 < iw - greyAreaX)
                                        {
                                            if (intBmp[thisx - 1, thisy + 1] > 0 && arrRect_P[thisx - 1, thisy + 1] == 0)
                                            {
                                                icc++;
                                                isok = true;
                                            }
                                        }
                                    }
                                    if (isok)
                                    {
                                        irow--;
                                        thisx = x + icc;
                                        thisy = y + irow;
                                        dot = intBmp[thisx, thisy];
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                            if (dot > 0 && arrRect_P[thisx, thisy] == 0)
                            {
                                //�µ���׶
                                arrRect_P[thisx, thisy] = 1;
                                myDot dott = new myDot((uint)thisx, (uint)thisy);
                                dott.iR = (byte)dot;
                                rect.Add(dott);
                                //�������þ��δ�С
                                rect.l = (uint)thisx;
                                rect.r = (uint)thisx;
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
                    if (rect.iNum >= inum)
                    {
                        arrRect.Add(rect);
                    }

                }

            }
        }

        return arrRect;
    }
    /// <summary>
    /// �Ƴ�ֱ�� ���� Ѱ�ҳ�����H V
    /// </summary>
    /// <param name="intBmp"></param>
    /// <param name="file"></param>
    /// <param name="AL_hb_H"></param>
    /// <param name="AL_hb_V"></param>
    /// <param name="it"></param>
    /// <param name="isMove">�Ƿ��Ƴ�</param>
    /// <param name="show">�Ƿ����/param>
    /// <param name="format">����ʱ��jpg ���� png</param>
    public static void moveLine(ref int[,] intBmp, FileInfo file, List<HoughHbClass> AL_hb_H, List<HoughHbClass> AL_hb_V, string it, bool isMove, bool show, string format)
    {
        int iw = intBmp.GetLength(0);
        int ih = intBmp.GetLength(1);
        List<Rect> al_rectH = new List<Rect>();
        List<Rect> al_rectV = new List<Rect>();

        int index = 0;
        int[,] outBmp;
        do
        {
            index++;
            //��ȡ���� HV ��ǿ��
            outBmp = AciBmpInt.getFrameHV(intBmp, 1, AL_hb_H, AL_hb_V);

            if (show)
            {
                AciBmpInt.DrawIntBmpPng(file.FullName.Replace("." + format, "_��ʱ_0" + it + "_" + index + "_HV������ȡ��." + format), outBmp);
            }
            //MessageBox.Show("asd");
            //��ͨ����ȡֱ��
            if (AL_hb_H != null)
            {
                int icutH = 10;
                //if (AL_hb_V == null)
                //{
                //    icutH = 10;
                //}
                //else
                //{
                //    icutH = 30;
                //}
                al_rectH = AciBmpInt.cutAreaLineH(outBmp, file, icutH, AL_hb_H, isMove);
            }

            if (AL_hb_V != null)
            {
                int icutV = 10;
                al_rectV = AciBmpInt.cutAreaLineV(outBmp, file, icutV, AL_hb_V, isMove);
            }

            outBmp = new int[iw, ih];
            if (AL_hb_H != null)
            {
                for (int i = 0; i < al_rectH.Count; i++)
                {
                    Rect rect = al_rectH[i];
                    for (int d = 0; d < rect.Al_dot.Count; d++)
                    {
                        myDot dot = (myDot)rect.Al_dot[d];
                        uint x = (uint)((float)dot.x);
                        uint y = (uint)((float)dot.y);
                        outBmp[x, y] = dot.iR;
                    }
                }
            }
            if (AL_hb_V != null)
            {
                for (int i = 0; i < al_rectV.Count; i++)
                {
                    Rect rect = (Rect)al_rectV[i];
                    for (int d = 0; d < rect.Al_dot.Count; d++)
                    {
                        myDot dot = (myDot)rect.Al_dot[d];
                        uint x = (uint)((float)dot.x);
                        uint y = (uint)((float)dot.y);
                        outBmp[x, y] = dot.iR;
                    }
                }
            }
            if (show)
            {
                AciBmpInt.DrawIntBmpPng(file.FullName.Replace("." + format, "_��ʱ_0" + it + "_" + index + "_HV�γ�ֱ��." + format), outBmp);
            }


            //���ֱ��H
            if (AL_hb_H != null)
            {
                for (int y = 0; y < AL_hb_H.Count; y++)
                {
                    HoughHbClass hb = (HoughHbClass)AL_hb_H[y];
                    if (hb.isok)
                    {
                        for (int j = hb.iB; j < hb.iE; j++)
                        {
                            for (int i = 1; i < iw - 1; i++)
                            {
                                //intBmp[i, j] -= outBmp[i, j];
                                if (outBmp[i, j] > 0)
                                {
                                    intBmp[i, j] = 0;
                                }
                            }
                        }
                    }
                }
            }
            if (AL_hb_V != null)
            {
                //���ֱ��V
                for (int x = 0; x < AL_hb_V.Count; x++)
                {
                    HoughHbClass hb = (HoughHbClass)AL_hb_V[x];
                    if (hb.isok)
                    {
                        for (int j = 1; j < ih - 1; j++)
                        {
                            for (int i = hb.iB; i < hb.iE; i++)
                            {
                                //intBmp[i, j] -= outBmp[i, j];
                                if (outBmp[i, j] > 0)
                                {
                                    intBmp[i, j] = 0;
                                }
                            }
                        }
                    }
                }
            }

            if (show)
            {
                AciBmpInt.DrawIntBmpPng(file.FullName.Replace("." + format, "_��ʱ_0" + it + "_" + index + "_��ȥ��." + format), intBmp);
            }
        } while ((al_rectH.Count != 0 || al_rectV.Count != 0) && index <= 3);
    }

    public static bool isBoxMatch55(int[,] box1, int[,] box2)
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (box1[i, j] != box2[i, j])
                {
                    return false;
                }
            }
        }
        return true;
    }
    /// <summary>
    /// intBmp ר�ù�����
    /// </summary>
    /// <param name="outBmp"></param>
    /// <param name="savePath">����ʱ���·��</param>
    public static void toSmallLine(ref int[,] outBmp, string savePath, bool show)
    {
        int iw = outBmp.GetLength(0);
        int ih = outBmp.GetLength(1);
        int it = 0;

        if (show) DrawIntBmpPng_Blank(savePath + "_" + it + "--1.png", outBmp);

        int[,] intBox;
        //�ȴ���ͻ���� 3*3 ͻ����λ������ʴ
        int ixC = iw - 5 + 1;
        int iyC = ih - 5 + 1;
        List<Point> AL_pointMove = new List<Point>();
        for (int y = 2; y <= iyC; y++)
        {
            for (int x = 2; x <= ixC; x++)
            {
                if (outBmp[x, y] > 0)
                {
                    //5*5����
                    intBox = new int[5, 5];
                    for (int j = -2; j <= 2; j++)
                    {
                        for (int i = -2; i <= 2; i++)
                        {
                            if (outBmp[x + i, y + j] > 0)
                            {
                                intBox[i + 2, j + 2] = 1;
                            }
                        }
                    }
                    int[,] box2_t = new int[,] {
                          { 1, 1, 1, 1, 1 }
                        , { 1, 1, 1, 1, 1 }
                        , { 0, 1, 1, 1, 0 }
                        , { 0, 1, 1, 1, 0 }
                        , { 0, 0, 0, 0, 0 } };
                    if (isBoxMatch55(intBox, box2_t))
                    {
                        AL_pointMove.Add(new Point(x - 1, y));
                        AL_pointMove.Add(new Point(x - 1, y + 1));
                        AL_pointMove.Add(new Point(x + 1, y));
                        AL_pointMove.Add(new Point(x + 1, y + 1));
                    }

                    int[,] box2_b = new int[,] {
                          { 0, 0, 0, 0, 0 }
                        , { 0, 1, 1, 1, 0 }
                        , { 0, 1, 1, 1, 0 }
                        , { 1, 1, 1, 1, 1 }
                        , { 1, 1, 1, 1, 1 } };
                    if (isBoxMatch55(intBox, box2_t))
                    {
                        AL_pointMove.Add(new Point(x - 1, y - 1));
                        AL_pointMove.Add(new Point(x + 1, y));
                        AL_pointMove.Add(new Point(x + 1, y - 1));
                        AL_pointMove.Add(new Point(x + 1, y));
                    }

                    int[,] box2_l = new int[,] {
                          { 1, 1, 0, 0, 0 }
                        , { 1, 1, 1, 1, 0 }
                        , { 1, 1, 1, 1, 0 }
                        , { 1, 1, 1, 1, 0 }
                        , { 1, 1, 0, 0, 0 } };
                    if (isBoxMatch55(intBox, box2_t))
                    {
                        AL_pointMove.Add(new Point(x, y - 1));
                        AL_pointMove.Add(new Point(x + 1, y - 1));
                        AL_pointMove.Add(new Point(x, y + 1));
                        AL_pointMove.Add(new Point(x + 1, y + 1));
                    }

                    int[,] box2_r = new int[,] {
                          { 0, 0, 0, 1, 1 }
                        , { 0, 1, 1, 1, 1 }
                        , { 0, 1, 1, 1, 1 }
                        , { 0, 1, 1, 1, 1 }
                        , { 0, 0, 0, 1, 1 } };
                    if (isBoxMatch55(intBox, box2_t))
                    {
                        AL_pointMove.Add(new Point(x - 1, y - 1));
                        AL_pointMove.Add(new Point(x, y -1));
                        AL_pointMove.Add(new Point(x - 1, y + 1));
                        AL_pointMove.Add(new Point(x, y + 1));
                    }

                    if (intBox[1, 1] == 1 && intBox[2, 1] == 1 && intBox[3, 1] == 1
                        && intBox[1, 2] == 1 && intBox[2, 2] == 1 && intBox[3, 2] == 1
                        && intBox[1, 3] == 1 && intBox[2, 3] == 1 && intBox[3, 3] == 1)
                    {
                        if (intBox[1, 0] == 1 && intBox[2, 0] == 1 && intBox[3, 0] == 1
                            && intBox[0, 1] == 0 && intBox[0, 2] == 0 && intBox[0, 3] == 0 && intBox[0, 4] == 0
                            && intBox[4, 1] == 0 && intBox[4, 2] == 0 && intBox[4, 3] == 0 && intBox[4, 4] == 0)
                        {
                            //��ͻ��
                            AL_pointMove.Add(new Point(x - 1, y - 1));
                            AL_pointMove.Add(new Point(x - 1, y));
                            AL_pointMove.Add(new Point(x - 1, y + 1));
                            AL_pointMove.Add(new Point(x + 1, y - 1));
                            AL_pointMove.Add(new Point(x + 1, y));
                            AL_pointMove.Add(new Point(x + 1, y + 1));
                        }
                        else if (intBox[1, 4] == 1 && intBox[2, 4] == 1 && intBox[3, 4] == 1
                            && intBox[0, 0] == 0 && intBox[0, 1] == 0 && intBox[0, 2] == 0 && intBox[0, 3] == 0
                             && intBox[4, 0] == 0 && intBox[4, 1] == 0 && intBox[4, 2] == 0 && intBox[4, 3] == 0)
                        {
                            //��ͻ��
                            AL_pointMove.Add(new Point(x - 1, y - 1));
                            AL_pointMove.Add(new Point(x - 1, y));
                            AL_pointMove.Add(new Point(x - 1, y + 1));
                            AL_pointMove.Add(new Point(x + 1, y - 1));
                            AL_pointMove.Add(new Point(x + 1, y));
                            AL_pointMove.Add(new Point(x + 1, y + 1));
                        }
                        else if (intBox[4, 1] == 1 && intBox[4, 2] == 1 && intBox[4, 3] == 1
                           && intBox[0, 0] == 0 && intBox[1, 0] == 0 && intBox[2, 0] == 0 && intBox[3, 0] == 0
                           && intBox[0, 4] == 0 && intBox[1, 4] == 0 && intBox[2, 4] == 0 && intBox[3, 4] == 0)
                        {
                            //��ͻ��
                            AL_pointMove.Add(new Point(x - 1, y - 1));
                            AL_pointMove.Add(new Point(x, y - 1));
                            AL_pointMove.Add(new Point(x + 1, y - 1));
                            AL_pointMove.Add(new Point(x - 1, y + 1));
                            AL_pointMove.Add(new Point(x, y + 1));
                            AL_pointMove.Add(new Point(x + 1, y + 1));
                        }
                        else if (intBox[0, 1] == 1 && intBox[0, 2] == 1 && intBox[0, 3] == 1
                          && intBox[1, 0] == 0 && intBox[2, 0] == 0 && intBox[3, 0] == 0 && intBox[4, 0] == 0
                          && intBox[1, 4] == 0 && intBox[2, 4] == 0 && intBox[3, 4] == 0 && intBox[4, 4] == 0)
                        {
                            //��ͻ��
                            AL_pointMove.Add(new Point(x - 1, y - 1));
                            AL_pointMove.Add(new Point(x, y - 1));
                            AL_pointMove.Add(new Point(x + 1, y - 1));
                            AL_pointMove.Add(new Point(x - 1, y + 1));
                            AL_pointMove.Add(new Point(x, y + 1));
                            AL_pointMove.Add(new Point(x + 1, y + 1));
                        }
                    }
                    
                }
            }
        }
        for (int i = 0; i < AL_pointMove.Count; i++)
        {
            outBmp[AL_pointMove[i].X, AL_pointMove[i].Y] = 0;
        }

        if (show) DrawIntBmpPng_Blank(savePath + "_" + it + "-0.png", outBmp);

        List<pointMove> AL_move = new List<pointMove>();
        do
        {
            it++;
            List<Point> AL_point = getFrameP(outBmp, 1);

            if (show) DrawIntBmpPng_Blank(savePath + "_" + it + "-1.png", getFrame(outBmp, 1));

            AL_move = new List<pointMove>();
            for (int i = 0; i < AL_point.Count; i++)
            {
                Point point = AL_point[i];
                //outBmp[point.X, point.Y] = 0;
                int[,] arrN = toN_N(outBmp, point.X, point.Y, iw, ih);

                bool ok1 = false;
                int NZ = arrN[1, 1];//�洢��Χ8�����ɫ����

                if (3 <= NZ && NZ <= 5)
                {
                    ok1 = true;
                }
                //2.  Z0(P)=1
                bool ok2 = false;
                //����Z0(P)

                int Z0P1 = getZO(arrN, 1, 1);

                if (Z0P1 == 1)
                {
                    ok2 = true;
                }
                bool ok3 = true;
                if (NZ == 2 && Z0P1 == 1)
                {
                    ok3 = false;
                }
                if (point.X == 5 && point.Y == 13)
                {
                    string s = "";
                }
                bool ok4 = true;
                if (Z0P1 == 1 && NZ == 5)
                {
                    if (arrN[2, 0] == 1 && arrN[2, 1] == 1 && arrN[2, 2] == 1)
                    {
                        int iNZ_rb = toN_N_R(arrN, outBmp, point.X, point.Y, iw, ih);
                        if (iNZ_rb == 0)
                        {
                            ok4 = false;
                        }
                    }
                    if (arrN[0, 2] == 1 && arrN[1, 2] == 1 && arrN[2, 2] == 1)
                    {
                        int iNZ_rb = toN_N_B(arrN, outBmp, point.X, point.Y, iw, ih);
                        if (iNZ_rb == 0)
                        {
                            ok4 = false;
                        }
                    }
                }
                if (ok1 && ok2 && ok3 && ok4)
                {
                    AL_move.Add(new pointMove(point, outBmp[point.X, point.Y]));
                }
            }
            for (int i = 0; i < AL_move.Count; i++)
            {
                outBmp[AL_move[i].point.X, AL_move[i].point.Y] = 0;
            }
            if (show) DrawIntBmpPng_Blank(savePath + "_" + it + "-2.png", outBmp);
        } while (AL_move.Count > 0);
    }
    public class pointMove
    {
        public Point point = new Point();
        public int ivalue = 0;
        public pointMove(Point p, int iv)
        {
            point = p;
            ivalue = iv;
        }
    }

    public static int toN_N_R(int[,] arrN, int[,] intBmp, int x, int y, int iw, int ih)
    {
        int N = 1;
        int icount = 0;
        for (int irow = -N; irow <= N; irow++)
        {
            for (int icell = -N; icell <= N; icell++)
            {
                if (arrN[icell + N, irow + N] == 1)
                {
                    if (icell == 1)
                    {
                        int thisx = (x + icell + 1);
                        int thisy = (y + irow);
                        //�������߽�
                        if (thisx >= 0 && thisx < iw && thisy >= 0 && thisy < ih)
                        {
                            if (intBmp[thisx, thisy] > 0)
                            {
                                icount++;
                            }
                        }
                    }

                }
            }
        }
        return icount;
    }
    public static int toN_N_B(int[,] arrN, int[,] intBmp, int x, int y, int iw, int ih)
    {
        int N = 1;
        int icount = 0;
        for (int irow = -N; irow <= N; irow++)
        {
            for (int icell = -N; icell <= N; icell++)
            {
                if (arrN[icell + N, irow + N] == 1)
                {
                    if (irow == 1)
                    {
                        int thisx = (x + icell);
                        int thisy = (y + irow + 1);
                        //�������߽�
                        if (thisx >= 0 && thisx < iw && thisy >= 0 && thisy < ih)
                        {
                            if (intBmp[thisx, thisy] > 0)
                            {
                                icount++;
                            }
                        }
                    }

                }
            }
        }
        return icount;
    }
    /// <summary>
    /// ����ĳһ��Ϊ���ĵ� N*N �ķ���  ��������߽� Ϊ 0, ��ɫ Ϊ 1
    /// </summary>
    /// <param name="p"></param>
    /// <param name="x">���ĵ�</param>
    /// <param name="y">���ĵ�</param>
    /// <param name="n">�����n  ��Ҫ����</param>
    /// <returns>n*n��С������ �������£���������</returns>
    public static int[,] toN_N(int[,] intBmp, int x, int y, int iw, int ih)
    {
        int N = 1;
        int[,] arrN = new int[3, 3];
        int icount = 0;
        for (int irow = -N; irow <= N; irow++)
        {
            for (int icell = -N; icell <= N; icell++)
            {
                int thisx = (x + icell);
                int thisy = (y + irow);
                //�������߽�
                if (thisx >= 0 && thisx < iw && thisy >= 0 && thisy < ih)
                {
                    if (intBmp[thisx, thisy] > 0)
                    {
                        arrN[icell + N, irow + N] = 1;
                        icount++;
                    }
                    else
                    {
                        arrN[icell + N, irow + N] = 0;
                    }
                }
                else
                {
                    arrN[icell + N, irow + N] = 0;
                }
            }
        }
        arrN[1, 1] = icount - 1;
        return arrN;
    }
    /// <summary>
    /// ȷ��һȦ�˸����Ƿ���ж��ȱ��
    /// </summary>
    /// <param name="arrN"></param>
    /// <param name="it"></param>
    /// <param name="ik"></param>
    /// <returns></returns>
    public static int getZO(int[,] arrN, int it, int ik)
    {
        int nCount = 0;
        if (arrN[it - 1, ik - 1] == 0 && arrN[it + 0, ik - 1] == 1) nCount++;
        if (arrN[it + 0, ik - 1] == 0 && arrN[it + 1, ik - 1] == 1) nCount++;
        if (arrN[it + 1, ik - 1] == 0 && arrN[it + 1, ik + 0] == 1) nCount++;
        if (arrN[it + 1, ik + 0] == 0 && arrN[it + 1, ik + 1] == 1) nCount++;
        if (arrN[it + 1, ik + 1] == 0 && arrN[it + 0, ik + 1] == 1) nCount++;
        if (arrN[it + 0, ik + 1] == 0 && arrN[it - 1, ik + 1] == 1) nCount++;
        if (arrN[it - 1, ik + 1] == 0 && arrN[it - 1, ik + 0] == 1) nCount++;
        if (arrN[it - 1, ik + 0] == 0 && arrN[it - 1, ik - 1] == 1) nCount++;
        return nCount;
    }
    /// <summary>
    /// intBmp ��ר�÷Ŵ����
    /// </summary>
    /// <param name="intBmp">ԭʼ intBmp</param>
    /// <param name="tow">Ŀ�� intBmp �Ŀ�</param>
    /// <param name="toh">Ŀ�� intBmp �ĸ�</param>
    /// <param name="rectDes">Ŀ�������</param>
    /// <param name="rectSrc">ԭʼ������</param>
    /// <param name="isTwo">�Ƿ�ʹ�ö�ֵ������ʱ���ã�</param>
    /// <returns></returns>
    public static int[,] intBmpScaleSmall(int[,] intBmp, int tow, int toh, Rectangle rectDes, Rectangle rectSrc, bool isTwo)
    {
        int itoX = rectDes.X;
        int itoY = rectDes.Y;
        int itoW = rectDes.Width;
        int itoH = rectDes.Height;
        float ipp = 0;
        if (rectSrc.Width > rectSrc.Height)
        {
            ipp = (float)itoW / (float)rectSrc.Width;
        }
        else
        {
            ipp = (float)itoH / (float)rectSrc.Height;
        }

        int[,] outBmp = new int[tow, toh];
        for (int y = rectSrc.Y; y < rectSrc.Y + rectSrc.Height; y++)
        {
            for (int x = rectSrc.X; x < rectSrc.X + rectSrc.Width; x++)
            {
                if (intBmp[x, y] > 0)
                {
                    int ix = (int)Math.Round((float)(x - rectSrc.X) * ipp) + itoX;
                    int iy = (int)Math.Round((float)(y - rectSrc.Y) * ipp) + itoY;

                    if (ix >= tow) ix = (int)tow - 1;
                    if (iy >= toh) iy = (int)toh - 1;
                    if (ix < 0) ix = 0;
                    if (iy < 0) iy = 0;

                    if (isTwo)
                    {
                        outBmp[ix, iy] = 255;
                    }
                    else
                    {
                        outBmp[ix, iy] = intBmp[x, y];
                    }
                }
            }
        }
        return outBmp;
    }
    //intBmpScaleBig(intBmp, (int)w, (int)h, new Rectangle(itoX, itoY, itoW, itoH), new Rectangle(rect.X, rect.Y, rect.Width, rect.Height));
    /// <summary>
    /// intBmp ��ר�÷Ŵ����
    /// </summary>
    /// <param name="intBmp">ԭʼ intBmp</param>
    /// <param name="tow">Ŀ�� intBmp �Ŀ�</param>
    /// <param name="toh">Ŀ�� intBmp �ĸ�</param>
    /// <param name="rectDes">Ŀ�������</param>
    /// <param name="rectSrc">ԭʼ������</param>
    /// <param name="isTwo">�Ƿ�ʹ�ö�ֵ��</param>
    /// <returns></returns>
    public static int[,] intBmpScaleBig(int[,] intBmp, int tow, int toh, Rectangle rectDes, Rectangle rectSrc, bool isTwo)
    {
        //DrawIntBmpPng_Blank(dir.FullName + "/" + file.Name + "1.png", intBmp);

        int itoX = rectDes.X;
        int itoY = rectDes.Y;
        int itoW = rectDes.Width;
        int itoH = rectDes.Height;

        float ipp = 0;
        if (rectSrc.Width > rectSrc.Height)
        {
            ipp = (float)itoW / (float)rectSrc.Width;
        }
        else
        {
            ipp = (float)itoH / (float)rectSrc.Height;
        }

        //�Ŵ�
        int[,] outBmp = new int[tow, toh];
        ipp = (float)Math.Round(ipp, 3);
        float iyu = (float)Math.Round(ipp * 1000f % 1000, 3);
        float pYu = iyu / 1000f;
        int ibei = (int)Math.Floor(ipp - pYu) / 2 + 1;
        for (int y = rectSrc.Y; y < rectSrc.Y + rectSrc.Height; y++)
        {
            for (int x = rectSrc.X; x < rectSrc.X + rectSrc.Width; x++)
            {
                if (intBmp[x, y] > 0)
                {
                    for (int nx = -ibei; nx <= ibei; nx++)
                    {
                        for (int ny = -ibei; ny <= ibei; ny++)
                        {
                            int ix = (int)Math.Round((float)(x - rectSrc.X) * ipp) + nx + itoX;
                            int iy = (int)Math.Round((float)(y - rectSrc.Y) * ipp) + ny + itoY;

                            if (ix >= tow) ix = (int)tow - 1;
                            if (iy >= toh) iy = (int)toh - 1;
                            if (ix < 0) ix = 0;
                            if (iy < 0) iy = 0;

                            if (nx == 0 && ny == 0)
                            {
                                outBmp[ix, iy] = intBmp[x, y];
                            }
                            else if (nx == 0 || ny == 0)
                            {
                                if (outBmp[ix, iy] <= 0) outBmp[ix, iy] = -1;
                            }
                            else
                            {
                                if (outBmp[ix, iy] == 0) outBmp[ix, iy] = -2;
                            }
                        }
                    }
                }
            }
        }

        bool isok1 = false;
        bool isok2 = false;

        //for (int y = 0; y < toh; y++)
        //{
        //    for (int x = 0; x < tow; x++)
        //    {
        //        if (outBmp[x, y] == -2)
        //        {
        //            outBmp[x, y] = 66;
        //            continue;
        //        }
        //    }
        //}

        for (int y = 0; y < toh; y++)
        {
            for (int x = 0; x < tow; x++)
            {
                if (outBmp[x, y] == -1)
                {
                    //outBmp[x, y] = 128;
                    //continue;
                    int ib = 0;
                    int ie = 0;
                    int nx1 = 1;
                    isok1 = false;
                    for (int i = 1; i <= ibei * 2; i++)
                    {
                        if (x - i >= 0)
                        {
                            if (outBmp[x - i, y] > 0)
                            {
                                nx1 = i;
                                isok1 = true;
                                break;
                            }
                        }
                    }
                    if (x - nx1 >= 0) ib = outBmp[x - nx1, y];
                    if (ib < 0) ib = 0;

                    int nx2 = 1;
                    isok2 = false;
                    for (int i = 1; i <= ibei * 2; i++)
                    {
                        if (x + i < tow)
                        {
                            if (outBmp[x + i, y] > 0)
                            {
                                nx2 = i;
                                isok2 = true;
                                break;
                            }
                        }
                    }
                    if (x + nx2 < tow) ie = outBmp[x + nx2, y];
                    if (ie < 0) ie = 0;

                    if (isTwo)
                    {
                        if (isok2 && isok1)
                        {
                            outBmp[x, y] = 255;
                        }
                    }
                    else
                    {
                        if (!isok2 && isok1)
                        {
                            outBmp[x, y] = -3;
                        }
                        else if ((isok1 && isok2) || (isok2 && !isok1))
                        {
                            int ito = (ib + ie) / 2;
                            if (ito > 0)
                            {
                                outBmp[x, y] = ito;
                            }
                        }
                    }
                    if (outBmp[x, y] > 255) outBmp[x, y] = 255;
                }
            }
        }
        if (!isTwo)
        {
            for (int y = 0; y < toh; y++)
            {
                for (int x = (int)tow - 1; x >= 0; x--)
                {
                    if (outBmp[x, y] == -3)
                    {
                        int ib = 0;
                        int ie = 0;
                        int nx1 = 1;
                        for (int i = 1; i <= ibei * 2; i++)
                        {
                            if (x - i >= 0)
                            {
                                if (outBmp[x - i, y] > 0)
                                {
                                    nx1 = i;
                                    break;
                                }
                            }
                        }
                        if (x - nx1 >= 0) ib = outBmp[x - nx1, y];
                        if (ib < 0) ib = 0;

                        int nx2 = 1;
                        for (int i = 1; i <= ibei * 2; i++)
                        {
                            if (x + i < tow)
                            {
                                if (outBmp[x + i, y] > 0)
                                {
                                    nx2 = i;
                                    break;
                                }
                            }
                        }
                        if (x + nx2 < tow) ie = outBmp[x + nx2, y];
                        if (ie < 0) ie = 0;
                        int ito = (ib + ie) / 2;
                        outBmp[x, y] = ito;
                        if (outBmp[x, y] > 255) outBmp[x, y] = 255;
                    }
                }
            }
        }

        for (int y = 0; y < toh; y++)
        {
            for (int x = 0; x < tow; x++)
            {
                if (outBmp[x, y] == -1)
                {
                    int ib = 0;
                    int ie = 0;
                    int ny = 1;
                    isok1 = false;
                    for (int i = 1; i <= ibei * 2; i++)
                    {
                        if (y - i >= 0)
                        {
                            if (outBmp[x, y - i] > 0)
                            {
                                ny = i;
                                isok1 = true;
                                break;
                            }
                        }
                    }
                    if (y - ny >= 0) ib = outBmp[x, y - ny];
                    if (ib < 0) ib = 0;
                    ny = 1;
                    isok2 = false;
                    for (int i = 1; i <= ibei * 2; i++)
                    {
                        if (y + i < toh)
                        {
                            if (outBmp[x, y + i] > 0)
                            {
                                ny = i;
                                isok2 = true;
                                break;
                            }
                        }
                    }
                    if (y + ny < toh) ie = outBmp[x, y + ny];
                    if (ie < 0) ie = 0;
                    if (isTwo)
                    {
                        if (isok2 && isok1)
                        {
                            outBmp[x, y] = 255;
                        }
                    }
                    else
                    {
                        if (!isok2 && isok1)
                        {
                            outBmp[x, y] = -3;
                        }
                        else if ((isok1 && isok2) || (isok2 && !isok1))
                        {
                            int ito = (ib + ie) / 2;
                            if (ito > 0)
                            {
                                outBmp[x, y] = ito;
                            }
                        }
                    }
                    if (outBmp[x, y] > 255) outBmp[x, y] = 255;
                }
            }
        }
        if (!isTwo)
        {
            for (int y = (int)toh - 1; y >= 0; y--)
            {
                for (int x = 0; x < tow; x++)
                {
                    if (outBmp[x, y] == -3)
                    {
                        int ib = 0;
                        int ie = 0;
                        int ny = 1;
                        isok1 = false;
                        for (int i = 1; i <= ibei * 2; i++)
                        {
                            if (y - i >= 0)
                            {
                                if (outBmp[x, y - i] > 0)
                                {
                                    ny = i;
                                    isok1 = true;
                                    break;
                                }
                            }
                        }
                        if (y - ny >= 0) ib = outBmp[x, y - ny];
                        if (ib < 0) ib = 0;
                        ny = 1;
                        isok2 = false;
                        for (int i = 1; i <= ibei * 2; i++)
                        {
                            if (y + i < toh)
                            {
                                if (outBmp[x, y + i] > 0)
                                {
                                    ny = i;
                                    isok2 = true;
                                    break;
                                }
                            }
                        }
                        if (y + ny < toh) ie = outBmp[x, y + ny];
                        if (ie < 0) ie = 0;
                        int ito = (ib + ie) / 2;
                        outBmp[x, y] = ito;
                        if (outBmp[x, y] > 255) outBmp[x, y] = 255;
                    }
                }
            }
        }

        for (int y = 0; y < toh; y++)
        {
            for (int x = 0; x < tow; x++)
            {
                if (outBmp[x, y] == -2)
                {
                    int ib = 0;
                    int ie = 0;
                    int nx1 = 1;
                    isok1 = false;
                    for (int i = 1; i <= ibei * 2; i++)
                    {
                        if (x - i >= 0)
                        {
                            if (outBmp[x - i, y] > 0)
                            {
                                nx1 = i;
                                isok1 = true;
                                break;
                            }
                        }
                    }
                    if (x - nx1 >= 0) ib = outBmp[x - nx1, y];
                    if (ib < 0) ib = 0;

                    int nx2 = 1;
                    isok2 = false;
                    for (int i = 1; i <= ibei * 2; i++)
                    {
                        if (x + i < tow)
                        {
                            if (outBmp[x + i, y] > 0)
                            {
                                nx2 = i;
                                isok2 = true;
                                break;
                            }
                        }
                    }
                    if (x + nx2 < tow) ie = outBmp[x + nx2, y];
                    if (ie < 0) ie = 0;

                    int ivX = -1;
                    if (isTwo)
                    {
                        if (isok2 && isok1)
                        {
                            outBmp[x, y] = 255;
                        }
                    }
                    else
                    {
                        if (!isok2 && isok1)
                        {
                            ivX = -3;
                        }
                        else if ((isok1 && isok2) || (isok2 && !isok1))
                        {
                            int ito = (ib + ie) / 2;
                            if (ito > 0)
                            {
                                ivX = ito;
                            }
                        }
                    }

                    ib = 0;
                    ie = 0;
                    int ny = 1;
                    isok1 = false;
                    for (int i = 1; i <= ibei * 2; i++)
                    {
                        if (y - i >= 0)
                        {
                            if (outBmp[x, y - i] > 0)
                            {
                                ny = i;
                                isok1 = true;
                                break;
                            }
                        }
                    }
                    if (y - ny >= 0) ib = outBmp[x, y - ny];
                    if (ib < 0) ib = 0;
                    ny = 1;
                    isok2 = false;
                    for (int i = 1; i <= ibei * 2; i++)
                    {
                        if (y + i < toh)
                        {
                            if (outBmp[x, y + i] > 0)
                            {
                                ny = i;
                                isok2 = true;
                                break;
                            }
                        }
                    }
                    if (y + ny < toh) ie = outBmp[x, y + ny];
                    if (ie < 0) ie = 0;

                    int ivY = -1;
                    if (isTwo)
                    {
                        if (isok2 && isok1)
                        {
                            outBmp[x, y] = 255;
                        }
                    }
                    else
                    {
                        if (!isok2 && isok1)
                        {
                            ivY = -3;
                        }
                        else if ((isok1 && isok2) || (isok2 && !isok1))
                        {
                            int ito = (ib + ie) / 2;
                            if (ito > 0)
                            {
                                ivY = ito;
                            }
                        }
                        if (ivX > ivY)
                        {
                            outBmp[x, y] = ivX;
                        }
                        else
                        {
                            outBmp[x, y] = ivY;
                        }
                        if (outBmp[x, y] > 255) outBmp[x, y] = 255;
                    }

                }
            }
        }
        if (!isTwo)
        {
            for (int y = (int)toh - 1; y >= 0; y--)
            {
                for (int x = 0; x < tow; x++)
                {
                    if (outBmp[x, y] == -3)
                    {
                        int ib = 0;
                        int ie = 0;
                        int nx1 = 1;
                        for (int i = 1; i <= ibei * 2; i++)
                        {
                            if (x - i >= 0)
                            {
                                if (outBmp[x - i, y] > 0)
                                {
                                    nx1 = i;
                                    break;
                                }
                            }
                        }
                        if (x - nx1 >= 0) ib = outBmp[x - nx1, y];
                        if (ib < 0) ib = 0;

                        int nx2 = 1;
                        for (int i = 1; i <= ibei * 2; i++)
                        {
                            if (x + i < tow)
                            {
                                if (outBmp[x + i, y] > 0)
                                {
                                    nx2 = i;
                                    break;
                                }
                            }
                        }
                        if (x + nx2 < tow) ie = outBmp[x + nx2, y];
                        if (ie < 0) ie = 0;
                        int ivX = (ib + ie) / 2;

                        ib = 0;
                        ie = 0;
                        int ny = 1;
                        isok1 = false;
                        for (int i = 1; i <= ibei * 2; i++)
                        {
                            if (y - i >= 0)
                            {
                                if (outBmp[x, y - i] > 0)
                                {
                                    ny = i;
                                    isok1 = true;
                                    break;
                                }
                            }
                        }
                        if (y - ny >= 0) ib = outBmp[x, y - ny];
                        if (ib < 0) ib = 0;
                        ny = 1;
                        isok2 = false;
                        for (int i = 1; i <= ibei * 2; i++)
                        {
                            if (y + i < toh)
                            {
                                if (outBmp[x, y + i] > 0)
                                {
                                    ny = i;
                                    isok2 = true;
                                    break;
                                }
                            }
                        }
                        if (y + ny < toh) ie = outBmp[x, y + ny];
                        if (ie < 0) ie = 0;
                        int ivY = (ib + ie) / 2;

                        if (ivX > ivY)
                        {
                            outBmp[x, y] = ivX;
                        }
                        else
                        {
                            outBmp[x, y] = ivY;
                        }
                        if (outBmp[x, y] > 255) outBmp[x, y] = 255;
                    }
                }
            }
        }

        return outBmp;
    }
    public static unsafe void DrawIntRGB_Bitmap(LineRow line, int[,] intBmpR, int[,] intBmpG, int[,] intBmpB, string savePath)
    {
        int w = (int)(line.r - line.l + 1);
        int h = (int)(line.b - line.t + 1);
        
        Bitmap bmp = new Bitmap(w, h);

        Graphics g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        g.Dispose();

        BitmapData bitData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        int ic = (int)(bitData.Stride - bitData.Width * 3);
        byte* p = (byte*)(bitData.Scan0.ToPointer());
        int iWidth = w * 3 + ic;

        for (int y = (int)line.t; y < (int)line.b; y++)
        {
            for (int x = (int)line.l; x < (int)line.r; x++)
            {
                int ix = x - (int)line.l;
                int iy = y - (int)line.t;
                int ib = iy * iWidth + ix * 3;
                p[ib] = (byte)(255 - intBmpB[x, y]);
                p[ib + 1] = (byte)(255 - intBmpG[x, y]);
                p[ib + 2] = (byte)(255 - intBmpR[x, y]);
            }
        }
        bmp.UnlockBits(bitData);
        saveImage img_save = new saveImage();
        img_save.Save(bmp, savePath);
    }

    /// <summary>
    /// ��������䷽�� �ο� ��䷽�� http://baike.baidu.com/view/1650357.htm
    /// </summary>
    /// <param name="pixelNum"></param>
    /// <returns></returns>
    public static uint getOstu(int[] pixelNum)
    {
        int n, n1, n2;
        double m1, m2, sum, csum, fmax, sb;     //sbΪ��䷽�fmax�洢��󷽲�ֵ
        int k, t, q;
        int threshValue = 1;                      // ��ֵ
        int step = 1;
        //����ֵ
        sum = csum = 0.0;
        n = 0;
        //�����ܵ�ͼ��ĵ����������أ�Ϊ����ļ�����׼��
        for (k = 0; k <= 255; k++)
        {
            sum += (double)k * (double)pixelNum[k];     //x*f(x)�����أ�Ҳ����ÿ���Ҷȵ�ֵ�������������һ����Ϊ���ʣ���sumΪ���ܺ�
            n += pixelNum[k];                       //nΪͼ���ܵĵ�������һ��������ۻ�����
        }

        fmax = -1.0;                          //��䷽��sb������Ϊ��������fmax��ʼֵΪ-1��Ӱ�����Ľ���
        n1 = 0;

        //double[] SB = new double[256];

        for (k = 0; k <= 255; k++)                  //��ÿ���Ҷȣ���0��255������һ�ηָ�����䷽��sb
        {
            n1 += pixelNum[k];                //n1Ϊ�ڵ�ǰ��ֵ��ǰ��ͼ��ĵ���
            if (n1 == 0) { continue; }            //û�зֳ�ǰ����
            n2 = n - n1;                        //n2Ϊ����ͼ��ĵ���
            if (n2 == 0) { break; }               //n2Ϊ0��ʾȫ�����Ǻ�ͼ����n1=0������ƣ�֮��ı���������ʹǰ���������ӣ����Դ�ʱ�����˳�ѭ��
            csum += (double)k * pixelNum[k];    //ǰ���ġ��Ҷȵ�ֵ*����������ܺ�
            m1 = csum / n1;                     //m1Ϊǰ����ƽ���Ҷ�
            m2 = (sum - csum) / n2;               //m2Ϊ������ƽ���Ҷ�
            sb = (double)n1 * (double)n2 * (m1 - m2) * (m1 - m2);   //sbΪ��䷽��
            if (sb > fmax)                  //����������䷽�����ǰһ���������䷽��
            {
                fmax = sb;                    //fmaxʼ��Ϊ�����䷽�otsu��
                threshValue = k;              //ȡ�����䷽��ʱ��Ӧ�ĻҶȵ�k���������ֵ
            }
            //SB[k] = Math.Round(sb/Math.Pow(10, 12));
        }

        //createtxt(SB, file);

        return (uint)threshValue;
    }
    public static uint getKittler(int[] HistGram)
    {
        int X, Y;
        int MinValue, MaxValue;
        int Threshold;
        int PixelBack, PixelFore;
        double OmegaBack, OmegaFore, MinSigma, Sigma, SigmaBack, SigmaFore;
        for (MinValue = 0; MinValue < 256 && HistGram[MinValue] == 0; MinValue++) ;
        for (MaxValue = 255; MaxValue > MinValue && HistGram[MinValue] == 0; MaxValue--) ;
        if (MaxValue == MinValue) return (uint)MaxValue;          // ͼ����ֻ��һ����ɫ             
        if (MinValue + 1 == MaxValue) return (uint)MinValue;      // ͼ����ֻ�ж�����ɫ
        Threshold = -1;
        MinSigma = 1E+20;
        for (Y = MinValue; Y < MaxValue; Y++)
        {
            PixelBack = 0; PixelFore = 0;
            OmegaBack = 0; OmegaFore = 0;
            for (X = MinValue; X <= Y; X++)
            {
                PixelBack += HistGram[X];
                OmegaBack = OmegaBack + X * HistGram[X];
            }
            for (X = Y + 1; X <= MaxValue; X++)
            {
                PixelFore += HistGram[X];
                OmegaFore = OmegaFore + X * HistGram[X];
            }
            OmegaBack = OmegaBack / PixelBack;
            OmegaFore = OmegaFore / PixelFore;
            SigmaBack = 0; SigmaFore = 0;
            for (X = MinValue; X <= Y; X++) SigmaBack = SigmaBack + (X - OmegaBack) * (X - OmegaBack) * HistGram[X];
            for (X = Y + 1; X <= MaxValue; X++) SigmaFore = SigmaFore + (X - OmegaFore) * (X - OmegaFore) * HistGram[X];
            if (SigmaBack == 0 || SigmaFore == 0)
            {
                if (Threshold == -1)
                    Threshold = Y;
            }
            else
            {
                SigmaBack = Math.Sqrt(SigmaBack / PixelBack);
                SigmaFore = Math.Sqrt(SigmaFore / PixelFore);
                Sigma = 1 + 2 * (PixelBack * Math.Log(SigmaBack / PixelBack) + PixelFore * Math.Log(SigmaFore / PixelFore));
                if (Sigma < MinSigma)
                {
                    MinSigma = Sigma;
                    Threshold = Y;
                }
            }
        }
        return (uint)Threshold;
    }

    public static uint getShang(int[] pixelNum)
    {
        int n = 0, n1 = 0, n2 = 0, k;
        double Pthd = 0;

        for (k = 0; k <= 255; k++)
        {
            n += pixelNum[k];
        }

        double max = 0;
        double sb = 0;

        int threshValue = 0;

        for (k = 0; k <= 255; k++)
        {
            n1 += pixelNum[k];
            if (n1 == 0) { continue; }
            n2 = n - n1;
            if (n2 == 0) { break; }

            Pthd = (double)n1 / (double)n;

            double Sum1 = 0;
            double Sum2 = 0;
            for (int f = 0; f <= 255; f++)
            {
                double Pi = pixelNum[f] / (double)n;
                if (f <= k)
                {
                    double P1 = Pi / Pthd;
                    if (P1 != 0)
                    {
                        Sum1 += -P1 * Math.Log(P1);
                    }
                }
                else
                {
                    double P2 = Pi / (1 - Pthd);
                    if (P2 != 0)
                    {
                        Sum2 += -P2 * Math.Log(P2);
                    }
                }
            }
            sb = Sum1 + Sum2;

            //AciDebug.Debug(Sum1 + ":" + Sum2 + " " + Sum1 + ":" + Sum2);

            if (max == 0)
            {
                max = sb;
            }
            if (sb > max)
            {
                max = sb;
                threshValue = k;
            }
        }
        return (uint)threshValue;
    }
    public static uint getHuang(int[] HistGram)
    {
        int X, Y;
        int First, Last;
        int Threshold = -1;
        double BestEntropy = Double.MaxValue, Entropy;
        //   �ҵ���һ�������һ����0��ɫ��ֵ
        for (First = 0; First < HistGram.Length && HistGram[First] == 0; First++) ;
        for (Last = HistGram.Length - 1; Last > First && HistGram[Last] == 0; Last--) ;
        if (First == Last) return (uint)First;                // ͼ����ֻ��һ����ɫ
        if (First + 1 == Last) return (uint)First;            // ͼ����ֻ�ж�����ɫ

        // �����ۼ�ֱ��ͼ�Լ���Ӧ�Ĵ�Ȩ�ص��ۼ�ֱ��ͼ
        int[] S = new int[Last + 1];
        int[] W = new int[Last + 1];            // �����ش�ͼ��������ı������ݿ��ܻᳬ��int�ı�ʾ��Χ�����Կ�����long����������
        S[0] = HistGram[0];
        for (Y = First > 1 ? First : 1; Y <= Last; Y++)
        {
            S[Y] = S[Y - 1] + HistGram[Y];
            W[Y] = W[Y - 1] + Y * HistGram[Y];
        }

        // ������ʽ��4������6�����õĲ��ұ�
        double[] Smu = new double[Last + 1 - First];
        for (Y = 1; Y < Smu.Length; Y++)
        {
            double mu = 1 / (1 + (double)Y / (Last - First));               // ��ʽ��4��
            Smu[Y] = -mu * Math.Log(mu) - (1 - mu) * Math.Log(1 - mu);      // ��ʽ��6��
        }

        // �������������ֵ
        for (Y = First; Y <= Last; Y++)
        {
            Entropy = 0;
            int mu = (int)Math.Round((double)W[Y] / S[Y]);             // ��ʽ17
            for (X = First; X <= Y; X++)
                Entropy += Smu[Math.Abs(X - mu)] * HistGram[X];
            mu = (int)Math.Round((double)(W[Last] - W[Y]) / (S[Last] - S[Y]));  // ��ʽ18       
            for (X = Y + 1; X <= Last; X++)
                Entropy += Smu[Math.Abs(X - mu)] * HistGram[X];       // ��ʽ8
            if (BestEntropy > Entropy)
            {
                BestEntropy = Entropy;      // ȡ��С�ش�Ϊ�����ֵ
                Threshold = Y;
            }
        }
        return (uint)Threshold;
    }
    public static uint getIsoData(int[] HistGram)
    {
        int i, l, toth, totl, h, g = 0;
        for (i = 1; i < HistGram.Length; i++)
        {
            if (HistGram[i] > 0)
            {
                g = i + 1;
                break;
            }
        }
        while (true)
        {
            l = 0;
            totl = 0;
            for (i = 0; i < g; i++)
            {
                totl = totl + HistGram[i];
                l = l + (HistGram[i] * i);
            }
            h = 0;
            toth = 0;
            for (i = g + 1; i < HistGram.Length; i++)
            {
                toth += HistGram[i];
                h += (HistGram[i] * i);
            }
            if (totl > 0 && toth > 0)
            {
                l /= totl;
                h /= toth;
                if (g == (int)Math.Round((l + h) / 2.0))
                    break;
            }
            g++;
            if (g > HistGram.Length - 2)
            {
                return 0;
            }
        }
        return (uint)g;
    }
}

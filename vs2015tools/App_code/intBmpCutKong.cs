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

public unsafe class intBmpCutKong
{
    public AciBmpInt.dotRGB RGB = null;
    public bool show = false;
    public string sAB = "";
    public saveImage imgSave = new saveImage();
    public int icutSmall = 5;
    public float icutS_pp = 0.35f;
    public int icutS_min = 20;
    public int icutS_max = 1800;
    public int icutNum_min = 20;
    public int icutNum_max = 1800;

    public intBmpCutKong()
    {
    }


    public Bitmap setImage(FileInfo file)
    {
        Bitmap bmp = (Bitmap)System.Drawing.Image.FromFile(file.FullName);
        return setImage(ref bmp, file, null);
    }

    public List<Rect> setSmall(ref Bitmap bmp, FileInfo file, AciBmpInt.dotRGB rgb)
    {
        List<Rect> rectBlack = new List<Rect>();
        int ismall = 1000;
        Bitmap bitmap = new Bitmap(ismall, (int)(ismall / (double)bmp.Width * (double)bmp.Height));
        Graphics g = Graphics.FromImage(bitmap);
        g.DrawImage(bmp, 0, 0, bitmap.Width, bitmap.Height);
        g.Dispose();

        if (rgb == null)
        {
            RGB = AciBmpInt.getAvgGrey(bitmap);
        }
        else
        {
            RGB = rgb;
        }

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
        if (ik < 135)
        {
            ik = 135;
        }

        AciBmpInt.toTwo(ref intBmp, ik, bitmap.Width, bitmap.Height);

        List<Rect> arrRect = null;
        if (sAB == "A")
        {
            arrRect = AciBmpInt.cutArea(ref intBmp, file, 0, (int)((float)bitmap.Height * 0.15f),
                (int)((float)bitmap.Width * 0.10f), (int)((float)bitmap.Height * 0.7f));
            for (int i = 0; i < arrRect.Count; i++)
            {
                Rect rect = arrRect[i];
                rect.calculateWH();
                rect.S = rect.width * rect.height;
                if ((float)rect.iNum / (float)rect.S > icutS_pp && rect.S > icutS_min && rect.S < icutS_max && rect.iNum > icutNum_min && rect.iNum < icutNum_max
                    && rect.width >= icutSmall && rect.height >= icutSmall)
                {
                    rectBlack.Add(rect);

                    if (show) rect.Draw(ref intBmp);
                }
                //if (show) rect.Draw(ref intBmp);
            }
        }

        if (sAB == "B" || rectBlack.Count == 0)
        {
            arrRect = AciBmpInt.cutArea(ref intBmp, file, (int)((float)bitmap.Width * 0.9f), (int)((float)bitmap.Height * 0.15f),
                (int)((float)bitmap.Width * 0.10f) - 2, (int)((float)bitmap.Height * 0.7f));
            for (int i = 0; i < arrRect.Count; i++)
            {
                Rect rect = arrRect[i];
                rect.calculateWH();
                rect.S = rect.width * rect.height;
                if ((float)rect.iNum / (float)rect.S > icutS_pp && rect.S > icutS_min && rect.S < icutS_max && rect.iNum > icutNum_min && rect.iNum < icutNum_max
                    && rect.width >= icutSmall && rect.height >= icutSmall)
                {
                    rectBlack.Add(rect);

                    if (show) rect.Draw(ref intBmp);
                }
                //if (show) rect.Draw(ref intBmp);
            }
        }
        if (sAB == "B" && rectBlack.Count == 0)
        {
            arrRect = AciBmpInt.cutArea(ref intBmp, file, 0, (int)((float)bitmap.Height * 0.15f),
                            (int)((float)bitmap.Width * 0.10f), (int)((float)bitmap.Height * 0.7f));
            for (int i = 0; i < arrRect.Count; i++)
            {
                Rect rect = arrRect[i];
                rect.calculateWH();
                rect.S = rect.width * rect.height;
                if ((float)rect.iNum / (float)rect.S > icutS_pp && rect.S > icutS_min && rect.S < icutS_max && rect.iNum > icutNum_min && rect.iNum < icutNum_max
                    && rect.width >= icutSmall && rect.height >= icutSmall)
                {
                    rectBlack.Add(rect);

                    if (show) rect.Draw(ref intBmp);
                }
            }
        }
        if (show) AciBmpInt.DrawIntBmpPng(file.FullName.Replace(".jpg", "_临时_00-2_小图二值化.png"), intBmp);

        bitmap.Dispose();
        return rectBlack;
    }

    public void Clear()
    {
        RGB = null;
        sAB = "";
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="bmp"></param>
    /// <param name="file"></param>
    /// <param name="AB">输入"A"面 "B"面</param>
    /// <param name="rgb"></param>
    /// <returns></returns>
    public Bitmap setImage(ref Bitmap bmp, FileInfo file, AciBmpInt.dotRGB rgb)
    {
        Clear();

        sAB = "A";
        if (file.Name.IndexOf("B") > -1)
        {
            sAB = "B";
        }
        
        List<Rect> rectBlack = setSmall(ref bmp, file, rgb);

        BitmapData bitData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        int ic = bitData.Stride - bitData.Width * 3;
        byte* p = (byte*)(bitData.Scan0.ToPointer());
        int iWidth = bmp.Width * 3 + ic;

        int iw = bmp.Width;
        int ih = bmp.Height;

        float ipwh = 1000 / (float)bmp.Width;
        int igrey = 0;
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
                        int ib = y * iWidth + x * 3;
                        int B = (int)p[ib];
                        int G = (int)p[ib + 1];
                        int R = (int)p[ib + 2];
                        igrey = (B + G + R) / 3;

                        if (igrey < 150)
                        {
                            p[ib] = RGB.B;
                            p[ib + 1] = RGB.G;
                            p[ib + 2] = RGB.R;
                        }
                    }
                }
            }
        }


       
        bmp.UnlockBits(bitData);
        return bmp;
    }
}

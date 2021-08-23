using System;
using System.Collections;
using System.Text;
using System.Data;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;


unsafe public class NPZ
{
    uint w = 0;
    uint h = 0;

    //一行的像素点
    uint iWidth = 0;
    uint ic = 0;
    public bool isNPZ = false;

    public NPZ(FileInfo file)
    {
        isNPZ = false;
        Bitmap bitmap = (Bitmap)System.Drawing.Image.FromFile(file.FullName);
        BitmapData bitData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        ic = (uint)(bitData.Stride - bitData.Width * 3);
        byte* p = (byte*)(bitData.Scan0.ToPointer());
        w = (uint)bitData.Width;
        h = (uint)bitData.Height;
        iWidth = w * 3 + ic;

        uint iR = 0;
        uint iG = 0;
        uint iB = 0;
        uint iGrey = 0;
        uint icount = 0;

        for (uint y = 0; y < 100; y++)
        {
            for (uint x = 0; x < 100; x++)
            {
                uint ib = y * iWidth + x * 3;
                //起始点
                iB = p[ib];
                iG = p[ib + 1];
                iR = p[ib + 2];
                uint igrey = (iB + iG + iR) / 3;
                iGrey += igrey;
                icount++;
            }
        }

        iGrey = iGrey / icount;
        if (iGrey < 200)
        {
            isNPZ = true;
        }

        bitmap.UnlockBits(bitData);
        bitmap.Dispose();
    }
}



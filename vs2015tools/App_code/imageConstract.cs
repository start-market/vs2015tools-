using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

public class imageConstract
{
    uint ic = 0;
    uint w = 0;
    uint h = 0;

    //一行的像素点
    uint iWidth = 0;
    uint r = 0;
    uint g = 0;
    uint b = 0;

    public int iBright = 10;
    public int iConstract = 25;

    public double dR = 1;
    public double dG = 1;
    public double dB = 1;
    Bitmap bitmap = null;

    System.Drawing.Imaging.ImageCodecInfo myImageCodecInfo;
    EncoderParameters encoderParams;


    public imageConstract()
    {
        setDefault();
    }
    void setDefault()
    {
        encoderParams = new EncoderParameters();
        long[] quality;

        //图片质量
        quality = new long[] { 50 };
        EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
        encoderParams.Param[0] = encoderParam;

        myImageCodecInfo = GetEncoderInfo("image/jpeg");
    }
    public void setVar(int bright, int iconstract, double dr, double dg, double db)
    {
        iBright = bright;
        iConstract = iconstract;

        dR = dr;
        dG = dg;
        dB = db;
    }
    
    public unsafe void toConstract(ref Bitmap bmp)
    {
        bitmap = bmp;
        BitmapData bitData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        //取出每行多出来的数
        ic = (uint)(bitData.Stride - bitData.Width * 3);

        //取出全部像素值 每隔 3个  为  R G B 每次循环删除 一个多数的 xx 数
        byte* p = (byte*)(bitData.Scan0.ToPointer());

        w = (uint)bitData.Width;
        h = (uint)bitData.Height;

        //一行的像素点
        iWidth = w * 3 + ic;

        //直方图
        //Histogram Htg = toZhiFang(p);
        //uint Oavg = getOstu(Htg.pixelNum);
        //uint Oshang = getShang(Htg.pixelNum);

        //MessageBox.Show(Oavg + " " + Oshang);
        //
        toConstract(p, iConstract);
        toBright(p, iBright);
        //toConstract(p, 20, 0);
        bitmap.UnlockBits(bitData);
    }

    /// <summary>
    /// 会产生graphics异常的PixelFormat
    /// </summary>
    private static PixelFormat[] indexedPixelFormats = { PixelFormat.Undefined, PixelFormat.DontCare, PixelFormat.Format16bppArgb1555, PixelFormat.Format1bppIndexed, PixelFormat.Format4bppIndexed, PixelFormat.Format8bppIndexed };

    /// <summary>
    /// 判断图片的PixelFormat 是否在 引发异常的 PixelFormat 之中
    /// </summary>
    /// <param name="imgPixelFormat">原图片的PixelFormat</param>
    /// <returns></returns>
    private static bool IsPixelFormatIndexed(PixelFormat imgPixelFormat)
    {
        foreach (PixelFormat pf in indexedPixelFormats)
        {
            if (pf.Equals(imgPixelFormat)) return true;
        }

        return false;
    }
    unsafe int getCenter(BitmapData bitData)
    {
        //取出每行多出来的数
        ic = (uint)(bitData.Stride - bitData.Width * 3);
        //取出全部像素值 每隔 3个  为  R G B 每次循环删除 一个多数的 xx 数
        byte* p = (byte*)(bitData.Scan0.ToPointer());
        w = (uint)bitData.Width;
        h = (uint)bitData.Height;
        //一行的像素点
        iWidth = w * 3 + ic;

        uint iall = 0;
        int ivv = 123;
        for (uint y = 0; y < h; y++)
        {
            for (uint x = 0; x < w; x++)
            {
                uint ib = y * iWidth + x * 3;
                r = p[ib];
                g = p[ib + 1];
                b = p[ib + 2];
                //灰度化
                int iee = (int)(0.11 * (double)r + 0.59 * (double)g + 0.3 * (double)b);
                if (iee < ivv)
                {
                    iall++;
                }
            }
        }
        iall /= 2;
        uint inow = 0;
        int myY = 0;
        for (uint y = 0; y < h; y++)
        {
            for (uint x = 0; x < w; x++)
            {
                uint ib = y * iWidth + x * 3;
                r = p[ib];
                g = p[ib + 1];
                b = p[ib + 2];
                //灰度化
                int iee = (int)(0.11 * (double)r + 0.59 * (double)g + 0.3 * (double)b);
                if (iee < ivv)
                {
                    inow++;
                }
                if (inow > iall)
                {
                    return (int)y;
                }
            }
        }
        return myY;
    }
    //获取ImageCodecInfo
    private static System.Drawing.Imaging.ImageCodecInfo GetEncoderInfo(string mineType)
    {
        System.Drawing.Imaging.ImageCodecInfo[] myEncoders =
        System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
        System.Drawing.Imaging.ImageCodecInfo codeInfo = null;
        foreach (System.Drawing.Imaging.ImageCodecInfo myEncoder in myEncoders)
        {
            if (myEncoder.MimeType == mineType)
            {
                codeInfo = myEncoder;
            }
        }
        return codeInfo;
    }
    /// <summary>
    /// 对比度
    /// </summary>
    /// <returns></returns>
    public unsafe void toConstract(byte* p, int degree)
    {
        if (degree < -100) degree = -100;
        if (degree > 100) degree = 100;
        double pixel = 0;
        double contrast = (100.0 + degree) / 100.0;
        contrast *= contrast;

        for (uint y = 0; y < h; y++)
        {
            for (uint x = 0; x < w; x++)
            {
                //起始点
                uint ib = y * iWidth + x * 3;

                for (int i = 0; i < 3; i++)
                {
                    pixel = ((p[ib + i] / 255.0 - 0.5) * contrast + 0.5) * 255;
                    if (pixel < 0) pixel = 0;
                    if (pixel > 255) pixel = 255;

                    p[ib + i] = (byte)pixel;
                }
            }
        }
    }
    /// <summary>
    /// 亮度
    /// </summary>
    /// <returns></returns>
    public unsafe void toBright(byte* p, int iValue)
    {
        int pix = 0;

        for (uint y = 0; y < h; y++)
        {
            for (uint x = 0; x < w; x++)
            {
                //起始点
                uint ib = y * iWidth + x * 3;

                for (int i = 0; i < 3; i++)
                {
                    pix = p[ib + i] + iValue;

                    if (iValue > 0) p[ib + i] = (byte)Math.Min(255, pix);

                }

            }
        }
    }

    /// <summary>
    /// 二值化
    /// </summary>
    /// <returns></returns>
    public unsafe void toTwo(byte* p, uint Ostu)
    {
        for (uint y = 0; y < h; y++)
        {
            for (uint x = 0; x < w; x++)
            {
                //起始点
                uint ib = y * iWidth + x * 3;
                if (p[ib] <= Ostu)
                {
                    p[ib] = (byte)0;
                    p[ib + 1] = (byte)0;
                    p[ib + 2] = (byte)0;
                }
                else
                {
                    p[ib] = (byte)255;
                    p[ib + 1] = (byte)255;
                    p[ib + 2] = (byte)255;
                }
            }
        }
    }

    /// <summary>
    /// 直方图
    /// </summary>
    /// <returns></returns>
    public unsafe Histogram toZhiFang(byte* p)
    {
        int[] pixelNum = new int[256];
        int max = 0;

        for (uint y = 0; y < h; y++)
        {
            for (uint x = 0; x < w; x++)
            {
                //起始点
                uint ib = y * iWidth + x * 3;
                r = p[ib];
                g = p[ib + 1];
                b = p[ib + 2];
                pixelNum[(r + g + b) / 3]++;
            }
        }
        uint maxK = 0;
        for (uint k = 0; k <= 255; k++)
        {
            if (pixelNum[k] > max)
            {
                max = pixelNum[k];
                maxK = (uint)k;
            }
        }

        //flash查看直方图
        //createtxt(pixelNum);

        return new Histogram(pixelNum, maxK);
    }
    /// <summary> 
    /// 获得最大熵
    /// </summary>
    /// <param name="pixelNum"></param>
    /// <returns></returns>
    public uint getShang(int[] pixelNum)
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
    /// <summary>
    /// 获得最大类间方差 参考 组间方差 http://baike.baidu.com/view/1650357.htm
    /// </summary>
    /// <param name="pixelNum"></param>
    /// <returns></returns>
    public uint getOstu(int[] pixelNum)
    {
        int n, n1, n2;
        double m1, m2, sum, csum, fmax, sb;     //sb为类间方差，fmax存储最大方差值
        int k, t, q;
        int threshValue = 1;                      // 阈值
        int step = 1;
        //求阈值
        sum = csum = 0.0;
        n = 0;
        //计算总的图象的点数和质量矩，为后面的计算做准备
        for (k = 0; k <= 255; k++)
        {
            sum += (double)k * (double)pixelNum[k];     //x*f(x)质量矩，也就是每个灰度的值乘以其点数（归一化后为概率），sum为其总和
            n += pixelNum[k];                       //n为图象总的点数，归一化后就是累积概率
        }

        fmax = -1.0;                          //类间方差sb不可能为负，所以fmax初始值为-1不影响计算的进行
        n1 = 0;

        //double[] SB = new double[256];

        for (k = 0; k <= 255; k++)                  //对每个灰度（从0到255）计算一次分割后的类间方差sb
        {
            n1 += pixelNum[k];                //n1为在当前阈值遍前景图象的点数
            if (n1 == 0) { continue; }            //没有分出前景后景
            n2 = n - n1;                        //n2为背景图象的点数
            if (n2 == 0) { break; }               //n2为0表示全部都是后景图象，与n1=0情况类似，之后的遍历不可能使前景点数增加，所以此时可以退出循环
            csum += (double)k * pixelNum[k];    //前景的“灰度的值*其点数”的总和
            m1 = csum / n1;                     //m1为前景的平均灰度
            m2 = (sum - csum) / n2;               //m2为背景的平均灰度
            sb = (double)n1 * (double)n2 * (m1 - m2) * (m1 - m2);   //sb为类间方差
            if (sb > fmax)                  //如果算出的类间方差大于前一次算出的类间方差
            {
                fmax = sb;                    //fmax始终为最大类间方差（otsu）
                threshValue = k;              //取最大类间方差时对应的灰度的k就是最佳阈值
            }
            //SB[k] = Math.Round(sb/Math.Pow(10, 12));
        }

        //createtxt(SB, file);

        return (uint)threshValue;
    }
    /// <summary>
    /// 直方图结构体
    /// </summary>
    public struct Histogram
    {
        public int[] pixelNum;
        public uint maxK;

        public Histogram(int[] pNum, uint max)
        {
            pixelNum = pNum;
            maxK = max;
        }
    }


}

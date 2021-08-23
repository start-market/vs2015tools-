using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

public unsafe class cuteHeiBian
{
    uint ic = 0;
    uint w = 0;
    uint h = 0;

    uint wBu = 0;
    uint hBu = 0;
    byte* pBu;
    uint iWidthBu = 0;
    int iAvgGreyBu = 0;
    BitmapData bitDataBu;

    //一行的像素点
    uint iWidth = 0;
    uint r = 0;
    uint g = 0;
    uint b = 0;

    public int iBright = 85;
    public int iConstract = 60;

    public double dR = 0.7;
    public double dG = 0.15;
    public double dB = 0.2;
    FileInfo file = null;
    public Bitmap bitmap = null;

    Bitmap bmpBu = null;

    Rectangle rectFrame;
    bool isRectFrame = false;

    System.Drawing.Imaging.ImageCodecInfo myImageCodecInfo;
    EncoderParameters encoderParams;

    /// <summary>是否启动调试</summary>
    public bool bDebug = false;
    /// <summary>是否需要是大黑边</summary>
    public bool isBuBig = false;
    /// <summary>是否需要修图</summary>
    public bool isNeedDo = false;
    /// <summary>自动补色 的最大块黑点总数 默认 10000</summary>
    public int isBuDotMax = 10000;
    /// <summary>贴边黑边中(除去定点的黑块）需要处理的大小伐值 默认 3500</summary>
    public int isBuBianMax = 3500;

    public bool isBuFrame = false;

    /// <summary>整行白色切割最大比例</summary>
    int iperMax = 15;
    /// <summary>整行白色切割最大比例连续次数</summary>
    int itimeMax = 3;

    /// <summary>自动补的底色</summary>
    public dotRGB iColorBu = new dotRGB(255, 255, 255);


    public string savePath;

    public cuteHeiBian()
    {
        setDefault();
    }

    public void open()
    {
        //bmpBu = (Bitmap)System.Drawing.Image.FromFile(Application.StartupPath + "/bgBu.jpg");

        //bitDataBu = bmpBu.LockBits(new Rectangle(0, 0, bmpBu.Width, bmpBu.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        ////取出每行多出来的数
        //ic = (uint)(bitDataBu.Stride - bitDataBu.Width * 3);

        ////取出全部像素值 每隔 3个  为  R G B 每次循环删除 一个多数的 xx 数
        //pBu = (byte*)(bitDataBu.Scan0.ToPointer());

        //wBu = (uint)bitDataBu.Width;
        //hBu = (uint)bitDataBu.Height;

        ////是 212
        ////iAvgGreyBu = getAvgGrey(pBu, 200, wBu, hBu);
        ////iAvgGreyBu = 236;
        ////MessageBox.Show(iAvgGreyBu + " a");

        ////一行的像素点
        //iWidthBu = wBu * 3 + ic;
    }
    public void close()
    {
        //bmpBu.UnlockBits(bitDataBu);
        //bmpBu.Dispose();
    }

    /// <summary>
    /// 处理扫描仪剩余的大黑边
    /// </summary>
    /// <param name="f"></param>
    /// <param name="savepath"></param>
    /// <returns></returns>
    public unsafe Bitmap setImage(FileInfo f)
    {
        isRectFrame = false;
        isNeedDo = false;
        isBuBig = false;

        file = f;
        bitmap = (Bitmap)System.Drawing.Image.FromFile(f.FullName);

        Graphics g = null;
        //if (IsPixelFormatIndexed(bitmap.PixelFormat))
        //{
        //    Bitmap bmp = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
        //    using (g = Graphics.FromImage(bmp))
        //    {
        //        g.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);
        //    }
        //    g.Dispose();
        //    bitmap.Dispose();
        //    bitmap = bmp;
        //}

        BitmapData bitData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        //取出每行多出来的数
        ic = (uint)(bitData.Stride - bitData.Width * 3);

        //取出全部像素值 每隔 3个  为  R G B 每次循环删除 一个多数的 xx 数
        byte* p = (byte*)(bitData.Scan0.ToPointer());

        w = (uint)bitData.Width;
        h = (uint)bitData.Height;

        //一行的像素点
        iWidth = w * 3 + ic;

        //取颜色同时   二值化
        iColorBu = getAvgGrey(p, 80, w, h);

        if (bDebug) bitmap.Save(file.DirectoryName + "/" + Path.GetFileNameWithoutExtension(file.Name) + "_二值化.jpg", myImageCodecInfo, encoderParams);


        //设置标记点（用于两个联通算法共享
        arrRect_P = new uint[w, h];

        //联通区域分割算法 仅四个角
        //ArrayList arrRect = cutArea2(p, file);

        //联通区域 (特殊：区域必须贴边)
        ArrayList arrRectBian = cutArea(p, file);

        ArrayList arrRectBu = new ArrayList();

        //MessageBox.Show(savepath + " " + imove);
        for (int i = 0; i < arrRectBian.Count; i++)
        {
            Rect rect = (Rect)arrRectBian[i];
            if (rect != null)
            {
                isRectFrame = true;
                //rect.Draw(p, iWidth);
                //rect.calculateP(p, rectFrame, -1, iWidth);
                //if (rect.iNumRect > isBuBianMax)
                //{
                //    isRectFrame = true;
                //    isNeedDo = true;
                //    break;
                //}\
                arrRectBu.Add(rect);
            }
        }

        //bitmap.Save(file.DirectoryName + "/" + Path.GetFileNameWithoutExtension(file.Name) + "_二值化.jpg", myImageCodecInfo, encoderParams);

        bitmap.UnlockBits(bitData);


        if (bDebug)
        {
            //画倾斜线
            bitmap.Save(file.DirectoryName + "/" + Path.GetFileNameWithoutExtension(file.Name) + "_大黑边.jpg", myImageCodecInfo, encoderParams);
        }


        if (isRectFrame)
        {
            //iColorBu = (byte)(iColorBu + (255 - iColorBu) / 2);
            //iColorBu = 255;
            bitmap.Dispose();
            bitmap = (Bitmap)System.Drawing.Image.FromFile(f.FullName);


            //有需要 自动补色的块
            if (arrRectBu.Count > 0)
            {
                bitData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

                //取出每行多出来的数
                ic = (uint)(bitData.Stride - bitData.Width * 3);

                //取出全部像素值 每隔 3个  为  R G B 每次循环删除 一个多数的 xx 数
                p = (byte*)(bitData.Scan0.ToPointer());

                w = (uint)bitData.Width;
                h = (uint)bitData.Height;

                //一行的像素点
                iWidth = w * 3 + ic;

                for (int i = 0; i < arrRectBu.Count; i++)
                {
                    Rect rect = (Rect)arrRectBu[i];
                    for (int j = 0; j < rect.Al_dot.Count; j++)
                    {
                        myDot dot = (myDot)rect.Al_dot[j];
                        if (dot.x >= 0 && dot.x < w && dot.y >= 0 && dot.y < h)
                        {
                            uint ib = dot.y * iWidth + dot.x * 3;
                            //uint ibBu = (dot.y % hBu) * iWidthBu + (dot.x % wBu) * 3;

                            //int iRr = (int)iColorBu;
                            //int iGg = (int)iColorBu;
                            //int iBb = (int)iColorBu;

                            //iR = pBu[ibBu];
                            //iG = pBu[ibBu + 1];
                            //iB = pBu[ibBu + 2];

                            p[ib] = iColorBu.B;
                            p[ib + 1] = iColorBu.G;
                            p[ib + 2] = iColorBu.R;
                            //AciDebug.Debug(dot.x + " " + dot.y + " " + iRr);
                        }
                    }
                }
                bitmap.UnlockBits(bitData);
            }
        }
        else
        {
            bitmap.Dispose();
            bitmap = (Bitmap)System.Drawing.Image.FromFile(f.FullName);
        }
        isBuFrame = isRectFrame;
        return bitmap;
    }

    public class myK
    {
        /// <summary>斜率</summary>
        public double k = 0;
        /// <summary>截距</summary>
        public double b = 0;
        /// <summary>角度</summary>
        public double r = 0;

        /// <summary>x轴点1</summary>
        public PointF pX_1 = new Point();
        /// <summary>x轴点2</summary>
        public PointF pX_2 = new Point();

        /// <summary>y轴点1</summary>
        public PointF pY_1 = new Point();
        /// <summary>y轴点2</summary>
        public PointF pY_2 = new Point();

        /// <summary></summary>
        public PointF p1 = new Point();
        /// <summary></summary>
        public PointF p2 = new Point();

        public myK()
        {
        }
        public void calculate()
        {
            //pX = new PointF((float)(-b / k), 0);
            //pY = new PointF(0, (float)b);
        }
        public void calculate(Rectangle rect)
        {
            pX_1 = calculateBy_x(rect.X);
            pX_2 = calculateBy_x(rect.Width);

            pY_1 = calculateBy_y(rect.Y);
            pY_2 = calculateBy_y(rect.Height);

            bool is1 = false;
            if (pX_1.X >= rect.X && pX_1.Y >= rect.Y && pX_1.X <= rect.Width && pX_1.Y <= rect.Height)
            {
                if (!is1)
                {
                    p1 = pX_1;
                    is1 = true;
                }
                else
                {
                    p2 = pX_1;
                }
            }
            if (pX_2.X >= rect.X && pX_2.Y >= rect.Y && pX_2.X <= rect.Width && pX_2.Y <= rect.Height)
            {
                if (!is1)
                {
                    p1 = pX_2;
                    is1 = true;
                }
                else
                {
                    p2 = pX_2;
                }
            }
            if (pY_1.X >= rect.X && pY_1.Y >= rect.Y && pY_1.X <= rect.Width && pY_1.Y <= rect.Height)
            {
                if (!is1)
                {
                    p1 = pY_1;
                    is1 = true;
                }
                else
                {
                    p2 = pY_1;
                }
            }
            if (pY_2.X >= rect.X && pY_2.Y >= rect.Y && pY_2.X <= rect.Width && pY_2.Y <= rect.Height)
            {
                if (!is1)
                {
                    p1 = pY_2;
                    is1 = true;
                }
                else
                {
                    p2 = pY_2;
                }
            }
        }
        public PointF calculateBy_x(float x)
        {
            return new PointF(x, (float)(x * k + b));
        }
        public PointF calculateBy_y(float y)
        {
            return new PointF((float)((y - b) / k), y);
        }
    }
    //直线拟合
    private myK getKB(ArrayList al)
    {
        myK kb = new myK();
        //获取数组长度
        int n = al.Count;

        //求模型中的几个和
        double sumX = 0.0, sumY = 0.0, sumXX = 0.0, sumYY = 0.0, sumXY = 0.0;
        for (int i = 0; i < n; i++)
        {
            myDot point = (myDot)al[i];
            sumX += point.x;    //Σx
            sumY += point.y;    //Σy
            sumXX += point.x * point.x;    //Σx^2
            sumYY += point.y * point.y;    //Σy^2
            sumXY += point.x * point.y;    //Σxy
        }

        //求模型中的几个均值
        double meanX = 0.0, meanY = 0.0;
        meanX = sumX / n;
        meanY = sumY / n;

        //斜率
        kb.k = (n * sumXY - sumX * sumY) / (n * sumXX - Math.Pow(sumX, 2));
        //截距
        kb.b = meanY - kb.k * meanX;
        //角度
        kb.r = Math.Atan(kb.k) / Math.PI * 180;

        kb.calculate(rectFrame);

        return kb;
    }

    /// <summary>
    /// 处理小黑边
    /// </summary>
    /// <param name="f"></param>
    /// <param name="savepath"></param>
    /// <returns></returns>
    public unsafe bool Do(FileInfo f, string savepath)
    {
        isBuBig = false;
        isRectFrame = false;
        file = f;
        bitmap = (Bitmap)System.Drawing.Image.FromFile(f.FullName);
        savePath = savepath;

        Graphics g = null;
        if (IsPixelFormatIndexed(bitmap.PixelFormat))
        {
            Bitmap bmp = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
            using (g = Graphics.FromImage(bmp))
            {
                g.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);
            }
            g.Dispose();
            bitmap.Dispose();
            bitmap = bmp;
        }

        BitmapData bitData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        //取出每行多出来的数
        ic = (uint)(bitData.Stride - bitData.Width * 3);

        //取出全部像素值 每隔 3个  为  R G B 每次循环删除 一个多数的 xx 数
        byte* p = (byte*)(bitData.Scan0.ToPointer());

        w = (uint)bitData.Width;
        h = (uint)bitData.Height;

        //一行的像素点
        iWidth = w * 3 + ic;

        //求平均灰度
        //int iAvgGrey = getAvgGrey(p, 180, w, h);
        //MessageBox.Show(iAvgGrey + " b");

        Histogram Htg = toZhiFang(p);

        uint Oavg = 50;
        //uint Oavg = getOstu(Htg.pixelNum);
        //uint Oshang = getShang(Htg.pixelNum);

        //AciDebug.Debug(Oavg + " " + Oshang);
        //Oavg = 50;

        //二值化
        toTwo(p, Oavg);
        //联通区域 (特殊：区域必须贴边）
        ArrayList arrRect = cutArea(p, file);

        bitmap.UnlockBits(bitData);
        if (bDebug) bitmap.Save(file.DirectoryName + "/" + Path.GetFileNameWithoutExtension(savePath) + "_小黑边.jpg", myImageCodecInfo, encoderParams);
        bitmap.Dispose();

        //MessageBox.Show("sad");


        /////////////////////////////////////////////////////////////////////////////////////
        //取得区域后更新图片
        /////////////////////////////////////////////////////////////////////////////////////
        bitmap = (Bitmap)System.Drawing.Image.FromFile(f.FullName);

        if (IsPixelFormatIndexed(bitmap.PixelFormat))
        {
            Bitmap bmp = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
            using (g = Graphics.FromImage(bmp))
            {
                g.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);
            }
            g.Dispose();
            bitmap.Dispose();
            bitmap = bmp;
        }

        bitData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        //取出每行多出来的数
        ic = (uint)(bitData.Stride - bitData.Width * 3);

        //取出全部像素值 每隔 3个  为  R G B 每次循环删除 一个多数的 xx 数
        p = (byte*)(bitData.Scan0.ToPointer());

        w = (uint)bitData.Width;
        h = (uint)bitData.Height;

        //一行的像素点
        iWidth = w * 3 + ic;

        //int icColor = (iAvgGrey - iAvgGreyBu);
        //MessageBox.Show(icColor + "");

        bool isBu = false;

        for (int i = 0; i < arrRect.Count; i++)
        {
            Rect rect = (Rect)arrRect[i];
            //rect.calculate2();

            //AciDebug.Debug(f.Name + "----------------贴边：" + rect.iNumTie + "  总点数：" + rect.iNum  + " 实心点：" + rect.iNumShi + " 贴边比：" + rect.iTie_per + "  长宽比：" + rect.iWH_per + " 密度比：" + rect.P);

            if (rect.iNum < 100000 && rect.iNumTie > 10 && (rect.iTie_per > 100 || rect.P > 700))
            {
                for (int j = 0; j < rect.Al_dot.Count; j++)
                {
                    isBu = true;
                    myDot dot = (myDot)rect.Al_dot[j];
                    uint ib = dot.y * iWidth + dot.x * 3;
                    uint ibBu = (dot.y % hBu) * iWidthBu + (dot.x % wBu) * 3;

                    int iR = 0;
                    int iG = 0;
                    int iB = 0;

                    iR = pBu[ibBu];
                    iG = pBu[ibBu + 1];
                    iB = pBu[ibBu + 2];

                    p[ib] = (byte)iR;
                    p[ib + 1] = (byte)iG;
                    p[ib + 2] = (byte)iB;
                }
            }
            else if (rect.iNum >= 100000)
            {
                //isBuBig = true;
            }
        }
        if (isBu)
        {
            bitmap.UnlockBits(bitData);
            bitmap.Save(savePath, myImageCodecInfo, encoderParams);
            bitmap.Dispose();
        }

        return isBu;
    }

    public uint getR(byte* p, uint x, uint y)
    {
        uint ib = y * iWidth + x * 3;
        return p[ib];
    }
    /// <summary>
    /// 计算黑框切割
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public unsafe int cutRectHeibian(byte* p)
    {
        uint r1 = getR(p, 0, 0);
        uint r3 = getR(p, w - 1, 0);
        uint r7 = getR(p, 0, h - 1);
        uint r9 = getR(p, w - 1, h - 1);

        int imove = 0;
        int itime = 0;

        if (r1 == 0 && r3 == 0)
        {
            //从上至下去黑边
            for (uint y = 0; y < h; y++)
            {
                int iHei = 0;
                for (uint x = 0; x < w; x++)
                {
                    uint ib = y * iWidth + x * 3;
                    if (p[ib] == 0)
                    {
                        iHei++;
                    }
                }
                double iper = (double)(w - iHei) / (double)w * 100;
                if (iper >= iperMax)
                {
                    itime++;
                    if (itime > itimeMax)
                    {
                        rectFrame.Y = (int)y;
                        int im = (int)y;
                        if (im > imove) imove = im;
                        break;
                    }
                }
                else
                {
                    itime = 0;
                }
                //AciDebug.Debug(+ " " + y);
            }
        }

        itime = 0;
        if (r7 == 0 && r9 == 0)
        {
            //从下至上去黑边
            for (uint y = h - 1; y < h; y--)
            {
                int iHei = 0;
                for (uint x = 0; x < w; x++)
                {
                    uint ib = y * iWidth + x * 3;
                    if (p[ib] == 0)
                    {
                        iHei++;
                    }
                }
                double iper = (double)(w - iHei) / (double)w * 100;
                if (iper >= iperMax)
                {
                    itime++;
                    if (itime > itimeMax)
                    {
                        rectFrame.Height = (int)y;
                        int im = (int)(h - y);
                        if (im > imove) imove = im;
                        break;
                    }
                }
                else
                {
                    itime = 0;
                }
                //AciDebug.Debug(+ " " + y);
            }
        }

        itime = 0;
        if (r1 == 0 && r7 == 0)
        {
            //从左至右去黑边
            for (uint x = 0; x < w; x++)
            {
                int iHei = 0;
                for (uint y = 0; y < h; y++)
                {
                    uint ib = y * iWidth + x * 3;
                    if (p[ib] == 0)
                    {
                        iHei++;
                    }
                }
                double iper = (double)(h - iHei) / (double)h * 100;
                if (iper >= iperMax)
                {
                    itime++;
                    if (itime > itimeMax)
                    {
                        rectFrame.X = (int)x;
                        int im = (int)x;
                        if (im > imove) imove = im;
                        break;
                    }
                }
                else
                {
                    itime = 0;
                }
                //AciDebug.Debug(+ " " + y);
            }
        }


        itime = 0;
        if (r3 == 0 && r9 == 0)
        {
            //从右至左去黑边
            for (uint x = w - 1; x < w; x--)
            {
                int iHei = 0;
                for (uint y = 0; y < h; y++)
                {
                    uint ib = y * iWidth + x * 3;
                    if (p[ib] == 0)
                    {
                        iHei++;
                    }
                }
                double iper = (double)(h - iHei) / (double)h * 100;
                if (iper >= iperMax)
                {
                    itime++;
                    if (itime > itimeMax)
                    {
                        rectFrame.Width = (int)x;
                        int im = (int)(w - x);
                        if (im > imove) imove = im;
                        //AciDebug.Debug(iper + " " + x);
                        break;
                    }
                }
                else
                {
                    itime = 0;
                }
            }
        }

        return imove;
    }

    public void createRect(Rect rect, FileInfo file)
    {
        string s = "";
        s = rect.l + " " + rect.t + " " + rect.r + " " + rect.b + " " + rect.iNum + ";";
        StreamWriter sw = new StreamWriter(file.DirectoryName + "/" + Path.GetFileNameWithoutExtension(file.Name) + "_2.rect", true);
        sw.Write(s);
        sw.Flush();
        sw.Close();
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
        public int R = 0;
        public int G = 0;
        public int B = 0;
        public int iC = 0;
        public greyRGB()
        {

        }
        public void add(int r, int g, int b)
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
    /// 求平均灰度
    /// </summary>
    /// <param name="p"></param>
    /// <param name="Ostu">大于这个值得平均灰度</param>
    /// <param name="iw"></param>
    /// <param name="ih"></param>
    /// <returns></returns>
    public unsafe dotRGB getAvgGrey(byte* p, uint Ostu, uint iw, uint ih)
    {
        greyRGB[] arrDot = new greyRGB[256];
        for (int i = 0; i < arrDot.Length; i++)
        {
            arrDot[i] = new greyRGB();
        }

        for (uint y = 0; y < ih; y++)
        {
            for (uint x = 0; x < iw; x++)
            {
                //起始点
                uint ib = y * iWidth + x * 3;
                int B = p[ib];
                int G = p[ib + 1];
                int R = p[ib + 2];
                int igrey = (R + G + B) / 3;

                arrDot[igrey].add(R, G, B);

                if (igrey <= 80)
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
        int imax = 0;
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
    /// 用于标记
    /// </summary>
    uint[,] arrRect_P = null;
    /// <summary>
    /// 联通区域分割算法 仅四个角
    /// </summary>
    public unsafe ArrayList cutArea2(byte* p, FileInfo file)
    {
        ArrayList arrRect = new ArrayList();
        arrRect.Add(getRect(p, (uint)rectFrame.X, (uint)rectFrame.Y));
        arrRect.Add(getRect(p, (uint)rectFrame.Width - 1, (uint)rectFrame.Y));
        arrRect.Add(getRect(p, (uint)rectFrame.X, (uint)rectFrame.Height - 1));
        arrRect.Add(getRect(p, (uint)rectFrame.Width - 1, (uint)rectFrame.Height - 1));

        return arrRect;
    }
    public unsafe Rect getRect(byte* p, uint x, uint y)
    {
        Rect rect = null;
        uint ib = y * iWidth + x * 3;
        r = p[ib];
        uint dot = 0;
        myDot point;

        //Rect rectWH = new Rect();
        //rectWH.l = x;
        //rectWH.t = y;
        //rectWH.r = x + 1;
        //rectWH.b = y + 1;
        //getRectWH(ref rectWH, p, x, y, -1, 0);
        //getRectWH(ref rectWH, p, x, y, 1, 0);
        //getRectWH(ref rectWH, p, x, y, 0, -1);
        //getRectWH(ref rectWH, p, x, y, 0, 1);
        //MessageBox.Show(rectWH.r + " " + rectWH.b);
        //联通锥
        ArrayList Al_sp = null;

        if (r == 0 && arrRect_P[x, y] == 0)
        {
            //基点入锥
            Al_sp = new ArrayList();
            //arrP[x, y] = 1;
            Al_sp.Add(new myDot(x, y));

            rect = new Rect();
            rect.l = x;
            rect.t = y;
            rect.r = x + 1;
            rect.b = y + 1;

            while (Al_sp.Count != 0)
            {
                //取第一个点计算8方向
                point = (myDot)Al_sp[0];
                for (int irow = -1; irow <= 1; irow++)
                {
                    for (int icell = -1; icell <= 1; icell++)
                    {
                        uint thisx = (uint)((int)point.x + icell);
                        uint thisy = (uint)((int)point.y + irow);
                        //不超出边界
                        //if (thisx >= rectWH.l && thisx < rectWH.r && thisy >= rectWH.t && thisy < rectWH.b)
                        if (thisx >= rectFrame.X && thisx < rectFrame.Width && thisy >= rectFrame.Y && thisy < rectFrame.Height)
                        {
                            dot = p[thisy * iWidth + thisx * 3];

                            if (dot == 0 && arrRect_P[thisx, thisy] == 0)
                            {
                                //新点入锥
                                arrRect_P[thisx, thisy] = 1;
                                myDot dott = new myDot(thisx, thisy);
                                //if (thisx == rectFrame.X || thisx == rectFrame.Width - 1 || thisy == rectFrame.Y || thisy == rectFrame.Height - 1)
                                //{
                                //    //判断贴边点
                                //    dott.bTie = true;
                                //}

                                //if (getTopDot(p, thisx, thisy) == 0 && getBottomDot(p, thisx, thisy) == 0 && getLeftDot(p, thisx, thisy) == 0 && getRightDot(p, thisx, thisy) == 0)
                                //{
                                //    //判断实点
                                //    dott.bShi = true;
                                //}

                                //AciDebug.Debug(thisx + "---" + thisy + "---" + x + "--" + y);

                                //if (thisx == 0 && thisy == 0)
                                //{
                                //    rect.dotLeftTop = dott;
                                //}
                                //else if (thisx == rectFrame.Width - 1 && thisy == 0)
                                //{
                                //    rect.dotRightTop = dott;
                                //}
                                //else if (thisx == 0 && thisy == rectFrame.Height - 1)
                                //{
                                //    rect.dotLeftBottom = dott;
                                //}
                                //else if (thisx == rectFrame.Width - 1 && thisy == rectFrame.Height - 1)
                                //{
                                //    rect.dotRightBottom = dott;
                                //}

                                Al_sp.Add(dott);
                                rect.Add(dott);
                                //重新设置矩形大小
                                if (thisx < rect.l)
                                {
                                    rect.l = thisx;
                                }
                                else if (thisx > rect.r)
                                {
                                    rect.r = thisx;
                                }
                                if (thisy < rect.t)
                                {
                                    rect.t = thisy;
                                }
                                else if (thisy > rect.b)
                                {
                                    rect.b = thisy;
                                }
                            }
                        }
                    }
                }
                //出锥
                Al_sp.RemoveAt(0);
            }
        }
        return rect;
    }

    public unsafe void getRectWH(ref Rect rect, byte* p, uint x, uint y, int to_x, int to_y)
    {
        uint ib = y * iWidth + x * 3;
        r = p[ib];
        uint dot = 0;
        myDot point;

        //联通锥
        ArrayList Al_sp = null;

        if (r == 0)
        {
            //基点入锥
            Al_sp = new ArrayList();
            //arrP[x, y] = 1;
            Al_sp.Add(new myDot(x, y));

            while (Al_sp.Count != 0)
            {
                //取第一个点计算8方向
                point = (myDot)Al_sp[0];

                uint thisx = (uint)((int)point.x + to_x);
                uint thisy = (uint)((int)point.y + to_y);
                //不超出边界
                if (thisx >= rectFrame.X && thisx < rectFrame.Width && thisy >= rectFrame.Y && thisy < rectFrame.Height)
                {
                    dot = p[thisy * iWidth + thisx * 3];
                    if (dot == 0)
                    {
                        //新点入锥
                        myDot dott = new myDot(thisx, thisy);
                        Al_sp.Add(dott);
                        rect.Add(dott);
                        //重新设置矩形大小
                        if (thisx < rect.l)
                        {
                            rect.l = thisx;
                        }
                        else if (thisx > rect.r)
                        {
                            rect.r = thisx;
                        }
                        if (thisy < rect.t)
                        {
                            rect.t = thisy;
                        }
                        else if (thisy > rect.b)
                        {
                            rect.b = thisy;
                        }
                    }
                }
                //出锥
                Al_sp.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// 联通区域分割算法 (特殊：区域必须贴边）
    /// </summary>
    public unsafe ArrayList cutArea(byte* p, FileInfo file)
    {
        ArrayList arrRect = new ArrayList();
        Rect rect = null;
        myDot point;
        uint dot = 0;

        //联通锥
        ArrayList Al_sp = null;

        for (uint y = 0; y < h; y++)
        {
            for (uint x = 0; x < w; x++)
            {
                if (x == 0 || x == w - 1 || y == 0 || y == h - 1)
                {
                    uint ib = y * iWidth + x * 3;
                    r = p[ib];
                    if (r == 0 && arrRect_P[x, y] == 0)
                    {
                        //基点入锥
                        Al_sp = new ArrayList();
                        //arrRect_P[x, y] = 1;
                        Al_sp.Add(new myDot(x, y));

                        rect = new Rect();
                        rect.l = x;
                        rect.t = y;
                        rect.r = x + 1;
                        rect.b = y + 1;

                        while (Al_sp.Count != 0)
                        {
                            //取第一个点计算8方向
                            point = (myDot)Al_sp[0];
                            for (int irow = -1; irow <= 1; irow++)
                            {
                                for (int icell = -1; icell <= 1; icell++)
                                {
                                    uint thisx = (uint)((int)point.x + icell);
                                    uint thisy = (uint)((int)point.y + irow);
                                    //不超出边界
                                    if (thisx >= 0 && thisx < w && thisy >= 0 && thisy < h)
                                    {
                                        dot = p[thisy * iWidth + thisx * 3];

                                        if (dot == 0 && arrRect_P[thisx, thisy] == 0)
                                        {
                                            //新点入锥
                                            arrRect_P[thisx, thisy] = 1;
                                            myDot dott = new myDot(thisx, thisy);
                                            //if (thisx == 0 || thisx == w - 1 || thisy == 0 || thisy == h - 1)
                                            //{
                                            //    //判断贴边点
                                            //    dott.bTie = true;
                                            //}

                                            //if (getTopDot(p, thisx, thisy) == 0 && getBottomDot(p, thisx, thisy) == 0 && getLeftDot(p, thisx, thisy) == 0 && getRightDot(p, thisx, thisy) == 0)
                                            //{
                                            //    //判断实点
                                            //    dott.bShi = true;
                                            //}

                                            //AciDebug.Debug(thisx + "---" + thisy + "---" + x + "--" + y);

                                            //if (thisx == 0 && thisy == 0)
                                            //{
                                            //    rect.dotLeftTop = dott;
                                            //}
                                            //else if (thisx == w - 1 && thisy == 0)
                                            //{
                                            //    rect.dotRightTop = dott;
                                            //}
                                            //else if (thisx == 0 && thisy == h - 1)
                                            //{
                                            //    rect.dotLeftBottom = dott;
                                            //}
                                            //else if (thisx == w - 1 && thisy == h - 1)
                                            //{
                                            //    rect.dotRightBottom = dott;
                                            //}

                                            Al_sp.Add(dott);
                                            rect.Add(dott);
                                            //重新设置矩形大小
                                            if (thisx < rect.l)
                                            {
                                                rect.l = thisx;
                                            }
                                            else if (thisx > rect.r)
                                            {
                                                rect.r = thisx;
                                            }
                                            if (thisy < rect.t)
                                            {
                                                rect.t = thisy;
                                            }
                                            else if (thisy > rect.b)
                                            {
                                                rect.b = thisy;
                                            }
                                        }
                                    }
                                }
                            }
                            //出锥
                            Al_sp.RemoveAt(0);
                        }
                        arrRect.Add(rect);
                        //createRect(rect, file);
                    }
                }
            }
        }
        return arrRect;
    }
    unsafe int getTopDot(byte* p, uint x, uint y)
    {
        if (y > rectFrame.Y)
        {
            return p[(y - 1) * iWidth + x * 3];
        }
        else
        {
            return 0;
        }
    }
    unsafe int getBottomDot(byte* p, uint x, uint y)
    {
        if (y < rectFrame.Height - 1)
        {
            return p[(y + 1) * iWidth + x * 3];
        }
        else
        {
            return 0;
        }
    }

    unsafe int getLeftDot(byte* p, uint x, uint y)
    {
        if (x > rectFrame.X)
        {
            return p[y * iWidth + (x - 1) * 3];
        }
        else
        {
            return 0;
        }
    }
    unsafe int getRightDot(byte* p, uint x, uint y)
    {
        if (x < rectFrame.Width - 1)
        {
            return p[y * iWidth + (x + 1) * 3];
        }
        else
        {
            return 0;
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
                int igrey = (p[ib] + p[ib + 1] + p[ib + 2]) / 3;
                if (igrey <= Ostu)
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

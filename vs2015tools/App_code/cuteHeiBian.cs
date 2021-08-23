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

    //һ�е����ص�
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

    /// <summary>�Ƿ���������</summary>
    public bool bDebug = false;
    /// <summary>�Ƿ���Ҫ�Ǵ�ڱ�</summary>
    public bool isBuBig = false;
    /// <summary>�Ƿ���Ҫ��ͼ</summary>
    public bool isNeedDo = false;
    /// <summary>�Զ���ɫ ������ڵ����� Ĭ�� 10000</summary>
    public int isBuDotMax = 10000;
    /// <summary>���ߺڱ���(��ȥ����ĺڿ飩��Ҫ����Ĵ�С��ֵ Ĭ�� 3500</summary>
    public int isBuBianMax = 3500;

    public bool isBuFrame = false;

    /// <summary>���а�ɫ�и�������</summary>
    int iperMax = 15;
    /// <summary>���а�ɫ�и���������������</summary>
    int itimeMax = 3;

    /// <summary>�Զ����ĵ�ɫ</summary>
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

        ////ȡ��ÿ�ж��������
        //ic = (uint)(bitDataBu.Stride - bitDataBu.Width * 3);

        ////ȡ��ȫ������ֵ ÿ�� 3��  Ϊ  R G B ÿ��ѭ��ɾ�� һ�������� xx ��
        //pBu = (byte*)(bitDataBu.Scan0.ToPointer());

        //wBu = (uint)bitDataBu.Width;
        //hBu = (uint)bitDataBu.Height;

        ////�� 212
        ////iAvgGreyBu = getAvgGrey(pBu, 200, wBu, hBu);
        ////iAvgGreyBu = 236;
        ////MessageBox.Show(iAvgGreyBu + " a");

        ////һ�е����ص�
        //iWidthBu = wBu * 3 + ic;
    }
    public void close()
    {
        //bmpBu.UnlockBits(bitDataBu);
        //bmpBu.Dispose();
    }

    /// <summary>
    /// ����ɨ����ʣ��Ĵ�ڱ�
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

        //ȡ��ÿ�ж��������
        ic = (uint)(bitData.Stride - bitData.Width * 3);

        //ȡ��ȫ������ֵ ÿ�� 3��  Ϊ  R G B ÿ��ѭ��ɾ�� һ�������� xx ��
        byte* p = (byte*)(bitData.Scan0.ToPointer());

        w = (uint)bitData.Width;
        h = (uint)bitData.Height;

        //һ�е����ص�
        iWidth = w * 3 + ic;

        //ȡ��ɫͬʱ   ��ֵ��
        iColorBu = getAvgGrey(p, 80, w, h);

        if (bDebug) bitmap.Save(file.DirectoryName + "/" + Path.GetFileNameWithoutExtension(file.Name) + "_��ֵ��.jpg", myImageCodecInfo, encoderParams);


        //���ñ�ǵ㣨����������ͨ�㷨����
        arrRect_P = new uint[w, h];

        //��ͨ����ָ��㷨 ���ĸ���
        //ArrayList arrRect = cutArea2(p, file);

        //��ͨ���� (���⣺�����������)
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

        //bitmap.Save(file.DirectoryName + "/" + Path.GetFileNameWithoutExtension(file.Name) + "_��ֵ��.jpg", myImageCodecInfo, encoderParams);

        bitmap.UnlockBits(bitData);


        if (bDebug)
        {
            //����б��
            bitmap.Save(file.DirectoryName + "/" + Path.GetFileNameWithoutExtension(file.Name) + "_��ڱ�.jpg", myImageCodecInfo, encoderParams);
        }


        if (isRectFrame)
        {
            //iColorBu = (byte)(iColorBu + (255 - iColorBu) / 2);
            //iColorBu = 255;
            bitmap.Dispose();
            bitmap = (Bitmap)System.Drawing.Image.FromFile(f.FullName);


            //����Ҫ �Զ���ɫ�Ŀ�
            if (arrRectBu.Count > 0)
            {
                bitData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

                //ȡ��ÿ�ж��������
                ic = (uint)(bitData.Stride - bitData.Width * 3);

                //ȡ��ȫ������ֵ ÿ�� 3��  Ϊ  R G B ÿ��ѭ��ɾ�� һ�������� xx ��
                p = (byte*)(bitData.Scan0.ToPointer());

                w = (uint)bitData.Width;
                h = (uint)bitData.Height;

                //һ�е����ص�
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
        /// <summary>б��</summary>
        public double k = 0;
        /// <summary>�ؾ�</summary>
        public double b = 0;
        /// <summary>�Ƕ�</summary>
        public double r = 0;

        /// <summary>x���1</summary>
        public PointF pX_1 = new Point();
        /// <summary>x���2</summary>
        public PointF pX_2 = new Point();

        /// <summary>y���1</summary>
        public PointF pY_1 = new Point();
        /// <summary>y���2</summary>
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
    //ֱ�����
    private myK getKB(ArrayList al)
    {
        myK kb = new myK();
        //��ȡ���鳤��
        int n = al.Count;

        //��ģ���еļ�����
        double sumX = 0.0, sumY = 0.0, sumXX = 0.0, sumYY = 0.0, sumXY = 0.0;
        for (int i = 0; i < n; i++)
        {
            myDot point = (myDot)al[i];
            sumX += point.x;    //��x
            sumY += point.y;    //��y
            sumXX += point.x * point.x;    //��x^2
            sumYY += point.y * point.y;    //��y^2
            sumXY += point.x * point.y;    //��xy
        }

        //��ģ���еļ�����ֵ
        double meanX = 0.0, meanY = 0.0;
        meanX = sumX / n;
        meanY = sumY / n;

        //б��
        kb.k = (n * sumXY - sumX * sumY) / (n * sumXX - Math.Pow(sumX, 2));
        //�ؾ�
        kb.b = meanY - kb.k * meanX;
        //�Ƕ�
        kb.r = Math.Atan(kb.k) / Math.PI * 180;

        kb.calculate(rectFrame);

        return kb;
    }

    /// <summary>
    /// ����С�ڱ�
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

        //ȡ��ÿ�ж��������
        ic = (uint)(bitData.Stride - bitData.Width * 3);

        //ȡ��ȫ������ֵ ÿ�� 3��  Ϊ  R G B ÿ��ѭ��ɾ�� һ�������� xx ��
        byte* p = (byte*)(bitData.Scan0.ToPointer());

        w = (uint)bitData.Width;
        h = (uint)bitData.Height;

        //һ�е����ص�
        iWidth = w * 3 + ic;

        //��ƽ���Ҷ�
        //int iAvgGrey = getAvgGrey(p, 180, w, h);
        //MessageBox.Show(iAvgGrey + " b");

        Histogram Htg = toZhiFang(p);

        uint Oavg = 50;
        //uint Oavg = getOstu(Htg.pixelNum);
        //uint Oshang = getShang(Htg.pixelNum);

        //AciDebug.Debug(Oavg + " " + Oshang);
        //Oavg = 50;

        //��ֵ��
        toTwo(p, Oavg);
        //��ͨ���� (���⣺����������ߣ�
        ArrayList arrRect = cutArea(p, file);

        bitmap.UnlockBits(bitData);
        if (bDebug) bitmap.Save(file.DirectoryName + "/" + Path.GetFileNameWithoutExtension(savePath) + "_С�ڱ�.jpg", myImageCodecInfo, encoderParams);
        bitmap.Dispose();

        //MessageBox.Show("sad");


        /////////////////////////////////////////////////////////////////////////////////////
        //ȡ����������ͼƬ
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

        //ȡ��ÿ�ж��������
        ic = (uint)(bitData.Stride - bitData.Width * 3);

        //ȡ��ȫ������ֵ ÿ�� 3��  Ϊ  R G B ÿ��ѭ��ɾ�� һ�������� xx ��
        p = (byte*)(bitData.Scan0.ToPointer());

        w = (uint)bitData.Width;
        h = (uint)bitData.Height;

        //һ�е����ص�
        iWidth = w * 3 + ic;

        //int icColor = (iAvgGrey - iAvgGreyBu);
        //MessageBox.Show(icColor + "");

        bool isBu = false;

        for (int i = 0; i < arrRect.Count; i++)
        {
            Rect rect = (Rect)arrRect[i];
            //rect.calculate2();

            //AciDebug.Debug(f.Name + "----------------���ߣ�" + rect.iNumTie + "  �ܵ�����" + rect.iNum  + " ʵ�ĵ㣺" + rect.iNumShi + " ���߱ȣ�" + rect.iTie_per + "  ����ȣ�" + rect.iWH_per + " �ܶȱȣ�" + rect.P);

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
    /// ����ڿ��и�
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
            //��������ȥ�ڱ�
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
            //��������ȥ�ڱ�
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
            //��������ȥ�ڱ�
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
            //��������ȥ�ڱ�
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
    /// ��ƽ���Ҷ�
    /// </summary>
    /// <param name="p"></param>
    /// <param name="Ostu">�������ֵ��ƽ���Ҷ�</param>
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
                //��ʼ��
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
    /// ���ڱ��
    /// </summary>
    uint[,] arrRect_P = null;
    /// <summary>
    /// ��ͨ����ָ��㷨 ���ĸ���
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
        //��ͨ׶
        ArrayList Al_sp = null;

        if (r == 0 && arrRect_P[x, y] == 0)
        {
            //������׶
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
                //ȡ��һ�������8����
                point = (myDot)Al_sp[0];
                for (int irow = -1; irow <= 1; irow++)
                {
                    for (int icell = -1; icell <= 1; icell++)
                    {
                        uint thisx = (uint)((int)point.x + icell);
                        uint thisy = (uint)((int)point.y + irow);
                        //�������߽�
                        //if (thisx >= rectWH.l && thisx < rectWH.r && thisy >= rectWH.t && thisy < rectWH.b)
                        if (thisx >= rectFrame.X && thisx < rectFrame.Width && thisy >= rectFrame.Y && thisy < rectFrame.Height)
                        {
                            dot = p[thisy * iWidth + thisx * 3];

                            if (dot == 0 && arrRect_P[thisx, thisy] == 0)
                            {
                                //�µ���׶
                                arrRect_P[thisx, thisy] = 1;
                                myDot dott = new myDot(thisx, thisy);
                                //if (thisx == rectFrame.X || thisx == rectFrame.Width - 1 || thisy == rectFrame.Y || thisy == rectFrame.Height - 1)
                                //{
                                //    //�ж����ߵ�
                                //    dott.bTie = true;
                                //}

                                //if (getTopDot(p, thisx, thisy) == 0 && getBottomDot(p, thisx, thisy) == 0 && getLeftDot(p, thisx, thisy) == 0 && getRightDot(p, thisx, thisy) == 0)
                                //{
                                //    //�ж�ʵ��
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
                                //�������þ��δ�С
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
                //��׶
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

        //��ͨ׶
        ArrayList Al_sp = null;

        if (r == 0)
        {
            //������׶
            Al_sp = new ArrayList();
            //arrP[x, y] = 1;
            Al_sp.Add(new myDot(x, y));

            while (Al_sp.Count != 0)
            {
                //ȡ��һ�������8����
                point = (myDot)Al_sp[0];

                uint thisx = (uint)((int)point.x + to_x);
                uint thisy = (uint)((int)point.y + to_y);
                //�������߽�
                if (thisx >= rectFrame.X && thisx < rectFrame.Width && thisy >= rectFrame.Y && thisy < rectFrame.Height)
                {
                    dot = p[thisy * iWidth + thisx * 3];
                    if (dot == 0)
                    {
                        //�µ���׶
                        myDot dott = new myDot(thisx, thisy);
                        Al_sp.Add(dott);
                        rect.Add(dott);
                        //�������þ��δ�С
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
                //��׶
                Al_sp.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// ��ͨ����ָ��㷨 (���⣺����������ߣ�
    /// </summary>
    public unsafe ArrayList cutArea(byte* p, FileInfo file)
    {
        ArrayList arrRect = new ArrayList();
        Rect rect = null;
        myDot point;
        uint dot = 0;

        //��ͨ׶
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
                        //������׶
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
                            //ȡ��һ�������8����
                            point = (myDot)Al_sp[0];
                            for (int irow = -1; irow <= 1; irow++)
                            {
                                for (int icell = -1; icell <= 1; icell++)
                                {
                                    uint thisx = (uint)((int)point.x + icell);
                                    uint thisy = (uint)((int)point.y + irow);
                                    //�������߽�
                                    if (thisx >= 0 && thisx < w && thisy >= 0 && thisy < h)
                                    {
                                        dot = p[thisy * iWidth + thisx * 3];

                                        if (dot == 0 && arrRect_P[thisx, thisy] == 0)
                                        {
                                            //�µ���׶
                                            arrRect_P[thisx, thisy] = 1;
                                            myDot dott = new myDot(thisx, thisy);
                                            //if (thisx == 0 || thisx == w - 1 || thisy == 0 || thisy == h - 1)
                                            //{
                                            //    //�ж����ߵ�
                                            //    dott.bTie = true;
                                            //}

                                            //if (getTopDot(p, thisx, thisy) == 0 && getBottomDot(p, thisx, thisy) == 0 && getLeftDot(p, thisx, thisy) == 0 && getRightDot(p, thisx, thisy) == 0)
                                            //{
                                            //    //�ж�ʵ��
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
                                            //�������þ��δ�С
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
                            //��׶
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
    /// ��ֵ��
    /// </summary>
    /// <returns></returns>
    public unsafe void toTwo(byte* p, uint Ostu)
    {
        for (uint y = 0; y < h; y++)
        {
            for (uint x = 0; x < w; x++)
            {
                //��ʼ��
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
    /// �����graphics�쳣��PixelFormat
    /// </summary>
    private static PixelFormat[] indexedPixelFormats = { PixelFormat.Undefined, PixelFormat.DontCare, PixelFormat.Format16bppArgb1555, PixelFormat.Format1bppIndexed, PixelFormat.Format4bppIndexed, PixelFormat.Format8bppIndexed };

    /// <summary>
    /// �ж�ͼƬ��PixelFormat �Ƿ��� �����쳣�� PixelFormat ֮��
    /// </summary>
    /// <param name="imgPixelFormat">ԭͼƬ��PixelFormat</param>
    /// <returns></returns>
    private static bool IsPixelFormatIndexed(PixelFormat imgPixelFormat)
    {
        foreach (PixelFormat pf in indexedPixelFormats)
        {
            if (pf.Equals(imgPixelFormat)) return true;
        }

        return false;
    }
    //��ȡImageCodecInfo
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

        //ͼƬ����
        quality = new long[] { 50 };
        EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
        encoderParams.Param[0] = encoderParam;

        myImageCodecInfo = GetEncoderInfo("image/jpeg");
    }




    /// <summary>
    /// ֱ��ͼ
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
                //��ʼ��
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

        //flash�鿴ֱ��ͼ
        //createtxt(pixelNum);

        return new Histogram(pixelNum, maxK);
    }
    /// <summary> 
    /// ��������
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
    /// ��������䷽�� �ο� ��䷽�� http://baike.baidu.com/view/1650357.htm
    /// </summary>
    /// <param name="pixelNum"></param>
    /// <returns></returns>
    public uint getOstu(int[] pixelNum)
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
    /// <summary>
    /// ֱ��ͼ�ṹ��
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

using System;
using System.Collections;
using System.Text;
using System.Data;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

class ImageFindTableRotate
{
    /// <summary>�������ݷ�ֵ</summary>
    public myKey Key = new myKey();

    uint ic = 0;

    FileInfo myfile;

    public bool test = false;

    TimeSpan ts;
    DateTime dt;

    int iOverIndex = 1104;

    double iP = 0;
    double iAll = 0;

    uint[,] intCheck = null;

    uint w = 0;
    uint h = 0;

    //һ�е����ص�
    uint iWidth = 0;
    uint r = 0;
    uint g = 0;
    uint b = 0;
    /// <summary>
    /// �Ҷȼ������Ч��Χ Ĭ��Ϊ 80
    /// </summary>
    public uint greyArea = 40;
    public bool show = false;
    Bitmap bitmap = null;
    FileInfo fileInfo = null;

    System.Drawing.Imaging.ImageCodecInfo myImageCodecInfo;
    EncoderParameters encoderParams;
    public bool b�Ƿ���ͼ = false;
    public bool b�Ƿ���ת��ü� = false;

    public uint ishang = 0;
    public uint istu = 0;

    public LineHV lineH;
    public LineHV lineV;

    /// <summary>�Զ����ĵ�ɫ</summary>
    public cuteHeiBian.dotRGB iColorBu = new cuteHeiBian.dotRGB(255, 255, 255);

    public ImageFindTableRotate()
    {
        encoderParams = new EncoderParameters();
        long[] quality;


        //ͼƬ����
        quality = new long[] { 100 };

        EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
        encoderParams.Param[0] = encoderParam;

        myImageCodecInfo = GetEncoderInfo("image/jpeg");
    }
    public Bitmap setImage(FileInfo file)
    {
        Bitmap img = (Bitmap)System.Drawing.Image.FromFile(file.FullName);
        return setImage(ref img, file);
    }
    /// <summary>
    /// ���ԵĿ�߰ٷֱ�
    /// </summary>
    public float iKeyPerWH = 0f;
    public float iRotate = 0;
    public unsafe float getIK(ref Bitmap bmp, FileInfo file, bool is90)
    {
        fileInfo = file;
        //��������
        bitmap = new Bitmap(Key.width, (int)(Key.width / (double)bmp.Width * (double)bmp.Height));
        iKeyPerWH = (float)Key.width / (float)bmp.Width;

        Graphics g = Graphics.FromImage(bitmap);
        g.DrawImage(bmp, 0, 0, bitmap.Width, bitmap.Height);
        g.Dispose();
        //bitmap.Save(file.FullName.Replace(".jpg", "_��ʱ.jpg"), myImageCodecInfo, encoderParams);
        //bitmap.Dispose();

        //string file_img_0 = file.FullName.Replace(".jpg", "_��ʱ.jpg");

        //bitmap = (Bitmap)System.Drawing.Image.FromFile(file_img_0);          

        BitmapData bitData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        //ȡ��ÿ�ж��������
        ic = (uint)(bitData.Stride - bitData.Width * 3);

        iP = 0;
        iAll = 0;

        //ȡ��ȫ������ֵ ÿ�� 3��  Ϊ  R G B ÿ��ѭ��ɾ�� һ�������� xx ��
        byte* p = (byte*)(bitData.Scan0.ToPointer());

        w = (uint)bitData.Width;
        h = (uint)bitData.Height;

        //һ�е����ص�
        iWidth = w * 3 + ic;


        //�ҶȻ�- ��ȥ����ɫ
        //toGrey(p);

        if (show)
        {
            bitmap.UnlockBits(bitData);
            bitmap.Save(file.FullName.Replace(".jpg", "_��ʱ_a�ҶȻ�.jpg"), myImageCodecInfo, encoderParams);
            bitData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        }

        //ֱ��ͼ
        int[] pixelNum = new int[256];
        uint[,] arrRect_P = new uint[w, h];

        uint iR = 0;
        uint iG = 0;
        uint iB = 0;
        //uint iR_all = 0;
        //uint iG_all = 0;
        //uint iB_all = 0;
        //uint iC = 0;
        for (uint y = 0; y < h; y++)
        {
            for (uint x = 0; x < w; x++)
            {
                //��ʼ��
                uint ib = y * iWidth + x * 3;
                

                if (y > greyArea && y < h - greyArea && x > greyArea && x < w - greyArea)
                {
                    iB = p[ib];
                    iG = p[ib + 1];
                    iR = p[ib + 2];

                    //if (iB != 255 || iG != 255 || iR != 255)
                    //{
                    //    AciDebug.Debug(x + " " + y + " " + iB + " " + iG + " " + iR);
                    //}
                    uint igrey = (iB + iG + iR) / 3;
                    pixelNum[igrey]++;

                    //iR_all += iR;
                    //iG_all += iG;
                    //iB_all += iB;
                    //iC++;

                    p[ib] = (byte)igrey;
                    p[ib + 1] = (byte)igrey;
                    p[ib + 2] = (byte)igrey;
                }
                else
                {
                    p[ib] = (byte)255;
                    p[ib + 1] = (byte)255;
                    p[ib + 2] = (byte)255;
                }
            }
        }

        //iB = iR_all / iC;
        //iG = iG_all / iC;
        //iR = iB_all / iC;
        //uint iGrey = (iB + iG + iR) / 3;

        //b�Ƿ���ͼ = false;
        //if (iGrey < 230)
        //{
        //    if (((float)iR / (float)iG > 1.2f || (float)iR / (float)iB > 1.2f)
        //    || ((float)iG / (float)iR > 1.2f || (float)iG / (float)iB > 1.2f)
        //    || ((float)iB / (float)iR > 1.2f || (float)iB / (float)iG > 1.2f))
        //    {
        //        //MessageBox.Show(iB + " " + iG + " " + iR);
        //        b�Ƿ���ͼ = true;
        //    }
        //}

        //ishang = getShang(pixelNum);
        uint ikittler = getKittler(pixelNum);

        uint ik = ikittler;
        //if (istu < ishang)
        //{
        //    ik = ishang;
        //}
        //if (!b�Ƿ���ͼ)
        //{
        //}
        //MessageBox.Show(ik + "");
        ////��ʼ��ֵ��
        if (ik > 240)
        {
            ik = 240;
        }
        float fPP = toTwo(p, ik);
        if (fPP > 0.2f)
        {
            //�ܺڵĴ���ͼ��
            iRotate = 0;
            return -1;
        }

        if (show)
        {
            bitmap.UnlockBits(bitData);
            bitmap.Save(file.FullName.Replace(".jpg", "_��ʱ_b��ֵ��.jpg"), myImageCodecInfo, encoderParams);
            bitData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        }

        ////ȥ������
        toClear(p);

       
        if (is90)
        {
            //ֱ���ж�90��
            iRotate = cutArea(p, bitData);
            bitmap.UnlockBits(bitData);
            return iRotate;
        }
        else
        {
            iRotate = 0;
            bitmap.UnlockBits(bitData);
            return 0;
        }
    }



    public unsafe Bitmap setImage(ref Bitmap bmp, FileInfo file)
    {
        fileInfo = file;
        //AciDebug.Debug("aa" + DateTime.Now.Second + " " + DateTime.Now.Millisecond);
        float irotate = getIK(ref bmp, file, false);
        //AciDebug.Debug("bb" + DateTime.Now.Second + " " + DateTime.Now.Millisecond);
        ///////////////////////////////////////////////////////////////////////
        //�������� Ѱ��ֱ��
        //////////////////////////////////////////////////////////////////////
        if (irotate == -1)
        {
            //�ܺڵĴ���ͼ��
            bitmap.Dispose();
            al_hough_X = new ArrayList();
            al_hough_Y = new ArrayList();
            return bmp;
        }


        iRotate = getLine(file);

        ///////////////////////////////////////////////////////////////////////
        //�������� Ѱ��ֱ��
        //////////////////////////////////////////////////////////////////////

        ////��ͨ����
        //if (iRotate == 0)
        //{
        //    iRotate = irotate;
        //}
        //AciDebug.Debug("cc" + DateTime.Now.Second + " " + DateTime.Now.Millisecond);
        //myTable.maxRect.calculate();
        //myTable.maxRect.P = Math.Round(myTable.maxRect.P * 1000);

        if (show)
        {
            bitmap.Save(file.FullName.Replace(".jpg", "_��ʱ_cȥС�����.jpg"), myImageCodecInfo, encoderParams);
        }

        bitmap.Dispose();
        //MessageBox.Show(iRotate + "");
        if (show) MessageBox.Show(iRotate + "");
        if (Math.Abs(iRotate) >= 1) RotateImg(ref bmp, file, iRotate);

        //for (int i = 0; i < al_hough_X.Count; i++)
        //{
        //    HoughClass hough = (HoughClass)al_hough_X[i];
        //    AciDebug.Debug(hough.iNum + "");
        //}
        //for (int i = 0; i < al_hough_Y.Count; i++)
        //{
        //    HoughClass hough = (HoughClass)al_hough_Y[i];
        //    AciDebug.Debug(hough.iNum + "");
        //}

        //AciDebug.Debug("dd" + DateTime.Now.Second + " " + DateTime.Now.Millisecond);
        return bmp;
    }
    /// <summary>
    /// ��ת����е��� x y 
    /// </summary>
    public Point pointCutXY;
    public string savejpg = "";
    public unsafe void RotateImg(ref Bitmap bmp, FileInfo file, float R)
    {
        BitmapData bitData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        pointCutXY = new Point();

        //ȡ��ÿ�ж��������
        ic = (uint)(bitData.Stride - bitData.Width * 3);

        iP = 0;
        iAll = 0;

        //ȡ��ȫ������ֵ ÿ�� 3��  Ϊ  R G B ÿ��ѭ��ɾ�� һ�������� xx ��
        byte* p = (byte*)(bitData.Scan0.ToPointer());

        w = (uint)bitData.Width;
        h = (uint)bitData.Height;

        //һ�е����ص�
        iWidth = w * 3 + ic;


        //��תͼ��
        if (R != 0)
        {
            myDot mydot;
            byte[,] arrR = new byte[w, h];
            byte[,] arrG = new byte[w, h];
            byte[,] arrB = new byte[w, h];
            byte[,] arrP = new byte[w, h];

            //�ı��и���������
            uint icut = 0;

            Point center = new Point((int)w / 2, (int)h / 2);
            //R = K / (float)Math.PI * 180;
            float myK = -(float)(R / 180 * Math.PI);
            for (uint y = 0; y < h; y++)
            {
                for (uint x = 0; x < w; x++)
                {
                    mydot = new myDot(x, y, iWidth, 0);
                    if (x <= icut || x >= w - icut || y <= icut || y >= h - icut)
                    {
                        p[mydot.ib] = p[mydot.ib + 1] = p[mydot.ib + 2] = 255;
                    }
                    else
                    {
                        byte r = p[mydot.ib];
                        byte g = p[mydot.ib + 1];
                        byte b = p[mydot.ib + 2];

                        uint myX = 0;
                        uint myY = 0;

                        myX = (uint)(center.X + (x - center.X) * Math.Cos(myK) - (y - center.Y) * Math.Sin(myK));
                        myY = (uint)(center.Y + (x - center.X) * Math.Sin(myK) + (y - center.Y) * Math.Cos(myK));

                        if (myX >= 0 && myX < w && myY >= 0 && myY < h)
                        {
                            arrR[myX, myY] = r;
                            arrG[myX, myY] = g;
                            arrB[myX, myY] = b;
                            arrP[myX, myY] = 1;
                        }
                    }
                }
            }
            for (uint y = 0; y < h; y++)
            {
                for (uint x = 0; x < w; x++)
                {
                    mydot = new myDot(x, y, iWidth, 0);
                    if (arrP[x, y] == 1)
                    {
                        p[mydot.ib] = arrR[x, y];
                        p[mydot.ib + 1] = arrG[x, y];
                        p[mydot.ib + 2] = arrB[x, y];
                    }
                    else
                    {
                        p[mydot.ib] = iColorBu.B;
                        p[mydot.ib + 1] = iColorBu.G;
                        p[mydot.ib + 2] = iColorBu.R;
                    }
                }
            }
        }
        bmp.UnlockBits(bitData);

        if (show) bmp.Save(file.FullName.Replace(".jpg", "_��ʱ_d��ת��.jpg"), myImageCodecInfo, encoderParams);

        if (b�Ƿ���ת��ü�)
        {
            float pi = 0;
            pi = (float)(Math.PI / 180) * ((float)Math.Abs(R));
            
            float ixx = (float)Math.Tan(pi) * h / 2;
            float iyy = (float)Math.Tan(pi) * w / 2;
            //MessageBox.Show(ixx + " " + iyy);
            int imoveW = (int)Math.Floor(ixx);
            int imoveH = (int)Math.Floor(iyy);
            //MessageBox.Show(imoveH + " " + imoveW + " " + R);

            Bitmap bmpsave = new Bitmap(bmp.Width - imoveW * 2, bmp.Height - imoveH * 2);
            Graphics g = Graphics.FromImage(bmpsave);
            g.Clear(Color.FromArgb(iColorBu.B, iColorBu.G, iColorBu.B));
            //MessageBox.Show(Color.FromArgb(iColorBu, iColorBu, iColorBu) + "");
            pointCutXY.X = imoveW;
            pointCutXY.Y = imoveH;
            g.DrawImage(bmp, -imoveW, -imoveH, bmp.Width, bmp.Height);
            g.Dispose();
            bmp.Dispose();

            bmp = bmpsave;
        }

        if (savejpg != "")
        {
            bmp.Save(savejpg + "/" + file.Name + "", myImageCodecInfo, encoderParams);
            bmp.Dispose();
        }
    }
    public int bmpSmallW = 300;
    public unsafe float getLine(FileInfo file)
    {
        Bitmap bmp = new Bitmap(bmpSmallW, (int)(bmpSmallW / (double)bitmap.Width * (double)bitmap.Height));
        Graphics g = Graphics.FromImage(bmp);

        g.DrawImage(bitmap, 0, 0, bmp.Width, bmp.Height);

        g.Dispose();
        //bmp.Save(file.FullName.Replace(".jpg", "_��ʱ_ֱ��.jpg"), myImageCodecInfo, encoderParams);
        //bmp.Dispose();

        //string file_img_0 = file.FullName.Replace(".jpg", "_��ʱ_ֱ��.jpg");

        //bmp = (Bitmap)System.Drawing.Image.FromFile(file_img_0);
        BitmapData bitData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        //ȡ��ÿ�ж��������
        ic = (uint)(bitData.Stride - bitData.Width * 3);
        byte* p = (byte*)(bitData.Scan0.ToPointer());

        w = (uint)bitData.Width;
        h = (uint)bitData.Height;

        //һ�е����ص�
        iWidth = w * 3 + ic;


        toTwo(p, 200);
        //bmp.Save(file.FullName.Replace(".jpg", "_��ʱ_������1.jpg"), myImageCodecInfo, encoderParams);

        toSmallLine(p, bmp, file, -1);
        if (show)
        {
            bmp.UnlockBits(bitData);
            bmp.Save(file.FullName.Replace(".jpg", "_��ʱ_������2.jpg"), myImageCodecInfo, encoderParams);
            bitData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        }


        float fR = houghR(p);

        bmp.UnlockBits(bitData);
        bmp.Dispose();

        //File.Delete(file_img_0);

        return fR;
    }
    public class HoughClass
    {
        public string str = "";
        public double k = 0;
        public double b = 0;
        public Point pointB;
        public Point pointE;
        public int iNum = 0;

        public Point pointB_byR = new Point();

        public HoughClass(){ }
        /// <summary>
        /// ���ݽǶȼ���ת����� pointB
        /// </summary>
        /// <param name="center">���ĵ�</param>
        /// <param name="ik">�ǶȵĻ���ֵ</param>
        public void setPointB_by_R(Point center, float ik)
        {
            //float myK = -(float)(R / 180 * Math.PI);
            pointB_byR.X = (int)(center.X + (pointB.X - center.X) * Math.Cos(ik) - (pointB.Y - center.Y) * Math.Sin(ik));
            pointB_byR.Y = (int)(center.Y + (pointB.X - center.X) * Math.Sin(ik) + (pointB.Y - center.Y) * Math.Cos(ik));

        }
    }
    public double irMax = 10f;
    public double irStep = 0.1f;
    public ArrayList al_hough_X;
    public ArrayList al_hough_Y;

    unsafe float houghR(byte* p)
    {
        irMax = 10f;
        irStep = 0.1f;
        int rho_max = (int)Math.Floor(Math.Sqrt(w * w + h * h)) + 1; //��ԭͼ����������������ֵ����ȡ�������ּ�1
        //��ֵ��Ϊ�ѣ�������ϵ�����ֵ
        int[,] accarray = new int[rho_max, 1800]; //����ѣ�������ϵ�����飬��ֵΪ0��
        string[,] accarray_xy = new string[rho_max, 1800]; //����ѣ�������ϵ�����飬��ֵΪ0��
        //�ȵ����ֵ��180��

        double[] Theta = new double[1800];
        //��������飬ȷ����ȡֵ��Χ
        for (double k = 0; k < irMax * 10; k += irStep * 10)
        {
            Theta[(int)k] = Math.PI / 180 * k / 10;
        }
        for (double k = (180 - irMax) * 10; k < 180 * 10; k += irStep * 10)
        {
            Theta[(int)k] = Math.PI / 180 * k / 10;
        }
        for (double k = (90 - irMax) * 10; k < (90 + irMax) * 10; k += irStep * 10)
        {
            Theta[(int)k] = Math.PI / 180 * k / 10;
        }

        ArrayList al_X = new ArrayList();
        al_hough_X = new ArrayList();

        ArrayList al_Y = new ArrayList();
        al_hough_Y = new ArrayList();
        int icutNum_X = (int)w * 2 / 3;
        int icutNum_Y = (int)h / 2;
        int icutNum_Y2 = (int)h * 2 / 3;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int ib = y * (int)iWidth + x * 3;
                int iB = (int)p[ib];

                if (iB == 0)
                {
                    for (double k = 0; k < irMax * 10; k += irStep * 10)
                    {
                        setHoughClass(icutNum_X, x, y, ref accarray, ref accarray_xy, ref al_X, ref al_hough_X, k, rho_max, Theta);
                    }
                    for (double k = (180 - irMax) * 10; k < 180 * 10; k += irStep * 10)
                    {
                        setHoughClass(icutNum_X, x, y, ref accarray, ref accarray_xy, ref al_X, ref al_hough_X, k, rho_max, Theta);
                    }
                    for (double k = (90 - irMax) * 10; k < (90 + irMax) * 10; k += irStep * 10)
                    {
                        setHoughClass(icutNum_Y, x, y, ref accarray, ref accarray_xy, ref al_Y, ref al_hough_Y, k, rho_max, Theta);
                    }
                }
            }
        }
        float iR_X = 0;
        if (al_X.Count > 1)
        {
            float iR_all_X = 0;
            float iR_count_X = 0;
            //ArrayList al_k = new ArrayList();
            for (int i = 0; i < al_X.Count; i++)
            {
                HoughClass hough = (HoughClass)al_hough_X[i];
                float inum = hough.iNum - icutNum_X;

                if (hough.k <= irMax)
                {
                    iR_all_X += (float)hough.k * inum;
                }
                else
                {
                    iR_all_X += (float)(hough.k - 180) * inum;
                }
                //al_k.Add((float)hough.k + " " + inum);
                iR_count_X += inum;
            }
            if (iR_count_X > 0)
            {
                iR_X = iR_all_X / iR_count_X;
            }

            //for (int i = 0; i < al_X.Count; i++)
            //{
            //    HoughClass hough = (HoughClass)al_hough_X[i];
            //    AciDebug.Debug(hough.str + "  " + hough.k + "  " + (hough.iNum - icutNum_X) + "   " + hough.pointB.ToString());
            //}
        }

        float iR_Y = 0;
        if (al_Y.Count > 1)
        {
            float iR_all_Y = 0;
            float iR_count_Y = 0;
            //ArrayList al_k_Y = new ArrayList();
            for (int i = 0; i < al_Y.Count; i++)
            {
                HoughClass hough = (HoughClass)al_hough_Y[i];
                if (hough.iNum >= icutNum_Y2)
                {
                    float inum = hough.iNum - icutNum_Y2;
                    iR_all_Y += (float)(hough.k - 90) * inum;
                    //al_k_Y.Add((float)hough.k + " " + inum);
                    iR_count_Y += inum;
                }
            }
            if (iR_count_Y > 0)
            {
                iR_Y = iR_all_Y / iR_count_Y;
            }
        }
        //���ҵ�һ��ֱ��
        if (al_Y.Count > 0)
        {
            al_hough_Y.Sort(new myAL_HoughPointX_Sorter());

            //for (int i = 0; i < al_Y.Count; i++)
            //{
            //    HoughClass hough = (HoughClass)al_hough_Y[i];
            //    AciDebug.Debug(hough.str + "  " + hough.k + "  " + (hough.iNum - icutNum_Y) + "   " + hough.pointB.ToString());
            //}
        }

        float ir = 0;

        if (al_X.Count + al_Y.Count > 0)
        {
            //ir = -((float)iR_X * al_X.Count + (float)iR_Y * al_Y.Count) / (al_X.Count + al_Y.Count);
            if (al_X.Count == 0)
            {
                ir = -(float)iR_Y;
            }
            else if (al_Y.Count == 0)
            {
                ir = -(float)iR_X;
            }
            else
            {
                ir = -((float)iR_X + (float)iR_Y) / 2;
            }
        }

        if (al_Y.Count > 0)
        {
            //������ת�Ƕ� �������� ��ʵ�� PointB_byR �����ݴ���������al_hough_Y
            float ik = -(float)(ir / 180 * Math.PI);
            Point center = new Point((int)w / 2, (int)h / 2);
            for (int i = 0; i < al_hough_Y.Count; i++)
            {
                HoughClass hough = (HoughClass)al_hough_Y[i];
                hough.setPointB_by_R(center, ik);
            }

            al_hough_Y.Sort(new myAL_HoughPointB_by_R_Sorter());
        }

        return ir;
    }

    void setHoughClass(int iCutNum, int x, int y, ref int[,] accarray, ref string[,] accarray_xy, ref ArrayList al, ref ArrayList al_hough, double k, int rho_max, double[] Theta)
    {
        int ik = (int)k;
        //����ֵ����hough�任���̣����ֵ
        double rho = (y * Math.Cos(Theta[ik])) + (x * Math.Sin(Theta[ik]));
        //����ֵ������ֵ�ĺ͵�һ����Ϊ�ѵ�����ֵ���������꣩����������Ϊ�˷�ֹ��ֵ���ָ���
        int rho_int = (int)Math.Round(rho / 2 + rho_max / 2);
        //�ڦѦ����꣨���飩�б�ʶ�㣬�������ۼ�
        accarray[rho_int, ik]++;
        string sXY = accarray_xy[rho_int, ik];
        if (sXY == null)
        {
            accarray_xy[rho_int, ik] = x + "_" + y;
        }
        int inum = accarray[rho_int, ik];
        if (inum >= iCutNum)
        {
            string str = rho_int + " " + ik;
            int it = al.IndexOf(str);
            if (it < 0)
            {
                al.Add(str);
                HoughClass hough = new HoughClass();
                hough.str = str;
                al_hough.Add(hough);

                hough.k = (double)ik / 10;
                string[] arr = sXY.Split('_');
                int xx = AciCvt.ToInt(arr[0]);
                int yy = AciCvt.ToInt(arr[1]);
                hough.pointB = new Point(xx, yy);
                hough.pointE = new Point(xx, yy);
                //y=kx+b   b=y-kx
                double b = 0;
                if (hough.k != 90)
                {
                    b = (double)y - hough.k * (double)x;
                }
                else
                {
                    b = (double)x;
                }
                hough.b = b;
                hough.iNum = inum;
            }
            else
            {
                HoughClass hough = (HoughClass)al_hough[it];
                hough.iNum = inum;
                hough.pointE = new Point(x, y);
            }
        }
    }

    /// <summary>
    /// ��Hough������������
    /// </summary>
    public class myAL_HoughPointX_Sorter : IComparer
    {
        public int Compare(object x, object y)
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
            HoughClass xInfo = (HoughClass)x;
            HoughClass yInfo = (HoughClass)y;


            //�����Q����    
            return xInfo.pointB.X.CompareTo(yInfo.pointB.X);//�f��    
            //return yInfo.FullName.CompareTo(xInfo.FullName);//�f�p   
        }
    }
    /// <summary>
    /// ��Hough PointB_by_R ������������
    /// </summary>
    public class myAL_HoughPointB_by_R_Sorter : IComparer
    {
        public int Compare(object x, object y)
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
            HoughClass xInfo = (HoughClass)x;
            HoughClass yInfo = (HoughClass)y;


            //�����Q����    
            return xInfo.pointB_byR.X.CompareTo(yInfo.pointB_byR.X);//�f��    
            //return yInfo.FullName.CompareTo(xInfo.FullName);//�f�p   
        }
    }

    public unsafe void debug_Rect(byte* p, ArrayList al, string imgMsg, BitmapData bitData)
    {
        ArrayList al_dotRect = new ArrayList();
        for (int i = 0; i < al.Count; i++)
        {
            Rect rect = (Rect)al[i];
            al_dotRect.Add(rect.Draw(p, iWidth));
        }
        bitmap.UnlockBits(bitData);
        bitmap.Save(fileInfo.FullName.Replace(".jpg", "_��ʱ_" + imgMsg + ".jpg"), myImageCodecInfo, encoderParams);
        bitData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        //�ָ�
        for (int i = 0; i < al_dotRect.Count; i++)
        {
            ArrayList al_dot = (ArrayList)al_dotRect[i];
            for (int j = 0; j < al_dot.Count; j++)
            {
                myDot dot = (myDot)al_dot[j];
                dot.Draw(p);
            }
        }
    }

    public unsafe void debug_Line(byte* p, ArrayList al, string imgMsg, BitmapData bitData)
    {
        ArrayList al_dotLine = new ArrayList();
        ArrayList al_dotRect = new ArrayList();
        for (int i = 0; i < al.Count; i++)
        {
            LineRow line = (LineRow)al[i];

            for (int j = 0; j < line.AL_Rect.Count; j++)
            {
                Rect rect = (Rect)line.AL_Rect[j];
                al_dotRect.Add(rect.Draw(p, iWidth));
            }

            al_dotLine.Add(line.Draw(p, w, h, iWidth, false, 0, 0, 255, 1f));
        }

        bitmap.UnlockBits(bitData);
        bitmap.Save(fileInfo.FullName.Replace(".jpg", "_��ʱ_" + imgMsg + ".jpg"), myImageCodecInfo, encoderParams);
        bitData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);


        //�ָ�
        for (int i = 0; i < al_dotLine.Count; i++)
        {
            ArrayList al_dot = (ArrayList)al_dotLine[i];
            for (int j = 0; j < al_dot.Count; j++)
            {
                myDot dot = (myDot)al_dot[j];
                dot.Draw(p);
            }
        }
        for (int i = 0; i < al_dotRect.Count; i++)
        {
            ArrayList al_dot = (ArrayList)al_dotRect[i];
            for (int j = 0; j < al_dot.Count; j++)
            {
                myDot dot = (myDot)al_dot[j];
                dot.Draw(p);
            }
        }
    }
    public int isRotate90 = 0;
    /// <summary>
    /// �ܶ��ޱȸ߶� ���ɫ��
    /// </summary>
    public ArrayList AL_rectP_Large = new ArrayList();
    /// <summary>
    /// ��ͨ����ָ��㷨
    /// </summary>
    public unsafe float cutArea(byte* p, BitmapData bitData)
    {
        Rect rect;
        Dot point;
        uint dot = 0;
        //���
        uint[,] arrP = new uint[w, h];
        //��ͨ׶
        ArrayList Al_sp = null;
        ArrayList Al_rect = new ArrayList();
        AL_rectP_Large = new ArrayList();
        Rect maxRect = new Rect();

        for (uint y = 0; y < h; y++)
        {
            for (uint x = 0; x < w; x++)
            {
                uint ib = y * iWidth + x * 3;
                r = p[ib];
                if (r == 0 && arrP[x, y] == 0)
                {
                    //������׶
                    Al_sp = new ArrayList();

                    arrP[x, y] = 1;
                    Al_sp.Add(new Dot(x, y));

                    rect = new Rect(y, x + 1, y + 1, x);
                    rect.Add(new myDot(x, y, iWidth, 0));

                    while (Al_sp.Count != 0)
                    {
                        //ȡ��һ�������4����
                        point = (Dot)Al_sp[0];
                        for (int ci = 0; ci < 4; ci++)
                        {
                            uint thisx = point.x;
                            uint thisy = point.y;
                            switch (ci)
                            {
                                case 0:
                                    thisy -= 1;
                                    break;
                                case 1:
                                    thisx -= 1;
                                    break;
                                case 2:
                                    thisx += 1;
                                    break;
                                case 3:
                                    thisy += 1;
                                    break;
                            }
                            //�������߽�
                            if (thisx >= 0 && thisx < w && thisy >= 0 && thisy < h)
                            {
                                dot = p[thisy * iWidth + thisx * 3];

                                if (dot == 0 && arrP[thisx, thisy] == 0)
                                {
                                    //�µ���׶
                                    arrP[thisx, thisy] = 1;
                                    Al_sp.Add(new Dot(thisx, thisy));
                                    rect.Add(new myDot(thisx, thisy, iWidth, 0));
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
                        //��׶
                        Al_sp.RemoveAt(0);
                    }
                    ////ȥ�� �����1/8���ϵġ� �������0.3���µĽϴ����� �����30���µġ�
                    bool isok = true;
                    rect.calculate();
                    if (rect.S > w * h / 8
                        || (rect.pp < 0.3 && (rect.width > w / 10 || rect.height > h / 10))
                        || rect.S < 30)
                    {
                        //�������һ���ֱ��ȥ��
                        isok = false;
                    }
                    if (rect.P >= 0.5 && (rect.width > w / 10 || rect.height > h / 10))
                    {
                        AL_rectP_Large.Add(rect);
                    }
                    if (isok) Al_rect.Add(rect);
                }
            }
        }

        ////ȥ�� �����1/8���ϵġ� �������0.3���µĽϴ����� �����30���µġ�
        //ArrayList al_move = new ArrayList();
        //for (int i = 0; i < Al_rect.Count; i++)
        //{
        //    rect = (Rect)Al_rect[i];
        //    rect.calculate();
        //    if (rect.S > w * h / 8
        //        || (rect.pp < 0.3 && (rect.width > w / 10 || rect.height > h / 10))
        //        || rect.S < 30)
        //    {
        //        //�������һ���ֱ��ȥ��
        //        al_move.Add(rect);
        //    }
        //}
        //for (int i = 0; i < al_move.Count; i++)
        //{
        //    rect = (Rect)al_move[i];
        //    Al_rect.Remove(rect);
        //}
       
        if (test) debug_Rect(p, Al_rect, "��01��ȥ��������������", bitData);
        //AciDebug.Debug(Al_rect.Count + " " + w + " " + h + "������������");
        //ȥ�����ϲ� ���ཻ������
        int index = 0;
        ArrayList al_move = new ArrayList();
        while (index < Al_rect.Count)
        {
            Rect rectA = (Rect)Al_rect[index];
            if (al_move.IndexOf(rectA) > -1)
            {
                index++;
                continue;
            }

            for (int j = 0; j < Al_rect.Count; j++)
            {
                Rect rectB = (Rect)Al_rect[j];
                if (al_move.IndexOf(rectB) > -1)
                {
                    continue;
                }

                if (rectA != rectB)
                {
                    if (Rect.RectA_vs_RectB(rectA, rectB))
                    {
                        //�����ཻ �ϲ� ��ȥ��
                        rectA.AddRect(rectB);
                        al_move.Add(rectB);
                        rectA.calculate();
                        break;
                    }
                }
            }
            index++;
        }
        for (int i = 0; i < al_move.Count; i++)
        {
            rect = (Rect)al_move[i];
            Al_rect.Remove(rect);
        }

        if (test) debug_Rect(p, Al_rect, "��02��ȥ�����ϲ��ཻ������", bitData);

        lineH = new LineHV();
        ArrayList AL_LineH = getLineH(p, bitData, Al_rect);
        for (int i = 0; i < AL_LineH.Count; i++)
        {
            LineRow line = (LineRow)AL_LineH[i];
            lineH.iw += line.width;
            lineH.ih += line.height;
        }


        lineV = new LineHV();
        ArrayList AL_LineV = getLineV(p, bitData, Al_rect);
        for (int i = 0; i < AL_LineV.Count; i++)
        {
            LineRow line = (LineRow)AL_LineV[i];
            lineV.iw += line.width;
            lineV.ih += line.height;
        }
        ArrayList AL_Line = new ArrayList();
        if (AL_LineH.Count > 0 && AL_LineV.Count > 0)
        {
            lineH.iper = lineH.iw / lineH.ih;
            lineV.iper = lineV.ih / lineV.iw;
            if (lineH.iper > lineV.iper)
            {
                AL_Line = AL_LineH;
                isRotate90 = 0;
            }
            else
            {
                AL_Line = AL_LineV;
                isRotate90 = 90;
            }
        }
        else
        {
            isRotate90 = 0;
            return 0;
        }

        //AciDebug.Debug(iH_width + " " + iH_height + " " + iV_width + " " + iV_height + " " + isRotate90 + " " + fileInfo.FullName);
        //AciDebug.Debug(iAll_width + " " + iAll_height + " " + isRotate90 + " " + fileInfo.FullName);

        //ArrayList AL_Line = AL_LineV;

        ArrayList al_ir = new ArrayList();
        ArrayList al_iNum = new ArrayList();
        float ir_per = 0;
        float ir_count = 0;

        //AciDebug.Debug(AL_Line.Count + "");
        for (int i = 0; i < AL_Line.Count; i++)
        {
            LineRow line = (LineRow)AL_Line[i];
            //AciDebug.Debug(line.width + " " + line.height);
            Line li = new Line();
            for (int j = 0; j < line.AL_Rect.Count; j++)
            {
                Rect rect_ls = (Rect)line.AL_Rect[j];
                if (show) rect_ls.Draw(p, iWidth);
                //AciDebug.Debug(rect_ls.pCenter.X + " " + (uint)rect_ls.pCenter.Y);
                li.Add(new myDot((uint)rect_ls.pCenter.X, (uint)rect_ls.pCenter.Y));
            }
            li.leastSquare();

            if (show) line.Draw(p, w, h, iWidth, false);

            if (li.r.ToString() != "������")
            {
                if (Math.Abs(li.r) <= 8f && li.iNum > 3)
                {
                    //AciDebug.Debug(li.r + " " + li.iNum + " " + line.width + " " + line.width * li.iNum);
                    al_ir.Add(li.r);
                    al_iNum.Add((float)line.width);
                    ir_per += li.r * line.width;
                    ir_count += line.width;
                }
            }
            else
            {
                //MessageBox.Show(li.r + "");
            }
        }
        if (al_ir.Count <= 5)
        {
            ir_per = 0;
            ir_count = 0;
            for (int i = 0; i < al_ir.Count; i++)
            {
                float irr = (float)al_ir[i];
                float inumber = (float)al_iNum[i];
                if (Math.Abs(irr) <= 3f)
                {
                    ir_per += irr * inumber;
                    ir_count += inumber;
                }
            }
        }
        if (ir_count > 0)
        {
            ir_per /= ir_count;
        }

        

        return ir_per;
        //MessageBox.Show("���б��" + iTableNumber);
    }

    public class LineHV
    {
        public float iw = 0;
        public float ih = 0;
        public float iper = 0;

        public LineHV()
        {
            
        }
    }

    //��ȡ���� AL_line
    unsafe ArrayList getLineH(byte* p, BitmapData bitData, ArrayList Al_rect)
    {
        Rect rect = null;
        ArrayList AL_noin = new ArrayList();
        ArrayList AL_Line = new ArrayList();
        for (int i = 0; i < Al_rect.Count; i++)
        {
            rect = (Rect)Al_rect[i];
            //MessageBox.Show(iPerH + "");
            bool isNew = true;
            for (int j = 0; j < AL_Line.Count; j++)
            {
                LineRow line = (LineRow)AL_Line[j];
                int iCenterC = Math.Abs(line.iCenterV - rect.pCenter.Y);
                float linePer = Math.Max(line.widthPer, line.heightPer);
                float rectPer = Math.Max(rect.width, rect.height);
                int iLineMin = (int)(linePer + rectPer) / 4;

                if (linePer > rectPer)
                {
                    float iper = linePer / rectPer;
                    if (iper > 1.5)
                    {
                        //��������ֱ��ȥ��
                        continue;
                    }
                }
                else
                {
                    float iper = rectPer / linePer;
                    if (iper > 1.5)
                    {
                        //��������ֱ��ȥ��
                        continue;
                    }
                }

                if (iCenterC <= iLineMin)
                {
                    bool isIn = false;
                    //���ֿ��������ǰ �� �е��Ҳ�
                    if (rect.l > line.r)
                    {
                        //������� ���� ���Ҳ� ���ó������ֿ鱾��
                        if (rect.l - line.r <= iLineMin)
                        {
                            isIn = true;
                        }
                    }
                    //���ֿ��������ǰ �� �е����
                    else if (rect.r < line.l)
                    {
                        //�����Ҳ� ���� ����� ���ó������ֿ鱾��
                        if (line.l - rect.r <= iLineMin)
                        {
                            isIn = true;
                        }
                    }
                    else
                    {
                        //���������ཻ
                        if (rect.b < line.t || rect.t > line.b)
                        {
                            //���ཻ
                        }
                        else
                        {
                            isIn = true;
                        }
                    }

                    if (isIn)
                    {
                        //if (indexi == 89)
                        //{
                        //    int it = 0;
                        //}

                        //��ͬ�е������������
                        line.Add(rect);
                        //rect.Draw(p, iWidth);
                        isNew = false;

                        //line.Draw(p, w, h, iWidth, false);
                        //if (show) bitmap.Save(fileInfo.FullName.Replace(".jpg", "_��ʱ_���롾" + AciMath.setNumberLength(indexi, 3) + "��.jpg"), myImageCodecInfo, encoderParams);
                        //indexi++;
                        break;
                    }
                }
            }
            if (isNew)
            {
                if (rect.height > 5)
                {
                    //û���ʺϵĵ㴴���µ�һ��
                    LineRow line = new LineRow(null);
                    line.Add(rect);
                    //rect.Draw(p, iWidth);
                    //MessageBox.Show(rect.pCenter.Y + "");
                    AL_Line.Add(line);
                }
                else
                {
                    AL_noin.Add(rect);
                }
            }
        }

        //�鿴�Ƿ���Ҫ����ĳһ��
        for (int i = 0; i < AL_noin.Count; i++)
        {
            rect = (Rect)AL_noin[i];

            for (int j = 0; j < AL_Line.Count; j++)
            {
                LineRow line = (LineRow)AL_Line[j];
                if (Rect.RectA_vs_LineB(rect, line))
                {
                    line.Add(rect);
                    break;
                }
            }
        }

        if (test) debug_Line(p, AL_Line, "��03�������С�����", bitData);

        ArrayList al_move = new ArrayList();
        //��ͨ����ϲ� ����
        int index = 0;
        while (index < AL_Line.Count)
        {
            LineRow line = (LineRow)AL_Line[index];
            if (al_move.IndexOf(line) > -1)
            {
                index++;
                continue;
            }
            bool isAdd = false;

            for (int j = 0; j < AL_Line.Count; j++)
            {
                LineRow line2 = (LineRow)AL_Line[j];
                if (al_move.IndexOf(line2) > -1)
                {
                    continue;
                }

                if (line != line2)
                {
                    int iCenterC = Math.Abs(line.iCenterV - line2.iCenterV);
                    float linePer = Math.Max(line.widthPer, line.heightPer);
                    float line2Per = Math.Max(line2.widthPer, line2.heightPer);
                    int iLineMin = (int)(linePer + line2Per) / 2;

                    bool isBigSmall = false;
                    if (linePer > line2Per)
                    {
                        float iper = linePer / line2Per;
                        if (iper > 2f)
                        {
                            //��������ֱ��ȥ��
                            isBigSmall = true;
                        }
                    }
                    else
                    {
                        float iper = line2Per / linePer;
                        if (iper > 2f)
                        {
                            //��������ֱ��ȥ��
                            isBigSmall = true;
                        }
                    }

                    bool isIn = false;
                    //������������֮��Ĳ��С�� ��С�еĸ߶�
                    if (iCenterC <= iLineMin)
                    {

                        //���ֿ��������ǰ �� �е��Ҳ�
                        if (line2.l > line.r)
                        {
                            //������� ���� ���Ҳ� ���ó������ֿ鱾��
                            if (line2.l - line.r <= iLineMin)
                            {
                                if (!isBigSmall) isIn = true;
                            }
                        }
                        //���ֿ��������ǰ �� �е����
                        else if (line2.r < line.l)
                        {
                            //�����Ҳ� ���� ����� ���ó������ֿ鱾��
                            if (line.l - line2.r <= iLineMin)
                            {
                                if (!isBigSmall) isIn = true;
                            }
                        }
                        else
                        {
                            if (line.b < line2.t || line.t > line2.b)
                            {
                                //���ཻ
                            }
                            else
                            {
                                isIn = true;
                            }
                        }
                    }
                    //else
                    //{
                    //    LineRow lineC = LineRow.LineA_in_LineB(line, line2);
                    //    if (lineC != null)
                    //    {
                    //        //�а�����ϵ
                    //        isIn = true;
                    //    }
                    //}

                    if (isIn)
                    {
                        isAdd = true;
                        line.Add(line2);
                        al_move.Add(line2);

                        //line.Draw(p, w, h, iWidth, false);
                        //if (show) bitmap.Save(fileInfo.FullName.Replace(".jpg", "_��ʱ_���������ߡ�" + AciMath.setNumberLength(indexi, 3) + "��.jpg"), myImageCodecInfo, encoderParams);
                        //indexi++;
                    }
                }
            }
            if (!isAdd)
            {
                index++;
            }
        }
        for (int i = 0; i < al_move.Count; i++)
        {
            LineRow line = (LineRow)al_move[i];
            AL_Line.Remove(line);
        }

        if (test) debug_Line(p, AL_Line, "��04���ϲ�������", bitData);

        return AL_Line;
    }

    //��ȡ���� AL_line
    unsafe ArrayList getLineV(byte* p, BitmapData bitData, ArrayList Al_rect)
    {
        Rect rect = null;
        ArrayList AL_noin = new ArrayList();
        ArrayList AL_Line = new ArrayList();
        for (int i = 0; i < Al_rect.Count; i++)
        {
            rect = (Rect)Al_rect[i];
            //MessageBox.Show(iPerH + "");
            bool isNew = true;
            for (int j = 0; j < AL_Line.Count; j++)
            {
                LineRow line = (LineRow)AL_Line[j];
                int iCenterC = Math.Abs(line.iCenterH - rect.pCenter.X);

                float linePer = Math.Max(line.widthPer, line.heightPer);
                float rectPer = Math.Max(rect.width, rect.height);
                int iLineMin = (int)(linePer + rectPer) / 4;

                if (linePer > rectPer)
                {
                    float iper = linePer / rectPer;
                    if (iper > 1.5)
                    {
                        //��������ֱ��ȥ��
                        continue;
                    }
                }
                else
                {
                    float iper = rectPer / linePer;
                    if (iper > 1.5)
                    {
                        //��������ֱ��ȥ��
                        continue;
                    }
                }
                if (iCenterC <= iLineMin)
                {
                    bool isIn = false;
                    //���ֿ��������ǰ �� �е��²�
                    if (rect.t > line.b)
                    {
                        //������� ���� ���²� ���ó������ֿ鱾��
                        if (rect.t - line.b <= iLineMin)
                        {
                            isIn = true;
                        }
                    }
                    //���ֿ��������ǰ �� �е��ϲ�
                    else if (rect.b < line.t)
                    {
                        //�����Ҳ� ���� ���ϲ� ���ó������ֿ鱾��
                        if (line.t - rect.b <= iLineMin)
                        {
                            isIn = true;
                        }
                    }
                    else
                    {
                        //���������ཻ
                        if (rect.r < line.l || rect.l > line.r)
                        {
                            //���ཻ
                        }
                        else
                        {
                            isIn = true;
                        }
                    }

                    if (isIn)
                    {
                        //if (indexi == 89)
                        //{
                        //    int it = 0;
                        //}

                        //��ͬ�е������������
                        line.Add(rect);
                        //rect.Draw(p, iWidth);
                        isNew = false;

                        //line.Draw(p, w, h, iWidth, false);
                        //if (show) bitmap.Save(fileInfo.FullName.Replace(".jpg", "_��ʱ_���롾" + AciMath.setNumberLength(indexi, 3) + "��.jpg"), myImageCodecInfo, encoderParams);
                        //indexi++;
                        break;
                    }
                }
            }
            if (isNew)
            {
                if (rect.width > 5)
                {
                    //û���ʺϵĵ㴴���µ�һ��
                    LineRow line = new LineRow(null);
                    line.Add(rect);
                    //rect.Draw(p, iWidth);
                    //MessageBox.Show(rect.pCenter.Y + "");
                    AL_Line.Add(line);
                }
                else
                {
                    AL_noin.Add(rect);
                }
            }
        }

        //�鿴�Ƿ���Ҫ����ĳһ��
        for (int i = 0; i < AL_noin.Count; i++)
        {
            rect = (Rect)AL_noin[i];

            for (int j = 0; j < AL_Line.Count; j++)
            {
                LineRow line = (LineRow)AL_Line[j];
                if (Rect.RectA_vs_LineB(rect, line))
                {
                    line.Add(rect);
                    break;
                }
            }
        }

        if (test) debug_Line(p, AL_Line, "��03�������С�����", bitData);

        ArrayList al_move = new ArrayList();
        //��ͨ����ϲ� ����
        int index = 0;
        while (index < AL_Line.Count)
        {
            LineRow line = (LineRow)AL_Line[index];
            if (al_move.IndexOf(line) > -1)
            {
                index++;
                continue;
            }

            bool isAdd = false;

            for (int j = 0; j < AL_Line.Count; j++)
            {
                LineRow line2 = (LineRow)AL_Line[j];
                if (al_move.IndexOf(line2) > -1)
                {
                    continue;
                }

                if (line != line2)
                {
                    int iCenterC = Math.Abs(line.iCenterH - line2.iCenterH);
                    float linePer = Math.Max(line.widthPer, line.heightPer);
                    float line2Per = Math.Max(line2.widthPer, line2.heightPer);
                    int iLineMin = (int)(linePer + line2Per) / 2;

                    bool isBigSmall = false;
                    if (linePer > line2Per)
                    {
                        float iper = linePer / line2Per;
                        if (iper > 2)
                        {
                            //��������ֱ��ȥ��
                            isBigSmall = true;
                        }
                    }
                    else
                    {
                        float iper = line2Per / linePer;
                        if (iper > 2)
                        {
                            //��������ֱ��ȥ��
                            isBigSmall = true;
                        }
                    }

                    bool isIn = false;
                    //������������֮��Ĳ��С�� ��С�еĸ߶�
                    if (iCenterC <= iLineMin)
                    {
                        //���ֿ��������ǰ �� �е��²�
                        if (line2.t > line.b)
                        {
                            //������� ���� ���²� ���ó������ֿ鱾��
                            if (line2.t - line.b <= iLineMin)
                            {
                                if (!isBigSmall) isIn = true;
                            }
                        }
                        //���ֿ��������ǰ �� �е��ϲ�
                        else if (line2.b < line.t)
                        {
                            //�����ϲ� ���� ���²� ���ó������ֿ鱾��
                            if (line.t - line2.b <= iLineMin)
                            {
                                if (!isBigSmall) isIn = true;
                            }
                        }
                        else
                        {
                            if (line.r < line2.l || line.l > line2.r)
                            {
                                //���ཻ
                            }
                            else
                            {
                                isIn = true;
                            }
                        }
                    }
                    //else
                    //{
                    //    LineRow lineC = LineRow.LineA_in_LineB(line, line2);
                    //    if (lineC != null)
                    //    {
                    //        //�а�����ϵ
                    //        isIn = true;
                    //    }
                    //}

                    if (isIn)
                    {
                        isAdd = true;
                        line.Add(line2);
                        al_move.Add(line2);

                        //line.Draw(p, w, h, iWidth, false);
                        //if (show) bitmap.Save(fileInfo.FullName.Replace(".jpg", "_��ʱ_���������ߡ�" + AciMath.setNumberLength(indexi, 3) + "��.jpg"), myImageCodecInfo, encoderParams);
                        //indexi++;
                    }
                }
            }
            if (!isAdd)
            {
                index++;
            }
        }
        for (int i = 0; i < al_move.Count; i++)
        {
            LineRow line = (LineRow)al_move[i];
            AL_Line.Remove(line);
        }

        if (test) debug_Line(p, AL_Line, "��04���ϲ�������", bitData);

        return AL_Line;
    }
    /// <summary>
    /// ���÷�ֵ
    /// </summary>
    /// <param name="Htg">ֱ��ͼ�ṹ��</param>
    /// <returns></returns>
    public uint setThreshold(OcrStruct.Histogram Htg)
    {
        ////Ostu ȫ�ֶ�ֵ�㷨
        ////��������䷽��
        //uint Oavg = 0;
        //uint Ostu = getOstu(Htg.pixelNum);
        uint Oshang = getShang(Htg.pixelNum);

        //System.Windows.Forms.MessageBox.Show(Ostu + "");

        return Oshang;
    }

    /// <summary>
    /// ֱ��ͼ
    /// </summary>
    /// <returns></returns>
    public unsafe OcrStruct.Histogram toZhiFang(byte* p)
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

        return new OcrStruct.Histogram(pixelNum, maxK);
    }



    /// <summary>
    /// �γ�ֱ�߼�� ��������б�Ƕ�
    /// </summary>
    public unsafe float toDotLine(byte* p, FileInfo file)
    {
        //��� Ϊ1ʱ˵���Ѿ����ʹ�
        uint[,] arrP = new uint[w, h];

        //bitmap.Save(file.FullName.Replace(".jpg", "_b4.jpg"), myImageCodecInfo, encoderParams);

        myDot d_up;//�ϵ� -1
        myDot d_next;//����� 0
        myDot d_down;//�µ� 1
        myDot dot, mydot;
        ArrayList Al_save;
        ArrayList Al_line = new ArrayList();

        for (uint y = 1; y < h - 1; y++)
        {
            for (uint x = 1; x < w - 1; x++)
            {
                mydot = new myDot(x, y, iWidth, 0);
                r = p[mydot.ib];
                if (r == 0 && arrP[x, y] == 0)
                {
                    Line line = new Line();
                    arrP[x, y] = 1;

                    dot = mydot;
                    //ת��������
                    Al_save = new ArrayList();

                    //���� +1 ����
                    int iNext = -1;

                    myDot d1 = new myDot(x - 1, y, iWidth, 0);
                    myDot d2 = new myDot(x + 1, y, iWidth, 0);


                    if (p[d1.ib] != 0 && p[d2.ib] != 0)
                    {
                        //��������һ���ڵ�Ҳû��  ֱ�����
                        p[mydot.ib] = p[mydot.ib + 1] = p[mydot.ib + 2] = 255;
                    }
                    else
                    {
                        //��������������һ���ڵ�
                        while (dot != null && (int)dot.x + iNext > 0 && (int)dot.x + iNext < w - 1)
                        {
                            line.Add(dot);

                            d_up = new myDot(dot.x, dot.y - 1, iWidth, -1);
                            //�����µ�ʱ��ȡ�������ϵ�
                            if (dot.y - 1 > 0)
                            {
                                if (dot.type != 1)
                                {
                                    d_up.c = -(p[d_up.ib] / 255 - 1);
                                }
                            }
                            else
                            {
                                d_up.c = 0;
                            }

                            d_next = new myDot((uint)((int)dot.x + iNext), dot.y, iWidth, 0);
                            d_next.c = -(p[d_next.ib] / 255 - 1);

                            d_down = new myDot(dot.x, dot.y + 1, iWidth, 1);
                            //�����ϵ�ʱ��ȡ�������µ�
                            if (dot.y + 1 < h)
                            {
                                if (dot.type != -1)
                                {
                                    d_down.c = -(p[d_down.ib] / 255 - 1);
                                }
                            }


                            //���
                            arrP[dot.x, dot.y] = 1;


                            int iNc = d_up.c + d_next.c + d_down.c;

                            //AciDebug.Debug(dot.type + " " + x + " " + y + " " + d_up.c + " " + d_next.c + " " + d_down.c);

                            //����3����ֻ��һ����
                            if (iNc == 1)
                            {
                                //�ҵ��ڷ����� ��ôֱ����һ����
                                if (d_next.c == 1)
                                {
                                    dot = d_next;
                                    Al_save.Clear();
                                }
                                else
                                {
                                    if (d_up.c == 1)
                                    {
                                        dot = d_up;
                                        //�洢�ϵ�
                                        Al_save.Add(d_up);
                                    }
                                    else
                                    {
                                        dot = d_down;
                                        //�洢�µ�
                                        Al_save.Add(d_down);
                                    }
                                    //�������۵�ǰ��2��� ���жϡ�
                                    if (Al_save.Count >= Key.toDotLine_dot)
                                    {
                                        for (int n = 0; n < Al_save.Count; n++)
                                        {
                                            myDot thisdot = (myDot)Al_save[n];
                                            if (n != Al_save.Count - 1)
                                            {
                                                p[thisdot.ib] = p[thisdot.ib + 1] = p[thisdot.ib + 2] = 255;
                                            }
                                            line.Remove(thisdot);
                                        }
                                        Al_save.Clear();
                                        dot = null;
                                    }
                                }
                            }
                            else if (iNc == 0)
                            {
                                //����һ���㶼û��ʱ�����жϡ�
                                Al_save.Clear();
                                dot = null;
                            }
                            else
                            {
                                //����3�����������㼰���ϵĺڵ㡾��·��
                                if (d_next.c == 1)
                                {
                                    //�������Ϊ��ɫʱ ѡ����� ���ѡ���·�Ͽ���

                                    //���Ͽ���·��
                                    if (d_up.c == 1)
                                    {
                                        //AciDebug.Debug("cut1");
                                        p[d_up.ib] = p[d_up.ib + 1] = p[d_up.ib + 2] = 255;
                                    }
                                    if (d_down.c == 1)
                                    {
                                        //AciDebug.Debug("cut2");
                                        p[d_down.ib] = p[d_down.ib + 1] = p[d_down.ib + 2] = 255;
                                    }
                                    //AciDebug.Debug("cut3");
                                    dot = d_next;
                                    Al_save.Clear();
                                }
                                else
                                {
                                    //�������Ϊ��ɫ�����µ�Ϊ��ɫʱ �Ͽ���ǰ�㡾�жϡ�
                                    p[dot.ib] = p[dot.ib + 1] = p[dot.ib + 2] = 255;
                                    Al_save.Clear();
                                    dot = null;
                                }
                            }
                            //�ҷ���
                            if (iNext == -1 && dot == null)
                            {
                                mydot = new myDot(x + 1, y, iWidth, 0);
                                r = p[mydot.ib];
                                if (r == 0)
                                {
                                    iNext = 1;
                                    dot = mydot;
                                }
                                //AciDebug.Debug("ת��");
                            }
                        }
                        //AciDebug.Debug("a " + line.iNum);
                        //�����߶�С��10���ȥ��
                        if (line.iNum < Key.toDotLine_line)
                        {
                            for (int k = 0; k < line.Al_dot.Count; k++)
                            {
                                myDot thisdot = (myDot)line.Al_dot[k];
                                p[thisdot.ib] = p[thisdot.ib + 1] = p[thisdot.ib + 2] = 255;
                            }
                        }
                        else
                        {
                            Al_line.Add(line);
                        }
                    }
                }
            }
        }

        //cutArea(p);

        //ȡx��������
        int gX = getGravityX(p);
        //��ͼ������λ���� Y �� b
        for (int i = 0; i < Al_line.Count; i++)
        {
            Line line = (Line)Al_line[i];
            line.leastSquare(gX);
        }

        toDotLineRotate(p, file, Al_line);

        int[] Y = new int[h];
        /// �� x=0 ���򼯽�
        for (uint y = 0; y < h; y++)
        {
            for (uint x = 0; x < w; x++)
            {
                mydot = new myDot(x, y, iWidth, 0);
                if (p[mydot.ib] == 0)
                {
                    Y[y]++;
                }
            }
        }
        //�����ݶ�ֵ
        int iDuan = 3;
        int iDuanNow = 0;
        ArrayList AL_Y = new ArrayList();

        int iNumber = 0;

        for (int i = 0; i < Y.Length; i++)
        {
            //����ʮ������Ч
            if (Y[i] > Key.toDotLine_T)
            {
                //�����ۼ�
                iNumber += Y[i];
            }
            else
            {
                //���Ӷ�ֵ
                iDuanNow++;
                //�������ݶ�ֵ
                if (iDuanNow > iDuan)
                {
                    if (iNumber > Key.toDotLine_end)
                    {
                        AL_Y.Add(iNumber);
                    }
                    iNumber = 0;
                }
            }
        }


        //AciDebug.Debug(file.Name + "������" + AL_Y.Count);
        return AL_Y.Count;
    }
    /// <summary>
    /// ȡ���ĵ�
    /// </summary>
    public unsafe int getGravityX(byte* p)
    {
        //ȡȫ���ڵ�����
        int pAll = 0;
        for (uint y = 0; y < h; y++)
        {
            for (uint x = 0; x < w; x++)
            {
                uint ib = y * iWidth + x * 3;
                r = p[ib];
                if (r == 0)
                {
                    pAll++;
                }
            }
        }
        int pX = 0;
        int gX = 0;
        for (uint x = 0; x < w; x++)
        {
            for (uint y = 0; y < h; y++)
            {
                uint ib = y * iWidth + x * 3;
                r = p[ib];
                if (r == 0)
                {
                    pX++;
                    if (pX >= pAll / 2)
                    {
                        gX = (int)x;
                        break;
                    }
                }
            }
            if (pX >= pAll / 2)
            {
                break;
            }
        }
        return gX;
    }
    /// <summary>
    /// �γ�ֱ�߼�� ������бУ��
    /// </summary>
    public unsafe float toDotLineRotate(byte* p, FileInfo file, ArrayList Al_line)
    {
        //�����б�Ƕ�
        float K = 0;
        int iall = 0;
        for (int i = 0; i < Al_line.Count; i++)
        {
            Line line = (Line)Al_line[i];

            //����50��Ĳż���Ȩ��
            if (line.iNum > Key.toDotLine_R_1)
            {
                //�����߶ε�б�ʴ���1

                //б��С��15�ȵ�ֱ��
                if (Math.Abs(line.r) < Key.toDotLine_R_2)
                {
                    K += line.k * line.iNum;
                    iall += line.iNum;
                }
            }
        }
        myDot mydot;


        float R = 0;

        if (iall != 0)
        {
            //��б�Ƕ�
            K /= iall;
            R = K / (float)Math.PI * 180;
            //AciDebug.Debug("��б�Ƕ� " + R);

            //��תͼ��
            if (R != 0)
            {
                uint[,] arrP = new uint[w, h];
                Point center = new Point((int)w / 2, (int)h / 2);
                float myK = -K;
                for (uint y = 0; y < h; y++)
                {
                    for (uint x = 0; x < w; x++)
                    {
                        mydot = new myDot(x, y, iWidth, 0);
                        r = p[mydot.ib];
                        if (r == 0)
                        {
                            int myX = (int)(center.X + (x - center.X) * Math.Cos(myK) - (y - center.Y) * Math.Sin(myK));
                            int myY = (int)(center.Y + (x - center.X) * Math.Sin(myK) + (y - center.Y) * Math.Cos(myK));
                            if (myX >= 0 && myX < w && myY >= 0 && myY < h)
                            {
                                arrP[myX, myY] = 1;
                            }
                            p[mydot.ib] = p[mydot.ib + 1] = p[mydot.ib + 2] = 255;
                        }
                    }
                }
                for (uint y = 0; y < h; y++)
                {
                    for (uint x = 0; x < w; x++)
                    {
                        if (arrP[x, y] == 1)
                        {
                            uint ib = (uint)y * iWidth + (uint)x * 3;
                            p[ib] = p[ib + 1] = p[ib + 2] = 0;
                        }
                    }
                }
            }
        }
        return R;
    }
    /// <summary>
    /// ϸ��������
    /// </summary>
    public unsafe void toSmallLine(byte* p, Bitmap bitmap, FileInfo file, int imax)
    {
        int it = 1;
        int index = 1;



        if (imax == -1)
        {
            imax = 1000;
        }

        while (it != 0 && index <= imax)
        {
            index++;

            //bitmap.Save(file.FullName.Replace(".jpg", "_" + (index+100) + "_1.jpg"), myImageCodecInfo, encoderParams);
            if (it != 0 && index == iOverIndex)
            {
                if (intCheck == null)
                {
                    intCheck = new uint[w, h];
                    for (uint y = 0; y < h; y++)
                    {
                        for (uint x = 0; x < w; x++)
                        {
                            uint ib = y * iWidth + x * 3;
                            if (p[ib] == 0)
                            {
                                intCheck[x, y] = 1;
                            }
                        }
                    }
                }
                it = toSmallLine_do2(p);
            }
            else
            {
                it = toSmallLine_do(p);
            }
        }

    }
    /// <summary>
    /// ϸ��������ϸ��ʵ��
    /// </summary>
    public unsafe int toSmallLine_do(byte* p)
    {
        //DateTime dt1 = DateTime.Now;
        //TimeSpan ts;

        int ijs = 0;

        ArrayList alXY = new ArrayList();
        int icheck = 0;
        for (uint y = 0; y < h; y++)
        {
            for (uint x = 0; x < w; x++)
            {
                myPoint myP = new myPoint(x, y, iWidth);

                if (p[myP.ib] == 0)
                {
                    icheck++;
                    int[,] arrN = toN_N(p, x, y, 3);

                    //1.  2<=NZ(p)<=6
                    bool ok1 = false;
                    int NZ = 0;
                    ArrayList al_dot = new ArrayList();
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (!(i == 1 && j == 1))
                            {
                                if (arrN[i, j] == 1)
                                {
                                    al_dot.Add(new Point(i, j));
                                    NZ += 1;
                                }
                            }
                        }
                    }
                    if (2 <= NZ && NZ <= 6)
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

                    if (ok1 && ok2 && ok3)
                    {
                        alXY.Add(myP);
                        //p[myP.ib] = 0;
                        //p[myP.ib + 1] = 0;
                        //p[myP.ib + 2] = 255;
                    }
                }
            }
        }
        //bitmap.Save(file.FullName.Replace(".jpg", "_" + index + ".jpg"), myImageCodecInfo, encoderParams);

        //ts = DateTime.Now - dt1;
        //AciDebug.Debug(index + "-1-������" + icheck + " " + ts.TotalMilliseconds);

        for (int it = 0; it < alXY.Count; it++)
        {
            myPoint myP = (myPoint)alXY[it];
            int[,] arrN = toN_N(p, myP.x, myP.y, 3);

            //1.  2<=NZ(p)<=6
            bool ok1 = false;
            int NZ = 0;
            ArrayList al_dot = new ArrayList();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (i == 1 && j == 1)
                    {

                    }
                    else
                    {
                        if (arrN[i, j] == 1)
                        {
                            al_dot.Add(new Point(i, j));
                            NZ += 1;
                        }
                    }
                }
            }
            if (2 <= NZ && NZ <= 6)
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
                if (arrN[0, 2] == 1 && arrN[1, 2] == 1)
                {
                    int icc = 0;
                    for (int ia = 0; ia < alXY.Count; ia++)
                    {
                        myPoint myp = (myPoint)alXY[ia];
                        if (myP.x - 1 == myp.x && myP.y + 1 == myp.y)
                        {
                            icc++;
                        }
                        if (myP.x == myp.x && myP.y + 1 == myp.y)
                        {
                            icc++;
                        }
                        if (icc == 2)
                        {
                            ok3 = false;
                            break;
                        }
                    }
                }
            }

            if (ok1 && ok2 && ok3)
            {
                ijs++;
                p[myP.ib] = 255;
                p[myP.ib + 1] = 255;
                p[myP.ib + 2] = 255;
            }
            else
            {
                //p[myP.ib] = 0;
                //p[myP.ib + 1] = 0;
                //p[myP.ib + 2] = 0;
            }
        }

        //ts = DateTime.Now - dt1;
        //AciDebug.Debug(index + "-2- " + ts.TotalMilliseconds);

        return ijs;
    }
    /// <summary>
    /// ϸ��������ϸ��ʵ��
    /// </summary>
    public unsafe int toSmallLine_do2(byte* p)
    {
        //DateTime dt1 = DateTime.Now;
        //TimeSpan ts;

        int ijs = 0;

        ArrayList alXY = new ArrayList();
        int icheck = 0;
        for (uint y = 0; y < h; y++)
        {
            for (uint x = 0; x < w; x++)
            {
                myPoint myP = new myPoint(x, y, iWidth);

                if (intCheck[x, y] == 1)
                {
                    icheck++;
                    int[,] arrN = toN_N(p, x, y, 3);

                    //1.  2<=NZ(p)<=6
                    bool ok1 = false;
                    int NZ = 0;
                    ArrayList al_dot = new ArrayList();
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (!(i == 1 && j == 1))
                            {
                                if (arrN[i, j] == 1)
                                {
                                    al_dot.Add(new Point(i, j));
                                    NZ += 1;
                                }
                            }
                        }
                    }
                    if (2 <= NZ && NZ <= 6)
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

                    if (ok1 && ok2 && ok3)
                    {
                        alXY.Add(myP);
                        //p[myP.ib] = 0;
                        //p[myP.ib + 1] = 0;
                        //p[myP.ib + 2] = 255;
                    }
                    intCheck[x, y] = 0;
                }
            }
        }
        //bitmap.Save(file.FullName.Replace(".jpg", "_" + index + ".jpg"), myImageCodecInfo, encoderParams);

        //ts = DateTime.Now - dt1;
        //AciDebug.Debug(index + "-1-������" + icheck + " " + ts.TotalMilliseconds);

        for (int it = 0; it < alXY.Count; it++)
        {
            myPoint myP = (myPoint)alXY[it];
            int[,] arrN = toN_N(p, myP.x, myP.y, 3);

            //1.  2<=NZ(p)<=6
            bool ok1 = false;
            int NZ = 0;
            ArrayList al_dot = new ArrayList();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (i == 1 && j == 1)
                    {

                    }
                    else
                    {
                        if (arrN[i, j] == 1)
                        {
                            al_dot.Add(new Point(i, j));
                            NZ += 1;
                        }
                    }
                }
            }
            if (2 <= NZ && NZ <= 6)
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
                if (arrN[0, 2] == 1 && arrN[1, 2] == 1)
                {
                    int icc = 0;
                    for (int ia = 0; ia < alXY.Count; ia++)
                    {
                        myPoint myp = (myPoint)alXY[ia];
                        if (myP.x - 1 == myp.x && myP.y + 1 == myp.y)
                        {
                            icc++;
                        }
                        if (myP.x == myp.x && myP.y + 1 == myp.y)
                        {
                            icc++;
                        }
                        if (icc == 2)
                        {
                            ok3 = false;
                            break;
                        }
                    }
                }
            }

            if (ok1 && ok2 && ok3)
            {
                ijs++;
                p[myP.ib] = 255;
                p[myP.ib + 1] = 255;
                p[myP.ib + 2] = 255;

                int N = 3;
                intCheck[myP.x, myP.y] = 0;

                for (int irow = -N; irow <= N; irow++)
                {
                    for (int icell = -N; icell <= N; icell++)
                    {
                        uint thisx = (uint)(myP.x + icell);
                        uint thisy = (uint)(myP.y + irow);
                        //�������߽�
                        if (thisx >= 0 && thisx < w && thisy >= 0 && thisy < h)
                        {
                            if (intCheck[thisx, thisy] == 0)
                            {
                                uint ib = thisy * iWidth + thisx * 3;
                                if (p[ib] == 0)
                                {
                                    intCheck[thisx, thisy] = 1;
                                }
                                else
                                {
                                    intCheck[thisx, thisy] = 0;
                                }
                            }
                        }
                    }
                }

            }
            else
            {
                //p[myP.ib] = 0;
                //p[myP.ib + 1] = 0;
                //p[myP.ib + 2] = 0;
            }
        }

        //ts = DateTime.Now - dt1;
        //AciDebug.Debug(index + "-2- " + ts.TotalMilliseconds);

        return ijs;
    }
    //public unsafe void cutArea(byte* p)
    //{
    //    tableCheck tb = new tableCheck();
    //    cutArea(p, ref tb);
    //}

    
    public struct Dot
    {
        public uint x;
        public uint y;
        public Dot(uint ix, uint iy)
        {
            x = ix;
            y = iy;
        }
    }
    double avgHui = 0;

    /// <summary>
    /// �ҶȻ�
    /// </summary>
    public unsafe void toGrey(byte* p)
    {
        for (uint y = 0; y < h; y++)
        {
            for (uint x = 0; x < w; x++)
            {
                //��ʼ��
                uint ib = y * iWidth + x * 3;

                //ȡƽ��ֵ
                r = p[ib];
                g = p[ib + 1];
                b = p[ib + 2];

                //�ҶȻ�
                int iee = (int)(0.11 * (double)r + 0.59 * (double)g + 0.3 * (double)b);
                p[ib] = (byte)iee;
                p[ib + 1] = (byte)iee;
                p[ib + 2] = (byte)iee;

                if (y > greyArea && y < h - greyArea && x > greyArea && x < w - greyArea)
                {
                   
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
    /// ��ֵ��
    /// </summary>
    /// <returns></returns>
    public unsafe float toTwo(byte* p, uint Ostu)
    {
        uint iDot = 0;
        for (uint y = 0; y < h; y++)
        {
            for (uint x = 0; x < w; x++)
            {
                //��ʼ��
                uint ib = y * iWidth + x * 3;
                if (p[ib] <= Ostu)
                {
                    p[ib] = (byte)0;
                    p[ib + 1] = (byte)0;
                    p[ib + 2] = (byte)0;
                    iDot++;
                }
                else
                {
                    p[ib] = (byte)255;
                    p[ib + 1] = (byte)255;
                    p[ib + 2] = (byte)255;
                }
            }
        }
        float fP = (float)iDot / (float)w / (float)h;
        return fP;
    }
    public unsafe void toClear(byte* p)
    {
        for (uint y = 0; y < h; y++)
        {
            for (uint x = 0; x < w; x++)
            {
                uint ib = y * iWidth + x * 3;
                if (p[ib] == 0)
                {
                    int[,] arrN = toN_N(p, x, y, 5);
                    bool ok = false;
                    if (testN5_N5(arrN, new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }) ||
                        testN5_N3(arrN, new int[] { 0, 0, 0, 0, 1, 0, 1, 1, 1 }) ||
                        testN5_N3(arrN, new int[] { 1, 0, 0, 1, 1, 0, 1, 0, 0 }) ||
                        testN5_N3(arrN, new int[] { 1, 1, 1, 0, 1, 0, 0, 0, 0 }) ||
                        testN5_N3(arrN, new int[] { 0, 0, 1, 0, 1, 1, 0, 0, 1 }))
                    {
                        ok = true;
                    }

                    if (y < 10 || y > h - 10 || x < 10 || x > w - 10)
                    {
                        ok = true;
                    }

                    if (ok)
                    {
                        p[ib] = 255;
                        p[ib + 1] = 255;
                        p[ib + 2] = 255;
                    }
                }
            }
        }
    }
    /// <summary>
    /// ����ģ��
    /// </summary>
    /// <param name="arrN">5*5ά�ȵ�����</param>
    /// <param name="template">3*3ά�ȵ�����</param>
    /// <returns></returns>
    public bool testN5_N3(int[,] arrN, int[] template)
    {
        bool ok = true;
        for (uint y = 1; y < 4; y++)
        {
            for (uint x = 1; x < 4; x++)
            {
                if (x == 2 && y == 2)
                {
                }
                else
                {
                    if (arrN[x, y] != template[3 * (y - 1) + (x - 1)])
                    {
                        return false;
                    }
                }
            }
        }
        return ok;
    }

    /// <summary>
    /// ����ģ��
    /// </summary>
    /// <param name="arrN">3*3ά�ȵ�����</param>
    /// <param name="template">3*3ά�ȵ�����</param>
    /// <returns></returns>
    public bool testN5_N5(int[,] arrN, int[] template)
    {
        for (uint y = 0; y < 5; y++)
        {
            for (uint x = 0; x < 5; x++)
            {
                if (!(x == 2 && y == 2))
                {
                    if (arrN[x, y] != template[5 * y + x])
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    /// <summary>
    /// ����ģ��
    /// </summary>
    /// <param name="arrN">3*3ά�ȵ�����</param>
    /// <param name="template">3*3ά�ȵ�����</param>
    /// <returns></returns>
    public bool testN3_N3(int[,] arrN, int[] template)
    {
        for (uint y = 0; y < 3; y++)
        {
            for (uint x = 0; x < 3; x++)
            {
                if (!(x == 1 && y == 1))
                {
                    if (arrN[x, y] != template[3 * y + x])
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    int getZO(int[,] arrN, int it, int ik)
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
    /// ����ĳһ��Ϊ���ĵ� N*N �ķ���  ��������߽� Ϊ 0, ��ɫ Ϊ 1
    /// </summary>
    /// <param name="p"></param>
    /// <param name="x">���ĵ�</param>
    /// <param name="y">���ĵ�</param>
    /// <param name="n">�����n  ��Ҫ����</param>
    /// <returns>n*n��С������ �������£���������</returns>
    public unsafe int[,] toN_N(byte* p, uint x, uint y, uint n)
    {
        int N = ((int)n - 1) / 2;
        int[,] arrN = new int[n, n];
        for (int irow = -N; irow <= N; irow++)
        {
            for (int icell = -N; icell <= N; icell++)
            {
                uint thisx = (uint)(x + icell);
                uint thisy = (uint)(y + irow);
                //�������߽�
                if (thisx >= 0 && thisx < w && thisy >= 0 && thisy < h)
                {
                    uint ib = thisy * iWidth + thisx * 3;
                    if (p[ib] == 0)
                    {
                        arrN[icell + N, irow + N] = 1;
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
        return arrN;
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
    public void outTime(string sName)
    {
        ts = DateTime.Now - dt;
        AciDebug.Debug(sName + "��" + ts.TotalMilliseconds);
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

    public uint getKittler(int[] HistGram)
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
}

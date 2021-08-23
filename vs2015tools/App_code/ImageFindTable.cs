using System;
using System.Collections;
using System.Text;
using System.Data;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

class ImageFindTable
{
    /// <summary>�������ݷ�ֵ</summary>
    myKey Key = new myKey();

    uint ic = 0;

    FileInfo myfile;

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
    public uint greyArea = 80;
    public bool show = false;
    Bitmap bitmap = null;

    public uint Oavg = 0;

    System.Drawing.Imaging.ImageCodecInfo myImageCodecInfo;
    EncoderParameters encoderParams;

    public ImageFindTable()
    {
        encoderParams = new EncoderParameters();
        long[] quality;


        //ͼƬ����
        quality = new long[] { 100 };

        EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
        encoderParams.Param[0] = encoderParam;

        myImageCodecInfo = GetEncoderInfo("image/jpeg");
    }
    unsafe void GetHistGram(Bitmap Src, ref int[] HistGram)
    {
        BitmapData SrcData = Src.LockBits(new Rectangle(0, 0, Src.Width, Src.Height), ImageLockMode.ReadWrite, Src.PixelFormat);
        int Width = SrcData.Width, Height = SrcData.Height, SrcStride = SrcData.Stride;
        byte* SrcP;
        for (int Y = 0; Y < 256; Y++) HistGram[Y] = 0;
        for (int Y = 0; Y < Height; Y++)
        {
            SrcP = (byte*)SrcData.Scan0 + Y * SrcStride;
            for (int X = 0; X < Width; X++, SrcP++) HistGram[*SrcP]++;
        }
        Src.UnlockBits(SrcData);
    }
    unsafe Bitmap ConvertToGrayBitmap(Bitmap Src)
    {
        Bitmap Dest = CreateGrayBitmap(Src.Width, Src.Height);
        BitmapData SrcData = Src.LockBits(new Rectangle(0, 0, Src.Width, Src.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        BitmapData DestData = Dest.LockBits(new Rectangle(0, 0, Dest.Width, Dest.Height), ImageLockMode.ReadWrite, Dest.PixelFormat);
        int Width = SrcData.Width, Height = SrcData.Height;
        int SrcStride = SrcData.Stride, DestStride = DestData.Stride;
        byte* SrcP, DestP;
        for (int Y = 0; Y < Height; Y++)
        {
            SrcP = (byte*)SrcData.Scan0 + Y * SrcStride;         // ������ĳ���ط�����unsafe���ܣ���ʵC#�е�unsafe��safe����ĺ����ˡ�            
            DestP = (byte*)DestData.Scan0 + Y * DestStride;
            for (int X = 0; X < Width; X++)
            {
                *DestP = (byte)((*SrcP + (*(SrcP + 1) << 1) + *(SrcP + 2)) >> 2);
                SrcP += 3;
                DestP++;
            }
        }
        Src.UnlockBits(SrcData);
        Dest.UnlockBits(DestData);
        return Dest;
    }
    unsafe Bitmap CreateGrayBitmap(int Width, int Height)
    {
        Bitmap Bmp = new Bitmap(Width, Height, PixelFormat.Format8bppIndexed);
        ColorPalette Pal = Bmp.Palette;
        for (int Y = 0; Y < Pal.Entries.Length; Y++) Pal.Entries[Y] = Color.FromArgb(255, Y, Y, Y);
        Bmp.Palette = Pal;
        return Bmp;
    }

    public bool b�Ƿ���ͼ = false;
    public bool setImage(FileInfo file, int imode)
    {
        bool isBai = false;
        //bool show = true;
        
        unsafe
        {
            System.Drawing.Image img = System.Drawing.Image.FromFile(file.FullName);


            //if (imode == 0)
            //{
            //    if (img.Width > img.Height)
            //    {
            //        img.RotateFlip(RotateFlipType.Rotate90FlipNone);
            //    }
            //}

            //dt = DateTime.Now;

            //myTable.fileSize = file.Length / 1024;
            int SS = 0;
            //��������
            bitmap = new Bitmap(Key.width, (int)(Key.width / (double)img.Width * (double)img.Height));
            Graphics g = Graphics.FromImage(bitmap);
            g.DrawImage(img, 0, 0, bitmap.Width, bitmap.Height);
            SS = (bitmap.Width - (int)greyArea * 2) * (bitmap.Height - (int)greyArea * 2);
            g.Dispose();
            //bitmap.Save(file.FullName.Replace(".jpg", "_��ʱ.jpg"), myImageCodecInfo, encoderParams);
            //bitmap.Dispose();
            //img.Dispose();

            //string file_img_0 = file.FullName.Replace(".jpg", "_��ʱ.jpg");

            //bitmap = (Bitmap)System.Drawing.Image.FromFile(file_img_0);

            //��ȡֱ��ͼ
            //Bitmap bmpGrey = ConvertToGrayBitmap(bitmap);
            //int[] HistGram = new int[256];
            //GetHistGram(bmpGrey, ref HistGram);
            //bmpGrey.Dispose();

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

            int[] pixelNum = new int[256];
            uint iR = 0;
            uint iG = 0;
            uint iB = 0;

            for (uint y = 0; y < h; y++)
            {
                for (uint x = 0; x < w; x++)
                {
                    uint ib = y * iWidth + x * 3;
                    //��ʼ��
                    iB = p[ib];
                    iG = p[ib + 1];
                    iR = p[ib + 2];
                    uint igrey = (iB + iG + iR) / 3;
                    

                    if (y > greyArea && y < h - greyArea && x > greyArea && x < w - greyArea)
                    {
                        p[ib] = (byte)igrey;
                        p[ib + 1] = (byte)igrey;
                        p[ib + 2] = (byte)igrey;
                        pixelNum[igrey]++;
                    }
                    else
                    {
                        p[ib] = (byte)255;
                        p[ib + 1] = (byte)255;
                        p[ib + 2] = (byte)255;
                    }
                }
            }

            uint ik = getIsoData(pixelNum);
            if (ik > 240)
            {
                ik = 240;
            }

            ////��ʼ��ֵ��
            toTwo(p, ik);

            if (show)
            {
                bitmap.UnlockBits(bitData);
                bitmap.Save(file.FullName.Replace(".jpg", "_" + imode + "_��ʱ_b��ֵ��.jpg"), myImageCodecInfo, encoderParams);
                bitData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            }

            ////��ͨ����
            ArrayList AL_rect = cutArea(p, file);
            int iS = 0;
            for (int i = 0; i < AL_rect.Count; i++)
            {
                Rect rect = (Rect)AL_rect[i];
                if (show) rect.Draw(p, iWidth);
                iS += rect.S;
            }
            float pp = (float)iS / (float)SS;

            if (AL_rect.Count == 0 || (AL_rect.Count < 10 && pp < 0.1f))
            {
                isBai = true;
            }
            if (AL_rect.Count > 1 && AL_rect.Count < 30 && pp < 0.02f)
            {
                isBai = true;
            }
            if (AL_rect.Count > 1 && AL_rect.Count < 50 && pp < 0.01f)
            {
                isBai = true;
            }
            if (AL_rect.Count > 1 && AL_rect.Count < 10 && pp > 0.05f)
            {
                isBai = false;
            }

            if (show)
            {
                bitmap.UnlockBits(bitData);
                bitmap.Save(file.FullName.Replace(".jpg", "_" + imode + "_��ʱ_cȥС�����.jpg"), myImageCodecInfo, encoderParams);
                bitData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            }
            


            //toSmallLine(p, bitmap, file, -1);

            //if (!show) bitmap.Save(file.FullName.Replace(".jpg", "_" + imode + "_��ʱ_������.jpg"), myImageCodecInfo, encoderParams);
            //if (!(myTable.iUpDot < 2000 || (myTable.iUpDot < 5000 && myTable.countRectAll < 10 && myTable.maxRect.iNum < 200)) && (myTable.maxRect.P < 200))
            //{
            //    ////ϸ��������
            //    toSmallLine(p, bitmap, file, -1);

            //    if (show) bitmap.Save(file.FullName.Replace(".jpg", "_" + imode + "e.jpg"), myImageCodecInfo, encoderParams);

            //    //�γ�ֱ�߼�� ������ ֱ������
            //    myTable.iTableLine = (int)toDotLine(p, file);

                

            //    if (show) bitmap.Save(file.FullName.Replace(".jpg", "_" + imode + "f.jpg"), myImageCodecInfo, encoderParams);
            //}

            //outTime("���");

            bitmap.UnlockBits(bitData);
            
            bitmap.Dispose();
            img.Dispose();

            //File.Delete(file_img_0);
            
        }

        return isBai;
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
        uint Ostu = getOstu(Htg.pixelNum);
        //uint Oshang = getShang(Htg.pixelNum);

        //System.Windows.Forms.MessageBox.Show(Ostu + "");

        return Ostu;
    }

    /// <summary>
    /// ֱ��ͼ
    /// </summary>
    /// <returns></returns>
    public unsafe OcrStruct.Histogram toZhiFang(byte* p)
    {
        int[] pixelNum = new int[256];
        int max = 0;
        //if (y > greyArea && y < h - greyArea && x > greyArea && x < w - greyArea)
        for (uint y = 0; y < h; y++)
        {
            for (uint x = 0; x < w; x++)
            {
                if (y > greyArea && y < h - greyArea && x > greyArea && x < w - greyArea)
                {
                    //��ʼ��
                    uint ib = y * iWidth + x * 3;
                    r = p[ib];
                    g = p[ib + 1];
                    b = p[ib + 2];
                    pixelNum[(r + g + b) / 3]++;
                }
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

        cutArea(p, file);

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

    /// <summary>
    /// ��ͨ����ָ��㷨
    /// </summary>
    public unsafe ArrayList cutArea(byte* p, FileInfo file)
    {
        //MessageBox.Show("a");

        Rect rect;
        Dot point;
        uint dot = 0;
        //���
        uint[,] arrP = new uint[w, h];
        //��ͨ׶
        ArrayList Al_sp = null;
        ArrayList Al_rect = new ArrayList();
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
                                        rect.r = thisx + 1;
                                    }
                                    if (thisy < rect.t)
                                    {
                                        rect.t = thisy;
                                    }
                                    else if (thisy > rect.b)
                                    {
                                        rect.b = thisy + 1;
                                    }
                                }
                            }
                        }
                        //��׶
                        Al_sp.RemoveAt(0);
                    }
                    Al_rect.Add(rect);
                }
            }
        }
        uint greyArea4 = 120;
        ArrayList AL_ok = new ArrayList();
        for (int i = 0; i < Al_rect.Count; i++)
        {
            rect = (Rect)Al_rect[i];
            rect.calculate();
            if (rect.S < 100)
            {
                if (show) rect.Clear(p);
            }
            else
            {
                if (rect.iNum > 20)
                {
                    if (rect.S < 10000)
                    {
                        float idot = rect.getDotBlack(file);
                        if (idot > 0.01f)
                        {
                            //װ����  �������
                            if ((rect.r < greyArea4 && rect.b < h / 7 * 6 && rect.t > h / 7)
                                || (rect.l > w - greyArea4 && rect.b < h / 7 * 6 && rect.t > h / 7))
                            {
                                //װ����  �������
                                if (rect.idotBlackNum > 0.01f)
                                {
                                    AL_ok.Add(rect);
                                }
                                else
                                {
                                    if (show) rect.Clear(p);
                                }
                            }
                            else
                            {
                                AL_ok.Add(rect);
                            }
                        }
                        else
                        {
                            if (show) rect.Clear(p);
                        }
                    }
                    else
                    {
                        AL_ok.Add(rect);
                    }
                }
                else
                {
                    if (show) rect.Clear(p);
                }
            }
        }
        
        ArrayList AL_ok2 = new ArrayList();
        for (int i = 0; i < AL_ok.Count; i++)
        {
            rect = (Rect)AL_ok[i];
            if (rect.r < greyArea4 || rect.b < greyArea4 || rect.l > w - greyArea4 || rect.t > h - greyArea4)
            {
                //��Ե��ȥ������
                if (rect.pp < 0.2f)
                {
                    if (show) rect.Clear(p);
                }
                else
                {
                    //�ı�Ҫ�����
                    if (rect.idotBlackNum > 0.01f)
                    {
                        AL_ok2.Add(rect);
                    }
                    else
                    {
                        if (show) rect.Clear(p);
                    }
                }
            }
            else
            {
                AL_ok2.Add(rect);
            }
        }

        return AL_ok2;
    }

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
    public unsafe int toGrey(byte* p)
    {
        double it = 0;
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
                    avgHui += (double)iee;
                    it++;
                }
                else
                {
                    p[ib] = (byte)255;
                    p[ib + 1] = (byte)255;
                    p[ib + 2] = (byte)255;
                }
            }
        }
        avgHui = avgHui / it;

        avgHui *= 0.6;
        //System.Windows.Forms.MessageBox.Show(avgHui.ToString());
        int iUpDot = 0;
        for (uint y = greyArea; y < h - greyArea; y++)
        {
            for (uint x = greyArea; x < w - greyArea; x++)
            {
                //��ʼ��
                uint ib = y * iWidth + x * 3;

                //ȡƽ��ֵ
                r = p[ib];

                if (r < avgHui)
                {
                    iUpDot++;
                }
            }
        }

        //System.Windows.Forms.MessageBox.Show(avgHui + " " + iUpDot);
        return iUpDot;
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
    public uint getYen(int[] HistGram)
    {
        int threshold;
        int ih, it;
        double crit;
        double max_crit;
        double[] norm_histo = new double[HistGram.Length]; /* normalized histogram */
        double[] P1 = new double[HistGram.Length]; /* cumulative normalized histogram */
        double[] P1_sq = new double[HistGram.Length];
        double[] P2_sq = new double[HistGram.Length];

        int total = 0;
        for (ih = 0; ih < HistGram.Length; ih++)
            total += HistGram[ih];

        for (ih = 0; ih < HistGram.Length; ih++)
            norm_histo[ih] = (double)HistGram[ih] / total;

        P1[0] = norm_histo[0];
        for (ih = 1; ih < HistGram.Length; ih++)
            P1[ih] = P1[ih - 1] + norm_histo[ih];

        P1_sq[0] = norm_histo[0] * norm_histo[0];
        for (ih = 1; ih < HistGram.Length; ih++)
            P1_sq[ih] = P1_sq[ih - 1] + norm_histo[ih] * norm_histo[ih];

        P2_sq[HistGram.Length - 1] = 0.0;
        for (ih = HistGram.Length - 2; ih >= 0; ih--)
            P2_sq[ih] = P2_sq[ih + 1] + norm_histo[ih + 1] * norm_histo[ih + 1];

        /* Find the threshold that maximizes the criterion */
        threshold = -1;
        max_crit = Double.MinValue;
        for (it = 0; it < HistGram.Length; it++)
        {
            crit = -1.0 * ((P1_sq[it] * P2_sq[it]) > 0.0 ? Math.Log(P1_sq[it] * P2_sq[it]) : 0.0) + 2 * ((P1[it] * (1.0 - P1[it])) > 0.0 ? Math.Log(P1[it] * (1.0 - P1[it])) : 0.0);
            if (crit > max_crit)
            {
                max_crit = crit;
                threshold = it;
            }
        }
        return (uint)threshold;
    }
    public uint getIsoData(int[] HistGram)
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

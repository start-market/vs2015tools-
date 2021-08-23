using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

public class saveImage
{
    ///jpgѹ���� tif
    //saveImage imgSaveTif = new saveImage();
    //imgSaveTif.setQuality(50);
    //string sfile2 = @"C:\Users\wangjia\Desktop\0001022\0000003A_��ʱ.jpg";
    //Bitmap bmp = (Bitmap)System.Drawing.Image.FromFile(sfile);
    //imgSaveTif.toTwo(ref bmp);
    //imgSaveTif.Save(bmp, sfile2);
    //imgSaveTif.writeTiffInfo(sfile2);
    //File.Delete(sfile);
    //File.Move(sfile2, sfile);

    ///�ڰ׶�ֵ�� tif
    //string sfile = @"C:\Users\wangjia\Desktop\0001022\0000003A.jpg";
    //saveImage imgSaveTif = new saveImage();
    //imgSaveTif.setDefaultTiff();
    //string sfile2 = @"C:\Users\wangjia\Desktop\0001022\0000003A.tif";
    //Bitmap bmp = (Bitmap)System.Drawing.Image.FromFile(sfile);
    //imgSaveTif.Save(bmp, sfile2);

    System.Drawing.Imaging.ImageCodecInfo myImageCodecInfo;
    EncoderParameters encoderParams;

    public int iQ = 80;

    public saveImage()
    {
        setDefault();
    }
    public void Save(Image img, string path)
    {
        img.Save(path, myImageCodecInfo, encoderParams);
        img.Dispose();
    }
    public void SaveNoDispose(Image img, string path)
    {
        img.Save(path, myImageCodecInfo, encoderParams);
    }
    public void setQuality(int iq)
    {
        iQ = iq;
        long[] quality;
        //ͼƬ����
        quality = new long[] { iQ };
        EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
        encoderParams.Param[0] = encoderParam;
    }
    void setDefault()
    {
        encoderParams = new EncoderParameters();
        long[] quality;

        //ͼƬ����
        quality = new long[] { iQ };
        EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
        encoderParams.Param[0] = encoderParam;

        myImageCodecInfo = GetEncoderInfo("image/jpeg");
    }

    public void setDefaultTiff()
    {
        encoderParams = new EncoderParameters(2);
        encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, Convert.ToInt32(EncoderValue.CompressionCCITT4));
        //ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, Convert.ToInt32(EncoderValue.CompressionNone));
        encoderParams.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, Convert.ToInt32(EncoderValue.MultiFrame));

        myImageCodecInfo = GetEncoderInfo("image/tiff");
    }
    public void SaveTiff(Bitmap bmp, string savepath)
    {
        Guid guid = bmp.FrameDimensionsList[0];
        System.Drawing.Imaging.FrameDimension dimension = new System.Drawing.Imaging.FrameDimension(guid);
        bmp.SelectActiveFrame(dimension, 0);

        bmp.Save(savepath, myImageCodecInfo, encoderParams);
        bmp.Dispose();
    }
    public void SaveTiffNoDispose(Bitmap bmp, string savepath)
    {
        Guid guid = bmp.FrameDimensionsList[0];
        System.Drawing.Imaging.FrameDimension dimension = new System.Drawing.Imaging.FrameDimension(guid);
        bmp.SelectActiveFrame(dimension, 0);

        bmp.Save(savepath, myImageCodecInfo, encoderParams);
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

    public unsafe void toTwo(ref Bitmap bmp)
    {
        BitmapData bitData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        //ȡ��ÿ�ж��������
        uint ic = (uint)(bitData.Stride - bitData.Width * 3);

        //ȡ��ȫ������ֵ ÿ�� 3��  Ϊ  R G B ÿ��ѭ��ɾ�� һ�������� xx ��
        byte* p = (byte*)(bitData.Scan0.ToPointer());

        uint w = (uint)bitData.Width;
        uint h = (uint)bitData.Height;

        //һ�е����ص�
        uint iWidth = w * 3 + ic;

        int[] pixelNum = new int[256];

        uint greyArea = 40;

        uint iR = 0;
        uint iG = 0;
        uint iB = 0;
        for (uint y = greyArea; y < h - greyArea; y++)
        {
            for (uint x = greyArea; x < w - greyArea; x++)
            {
                //��ʼ��

                uint ib = y * iWidth + x * 3;
                iB = p[ib];
                iG = p[ib + 1];
                iR = p[ib + 2];
                pixelNum[(iB + iG + iR) / 3]++;
            }
        }
        uint ishang = 0;
        uint istu = 0;

        if (ishang == 0) ishang = getShang(pixelNum);
        if (istu == 0) istu = getOstu(pixelNum);

        uint ik = istu;
        if (istu > ishang)
        {
            ik = ishang;
        }
        //MessageBox.Show(ik + "");
        ////��ʼ��ֵ��
        toTwo(p, ik, w, h, iWidth);
        bmp.UnlockBits(bitData);
    }
    /// <summary>
    /// ��ֵ��
    /// </summary>
    /// <returns></returns>
    public unsafe void toTwo(byte* p, uint Ostu, uint w, uint h, uint iWidth)
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
    /// <summary>
    /// ��ֵ��
    /// </summary>
    /// <returns></returns>
    public unsafe void toTwo(byte* p, uint Ostu, uint ibX, uint ieX, uint ibY, uint ieY, uint iWidth)
    {
        for (uint y = ibY; y < ieY; y++)
        {
            for (uint x = ibX; x < ieX; x++)
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

    public unsafe void toTwo(ref Bitmap bmp, uint icutX, uint icutY, bool isGrey)
    {
        BitmapData bitData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        if (bmp.Width > bmp.Height)
        {
            uint ii = icutX;
            icutX = icutY;
            icutY = ii;
        }

        //ȡ��ÿ�ж��������
        uint ic = (uint)(bitData.Stride - bitData.Width * 3);

        //ȡ��ȫ������ֵ ÿ�� 3��  Ϊ  R G B ÿ��ѭ��ɾ�� һ�������� xx ��
        byte* p = (byte*)(bitData.Scan0.ToPointer());

        uint w = (uint)bitData.Width;
        uint h = (uint)bitData.Height;

        //һ�е����ص�
        uint iWidth = w * 3 + ic;

        if (isGrey)
        {
            for (uint y = 0; y < h; y++)
            {
                for (uint x = 0; x < w; x++)
                {
                    uint ib = y * iWidth + x * 3;
                    uint b = p[ib];
                    uint g = p[ib + 1];
                    uint r = p[ib + 2];

                    float igrey = (0.11f * (float)r + 0.59f * (float)g + 0.3f * (float)b);

                    float ic_r_g = (float)r / (float)g;
                    float ic_r_b = (float)r / (float)b;

                    float ic_b_r = (float)b / (float)r;
                    float ic_b_g = (float)b / (float)g;

                    float ic_g_r = (float)g / (float)r;
                    float ic_g_b = (float)g / (float)b;


                    //float iall = 0;
                    //float it = 0;
                    //if (ic_r_g > 1f) { iall += ic_r_g; it++; }
                    //if (ic_r_b > 1f) { iall += ic_r_b; it++; }
                    //if (ic_b_r > 1f) { iall += ic_b_r; it++; }
                    //if (ic_b_g > 1f) { iall += ic_b_g; it++; }
                    //if (ic_g_r > 1f) { iall += ic_g_r; it++; }
                    //if (ic_g_b > 1f) { iall += ic_g_b; it++; }

                    bool isBao = false;
                    float iBaoRed = 1.2f;
                    if (ic_r_g > iBaoRed && ic_r_b > iBaoRed)
                    {
                        isBao = true;
                    }

                    if (isBao)
                    {
                        igrey *= 0.9f;
                    }

                    p[ib] = (byte)igrey;
                    p[ib + 1] = (byte)igrey;
                    p[ib + 2] = (byte)igrey;
                }
            }
        }


        uint ijuX = w / icutX;
        uint ijuY = h / icutY;

        uint ik_all = 0;
        uint ik_it = 0;

        for (uint iy = 0; iy < icutY; iy++)
        {
            uint ibY = iy * ijuY;
            uint ieY = ibY + ijuY;
            if (iy == icutY - 1)
            {
                ieY = h;
            }

            for (uint ix = 0; ix < icutX; ix++)
            {
                int[] pixelNum = new int[256];

                uint ibX = ix * ijuX;
                uint ieX = ibX + ijuX;
                if (ix == icutX - 1)
                {
                    ieX = w;
                }
                //uint greyArea = 40;

                uint iR = 0;
                uint iG = 0;
                uint iB = 0;

                for (uint y = ibY; y < ieY; y++)
                {
                    for (uint x = ibX; x < ieX; x++)
                    {
                        //��ʼ��

                        uint ib = y * iWidth + x * 3;
                        iB = p[ib];
                        iG = p[ib + 1];
                        iR = p[ib + 2];
                        pixelNum[(iB + iG + iR) / 3]++;
                    }
                }


                uint ishang = 0;
                uint istu = 0;

                ishang = getShang(pixelNum);
                istu = getOstu(pixelNum);

                uint ik = istu;
                if (ishang < istu)
                {
                    ik = ishang;
                }
                uint ik_per = 0;
                if (ik_it > 0)
                {
                    ik_per = ik_all / ik_it;
                    if (ik > ik_per && ik > 220)
                    {
                        if (ik - ik_per > 30)
                        {
                            ik = ik_per;
                        }
                    }
                }
                ik_all += ik;
                ik_it++;

                //AciDebug.Debug(ik + "  " + ik_per + "  " + ishang + "  " + istu + "  " + ix + "  " + iy + "  "); 
                //ik = istu;
                //MessageBox.Show(ik + "");
                ////��ʼ��ֵ��
                toTwo(p, ik, ibX, ieX, ibY, ieY, iWidth);
            }
        }


        bmp.UnlockBits(bitData);
    }

    public void cutTiffInfo(string filename, string to_filename)
    {
        string saveFile = to_filename;
        FileStream fs = new FileStream(saveFile, FileMode.Create);
        FileStream fsRead = new FileStream(filename, FileMode.Open);
        if (fsRead.ReadByte() == 73)
        {
            for (int j = 1; j < fsRead.Length; j++)
            {
                byte byt = (byte)fsRead.ReadByte();
                if (j >= 170)
                {
                    fs.WriteByte(byt);
                }
            }
            fsRead.Close();
        }
        else
        {
            fsRead.Close();
        }
        fs.Close();
    }
    public void writeTiffInfo(string filename)
    {
        ArrayList al = new ArrayList();
        al.Add(filename);
        writeTiffInfo(al, filename);
    }
    public void writeTiffInfo(ArrayList AL_file, string to_filename)
    {
        writeTiffInfo(AL_file, to_filename, Size.Empty);
    }
    public void writeTiffInfo(ArrayList AL_file, string to_filename, Size sizeWH)
    {
        string saveFile = "";
        if (AL_file.IndexOf(to_filename) > -1)
        {
            saveFile = to_filename + "��ʱ";
        }
        else
        {
            saveFile = to_filename;
        }
        FileStream fs = new FileStream(saveFile, FileMode.Create);


        long iPrevLength = 0;
        for (int i = 0; i < AL_file.Count; i++)
        {
            string filename = AL_file[i].ToString();

            jpgSizeDPI size = saveImage.getJpgSizeDPI(filename);
            if (sizeWH != Size.Empty)
            {
                size.sizeWH = sizeWH;
            }
            int iwidth = size.sizeWH.Width;
            int iheight = size.sizeWH.Height;

            if (i == 0)
            {
                string sdpi = "C8 00";
                if (size.sizeDPI.Width == 300)
                {
                    sdpi = "2C 01";
                }
                fileStreamWrite(ref fs, "49 49 2A 00");
                fileStreamWrite(ref fs, "1E 00 00 00");//��һ��IDFƫ����
                fileStreamWrite(ref fs, sdpi + " 00 00 01 00 00 00");//ˮƽ�ֱ���  C8 00 200dpi  2C 01 300dpi
                fileStreamWrite(ref fs, sdpi + " 00 00 01 00 00 00");//��ֱ�ֱ���  C8 00 200dpi  2C 01 300dpi
                fileStreamWrite(ref fs, "08 00 08 00 08 00");
            }

            //AciDebug.Debug(iwidth + "");
            FileStream fsRead = new FileStream(filename, FileMode.Open);
            if (fsRead.ReadByte() == 255)
            {
                fileStreamWrite(ref fs, "0D 00");//����13������
                fileStreamWrite(ref fs, "FE 00 04 00 01 00 00 00 00 00 00 00");//01.tif�̶�ͷ����
                fileStreamWrite(ref fs, "00 01 04 00 01 00 00 00 " + toHex2(iwidth, 4));//02.ͼ���� ����ĸ��ֽ�
                fileStreamWrite(ref fs, "01 01 04 00 01 00 00 00 " + toHex2(iheight, 4));//03.ͼ��߶� ����ĸ��ֽ�
                fileStreamWrite(ref fs, "02 01 03 00 03 00 00 00 18 00 00 00");//04.��ɫ��� 01 ��ɫ 04 16ɫ 08 256ɫ  2���ֽ�����Ϊ���ɫ
                fileStreamWrite(ref fs, "03 01 03 00 01 00 00 00 07 00 00 00");//05.ͼƬ�Ƿ�ѹ�� 05 ѹ��
                fileStreamWrite(ref fs, "06 01 03 00 01 00 00 00 06 00 00 00");//06.�Ƿ�ɫ 01 ��ɫ ��������ɫ
                fileStreamWrite(ref fs, "11 01 04 00 01 00 00 00 " + toHex2(30 + 162 * (i + 1) + iPrevLength, 4));//07.ͼ��������ʼ�ֽ�������ļ���ʼ����λ�� ����ĸ��ֽ�
                fileStreamWrite(ref fs, "15 01 03 00 01 00 00 00 03 00 00 00");//08.δ֪����ֵ=3
                fileStreamWrite(ref fs, "16 01 04 00 01 00 00 00 " + toHex2(iheight, 4));//09.ͼ���м���ɨ���� ����ͼ��߶� ����ĸ��ֽ�
                fileStreamWrite(ref fs, "17 01 04 00 01 00 00 00 " + toHex2(fsRead.Length - 1, 4));//10.ͼ���ܼ��ֽ��� �ļ�����-606 ����ĸ��ֽ�
                fileStreamWrite(ref fs, "1A 01 05 00 01 00 00 00 08  00 00 00");//11.ˮƽ�ֱ��� ����ڵڼ����ֽ�  C8 00 200dpi  2C 01 300dpi
                fileStreamWrite(ref fs, "1B 01 05 00 01 00 00 00 10 00 00 00");//12.��ֱ�ֱ��� ����ڵڼ����ֽ�
                fileStreamWrite(ref fs, "28 01 03 00 01 00 00 00 02 00 00 00");//13.δ֪����ֵ=2
                if (i != AL_file.Count - 1)
                {
                    fileStreamWrite(ref fs, toHex2(30 + 162 * (i + 1) + iPrevLength + fsRead.Length, 4));
                }
                else
                {
                    fileStreamWrite(ref fs, "00 00 00 00");
                }
                iPrevLength += fsRead.Length;

                //ͼ������
                fileStreamWrite(ref fs, "FF");
                //fileStreamWrite(ref fs, "FF FF 00");
                for (int j = 1; j < fsRead.Length; j++)
                {
                    fs.WriteByte((byte)fsRead.ReadByte());
                }

                fsRead.Close();
            }
            else
            {
                fsRead.Close();
            }
        }
        fs.Close();

        if (saveFile.IndexOf("��ʱ") > -1)
        {
            FileInfo file = new FileInfo(to_filename);
            file.Delete();
            File.Move(saveFile, to_filename);
        }
    }
    public struct jpgSizeDPI
    {
        public Size sizeWH;
        public Size sizeDPI;
    }
    /// <summary>
    /// ��ȡͼƬ��С
    /// </summary>
    /// <param name="FileName"></param>
    /// <returns></returns>
    public static jpgSizeDPI getJpgSizeDPI(string FileName)
    {
        //C#���ٻ�ȡJPGͼƬ��С��Ӣ��ֱ���
        int rx = 0;
        jpgSizeDPI size = new jpgSizeDPI();
        if (!File.Exists(FileName)) return size;
        FileStream F_Stream = File.OpenRead(FileName);
        int ff = F_Stream.ReadByte();
        int type = F_Stream.ReadByte();
        if (ff != 0xff || type != 0xd8)
        {//��JPG�ļ�
            F_Stream.Close();
            return size;
        }
        long ps = 0;
        do
        {
            do
            {
                ff = F_Stream.ReadByte();
                if (ff < 0) //�ļ�����
                {
                    F_Stream.Close();
                    return size;
                }
            } while (ff != 0xff);

            do
            {
                type = F_Stream.ReadByte();
            } while (type == 0xff);

            //MessageBox.Show(ff.ToString() + "," + type.ToString(), F_Stream.Position.ToString());
            ps = F_Stream.Position;
            switch (type)
            {
                case 0x00:
                case 0x01:
                case 0xD0:
                case 0xD1:
                case 0xD2:
                case 0xD3:
                case 0xD4:
                case 0xD5:
                case 0xD6:
                case 0xD7:
                    break;
                case 0xc0: //SOF0��
                    ps = F_Stream.ReadByte() * 256;
                    ps = F_Stream.Position + ps + F_Stream.ReadByte() - 2; //�Ӷγ���

                    F_Stream.ReadByte(); //������������
                    //�߶�
                    //�߶�
                    int ih = F_Stream.ReadByte() * 256;
                    ih = ih + F_Stream.ReadByte();
                    size.sizeWH.Height = ih;

                    //���
                    int iw = F_Stream.ReadByte() * 256;
                    iw = iw + F_Stream.ReadByte();
                    size.sizeWH.Width = iw;

                    F_Stream.Close();
                    return size;
                    //������Ϣ����
                    //if (rx != 1 && rx < 3) rx = rx + 1;
                    break;
                case 0xe0: //APP0��
                    ps = F_Stream.ReadByte() * 256;
                    ps = F_Stream.Position + ps + F_Stream.ReadByte() - 2; //�Ӷγ���

                    F_Stream.Seek(5, SeekOrigin.Current); //����APP0���(5bytes)
                    F_Stream.Seek(2, SeekOrigin.Current); //�������汾��(1bytes)���ΰ汾��(1bytes)
                    int units = F_Stream.ReadByte(); //X��Y���ܶȵ�λ,units=0���޵�λ,units=1������/Ӣ��,units=2������/����

                    //ˮƽ����(����/Ӣ��)�ֱ���
                    int Wpx = F_Stream.ReadByte() * 256;
                    Wpx = Wpx + F_Stream.ReadByte();
                    if (units == 2) Wpx = (int)Math.Round((Wpx * 2.54)); //���ױ�ΪӢ��
                    //��ֱ����(����/Ӣ��)�ֱ���
                    int Hpx = F_Stream.ReadByte() * 256;
                    Hpx = Hpx + F_Stream.ReadByte();
                    if (units == 2) Hpx = (int)Math.Round((Hpx * 2.54)); //���ױ�ΪӢ��

                    size.sizeDPI.Width = Wpx;
                    size.sizeDPI.Height = Hpx;

                    //������Ϣ����
                    if (rx != 2 && rx < 3) rx = rx + 2;
                    break;

                default: //��Ķζ�����////////////////
                    ps = F_Stream.ReadByte() * 256;
                    ps = F_Stream.Position + ps + F_Stream.ReadByte() - 2; //�Ӷγ���
                    break;
            }
            if (ps + 1 >= F_Stream.Length) //�ļ�����
            {
                F_Stream.Close();
                return size;
            }
            F_Stream.Position = ps; //�ƶ�ָ��
        } while (type != 0xda); // ɨ���п�ʼ
        F_Stream.Close();
        return size;
    }


    //fileStreamWrite(ref fs, "49 49 2A 0 8 0 0 0");
    //fileStreamWrite(ref fs, "16 0");//����22������
    //fileStreamWrite(ref fs, "FE 0 4 0 1 0 0 0 0 0 0 0");//tif����ͷ
    //fileStreamWrite(ref fs, "0 1 3 0 1 0 0 0 " + toHex2(iwidth, 4));//ͼƬ���
    //fileStreamWrite(ref fs, "1 1 3 0 1 0 0 0 " + toHex2(iheight, 4));//ͼƬ�߶�
    //fileStreamWrite(ref fs, "2 1 3 0 3 0 0 0 16 1 0 0");
    //fileStreamWrite(ref fs, "3 1 3 0 1 0 0 0 6 0 0 0");
    //fileStreamWrite(ref fs, "6 1 3 0 1 0 0 0 6 0 0 0");
    //fileStreamWrite(ref fs, "A 1 3 0 1 0 0 0 1 0 0 0");
    //fileStreamWrite(ref fs, "11 1 4 0 1 0 0 0 AE 3 0 0");
    //fileStreamWrite(ref fs, "12 1 3 0 1 0 0 0 1 0 0 0");
    //fileStreamWrite(ref fs, "15 1 3 0 1 0 0 0 3 0 0 0");
    //fileStreamWrite(ref fs, "16 1 4 0 1 0 0 0 " + toHex2(iheight, 4));
    //fileStreamWrite(ref fs, "17 1 4 0 1 0 0 0 " + toHex2(file.Length - 606, 4));
    //fileStreamWrite(ref fs, "1A 1 5 0 1 0 0 0 1C 1 0 0");
    //fileStreamWrite(ref fs, "1B 1 5 0 1 0 0 0 24 1 0 0");
    //fileStreamWrite(ref fs, "28 1 3 0 1 0 0 0 2 0 0 0");
    //fileStreamWrite(ref fs, "0 2 3 0 1 0 0 0 1 0 0 0");
    //fileStreamWrite(ref fs, "1 2 4 0 1 0 0 0 50 1 0 0");
    //fileStreamWrite(ref fs, "2 2 4 0 1 0 0 0 " + toHex2(file.Length, 4));
    //fileStreamWrite(ref fs, "7 2 4 0 3 0 0 0 2C 1 0 0");
    //fileStreamWrite(ref fs, "8 2 4 0 3 0 0 0 38 1 0 0");
    //fileStreamWrite(ref fs, "9 2 4 0 3 0 0 0 44 1 0 0");
    //fileStreamWrite(ref fs, "12 2 3 0 2 0 0 0 2 0 2 0");

    //fileStreamWrite(ref fs, "0 0 0 0 8 0 8 0 8 0 ");
    //fileStreamWrite(ref fs, "C8 0 0 0 1 0 0 0");
    //fileStreamWrite(ref fs, "C8 0 0 0 1 0 0 0");
    //fileStreamWrite(ref fs, "89 1 0 0 89 1 0 0");
    //fileStreamWrite(ref fs, "89 1 0 0 F 2 0 0");
    //fileStreamWrite(ref fs, "DF 2 0 0 DF 2 0 0");
    //fileStreamWrite(ref fs, "2C 2 0 0 FC 2 0 0");
    //fileStreamWrite(ref fs, "FC 2 0 0");
    void fileStreamWrite(ref FileStream fs, string str)
    {
        string[] arr = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < arr.Length; i++)
        {
            int ibyte = Convert.ToInt32(arr[i], 16);
            //MessageBox.Show(ibyte + "");
            fs.WriteByte((byte)ibyte);
        }
    }

    string toHex2(long it, int iwei)
    {
        string hex = Convert.ToString(it, 16).ToUpper();
        int ilength = hex.Length;
        for (int i = 0; i < iwei * 2 - ilength; i++)
        {
            hex = "0" + hex;
        }

        string newhex = "";
        for (int i = 0; i < hex.Length; i++)
        {
            if (i % 2 == 0)
            {
                newhex += " ";
            }
            newhex += hex[i].ToString();
        }

        string[] arr = newhex.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        Array.Reverse(arr);
        newhex = AciCvt.Array_to_Str(arr, " ");
        return newhex;
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
}

using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

public class saveImage
{
    ///jpg压缩的 tif
    //saveImage imgSaveTif = new saveImage();
    //imgSaveTif.setQuality(50);
    //string sfile2 = @"C:\Users\wangjia\Desktop\0001022\0000003A_临时.jpg";
    //Bitmap bmp = (Bitmap)System.Drawing.Image.FromFile(sfile);
    //imgSaveTif.toTwo(ref bmp);
    //imgSaveTif.Save(bmp, sfile2);
    //imgSaveTif.writeTiffInfo(sfile2);
    //File.Delete(sfile);
    //File.Move(sfile2, sfile);

    ///黑白二值化 tif
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
        //图片质量
        quality = new long[] { iQ };
        EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
        encoderParams.Param[0] = encoderParam;
    }
    void setDefault()
    {
        encoderParams = new EncoderParameters();
        long[] quality;

        //图片质量
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

    public unsafe void toTwo(ref Bitmap bmp)
    {
        BitmapData bitData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        //取出每行多出来的数
        uint ic = (uint)(bitData.Stride - bitData.Width * 3);

        //取出全部像素值 每隔 3个  为  R G B 每次循环删除 一个多数的 xx 数
        byte* p = (byte*)(bitData.Scan0.ToPointer());

        uint w = (uint)bitData.Width;
        uint h = (uint)bitData.Height;

        //一行的像素点
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
                //起始点

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
        ////开始二值化
        toTwo(p, ik, w, h, iWidth);
        bmp.UnlockBits(bitData);
    }
    /// <summary>
    /// 二值化
    /// </summary>
    /// <returns></returns>
    public unsafe void toTwo(byte* p, uint Ostu, uint w, uint h, uint iWidth)
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
    /// 二值化
    /// </summary>
    /// <returns></returns>
    public unsafe void toTwo(byte* p, uint Ostu, uint ibX, uint ieX, uint ibY, uint ieY, uint iWidth)
    {
        for (uint y = ibY; y < ieY; y++)
        {
            for (uint x = ibX; x < ieX; x++)
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

    public unsafe void toTwo(ref Bitmap bmp, uint icutX, uint icutY, bool isGrey)
    {
        BitmapData bitData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        if (bmp.Width > bmp.Height)
        {
            uint ii = icutX;
            icutX = icutY;
            icutY = ii;
        }

        //取出每行多出来的数
        uint ic = (uint)(bitData.Stride - bitData.Width * 3);

        //取出全部像素值 每隔 3个  为  R G B 每次循环删除 一个多数的 xx 数
        byte* p = (byte*)(bitData.Scan0.ToPointer());

        uint w = (uint)bitData.Width;
        uint h = (uint)bitData.Height;

        //一行的像素点
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
                        //起始点

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
                ////开始二值化
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
            saveFile = to_filename + "临时";
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
                fileStreamWrite(ref fs, "1E 00 00 00");//第一个IDF偏移量
                fileStreamWrite(ref fs, sdpi + " 00 00 01 00 00 00");//水平分辨率  C8 00 200dpi  2C 01 300dpi
                fileStreamWrite(ref fs, sdpi + " 00 00 01 00 00 00");//垂直分辨率  C8 00 200dpi  2C 01 300dpi
                fileStreamWrite(ref fs, "08 00 08 00 08 00");
            }

            //AciDebug.Debug(iwidth + "");
            FileStream fsRead = new FileStream(filename, FileMode.Open);
            if (fsRead.ReadByte() == 255)
            {
                fileStreamWrite(ref fs, "0D 00");//共有13个属性
                fileStreamWrite(ref fs, "FE 00 04 00 01 00 00 00 00 00 00 00");//01.tif固定头属性
                fileStreamWrite(ref fs, "00 01 04 00 01 00 00 00 " + toHex2(iwidth, 4));//02.图像宽度 最后四个字节
                fileStreamWrite(ref fs, "01 01 04 00 01 00 00 00 " + toHex2(iheight, 4));//03.图像高度 最后四个字节
                fileStreamWrite(ref fs, "02 01 03 00 03 00 00 00 18 00 00 00");//04.颜色深度 01 单色 04 16色 08 256色  2个字节以上为真彩色
                fileStreamWrite(ref fs, "03 01 03 00 01 00 00 00 07 00 00 00");//05.图片是否压缩 05 压缩
                fileStreamWrite(ref fs, "06 01 03 00 01 00 00 00 06 00 00 00");//06.是否反色 01 反色 其他不反色
                fileStreamWrite(ref fs, "11 01 04 00 01 00 00 00 " + toHex2(30 + 162 * (i + 1) + iPrevLength, 4));//07.图像数据起始字节相对于文件开始处的位置 最后四个字节
                fileStreamWrite(ref fs, "15 01 03 00 01 00 00 00 03 00 00 00");//08.未知属性值=3
                fileStreamWrite(ref fs, "16 01 04 00 01 00 00 00 " + toHex2(iheight, 4));//09.图像有几行扫描线 等于图像高度 最后四个字节
                fileStreamWrite(ref fs, "17 01 04 00 01 00 00 00 " + toHex2(fsRead.Length - 1, 4));//10.图像总计字节数 文件长度-606 最后四个字节
                fileStreamWrite(ref fs, "1A 01 05 00 01 00 00 00 08  00 00 00");//11.水平分辨率 存放在第几个字节  C8 00 200dpi  2C 01 300dpi
                fileStreamWrite(ref fs, "1B 01 05 00 01 00 00 00 10 00 00 00");//12.垂直分辨率 存放在第几个字节
                fileStreamWrite(ref fs, "28 01 03 00 01 00 00 00 02 00 00 00");//13.未知属性值=2
                if (i != AL_file.Count - 1)
                {
                    fileStreamWrite(ref fs, toHex2(30 + 162 * (i + 1) + iPrevLength + fsRead.Length, 4));
                }
                else
                {
                    fileStreamWrite(ref fs, "00 00 00 00");
                }
                iPrevLength += fsRead.Length;

                //图像数据
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

        if (saveFile.IndexOf("临时") > -1)
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
    /// 获取图片大小
    /// </summary>
    /// <param name="FileName"></param>
    /// <returns></returns>
    public static jpgSizeDPI getJpgSizeDPI(string FileName)
    {
        //C#快速获取JPG图片大小及英寸分辨率
        int rx = 0;
        jpgSizeDPI size = new jpgSizeDPI();
        if (!File.Exists(FileName)) return size;
        FileStream F_Stream = File.OpenRead(FileName);
        int ff = F_Stream.ReadByte();
        int type = F_Stream.ReadByte();
        if (ff != 0xff || type != 0xd8)
        {//非JPG文件
            F_Stream.Close();
            return size;
        }
        long ps = 0;
        do
        {
            do
            {
                ff = F_Stream.ReadByte();
                if (ff < 0) //文件结束
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
                case 0xc0: //SOF0段
                    ps = F_Stream.ReadByte() * 256;
                    ps = F_Stream.Position + ps + F_Stream.ReadByte() - 2; //加段长度

                    F_Stream.ReadByte(); //丢弃精度数据
                    //高度
                    //高度
                    int ih = F_Stream.ReadByte() * 256;
                    ih = ih + F_Stream.ReadByte();
                    size.sizeWH.Height = ih;

                    //宽度
                    int iw = F_Stream.ReadByte() * 256;
                    iw = iw + F_Stream.ReadByte();
                    size.sizeWH.Width = iw;

                    F_Stream.Close();
                    return size;
                    //后面信息忽略
                    //if (rx != 1 && rx < 3) rx = rx + 1;
                    break;
                case 0xe0: //APP0段
                    ps = F_Stream.ReadByte() * 256;
                    ps = F_Stream.Position + ps + F_Stream.ReadByte() - 2; //加段长度

                    F_Stream.Seek(5, SeekOrigin.Current); //丢弃APP0标记(5bytes)
                    F_Stream.Seek(2, SeekOrigin.Current); //丢弃主版本号(1bytes)及次版本号(1bytes)
                    int units = F_Stream.ReadByte(); //X和Y的密度单位,units=0：无单位,units=1：点数/英寸,units=2：点数/厘米

                    //水平方向(像素/英寸)分辨率
                    int Wpx = F_Stream.ReadByte() * 256;
                    Wpx = Wpx + F_Stream.ReadByte();
                    if (units == 2) Wpx = (int)Math.Round((Wpx * 2.54)); //厘米变为英寸
                    //垂直方向(像素/英寸)分辨率
                    int Hpx = F_Stream.ReadByte() * 256;
                    Hpx = Hpx + F_Stream.ReadByte();
                    if (units == 2) Hpx = (int)Math.Round((Hpx * 2.54)); //厘米变为英寸

                    size.sizeDPI.Width = Wpx;
                    size.sizeDPI.Height = Hpx;

                    //后面信息忽略
                    if (rx != 2 && rx < 3) rx = rx + 2;
                    break;

                default: //别的段都跳过////////////////
                    ps = F_Stream.ReadByte() * 256;
                    ps = F_Stream.Position + ps + F_Stream.ReadByte() - 2; //加段长度
                    break;
            }
            if (ps + 1 >= F_Stream.Length) //文件结束
            {
                F_Stream.Close();
                return size;
            }
            F_Stream.Position = ps; //移动指针
        } while (type != 0xda); // 扫描行开始
        F_Stream.Close();
        return size;
    }


    //fileStreamWrite(ref fs, "49 49 2A 0 8 0 0 0");
    //fileStreamWrite(ref fs, "16 0");//共计22个属性
    //fileStreamWrite(ref fs, "FE 0 4 0 1 0 0 0 0 0 0 0");//tif属性头
    //fileStreamWrite(ref fs, "0 1 3 0 1 0 0 0 " + toHex2(iwidth, 4));//图片宽度
    //fileStreamWrite(ref fs, "1 1 3 0 1 0 0 0 " + toHex2(iheight, 4));//图片高度
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
}

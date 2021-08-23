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
    /// <summary>核心数据阀值</summary>
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

    //一行的像素点
    uint iWidth = 0;
    uint r = 0;
    uint g = 0;
    uint b = 0;
    /// <summary>
    /// 灰度计算的有效范围 默认为 80
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


        //图片质量
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
            SrcP = (byte*)SrcData.Scan0 + Y * SrcStride;         // 必须在某个地方开启unsafe功能，其实C#中的unsafe很safe，搞的好吓人。            
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

    public bool b是否深图 = false;
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
            //设置缩放
            bitmap = new Bitmap(Key.width, (int)(Key.width / (double)img.Width * (double)img.Height));
            Graphics g = Graphics.FromImage(bitmap);
            g.DrawImage(img, 0, 0, bitmap.Width, bitmap.Height);
            SS = (bitmap.Width - (int)greyArea * 2) * (bitmap.Height - (int)greyArea * 2);
            g.Dispose();
            //bitmap.Save(file.FullName.Replace(".jpg", "_临时.jpg"), myImageCodecInfo, encoderParams);
            //bitmap.Dispose();
            //img.Dispose();

            //string file_img_0 = file.FullName.Replace(".jpg", "_临时.jpg");

            //bitmap = (Bitmap)System.Drawing.Image.FromFile(file_img_0);

            //获取直方图
            //Bitmap bmpGrey = ConvertToGrayBitmap(bitmap);
            //int[] HistGram = new int[256];
            //GetHistGram(bmpGrey, ref HistGram);
            //bmpGrey.Dispose();

            BitmapData bitData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            //取出每行多出来的数
            ic = (uint)(bitData.Stride - bitData.Width * 3);

            iP = 0;
            iAll = 0;

            //取出全部像素值 每隔 3个  为  R G B 每次循环删除 一个多数的 xx 数
            byte* p = (byte*)(bitData.Scan0.ToPointer());

            w = (uint)bitData.Width;
            h = (uint)bitData.Height;

            //一行的像素点
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
                    //起始点
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

            ////开始二值化
            toTwo(p, ik);

            if (show)
            {
                bitmap.UnlockBits(bitData);
                bitmap.Save(file.FullName.Replace(".jpg", "_" + imode + "_临时_b二值化.jpg"), myImageCodecInfo, encoderParams);
                bitData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            }

            ////连通区域
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
                bitmap.Save(file.FullName.Replace(".jpg", "_" + imode + "_临时_c去小区域后.jpg"), myImageCodecInfo, encoderParams);
                bitData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            }
            


            //toSmallLine(p, bitmap, file, -1);

            //if (!show) bitmap.Save(file.FullName.Replace(".jpg", "_" + imode + "_临时_骨骼化.jpg"), myImageCodecInfo, encoderParams);
            //if (!(myTable.iUpDot < 2000 || (myTable.iUpDot < 5000 && myTable.countRectAll < 10 && myTable.maxRect.iNum < 200)) && (myTable.maxRect.P < 200))
            //{
            //    ////细化骨骼化
            //    toSmallLine(p, bitmap, file, -1);

            //    if (show) bitmap.Save(file.FullName.Replace(".jpg", "_" + imode + "e.jpg"), myImageCodecInfo, encoderParams);

            //    //游程直线检测 并返回 直线数量
            //    myTable.iTableLine = (int)toDotLine(p, file);

                

            //    if (show) bitmap.Save(file.FullName.Replace(".jpg", "_" + imode + "f.jpg"), myImageCodecInfo, encoderParams);
            //}

            //outTime("完成");

            bitmap.UnlockBits(bitData);
            
            bitmap.Dispose();
            img.Dispose();

            //File.Delete(file_img_0);
            
        }

        return isBai;
    }
    /// <summary>
    /// 设置阀值
    /// </summary>
    /// <param name="Htg">直方图结构体</param>
    /// <returns></returns>
    public uint setThreshold(OcrStruct.Histogram Htg)
    {
        ////Ostu 全局二值算法
        ////获得最大类间方差
        //uint Oavg = 0;
        uint Ostu = getOstu(Htg.pixelNum);
        //uint Oshang = getShang(Htg.pixelNum);

        //System.Windows.Forms.MessageBox.Show(Ostu + "");

        return Ostu;
    }

    /// <summary>
    /// 直方图
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
                    //起始点
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

        //flash查看直方图
        //createtxt(pixelNum);

        return new OcrStruct.Histogram(pixelNum, maxK);
    }

    

    /// <summary>
    /// 游程直线检测 并返回倾斜角度
    /// </summary>
    public unsafe float toDotLine(byte* p, FileInfo file)
    {
        //标记 为1时说明已经访问过
        uint[,] arrP = new uint[w, h];

        //bitmap.Save(file.FullName.Replace(".jpg", "_b4.jpg"), myImageCodecInfo, encoderParams);

        myDot d_up;//上点 -1
        myDot d_next;//方向点 0
        myDot d_down;//下点 1
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
                    //转向点的数量
                    Al_save = new ArrayList();

                    //左方向 +1 方向
                    int iNext = -1;

                    myDot d1 = new myDot(x - 1, y, iWidth, 0);
                    myDot d2 = new myDot(x + 1, y, iWidth, 0);


                    if (p[d1.ib] != 0 && p[d2.ib] != 0)
                    {
                        //左右两边一个黑点也没有  直接清除
                        p[mydot.ib] = p[mydot.ib + 1] = p[mydot.ib + 2] = 255;
                    }
                    else
                    {
                        //左右两边至少有一个黑点
                        while (dot != null && (int)dot.x + iNext > 0 && (int)dot.x + iNext < w - 1)
                        {
                            line.Add(dot);

                            d_up = new myDot(dot.x, dot.y - 1, iWidth, -1);
                            //不是下点时，取方向点和上点
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
                            //不是上点时，取方向点和下点
                            if (dot.y + 1 < h)
                            {
                                if (dot.type != -1)
                                {
                                    d_down.c = -(p[d_down.ib] / 255 - 1);
                                }
                            }


                            //标记
                            arrP[dot.x, dot.y] = 1;
                            

                            int iNc = d_up.c + d_next.c + d_down.c;

                            //AciDebug.Debug(dot.type + " " + x + " " + y + " " + d_up.c + " " + d_next.c + " " + d_down.c);

                            //方向3点上只有一个点
                            if (iNc == 1)
                            {
                                //且点在方向上 那么直接下一个点
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
                                        //存储上点
                                        Al_save.Add(d_up);
                                    }
                                    else
                                    {
                                        dot = d_down;
                                        //存储下点
                                        Al_save.Add(d_down);
                                    }
                                    //当连续折点前进2格后 【中断】
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
                                //方向一个点都没有时，【中断】
                                Al_save.Clear();
                                dot = null;
                            }
                            else
                            {
                                //方向3点上有两个点及以上的黑点【叉路】
                                if (d_next.c == 1)
                                {
                                    //当方向点为黑色时 选择方向点 并把【叉路断开】

                                    //【断开叉路】
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
                                    //当方向点为白色，上下点为黑色时 断开当前点【中断】
                                    p[dot.ib] = p[dot.ib + 1] = p[dot.ib + 2] = 255;
                                    Al_save.Clear();
                                    dot = null;
                                }
                            }
                            //右方向
                            if (iNext == -1 && dot == null)
                            {
                                mydot = new myDot(x + 1, y, iWidth, 0);
                                r = p[mydot.ib];
                                if (r == 0)
                                {
                                    iNext = 1;
                                    dot = mydot;
                                }
                                //AciDebug.Debug("转向");
                            }
                        }
                        //AciDebug.Debug("a " + line.iNum);
                        //假如线段小于10点就去掉
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

        //取x方向质心
        int gX = getGravityX(p);
        //以图像中轴位算求 Y 即 b
        for (int i = 0; i < Al_line.Count; i++)
        {
            Line line = (Line)Al_line[i];
            line.leastSquare(gX);
        }

        toDotLineRotate(p, file, Al_line);

        int[] Y = new int[h];
        /// 向 x=0 方向集结
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
        //断线容断值
        int iDuan = 3;
        int iDuanNow = 0;
        ArrayList AL_Y = new ArrayList();

        int iNumber = 0;

        for (int i = 0; i < Y.Length; i++)
        {
            //大于十个点有效
            if (Y[i] > Key.toDotLine_T)
            {
                //连续累计
                iNumber += Y[i];
            }
            else
            {
                //增加断值
                iDuanNow++;
                //当大于容断值
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
        

        //AciDebug.Debug(file.Name + "线数：" + AL_Y.Count);
        return AL_Y.Count;
    }
    /// <summary>
    /// 取重心点
    /// </summary>
    public unsafe int getGravityX(byte* p)
    {
        //取全部黑点像素
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
    /// 游程直线检测 后做倾斜校正
    /// </summary>
    public unsafe float toDotLineRotate(byte* p, FileInfo file, ArrayList Al_line)
    {
        //检查倾斜角度
        float K = 0;
        int iall = 0;
        for (int i = 0; i < Al_line.Count; i++)
        {
            Line line = (Line)Al_line[i];

            //大于50点的才加入权重
            if (line.iNum > Key.toDotLine_R_1)
            {
                //假如线段的斜率大于1

                //斜率小于15度的直线
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
            //倾斜角度
            K /= iall;
            R = K / (float)Math.PI * 180;
            //AciDebug.Debug("倾斜角度 " + R);

            //旋转图像
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
    /// 细化骨骼化
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
    /// 细化骨骼化细节实现
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
                    //计算Z0(P)

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
        //AciDebug.Debug(index + "-1-次数：" + icheck + " " + ts.TotalMilliseconds);

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
            //计算Z0(P)

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
    /// 细化骨骼化细节实现
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
                    //计算Z0(P)

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
        //AciDebug.Debug(index + "-1-次数：" + icheck + " " + ts.TotalMilliseconds);

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
            //计算Z0(P)

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
                        //不超出边界
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
    /// 联通区域分割算法
    /// </summary>
    public unsafe ArrayList cutArea(byte* p, FileInfo file)
    {
        //MessageBox.Show("a");

        Rect rect;
        Dot point;
        uint dot = 0;
        //标记
        uint[,] arrP = new uint[w, h];
        //联通锥
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
                    //基点入锥
                    Al_sp = new ArrayList();

                    arrP[x, y] = 1;
                    Al_sp.Add(new Dot(x, y));

                    rect = new Rect(y, x + 1, y + 1, x);

                    while (Al_sp.Count != 0)
                    {
                        //取第一个点计算4方向
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
                            //不超出边界
                            if (thisx >= 0 && thisx < w && thisy >= 0 && thisy < h)
                            {
                                dot = p[thisy * iWidth + thisx * 3];

                                if (dot == 0 && arrP[thisx, thisy] == 0)
                                {
                                    //新点入锥
                                    arrP[thisx, thisy] = 1;
                                    Al_sp.Add(new Dot(thisx, thisy));
                                    rect.Add(new myDot(thisx, thisy, iWidth, 0));
                                    //重新设置矩形大小
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
                        //出锥
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
                            //装订孔  特殊计算
                            if ((rect.r < greyArea4 && rect.b < h / 7 * 6 && rect.t > h / 7)
                                || (rect.l > w - greyArea4 && rect.b < h / 7 * 6 && rect.t > h / 7))
                            {
                                //装订孔  特殊计算
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
                //边缘点去除狭长
                if (rect.pp < 0.2f)
                {
                    if (show) rect.Clear(p);
                }
                else
                {
                    //四边要求提高
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
    /// 灰度化
    /// </summary>
    public unsafe int toGrey(byte* p)
    {
        double it = 0;
        for (uint y = 0; y < h; y++)
        {
            for (uint x = 0; x < w; x++)
            {
                //起始点
                uint ib = y * iWidth + x * 3;

                //取平均值
                r = p[ib];
                g = p[ib + 1];
                b = p[ib + 2];

                //灰度化
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
                //起始点
                uint ib = y * iWidth + x * 3;

                //取平均值
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
    /// 测试模板
    /// </summary>
    /// <param name="arrN">5*5维度的网格</param>
    /// <param name="template">3*3维度的网格</param>
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
    /// 测试模板
    /// </summary>
    /// <param name="arrN">3*3维度的网格</param>
    /// <param name="template">3*3维度的网格</param>
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
    /// 测试模板
    /// </summary>
    /// <param name="arrN">3*3维度的网格</param>
    /// <param name="template">3*3维度的网格</param>
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
    /// 返回某一点为中心的 N*N 的方格  如果超出边界 为 0, 黑色 为 1
    /// </summary>
    /// <param name="p"></param>
    /// <param name="x">中心点</param>
    /// <param name="y">中心点</param>
    /// <param name="n">矩阵的n  需要单数</param>
    /// <returns>n*n大小的数组 从上至下，从左至右</returns>
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
                //不超出边界
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
    public void outTime(string sName)
    {
        ts = DateTime.Now - dt;
        AciDebug.Debug(sName + "：" + ts.TotalMilliseconds);
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

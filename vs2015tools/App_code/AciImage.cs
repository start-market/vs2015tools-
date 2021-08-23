using System;
using System.Collections;
using System.Text;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

public class AciImage
{
    public AciImage()
    {
    }
    /// <summary>
    /// 强力删除图片 可设置重复次数
    /// </summary>
    /// <param name="filepath">删除的文件</param>
    public static void FileDelete(string filepath)
    {
        FileDelete(filepath, 5);
    }
    /// <summary>
    /// 强力移动图片 可设置重复次数
    /// </summary>
    /// <param name="oldfile">原始位置</param>
    /// <param name="newfile">新位置</param>
    public static void FileMove(string oldfile, string newfile)
    {
        FileMove(oldfile, newfile, 5);
    }
    /// <summary>
    /// 强力删除图片 可设置重复次数
    /// </summary>
    /// <param name="filepath">删除的文件</param>
    /// <param name="imax">重复删除次数</param>
    public static void FileDelete(string filepath, int imax)
    {
        int itime = 0;
        while (itime <= imax)
        {
            try
            {
                File.Delete(filepath);
                break;
            }
            catch
            {
                itime++;
                System.Threading.Thread.Sleep(50);
            }
        }
    }
    /// <summary>
    /// 强力移动图片 可设置重复次数
    /// </summary>
    /// <param name="oldfile">原始位置</param>
    /// <param name="newfile">新位置</param>
    /// <param name="imax">重复移动次数</param>
    public static void FileMove(string oldfile, string newfile, int imax)
    {
        int itime = 0;
        while (itime <= imax)
        {
            try
            {
                File.Move(oldfile, newfile);
                break;
            }
            catch
            {
                itime++;
                System.Threading.Thread.Sleep(50);
            }
        }
    }
    /// <summary>
    /// A3 超长连续页切割 仅先 A B 两页间
    /// </summary>
    /// <param name="al_file">需要切割的图片组</param>
    /// <param name="savedir">保存路径</param>
    /// <param name="smode">1,2,3,4 四页切割   1,2,3,4,5,6 六页切割 </param>
    public static void A3long_ByFile(ArrayList al_file, string savedir, string smode)
    {
        ArrayList al_img = new ArrayList();

        for (int i = 0; i < al_file.Count; i++)
        {
            Image image = Image.FromFile(savedir + "\\" + al_file[i].ToString());
            al_img.Add(image);
        }

        AciImage.A3long_Doing(al_file, al_img, savedir, smode);
    }

    public static void A3long_Doing(ArrayList AL_file, ArrayList AL_image, string sSaveDir, string smode)
    {
        string[] arr = smode.Split(',');


        if (AL_image.Count % 2 == 0 && arr.Length % 2 == 0)
        {
            int[] iArr = AciCvt.toIntArray(arr);
            //每面切割数
            int iCut = iArr.Length / 2;

            saveImage imgSave = new saveImage();

            for (int i = 0; i < AL_image.Count; i += 2)
            {
                Image imgA = (Image)AL_image[i];
                Image imgB = (Image)AL_image[i + 1];

                FileInfo fileA = (FileInfo)AL_file[i];
                FileInfo fileB = (FileInfo)AL_file[i + 1];

                //A面先横向旋转
                if (imgA.Height > imgA.Width)
                {
                    imgA.RotateFlip(RotateFlipType.Rotate90FlipNone);
                }
                //A面切割
                for (int n = 0; n < iCut; n++)
                {
                    int iw = imgA.Width / iCut;

                    Bitmap bmp = new Bitmap(iw, imgA.Height);
                    Graphics g = Graphics.FromImage(bmp);
                    g.Clear(Color.White);
                    g.DrawImage(imgA, -iw * n, 0, imgA.Width, imgA.Height);
                    g.Dispose();

                    string name = Path.GetFileNameWithoutExtension(AL_file[i].ToString());
                    imgSave.Save(bmp, sSaveDir + "\\" + name + "-" + iArr[n] + ".jpg");

                    bmp.Dispose();
                }

                //B面先横向旋转
                if (imgB.Height > imgB.Width)
                {
                    imgB.RotateFlip(RotateFlipType.Rotate270FlipNone);
                }
                //B面切割
                for (int n = 0; n < iCut; n++)
                {
                    int iw = imgB.Width / iCut;

                    Bitmap bmp = new Bitmap(iw, imgB.Height);
                    Graphics g = Graphics.FromImage(bmp);
                    g.Clear(Color.White);
                    g.DrawImage(imgB, -iw * n, 0, imgB.Width, imgB.Height);
                    g.Dispose();

                    string name = Path.GetFileNameWithoutExtension(AL_file[i].ToString());
                    imgSave.Save(bmp, sSaveDir + "\\" + name + "-" + iArr[n + iCut] + ".jpg");

                    bmp.Dispose();
                }
                imgA.Dispose();
                fileA.Delete();
                imgB.Dispose();
                fileB.Delete();
            }
        }
        else
        {
            for (int i = 0; i < AL_image.Count; i++)
            {
                Image image = (Image)AL_image[i];
                image.Dispose();
            }
        }
    }

    /// <summary>
    /// A3切割
    /// </summary>
    /// <param name="al_file">需要切割的图片组</param>
    /// <param name="savedir">保存路径</param>
    /// <param name="imode"> 0 正向  1 反向</param>
    public static void A3book_ByFile(ArrayList al_file, string savedir, int imode)
    {
        ArrayList al_img = new ArrayList();

        for (int i = 0; i < al_file.Count; i++)
        {
            Image image = Image.FromFile(savedir + "\\" + al_file[i].ToString());
            al_img.Add(image);
        }

        AciImage.A3book_Doing(al_file, al_img, savedir, imode);
    }
    /// <summary>
    /// 书本切割
    /// </summary>
    /// <param name="AL_file">切割的图片组</param>
    /// <param name="AL_image"></param>
    /// <param name="sSaveDir">保存路径</param>
    /// <param name="imode">0正向 1反向</param>
    public static void A3book_Doing(ArrayList AL_file, ArrayList AL_image, string sSaveDir, int imode)
    {
        if (AL_image.Count % 2 == 0)
        {
            saveImage imgSave = new saveImage();
            for (int i = 0; i < AL_image.Count; i++)
            {
                //AciDebug.Debug(i.ToString());

                Image img = (Image)AL_image[i];

                //上半边图
                Bitmap bmp = new Bitmap(img.Width, img.Height / 2, PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(bmp);

                if (imode == 0)
                {
                    g.DrawImage(img, 0, 0, img.Width, img.Height);
                }
                else
                {
                    g.DrawImage(img, 0, -img.Height / 2, img.Width, img.Height);
                }
                g.Dispose();



                int index = i / 2;

                string snum = "1";

                if (imode == 0)
                {
                    if (i % 2 == 1)
                    {
                        snum = "2";
                        bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    }
                    else
                    {
                        bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    }
                }
                else
                {
                    if (i % 2 == 1)
                    {
                        snum = "2";
                        bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    }
                    else
                    {
                        bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    }
                }

                string spath = sSaveDir + "\\" + Path.GetFileNameWithoutExtension(AL_file[index].ToString()) + "-" + snum + ".jpg";
                imgSave.Save(bmp, spath);
                bmp.Dispose();

                //AciDebug.Debug(index + "-" + snum + " " + spath);


                //下半边图
                bmp = new Bitmap(img.Width, img.Height / 2, PixelFormat.Format32bppArgb);
                g = Graphics.FromImage(bmp);
                if (imode == 0)
                {
                    g.DrawImage(img, 0, -img.Height / 2, img.Width, img.Height);
                }
                else
                {
                    g.DrawImage(img, 0, 0, img.Width, img.Height);
                }
                g.Dispose();

                index = (AL_image.Count - (i / 2) - 1);

                snum = "1";

                if (imode == 0)
                {
                    if (i % 2 == 0)
                    {
                        snum = "2";
                        bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    }
                    else
                    {
                        bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    }
                }
                else
                {
                    if (i % 2 == 0)
                    {
                        snum = "2";
                        bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    }
                    else
                    {
                        bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    }
                }

                spath = sSaveDir + "\\" + Path.GetFileNameWithoutExtension(AL_file[index].ToString()) + "-" + snum + ".jpg";
                imgSave.Save(bmp, spath);
                bmp.Dispose();

                img.Dispose();
                //AciDebug.Debug(index + "-" + snum + " " + spath);

                File.Delete(sSaveDir + "\\" + AL_file[i]);

                //AciDebug.Debug("");
            }

            //System.Windows.Forms.MessageBox.Show(sSaveDir + " 转换完成");
        }
        else
        {
            for (int i = 0; i < AL_image.Count; i++)
            {
                Image image = (Image)AL_image[i];
                image.Dispose();
            }
        }
    }

    /// <summary>
    /// 检查文件夹文件错误情况 【必须是】 image0000001A image0000001B 格式
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static string FileName_checkDir(DirectoryInfo dir)
    {
        string sWorry = "";
        //判断是否有子文件夹
        if (dir.GetDirectories().Length > 0)
        {
            sWorry = "发现子文件夹 " + dir.FullName + " ";
            return sWorry;
        }

        FileInfo[] files = dir.GetFiles("*.jpg", SearchOption.TopDirectoryOnly);

        AciImage.FileName_ReSort(ref files);

        for (int n = 0; n < files.Length; n++)
        {

            string filename = Path.GetFileNameWithoutExtension(files[n].Name).ToLower();
            //判断文件名长度
            if (filename.Length < 13)
            {
                sWorry = files[n].FullName + " 文件名长度大于等于 13位";
                return sWorry;
            }
            //判断是否有image
            if (filename.IndexOf("image") != 0)
            {
                sWorry = files[n].FullName + " 文件名格式错误：没有image";
                return sWorry;
            }

            filename = filename.Substring(5, filename.Length - 5);
            string sNum = filename.Substring(0, 7);//0000001
            string sAB = filename.Substring(7, 1);//A or B
            string sOther = "";//-1 or 副本 等其他部分
            if (filename.Length > 8)
            {
                sOther = filename.Substring(8, filename.Length - 8);
            }

            if (!AciReg.isInteger(sNum))
            {
                sWorry = files[n].FullName + " 文件名格式错误：image 后不是 7位数字";
                return sWorry;
            }

            string sOkAB = "b";
            if (n % 2 == 0)
            {
                sOkAB = "a";
            }
            int iNum = AciCvt.ToInt(sNum);
            int iOkNum = n / 2 + 1;
            if (iNum != iOkNum)
            {
                sWorry = files[n].FullName + " 文件名错误：应当为 " + AciMath.setNumberLength(iOkNum, 7) + sOkAB + " 实际为 " + AciMath.setNumberLength(iNum, 7) + sAB + "";
                return sWorry;
            }


            if (sAB != sOkAB)
            {
                sWorry = files[n].FullName + " 文件名错误：应当为 " + AciMath.setNumberLength(iOkNum, 7) + sOkAB + " 实际为 " + AciMath.setNumberLength(iNum, 7) + sAB + "";
                return sWorry;
            }

            if (sOther != "")
            {
                //if (sOther.IndexOf("-") != 0)
                //{
                //    sWorry = files[n].FullName + " 文件出现错误后缀：【" + sOther + "】";
                //    return sWorry;
                //}
                //sOther = sOther.Substring(1, sOther.Length - 1);
                //if (!AciReg.isInteger(sOther))
                //{
                //    sWorry = files[n].FullName + " 文件出现错误后缀：【" + sOther + "】";
                //    return sWorry;
                //}
                sWorry = files[n].FullName + " 文件出现错误后缀：【" + sOther + "】";
                return sWorry;
            }
        }

        return "";
    }

    /// <summary>
    /// 检查文件夹文件错误情况 【非】 image0000001A image0000001B 格式
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static string FileName_checkDir2(DirectoryInfo dir)
    {
        string sWorry = "";
        //判断是否有子文件夹
        if (dir.GetDirectories().Length > 0)
        {
            sWorry = "发现子文件夹 " + dir.FullName + " ";
            return sWorry;
        }

        FileInfo[] files = dir.GetFiles("*.jpg", SearchOption.TopDirectoryOnly);

        AciImage.FileName_ReSort(ref files);

        for (int n = 0; n < files.Length; n++)
        {

            string filename = Path.GetFileNameWithoutExtension(files[n].Name).ToLower();
            //判断文件名长度
            if (filename.Length < 5)
            {
                sWorry = files[n].FullName + " 文件名长度大于等于 5 位";
                return sWorry;
            }

            if (filename.IndexOf("副本") > -1)
            {
                sWorry = files[n].FullName + " 文件出现错误后缀：【发现副本】";
                return sWorry;
            }
        }

        return "";
    }

    /// <summary>
    /// 重命名所有文件夹中的文件
    /// </summary>
    /// <param name="dir"></param>
    public static void FileName_reNameDir(DirectoryInfo dir)
    {
        FileInfo[] files = dir.GetFiles("*.jpg", SearchOption.TopDirectoryOnly);
        AciImage.FileName_ReSort(ref files);

        DirectoryInfo dirMove = new DirectoryInfo(dir.FullName + "\\move");
        if (!dirMove.Exists)
        {
            dirMove.Create();
            //MessageBox.Show("1");

            for (int n = 0; n < files.Length; n++)
            {
                string filename = Path.GetFileNameWithoutExtension(files[n].Name);
                string rightName = "image" + AciMath.setNumberLength(n / 2 + 1, 7);
                if (n % 2 == 0)
                {
                    rightName += "A";
                }
                else
                {
                    rightName += "B";
                }
                files[n].MoveTo(dirMove.FullName + "\\" + rightName + ".jpg");
            }

            //MessageBox.Show("2");

            if (files.Length % 2 == 1)
            {
                //落单创建一张白色图片
                AciImage.createImageWhite(100, 100, dirMove.FullName + "\\image" + AciMath.setNumberLength((files.Length / 2 + 1), 7) + "B.jpg");
            }

            files = dirMove.GetFiles("*.jpg", SearchOption.TopDirectoryOnly);
            for (int n = 0; n < files.Length; n++)
            {
                files[n].MoveTo(dirMove.Parent.FullName + "\\" + files[n].Name);
            }


            //MessageBox.Show("3");
            dirMove.Delete();

            //MessageBox.Show("4");
        }
    }

    /// <summary>
    /// 创建一张白色的图片
    /// </summary>
    /// <param name="iw">宽</param>
    /// <param name="ih">高</param>
    /// <param name="savePath">保存路径</param>
    /// <returns></returns>
    public static void createImageWhite(int iw, int ih, string savePath)
    {
        Bitmap bmp = new Bitmap(iw, ih);
        Graphics g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        g.Dispose();

        saveImage imgSave = new saveImage();
        imgSave.Save(bmp, savePath);

        bmp.Dispose();
    }


    /// <summary>
    /// 按文件名排序
    /// </summary>
    /// <param name="arrFile"></param>
    public static void FileName_ReSort(ref FileInfo[] arrFile)
    {
        Array.Sort(arrFile, new myFileSorter());
    }
    /// <summary>
    /// 按文件名排序
    /// </summary>
    /// <param name="arrFile"></param>
    public static void DirName_ReSort(ref DirectoryInfo[] arrDir)
    {
        Array.Sort(arrDir, new myDirSorter());
    }

    //public static void 


    public static int getJpgSizeW(string FileName)
    {
        //C#快速获取JPG图片大小及英寸分辨率
        int rx = 0;
        if (!File.Exists(FileName)) return rx;
        FileStream F_Stream = File.OpenRead(FileName);
        int ff = F_Stream.ReadByte();
        int type = F_Stream.ReadByte();
        if (ff != 0xff || type != 0xd8)
        {//非JPG文件
            F_Stream.Close();
            return rx;
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
                    return rx;
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
                    F_Stream.ReadByte();
                    F_Stream.ReadByte();
                    //宽度
                    int iw = F_Stream.ReadByte() * 256;
                    iw = iw + F_Stream.ReadByte();

                    F_Stream.Close();
                    return iw;
                    //后面信息忽略
                    //if (rx != 1 && rx < 3) rx = rx + 1;
                    break;
                //case 0xe0: //APP0段
                //ps = F_Stream.ReadByte() * 256;
                //ps = F_Stream.Position + ps + F_Stream.ReadByte() - 2; //加段长度

                //F_Stream.Seek(5, SeekOrigin.Current); //丢弃APP0标记(5bytes)
                //F_Stream.Seek(2, SeekOrigin.Current); //丢弃主版本号(1bytes)及次版本号(1bytes)
                //int units = F_Stream.ReadByte(); //X和Y的密度单位,units=0：无单位,units=1：点数/英寸,units=2：点数/厘米

                ////水平方向(像素/英寸)分辨率
                //Wpx = F_Stream.ReadByte() * 256;
                //Wpx = Wpx + F_Stream.ReadByte();
                //if (units == 2) Wpx = (float)(Wpx * 2.54); //厘米变为英寸
                //垂直方向(像素/英寸)分辨率
                //Hpx = F_Stream.ReadByte() * 256;
                //Hpx = Hpx + F_Stream.ReadByte();
                //if (units == 2) Hpx = (float)(Hpx * 2.54); //厘米变为英寸
                //后面信息忽略
                //if (rx != 2 && rx < 3) rx = rx + 2;
                //break;

                default: //别的段都跳过////////////////
                    ps = F_Stream.ReadByte() * 256;
                    ps = F_Stream.Position + ps + F_Stream.ReadByte() - 2; //加段长度
                    break;
            }
            if (ps + 1 >= F_Stream.Length) //文件结束
            {
                F_Stream.Close();
                return rx;
            }
            F_Stream.Position = ps; //移动指针
        } while (type != 0xda); // 扫描行开始
        F_Stream.Close();
        return rx;
    }

    public static int getJpgSizeH(string FileName)
    {
        //C#快速获取JPG图片大小及英寸分辨率
        int rx = 0;

        if (!File.Exists(FileName)) return rx;
        FileStream F_Stream = File.OpenRead(FileName);
        int ff = F_Stream.ReadByte();
        int type = F_Stream.ReadByte();
        if (ff != 0xff || type != 0xd8)
        {//非JPG文件
            F_Stream.Close();
            return rx;
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
                    return rx;
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
                    int ih = F_Stream.ReadByte() * 256;
                    ih = ih + F_Stream.ReadByte();

                    F_Stream.ReadByte();
                    F_Stream.ReadByte();

                    F_Stream.Close();
                    return ih;
                    //后面信息忽略
                    //if (rx != 1 && rx < 3) rx = rx + 1;
                    break;
                //case 0xe0: //APP0段
                //ps = F_Stream.ReadByte() * 256;
                //ps = F_Stream.Position + ps + F_Stream.ReadByte() - 2; //加段长度

                //F_Stream.Seek(5, SeekOrigin.Current); //丢弃APP0标记(5bytes)
                //F_Stream.Seek(2, SeekOrigin.Current); //丢弃主版本号(1bytes)及次版本号(1bytes)
                //int units = F_Stream.ReadByte(); //X和Y的密度单位,units=0：无单位,units=1：点数/英寸,units=2：点数/厘米

                ////水平方向(像素/英寸)分辨率
                //Wpx = F_Stream.ReadByte() * 256;
                //Wpx = Wpx + F_Stream.ReadByte();
                //if (units == 2) Wpx = (float)(Wpx * 2.54); //厘米变为英寸
                //垂直方向(像素/英寸)分辨率
                //Hpx = F_Stream.ReadByte() * 256;
                //Hpx = Hpx + F_Stream.ReadByte();
                //if (units == 2) Hpx = (float)(Hpx * 2.54); //厘米变为英寸
                //后面信息忽略
                //if (rx != 2 && rx < 3) rx = rx + 2;
                //break;

                default: //别的段都跳过////////////////
                    ps = F_Stream.ReadByte() * 256;
                    ps = F_Stream.Position + ps + F_Stream.ReadByte() - 2; //加段长度
                    break;
            }
            if (ps + 1 >= F_Stream.Length) //文件结束
            {
                F_Stream.Close();
                return rx;
            }
            F_Stream.Position = ps; //移动指针
        } while (type != 0xda); // 扫描行开始
        F_Stream.Close();
        return rx;
    }
    /// <summary>
    /// 获取图片大小
    /// </summary>
    /// <param name="FileName"></param>
    /// <returns></returns>
    public static Size getJpgSize(string FileName)
    {
        //C#快速获取JPG图片大小及英寸分辨率
        int rx = 0;
        Size size = new Size();
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
                    size.Height = ih;

                    //宽度
                    int iw = F_Stream.ReadByte() * 256;
                    iw = iw + F_Stream.ReadByte();
                    size.Width = iw;

                    F_Stream.Close();
                    return size;
                    //后面信息忽略
                    //if (rx != 1 && rx < 3) rx = rx + 1;
                    break;
                //case 0xe0: //APP0段
                //ps = F_Stream.ReadByte() * 256;
                //ps = F_Stream.Position + ps + F_Stream.ReadByte() - 2; //加段长度

                //F_Stream.Seek(5, SeekOrigin.Current); //丢弃APP0标记(5bytes)
                //F_Stream.Seek(2, SeekOrigin.Current); //丢弃主版本号(1bytes)及次版本号(1bytes)
                //int units = F_Stream.ReadByte(); //X和Y的密度单位,units=0：无单位,units=1：点数/英寸,units=2：点数/厘米

                ////水平方向(像素/英寸)分辨率
                //Wpx = F_Stream.ReadByte() * 256;
                //Wpx = Wpx + F_Stream.ReadByte();
                //if (units == 2) Wpx = (float)(Wpx * 2.54); //厘米变为英寸
                //垂直方向(像素/英寸)分辨率
                //Hpx = F_Stream.ReadByte() * 256;
                //Hpx = Hpx + F_Stream.ReadByte();
                //if (units == 2) Hpx = (float)(Hpx * 2.54); //厘米变为英寸
                //后面信息忽略
                //if (rx != 2 && rx < 3) rx = rx + 2;
                //break;

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
    /// <summary>
    /// 获取图片大小
    /// </summary>
    /// <param name="FileName"></param>
    /// <returns></returns>
    public static Size getJpgDPI(string FileName)
    {
        //C#快速获取JPG图片大小及英寸分辨率
        int rx = 0;
        Size size = new Size();
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
                    //ps = F_Stream.ReadByte() * 256;
                    //ps = F_Stream.Position + ps + F_Stream.ReadByte() - 2; //加段长度

                    //F_Stream.ReadByte(); //丢弃精度数据
                    ////高度
                    ////高度
                    //int ih = F_Stream.ReadByte() * 256;
                    //ih = ih + F_Stream.ReadByte();
                    ////size.Height = ih;

                    ////宽度
                    //int iw = F_Stream.ReadByte() * 256;
                    //iw = iw + F_Stream.ReadByte();
                    ////size.Width = iw;

                    //F_Stream.Close();
                    //return size;
                    ////后面信息忽略
                    //if (rx != 1 && rx < 3) rx = rx + 1;
                    break;
                case 0xe0: //APP0段
                    ps = F_Stream.ReadByte() * 256;
                    ps = F_Stream.Position + ps + F_Stream.ReadByte() - 2; //加段长度

                    F_Stream.Seek(5, SeekOrigin.Current); //丢弃APP0标记(5bytes)
                    F_Stream.Seek(2, SeekOrigin.Current); //丢弃主版本号(1bytes)及次版本号(1bytes)
                    int units = F_Stream.ReadByte(); //X和Y的密度单位,units=0：无单位,units=1：点数/英寸,units=2：点数/厘米

                    ////水平方向(像素/英寸)分辨率
                    float Wpx = F_Stream.ReadByte() * 256;
                    Wpx = Wpx + F_Stream.ReadByte();
                    if (units == 2) Wpx = (float)(Wpx * 2.54); //厘米变为英寸
                    //垂直方向(像素/英寸)分辨率
                    float Hpx = F_Stream.ReadByte() * 256;
                    Hpx = Hpx + F_Stream.ReadByte();
                    if (units == 2) Hpx = (float)(Hpx * 2.54); //厘米变为英寸

                    size = new Size((int)Wpx, (int)Hpx);

                    F_Stream.Close();
                    return size;
                    //后面信息忽略
                    //if (rx != 2 && rx < 3) rx = rx + 2;
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
    /// <summary>
    /// 判断图片是A3还是A4
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public static bool isA3(Size size)
    {
        int S = size.Width * size.Height;
        //A4 大约在 3600000 A3 大约 7200000   在200 DPI下
        if (S > 6500000)
        {
            //确认是A3
            return true;
        }
        else
        {
            return false;
        }
    }
}

/// <summary>
/// 按文件名排序
/// </summary>
public class myFileSorter : IComparer
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
        FileInfo xInfo = (FileInfo)x;
        FileInfo yInfo = (FileInfo)y;


        //依名稱排序    
        return xInfo.FullName.CompareTo(yInfo.FullName);//遞增    
        //return yInfo.FullName.CompareTo(xInfo.FullName);//遞減    

        //依修改日期排序    
        //return xInfo.LastWriteTime.CompareTo(yInfo.LastWriteTime);//遞增    
        //return yInfo.LastWriteTime.CompareTo(xInfo.LastWriteTime);//遞減    
    }
}
/// <summary>
/// 按文件名排序
/// </summary>
public class myDirSorter : IComparer
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
        DirectoryInfo xInfo = (DirectoryInfo)x;
        DirectoryInfo yInfo = (DirectoryInfo)y;


        //依名稱排序    
        return xInfo.FullName.CompareTo(yInfo.FullName);//遞增    
        //return yInfo.FullName.CompareTo(xInfo.FullName);//遞減    

        //依修改日期排序    
        //return xInfo.LastWriteTime.CompareTo(yInfo.LastWriteTime);//遞增    
        //return yInfo.LastWriteTime.CompareTo(xInfo.LastWriteTime);//遞減    
    }
}
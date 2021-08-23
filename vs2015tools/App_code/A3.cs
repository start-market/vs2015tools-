using System;
using System.Collections;
using System.Text;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing;

class A3
{
    public string sSaveDir = "";
    public ArrayList AL_file;
    public ArrayList AL_image;

    System.Drawing.Imaging.ImageCodecInfo myImageCodecInfo;
    EncoderParameters encoderParams;

    /// <summary>
    /// 0 正向  1 反向
    /// </summary>
    public int imode = 0;

    public A3()
    {
        setDefault();
    }
    
    public void InitByFile(ArrayList al_file, string savedir)
    {
        sSaveDir = savedir;
        AL_file = al_file;
        AL_image = new ArrayList();

        for (int i = 0; i < AL_file.Count; i++)
        {
            Image image = Image.FromFile(savedir + "\\" + AL_file[i].ToString());
            AL_image.Add(image);
        }

        Doing();
    }
    public void InitByImage(ArrayList al_img, ArrayList al_file, string savedir)
    {
        sSaveDir = savedir;
        AL_image = al_img;
        AL_file = al_file;
        Doing();
    }

    void Doing()
    {
        if (AL_image.Count % 2 == 0)
        {
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
                bmp.Save(spath, myImageCodecInfo, encoderParams);
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

                bmp.Save(spath, myImageCodecInfo, encoderParams);
                bmp.Dispose();

                img.Dispose();
                //AciDebug.Debug(index + "-" + snum + " " + spath);

                File.Delete(sSaveDir + "\\" + AL_file[i]);

                //AciDebug.Debug("");
            }

            System.Windows.Forms.MessageBox.Show(sSaveDir + " 转换完成");
        }
        else
        {
            System.Windows.Forms.MessageBox.Show(sSaveDir + " 总数：" + AL_image.Count + "为单数");
        }
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
}

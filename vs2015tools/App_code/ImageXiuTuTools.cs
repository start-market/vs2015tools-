using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
public unsafe class ImageXiuTuTools
{
    byte* p;
    int iWidth;
    int ic;
    Bitmap bmp;
    BitmapData bitData;
    Bitmap bmpClone = null;
    bool bOpen = false;
    AciBmpInt.dotRGB RGB = new AciBmpInt.dotRGB(0, 0, 0);
    PictureBox picMain = null;
    PictureBox picBG = null;
    Panel pl_main = null;
    Label lbSave = null;
    Panel PL_box = null;
    float pp = 0;
    float per = 0;
    public bool bBeginZ = false;
    int intZ = 0;
    saveImage imgSave = new saveImage();
    bool isSave = false;
    Form1 form;
    /// <summary>加页码</summary>
    public string pageNumber = "";

    public ImageXiuTuTools(Form1 form1, Panel plmain, PictureBox pic, PictureBox picbg, Label lbsave, Panel Pl_box)
    {
        form = form1;
        pl_main = plmain;
        picMain = pic;
        picBG = picbg;
        lbSave = lbsave;
        PL_box = Pl_box;
        PL_box.Cursor = Cursors.Hand;
        PL_box.BringToFront();
        PL_box.Visible = false;
        PL_box.DoubleClick += PL_box_DoubleClick;
        picMain.MouseDown += PicMain_MouseDown;
        //picMain.MouseLeave += PicMain_MouseLeave;
        picMain.MouseClick += PicMain_MouseClick;
        picMain.MouseDoubleClick += PicMain_MouseDoubleClick;
        PL_box.MouseClick += PL_box_MouseClick;
    }

    private void PL_box_DoubleClick(object sender, EventArgs e)
    {
        if (PL_box.Width > 10)
        {
            Point pointXY = getPoint(PL_box.Location.X - picMain.Location.X, PL_box.Location.Y - picMain.Location.Y);
            Point pointWH = getPoint(PL_box.Width, PL_box.Height);

            int ix = pointXY.X;
            if (ix < 0) ix = 0;
            int iw = pointWH.X;
            if (ix + iw >= bmp.Width) iw = bmp.Width - 1 - ix;
            
            int iy = pointXY.Y;
            if (iy < 0) iy = 0;
            int ih = pointWH.Y;
            if (iy + ih >= bmp.Height) ih = bmp.Height - 1 - iy;

            bmpClose();
            Bitmap bmp2 = new Bitmap(iw, ih);
            Graphics g = Graphics.FromImage(bmp2);
            g.DrawImage(bmp, -ix, -iy, bmp.Width, bmp.Height);
            g.Dispose();
            
            setBitmap(bmp2, true);
        }
    }

    private void PicMain_MouseClick(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            if (picMain.Cursor == Cursors.VSplit)
            {
                if (!PL_box.Visible)
                {
                    PL_box.Visible = true;
                    PL_box.Location = new Point(e.Location.X - 2 + picMain.Location.X, picMain.Location.Y);
                    PL_box.Size = new Size(2, picMain.Height);
                }
                else
                {
                    int ix = PL_box.Location.X + 2;
                    if (e.Location.X > ix)
                    {
                        PL_box.Location = new Point(ix, picMain.Location.Y);
                        PL_box.Size = new Size(e.Location.X - ix + picMain.Location.X, picMain.Height);
                    }
                }
            }
            else if (picMain.Cursor == Cursors.HSplit)
            {
                if (!PL_box.Visible)
                {
                    PL_box.Visible = true;
                    PL_box.Location = new Point(picMain.Location.X, e.Location.Y - 2 + picMain.Location.Y);
                    PL_box.Size = new Size(picMain.Width, 2);
                }
                else
                {
                    int iy = PL_box.Location.Y + 2;
                    if (e.Location.Y > iy)
                    {
                        PL_box.Location = new Point(picMain.Location.X, iy);
                        PL_box.Size = new Size(picMain.Width, e.Location.Y - iy + picMain.Location.Y);
                    }
                }
            }
        }
        else if (e.Button == MouseButtons.Right)
        {
            if (picMain.Cursor == Cursors.VSplit || picMain.Cursor == Cursors.HSplit)
            {
                cancelCut();
            }
            else if (picMain.Cursor == Cursors.Arrow)
            {
                form.toPrev();
            }
            else
            {
                CloseAll_gn();
            }
        }
    }
    private void PL_box_MouseClick(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
        {
            cancelCut();
        }
    }

    private void PicMain_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        if (picMain.Cursor == Cursors.VSplit)
        {
            if (e.Location.X < picMain.Width / 2)
            {
                PL_box.Visible = true;
                PL_box.Location = new Point(e.Location.X + picMain.Location.X, picMain.Location.Y);
                PL_box.Size = new Size(picMain.Width - e.Location.X, picMain.Height);
            }
            else
            {
                PL_box.Visible = true;
                PL_box.Location = new Point(picMain.Location.X, picMain.Location.Y);
                PL_box.Size = new Size(e.Location.X, picMain.Height);
            }
        }
        else if (picMain.Cursor == Cursors.HSplit)
        {
            if (e.Location.Y < picMain.Height / 2)
            {
                PL_box.Visible = true;
                PL_box.Location = new Point(picMain.Location.X, e.Location.Y + picMain.Location.Y);
                PL_box.Size = new Size(picMain.Width, picMain.Height - e.Location.Y);
            }
            else
            {
                PL_box.Visible = true;
                PL_box.Location = new Point(picMain.Location.X, picMain.Location.Y);
                PL_box.Size = new Size(picMain.Width, e.Location.Y);
            }
        }
        else if (picMain.Cursor == Cursors.Arrow)
        {
            form.toNext();
        }
    }
    

    public void Clear()
    {
        CloseAll_gn();
        needSave(isSave);
        PL_box.Visible = false;
        picMain.Cursor = Cursors.Arrow;
        bBeginZ = false;
        bOpen = false;
        RGB = new AciBmpInt.dotRGB(0, 0, 0);
        if (bmp != null)
        {
            picMain.Image.Dispose();
            bmpClose();
            bmp.Dispose();
        }
    }
    public void cancelCut()
    {
        picMain.Cursor = Cursors.Arrow;
        if (PL_box.Visible)
        {
            PL_box.Visible = false;
        }
    }
    public void CloseAll_gn()
    {
        picMain.Cursor = Cursors.Arrow;
        bBeginZ = false;
        if (bmpClone != null) bmpClone.Dispose();
        if (PL_box.Visible)
        {
            PL_box.Visible = false;
        }
    }
    public void Save(string sNewPath)
    {
        CloseAll_gn();
        if (isSave)
        {
            bmpClose();
            imgSave.SaveNoDispose(bmp, sNewPath);
            lbSave.Text = "保存成功";
        }
    }
    public void needSave(bool issave)
    {
        isSave = issave;
        if (isSave)
        {
            lbSave.Text = "需要保存";
        }
        else
        {
            lbSave.Text = "无操作无需保存";
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sNewPath"></param>
    /// <param name="issave">是否触发存储</param>
    public void setBitmap(string sNewPath, bool issave)
    {
        byte[] bytes = getImageBytes(sNewPath);
        MemoryStream ms = new MemoryStream(bytes);
        setBitmap((Bitmap)Image.FromStream(ms), issave);
        ms.Dispose();
        ms.Close();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="bitmap"></param>
    /// <param name="issave">是否触发存储</param>
    public void setBitmap(Bitmap bitmap, bool issave)
    {
        isSave = issave;
        Clear();
        bmp = bitmap;
        //picMain.SizeMode = PictureBoxSizeMode.StretchImage;
        picMain.SizeMode = PictureBoxSizeMode.Normal;

        if (bmp.Height >= bmp.Width)
        {
            pp = (float)bmp.Width / (float)bmp.Height;
            //picMain.BackColor = Color.FromArgb(0, 0, 0);
            picMain.Height = pl_main.Height;
            picMain.Width = (int)Math.Round((float)picMain.Height * pp);
            picMain.Location = new Point((pl_main.Width - picMain.Width) / 2, 0);
        }
        else if (bmp.Width > bmp.Height)
        {
            pp = (float)bmp.Height / (float)bmp.Width;
            //picMain.BackColor = Color.FromArgb(0, 0, 0);
            picMain.Width = pl_main.Height;
            picMain.Height = (int)Math.Round((float)picMain.Width * pp);
            picMain.Location = new Point(0, (pl_main.Height - picMain.Height) / 2);
        }
        per = (float)bmp.Height / (float)picMain.Height;
        picMain.Image = bmp.GetThumbnailImage(picMain.Width, picMain.Height, null, IntPtr.Zero);
    }

    public void bmpOpen()
    {
        if (!bOpen)
        {
            lbSave.Text = "未保存";
            bitData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            ic = bitData.Stride - bitData.Width * 3;
            p = (byte*)(bitData.Scan0.ToPointer());
            iWidth = bmp.Width * 3 + ic;
            bOpen = true;
        }
    }

    public void bmpClose()
    {
        if (bOpen)
        {
            bmp.UnlockBits(bitData);
            bOpen = false;
        }
    }

    public void getAutoRGB()
    {
        CloseAll_gn();
        bmpOpen();
        picMain.Cursor = Cursors.Arrow;
        RGB = AciBmpInt.getAvgGrey(p, iWidth, bmp.Width, bmp.Height);
        picBG.BackColor = Color.FromArgb(RGB.R, RGB.G, RGB.B);
    }
    public void getMyRGB()
    {
        CloseAll_gn();
        bmpOpen();
        picMain.Cursor = Cursors.Cross;
    }
    //开始仿制印章
    public void beginZ(int iZ, bool isAgain)
    {
        CloseAll_gn();
        bmpOpen();
        intZ = iZ;
        if (!bBeginZ || isAgain || picMain.Cursor == Cursors.Arrow)
        {
            if (RGB.R == 0 && RGB.G == 0 && RGB.B == 0)
            {
                getAutoRGB();
            }
            bBeginZ = true;
            Bitmap myNewCursor = new Bitmap(iZ, iZ);
            Graphics g = Graphics.FromImage(myNewCursor);
            g.Clear(Color.FromArgb(RGB.R, RGB.G, RGB.B));
            g.DrawRectangle(new Pen(Color.FromArgb(0, 0, 0)), new Rectangle(0, 0, iZ - 1, iZ - 1));
            g.Dispose();

            picMain.Cursor = new Cursor(myNewCursor.GetHicon());
        }
    }
    //使用原图
    public void getAvgBmp(string sOldPath)
    {
        CloseAll_gn();
        intBmpAvgRGB intAvg = new intBmpAvgRGB();
        setBitmap(intAvg.setImage(new FileInfo(sOldPath)), true);
    }
    //使用一次去洞
    public void getCutDong(string sOldPath)
    {
        CloseAll_gn();
        if (bmp != null)
        {
            bmpClose();
        }
        needSave(true);
        intBmpCutKong intKong = new intBmpCutKong();
        intKong.setImage(ref bmp, new FileInfo(sOldPath), null);
        picMain.Image.Dispose();
        picMain.Image = bmp.GetThumbnailImage(picMain.Width, picMain.Height, null, IntPtr.Zero);
    }
    //旋转
    public void getRotate(int iR, string sOldPath)
    {
        CloseAll_gn();
        if (iR == 0)
        {
            intBmpAvgRGB intAvg = new intBmpAvgRGB();
            setBitmap(intAvg.setImage(new FileInfo(sOldPath)), true);
        }
        else
        {
            Bitmap bitmap = (Bitmap)Image.FromFile(sOldPath);
            if (iR == 90)
            {
                bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
            }
            else if (iR == -90)
            {
                bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
            }
            else if (iR == 180)
            {
                bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
            }
            intBmpAvgRGB intAvg = new intBmpAvgRGB();
            setBitmap(intAvg.setImage(ref bitmap, new FileInfo(sOldPath), null), true);
        }
    }
    public void getRotateYT(int iR, string sOldPath)
    {
        CloseAll_gn();
        if (iR == 0)
        {
            setBitmap((Bitmap)Image.FromFile(sOldPath), true);
        }
        else
        {
            Bitmap bitmap = (Bitmap)Image.FromFile(sOldPath);
            if (iR == 90)
            {
                bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
            }
            else if (iR == -90)
            {
                bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
            }
            else if (iR == 180)
            {
                bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
            }
            setBitmap(bitmap, true);
        }
    }

    private void PicMain_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            if (picMain.Cursor == Cursors.Cross)//吸取颜色时
            {
                Point point = getPoint(e.Location.X, e.Location.Y);
                int ib = point.Y * iWidth + point.X * 3;
                byte B = (byte)p[ib];
                byte G = (byte)p[ib + 1];
                byte R = (byte)p[ib + 2];
                RGB = new AciBmpInt.dotRGB(R, G, B);
                picBG.BackColor = Color.FromArgb(RGB.R, RGB.G, RGB.B);
            }
            else if (bBeginZ && picMain.Cursor != Cursors.Arrow)//开始处理仿制印章
            {
                bmpOpen();
                int iZ = (int)Math.Round((float)(intZ / 2 - 1) * per); ;
                Point point = getPoint(e.Location.X, e.Location.Y);
                int ix = point.X - iZ;
                if (ix < 0) ix = 0;
                if (ix >= bmp.Width) ix = bmp.Width - 1;
                int iy = point.Y - iZ;
                if (iy < 0) iy = 0;
                if (iy >= bmp.Height) iy = bmp.Height - 1;

                int iw = point.X + iZ;
                int ih = point.Y + iZ;

                if (iw >= bmp.Width) iw = bmp.Width - 1;
                if (ih >= bmp.Height) ih = bmp.Height - 1;

                for (int x = ix; x < iw; x++)
                {
                    for (int y = iy; y < ih; y++)
                    {
                        int ib = y * iWidth + x * 3;
                        p[ib] = RGB.B;
                        p[ib + 1] = RGB.G;
                        p[ib + 2] = RGB.R;
                    }
                }
                bmpClose();
                picMain.Image.Dispose();
                picMain.Image = bmp.GetThumbnailImage(picMain.Width, picMain.Height, null, IntPtr.Zero);
                needSave(true);
            }
            else if (picMain.Cursor == Cursors.IBeam && pageNumber != "")
            {
                #region 虚拟页码
                cloneBmp();
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Near;
                sf.LineAlignment = StringAlignment.Center;
                Point point = getPoint(e.Location.X, e.Location.Y);

                Graphics g = Graphics.FromImage(bmp);
                g.DrawString(pageNumber, new Font("仿宋", 18, FontStyle.Bold), new SolidBrush(Color.Black), new Rectangle(point.X, point.Y, 200, 70), sf);
                g.Dispose();
                picMain.Image.Dispose();
                picMain.Image = bmp.GetThumbnailImage(picMain.Width, picMain.Height, null, IntPtr.Zero);
                needSave(true);

                pageNumber = "";
                #endregion
            }
        }
    }

    void cloneBmp()
    {
        bmpClone = new Bitmap(bmp.Width, bmp.Height);
        Graphics g = Graphics.FromImage(bmpClone);
        g.DrawImage(bmp, 0, 0, bmpClone.Width, bmpClone.Height);
        g.Dispose();
    }

    public void toBack()
    {
        bmp = (Bitmap)bmpClone.Clone();
        picMain.Image.Dispose();
        picMain.Image = bmp.GetThumbnailImage(picMain.Width, picMain.Height, null, IntPtr.Zero);
        needSave(true);
    }

    private void PicMain_MouseLeave(object sender, EventArgs e)
    {
        //bBeginZ = false;
        //picMain.Cursor = Cursors.Arrow;
    }
    public Point getPoint(int x, int y)
    {
        Point point = new Point();
        point.X = (int)Math.Round((float)x * per);
        point.Y = (int)Math.Round((float)y * per);
        if (point.X >= bmp.Width) point.X = bmp.Width - 1;
        if (point.Y >= bmp.Height) point.Y = bmp.Height - 1;
        return point;
    }

    public byte[] getImageBytes(String path)
    {
        FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read); //将图片以文件流的形式进行保存
        byte[] imgBytesIn = new byte[fs.Length];
        fs.Read(imgBytesIn, 0, imgBytesIn.Length);
        fs.Close();
        return imgBytesIn;
    }
}

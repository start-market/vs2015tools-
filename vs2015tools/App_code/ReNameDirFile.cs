using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

public class ReNameDirFile
{
    public ReNameDirFile()
    {
    }
    public static string[] ReName(DirectoryInfo dir, string name, int icount)
    {

        FileInfo[] files = dir.GetFiles("*.jpg", SearchOption.TopDirectoryOnly);

        DirectoryInfo dir_ls = new DirectoryInfo(dir.FullName + "\\临时");
        if (!dir_ls.Exists)
        {
            dir_ls.Create();
        }
        //全部移入临时文件夹
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].FullName.IndexOf("_临时") > -1)
            {
                files[i].Delete();
            }
            else
            {
                files[i].MoveTo(dir_ls.FullName + "\\" + files[i].Name);
            }
        }


        files = dir_ls.GetFiles("*.jpg", SearchOption.TopDirectoryOnly);
        if (files.Length % 2 == 1)
        {
            //是单数需要补充一页
            Bitmap bmp = new Bitmap(1700, 2100);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            g.Dispose();
            saveImage imgSave = new saveImage();
            imgSave.Save(bmp, dir_ls.FullName + "\\补充一页ls.jpg");
        }
        files = dir_ls.GetFiles("*.jpg", SearchOption.TopDirectoryOnly);

        string[] arrName = new string[files.Length];
        for (int n = 0; n < arrName.Length; n++)
        {
            arrName[n] = files[n].FullName;
        }
        Array.Sort<string>(arrName);

        //全部重命名后移动回来
        int index = 0;

        ArrayList al_file = new ArrayList();
        for (int n = 0; n < arrName.Length; n++)
        {
            string sAB = "B";
            if (index % 2 == 0)
            {
                sAB = "A";
            }
            string newname = dir.FullName + "\\" + name + "" + AciMath.setNumberLength((int)Math.Floor((double)index / 2) + 1, icount) + sAB + ".jpg";
            File.Move(arrName[n], newname);
            al_file.Add(newname);
            index++;
        }
        arrName = AciCvt.ArrayList_to_StringArr(al_file);
        Array.Sort<string>(arrName);

        dir_ls.Delete(true);

        return arrName;
    }
}

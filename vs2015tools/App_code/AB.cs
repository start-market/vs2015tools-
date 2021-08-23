using System;
using System.Collections;
using System.Text;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing;

public class AB
{
    public string sA = "";
    public string sB = "";
    saveImage imgSave = new saveImage();
    public AB(string a, string b)
    {
        sA = a;
        sB = b;
    }
    public void Init()
    {
        string sdir = Path.GetDirectoryName(sA);
        string filename = Path.GetFileNameWithoutExtension(sA);
        filename = filename.Substring(0, filename.Length - 1);

        Image imgA = Image.FromFile(sA);
        imgA.RotateFlip(RotateFlipType.Rotate180FlipNone);
        imgSave.Save(imgA, sdir + "\\" + filename + "A_¡Ÿ ±.jpg");
        imgA.Dispose();

        File.Delete(sA);

        Image imgB = Image.FromFile(sB);
        imgB.RotateFlip(RotateFlipType.Rotate180FlipNone);
        imgSave.Save(imgB, sdir + "\\" + filename + "A.jpg");
        imgB.Dispose();

        File.Delete(sB);

        File.Move(sdir + "\\" + filename + "A_¡Ÿ ±.jpg", sdir + "\\" + filename + "B.jpg");
    }
}

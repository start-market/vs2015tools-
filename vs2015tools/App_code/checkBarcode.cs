using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;

class checkBarcode
{
    public checkBarcode()
    {

    }

    [DllImport("kernel32")]
    public extern static int LoadLibrary(string librayName);

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    static extern bool EndDialog(IntPtr hDlg, out IntPtr nResult);

    public string Read(Image img)
    {
        string s = "";


        if (img.Width > img.Height)
        {
            img.RotateFlip(RotateFlipType.Rotate90FlipNone);
        }
        if (img.Width < 3000)
        {
            int iH = 500;
            Bitmap bmp = new Bitmap(img.Width, iH);
            Graphics g = Graphics.FromImage(bmp);
            g.DrawImage(img, 0, 0, img.Width, img.Height);

            //bmp.Save(Application.StartupPath + "/aa.jpg");

            s = ReadBarCode(bmp);

            //MessageBox.Show(s + "---------");

            //AciDebug.Debug(Application.StartupPath + "/aa.jpg");

            g.Dispose();
            bmp.Dispose();
        }
        else
        {
            img.RotateFlip(RotateFlipType.Rotate90FlipNone);
            int iH = 500;
            Bitmap bmp = new Bitmap(img.Width / 2, iH);
            Graphics g = Graphics.FromImage(bmp);
            g.DrawImage(img, 0, 0, img.Width, img.Height);

            //bmp.Save(Application.StartupPath + "/aa.jpg");
            s = ReadBarCode(bmp);

            //AciDebug.Debug("nnnnn");

            g.Dispose();
            bmp.Dispose();
        }

        //img.Dispose();
        //MessageBox.Show("asd");
        return s;
    }

    public string ReadBarCode(Bitmap bmp)
    {
        string s = "";

        SoftekBarcodeLib3.BarcodeReader barcode;

        // For the purposes of this demo we first give a path to the installation folder
        // and the class adds either x86 or x64 to the end and tries to load the other dll files.
        // If that fails (perhaps this project has been moved) then we give no path and the class
        // assumes that the dll files will be somewhere on the PATH.
        barcode = new SoftekBarcodeLib3.BarcodeReader("..\\");



        ThreadPool.QueueUserWorkItem(new WaitCallback(CloseMessageBox), new CloseState("Softek Barcode Reader SDK", 100));

        // Enter your license key here
        // You can get a trial license key from sales@bardecode.com
        // Example:
        // barcode.LicenseKey = "MY LICENSE KEY";

        // Turn on the barcode types you want to read.
        // Turn off the barcode types you don't want to read (this will increase the speed of your application)
        barcode.ReadCode128 = true;
        barcode.ReadCode39 = false;
        barcode.ReadCode93 = false;
        barcode.ReadCode25 = false;
        barcode.ReadCode25ni = false;
        barcode.ReadEAN13 = false;
        barcode.ReadEAN8 = false;
        barcode.ReadUPCA = false;
        barcode.ReadUPCE = false;
        barcode.ReadCodabar = false;
        barcode.ReadPDF417 = false;
        barcode.ReadDataMatrix = false;
        barcode.ReadDatabar = false;
        barcode.ReadMicroPDF417 = false;
        barcode.ReadQRCode = false;

        // Databar Options is a mask that controls which type of databar barcodes will be read and whether or not
        // the software will look for a quiet zone around the barcode.
        // 1 = 2D-Linkage flag (handle micro-PDf417 barcodes as supplementary data - requires ReadMicroPDF417 to be true).
        // 2 = Read RSS14
        // 4 = Read RSS14 Stacked
        // 8 = Read Limited
        // 16 = Read Expanded
        // 32 = Read Expanded Stacked
        // 64 = Require quiet zone
        barcode.DatabarOptions = 255;

        // If you want to read more than one barcode then set Multiple Read to 1
        // Setting MutlipleRead to False will make the recognition faster
        barcode.MultipleRead = true;

        // Noise reduction takes longer but can make it possible to read some difficult barcodes
        // When using noise reduction a typical value is 10 - the smaller the value the more effect it has.
        // A zero value turns off noise reduction. 
        // barcode.NoiseReduction = 0 ;

        // You may need to set a small quiet zone if your barcodes are close to text and pictures in the image.
        // A value of zero uses the default.
        barcode.QuietZoneSize = 0;

        // LineJump controls the frequency at which scan lines in an image are sampled.
        // The default is 9 - decrease this for difficult barcodes.
        barcode.LineJump = 1;

        // You can restrict your search to a particular area of the image if you wish.
        // This example limits the search to the upper half of the page
        // System.Drawing.Rectangle scanArea = new System.Drawing.Rectangle(0, 0, 100, 50);
        // barcode.SetScanRect(scanArea, 1);

        // Set the direction that the barcode reader should scan for barcodes
        // The value is a mask where 1 = Left to Right, 2 = Top to Bottom, 4 = Right To Left, 8 = Bottom to Top
        barcode.ScanDirection = 15;

        // SkewTolerance controls the angle of skew that the barcode toolkit will tolerate. By default
        // the toolkit checks for barcodes along horizontal and vertical lines in an image. This works 
        // OK for most barcodes because even at an angle it is possible to pass a line through the entire
        // length. SkewTolerance can range from 0 to 5 and allows for barcodes skewed to an angle of 45
        // degrees.
        barcode.SkewTolerance = 0;

        // ColorProcessingLevel controls how much time the toolkit will searching a color image for a barcode.
        // The default value is 2 and the range of values is 0 to 5. If ColorThreshold is non-zero then 
        // ColorProcessingLevel is effectively set to 0.
        barcode.ColorProcessingLevel = 2;

        // MaxLength and MinLength can be used to specify the number of characters you expect to
        // find in a barcode. This can be useful to increase accuracy or if you wish to ignore some
        // barcodes in an                                    image.
        barcode.MinLength = 4;
        barcode.MaxLength = 999;

        // When the toolkit scans an image it records the score it gets for each barcode that
        // MIGHT be in the image. If the scores recorded for any of the barcodes are >= PrefOccurrence
        // then only these barcodes are returned. Otherwise, any barcode whose scores are >= MinOccurrence
        // are reported. If you have a very poor quality image then try setting MinOccurrence to 1, but you
        // may find that some false positive results are returned.
        // barcode.MinOccurrence = 2 ;
        // barcode.PrefOccurrence = 4 ;

        // Flags for handling PDF files
        // PdfImageOnly defaults to true and indicates that the PDF documents are simple images.
        barcode.PdfImageOnly = true;

        // The PdfImageExtractOptions mask controls how images are removed from PDF documents (when PdfImageOnly is True)
        // 1 = Enable fast extraction
        // 2 = Auto-invert black and white images
        // 4 = Auto-merge strips
        // 8 = Auto-correct photometric values in black and white images
        barcode.PdfImageExtractOptions = 15;

        // The PdfImageRasterOptions mask controls how images are rasterized when PdfImageOnly is false or when image extraction fails
        // 1 = Use alternative pdf-to-tif conversion function
        // 2 = Always use pdf-to-tif conversion rather than loading the rasterized image directly into memory
        barcode.PdfImageRasterOptions = 0;

        // PdfDpi and PdfBpp control what sort of image the PDF document is rasterized into
        barcode.PdfDpi = 300;
        barcode.PdfBpp = 8;

        int nBarCode;

        nBarCode = barcode.ScanBarCodeFromBitmap(bmp);


        if (nBarCode <= -6)
        {
            //Result.Text += "License key error: either an evaluation key has expired or the license key is not valid for processing pdf documents\r\n";
        }
        else if (nBarCode < 0)
        {
            //Result.Text += "ScanBarCode returned error number ";
            //Result.Text += nBarCode.ToString();
            //Result.Text += "\r\n";
            //Result.Text += "Last Softek Error Number = ";
            //Result.Text += barcode.GetLastError().ToString();
            //Result.Text += "\r\n";
            //Result.Text += "Last Windows Error Number = ";
            //Result.Text += barcode.GetLastWinError().ToString();
            //Result.Text += "\r\n";
        }
        else if (nBarCode == 0)
        {
            //Result.Text += "No barcode found on this image\r\n";
        }
        else
        {
            for (int i = 1; i <= nBarCode; i++)
            {
                if (s == "")
                {
                    s = barcode.GetBarString(i);
                    //MessageBox.Show(s + "--------------" + i);
                    break;
                }
                //Result.Text += String.Format("Barcode {0}\r\n", i);
                //Result.Text += String.Format("Value = {0}\r\n", barcode.GetBarString(i));
                //Result.Text += String.Format("Type = {0}\r\n", barcode.GetBarStringType(i));
                //int nDirection = barcode.GetBarStringDirection(i);
                //if (nDirection == 1)
                //    Result.Text += "Direction = Left to Right\r\n";
                //else if (nDirection == 2)
                //    Result.Text += "Direction = Top to Bottom\r\n";
                //else if (nDirection == 4)
                //    Result.Text += "Direction = Right to Left\r\n";
                //else if (nDirection == 8)
                //    Result.Text += "Direction = Bottom to Top\r\n";
                //int nPage = barcode.GetBarStringPage(i);
                //Result.Text += String.Format("Page = {0}\r\n", nPage);
                //System.Drawing.Rectangle rect = barcode.GetBarStringRect(i);
                //Result.Text += String.Format("Top Left = ({0},{1})\r\n", rect.X, rect.Y);
                //Result.Text += String.Format("Bottom Right = ({0},{1})\r\n", rect.X + rect.Width, rect.Y + rect.Height);
                //Result.Text += "\r\n";
            }
        }
        return s;
    }

    public void ShowMessageBoxTimeout(string text, string caption, MessageBoxButtons buttons, int timeout)
    {
        ThreadPool.QueueUserWorkItem(new WaitCallback(CloseMessageBox), new CloseState(caption, timeout));

        MessageBox.Show(text, caption, buttons);

    }
    private class CloseState
    {
        private int _Timeout;

        /**/
        /// <summary>
        /// In millisecond
        /// </summary>
        public int Timeout
        {
            get
            {
                return _Timeout;
            }
        }

        private string _Caption;

        /**/
        /// <summary>
        /// Caption of dialog
        /// </summary>
        public string Caption
        {
            get
            {
                return _Caption;
            }
        }

        public CloseState(string caption, int timeout)
        {
            _Timeout = timeout;
            _Caption = caption;
        }
    }
    private void CloseMessageBox(object state)
    {
        CloseState closeState = state as CloseState;

        Thread.Sleep(closeState.Timeout);
        IntPtr dlg = FindWindow(null, closeState.Caption);

        if (dlg != IntPtr.Zero)
        {
            IntPtr result;
            EndDialog(dlg, out result);
        }
    }
}

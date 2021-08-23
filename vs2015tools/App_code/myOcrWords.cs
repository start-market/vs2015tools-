using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;

public unsafe class myOcrWord
{
    uint w = 0;
    uint h = 0;

    //һ�е����ص�
    uint iWidth = 0;
    uint r = 0;
    uint g = 0;
    uint b = 0;

    public string sWord = "";

    public saveImage imgSave = new saveImage();
    public myDataConn myDC = null;

    DataSet ds = new DataSet();

    System.Drawing.Imaging.ImageCodecInfo myImageCodecInfo;
    EncoderParameters encoderParams;

    public bool show = false;
    public Form form = null;


    /// <summary>�Ƿ����OCR���� ���ǽ��ṹ Ĭ�� -1 ����ȫ������ ���� -1 �޶�ĳ����</summary>
    public int iOcrWord = -1;
    /// <summary>�Ƿ���е��� Ĭ�� false</summary>
    public bool isDebug = false;
    /// <summary>�������ֱȽ� </summary>
    public string sDebugWord = "";

    public myOcrWord()
    {
        //myDC = new myDataConn("oledb", Application.StartupPath + "\\words_25\\words.mdb");
        //ds.Tables.Add(myDC.Fc_OdaDataTable("select * from words", "25"));
        //myDC = new myDataConn("oledb", Application.StartupPath + "\\words_30\\words.mdb");
        //ds.Tables.Add(myDC.Fc_OdaDataTable("select * from words", "30"));
        //myDC = new myDataConn("oledb", Application.StartupPath + "\\words_35\\words.mdb");
        //ds.Tables.Add(myDC.Fc_OdaDataTable("select * from words", "35"));

        myDC = new myDataConn("oledb", Application.StartupPath + "\\words_40\\words.mdb");
        ds.Tables.Add(myDC.Fc_OdaDataTable("select * from words", "40"));

        encoderParams = new EncoderParameters();
        long[] quality;
        //ͼƬ����
        quality = new long[] { 100 };
        EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
        encoderParams.Param[0] = encoderParam;
        myImageCodecInfo = GetEncoderInfo("image/jpeg");
    }

    public void setInvoke(Form f)
    {
        form = f;
    }

    public int iConstract = -1;

    public string sOcr_In = "";
    public string sOcr_NotIn = "";
    /// <summary>
    /// ���� ����ʶ��
    /// </summary>
    /// <param name="bitmap"></param>
    /// <param name="ocrMode">Rect.OcrMode</param>
    /// <param name="sIn">������������ַ�</param>
    /// <param name="sNotIn">����ʶ�����ȥ�����ַ�</param>
    /// <returns></returns>
    public OcrWords OcrStart(Bitmap bitmap, Rect.OcrMode ocrMode, string sIn, string sNotIn)
    {
        BitmapData bitData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        sOcr_In = sIn;
        sOcr_NotIn = sNotIn;


        //ȡ��ÿ�ж��������
        uint ic = (uint)(bitData.Stride - bitData.Width * 3);

        //ȡ��ȫ������ֵ ÿ�� 3��  Ϊ  R G B ÿ��ѭ��ɾ�� һ�������� xx ��
        byte* p = (byte*)(bitData.Scan0.ToPointer());

        w = (uint)bitData.Width;
        h = (uint)bitData.Height;

        //һ�е����ص�
        iWidth = w * 3 + ic;

        

        //�ҶȻ�
        toGrey(p);
        if (show) bitmap.Save(Application.StartupPath + "\\test\\��ʱ_a�ҶȻ�.png", ImageFormat.Png);

        if (iConstract > -1)
        {
            toConstract(p, iConstract);
        }

        toTwo2(p, 0, w, h, iWidth, bitmap);

        //ֱ��ͼ
        ////��ʼ��ֵ��
        //ֱ��ͼ
        //OcrStruct.Histogram Htg = toZhiFang(p);
        //uint Oavg = setThreshold(Htg);

        //toTwo(p, Oavg);

        //toSmallLine(p, w, h, iWidth, -1);
        //toBigLine(p, w, h, iWidth, 1, 1);

        if (show) bitmap.Save(Application.StartupPath + "\\test\\��ʱ_b��ֵ��.png", ImageFormat.Png);

        ////ȥ������
        //toClear(p);
        //if (show) bitmap.Save(Application.StartupPath + "\\test\\��ʱ_c���Ե.jpg", myImageCodecInfo, encoderParams);
        
        ////��ͨ����
        OcrWords ocrWords = cutArea(p, ocrMode);
        bitmap.UnlockBits(bitData);

        if (show) bitmap.Save(Application.StartupPath + "\\test\\��ʱ_d��ͨ����.jpg", myImageCodecInfo, encoderParams);


        return ocrWords;
        return null;
    }

    public unsafe void toConstract(byte* p, int degree)
    {
        for (uint y = 0; y < h; y++)
        {
            for (uint x = 0; x < w; x++)
            {
                //��ʼ��
                uint ib = y * iWidth + x * 3;
                if ((p[ib] + p[ib + 1] + p[ib + 2]) / 3 > degree)
                {
                    p[ib] = p[ib + 1] = p[ib + 2] = (byte)255;
                }
            }
        }
    }

    byte iThisOavg = 80;
    public void toTwo2(byte* p, uint Oavg, uint bmp_w, uint bmp_h, uint bmp_iWidth, Bitmap bitmap)
    {
        int iw = (int)bmp_w;
        int ih = (int)bmp_h;



        setFrame1(p, Oavg, bmp_w, bmp_h, bmp_iWidth, iw, ih); if (isDebug) bitmap.Save(Application.StartupPath + "\\test\\1.png", ImageFormat.Png);
        setFrame2(p, Oavg, bmp_w, bmp_h, bmp_iWidth, iw, ih); if (isDebug) bitmap.Save(Application.StartupPath + "\\test\\2.png", ImageFormat.Png);
        setFrame3(p, Oavg, bmp_w, bmp_h, bmp_iWidth, iw, ih); if (isDebug) bitmap.Save(Application.StartupPath + "\\test\\3.png", ImageFormat.Png);
        //setFrame3(p, Oavg, bmp_w, bmp_h, bmp_iWidth, iw, ih); bitmap.Save(Application.StartupPath + "\\test\\3.png", ImageFormat.Png);
        toBlack(p, iThisOavg, bmp_w, bmp_h, bmp_iWidth); if (isDebug) bitmap.Save(Application.StartupPath + "\\test\\5.png", ImageFormat.Png);

        //setFrame_cut1(p, Oavg, bmp_w, bmp_h, bmp_iWidth, iw, ih); bitmap.Save(Application.StartupPath + "\\test\\3.png", ImageFormat.Png);
        //setFrame3(p, Oavg, bmp_w, bmp_h, bmp_iWidth, iw, ih); bitmap.Save(Application.StartupPath + "\\test\\4.png", ImageFormat.Png);
        //setFrame(p, Oavg, bmp_w, bmp_h, bmp_iWidth, iw, ih);
        //setFrame(p, Oavg, bmp_w, bmp_h, bmp_iWidth, iw, ih);
    }

    void toBlack(byte* p, byte Oavg, uint bmp_w, uint bmp_h, uint bmp_iWidth)
    {
        for (uint y = 0; y < bmp_h; y++)
        {
            for (uint x = 0; x < bmp_w; x++)
            {
                //��ʼ��
                uint ib = y * bmp_iWidth + x * 3;
                int igrey = p[ib];
                if (igrey <= Oavg)
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

    void setFrame1(byte* p, uint Oavg, uint bmp_w, uint bmp_h, uint bmp_iWidth, int iw, int ih)
    {
        int[,] bmp_p = new int[bmp_w, bmp_h];

        ArrayList AL_256 = new ArrayList();
        for (int i = 0; i < 256; i++)
        {
            ArrayList al = new ArrayList();
            AL_256.Add(al);
        }

        for (uint y = 0; y < bmp_h; y++)
        {
            for (uint x = 0; x < bmp_w; x++)
            {
                //��ʼ��
                uint ib = y * bmp_iWidth + x * 3;
                byte R = p[ib];
                ArrayList al = (ArrayList)AL_256[(int)R];
                al.Add(new myDot(x, y, bmp_iWidth, p));
            }
        }

        int iCha = 220;
        //�� 253 ��ʼ
        for (int i = AL_256.Count - 1; i > 0; i--)
        {
            ArrayList Al_dot = (ArrayList)AL_256[i];
            for (int j = 0; j < Al_dot.Count; j++)
            {
                myDot pNow = (myDot)Al_dot[j];
                byte R = (byte)i;
                myDot p1 = getDot(p, (int)pNow.x, (int)pNow.y, -1, -1, iw, ih, bmp_iWidth);//p1
                myDot p2 = getDot(p, (int)pNow.x, (int)pNow.y, 0, -1, iw, ih, bmp_iWidth);//p2
                myDot p3 = getDot(p, (int)pNow.x, (int)pNow.y, 1, -1, iw, ih, bmp_iWidth);//p3
                myDot p4 = getDot(p, (int)pNow.x, (int)pNow.y, -1, 0, iw, ih, bmp_iWidth);//p4
                myDot p6 = getDot(p, (int)pNow.x, (int)pNow.y, 1, 0, iw, ih, bmp_iWidth);//p6
                myDot p7 = getDot(p, (int)pNow.x, (int)pNow.y, -1, 1, iw, ih, bmp_iWidth);//p7
                myDot p8 = getDot(p, (int)pNow.x, (int)pNow.y, 0, 1, iw, ih, bmp_iWidth);//p8
                myDot p9 = getDot(p, (int)pNow.x, (int)pNow.y, 1, 1, iw, ih, bmp_iWidth);//p9

                if (p1 != null && p2 != null && p3 != null
                    && p4 != null && p6 != null
                    && p7 != null && p8 != null && p9 != null)
                {
                    ArrayList al = new ArrayList();
                    if (p1.R <= R) al.Add(p1);
                    if (p2.R <= R) al.Add(p2);
                    if (p3.R <= R) al.Add(p3);
                    if (p4.R <= R) al.Add(p4);
                    if (p6.R <= R) al.Add(p6);
                    if (p7.R <= R) al.Add(p7);
                    if (p8.R <= R) al.Add(p8);
                    if (p9.R <= R) al.Add(p9);

                    if (p1.R >= iCha && p2.R >= iCha && p3.R >= iCha)
                    {
                        int perR = (p4.R + R + p6.R + p7.R + p8.R + p9.R) / 6;
                        if ((p4.R < iCha && R < iCha && p6.R < iCha
                            && p7.R < iCha && p8.R < iCha && p9.R < iCha)
                            || (perR < iCha && perR > 150))
                        {
                            bmp_p[p4.x, p4.y] += (255 - p4.R) / 2;
                            bmp_p[pNow.x, pNow.y] += (255 - pNow.R) / 2;
                            bmp_p[p6.x, p6.y] += (255 - p6.R) / 2;
                            bmp_p[p7.x, p7.y] += (255 - p7.R) / 2;
                            bmp_p[p8.x, p8.y] += (255 - p8.R) / 2;
                            bmp_p[p9.x, p9.y] += (255 - p9.R) / 2;
                        }
                    }
                    else if (p7.R >= iCha && p8.R >= iCha && p9.R >= iCha)
                    {
                        int perR = (p4.R + R + p6.R + p1.R + p2.R + p3.R) / 6;
                        if ((p4.R < iCha && R < iCha && p6.R < iCha
                            && p1.R < iCha && p2.R < iCha && p3.R < iCha)
                            || (perR < iCha && perR > 150))
                        {
                            bmp_p[p4.x, p4.y] += (255 - p4.R) / 2;
                            bmp_p[pNow.x, pNow.y] += (255 - pNow.R) / 2;
                            bmp_p[p6.x, p6.y] += (255 - p6.R) / 2;
                            bmp_p[p1.x, p1.y] += (255 - p1.R) / 2;
                            bmp_p[p2.x, p2.y] += (255 - p2.R) / 2;
                            bmp_p[p3.x, p3.y] += (255 - p3.R) / 2;
                        }
                    }

                    else if (p1.R >= iCha && p4.R >= iCha && p7.R >= iCha)
                    {
                        int perR = (p2.R + R + p8.R + p3.R + p6.R + p9.R) / 6;
                        if ((perR > 200
                            && p2.R < iCha && R < iCha && p8.R < iCha
                            && p3.R < iCha && p6.R < iCha && p9.R < iCha)
                            || (perR < iCha && perR > 200))
                        {
                            bmp_p[p2.x, p2.y] += (255 - p2.R) / 2;
                            bmp_p[pNow.x, pNow.y] += (255 - pNow.R) / 2;
                            bmp_p[p8.x, p8.y] += (255 - p8.R) / 2;
                            bmp_p[p3.x, p3.y] += (255 - p3.R) / 2;
                            bmp_p[p6.x, p6.y] += (255 - p6.R) / 2;
                            bmp_p[p9.x, p9.y] += (255 - p9.R) / 2;
                        }
                    }
                    else if (p3.R >= iCha && p6.R >= iCha && p9.R >= iCha)
                    {
                        int perR = (p2.R + R + p8.R + p1.R + p4.R + p7.R) / 6;
                        if ((perR > 200
                            && p2.R < iCha && R < iCha && p8.R < iCha
                            && p1.R < iCha && p4.R < iCha && p7.R < iCha)
                            || (perR < iCha && perR > 200))
                        {
                            bmp_p[p2.x, p2.y] += (255 - p2.R) / 2;
                            bmp_p[pNow.x, pNow.y] += (255 - pNow.R) / 2;
                            bmp_p[p8.x, p8.y] += (255 - p8.R) / 2;
                            bmp_p[p1.x, p1.y] += (255 - p1.R) / 2;
                            bmp_p[p4.x, p4.y] += (255 - p4.R) / 2;
                            bmp_p[p7.x, p7.y] += (255 - p7.R) / 2;
                        }
                    }

                    byte iAdd = (byte)Math.Round((double)(255 - R) / al.Count);
                    for (int k = 0; k < al.Count; k++)
                    {
                        myDot dot = (myDot)al[k];
                        bmp_p[dot.x, dot.y] += iAdd;
                    }
                    //bmp_p[pNow.x, pNow.y] += iAdd;
                }

                //bmp_p[pNow.x, pNow.y] -= iAdd;
            }
        }

        setFrame_do(bmp_p, p, bmp_w, bmp_h, bmp_iWidth);
    }
    void setFrame2(byte* p, uint Oavg, uint bmp_w, uint bmp_h, uint bmp_iWidth, int iw, int ih)
    {
        int[,] bmp_p = new int[bmp_w, bmp_h];

        for (uint y = 0; y < bmp_h; y++)
        {
            for (uint x = 0; x < bmp_w; x++)
            {
                //��ʼ��
                uint ib = y * bmp_iWidth + x * 3;
                byte R = p[ib];

                myDot p1 = getDot(p, (int)x, (int)y, -1, -1, iw, ih, bmp_iWidth);//p1
                myDot p2 = getDot(p, (int)x, (int)y, 0, -1, iw, ih, bmp_iWidth);//p2
                myDot p3 = getDot(p, (int)x, (int)y, 1, -1, iw, ih, bmp_iWidth);//p3
                myDot p4 = getDot(p, (int)x, (int)y, -1, 0, iw, ih, bmp_iWidth);//p4
                myDot p6 = getDot(p, (int)x, (int)y, 1, 0, iw, ih, bmp_iWidth);//p6
                myDot p7 = getDot(p, (int)x, (int)y, -1, 1, iw, ih, bmp_iWidth);//p7
                myDot p8 = getDot(p, (int)x, (int)y, 0, 1, iw, ih, bmp_iWidth);//p8
                myDot p9 = getDot(p, (int)x, (int)y, 1, 1, iw, ih, bmp_iWidth);//p9

                if (p1 != null && p2 != null && p3 != null
                    && p4 != null && p6 != null
                    && p7 != null && p8 != null && p9 != null)
                {
                    // 2_4 ת��
                    if (p1.R > R && p2.R <= R && p3.R > R
                        && p4.R <= R && p6.R > R
                        && p7.R > R && p8.R > R && p9.R > R)
                    {
                        bmp_p[x, y] += (255 - p2.R + 255 - p4.R) / 2;
                    }
                    else if (p1.R > R && p2.R <= R && p3.R <= R
                        && p4.R <= R && p6.R > R
                        && p7.R > R && p8.R > R && p9.R > R)
                    {
                        bmp_p[x, y] += (255 - p2.R + 255 - p4.R) / 2;
                    }
                    else if (p1.R > R && p2.R <= R && p3.R > R
                         && p4.R <= R && p6.R > R
                         && p7.R <= R && p8.R > R && p9.R > R)
                    {
                        bmp_p[x, y] += (255 - p2.R + 255 - p4.R) / 2;
                    }
                    else if (p1.R > R && p2.R <= R && p3.R <= R
                       && p4.R <= R && p6.R > R
                       && p7.R <= R && p8.R > R && p9.R > R)
                    {
                        bmp_p[x, y] += (255 - p2.R + 255 - p4.R) / 2;
                    }

                    // 2_6 ת��
                    else if (p1.R > R && p2.R <= R && p3.R > R
                        && p4.R > R && p6.R <= R
                        && p7.R > R && p8.R > R && p9.R > R)
                    {
                        bmp_p[x, y] += (255 - p2.R + 255 - p6.R) / 2;
                    }
                    else if (p1.R <= R && p2.R <= R && p3.R > R
                        && p4.R > R && p6.R <= R
                        && p7.R > R && p8.R > R && p9.R > R)
                    {
                        bmp_p[x, y] += (255 - p2.R + 255 - p6.R) / 2;
                    }
                    else if (p1.R > R && p2.R <= R && p3.R > R
                        && p4.R > R && p6.R <= R
                        && p7.R > R && p8.R > R && p9.R <= R)
                    {
                        bmp_p[x, y] += (255 - p2.R + 255 - p6.R) / 2;
                    }
                    else if (p1.R <= R && p2.R <= R && p3.R > R
                        && p4.R > R && p6.R <= R
                        && p7.R > R && p8.R > R && p9.R <= R)
                    {
                        bmp_p[x, y] += (255 - p2.R + 255 - p6.R) / 2;
                    }

                    // 6_8 ת��
                    else if (p1.R > R && p2.R > R && p3.R > R
                        && p4.R > R && p6.R <= R
                        && p7.R > R && p8.R <= R && p9.R > R)
                    {
                        bmp_p[x, y] += (255 - p6.R + 255 - p8.R) / 2;
                    }
                    else if (p1.R > R && p2.R > R && p3.R <= R
                        && p4.R > R && p6.R <= R
                        && p7.R > R && p8.R <= R && p9.R > R)
                    {
                        bmp_p[x, y] += (255 - p6.R + 255 - p8.R) / 2;
                    }
                    else if (p1.R > R && p2.R > R && p3.R > R
                     && p4.R > R && p6.R <= R
                     && p7.R <= R && p8.R <= R && p9.R > R)
                    {
                        bmp_p[x, y] += (255 - p6.R + 255 - p8.R) / 2;
                    }
                    else if (p1.R > R && p2.R > R && p3.R <= R
                          && p4.R > R && p6.R <= R
                          && p7.R <= R && p8.R <= R && p9.R > R)
                    {
                        bmp_p[x, y] += (255 - p6.R + 255 - p8.R) / 2;
                    }

                    // 4_8 ת��
                    else if (p1.R > R && p2.R > R && p3.R > R
                        && p4.R <= R && p6.R > R
                        && p7.R > R && p8.R <= R && p9.R > R)
                    {
                        bmp_p[x, y] += (255 - p4.R + 255 - p8.R) / 2;
                    }
                    else if (p1.R <= R && p2.R > R && p3.R > R
                        && p4.R <= R && p6.R > R
                        && p7.R > R && p8.R <= R && p9.R > R)
                    {
                        bmp_p[x, y] += (255 - p4.R + 255 - p8.R) / 2;
                    }
                    else if (p1.R > R && p2.R > R && p3.R > R
                       && p4.R <= R && p6.R > R
                        && p7.R > R && p8.R <= R && p9.R <= R)
                    {
                        bmp_p[x, y] += (255 - p4.R + 255 - p8.R) / 2;
                    }
                    else if (p1.R <= R && p2.R > R && p3.R > R
                       && p4.R <= R && p6.R > R
                       && p7.R > R && p8.R <= R && p9.R <= R)
                    {
                        bmp_p[x, y] += (255 - p4.R + 255 - p8.R) / 2;
                    }


                    // ��������
                    else if (p2.R <= R && p8.R <= R)
                    {
                        if (p1.R > R && p3.R > R
                            && p4.R > R && p6.R > R
                            && p7.R > R && p9.R > R)
                        {
                            bmp_p[x, y] += (255 - p2.R + 255 - p8.R) / 2;
                        }
                        //else if (R < 20)
                        //{
                        //    bmp_p[x, y] += (255 - p2.R + 255 - p8.R) / 2;
                        //}
                    }
                    // ��������
                    else if (p4.R <= R && p6.R <= R)
                    {
                        if (p1.R > R && p2.R > R && p3.R > R
                        && p7.R > R && p8.R > R && p9.R > R)
                        {
                            bmp_p[x, y] += (255 - p4.R + 255 - p6.R) / 2;
                        }
                        //else if (R < 20)
                        //{
                        //    bmp_p[x, y] += (255 - p4.R + 255 - p6.R) / 2;
                        //}
                    }

                    //������һ��
                    else if (p1.R > R && p2.R <= R && p3.R > R
                        && p4.R > R && p6.R > R
                        && p7.R > R && p8.R > R && p9.R > R)
                    {
                        bmp_p[x, y] += 255 - p2.R;
                    }
                    else if (p1.R > R && p2.R > R && p3.R > R
                        && p4.R <= R && p6.R > R
                        && p7.R > R && p8.R > R && p9.R > R)
                    {
                        bmp_p[x, y] += 255 - p4.R;
                    }
                    else if (p1.R > R && p2.R > R && p3.R > R
                       && p4.R > R && p6.R <= R
                       && p7.R > R && p8.R > R && p9.R > R)
                    {
                        bmp_p[x, y] += 255 - p6.R;
                    }
                    else if (p1.R > R && p2.R > R && p3.R > R
                      && p4.R > R && p6.R > R
                      && p7.R > R && p8.R <= R && p9.R > R)
                    {
                        bmp_p[x, y] += 255 - p8.R;
                    }

                   // //���������� ��·
                   // else if (p1.R > R && p2.R <= R && p3.R > R
                   //&& p4.R > R && p6.R > R
                   //&& p7.R > R && p8.R > R && p9.R > R)
                   // {
                   //     bmp_p[x, y] += 255 - R;
                   // }
                   // else if (p1.R <= R && p2.R <= R && p3.R > R
                   //     && p4.R > R && p6.R > R
                   //     && p7.R > R && p8.R > R && p9.R > R)
                   // {
                   //     bmp_p[x, y] += 255 - R;
                   // }
                   // else if (p1.R > R && p2.R <= R && p3.R <= R
                   //     && p4.R > R && p6.R > R
                   //     && p7.R > R && p8.R > R && p9.R > R)
                   // {
                   //     bmp_p[x, y] += 255 - R;
                   // }
                   // //���������� ��·
                   // else if (p1.R > R && p2.R > R && p3.R > R
                   //     && p4.R <= R && p6.R > R
                   //     && p7.R > R && p8.R > R && p9.R > R)
                   // {
                   //     bmp_p[x, y] += 255 - R;
                   // }
                   // else if (p1.R <= R && p2.R > R && p3.R > R
                   //     && p4.R <= R && p6.R > R
                   //     && p7.R > R && p8.R > R && p9.R > R)
                   // {
                   //     bmp_p[x, y] += 255 - R;
                   // }
                   // else if (p1.R > R && p2.R > R && p3.R > R
                   //     && p4.R <= R && p6.R > R
                   //     && p7.R <= R && p8.R > R && p9.R > R)
                   // {
                   //     bmp_p[x, y] += 255 - R;
                   // }

                   // //���������� ��·
                   // else if (p1.R > R && p2.R > R && p3.R > R
                   //    && p4.R > R && p6.R <= R
                   //    && p7.R > R && p8.R > R && p9.R > R)
                   // {
                   //     bmp_p[x, y] += 255 - R;
                   // }
                   // else if (p1.R > R && p2.R > R && p3.R <= R
                   //     && p4.R > R && p6.R <= R
                   //     && p7.R > R && p8.R > R && p9.R > R)
                   // {
                   //     bmp_p[x, y] += 255 - R;
                   // }
                   // else if (p1.R > R && p2.R > R && p3.R > R
                   //     && p4.R > R && p6.R <= R
                   //     && p7.R > R && p8.R > R && p9.R <= R)
                   // {
                   //     bmp_p[x, y] += 255 - R;
                   // }

                   // //���������� ��·
                   // else if (p1.R > R && p2.R > R && p3.R > R
                   //     && p4.R > R && p6.R > R
                   //     && p7.R > R && p8.R <= R && p9.R > R)
                   // {
                   //     bmp_p[x, y] += 255 - R;
                   // }
                   // else if (p1.R > R && p2.R > R && p3.R > R
                   //     && p4.R > R && p6.R > R
                   //     && p7.R <= R && p8.R <= R && p9.R > R)
                   // {
                   //     bmp_p[x, y] += 255 - R;
                   // }
                   // else if (p1.R > R && p2.R > R && p3.R > R
                   //     && p4.R > R && p6.R > R
                   //     && p7.R > R && p8.R <= R && p9.R <= R)
                   // {
                   //     bmp_p[x, y] += 255 - R;
                   // }

                    ////��һ��
                    //else if (p1.R <= R && p2.R <= R && p3.R <= R
                    //      && p4.R > R && p6.R > R
                    //      && p7.R > R && p8.R > R && p9.R > R)
                    //{
                    //    bmp_p[x, y] += 255 - p4.R + 255 - p6.R;
                    //}
                    ////��һ��
                    //else if (p1.R > R && p2.R > R && p3.R <= R
                    // && p4.R > R && p6.R <= R
                    // && p7.R > R && p8.R > R && p9.R <= R)
                    //{
                    //    bmp_p[x, y] += 255 - p2.R + 255 - p8.R;
                    //}
                    ////��һ��
                    //else if (p1.R > R && p2.R > R && p3.R > R
                    //      && p4.R > R && p6.R > R
                    //      && p7.R <= R && p8.R <= R && p9.R <= R)
                    //{
                    //    bmp_p[x, y] += 255 - p4.R + 255 - p6.R;
                    //}
                    ////��һ��
                    //else if (p1.R <= R && p2.R > R && p3.R > R
                    //    && p4.R <= R && p6.R > R
                    //    && p7.R <= R && p8.R > R && p9.R > R)
                    //{
                    //    bmp_p[x, y] += 255 - p2.R + 255 - p8.R;
                    //}
                }
            }
        }

        setFrame_do(bmp_p, p, bmp_w, bmp_h, bmp_iWidth);
    }
    void setFrame3(byte* p, uint Oavg, uint bmp_w, uint bmp_h, uint bmp_iWidth, int iw, int ih)
    {
        for (uint y = 0; y < bmp_h; y++)
        {
            for (uint x = 0; x < bmp_w; x++)
            {
                //��ʼ��
                uint ib = y * bmp_iWidth + x * 3;
                byte R = p[ib];

                myDot p1 = getDot(p, (int)x, (int)y, -1, -1, iw, ih, bmp_iWidth);//p1
                myDot p2 = getDot(p, (int)x, (int)y, 0, -1, iw, ih, bmp_iWidth);//p2
                myDot p3 = getDot(p, (int)x, (int)y, 1, -1, iw, ih, bmp_iWidth);//p3
                myDot p4 = getDot(p, (int)x, (int)y, -1, 0, iw, ih, bmp_iWidth);//p4
                myDot p6 = getDot(p, (int)x, (int)y, 1, 0, iw, ih, bmp_iWidth);//p6
                myDot p7 = getDot(p, (int)x, (int)y, -1, 1, iw, ih, bmp_iWidth);//p7
                myDot p8 = getDot(p, (int)x, (int)y, 0, 1, iw, ih, bmp_iWidth);//p8
                myDot p9 = getDot(p, (int)x, (int)y, 1, 1, iw, ih, bmp_iWidth);//p9

                if (p1 != null && p2 != null && p3 != null
                    && p4 != null && p6 != null
                    && p7 != null && p8 != null && p9 != null)
                {
                    int it = 0;

                    if (p1.R <= iThisOavg) it++;
                    if (p2.R <= iThisOavg) it++;
                    if (p3.R <= iThisOavg) it++;
                    if (p4.R <= iThisOavg) it++;
                    if (p6.R <= iThisOavg) it++;
                    if (p7.R <= iThisOavg) it++;
                    if (p8.R <= iThisOavg) it++;
                    if (p9.R <= iThisOavg) it++;

                    if (p4.R <= iThisOavg && p4.R < p1.R && p4.R < p7.R
                        && (p6.R < iThisOavg * 1.5 || p3.R < iThisOavg * 1.5 || p9.R < iThisOavg * 1.5))
                    {
                        p[ib] = p[ib + 1] = p[ib + 2] = (byte)(R / 3);
                    }

                    else if (p2.R <= iThisOavg && p2.R < p1.R && p2.R < p3.R
                        && (p8.R < iThisOavg * 1 || p7.R < iThisOavg * 1 || p9.R < iThisOavg * 1))
                    {
                        p[ib] = p[ib + 1] = p[ib + 2] = (byte)(R / 3);
                    }
                }
            }
        }

        for (uint y = bmp_h - 1; y > 0; y--)
        {
            for (uint x = bmp_w - 1; x > 0; x--)
            {
                //��ʼ��
                uint ib = y * bmp_iWidth + x * 3;
                byte R = p[ib];

                myDot p1 = getDot(p, (int)x, (int)y, -1, -1, iw, ih, bmp_iWidth);//p1
                myDot p2 = getDot(p, (int)x, (int)y, 0, -1, iw, ih, bmp_iWidth);//p2
                myDot p3 = getDot(p, (int)x, (int)y, 1, -1, iw, ih, bmp_iWidth);//p3
                myDot p4 = getDot(p, (int)x, (int)y, -1, 0, iw, ih, bmp_iWidth);//p4
                myDot p6 = getDot(p, (int)x, (int)y, 1, 0, iw, ih, bmp_iWidth);//p6
                myDot p7 = getDot(p, (int)x, (int)y, -1, 1, iw, ih, bmp_iWidth);//p7
                myDot p8 = getDot(p, (int)x, (int)y, 0, 1, iw, ih, bmp_iWidth);//p8
                myDot p9 = getDot(p, (int)x, (int)y, 1, 1, iw, ih, bmp_iWidth);//p9

                if (p1 != null && p2 != null && p3 != null
                    && p4 != null && p6 != null
                    && p7 != null && p8 != null && p9 != null)
                {
                    int it = 0;

                    if (p1.R <= iThisOavg) it++;
                    if (p2.R <= iThisOavg) it++;
                    if (p3.R <= iThisOavg) it++;
                    if (p4.R <= iThisOavg) it++;
                    if (p6.R <= iThisOavg) it++;
                    if (p7.R <= iThisOavg) it++;
                    if (p8.R <= iThisOavg) it++;
                    if (p9.R <= iThisOavg) it++;

                    if (p6.R <= iThisOavg && p6.R < p3.R && p6.R < p9.R
                        && (p4.R < iThisOavg * 1.5 || p1.R < iThisOavg * 1.5 || p7.R < iThisOavg * 1.5))
                    {
                        p[ib] = p[ib + 1] = p[ib + 2] = (byte)(R / 3);
                    }

                    else if (p8.R <= iThisOavg && p8.R < p7.R && p8.R < p9.R
                        && (p2.R < iThisOavg * 1 || p1.R < iThisOavg * 1 || p3.R < iThisOavg * 1))
                    {
                        p[ib] = p[ib + 1] = p[ib + 2] = (byte)(R / 3);
                    }
                }
            }
        }
    }
    void setFrame_cut1(byte* p, uint Oavg, uint bmp_w, uint bmp_h, uint bmp_iWidth, int iw, int ih)
    {
        int[,] bmp_p = new int[bmp_w, bmp_h];

        for (uint y = 0; y < bmp_h; y++)
        {
            for (uint x = 0; x < bmp_w; x++)
            {
                //��ʼ��
                uint ib = y * bmp_iWidth + x * 3;
                byte R = p[ib];

                myDot p1 = getDot(p, (int)x, (int)y, -1, -1, iw, ih, bmp_iWidth);//p1
                myDot p2 = getDot(p, (int)x, (int)y, 0, -1, iw, ih, bmp_iWidth);//p2
                myDot p3 = getDot(p, (int)x, (int)y, 1, -1, iw, ih, bmp_iWidth);//p3
                myDot p4 = getDot(p, (int)x, (int)y, -1, 0, iw, ih, bmp_iWidth);//p4
                myDot p6 = getDot(p, (int)x, (int)y, 1, 0, iw, ih, bmp_iWidth);//p6
                myDot p7 = getDot(p, (int)x, (int)y, -1, 1, iw, ih, bmp_iWidth);//p7
                myDot p8 = getDot(p, (int)x, (int)y, 0, 1, iw, ih, bmp_iWidth);//p8
                myDot p9 = getDot(p, (int)x, (int)y, 1, 1, iw, ih, bmp_iWidth);//p9

                if (p1 != null && p2 != null && p3 != null
                    && p4 != null && p6 != null
                    && p7 != null && p8 != null && p9 != null)
                {
                    int it = 0;
                    if (p1.R <= R) it++;
                    if (p2.R <= R) it++;
                    if (p3.R <= R) it++;
                    if (p4.R <= R) it++;
                    if (p6.R <= R) it++;
                    if (p7.R <= R) it++;
                    if (p8.R <= R) it++;
                    if (p9.R <= R) it++;


                    int nCount = 0;
                    if (p1.R > R && p2.R <= R) nCount++;
                    if (p2.R > R && p3.R <= R) nCount++;
                    if (p3.R > R && p6.R <= R) nCount++;
                    if (p6.R > R && p9.R <= R) nCount++;
                    if (p9.R > R && p8.R <= R) nCount++;
                    if (p8.R > R && p7.R <= R) nCount++;
                    if (p7.R > R && p4.R <= R) nCount++;
                    if (p4.R > R && p1.R <= R) nCount++;

                    if (it >= 4 && nCount == 1)
                    {
                        //���������ӵ�
                        if ((p2.R <= R && p8.R <= R) || (p4.R <= R && p6.R <= R))
                        {
                            //bmp_p[x, y] += (255 - R) / 2;
                        }
                        else
                        {
                            bmp_p[x, y] -= (255 - R) / 3;
                        }
                    }
                }
            }
        }
        setFrame_do(bmp_p, p, bmp_w, bmp_h, bmp_iWidth);
        
    }

    void setFrame_do(int[,] bmp_p, byte* p, uint bmp_w, uint bmp_h, uint bmp_iWidth)
    {
        for (uint y = 0; y < bmp_h; y++)
        {
            for (uint x = 0; x < bmp_w; x++)
            {
                //��ʼ��
                int iAdd = bmp_p[x, y];
                if (iAdd != 0)
                {
                    uint ib = y * bmp_iWidth + x * 3;
                    byte R = p[ib];
                    if (iAdd > 0)
                    {
                        if (R < iAdd)
                        {
                            p[ib] = p[ib + 1] = p[ib + 2] = 0;
                        }
                        else
                        {
                            p[ib] = p[ib + 1] = p[ib + 2] = (byte)(R - iAdd);
                        }
                    }
                    else if (iAdd < 0)
                    {
                        if (R - iAdd >= 255)
                        {
                            p[ib] = p[ib + 1] = p[ib + 2] = 255;
                        }
                        else
                        {
                            p[ib] = p[ib + 1] = p[ib + 2] = (byte)(R - iAdd);
                        }
                    }
                }
            }
        }
    }
    void setFrame_do2(int[,] bmp_p, byte* p, uint bmp_w, uint bmp_h, uint bmp_iWidth)
    {
        for (uint y = 0; y < bmp_h; y++)
        {
            for (uint x = 0; x < bmp_w; x++)
            {
                //��ʼ��
                int iAdd = bmp_p[x, y];
                if (iAdd != -1)
                {
                    uint ib = y * bmp_iWidth + x * 3;
                    p[ib] = p[ib + 1] = p[ib + 2] = (byte)iAdd;
                }
            }
        }
    }

    
    public myDot getDot(byte* p, int ix, int iy, int ichax, int ichay, int iw, int ih, uint bmp_iWidth)
    {
        int thisx = ix + ichax;
        int thisy = iy + ichay;
        if (thisx >= 0 && thisx < iw && thisy >= 0 && thisy < ih)
        {
            return new myDot((uint)thisx, (uint)thisy, bmp_iWidth, p);
        }
        return null;
    }


    public enum textModeSize : int
    {
        w_40,
        w_35,
        w_30,
        w_25,
    }
    //����ѵ���ļ�
    public void createTextMode(string sword, Rect.OcrMode ocrMode, textModeSize modeSize, string font)
    {
        imgSave.iQ = 100;

        int iFrame = 0;
        int iFontSize = 0;
        int ixc = 0;
        int iyc = 0;
        int iCheng = 0;
        int iChu = 0;

        //if (modeSize == textModeSize.w_100)
        //{
        //    iFrame = 100;
        //    iFontSize = 81;
        //    ixc = -23;
        //    iyc = -4;
        //    iCheng = 3;
        //    iChu = 2;
        //}
        if (modeSize == textModeSize.w_40)
        {
            iFrame = 40;
            iFontSize = 32;
            ixc = -9;
            iyc = -1;
            iCheng = 8;
            iChu = 8;
        }
        else if (modeSize == textModeSize.w_35)
        {
            iFrame = 35;
            iFontSize = 28;
            ixc = -7;
            iyc = -1;
            iCheng = 5;
            iChu = 4;
        }
        else if (modeSize == textModeSize.w_30)
        {
            iFrame = 30;
            iFontSize = 25;
            ixc = -7;
            iyc = -2;
            iCheng = 5;
            iChu = 4;
        }
        else if (modeSize == textModeSize.w_25)
        {
            iFrame = 25;
            iFontSize = 20;
            ixc = -5;
            iyc = -1;
            iCheng = 5;
            iChu = 4;
        }

        myDC = new myDataConn("oledb", Application.StartupPath + "\\words_" + iFrame + "\\words.mdb");
        myDC.open();

        if (ocrMode == Rect.OcrMode.Int || ocrMode == Rect.OcrMode.English) { ixc = 0; iyc -= 2; }

        sWord = "";
        for (int i = 0; i < sword.Length; i++)
        {
            string word = sword[i].ToString();
            sWord = word;
            Bitmap bmp = new Bitmap(iFrame, iFrame);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            g.DrawString(word, new Font(font, iFontSize), new SolidBrush(Color.Black), ixc, iyc);


            Bitmap bmp2 = new Bitmap(iFrame / iCheng * iChu, iFrame / iCheng * iChu);
            Graphics g2 = Graphics.FromImage(bmp2);
            g2.Clear(Color.White);
            g2.DrawImage(bmp, 0, 0, iFrame / iCheng * iChu, iFrame / iCheng * iChu);
            g2.Dispose();

            g.Clear(Color.White);
            g.DrawImage(bmp2, 0, 0, iFrame, iFrame);
            g.Dispose();

            string dir = Application.StartupPath + "\\words_" + iFrame + "";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string path = dir + "\\" + word + ".jpg";
            if ("ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(word) > -1)
            {
                //��д����
                path = dir + "\\" + word + word + ".jpg";
            }
            //if (word != "")
            //{
                imgSave.Save(bmp, path);
            //}

            imgSave.iQ = 100;
            bmp.Dispose();
            //string path = @"D:\C#\OcrWord\OcrWord\bin\Debug\words_" + iFrame + "\��.jpg";
            //createImage(path);
            //path = @"D:\C#\OcrWord\OcrWord\bin\Debug\words_" + iFrame + "\��2.jpg";
            createImage(path, ocrMode, iFrame);
        }

        myDC.close();
    }
    bool isModeTrain = false;
    //����ѵ���ļ�
    public void createImage(string path, Rect.OcrMode ocrMode, int iframes)
    {
        isModeTrain = true;
        FileInfo file = new FileInfo(path);

        Bitmap bitmap = (Bitmap)Image.FromFile(path);
        BitmapData bitData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        //ȡ��ÿ�ж��������
        
        //ȡ��ȫ������ֵ ÿ�� 3��  Ϊ  R G B ÿ��ѭ��ɾ�� һ�������� xx ��
        byte* p = (byte*)(bitData.Scan0.ToPointer());

        uint ic = (uint)(bitData.Stride - bitData.Width * 3);
        w = (uint)bitData.Width;
        h = (uint)bitData.Height;
        //һ�е����ص�
        iWidth = w * 3 + ic;

        //��ֵ��
        toTwo(p, 220);

        

        //������С���� 
        RectangleL rect = cutFrame(p, w, h, iWidth);
        //imgSave.SaveNoDispose(bitmap, file.DirectoryName + "\\" + Path.GetFileNameWithoutExtension(file.Name) + "_��2.jpg");

        OcrResult result = getRectImg(p, rect, iframes, ocrMode);
        result.createWords(sWord, ocrMode, (float)Math.Round((float)rect.Width / (float)rect.Height, 3), 1);

        imgSave.SaveNoDispose(result.bmp, file.DirectoryName + "\\" + Path.GetFileNameWithoutExtension(file.Name) + "_��.jpg");
        result.bmp.Dispose();

        //imgSave.Save(bitmap, file.DirectoryName + "/" + Path.GetFileNameWithoutExtension(file.Name) + "_��.jpg");

        bitmap.UnlockBits(bitData);
        bitmap.Dispose();
    }

    int iImgCount = 0;
    /// <summary>����С����������ͼ��</summary>
    public OcrResult getRectImg(byte* p, RectangleL rect, int iframes, Rect.OcrMode ocrMode)
    {
        int ithisW = iframes;
        int ithisH = iframes;

        Bitmap bmp = new Bitmap((int)rect.Width, (int)rect.Height, PixelFormat.Format1bppIndexed);
        BitmapData bitData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        uint bmp_ic = (uint)(bitData.Stride - bitData.Width * 3);
        byte* bmp_p = (byte*)(bitData.Scan0.ToPointer());

        uint bmp_w = (uint)bitData.Width;
        uint bmp_h = (uint)bitData.Height;

        //һ�е����ص�
        uint bmp_iWidth = bmp_w * 3 + bmp_ic;

        for (uint y = rect.Y; y < rect.Y + rect.Height; y++)
        {
            for (uint x = rect.X; x < rect.X + rect.Width; x++)
            {
                uint ib = y * iWidth + x * 3;

                if (p[ib] == 255)
                {
                    //MessageBox.Show((y + iMoveH) + " " + (x + iMoveW));
                    uint bmp_ib = (uint)((y - rect.Y) * bmp_iWidth + (x - rect.X) * 3);
                    bmp_p[bmp_ib] = 255;
                    bmp_p[bmp_ib + 1] = 255;
                    bmp_p[bmp_ib + 2] = 255;
                }
            }
        }
        //��ë��
        //toDotClear(bmp_p, bmp_w, bmp_h, bmp_iWidth);

        Bitmap bitmap = null;
        Graphics g = null;

        RectangleL rectNew = null;
        //���⻯
        rectNew = cutFrame(bmp_p, bmp_w, bmp_h, bmp_iWidth);
        if (ocrMode == Rect.OcrMode.Chinese || ocrMode == Rect.OcrMode.Chinese_ts)
        {
            if ((float)rect.Height / (float)rect.Width < 0.5)
            {
                //�߶�С��һ����޸ĵȱ�����
                uint icc = (rectNew.Width - rectNew.Height) / 2;
                rectNew.Height = rectNew.Width;
                if (rectNew.Y - icc > 0)
                {
                    rectNew.Y -= icc;
                }
                else
                {
                    rectNew.Y = 0;
                }
            }
            
            //toBigLine(bmp_p, bmp_w, bmp_h, bmp_iWidth, 1, 1);
        }
        //else if (ocrMode == Rect.OcrMode.)
        //{
        //    toSmallLine(bmp_p, bmp_w, bmp_h, bmp_iWidth, -1);
        //}
        else
        {
            //С��80���ص�ͼ��ſ����ȹ�����
            if (rect.Height < 80)
            {
                toSmallLine(bmp_p, bmp_w, bmp_h, bmp_iWidth, -1);
            }
            //toSmallLine(bmp_p, bmp_w, bmp_h, bmp_iWidth, -1);
            //toBigLine(bmp_p, bmp_w, bmp_h, bmp_iWidth, 1, 1);

            //toBigLine(bmp_p, bmp_w, bmp_h, bmp_iWidth, 1, 0);
        }

        bmp.UnlockBits(bitData);
        

        ////////////////////////////////////////
        //ͳһ��С���ͼƬ
        ////////////////////////////////////////
        bitmap = new Bitmap(ithisW, ithisH);
        g = Graphics.FromImage(bitmap);
        g.Clear(Color.White);
        if ((double)rect.Height / (double)rect.Width < 0.2)
        {
            g.DrawImage(bmp, new Rectangle(0, (ithisH - (int)rectNew.Height) / 2, ithisW, (int)rectNew.Height), new Rectangle((int)rectNew.X, (int)rectNew.Y, (int)rectNew.Width, (int)rectNew.Height), GraphicsUnit.Pixel);
        }
        else
        {
            g.DrawImage(bmp, new Rectangle(0, 0, ithisW, ithisH), new Rectangle((int)rectNew.X, (int)rectNew.Y, (int)rectNew.Width, (int)rectNew.Height), GraphicsUnit.Pixel);
        }
        g.Dispose();
        //bitmap.Save(Application.StartupPath + "\\test\\ͳһ����.jpg", myImageCodecInfo, encoderParams);
        bmp.Dispose();

        //imgSave.Save(bitmap, @"D:\C#\OcrWord\OcrWord\bin\Debug\test\cc.jpg");

        //bitmap = (Bitmap)Image.FromFile(@"D:\C#\OcrWord\OcrWord\bin\Debug\test\cc.jpg");

        bitData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        bmp_ic = (uint)(bitData.Stride - bitData.Width * 3);
        bmp_p = (byte*)(bitData.Scan0.ToPointer());

        bmp_w = (uint)bitData.Width;
        bmp_h = (uint)bitData.Height;
        bmp_iWidth = bmp_w * 3 + bmp_ic;

        //ת����ͼ
        for (uint y = 0; y < bmp_h; y++)
        {
            for (uint x = 0; x < bmp_w; x++)
            {
                //��ʼ��
                uint ib = y * bmp_iWidth + x * 3;
                int igrey = (bmp_p[ib] + bmp_p[ib + 1] + bmp_p[ib + 2]) / 3;
                if (igrey <= 150)
                {
                    bmp_p[ib] = (byte)0;
                    bmp_p[ib + 1] = (byte)0;
                    bmp_p[ib + 2] = (byte)0;
                }
                else
                {
                    bmp_p[ib] = (byte)255;
                    bmp_p[ib + 1] = (byte)255;
                    bmp_p[ib + 2] = (byte)255;
                }
            }
        }

        //������
        toSmallLine(bmp_p, bmp_w, bmp_h, bmp_iWidth, -1);
        //���⻯
        toBigLine(bmp_p, bmp_w, bmp_h, bmp_iWidth, 1, 1);

        OcrResult result = new OcrResult(myDC, imgSave, ithisW);
        result.sDebugWord = sDebugWord;
        result.bmp = bitmap;

        //���񻯷���
        result.get_iCut(bmp_p, bmp_w, bmp_h, bmp_iWidth, ithisW, ithisH);

        bitmap.UnlockBits(bitData);

       
        iImgCount++;
        //MessageBox.Show("a");
        return result;
    }

    

    /// <summary>
    /// ��ͨ����ָ��㷨
    /// </summary>
    public unsafe OcrWords cutArea(byte* p, Rect.OcrMode ocrMode)
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
                    rect.Add(new myDot(x, y, iWidth, 0));

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
                                        rect.r = thisx;
                                    }
                                    if (thisy < rect.t)
                                    {
                                        rect.t = thisy;
                                    }
                                    else if (thisy > rect.b)
                                    {
                                        rect.b = thisy;
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

        //����ƽ���߶�
        float iPerH = 0;
        float iPerH_num = 0;

        //������ϲ�
        bool isHB = true;//�Ƿ��н���ϲ�������Ҫѭ���ϲ�

        ArrayList AL_HB = new ArrayList();
        int[] arrRemove = new int[Al_rect.Count];
        int itIndex = 0;
        while (isHB)
        {
            isHB = false;

            ArrayList al_hb = new ArrayList();
            for (int i = 0; i < Al_rect.Count; i++)
            {
                rect = (Rect)Al_rect[i];
                rect.calculate();

                if (itIndex >= 1 && arrRemove[i] == 1)
                {
                    //�ڶ���ʱ�����ܹ���ֱ��ȥ��
                    //MessageBox.Show("a");
                    continue;
                }

                bool isIn = false;
                //����һ��ѭ������
                if (itIndex == 0)
                {
                    //��ȫ������
                    for (int j = 0; j < Al_rect.Count; j++)
                    {
                        if (i != j && arrRemove[j] != 1)
                        {
                            Rect r = (Rect)Al_rect[j];
                            if (r.l <= rect.l && r.r >= rect.r && r.t <= rect.t && r.b >= rect.b)
                            {
                                arrRemove[i] = 1;
                                //rect.Draw(p, iWidth, 0, 255, 0);
                                isIn = true;
                                break;
                            }
                        }
                    }
                }

                if (!isIn)
                {
                    ////�ཻ�ͺϲ�
                    for (int j = 0; j < Al_rect.Count; j++)
                    {
                        if (i != j && arrRemove[j] != 1)
                        {
                            Rect r = (Rect)Al_rect[j];
                            ////�ཻ�ͺϲ�
                            if ((r.l <= rect.l && r.r >= rect.l && r.t <= rect.t && r.b >= rect.t)
                                || (r.l <= rect.r && r.r >= rect.r && r.t <= rect.t && r.b >= rect.t)
                                || (r.l <= rect.l && r.r >= rect.l && r.t <= rect.b && r.b >= rect.b)
                                || (r.l <= rect.r && r.r >= rect.r && r.t <= rect.b && r.b >= rect.b)
                                )
                            {
                                r.AddRect(rect);
                                //r.Draw(p, iWidth);
                                //rect.Draw(p, iWidth, 255, 0, 0);
                                arrRemove[i] = 1;
                                isIn = true;
                                isHB = true;
                                break;
                            }
                        }
                    }
                }

                if (!isIn)
                {
                    //δ���ϲ������� ȥ����С���� ���С�ĵ�
                    float iw = (float)rect.width;
                    float ih = (float)rect.height;
                    float pp = 0;
                    if (iw > ih)
                    {
                        pp = ih / iw;
                    }
                    else
                    {
                        pp = iw / ih;
                    }
                    
                    if (rect.iNum > 4)
                    {
                        //���������
                        if (pp >= 0.35)
                        {
                            iPerH += ih * rect.Al_dot.Count;
                            iPerH_num += rect.Al_dot.Count;
                        }
                        al_hb.Add(rect);
                    }
                    else
                    {
                        rect.Clear(p);
                    }
                }
            }
            if (!isHB)
            {
                AL_HB = al_hb;
            }
            itIndex++;
        }

        

        //�õ����������ƽ���߶�
        if (iPerH_num != 0)
        {
            iPerH = (float)Math.Round(iPerH / iPerH_num);
        }
        else
        {
            iPerH = -1;
        }
        //MessageBox.Show(iPerH + " ");

        //����һ�����ж���������
        ArrayList AL_noin = new ArrayList();
        ArrayList AL_Line = new ArrayList();
        for (int i = 0; i < AL_HB.Count; i++)
        {
            rect = (Rect)AL_HB[i];
            //MessageBox.Show(iPerH + "");
            bool isNew = true;
            for (int j = 0; j < AL_Line.Count; j++)
            {
                LineRow line = (LineRow)AL_Line[j];
                if (Math.Abs(line.iCenterV - rect.pCenter.Y) <= iPerH / 2 * 1.1)
                {
                    //��ͬ�е������������
                    line.Add(rect);
                    //rect.Draw(p, iWidth);
                    isNew = false;
                    break;
                }
            }
            if (isNew)
            {
                if (rect.height > 5 && iPerH != -1)
                {
                    //û���ʺϵĵ㴴���µ�һ��
                    LineRow line = new LineRow(this);
                    line.Add(rect);
                    //rect.Draw(p, iWidth);
                    //MessageBox.Show(rect.pCenter.Y + "");
                    AL_Line.Add(line);
                }
                else
                {
                    AL_noin.Add(rect);
                }
            }
        }
       
        //�鿴�Ƿ���Ҫ����ĳһ��
        for (int i = 0; i < AL_noin.Count; i++)
        {
            rect = (Rect)AL_noin[i];

            for (int j = 0; j < AL_Line.Count; j++)
            {
                LineRow line = (LineRow)AL_Line[j];
                if ((line.l <= rect.l && line.r >= rect.l && line.t <= rect.t && line.b >= rect.t)
                    || (line.l <= rect.r && line.r >= rect.r && line.t <= rect.t && line.b >= rect.t)
                    || (line.l <= rect.l && line.r >= rect.l && line.t <= rect.b && line.b >= rect.b)
                    || (line.l <= rect.r && line.r >= rect.r && line.t <= rect.b && line.b >= rect.b)
                    )
                {
                    line.Add(rect);
                    break;
                }
            }
        }

        
        
        int icount = 0;
        string str = "";
        
        OcrWords ocrWords = new OcrWords();

        //��¼��һ�����
        OcrResult ResultPrev = null;
        for (int i = 0; i < AL_Line.Count; i++)
        {
            LineRow line = (LineRow)AL_Line[i];
            line.Count = AL_Line.Count;
            line.Index = i;
            line.RectSort();
            line.RectReset(p, ocrMode);

            int iIndex = ocrWords.CreateLineRow(line.arrRect.Length);

            for (int j = 0; j < line.arrRect.Length; j++)
            {
                icount++;
                //if (icount >= 33 && icount <= 35)
                //if (icount == 1)
                if (iOcrWord == -1 || icount == iOcrWord)
                {
                    rect = (Rect)line.arrRect[j];

                    OcrResult result = OcrRect(p, rect, icount, ResultPrev, "����(" + (j + 1) + "/" + line.arrRect.Length + ") ������(" + (i + 1) + "/" + AL_Line.Count + ")", true);
                    ocrWords.AddResult(iIndex, j, result); //������
                    //MessageBox.Show(ocrWords.getRectString());

                    if (form != null)
                    {
                        //�޸���һ������
                        //string sPrev = "";
                        //if (ResultPrev != null) sPrev = ResultPrev.arrName[0];
                        //string sNow = result.getResult();

                        //form.BeginInvoke(form.IK_word, new Object[] { ocrWords.sALL_words });
                    }

                    //��¼��һ�����
                    ResultPrev = result;
                }
            }
        }

        //AciDebug.Debug(str);
        //MessageBox.Show(str);

        //MessageBox.Show(rect.l + " " + rect.t + " " + rect.r + " " + rect.b + " " + (uint)rect.width + " " + (uint)rect.height);


        //�������е�����
        if (show)
        {
            for (int i = 0; i < AL_Line.Count; i++)
            {
                LineRow line = (LineRow)AL_Line[i];
                line.Draw(p, w, h, iWidth);
                line.DrawRect(p, w, h, iWidth);
            }
        }

        return ocrWords;

    }

    

    /// <summary>
    /// OCR����ʶ��
    /// </summary>
    /// <param name="p"></param>
    /// <param name="rect"></param>
    /// <param name="icount"></param>
    /// <param name="ResultPrev"></param>
    /// <param name="sprocess"></param>
    /// <param name="isRectResult">�Ƿ�ʹ��rect�Դ��� OcrResult</param>
    /// <returns></returns>
    public OcrResult OcrRect(byte* p, Rect rect, int icount, OcrResult ResultPrev, string sprocess, bool isRectResult)
    {
        RectangleL rectangle = new RectangleL(rect.l, rect.t, (uint)rect.width, (uint)rect.height);
        //if (icount == 3) MessageBox.Show(rectangle.DebugString());

        //�Զ�����ʹ�õ�������
        int iframe = 0;
        if (rect.width > rect.height)
        {
            iframe = rect.width;
        }
        else
        {
            iframe = rect.height;
        }
        int toframe = 40;

        OcrResult result = null;
        if (isRectResult && rect.result != null)
        {
            result = rect.result;
        }
        else
        {
            result = getRectImg(p, rectangle, toframe, rect.iOcrMode);
            string swhere = "";
            if (sOcr_In != "") swhere = " and name in(" + getSqlWord(sOcr_In) + ")";
            if (sOcr_NotIn != "") swhere = " and name not in(" + getSqlWord(sOcr_NotIn) + ")";
            
            result.Ocr(ds, rect, this, icount, sprocess, swhere);
            //����ʶ��ʱ��ʶ������ֵ���� 200  �� ǰ��ʶ������������
            if (rect.iOcrMode == Rect.OcrMode.Chinese && result.arrMin[0] > -1 && result.arrMin[1] > -1
                && (result.arrMin[0] > 200 || (float)result.arrMin[1] / (float)result.arrMin[0] < 2))
            {
                result.isCheckLine = false;
                result.Ocr(ds, rect, this, icount, sprocess, swhere);
                //MessageBox.Show("a");
            }
        }
        

       
        result.bmp.Dispose();

        //AciDebug.Debug(s);
        //MessageBox.Show(s);

        return result;
    }
    string getSqlWord(string sword)
    {
        StringBuilder str = new StringBuilder();
        for (int i = 0; i < sword.Length; i++)
        {
            if (str.Length != 0)
            {
                str.Append(",");
            }
            str.Append("'" + sword[i] + "'");
        }
        return str.ToString();
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


    public class RectangleL
    {
        public uint X = 0;
        public uint Y = 0;
        public uint Width = 0;
        public uint Height = 0;
        public RectangleL(uint x, uint y, uint width, uint height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        public void Debug()
        {
            AciDebug.Debug(X + " " + Y + " " + Width + " " + Height);
        }
        public string DebugString()
        {
            return "��" + X + " �ң�" + Y + "�ϣ�" + Width + " �£�" + Height;
        }
    }
    /// <summary>������С����</summary>
    public RectangleL cutFrame(byte* p, uint bmp_w, uint bmp_h, uint bmp_iWidth)
    {
        RectangleL rect = new RectangleL(0, 0, 0, 0);

        //���϶���
        bool isOK = false;
        for (uint y = 0; y < bmp_h; y++)
        {
            for (uint x = 0; x < bmp_w; x++)
            {
                uint ib = y * bmp_iWidth + x * 3;
                if (p[ib] == 0)
                {
                    //���϶���
                    rect.Y = y;
                    isOK = true;
                    break;
                }
            }
            if (isOK) break;
        }

        //���¶���
        isOK = false;
        for (uint y = bmp_h - 1; y >= 0; y--)
        {
            for (uint x = 0; x < bmp_w; x++)
            {
                uint ib = y * bmp_iWidth + x * 3;
                if (p[ib] == 0)
                {
                    //���¶���
                    rect.Height = y - rect.Y + 1;
                    isOK = true;
                    break;
                }
            }
            if (isOK) break;
        }

        //���󶨵�
        isOK = false;
        for (uint x = 0; x < bmp_w; x++)
        {
            for (uint y = 0; y < bmp_h; y++)
            {
                uint ib = y * bmp_iWidth + x * 3;
                if (p[ib] == 0)
                {
                    //���󶨵�
                    rect.X = x;
                    isOK = true;
                    break;
                }
            }
            if (isOK) break;
        }

        //���Ҷ���
        isOK = false;
        for (uint x = bmp_w - 1; x >= 0; x--)
        {
            for (uint y = 0; y < bmp_h; y++)
            {
                uint ib = y * bmp_iWidth + x * 3;
                if (p[ib] == 0)
                {
                    //���Ҷ���
                    rect.Width = x - rect.X + 1;
                    isOK = true;
                    break;
                }
            }
            if (isOK) break;
        }

        //rect.Debug();

        return rect;
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
                int igrey = (p[ib] + p[ib + 1] + p[ib + 2]) / 3;
                if (igrey <= Ostu)
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
    ArrayList AL_ClearH = null;
    ArrayList AL_ClearV = null;
    /// <summary>
    /// �����͹�� ����ϸͳһ��
    /// </summary>
    public unsafe void toDotClear(byte* p, uint bmp_w, uint bmp_h, uint bmp_iWidth)
    {
        bool isClear = true;
        while (isClear)
        {
            isClear = false;
            if (SmallLineX(p, bmp_w, bmp_h, bmp_iWidth)) isClear = true;
            if (SmallLineY(p, bmp_w, bmp_h, bmp_iWidth)) isClear = true;
        }
    }

    public unsafe string toDotTong(byte* p, uint bmp_w, uint bmp_h, uint bmp_iWidth)
    {
        AL_ClearH = new ArrayList();
        AL_ClearV = new ArrayList();
        SmallLineX_Only(p, bmp_w, bmp_h, bmp_iWidth);
        SmallLineY_Only(p, bmp_w, bmp_h, bmp_iWidth);

        for (int i = 0; i < AL_ClearV.Count; i++)
        {
            ArrayList AL_Now = (ArrayList)AL_ClearV[i];
            for (int j = 0; j < AL_Now.Count; j++)
            {
                Rect rect = (Rect)AL_Now[j];
                if (!rect.isHB)
                {
                    rect.Draw(p, bmp_iWidth);
                    MessageBox.Show(rect.DebugString());
                }
            }
        }
        //for (int i = 0; i < AL_ClearH.Count; i++)
        //{
        //    ArrayList AL_Now = (ArrayList)AL_ClearH[i];
        //    for (int j = 0; j < AL_Now.Count; j++)
        //    {
        //        Rect rect = (Rect)AL_Now[j];
        //        rect.Draw(p, bmp_iWidth);
        //    }
        //}

        return "";

        int iHouDu = 5;

        for (int i = 0; i < AL_ClearV.Count; i++)
        {
            ArrayList AL_Now = (ArrayList)AL_ClearV[i];

            for (int j = 0; j < AL_Now.Count; j++)
            {
                Rect rect = (Rect)AL_Now[j];
                if (!rect.isHB)
                {
                    if (rect.width != iHouDu)
                    {
                        int iNei = 0;
                        int iWai = 0;
                        byte iC_no = 0;
                        byte iC_yes = 255;
                        Rect r = null;

                        if (rect.width > iHouDu)
                        {
                            iNei = iHouDu;
                            iWai = rect.width;
                            iC_no = 0;
                            iC_yes = 255;
                        }
                        else
                        {
                            iNei = rect.width;
                            iWai = iHouDu;
                            iC_no = 0;
                            iC_yes = 0;
                        }

                        int iCha = iWai - iNei;

                        int iLiuB = 0;
                        int iLiuE = 0;

                        if (iCha % 2 == 0)
                        {
                            //��Ҫ��ȥ�Ĳ�����˫��
                            iLiuB = iCha / 2;
                            iLiuE = iLiuB + iNei - 1;

                            if (rect.width > iHouDu)
                            {
                                r = new Rect(rect.t, rect.r, rect.b, rect.l);
                            }
                            else
                            {
                                if ((int)rect.l - iLiuB < 0)
                                {
                                    r = new Rect(rect.t, rect.r + (uint)iLiuB, rect.b, 0);
                                }
                                else
                                {
                                    r = new Rect(rect.t, rect.r + (uint)iLiuB, rect.b, rect.l - (uint)iLiuB);
                                }
                            }
                        }
                        else
                        {
                            //��Ҫ��ȥ�Ĳ����ǵ���
                            if (rect.width > iHouDu)
                            {
                                //��խ
                                r = new Rect(rect.t, rect.r, rect.b, rect.l);
                                //��Ҫ��ȥ�Ĳ����ǵ���
                                if (r.l + r.width / 2 < bmp_w / 2)
                                {
                                    iLiuB = (iCha - 1) / 2;
                                }
                                else
                                {
                                    iLiuB = (iCha - 1) / 2 + 1;
                                }

                            }
                            else
                            {
                                //���
                                uint icc = (uint)(iCha - 1) / 2;

                                if (rect.l + rect.width / 2 > bmp_w / 2)
                                {
                                    iLiuB = (iCha - 1) / 2;
                                    if ((int)rect.l - icc < 0)
                                    {
                                        r = new Rect(rect.t, rect.r + icc + 1, rect.b, 0);
                                    }
                                    else
                                    {
                                        r = new Rect(rect.t, rect.r + icc + 1, rect.b, rect.l - icc);
                                    }
                                }
                                else
                                {
                                    iLiuB = (iCha - 1) / 2 + 1;
                                    if ((int)rect.l - icc - 1 < 0)
                                    {
                                        r = new Rect(rect.t, rect.r + icc + 1, rect.b, 0);
                                    }
                                    else
                                    {
                                        r = new Rect(rect.t, rect.r + icc, rect.b, rect.l - icc - 1);
                                    }
                                }
                            }
                            iLiuE = iLiuB + iNei - 1;
                        }
                        //MessageBox.Show(iLiuB + " " + iLiuE + " " + iWai + " " + iNei);


                        for (int k = 0; k < iLiuB; k++)
                        {
                            //MessageBox.Show(k + " " + iLiuB + " " + iLiuE);
                            uint imaxH = r.t + (uint)r.height - 1;
                            if (imaxH >= bmp_h)
                            {
                                imaxH = bmp_h - 1;
                            }

                            for (uint n = r.t; n <= imaxH; n++)
                            {
                                uint imaxW = r.l + (uint)k;
                                if (imaxW >= bmp_w)
                                {
                                    imaxW = bmp_w - 1;
                                }
                                //MessageBox.Show(imaxW + " " + n);

                                myDot dot = new myDot(imaxW, n, bmp_iWidth, 0);
                                //�����������ӵ㲻��ɾ��
                                int imaxL = (int)dot.x - 1;
                                if (imaxL < 0 || iC_yes == 0)
                                {
                                    p[dot.ib] = p[dot.ib + 1] = p[dot.ib + 2] = iC_yes;
                                }
                                else
                                {
                                    uint ib = dot.y * bmp_iWidth + (dot.x - 1) * 3;
                                    //MessageBox.Show(p[ib] + " " + iC_no);
                                    if (p[ib] != iC_no)
                                    {
                                        p[dot.ib] = p[dot.ib + 1] = p[dot.ib + 2] = iC_yes;
                                    }
                                }
                            }
                        }
                        for (int k = iWai - 1; k > iLiuE; k--)
                        {
                            //MessageBox.Show(k + " " + iLiuB + " " + iLiuE);
                            uint imaxH = r.t + (uint)r.height - 1;
                            if (imaxH >= bmp_h)
                            {
                                imaxH = bmp_h - 1;
                            }

                            for (uint n = r.t; n <= imaxH; n++)
                            {
                                uint imaxW = r.l + (uint)k;
                                if (imaxW >= bmp_w)
                                {
                                    imaxW = bmp_w - 1;
                                }
                                //MessageBox.Show(imaxW + " " + n);

                                myDot dot = new myDot(imaxW, n, bmp_iWidth, 0);

                                //�����������ӵ㲻��ɾ��
                                uint imaxR = dot.x + 1;
                                if (imaxR >= bmp_w || iC_yes == 0)
                                {
                                    p[dot.ib] = p[dot.ib + 1] = p[dot.ib + 2] = iC_yes;
                                }
                                else
                                {
                                    uint ib = dot.y * bmp_iWidth + (dot.x + 1) * 3;
                                    if (p[ib] != iC_no)
                                    {
                                        p[dot.ib] = p[dot.ib + 1] = p[dot.ib + 2] = iC_yes;
                                    }
                                }
                            }

                        }
                    }
                    //rect.Draw(p, bmp_iWidth);
                    //MessageBox.Show("������������" + rect.isHB + " " + rect.DebugString() + "     ");
                }
            }
        }
    }
    /// <summary>
    /// ��ȡ���ϲ����е� ���� 25% �� ����ֱ��
    /// </summary>
    /// <param name="p"></param>
    /// <param name="bmp_w"></param>
    /// <param name="bmp_h"></param>
    /// <param name="bmp_iWidth"></param>
    void SmallLineX_Only(byte* p, uint bmp_w, uint bmp_h, uint bmp_iWidth)
    {
        for (uint y = 0; y < bmp_h; y++)
        {
            ArrayList AL_Now = new ArrayList();
            //���е������߶�
            Rect rect = null;
            for (uint x = 0; x < bmp_w; x++)
            {
                myDot dot = new myDot(x, y, bmp_iWidth, p);
                if (dot.R == 0)
                {
                    if (rect == null)
                    {
                        //�������߶�
                        rect = new Rect(y, x, y, x);
                    }
                    //���뵽�߶�
                    rect.Add(dot);
                    rect.r = x;
                    rect.width = Math.Abs((int)rect.l - (int)rect.r) + 1;
                    rect.height = 1;
                }
                else
                {
                    //�����׵�Ͽ�
                    
                    if (rect != null)
                    {
                        //MessageBox.Show("a" + y + " " + rect.l + " " + rect.width);
                        if (rect.width > bmp_h / 4)
                        {
                            AL_Now.Add(rect);
                            //MessageBox.Show("b" + y + " " + rect.l + " " + rect.width);
                        }
                        rect = null;
                    }
                }
            }
            if (rect != null && rect.width > bmp_h / 4)
            {
                AL_Now.Add(rect);
               // MessageBox.Show(y + " " + rect.width);
            }
            
            AL_ClearH.Add(AL_Now);
        }

        for (int i = 0; i < AL_ClearH.Count; i++)
        {
            if (i != 0)
            {
                ArrayList AL_Prev = (ArrayList)AL_ClearH[i - 1];
                ArrayList AL_Now = (ArrayList)AL_ClearH[i];
                ArrayList AL_hb = new ArrayList();

                for (int j = 0; j < AL_Now.Count; j++)
                {
                    Rect rect = (Rect)AL_Now[j];
                    for (int k = 0; k < AL_Prev.Count + AL_hb.Count; k++)
                    {
                        Rect r = null;
                        if (k < AL_Prev.Count)
                        {
                            r = (Rect)AL_Prev[k];
                        }
                        else
                        {
                            r = (Rect)AL_hb[k - AL_Prev.Count];
                        }

                        //��� 1 ���ཻ���ߣ����кϲ� �²�Ӧ�����
                        if (!r.isHB && (rect.t - r.b == 1 || rect.b == r.b))
                        {
                            if (!r.isHB)
                            {
                                //��һ�������� ���ཻ���ִ��� 50% 
                                if ((rect.l <= r.l && r.l <= rect.r)
                                    && (rect.l <= r.r && r.r <= rect.r)
                                    )
                                {
                                    r.isHB = true;
                                    rect.AddRect(r);
                                    AL_hb.Add(rect);
                                    break;
                                }
                            }
                            if (!r.isHB)
                            {
                                //��һ�����ұ� ���ཻ���ִ��� 50% 
                                int prevLeft = (int)rect.r - (int)r.l;
                                //float iall = (float)(rect.width + r.width) / 2;
                                if (prevLeft > 0 && rect.r <= r.r)
                                {
                                    r.isHB = true;
                                    rect.AddRect(r);
                                    AL_hb.Add(rect);
                                    break;
                                }
                            }
                            if (!r.isHB)
                            {
                                //��һ������� ���ཻ���ִ��� 50% 
                                int prevRight = (int)r.r - (int)rect.l;
                                //float iall = (float)(rect.width + r.width) / 2;
                                if (prevRight > 0 && r.l <= rect.l)
                                {
                                    r.isHB = true;
                                    rect.AddRect(r);
                                    AL_hb.Add(rect);
                                    break;
                                }
                            }
                            // && (float)prevLeft / iall > 0.5
                        }
                        
                    }
                }
            }
        }
    }
    /// <summary>
    /// ��ȡ���ϲ����е� ���� 25% �� ����ֱ��
    /// </summary>
    /// <param name="p"></param>
    /// <param name="bmp_w"></param>
    /// <param name="bmp_h"></param>
    /// <param name="bmp_iWidth"></param>
    void SmallLineY_Only(byte* p, uint bmp_w, uint bmp_h, uint bmp_iWidth)
    {
        for (uint x = 0; x < bmp_w; x++)
        {
            ArrayList AL_Now = new ArrayList();
            //���е������߶�
            Rect rect = null;
            for (uint y = 0; y < bmp_h; y++)
            {
                myDot dot = new myDot(x, y, bmp_iWidth, p);
                if (dot.R == 0)
                {
                    if (rect == null)
                    {
                        //�������߶�
                        rect = new Rect(y, x, y, x);
                    }
                    //���뵽�߶�
                    rect.Add(dot);
                    rect.b = y;
                    rect.height = Math.Abs((int)rect.t - (int)rect.b) + 1;
                    rect.width = 1;
                }
                else
                {
                    //�����׵�Ͽ�
                    if (rect != null)
                    {
                        //MessageBox.Show("a" + y + " " + rect.l + " " + rect.width);
                        if (rect.height > bmp_h / 4)
                        {
                            AL_Now.Add(rect);
                            //MessageBox.Show("b" + y + " " + rect.l + " " + rect.width);
                        }
                        rect = null;
                    }
                }
            }

            if (rect != null && rect.height > bmp_h / 4)
            {
                AL_Now.Add(rect);
                // MessageBox.Show(y + " " + rect.width);
            }

            AL_ClearV.Add(AL_Now);
        }
        
        for (int i = 0; i < AL_ClearV.Count; i++)
        {
            if (i != 0)
            {
                ArrayList AL_Prev = (ArrayList)AL_ClearV[i - 1];
                ArrayList AL_Now = (ArrayList)AL_ClearV[i];
                ArrayList AL_hb = new ArrayList();
                
                for (int j = 0; j < AL_Now.Count; j++)
                {
                    Rect rect = (Rect)AL_Now[j];
                    for (int k = 0; k < AL_Prev.Count + AL_hb.Count; k++)
                    {
                        Rect r = null;
                        if (k < AL_Prev.Count)
                        {
                            r = (Rect)AL_Prev[k];
                        }
                        else
                        {
                            r = (Rect)AL_hb[k - AL_Prev.Count];
                        }
                        //��� 1 ���ཻ���ߣ����кϲ� �Ҳ�Ӧ�����
                        if (!r.isHB && (rect.l - r.r == 1 || rect.r == r.r))
                        {
                            if (!r.isHB)
                            {
                                //��һ�������� ���ཻ���ִ��� 50% 
                                if ((rect.t <= r.t && r.t <= rect.b)
                                    && (rect.t <= r.b && r.b <= rect.b)
                                    )
                                {
                                    r.isHB = true;
                                    rect.AddRect(r);
                                    AL_hb.Add(rect);
                                    break;
                                }
                            }
                            if (!r.isHB)
                            {
                                //��һ�����ұ� ���ཻ���ִ��� 50% 
                                int prevLeft = (int)rect.b - (int)r.t;
                                //float iall = (float)(rect.height + r.height) / 2;
                                if (prevLeft > 0 && rect.b <= r.b)
                                {
                                    r.isHB = true;
                                    rect.AddRect(r);
                                    AL_hb.Add(rect);
                                    break;
                                }
                            }
                            if (!r.isHB)
                            {
                                //��һ������� ���ཻ���ִ��� 50% 
                                int prevRight = (int)r.b - (int)rect.t;
                                //float iall = (float)(rect.height + r.height) / 2;
                                if (prevRight > 0 && r.t <= rect.t)
                                {
                                    r.isHB = true;
                                    rect.AddRect(r);
                                    AL_hb.Add(rect);
                                    break;
                                }
                            }
                            // && (float)prevLeft / iall > 0.5
                        }
                    }
                }
            }
        }
    }
    bool SmallLineX(byte* p, uint bmp_w, uint bmp_h, uint bmp_iWidth)
    {
        bool isClear = false;
        ArrayList AL_Prev = null;
        for (uint y = 0; y < bmp_h; y++)
        {
            ArrayList AL_Now = new ArrayList();
            //���е������߶�
            Rect rect = null;
            for (uint x = 0; x < bmp_w; x++)
            {
                myDot dot = new myDot(x, y, bmp_iWidth, p);
                if (dot.R == 0)
                {
                    if (rect == null)
                    {
                        //�������߶�
                        rect = new Rect(y, x, y, x);
                        AL_Now.Add(rect);
                    }
                    //���뵽�߶�
                    rect.Add(dot);
                    rect.r = x;
                    rect.width = Math.Abs((int)rect.l - (int)rect.r) + 1;
                    rect.height = 1;
                }
                else
                {
                    //�����׵�Ͽ�
                    if (rect != null) rect = null;
                }
            }
            //����һ��
            if (AL_Prev != null)
            {
                for (int k = 0; k < AL_Prev.Count; k++)
                {
                    rect = (Rect)AL_Prev[k];
                    for (int n = 0; n < AL_Now.Count; n++)
                    {
                        Rect r = (Rect)AL_Now[n];

                        if (SmallDotClear_X(rect, r, p, bmp_h, 1)) isClear = true;//r �Ƿ��� rect ��
                        if (SmallDotClear_X(r, rect, p, bmp_h, -1)) isClear = true;//rect �Ƿ��� r ��
                    }
                }
            }
            AL_Prev = AL_Now;
        }
        return isClear;
    }
    bool SmallDotClear_X(Rect rectBig, Rect rectSmall, byte* p, uint bmp_h, int ic)
    {
        bool isClear = false;
        //rectSmall �Ƿ��� rectBig ��
        bool in_left = (rectBig.l <= rectSmall.l && rectSmall.l <= rectBig.r);
        bool in_right = (rectBig.l <= rectSmall.r && rectSmall.r <= rectBig.r);
        if (in_left && in_right)
        {
            //��������һ��ֱ���� �����С�� 40% ʱ �޳����߶�
            if ((double)rectSmall.width / (double)rectBig.width < 0.5)
            {
                for (int g = 0; g < rectSmall.Al_dot.Count; g++)
                {
                    myDot dDot = (myDot)rectSmall.Al_dot[g];
                    if (dDot.y + ic < 0 || dDot.y + ic >= bmp_h)
                    {
                        //MessageBox.Show(dDot.y + " " + ic + " " + dDot.y  + " " + bmp_w);
                        //���߽��ɾ��
                        p[dDot.ib] = p[dDot.ib + 1] = p[dDot.ib + 2] = 255;
                        isClear = true;
                    }
                    else
                    {
                        myDot dDot2 = new myDot(dDot.x, (uint)(dDot.y + ic), dDot.iwidth, p);
                        if (dDot2.R != 0)
                        {
                            //����û�����ӵ㣬��ɾ��
                            p[dDot.ib] = p[dDot.ib + 1] = p[dDot.ib + 2] = 255;
                            isClear = true;
                        }
                    }

                }
            }
        }
        return isClear;
    }
    bool SmallLineY(byte* p, uint bmp_w, uint bmp_h, uint bmp_iWidth)
    {
        bool isClear = false;
        ArrayList AL_Prev = null;
        for (uint x = 0; x < bmp_w; x++)
        {
            ArrayList AL_Now = new ArrayList();
            //���е������߶�
            Rect rect = null;
            for (uint y = 0; y < bmp_h; y++)
            {
                myDot dot = new myDot(x, y, bmp_iWidth, p);
                if (dot.R == 0)
                {
                    if (rect == null)
                    {
                        //�������߶�
                        rect = new Rect(y, x, y, x);
                        AL_Now.Add(rect);
                    }
                    //���뵽�߶�
                    rect.Add(dot);
                    rect.b = y;
                    rect.height = Math.Abs((int)rect.t - (int)rect.b) + 1;
                }
                else
                {
                    //�����׵�Ͽ�
                    if (rect != null) rect = null;
                }
            }
            //û����һ�� ֱ��
            if (AL_Prev != null)
            {
                for (int k = 0; k < AL_Prev.Count; k++)
                {
                    rect = (Rect)AL_Prev[k];
                    for (int n = 0; n < AL_Now.Count; n++)
                    {
                        Rect r = (Rect)AL_Now[n];

                        if (SmallDotClear_Y(rect, r, p, bmp_w, 1)) isClear = true;//r �Ƿ��� rect ��
                        if (SmallDotClear_Y(r, rect, p, bmp_w, -1)) isClear = true;//rect �Ƿ��� r ��
                    }
                }
            }
            AL_Prev = AL_Now;
        }
        return isClear;
    }
    bool SmallDotClear_Y(Rect rectBig, Rect rectSmall, byte* p, uint bmp_w, int ic)
    {
        bool isClear = false;
        //rectSmall �Ƿ��� rectBig ��
        bool in_left = (rectBig.t <= rectSmall.t && rectSmall.t <= rectBig.b);
        bool in_right = (rectBig.t <= rectSmall.b && rectSmall.b <= rectBig.b);
        if (in_left && in_right)
        {
            //��������һ��ֱ���� �����С�� 40% ʱ ������߶�
            if ((double)rectSmall.height / (double)rectBig.height < 0.5)
            {
                for (int g = 0; g < rectSmall.Al_dot.Count; g++)
                {
                    myDot dDot = (myDot)rectSmall.Al_dot[g];
                    if (dDot.x + ic < 0 || dDot.x + ic >= bmp_w)
                    {
                        //���߽��ɾ��
                        p[dDot.ib] = p[dDot.ib + 1] = p[dDot.ib + 2] = 255;
                        isClear = true;
                    }
                    else
                    {
                        myDot dDot2 = new myDot((uint)(dDot.x + ic), dDot.y, dDot.iwidth, p);
                        if (dDot2.R != 0)
                        {
                            //����û�����ӵ㣬��ɾ��
                            p[dDot.ib] = p[dDot.ib + 1] = p[dDot.ib + 2] = 255;
                            isClear = true;
                        }
                    }
                }
            }
        }
        return isClear;
    }
    /// <summary>
    /// ���⻯ �̶�����������
    /// </summary>
    /// <param name="isKuo">Ϊ0ʱ�������� 1ʱ������С��˫��ʱ��Ч</param>
    public unsafe void toBigLine(byte* p, uint bmp_w, uint bmp_h, uint bmp_iWidth)
    {
        uint[,] arr = new uint[bmp_w, bmp_h];
        for (uint y = 0; y < bmp_h; y++)
        {
            for (uint x = 0; x < bmp_w; x++)
            {
                uint ib = y * bmp_iWidth + x * 3;
                if (p[ib] == 0)
                {
                    for (int thisy = (int)y; thisy <= (int)y + 1; thisy++)
                    {
                        for (int thisx = (int)x; thisx <= (int)x + 1; thisx++)
                        {
                            int ix = thisx;
                            int iy = thisy;
                            if (ix >= 0 && ix < bmp_w && iy >= 0 && iy < bmp_h)
                            {
                                arr[ix, iy] = 1;
                            }
                        }
                    }
                }
            }
        }
        for (uint y = 0; y < bmp_h; y++)
        {
            for (uint x = 0; x < bmp_w; x++)
            {
                if (arr[x, y] == 1)
                {
                    uint ib = (uint)y * bmp_iWidth + (uint)x * 3;
                    p[ib] = p[ib + 1] = p[ib + 2] = 0;
                }
            }
        }
    }
    /// <summary>
    /// ���⻯ ����ɢ��������
    /// </summary>
    /// <param name="isKuo">Ϊ0ʱ�������� 1ʱ������С��˫��ʱ��Ч</param>
    public unsafe void toBigLine(byte* p, uint bmp_w, uint bmp_h, uint bmp_iWidth, int N, int isKuo)
    {
        int N1 = 0;
        int N2 = 0;

        if (N % 2 == 0)
        {
            N1 = -N / 2;
            N2 = N / 2;
        }
        else
        {
            N1 = -(N + 1) / 2;
            N2 = (N + 1) / 2;
        }

        uint[,] arr = new uint[bmp_w, bmp_h];
        for (uint y = 0; y < bmp_h; y++)
        {
            for (uint x = 0; x < bmp_w; x++)
            {
                uint ib = y * bmp_iWidth + x * 3;
                if (p[ib] == 0)
                {
                    for (int thisy = (int)y + N1; thisy <= (int)y + N2; thisy++)
                    {
                        for (int thisx = (int)x + N1; thisx <= (int)x + N2; thisx++)
                        {
                            int ix = thisx;
                            int iy = thisy;
                            if (N % 2 == 1)
                            {
                                if (isKuo == 0)
                                {
                                    //����
                                    if (x <= bmp_w / 2)//����
                                    {
                                        if (thisx == (int)x + N2) ix = -1;
                                    }
                                    else if (x > bmp_w / 2)//�Ұ��
                                    {
                                        if (thisx == (int)x + N1) ix = -1;
                                    }
                                    if (y <= bmp_h / 2)//�ϰ��
                                    {
                                        if (thisy == (int)y + N2) iy = -1;
                                    }
                                    else if (y > bmp_h / 2)//�°��
                                    {
                                        if (thisy == (int)y + N1) iy = -1;
                                    }
                                }
                                else
                                {
                                    //��С
                                    if (x <= bmp_w / 2)//����
                                    {
                                        if (thisx == (int)x + N1) ix = -1;
                                    }
                                    else if (x > bmp_w / 2)//�Ұ��
                                    {
                                        if (thisx == (int)x + N2) ix = -1;
                                    }
                                    if (y <= bmp_h / 2)//�ϰ��
                                    {
                                        if (thisy == (int)y + N1) iy = -1;
                                    }
                                    else if (y > bmp_h / 2)//�°��
                                    {
                                        if (thisy == (int)y + N2) iy = -1;
                                    }
                                }
                            }
                            if (ix >= 0 && ix < bmp_w && iy >= 0 && iy < bmp_h)
                            {
                                arr[ix, iy] = 1;
                            }
                        }
                    }
                }
            }
        }
        for (uint y = 0; y < bmp_h; y++)
        {
            for (uint x = 0; x < bmp_w; x++)
            {
                if (arr[x, y] == 1)
                {
                    uint ib = (uint)y * bmp_iWidth + (uint)x * 3;
                    p[ib] = p[ib + 1] = p[ib + 2] = 0;
                }
            }
        }
    }

    uint[,] intCheck = null;
    int iOverIndex = 1104;
    /// <summary>
    /// ϸ��������
    /// </summary>
    public unsafe void toSmallLine(byte* p, uint bmp_w, uint bmp_h, uint bmp_iWidth, int imax)
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
                    intCheck = new uint[bmp_w, bmp_h];
                    for (uint y = 0; y < bmp_h; y++)
                    {
                        for (uint x = 0; x < bmp_w; x++)
                        {
                            uint ib = y * bmp_iWidth + x * 3;
                            if (p[ib] == 0)
                            {
                                intCheck[x, y] = 1;
                            }
                        }
                    }
                }
                it = toSmallLine_do2(p, bmp_w, bmp_h, bmp_iWidth);
            }
            else
            {
                it = toSmallLine_do(p, bmp_w, bmp_h, bmp_iWidth);
            }
        }

    }

    
    /// <summary>
    /// ϸ��������ϸ��ʵ��
    /// </summary>
    public unsafe int toSmallLine_do(byte* p, uint bmp_w, uint bmp_h, uint bmp_iWidth)
    {
        //DateTime dt1 = DateTime.Now;
        //TimeSpan ts;

        int ijs = 0;

        ArrayList alXY = new ArrayList();
        int icheck = 0;
        for (uint y = 0; y < bmp_h; y++)
        {
            for (uint x = 0; x < bmp_w; x++)
            {
                myPoint myP = new myPoint(x, y, bmp_iWidth);

                if (p[myP.ib] == 0)
                {
                    icheck++;
                    //��ȡ�õ���Χ�� �ڵ�ֲ�
                    int[,] arrN = toN_N(p, x, y, 3, bmp_w, bmp_h, bmp_iWidth);

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
            int[,] arrN = toN_N(p, myP.x, myP.y, 3, bmp_w, bmp_h, bmp_iWidth);

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
    public unsafe int toSmallLine_do2(byte* p, uint bmp_w, uint bmp_h, uint bmp_iWidth)
    {
        //DateTime dt1 = DateTime.Now;
        //TimeSpan ts;

        int ijs = 0;

        ArrayList alXY = new ArrayList();
        int icheck = 0;
        for (uint y = 0; y < bmp_h; y++)
        {
            for (uint x = 0; x < bmp_w; x++)
            {
                myPoint myP = new myPoint(x, y, bmp_iWidth);

                if (intCheck[x, y] == 1)
                {
                    icheck++;
                    int[,] arrN = toN_N(p, x, y, 3, bmp_w, bmp_h, bmp_iWidth);

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
            int[,] arrN = toN_N(p, myP.x, myP.y, 3, bmp_w, bmp_h, bmp_iWidth);

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
                        if (thisx >= 0 && thisx < bmp_w && thisy >= 0 && thisy < bmp_h)
                        {
                            if (intCheck[thisx, thisy] == 0)
                            {
                                uint ib = thisy * bmp_iWidth + thisx * 3;
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
    /// ����ĳһ��Ϊ���ĵ� N*N �ķ���  ��������߽� Ϊ 0, ��ɫ Ϊ 1
    /// </summary>
    /// <param name="p"></param>
    /// <param name="x">���ĵ�</param>
    /// <param name="y">���ĵ�</param>
    /// <param name="n">�����n  ��Ҫ����</param>
    /// <returns>n*n��С������ �������£���������</returns>
    public unsafe int[,] toN_N(byte* p, uint x, uint y, uint n, uint bmp_w, uint bmp_h, uint bmp_iWidth)
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
                if (thisx >= 0 && thisx < bmp_w && thisy >= 0 && thisy < bmp_h)
                {
                    uint ib = thisy * bmp_iWidth + thisx * 3;
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
    /// ����8����ֻ��һ�鰼͹���� �������ӵ� ��ɾ��
    /// </summary>
    /// <param name="arrN"></param>
    /// <param name="it"></param>
    /// <param name="ik"></param>
    /// <returns></returns>
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
    /// �ҶȻ�
    /// </summary>
    public unsafe int toGrey(byte* p)
    {
        double it = 0;
        uint greyArea = 0;
        double avgHui = 0;
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
                    //avgHui += (double)iee;
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
        //avgHui = avgHui / it;

        return 0;
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
    /// <summary>
    /// ֱ��ͼ
    /// </summary>
    /// <returns></returns>
    public unsafe OcrStruct.Histogram toZhiFang(byte* p)
    {
        int[] pixelNum = new int[256];
        int max = 0;

        for (uint y = 0; y < h; y++)
        {
            for (uint x = 0; x < w; x++)
            {
                //��ʼ��
                uint ib = y * iWidth + x * 3;
                r = p[ib];
                g = p[ib + 1];
                b = p[ib + 2];
                pixelNum[(r + g + b) / 3]++;
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
    public unsafe void toClear(byte* p)
    {
        for (uint y = 0; y < h; y++)
        {
            for (uint x = 0; x < w; x++)
            {
                uint ib = y * iWidth + x * 3;
                if (p[ib] == 0)
                {
                    int[,] arrN = toN_N(p, x, y, 5, w, h, iWidth);
                    bool ok = false;
                    if (testN5_N5(arrN, new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }) ||
                        testN5_N3(arrN, new int[] { 0, 0, 0, 0, 1, 0, 1, 1, 1 }) ||
                        testN5_N3(arrN, new int[] { 1, 0, 0, 1, 1, 0, 1, 0, 0 }) ||
                        testN5_N3(arrN, new int[] { 1, 1, 1, 0, 1, 0, 0, 0, 0 }) ||
                        testN5_N3(arrN, new int[] { 0, 0, 1, 0, 1, 1, 0, 0, 1 }))
                    {
                        ok = true;
                    }

                    if (y < 2 || y > h - 2 || x < 2 || x > w - 2)
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

    public OcrResult getRectImg2(byte* p, RectangleL rect)
    {
        int ithisW = 40;
        int ithisH = 40;

        Bitmap bmp = new Bitmap(ithisW, ithisH);
        BitmapData bitData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        uint bmp_ic = (uint)(bitData.Stride - bitData.Width * 3);
        byte* bmp_p = (byte*)(bitData.Scan0.ToPointer());

        uint bmp_w = (uint)bitData.Width;
        uint bmp_h = (uint)bitData.Height;

        //һ�е����ص�
        uint bmp_iWidth = bmp_w * 3 + bmp_ic;

        float iPerW = (float)ithisW / (float)rect.Width;//�����ĵ���
        float iPerH = (float)ithisH / (float)rect.Height;//�����ĵ���

        //MessageBox.Show(rect.Height + " " + rect.Width);
        float ihelf_w_dot = 0;
        float ihelf_h_dot = 0;
        float ihelf_w = 0;
        float ihelf_h = 0;
        bool isSingleX = false;
        bool isSingleY = false;
        if (rect.Width % 2 == 0)
        {
            //˫����
            ihelf_w = (float)rect.Width / 2;
            ihelf_w_dot = ihelf_w + 0.5f;
        }
        else
        {
            //������
            isSingleX = true;
            ihelf_w = (float)(rect.Width - 1) / 2;
            ihelf_w_dot = ihelf_w + 1;
        }
        if (rect.Height % 2 == 0)
        {
            //˫����
            ihelf_h = (float)rect.Height / 2;
            ihelf_h_dot = ihelf_h + 0.5f;
        }
        else
        {
            //������
            isSingleY = true;
            ihelf_h = (float)(rect.Height - 1) / 2;
            ihelf_h_dot = ihelf_h + 1;
        }

        float iMoveW = (float)ithisW - (float)rect.Width;//�����ĵ���
        float iMoveW_l = 0;
        float iMoveW_r = 0;
        if (iMoveW % 2 == 0)
        {
            //˫���ƶ�ƽ��������
            iMoveW_l = iMoveW_r = iMoveW / 2;
        }
        else
        {
            //�����ƶ��������1
            iMoveW_l = (float)Math.Floor(iMoveW / 2);
            iMoveW_r = iMoveW_l + 1;
        }

        float iMoveH = (float)ithisH - (float)rect.Height;//�����ĵ���
        float iMoveH_t = 0;
        float iMoveH_b = 0;
        if (iMoveH % 2 == 0)
        {
            //˫���ƶ�ƽ��������
            iMoveH_t = iMoveH_b = iMoveH / 2;
        }
        else
        {
            //�����ƶ��ϱ�����1
            iMoveH_t = (float)Math.Floor(iMoveH / 2);
            iMoveH_b = iMoveH_t + 1;
        }

        int[,] arrUint = new int[bmp_w, bmp_h];
        for (uint y = 0; y < bmp_h; y++)
        {
            for (uint x = 0; x < bmp_w; x++)
            {
                uint ib = y * bmp_iWidth + x * 3;
                bmp_p[ib] = bmp_p[ib + 1] = bmp_p[ib + 2] = 255;
            }
        }


        //MessageBox.Show(rect.X + " " + rect.Y + " " + rect.Width + " " + rect.Height);
        //�� X ��������
        for (uint y = rect.Y; y <= rect.Y + rect.Height; y++)
        {
            for (uint x = rect.X; x <= rect.X + rect.Width; x++)
            {
                uint ib = y * iWidth + x * 3;

                if (p[ib] == 0)
                {
                    //�����㷨
                    uint toX = 0;
                    uint countX = 0;
                    int fh_x = 1;

                    uint thisx = x + 1 - rect.X;//����X

                    if (thisx >= ihelf_w_dot)
                    {
                        float ihelf = ihelf_w;
                        if (isSingleX) ihelf++;//�����Ҳ� +1
                        //���ƶ�
                        float per = ((float)thisx - ihelf) / ihelf_w;
                        //AciDebug.Debug((x + 1) + " " + rect.X + " " + thisx + " " + per + " " + ihelf + " " + iMoveW + " " + (ihelf_w - (float)thisx));
                        float izhang = (float)Math.Floor((iMoveW_r - 1) * per);
                        double ithis = (uint)Math.Floor((float)thisx + iMoveW_l + izhang);
                        if (ithis <= bmp_w)
                        {
                            toX = (uint)ithis;
                        }
                        else
                        {
                            toX = bmp_w - 1;
                        }
                        //AciDebug.Debug(izhang + " " + thisx + " " + toX);
                        countX = (uint)Math.Ceiling(iPerW);
                        countX = 0;
                        fh_x = -1;
                    }
                    else if (thisx < ihelf_w_dot)
                    {
                        float ihelf = ihelf_w + 1;
                        //���ƶ�
                        float per = (ihelf - (float)thisx) / ihelf_w;
                        //AciDebug.Debug(x + " " + rect.X + " " + thisx + " " + per + " " + ihelf + " " + (ihelf_w - (float)thisx));
                        float izhang = (float)Math.Ceiling((iMoveW_r - 1) * per);
                        double ithis = Math.Ceiling((float)thisx + iMoveW_l - izhang);
                        if (ithis > 0)
                        {
                            toX = (uint)ithis;
                        }
                        else
                        {
                            toX = 0;
                        }
                        countX = (uint)Math.Ceiling(iPerW);
                        fh_x = 1;
                    }

                    uint thisy = y - rect.Y;//����Y
                    for (uint n = 0; n <= countX; n++)
                    {
                        uint ix = (uint)(toX + (int)n * fh_x) - 1;
                        uint iy = thisy;
                        if (ix >= bmp_w)
                        {
                            ix = bmp_w - 1;
                        }
                        arrUint[ix, iy] = 1;

                        //uint bmp_ib = (uint)(iy * bmp_iWidth + ix * 3);
                        //bmp_p[bmp_ib] = bmp_p[bmp_ib + 1] = bmp_p[bmp_ib + 2] = 0;
                    }
                }
            }
        }

        //�� Y ��������
        for (uint y = 0; y < bmp_h; y++)
        {
            for (uint x = 0; x < bmp_w; x++)
            {
                if (arrUint[x, y] == 1)
                {
                    uint toY = 0;
                    uint countY = 0;
                    int fh_y = 1;

                    uint thisy = y + 1;//����Y

                    if (thisy >= ihelf_h_dot)
                    {
                        float ihelf = ihelf_h;
                        if (isSingleY) ihelf++;//�����Ҳ� +1
                        //���ƶ�
                        float per = ((float)thisy - ihelf) / ihelf_h;
                        float izhang = (float)Math.Floor((iMoveH_t - 1) * per);
                        double ithis = (uint)Math.Floor((float)thisy + iMoveH_b + izhang);
                        if (ithis <= bmp_h)
                        {
                            toY = (uint)ithis;
                        }
                        else
                        {
                            toY = bmp_h - 1;
                        }
                        //AciDebug.Debug(y + " " + rect.Y + " " + thisy + " " + per + " " + ihelf + " " + (ihelf_h - (float)thisy) + " " + toY);
                        countY = (uint)Math.Ceiling(iPerH);
                        fh_y = -1;
                    }
                    else if (thisy < ihelf_h_dot)
                    {
                        float ihelf = ihelf_h + 1;
                        //���ƶ�
                        float per = (ihelf - (float)thisy) / ihelf_h;
                        float izhang = (float)Math.Ceiling((iMoveH_b - 1) * per);
                        double ithis = Math.Ceiling((float)thisy + iMoveH_b - izhang);
                        if (ithis > 0)
                        {
                            toY = (uint)ithis;
                        }
                        else
                        {
                            toY = 0;
                        }
                        //AciDebug.Debug(y + " " + rect.Y + " " + thisy + " " + per + " " + ihelf + " " + (ihelf_h - (float)thisy) + " " + toY);
                        countY = (uint)Math.Ceiling(iPerH);
                        fh_y = 1;
                    }

                    uint thisx = x;//����X
                    for (uint n = 0; n <= countY; n++)
                    {
                        uint ix = thisx;
                        uint iy = (uint)(toY + (int)n * fh_y) - 1;
                        if (iy >= bmp_h)
                        {
                            iy = bmp_h - 1;
                        }
                        uint bmp_ib = (uint)(iy * bmp_iWidth + ix * 3);
                        bmp_p[bmp_ib] = bmp_p[bmp_ib + 1] = bmp_p[bmp_ib + 2] = 0;
                    }

                }
            }
        }


        //�����м�
        //for (uint y = rect.Y; y <= rect.Y + rect.Height; y++)
        //{
        //    for (uint x = rect.X; x <= rect.X + rect.Width; x++)
        //    {
        //        uint ib = y * iWidth + x * 3;

        //        if (p[ib] == 0)
        //        {
        //            //MessageBox.Show((y + iMoveH) + " " + (x + iMoveW));
        //            uint bmp_ib = (uint)((y + (uint)iMoveH_t - rect.Y) * bmp_iWidth + (x + (uint)iMoveW_l - rect.X) * 3);
        //            bmp_p[bmp_ib] = 255;
        //            bmp_p[bmp_ib + 1] = 0;
        //            bmp_p[bmp_ib + 2] = 0;
        //        }
        //    }
        //}

        OcrResult result = new OcrResult(myDC, imgSave, 25);

        //���񻯷���
        result.get_iCut(bmp_p, bmp_w, bmp_h, bmp_iWidth, ithisW, ithisH);

        bmp.UnlockBits(bitData);

        result.bmp = bmp;
        iImgCount++;

        return result;
    }



    /// <summary>
    /// ���� ����ʶ��
    /// </summary>
    /// <param name="bitmap"></param>
    /// <returns></returns>
    public ArrayList OcrCutArea(Bitmap bitmap)
    {


        BitmapData bitData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        //ȡ��ÿ�ж��������
        uint ic = (uint)(bitData.Stride - bitData.Width * 3);

        //ȡ��ȫ������ֵ ÿ�� 3��  Ϊ  R G B ÿ��ѭ��ɾ�� һ�������� xx ��
        byte* p = (byte*)(bitData.Scan0.ToPointer());

        w = (uint)bitData.Width;
        h = (uint)bitData.Height;

        //һ�е����ص�
        iWidth = w * 3 + ic;


        //�ҶȻ�
        toGrey(p);
        //if (show) bitmap.Save(Application.StartupPath + "\\test\\��ʱ_a�ҶȻ�.jpg", myImageCodecInfo, encoderParams);

        ////��ʼ��ֵ��
        toTwo(p, 200);
        //if (show) bitmap.Save(Application.StartupPath + "\\test\\��ʱ_b��ֵ��.jpg", myImageCodecInfo, encoderParams);

        ////ȥ������
        toClear(p);
        //if (show) bitmap.Save(Application.StartupPath + "\\test\\��ʱ_c���Ե.jpg", myImageCodecInfo, encoderParams);

        ////��ͨ����
        ArrayList AL_HB = cutArea_AL(p);
        bitmap.UnlockBits(bitData);

        //if (show) bitmap.Save(Application.StartupPath + "\\test\\��ʱ_d��ͨ����.jpg", myImageCodecInfo, encoderParams);


        return AL_HB;
    }

    /// <summary>
    /// ��ͨ����ָ��㷨
    /// </summary>
    public unsafe ArrayList cutArea_AL(byte* p)
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
                    rect.Add(new myDot(x, y, iWidth, 0));

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
                                        rect.r = thisx;
                                    }
                                    if (thisy < rect.t)
                                    {
                                        rect.t = thisy;
                                    }
                                    else if (thisy > rect.b)
                                    {
                                        rect.b = thisy;
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

        //����ƽ���߶�
        float iPerH = 0;
        float iPerH_num = 0;

        //������ϲ�
        bool isHB = true;//�Ƿ��н���ϲ�������Ҫѭ���ϲ�

        ArrayList AL_HB = new ArrayList();
        int[] arrRemove = new int[Al_rect.Count];
        int itIndex = 0;
        while (isHB)
        {
            isHB = false;

            ArrayList al_hb = new ArrayList();
            for (int i = 0; i < Al_rect.Count; i++)
            {
                rect = (Rect)Al_rect[i];
                rect.calculate();

                if (itIndex >= 1 && arrRemove[i] == 1)
                {
                    //�ڶ���ʱ�����ܹ���ֱ��ȥ��
                    //MessageBox.Show("a");
                    continue;
                }

                bool isIn = false;
                //����һ��ѭ������
                if (itIndex == 0)
                {
                    //��ȫ������
                    for (int j = 0; j < Al_rect.Count; j++)
                    {
                        if (i != j && arrRemove[j] != 1)
                        {
                            Rect r = (Rect)Al_rect[j];
                            if (r.l <= rect.l && r.r >= rect.r && r.t <= rect.t && r.b >= rect.b)
                            {
                                arrRemove[i] = 1;
                                //rect.Draw(p, iWidth, 0, 255, 0);
                                isIn = true;
                                break;
                            }
                        }
                    }
                }

                if (!isIn)
                {
                    ////�ཻ�ͺϲ�
                    for (int j = 0; j < Al_rect.Count; j++)
                    {
                        if (i != j && arrRemove[j] != 1)
                        {
                            Rect r = (Rect)Al_rect[j];
                            ////�ཻ�ͺϲ�
                            if ((r.l <= rect.l && r.r >= rect.l && r.t <= rect.t && r.b >= rect.t)
                                || (r.l <= rect.r && r.r >= rect.r && r.t <= rect.t && r.b >= rect.t)
                                || (r.l <= rect.l && r.r >= rect.l && r.t <= rect.b && r.b >= rect.b)
                                || (r.l <= rect.r && r.r >= rect.r && r.t <= rect.b && r.b >= rect.b)
                                )
                            {
                                r.AddRect(rect);
                                //r.Draw(p, iWidth);
                                //rect.Draw(p, iWidth, 255, 0, 0);
                                arrRemove[i] = 1;
                                isIn = true;
                                isHB = true;
                                break;
                            }
                        }
                    }
                }

                if (!isIn)
                {
                    //δ���ϲ������� ȥ����С���� ���С�ĵ�
                    float iw = (float)rect.width;
                    float ih = (float)rect.height;
                    float pp = 0;
                    if (iw > ih)
                    {
                        pp = ih / iw;
                    }
                    else
                    {
                        pp = iw / ih;
                    }
                    if (rect.iNum > 4)
                    {
                        //���������
                        if (pp >= 0.5)
                        {
                            iPerH += ih;
                            iPerH_num++;
                        }
                        al_hb.Add(rect);
                    }
                    else
                    {
                        rect.Clear(p);
                    }
                }
            }
            if (!isHB)
            {
                AL_HB = al_hb;
            }
            itIndex++;
        }

        
        arrRemove = new int[AL_HB.Count];
        double iH_105 = (float)w * 1.05;

        for (int i = 0; i < AL_HB.Count; i++)
        {
            if (arrRemove[i] != 1)
            {
                rect = (Rect)AL_HB[i];
                ////////////////////////////////////////////////////
                //����ֻ�������أ��ϲ����ϵĲ���
                ////////////////////////////////////////////////////
                for (int j = 0; j < AL_HB.Count; j++)
                {
                    if (i != j && arrRemove[i] != 1)
                    {
                        Rect r = (Rect)AL_HB[j];
                        if (j < i)
                        {
                            if (rect.r - r.l > iH_105) continue;
                        }
                        else
                        {
                            if (r.r - rect.l > iH_105) continue;
                        }

                        if ((rect.l <= r.l && r.l <= rect.r) || (rect.l <= r.r && r.r <= rect.r))
                        {
                            arrRemove[j] = 1;
                            rect.AddRect(r);
                        }
                    }
                }
            }
        }

        ArrayList al_hb2 = new ArrayList();
        for (int i = 0; i < AL_HB.Count; i++)
        {
            if (arrRemove[i] != 1)
            {
                al_hb2.Add((Rect)AL_HB[i]);
            }
        }

        return al_hb2;
    }
}


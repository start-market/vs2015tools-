using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

public unsafe class Rect
{
    /// <summary>上</summary>
    public uint t;
    /// <summary>下</summary>
    public uint b;
    /// <summary>左</summary>
    public uint l;
    /// <summary>右</summary>
    public uint r;
    /// <summary>总点数</summary>
    public int iNum = 0;
    /// <summary>贴边点总点数</summary>
    public int iNumTie = 0;
    /// <summary>上下左右四点都是黑点的实心点数</summary>
    public int iNumShi = 0;
    /// <summary>总集合</summary>
    public List<myDot> Al_dot;
    /// <summary>宽</summary>
    public int width;
    /// <summary>高</summary>
    public int height;
    /// <summary>宽在line中</summary>
    public int width_line;
    /// <summary>高在line中</summary>
    public int height_line;
    /// <summary>面积</summary>
    public int S;
    /// <summary>长宽比</summary>
    public float pp;
    /// <summary>中心点</summary>
    public Point pCenter;
    /// <summary>密度</summary>
    public double P;
    /// <summary>离散程度 getD() 获得</summary>
    public float D;
    /// <summary>是否足够大的联通区域</summary>
    public bool isUse;

    /// <summary>上</summary>
    public uint t2;
    /// <summary>下</summary>
    public uint b2;
    /// <summary>左</summary>
    public uint l2;
    /// <summary>右</summary>
    public uint r2;
    /// <summary>点</summary>
    public byte* pP;
    /// <summary>点</summary>
    public uint i_Width;


    /// <summary>初步识别结果</summary>
    public OcrResult result;

    public myDot dotLeftTop;
    public myDot dotRightTop;
    public myDot dotLeftBottom;
    public myDot dotRightBottom;
    public int dotAll;

    public LineRow line = null;

    /// <summary>需要分割的数量</summary>
    public int icut = 0;

    public bool isHB = false;

    /// <summary>拥有的顶点数量</summary>
    public int iDingNum = 0;

    /// <summary>
    /// 设置 l t b r 在一定iper比例后的值  保存成  l2 t2  b2  r2
    /// </summary>
    /// <param name="iper"></param>
    public void setLTRB2(float iper)
    {
        l2 = (uint)((float)l / iper);
        t2 = (uint)((float)t / iper);
        b2 = (uint)((float)b / iper);
        r2 = (uint)((float)r / iper);
    }

    /// <summary>
    /// 区域A 与 区域B 是否相交
    /// </summary>
    /// <param name="RectA"></param>
    /// <param name="RectB"></param>
    /// <returns></returns>
    public static bool RectA_vs_RectB(Rect RectA, Rect RectB)
    {
        if (RectA.r < RectB.l //A 在 B 左侧
            || RectA.l > RectB.r //A 在 B 右侧
            || RectA.b < RectB.t //A 在 B 上方
            || RectA.t > RectB.b) //A 在 B 下方
        {
            //不相交
            return false;
        }
        else
        {
            //相交
            return true;
        }
    }
    /// <summary>
    /// 区域A 与 行B 是否相交
    /// </summary>
    /// <param name="RectA"></param>
    /// <param name="RectB"></param>
    /// <returns></returns>
    public static bool RectA_vs_LineB(Rect RectA, LineRow lineB)
    {
        if (RectA.r < lineB.l //A 在 B 左侧
            || RectA.l > lineB.r //A 在 B 右侧
            || RectA.b < lineB.t //A 在 B 上方
            || RectA.t > lineB.b) //A 在 B 下方
        {
            //不相交
            return false;
        }
        else
        {
            //相交
            return true;
        }
    }
    /// <summary>
    /// 区域A 与 区域B 是否有包含关系 没有返回 null 有返回 大的包含体
    /// </summary>
    /// <param name="RectA"></param>
    /// <param name="RectB"></param>
    /// <returns></returns>
    public static Rect RectA_in_RectB(Rect RectA, Rect RectB)
    {
        if (RectA.l <= RectB.l && RectA.r >= RectB.r && RectA.b <= RectB.b && RectA.t >= RectB.t)
        {
            return RectA;
        }
        else if (RectB.l <= RectA.l && RectB.r >= RectA.r && RectB.b <= RectA.b && RectB.t >= RectA.t)
        {
            //相交
            return RectB;
        }
        return null;
    }

    /// <summary>
    /// 区域A 与 行B 是否有包含关系 没有返回 null 有返回 大的包含体
    /// </summary>
    /// <param name="RectA"></param>
    /// <param name="RectB"></param>
    /// <returns></returns>
    public static object RectA_in_LineB(Rect RectA, LineRow LineB)
    {
        if (RectA.l <= LineB.l && RectA.r >= LineB.r && RectA.b <= LineB.b && RectA.t >= LineB.t)
        {
            return (object)RectA;
        }
        else if (LineB.l <= RectA.l && LineB.r >= RectA.r && LineB.b <= RectA.b && LineB.t >= RectA.t)
        {
            //相交
            return (object)LineB;
        }
        return null;
    }

    /// <summary>0 标准中文字  1 数字或字母 2 英文 3 标点符号 4 部首 5 上标点  6 下标点</summary>
    public enum OcrMode : int
    {
        Chinese,//0
        Int,//1
        English,//2
        Code,//3
        Bushou,//4
        CodeUp,//5  
        CodeDown,//6  
        Int_English,
        Code_English,
        Code_Int_English,
        Code_Int_English_Bushou,
        Chinese_ts,
        Int_ts
    }
    public Rect.OcrMode iOcrMode = OcrMode.Chinese;

    public int iNumRect = 0;

    /// <summary>
    /// 上右下左
    /// </summary>
    /// <param name="it"></param>
    /// <param name="ir"></param>
    /// <param name="ib"></param>
    /// <param name="il"></param>
    public Rect(uint it, uint ir, uint ib, uint il)
    {
        dotLeftTop = null;
        dotRightTop = null;
        dotLeftBottom = null;
        dotRightBottom = null;
        dotAll = 0;

        t = it;
        r = ir;
        b = ib;
        l = il;
        width = Math.Abs((int)l - (int)r) + 1;
        height = Math.Abs((int)t - (int)b) + 1;

        Al_dot = new List<myDot>();
    }

    public Rect()
    {
        t = r = b = l = 0;
        Al_dot = new List<myDot>();
    }
    public void Add(myDot dot)
    {
        Al_dot.Add(dot);
        iNum = Al_dot.Count;
    }
    public void Remove(myDot dot)
    {
        Al_dot.Remove(dot);
        iNum = Al_dot.Count;
    }
    /// <summary>
    /// 合并入另一个区域
    /// </summary>
    /// <param name="rect"></param>
    public void AddRect(Rect rect)
    {
        for (int i = 0; i < rect.Al_dot.Count; i++)
        {
            Al_dot.Add(rect.Al_dot[i]);
        }

        iNum = Al_dot.Count;

        if (l > rect.l)
        {
            l = rect.l;
        }
        if (t > rect.t)
        {
            t = rect.t;
        }
        if (r < rect.r)
        {
            r = rect.r;
        }
        if (b < rect.b)
        {
            b = rect.b;
        }
        width = Math.Abs((int)l - (int)r) + 1;
        height = Math.Abs((int)t - (int)b) + 1;
    }
    /// <summary>
    /// 用红框 画出区域
    /// </summary>
    /// <param name="p"></param>
    /// <param name="iWidth"></param>
    public ArrayList Draw(byte* p, uint iWidth)
    {
        return Draw(p, iWidth, 255, 0, 0);
    }
    public string DebugString()
    {
        return "左：【" + l + "】 右：【" + r + "】上：【" + t + "】 下：【" + b + "】Width：【" + width + "】 Height：【" + height + "】";
    }
    public ArrayList AL_drawOld;
    public ArrayList AL_drawPenOld;
    /// <summary>
    /// 用红框 画出区域
    /// </summary>
    /// <param name="p"></param>
    /// <param name="iWidth"></param>
    public ArrayList Draw(byte* p, uint iWidth, byte iR, byte iG, byte iB)
    {
        if (AL_drawOld == null) AL_drawOld = new ArrayList();

        uint thist = 0;
        if ((int)t - 1 >= 0)
        {
            thist = t - 1;
        }

        uint thisl = 0;
        if ((int)l - 1 >= 0)
        {
            thisl = l - 1;
        }

        for (uint y = thist; y <= b - 1; y++)
        {
            for (uint x = thisl; x <= r - 1; x++)
            {
                if (y == thist || y == b - 1 || x == thisl || x == r - 1)
                {
                    uint ib = y * iWidth + x * 3;
                    myDot dot = new myDot(x, y, iWidth, p);
                    dot.iB = p[ib];
                    dot.iG = p[ib + 1];
                    dot.iR = p[ib + 2];
                    AL_drawOld.Add(dot);

                    p[ib] = iB;
                    p[ib + 1] = iG;
                    p[ib + 2] = iR;
                }
            }
        }
        return AL_drawOld;
    }
    /// <summary>
    /// 用红框 画出区域
    /// </summary>
    /// <param name="p"></param>
    /// <param name="iWidth"></param>
    public ArrayList Draw(ref int[,] intBmp)
    {
        if (AL_drawOld == null) AL_drawOld = new ArrayList();

        uint thist = 0;
        if ((int)t - 1 >= 0)
        {
            thist = t - 1;
        }

        uint thisl = 0;
        if ((int)l - 1 >= 0)
        {
            thisl = l - 1;
        }

        for (uint y = thist; y <= b - 1; y++)
        {
            for (uint x = thisl; x <= r - 1; x++)
            {
                if (y == thist || y == b - 1 || x == thisl || x == r - 1)
                {
                    int ito = intBmp[x, y];
                    if (ito >= 0)
                    {
                        myDot dot = new myDot(x, y);
                        dot.iR = (byte)ito;
                        AL_drawOld.Add(dot);
                        intBmp[x, y] = -1;
                    }
                }
            }
        }
        return AL_drawOld;
    }
    /// <summary>
    /// 用框 画出区域 可以设置宽度
    /// </summary>
    /// <param name="p"></param>
    /// <param name="iWidth"></param>
    public ArrayList DrawBorder(byte* p, uint iWidth, uint borderW, byte iR, byte iG, byte iB)
    {
        if (AL_drawOld == null) AL_drawOld = new ArrayList();

        uint thist = 0;
        if ((int)t - borderW >= 0)
        {
            thist = t - borderW;
        }

        uint thisl = 0;
        if ((int)l - borderW >= 0)
        {
            thisl = l - borderW;
        }
        for (uint y = thist; y <= b + borderW; y++)
        {
            for (uint x = thisl; x <= r + borderW; x++)
            {
                if ((y >= thist && y < thist + borderW) || (y >= b && y < b + borderW)
                    || (x >= thisl && x < thisl + borderW) || (x >= r && x < r + borderW))
                {
                    uint ib = y * iWidth + x * 3;

                    myDot dot = new myDot(x, y, iWidth, p);
                    dot.iB = p[ib];
                    dot.iG = p[ib + 1];
                    dot.iR = p[ib + 2];

                    if (dot.iB != iB || dot.iG != iG || dot.iR != iR)
                    {
                        AL_drawOld.Add(dot);
                    }

                    p[ib] = iB;
                    p[ib + 1] = iG;
                    p[ib + 2] = iR;
                }
            }
        }
        return AL_drawOld;
    }
    /// <summary>
    /// 笔刷 修图
    /// </summary>
    /// <param name="p"></param>
    /// <param name="iWidth"></param>
    /// <param name="iR"></param>
    /// <param name="iG"></param>
    /// <param name="iB"></param>
    public ArrayList DrawPan(byte* p, uint iWidth, byte iR, byte iG, byte iB, uint w, uint h)
    {
        if (AL_drawPenOld == null) AL_drawPenOld = new ArrayList();

        for (uint y = t; y <= b; y++)
        {
            for (uint x = l; x <= r; x++)
            {
                if (x >= 0 && x < w - 1 && y >= 0 && y < h - 1)
                {
                    uint ib = y * iWidth + x * 3;
                    myDot dot = new myDot(x, y, iWidth, p);
                    dot.iB = p[ib];
                    dot.iG = p[ib + 1];
                    dot.iR = p[ib + 2];
                    AL_drawPenOld.Add(dot);

                    p[ib] = iB;
                    p[ib + 1] = iG;
                    p[ib + 2] = iR;
                }
            }
        }
        return AL_drawPenOld;
    }
    /// <summary>
    /// 笔刷 修图 仿制印章
    /// </summary>
    /// <param name="p"></param>
    /// <param name="iWidth"></param>
    /// <param name="iR"></param>
    /// <param name="iG"></param>
    /// <param name="iB"></param>
    /// <param name="smode">上 下 左 右  仿制印章 仿制的方向</param>
    /// <param name="w">图像整体宽 防止超出边界</param>
    /// <param name="h">图像整体高 防止超出边界</param>
    /// <returns></returns>
    public ArrayList DrawPenFZ(byte* p, uint iWidth, byte iR, byte iG, byte iB, string smode, uint w, uint h)
    {
        if (AL_drawPenOld == null) AL_drawPenOld = new ArrayList();

        uint xx = 0;
        uint yy = 0;
        if (smode == "上")
        {
            yy = (uint)-height;
        }
        else if (smode == "下")
        {
            yy = (uint)height;
        }
        else if (smode == "左")
        {
            xx = (uint)-width;
        }
        else if (smode == "右")
        {
            xx = (uint)width;
        }
        for (uint y = t; y <= b; y++)
        {
            for (uint x = l; x <= r; x++)
            {
                if (x >= 0 && x < w - 1 && y >= 0 && y < h - 1)
                {
                    uint ib = y * iWidth + x * 3;
                    myDot dot = new myDot(x, y, iWidth, p);
                    dot.iB = p[ib];
                    dot.iG = p[ib + 1];
                    dot.iR = p[ib + 2];
                    AL_drawPenOld.Add(dot);

                    uint thisy = y + yy;
                    uint thisx = x + xx;
                    if (thisx < 0) thisx = 0;
                    if (thisx > w - 1) thisx = w - 1;
                    if (thisy < 0) thisy = 0;
                    if (thisy > h - 1) thisy = h - 1;

                    uint ib2 = thisy * iWidth + thisx * 3;
                    p[ib] = p[ib2];
                    p[ib + 1] = p[ib2 + 1];
                    p[ib + 2] = p[ib2 + 2];
                }
            }
        }
        return AL_drawPenOld;
    }
    /// <summary>
    /// 画出区域 后的恢复
    /// </summary>
    /// <param name="p"></param>
    /// <param name="iWidth"></param>
    public void DrawReturn(byte* p)
    {
        for (int i = 0; i < AL_drawOld.Count; i++)
        {
            myDot dot = (myDot)AL_drawOld[i];
            dot.Draw(p);
        }
        AL_drawOld = new ArrayList();
    }
    /// <summary>
    /// 画出区域 后的恢复
    /// </summary>
    /// <param name="p"></param>
    /// <param name="iWidth"></param>
    public void DrawReturn(ref int[,] intBmp)
    {
        for (int i = 0; i < AL_drawOld.Count; i++)
        {
            myDot dot = (myDot)AL_drawOld[i];
            intBmp[dot.x, dot.y] = dot.iR;
        }
        AL_drawOld = new ArrayList();
    }
    /// <summary>
    /// 画笔修图 后的恢复
    /// </summary>
    /// <param name="p"></param>
    /// <param name="iWidth"></param>
    public void DrawPenReturn(byte* p)
    {
        for (int i = 0; i < AL_drawPenOld.Count; i++)
        {
            myDot dot = (myDot)AL_drawPenOld[i];
            dot.Draw(p);
        }
        AL_drawPenOld = new ArrayList();
    }


    /// <summary>
    /// 清除区域中的所有点
    /// </summary>
    /// <param name="p"></param>
    /// <param name="iWidth"></param>
    public void Clear(byte* p)
    {
        for (int n = 0; n < Al_dot.Count; n++)
        {
            myDot dott = Al_dot[n];
            p[dott.ib] = p[dott.ib + 1] = p[dott.ib + 2] = 255;
        }
    }
    /// <summary>
    /// 清除区域中的所有点
    /// </summary>
    /// <param name="p"></param>
    /// <param name="iWidth"></param>
    public void Clear(ref int[,] intBmp)
    {
        for (int n = 0; n < Al_dot.Count; n++)
        {
            myDot dott = Al_dot[n];
            intBmp[dott.x, dott.y] = 0;
        }
    }
    public void calculateWH()
    {
        width = Math.Abs((int)l - (int)r) + 1;
        height = Math.Abs((int)t - (int)b) + 1;
    }
    /// <summary>开始计算,矩形宽，高，面积，中心点，密度</summary>
    public void calculate()
    {
        width = Math.Abs((int)l - (int)r) + 1;
        height = Math.Abs((int)t - (int)b) + 1;
        S = width * height;
        pCenter = new Point(((int)l + (int)r) / 2, ((int)t + (int)b) / 2);
        if (width > height)
        {
            pp = (float)height / (float)width;
        }
        else
        {
            pp = (float)width / (float)height;
        }
        if (S != 0)
        {
            P = (double)iNum / (double)S;
        }
        else
        {
            P = 0;
        }
    }
    /// <summary>开始计算,矩形宽，高，面积，中心点，密度</summary>
    public void calculate2()
    {
        calculate();
        if (iNum != 0)
        {
            iTie_per = (double)iNumTie / (double)iNumShi * 1000;
        }
    }
    /// <summary>计算,方差  返回方差值</summary>
    public double getD()
    {
        int ix = 0;
        int iy = 0;
        for (int n = 0; n < Al_dot.Count; n++)
        {
            myDot dots = Al_dot[n];
            int xx = (int)(dots.x - l);
            int yy = (int)(dots.y - l);
            ix += xx;
            iy += yy;
        }
        ix /= Al_dot.Count;
        iy /= Al_dot.Count;


        int ixS = 0;
        int iyS = 0;
        for (int n = 0; n < Al_dot.Count; n++)
        {
            myDot dots = Al_dot[n];
            int xx = (int)(dots.x - l);
            int yy = (int)(dots.y - l);

            ixS += (xx - ix) * ((int)xx - ix);
            iyS += (yy - iy) * ((int)yy - iy);
        }

        ixS /= Al_dot.Count;
        iyS /= Al_dot.Count;

        return (ixS + iyS) / 2;
    }


    /// <summary>开始计算,矩形宽，高，面积，中心点，密度</summary>
    public void calculateS()
    {
        width = Math.Abs((int)l - (int)r);
        height = Math.Abs((int)t - (int)b);
        S = width * height;
    }

    public double iWH_per = 0;

    public double iTie_per = 0;
    public Rectangle rectFrame;

    public ArrayList AL_dotLeft;
    public ArrayList AL_dotRight;
    public ArrayList AL_dotTop;
    public ArrayList AL_dotBottom;
    uint iWidth = 0;

    /// <summary>开始计算,矩形宽，高，面积，中心点，密度</summary>
    public ArrayList calculateP(byte* p, Rectangle rect, int index, uint iwidth)
    {
        iWidth = iwidth;
        rectFrame = rect;
        width = Math.Abs((int)l - (int)r);
        height = Math.Abs((int)t - (int)b);

        AL_dotLeft = new ArrayList();
        AL_dotRight = new ArrayList();
        AL_dotTop = new ArrayList();
        AL_dotBottom = new ArrayList();
        ArrayList al = null;

        for (int i = 0; i < Al_dot.Count; i++)
        {
            myDot dot = Al_dot[i];

            if (dot.x >= rectFrame.X && dot.x < rectFrame.Width && dot.y >= rectFrame.Y && dot.y < rectFrame.Height)
            {
                iNumRect++;
            }

            if (index == 0)
            {
                //左上点 ->  往右 往下
                if (getRightDot(p, dot.x, dot.y) != 0)
                {
                    AL_dotRight.Add(dot);
                }
                if (getBottomDot(p, dot.x, dot.y) != 0)
                {
                    AL_dotBottom.Add(dot);
                }
            }
            else if (index == 1)
            {
                //右上点 ->  往左 往下
                if (getLeftDot(p, dot.x, dot.y) != 0)
                {
                    AL_dotLeft.Add(dot);
                }
                if (getBottomDot(p, dot.x, dot.y) != 0)
                {
                    AL_dotBottom.Add(dot);
                }
            }
            else if (index == 2)
            {
                //左下点 ->  往右 往上
                if (getRightDot(p, dot.x, dot.y) != 0)
                {
                    AL_dotRight.Add(dot);
                }
                if (getTopDot(p, dot.x, dot.y) != 0)
                {
                    AL_dotTop.Add(dot);
                }
            }
            else if (index == 3)
            {
                //右下点 ->  往左 往上
                if (getLeftDot(p, dot.x, dot.y) != 0)
                {
                    AL_dotLeft.Add(dot);
                }
                if (getTopDot(p, dot.x, dot.y) != 0)
                {
                    AL_dotTop.Add(dot);
                }
            }

            if (dot.x == rect.X && dot.y == rect.Y)
            {
                iDingNum++;
                dotLeftTop = dot;
            }
            else if (dot.x == rect.Width - 1 && dot.y == rect.Y)
            {
                iDingNum++;
                dotRightTop = dot;
            }
            else if (dot.x == rect.X && dot.y == rect.Height - 1)
            {
                iDingNum++;
                dotLeftBottom = dot;
            }
            else if (dot.x == rect.Width - 1 && dot.y == rect.Height - 1)
            {
                iDingNum++;
                dotRightBottom = dot;
            }
        }
        if (index == 0)
        {
            al = AL_dotRight;
            if (AL_dotRight.Count < AL_dotBottom.Count)
            {
                al = AL_dotBottom;
            }
        }
        else if (index == 1)
        {
            al = AL_dotLeft;
            if (AL_dotLeft.Count < AL_dotBottom.Count)
            {
                al = AL_dotBottom;
            }
        }
        else if (index == 2)
        {
            al = AL_dotRight;
            if (AL_dotRight.Count < AL_dotTop.Count)
            {
                al = AL_dotTop;
            }
        }
        else if (index == 3)
        {
            al = AL_dotLeft;
            if (AL_dotLeft.Count < AL_dotTop.Count)
            {
                al = AL_dotTop;
            }
        }
        return al;
    }

    int getTopDot(byte* p, uint x, uint y)
    {
        if (y > rectFrame.Y)
        {
            return p[(y - 1) * iWidth + x * 3];
        }
        else
        {
            return 0;
        }
    }
    int getBottomDot(byte* p, uint x, uint y)
    {
        if (y < rectFrame.Height - 1)
        {
            return p[(y + 1) * iWidth + x * 3];
        }
        else
        {
            return 0;
        }
    }

    int getLeftDot(byte* p, uint x, uint y)
    {
        if (x > rectFrame.X)
        {
            return p[y * iWidth + (x - 1) * 3];
        }
        else
        {
            return 0;
        }
    }
    int getRightDot(byte* p, uint x, uint y)
    {
        if (x < rectFrame.Width - 1)
        {
            return p[y * iWidth + (x + 1) * 3];
        }
        else
        {
            return 0;
        }
    }
    public void DrawDotBmp(FileInfo file, int w, int h, int[,] intRect, int index)
    {
        Bitmap bmp = new Bitmap(w, h);
        Graphics g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        g.Dispose();


        BitmapData bitData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        uint ic = (uint)(bitData.Stride - bitData.Width * 3);
        byte* p = (byte*)(bitData.Scan0.ToPointer());
        uint iWidth = (uint)w * 3 + ic;

        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                if (intRect[i, j] == -1)
                {
                    uint ib = (uint)j * iWidth + (uint)i * 3;
                    p[ib] = 0;
                    p[ib + 1] = 0;
                    p[ib + 2] = 0;
                }
            }
        }


        bmp.UnlockBits(bitData);

        if (1 == 1)
        {
            saveImage saveImage = new saveImage();
            saveImage.SaveNoDispose(bmp, file.FullName.Replace(".jpg", "_[" + l + "," + t + "," + index + "] _临时_" + Math.Round(idotBlack, 5) + ".jpg"));
        }
        bmp.Dispose();
    }

    /// <summary>
    /// 细化骨骼化细节实现
    /// </summary>
    public void toSmallLine_do(ref int[,] intRect, int w, int h)
    {
        ArrayList alXY = new ArrayList();
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (intRect[x, y] == -1)
                {
                    int[,] arrN = toN_N_int(intRect, x, y, 3, w, h);

                    //1.  2<=NZ(p)<=6
                    bool ok1 = false;
                    int NZ = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (!(i == 1 && j == 1))
                            {
                                if (arrN[i, j] == 1)
                                {
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
                        alXY.Add(new Point(x, y));
                    }
                }
            }
        }
        for (int i = 0; i < alXY.Count; i++)
        {
            Point point = (Point)alXY[i];
            intRect[point.X, point.Y] = 0;
        }

    }
    /// <summary>
    /// 加入了白点影响机制的 黑点比例
    /// </summary>
    public float idotBlack = 1;
    /// <summary>
    /// 未加入了白点影响机制 黑点比例
    /// </summary>
    public float idotBlackNum = 1;
    /// <summary>
    /// 计算联通区域的黑色块 是否为实心
    /// </summary>
    /// <param name="file"></param>
    /// <param name="iline"></param>
    /// <param name="irect"></param>
    /// <returns></returns>
    public float getDotBlack(FileInfo file)
    {
        if (width != 0) width = Math.Abs((int)l - (int)r) + 1;
        if (height != 0) height = Math.Abs((int)t - (int)b) + 1;

        int w = width;
        int h = height;
        int[,] intRect = null;
        float fw = 30f;
        if (width > fw)
        {
            //缩小
            float ipp = (float)width / fw;
            w = (int)Math.Round((float)width / ipp);
            h = (int)Math.Round((float)height / ipp);
            if (h == 0) h = 1;
            intRect = new int[w, h];
            for (int i = 0; i < Al_dot.Count; i++)
            {
                myDot dot = Al_dot[i];

                int ix = (int)Math.Round((float)(dot.x - l) / ipp);
                int iy = (int)Math.Round((float)(dot.y - t) / ipp);

                if (ix >= w) ix = w - 1;
                if (iy >= h) iy = h - 1;

                intRect[ix, iy] = -1;
            }
        }
        else
        {
            //放大
            //float ipp = (float)width / fw;
            //w = (int)Math.Round((float)width / ipp);
            //h = (int)Math.Round((float)height / ipp);
            //intRect = new int[w, h];
            //for (int i = 0; i < Al_dot.Count; i++)
            //{
            //    myDot dot = Al_dot[i];

            //    float ibei = 1 / ipp;
            //    for (int nx = 0; nx < (int)Math.Ceiling(ibei); nx++)
            //    {
            //        for (int ny = 0; ny < (int)Math.Ceiling(ibei); ny++)
            //        {
            //            int ix = (int)Math.Round((float)(dot.x - l) / ipp) + nx;
            //            int iy = (int)Math.Round((float)(dot.y - t) / ipp) + ny;

            //            if (ix >= w) ix = w - 1;
            //            if (iy >= h) iy = h - 1;

            //            intRect[ix, iy] = -1;
            //        }
            //    }
            //}

            intRect = new int[w, h];
            for (int i = 0; i < Al_dot.Count; i++)
            {
                myDot dot = Al_dot[i];
                intRect[dot.x - l, dot.y - t] = -1;
            }
        }

        //DrawDotBmp(file, w, h, intRect, 0);
        toSmallLine_do(ref intRect, w, h);
        //DrawDotBmp(file, w, h, intRect, 1);


        int idotNum = 0;
        int x = 0;
        int y = 0;
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                if (intRect[i, j] == -1)
                {
                    int idot = 0;
                    //向左
                    for (int n = -1; n <= 1; n++)
                    {
                        x = (int)i - 1; y = j + n;
                        if (x >= 0 && y >= 0 && x < w && y < h)
                        {
                            if (intRect[x, y] == -1)
                            {
                                idot += 1;
                            }
                            else
                            {
                                intRect[x, y]++;
                            }
                        }
                    }
                    //向中（上下）
                    for (int n = -1; n <= 1; n++)
                    {
                        if (n != 0)
                        {
                            x = (int)i; y = j + n;
                            if (x >= 0 && y >= 0 && x < w && y < h)
                            {
                                if (intRect[x, y] == -1)
                                {
                                    idot += 1;
                                }
                                else
                                {
                                    intRect[x, y]++;
                                }
                            }
                        }
                    }
                    //向右
                    for (int n = -1; n <= 1; n++)
                    {
                        x = (int)i + 1; y = j + n;
                        if (x >= 0 && y >= 0 && x < w && y < h)
                        {
                            if (intRect[x, y] == -1)
                            {
                                idot += 1;
                            }
                            else
                            {
                                intRect[x, y]++;
                            }
                        }
                    }
                    switch (idot)
                    {
                        case 1:
                            idotNum += 0;
                            break;
                        case 2:
                            idotNum += 1;
                            break;
                        case 3:
                            idotNum += 8;
                            break;
                        case 4:
                            idotNum += 32;
                            break;
                        case 5:
                            idotNum += 64;
                            break;
                        case 6:
                            idotNum += 128;
                            break;
                        case 7:
                            idotNum += 256;
                            break;
                        case 8:
                            idotNum += 512;
                            break;
                    }
                }
            }
        }
        int idotWhite = 0;
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                switch (intRect[i, j])
                {
                    case 1:
                        idotWhite += 0;
                        break;
                    case 2:
                        idotWhite += 0;
                        break;
                    case 3:
                        idotWhite += 0;
                        break;
                    case 4:
                        idotWhite += 0;
                        break;
                    case 5:
                        idotWhite += -2000;
                        break;
                    case 6:
                        idotWhite += -4000;
                        break;
                    case 7:
                        idotWhite += -6000;
                        break;
                    case 8:
                        idotWhite += -8000;
                        break;
                }
            }
        }
        idotBlack = 1;
        if (idotNum > 0)
        {
            idotBlack = (float)iNum / (float)(idotNum + idotWhite);
            idotBlackNum = (float)iNum / (float)(idotNum);
        }
        if (idotBlack < 0) idotBlack = 1;

        //DrawDotBmp(file, w, h, intRect, 0);
        //AciDebug.Debug(l + " " + t + " " + idotBlack + " " + iNum + " " + idotNum + " " + idotWhite);
        return idotBlack;
    }
    /// <summary>
    /// getD 以后才有值，里面是所有的边缘点 起点是从0开始 使用时需要加上 l 和 t
    /// </summary>
    public ArrayList AL_dotBian;
    /// <summary>
    /// 用边缘 与 点数 比较所有点的离散程度 一般在 0.2以下为实心
    /// </summary>
    /// <param name="file"></param>
    /// <param name="iline"></param>
    /// <param name="irect"></param>
    /// <returns></returns>
    public float getD(FileInfo file, int iline, int irect, bool show)
    {
        //if (iline != 7)
        //{
        //    return 0;
        //}
        float ipX = 1;
        float ipY = 1;
        float iDot = 30f;

        //int left = 0;
        //int top = 0;

        int w = width;
        int h = height;
        //float ip2 = 1;
        //if (width > iDot && height > iDot)
        //{
        if (w > h)
        {
            float per = (float)w / (float)h;
            ipX = ipY = (float)w / iDot;
            //ipY = (float)height / iDot;
            //left = (int)(iDot * pp);
            w = (int)iDot;
            h = (int)iDot;
        }
        else
        {
            float per = (float)h / (float)w;
            //ipX = (float)width / iDot;
            ipY = ipX = (float)h / iDot;
            h = (int)iDot;
            w = (int)iDot;
        }

        //}
        //else
        //{
        //if (width > height)
        //{
        //    ip2 = (float)height / iDot;
        //}
        //else
        //{
        //    ip2 = (float)width / iDot;
        //}
        //}

        //Bitmap bmp2 = new Bitmap(width, height);
        //Graphics g2 = Graphics.FromImage(bmp2);
        //g2.Clear(Color.White);
        //g2.Dispose();
        //for (int i = 0; i < Al_dot.Count; i++)
        //{
        //    myDot dot = Al_dot[i];
        //    bmp2.SetPixel((int)(dot.x - l), (int)(dot.y - t), Color.Black); 
        //}

        Bitmap bmp = new Bitmap(w, h);
        Graphics g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        //g.DrawImage(bmp2, 0, 0, w, h);
        g.Dispose();


        BitmapData bitData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        //取出每行多出来的数
        uint ic = (uint)(bitData.Stride - bitData.Width * 3);

        //取出全部像素值 每隔 3个  为  R G B 每次循环删除 一个多数的 xx 数
        byte* p = (byte*)(bitData.Scan0.ToPointer());

        //uint w1 = (uint)bitData.Width;
        //uint h1 = (uint)bitData.Height;

        //一行的像素点
        uint iWidth = (uint)w * 3 + ic;
        uint ipX_1 = (uint)(1 / ipX);
        uint ipY_1 = (uint)(1 / ipY);

        for (int i = 0; i < Al_dot.Count; i++)
        {
            myDot dot = Al_dot[i];

            uint ix = (uint)((float)(dot.x - l) / ipX);
            uint iy = (uint)((float)(dot.y - t) / ipY);

            if (ipX < 1)
            {
                for (uint y = iy; y <= iy + ipY_1; y++)
                {
                    for (uint x = ix; x <= ix + ipX_1; x++)
                    {
                        if (x < w && y < h)
                        {
                            uint ib = (uint)y * iWidth + (uint)x * 3;
                            p[ib] = 0;
                            p[ib + 1] = 0;
                            p[ib + 2] = 0;
                        }
                    }
                }
            }
            else
            {
                if (ix < w && iy < h)
                {
                    uint ib = (uint)iy * iWidth + (uint)ix * 3;
                    p[ib] = 0;
                    p[ib + 1] = 0;
                    p[ib + 2] = 0;
                }
            }
        }
        AL_dotBian = new ArrayList();
        ArrayList al_bian2 = new ArrayList();
        ArrayList al_xin = new ArrayList();
        Rect.getDotBian(ref AL_dotBian, ref al_bian2, ref al_xin, p, 0, 0, (uint)w, (uint)h, iWidth);
        float dd = ((float)al_xin.Count);// - (float)AL_dotBian.Count * 2
        if (dd > 0)
        {
            D = ((float)(AL_dotBian.Count + al_bian2.Count)) / dd;
        }
        else
        {
            D = 1000;
        }

        bmp.UnlockBits(bitData);

        if (show)
        {
            saveImage saveImage = new saveImage();
            saveImage.SaveNoDispose(bmp, file.FullName.Replace(".jpg", "_临时_s【" + AciMath.setNumberLength(iline, 3)
                + "-" + AciMath.setNumberLength(irect, 3) + "】【" + D + "】【" + al_xin.Count + "-" + AL_dotBian.Count + "-" + al_bian2.Count + "】.jpg"));
        }
        bmp.Dispose();

        return D;
    }
    /// <summary>
    /// 返回边缘点
    /// </summary>
    public static void getDotBian(ref ArrayList al_bian, ref ArrayList al_bian2, ref ArrayList al_xin, byte* p, uint l, uint t, uint w, uint h, uint iWidth)
    {
        uint[,] intCheck = null;

        w = l + w;
        h = t + h;
        int icheck = 0;
        ArrayList al_bian_xy = new ArrayList();

        for (uint y = t; y < h; y++)
        {
            for (uint x = l; x < w; x++)
            {
                myDot myP = new myDot(x, y, iWidth, p);

                if (p[myP.ib] == 0)
                {
                    icheck++;
                    int[,] arrN = toN_N(p, x, y, 3, w, h, iWidth);

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
                    if (ok1)
                    {
                        al_bian.Add(myP);
                        al_bian_xy.Add(x + "_" + y);
                        //p[myP.ib] = 0;
                        //p[myP.ib + 1] = 0;
                        //p[myP.ib + 2] = 255;
                    }
                    else
                    {
                        al_xin.Add(myP);
                    }
                }
            }
        }

        for (int k = 0; k <= 3; k++)
        {
            ArrayList al = (ArrayList)al_bian_xy.Clone();
            for (int i = 0; i < al_xin.Count; i++)
            {
                myDot myP = (myDot)al_xin[i];
                bool isok = false;
                for (uint y = myP.y - 1; y < myP.y + 1; y++)
                {
                    for (uint x = myP.x - 1; x < myP.x + 1; x++)
                    {
                        if (al.IndexOf(x + "_" + y) > -1)
                        {
                            al_bian_xy.Add(myP.x + "_" + myP.y);
                            al_bian2.Add(myP);
                            al_xin.Remove(myP);
                            i--;
                            isok = true;
                            break;
                        }
                    }
                    if (isok) break;
                }
            }
        }
    }
    /// <summary>
    /// 细化骨骼化
    /// </summary>
    public static void toSmallLine(byte* p, uint l, uint t, uint w, uint h, uint iWidth)
    {
        int iOverIndex = 1104;
        int imax = 1000;
        uint[,] intCheck = null;

        int it = 1;
        int index = 1;

        while (it != 0 && index <= imax)
        {
            index++;

            //bitmap.Save(file.FullName.Replace(".jpg", "_" + (index+100) + "_1.jpg"), myImageCodecInfo, encoderParams);
            if (it != 0 && index == iOverIndex)
            {
                if (intCheck == null)
                {
                    intCheck = new uint[w, h];
                    for (uint y = t; y < h; y++)
                    {
                        for (uint x = l; x < w; x++)
                        {
                            uint ib = y * iWidth + x * 3;
                            if (p[ib] == 0)
                            {
                                intCheck[x, y] = 1;
                            }
                        }
                    }
                }
                it = Rect.toSmallLine_do2(p, l, t, w, h, iWidth, intCheck);
            }
            else
            {
                it = Rect.toSmallLine_do(p, l, t, w, h, iWidth, intCheck);
            }
        }

    }
    /// <summary>
    /// 细化骨骼化细节实现
    /// </summary>
    public static int toSmallLine_do(byte* p, uint l, uint t, uint w, uint h, uint iWidth, uint[,] intCheck)
    {
        //DateTime dt1 = DateTime.Now;
        //TimeSpan ts;

        int ijs = 0;

        w = l + w;
        h = t + h;

        ArrayList alXY = new ArrayList();
        int icheck = 0;
        for (uint y = t; y < h; y++)
        {
            for (uint x = l; x < w; x++)
            {
                myPoint myP = new myPoint(x, y, iWidth);

                if (p[myP.ib] == 0)
                {
                    icheck++;
                    int[,] arrN = toN_N(p, x, y, 3, w, h, iWidth);

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
            int[,] arrN = toN_N(p, myP.x, myP.y, 3, w, h, iWidth);

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
    public static int toSmallLine_do2(byte* p, uint l, uint t, uint w, uint h, uint iWidth, uint[,] intCheck)
    {
        //DateTime dt1 = DateTime.Now;
        //TimeSpan ts;

        int ijs = 0;

        ArrayList alXY = new ArrayList();
        int icheck = 0;
        for (uint y = t; y < h; y++)
        {
            for (uint x = l; x < w; x++)
            {
                myPoint myP = new myPoint(x, y, iWidth);

                if (intCheck[x, y] == 1)
                {
                    icheck++;
                    int[,] arrN = toN_N(p, x, y, 3, w, h, iWidth);

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
            int[,] arrN = toN_N(p, myP.x, myP.y, 3, w, h, iWidth);

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
    public static int getZO(int[,] arrN, int it, int ik)
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
    public static int[,] toN_N(byte* p, uint x, uint y, uint n, uint w, uint h, uint iWidth)
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
    /// 返回某一点为中心的 N*N 的方格  如果超出边界 为 0, 黑色 为 1
    /// </summary>
    /// <param name="p"></param>
    /// <param name="x">中心点</param>
    /// <param name="y">中心点</param>
    /// <param name="n">矩阵的n  需要单数</param>
    /// <returns>n*n大小的数组 从上至下，从左至右</returns>
    public static int[,] toN_N_int(int[,] intRect, int x, int y, int n, int w, int h)
    {
        int N = ((int)n - 1) / 2;
        int[,] arrN = new int[n, n];
        for (int irow = -N; irow <= N; irow++)
        {
            for (int icell = -N; icell <= N; icell++)
            {
                int thisx = x + icell;
                int thisy = y + irow;
                //不超出边界
                if (thisx >= 0 && thisx < w && thisy >= 0 && thisy < h)
                {
                    if (intRect[thisx, thisy] == -1)
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










    //if (width > height)
    //{
    //    iWH_per = width / height;
    //}
    //else
    //{
    //    iWH_per = height / width;
    //}

    //S = width * height;
    //if (S != 0)
    //{
    //    P = (double)iNumShi / (double)iNum * 1000;
    //}
    //else
    //{
    //    P = 0;
    //}
    //}
}

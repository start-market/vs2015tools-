using System;
using System.Collections;
using System.Text;
using System.Drawing;

public unsafe class myDot
{
    public uint x;
    public uint y;
    /// <summary>-1�ϵ� 0����� 1�µ�</summary>
    public int type;
    public uint ib;
    /// <summary>1 ��ɫ 0 ��ɫ</summary>
    public int c;
    /// <summary> �Ƿ����ߵ�</summary>
    public bool bTie = false;
    /// <summary> �Ƿ���ʵ�ĵ�</summary>
    public bool bShi = false;
    public byte R = 255;
    public uint iwidth = 0;

    public byte iR = 0;
    public byte iG = 0;
    public byte iB = 0;

    public byte* p;

    public Point point;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ix"></param>
    /// <param name="iy"></param>
    /// <param name="iwidth"></param>
    /// <param name="t">-1�ϵ� 0����� 1�µ�</param>
    public myDot(uint ix, uint iy, uint width, int t)
    {
        x = ix;
        y = iy;
        type = t;
        iwidth = width;
        ib = y * iwidth + x * 3;
        c = 0;
        point = new Point((int)x, (int)y);
    }
    public myDot(uint ix, uint iy, uint iWidth, byte* P)
    {
        x = ix;
        y = iy;
        p = P;
        iwidth = iWidth;
        ib = y * iwidth + x * 3;
        R = p[ib];
        point = new Point((int)x, (int)y);
    }

    public myDot(uint ix, uint iy)
    {
        x = ix;
        y = iy;
        point = new Point((int)x, (int)y);
    }

    public void Draw(byte* pP)
    {
        uint ib = y * iwidth + x * 3;
        pP[ib] = iB;
        pP[ib + 1] = iG;
        pP[ib + 2] = iR;
    }
    
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;


public class HoughHbClass
{
    public double iall = 0;
    public uint ixy = 0;
    public bool isok = false;
    public int iLength = 0;
    public int iLength255 = 0;

    
    public int[] arrLength;
    public int[] arrLength255;
    public int[] arrLengthLong;

    /// <summary>直线的反向的区域起始</summary>
    public int iB = 0;
    /// <summary>直线的反向的区域结束</summary>
    public int iE = 0;
    public List<double> al_xy;
    public List<double> al_weight;
    public Rect rect = null;
    public string sHV = "";

    /// <summary>直线的起始</summary>
    public int ib = -1;
    /// <summary>直线的结束</summary>
    public int ie = -1;

    /// <summary>直线反向的起始</summary>
    public int ib2 = -1;
    /// <summary>直线反向的结束</summary>
    public int ie2 = -1;

    public HoughHbClass(int wh)
    {
        arrLength = new int[wh];
        al_xy = new List<double>();
        al_weight = new List<double>();
    }
    public void add(uint xy, Rect rRect, string hv)
    {
        sHV = hv;
        double dotAll = 0;
        rect = rRect;
        for (int i = 0; i < rect.Al_dot.Count; i++)
        {
            myDot dot = (myDot)rect.Al_dot[i];
            dotAll += dot.iR;
            uint _xy = 0;
            if (sHV == "V")
            {
                _xy = dot.y;
                if (ib == -1)
                {
                    ib = (int)rect.t;
                }
                else if (ib > rect.t)
                {
                    ib = (int)rect.t;
                }
                if (ie == -1)
                {
                    ie = (int)rect.b;
                }
                else if (ie < rect.b)
                {
                    ie = (int)rect.b;
                }
                if (ib2 == -1)
                {
                    ib2 = (int)rect.l;
                }
                else if (ib2 > rect.l)
                {
                    ib2 = (int)rect.l;
                }
                if (ie2 == -1)
                {
                    ie2 = (int)rect.r;
                }
                else if (ie2 < rect.r)
                {
                    ie2 = (int)rect.r;
                }
            }
            else if (sHV == "H")
            {
                _xy = dot.x;
                if (ib == -1)
                {
                    ib = (int)rect.l;
                }
                else if (ib > rect.l)
                {
                    ib = (int)rect.l;
                }
                if (ie == -1)
                {
                    ie = (int)rect.r;
                }
                else if (ie < rect.r)
                {
                    ie = (int)rect.r;
                }
                if (ib2 == -1)
                {
                    ib2 = (int)rect.t;
                }
                else if (ib2 > rect.t)
                {
                    ib2 = (int)rect.t;
                }
                if (ie2 == -1)
                {
                    ie2 = (int)rect.b;
                }
                else if (ie2 < rect.b)
                {
                    ie2 = (int)rect.b;
                }
            }
            arrLength[_xy] += dot.iR;
        }
        al_xy.Add((double)xy);
        al_weight.Add(dotAll);
        iall += dotAll;

        double dxy = 0;
        for (int i = 0; i < al_weight.Count; i++)
        {
            double ip = al_weight[i] / iall;
            dxy += al_xy[i] * ip;
        }
        ixy = (uint)dxy;
    }
    
    /// <summary>
    /// 01 化 arrLength  并设置  iLength
    /// </summary>
    public void setLength01()
    {
        int index = 0;
        for (int i = 0; i < arrLength.Length; i++)
        {
            double ivalue = (double)arrLength[i];
            if (ivalue > 0)
            {
                index++;
            }
        }
        iLength = index;
    }
    /// <summary>
    /// 取出较小的灰度 直线段 并设置  iLengthCut
    /// </summary>
    /// <param name="var">分段的数量 一般为10</param>
    /// <param name="ik">伐值 一般 80</param>
    public void setLength255(float var, int ik)
    {
        arrLength255 = new int[arrLength.Length];
        //获取黑度最大值
        float iMax = 0;
        for (int i = 0; i < arrLength.Length; i++)
        {
            if (arrLength[i] > 0)
            {
                if (iMax < arrLength[i])
                {
                    iMax = (float)arrLength[i];
                }
            }
        }
        

        int[] intArr = new int[(int)var + 1];
        //获得每个颜色的 个数
        int index = 0;
        for (int i = 0; i < arrLength.Length; i++)
        {
            int ivalue = arrLength[i];
            if (ivalue > 0)
            {
                int iv = (int)((float)ivalue / iMax * var);
                intArr[iv]++;
                index++;
            }
        }

        float iMaxDuan = 1;
        //更新黑度最大值为 总点数的 30%
        for (int i = intArr.Length - 1; i >= 0; i--)
        {
            if ((float)intArr[i] / index > 0.2f)
            {
                iMaxDuan = i + 1;
                break;
            }
        }
        //if (ixy == 407)
        //{
        //    string s = "";
        //}
        

        //255 归一化
        float pp = (iMax / var * (iMaxDuan + 1)) / 255f;
        for (int i = 0; i < arrLength.Length; i++)
        {
            if (arrLength[i] > 0)
            {
                int ivalue = (int)Math.Round((float)arrLength[i] / pp);
                if (ivalue > 255) ivalue = 255;
                arrLength255[i] = ivalue;
            }
        }

        index = 0;
        for (int i = 0; i < arrLength255.Length; i++)
        {
            if (arrLength255[i] > 0 && arrLength255[i] < ik)
            {
                arrLength255[i] = 0;
            }
            else if (arrLength255[i] > 0)
            {
                index++;
            }
        }
        iLength255 = index;
    }
    /// <summary>
    /// 设置加长线  在 arrLength 或 arrLengthCut 的每段基础上进行延伸
    /// </summary>
    /// <param name="value">每段延伸多少点</param>
    public void setLengthLong(int[] arrL, int value)
    {
        arrLengthLong = new int[arrL.Length];
        int index = 0;
        int ibb = 0;
        bool bBegin = false;
        for (int i = 0; i < arrL.Length; i++)
        {
            if (arrL[i] > 0)
            {
                if (bBegin == false)
                {
                    bBegin = true;
                    index = 1;
                    ibb = i;
                }
                else
                {
                    index++;
                }
            }
            else
            {
                if (bBegin)
                {
                    bBegin = false;
                    for (int k = ibb - value; k < ibb + index + value; k++)
                    {
                        if (k >= 0 && k < arrL.Length)
                        {
                            arrLengthLong[k] = 255;
                        }
                    }
                    string s = "";
                }
            }
        }
    }
    public int getLength255ByBE(int ib, int ie)
    {
        int inum = 0;
        for (int i = ib; i <= ie; i++)
        {
            if (arrLength255[i] >= 1)
            {
                inum++;
            }
        }
        return inum;
    }
    public bool isExistLine(int ib, int ie, int ilength)
    {
        bool isExist = false;
        int inum = 0;
        for (int i = ib; i <= ie; i++)
        {
            if (arrLength[i] >= 1)
            {
                inum++;
            }
        }
        if (ilength <= inum)
        {
            isExist = true;
        }

        return isExist;
    }

    ///// <summary>
    ///// 456号版本有断线
    ///// </summary>
    ///// <param name="ib"></param>
    ///// <param name="ie"></param>
    ///// <returns></returns>
    //public float getLength456(float ib, float ie)
    //{
    //    float ilength = 0;
    //    for (float i = ib; i < ie; i++)
    //    {
    //        if (arrLength255[(int)i] >= iavgGrey / 2)
    //        {
    //            ilength++;
    //        }
    //    }
    //    return ilength;
    //}

    
    ///// <summary>
    ///// 查看直线是否连接的比例 百分比越高 直接越完整 越没有断
    ///// </summary>
    ///// <returns></returns>
    //public float getPer()
    //{
    //    float index = 0;
    //    for (int i = 0; i < arrLength.Length; i++)
    //    {
    //        if (arrLength[i] >= 1)
    //        {
    //            index++;
    //        }
    //    }
    //    return index / (float)arrLength.Length;
    //}

}

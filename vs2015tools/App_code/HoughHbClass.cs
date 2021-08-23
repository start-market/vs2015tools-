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

    /// <summary>ֱ�ߵķ����������ʼ</summary>
    public int iB = 0;
    /// <summary>ֱ�ߵķ�����������</summary>
    public int iE = 0;
    public List<double> al_xy;
    public List<double> al_weight;
    public Rect rect = null;
    public string sHV = "";

    /// <summary>ֱ�ߵ���ʼ</summary>
    public int ib = -1;
    /// <summary>ֱ�ߵĽ���</summary>
    public int ie = -1;

    /// <summary>ֱ�߷������ʼ</summary>
    public int ib2 = -1;
    /// <summary>ֱ�߷���Ľ���</summary>
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
    /// 01 �� arrLength  ������  iLength
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
    /// ȡ����С�ĻҶ� ֱ�߶� ������  iLengthCut
    /// </summary>
    /// <param name="var">�ֶε����� һ��Ϊ10</param>
    /// <param name="ik">��ֵ һ�� 80</param>
    public void setLength255(float var, int ik)
    {
        arrLength255 = new int[arrLength.Length];
        //��ȡ�ڶ����ֵ
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
        //���ÿ����ɫ�� ����
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
        //���ºڶ����ֵΪ �ܵ����� 30%
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
        

        //255 ��һ��
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
    /// ���üӳ���  �� arrLength �� arrLengthCut ��ÿ�λ����Ͻ�������
    /// </summary>
    /// <param name="value">ÿ��������ٵ�</param>
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
    ///// 456�Ű汾�ж���
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
    ///// �鿴ֱ���Ƿ����ӵı��� �ٷֱ�Խ�� ֱ��Խ���� Խû�ж�
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

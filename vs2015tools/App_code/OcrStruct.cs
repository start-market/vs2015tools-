using System;
using System.Collections.Generic;
using System.Text;


public class OcrStruct
{
    public OcrStruct()
    {
    }
    /// <summary>
    /// ֱ��ͼ�ṹ��
    /// </summary>
    public struct Histogram
    {
        public int[] pixelNum;
        public uint maxK;

        public Histogram(int[] pNum, uint max)
        {
            pixelNum = pNum;
            maxK = max;
        }
    }
    //public struct Projection
    //{

    //}
}

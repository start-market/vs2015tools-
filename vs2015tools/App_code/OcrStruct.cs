using System;
using System.Collections.Generic;
using System.Text;


public class OcrStruct
{
    public OcrStruct()
    {
    }
    /// <summary>
    /// 直方图结构体
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

using System;
using System.Collections;
using System.Text;
using System.Drawing;

public class myKey
{
    /// <summary>页面宽度 默认为 800</summary>
    public int width = 500;
    /// <summary>二值化阀值 默认为 245</summary>
    public uint toTwo_value = 220;

    /// <summary>游程计算中的折点断开数 默认为 2</summary>
    public int toDotLine_dot = 3;
    /// <summary>游程计算中的计算过小的线段的阀值 默认为 10 个点 width/80</summary>
    public int toDotLine_line = 5;
    /// <summary>游程计算中去峰值的阀值 默认为 10 个点 width/100</summary>
    public int toDotLine_T = 10;
    /// <summary>游程计算中去除直线合并后点数小于该值的点 的直线 默认为 500 个点 width/2</summary>
    public int toDotLine_end = 300;

    /// <summary>游程计算倾斜中 大于该点的才加入倾斜权重 默认为 50 个点 width/40</summary>
    public int toDotLine_R_1 = 50;
    /// <summary>游程计算倾斜中 斜率小于该度的直线 默认为 15 度</summary>
    public int toDotLine_R_2 = 15;
    /// <summary>联通区域分割算法中 联通点数小于该点数(25点)的不纳入计算有效区域 width/40</summary>
    public int cutArea_dot = 0;
    /// <summary>联通区域分割算法中 分割表格的宽度 默认为 400 以上即 width/2</summary>
    public int cutArea_table = 0;


    ///<summary>有效区域矩形</summary>
    public Rect xRect = null;

    /// <summary>核心数据阀值</summary>
    public myKey()
    {
        toDotLine_line = width / 120;

        toDotLine_R_1 = width / 40;

        cutArea_dot = width / 40;
        cutArea_table = width / 20;
    }
    /// <summary>
    /// 设置有效区域
    /// </summary>
    /// <param name="rect"></param>
    public void setRect(Rect rect)
    {
        xRect = rect;
        xRect.calculate();

        toDotLine_line = xRect.width / 80;
        toDotLine_T = xRect.width / 50;
        toDotLine_end = xRect.width / 2;

        toDotLine_R_1 = xRect.width / 40;

        cutArea_table = width / 5;
    }
}

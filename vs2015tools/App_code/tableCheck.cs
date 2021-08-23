using System;
using System.Collections.Generic;
using System.Text;

public class tableCheck
{
    /// <summary>
    /// 直线数
    /// </summary>
    public int iTableLine = 0;
    /// <summary>
    /// 最大点数区域
    /// </summary>
    public Rect maxRect;
    /// <summary>
    /// 扁长型联通区域数量
    /// </summary>
    public int countRect = 0;
    /// <summary>
    /// 文件大小
    /// </summary>
    public long fileSize = 0;
    /// <summary>
    /// 方型联通区域
    /// </summary>
    public int countRectAll = 0;
    /// <summary>
    /// 有效联通区域的所占全部点数的百分比
    /// </summary>
    public double allRectDot = 0;
    /// <summary>
    /// 在平均灰度60% 深度以上的点的个数
    /// </summary>
    public int iUpDot = 0;

    public tableCheck()
    {
        maxRect = new Rect();
    }
}

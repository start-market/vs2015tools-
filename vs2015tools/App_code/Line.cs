using System;
using System.Collections;
using System.Text;
using System.Drawing;

public class Line
{
    public float k = 0;
    public float b = 0;
    /// <summary>倾斜角度</summary>
    public float r = 0;
    /// <summary>中心点</summary>
    public PointF pCenter;
    /// <summary>给定x 后的 参数点y</summary>
    public int Y;
    /// <summary>给定y 后的 参数点x</summary>
    public int X;

    /// <summary>最上点 Y</summary>
    public int minY;
    /// <summary>最下点 Y</summary>
    public int maxY;
    /// <summary>线段的宽度</summary>
    public int W;
    /// <summary>线段的高度x</summary>
    public int H;

    public uint x1;
    public uint y1;
    public uint x2;
    public uint y2;
    public int iNum;
    public ArrayList Al_dot;
    public ArrayList aPoint;


    public Line()
    {
        x1 = 0;
        y1 = 0;
        x2 = 0;
        y2 = 0;
        iNum = 0;
        pCenter = new PointF();
        Al_dot = new ArrayList();
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
    /// <summary>最小二乘法</summary>
    /// <param name="myX">无需计算时填-1</param>
    public void leastSquare(int myX)
    {
        leastSquare();
        getY(myX);
    }

    /// <summary>计算直线高度 与 直线的最上点minY 最下点maxY</summary>
    public void getHeight()
    {
        minY = maxY = -1;

        for (int i = 0; i < Al_dot.Count; i++)
        {
            myDot p = (myDot)Al_dot[i];
            int iy = (int)p.y;
            if (minY == -1)
            {
                minY = iy;
            }
            else if (minY > iy)
            {
                minY = iy;
            }

            if (maxY == -1)
            {
                maxY = iy;
            }
            else if (maxY < iy)
            {
                maxY = iy;
            }
        }
        H = Math.Abs(maxY - minY);

    }

    /// <summary>计算中心的X点</summary>
    public void getCenterX()
    {
        for (int i = 0; i < Al_dot.Count; i++)
        {
            myDot p = (myDot)Al_dot[i];
            pCenter.X += (float)p.x;
        }
        pCenter.X /= (float)iNum;
    }
    /// <summary>计算中心的Y点</summary>
    public void getCenterY()
    {
        for (int i = 0; i < Al_dot.Count; i++)
        {
            myDot p = (myDot)Al_dot[i];
            pCenter.Y += (float)p.y;
        }
        pCenter.Y /= (float)iNum;
    }

    /// <summary>最小二乘法</summary>
    /// <param name="myX">无需计算时填-1</param>
    public void leastSquare()
    {
        //pCenter = new Point();
        float n = Al_dot.Count;
        float xy = 0, x = 0, y = 0, x2 = 0;
        for (int i = 0; i < Al_dot.Count; i++)
        {
            myDot p = (myDot)Al_dot[i];
            //pCenter.X += (int)p.x;
            //pCenter.Y += (int)p.y;
            float px = (float)p.x;
            float py = (float)p.y;
            xy += px * py;
            x += px;
            y += py;
            x2 += px * px;
        }
        //pCenter.X /= Al_dot.Count;
        //pCenter.Y /= Al_dot.Count;

        k = (n * xy - x * y) / (n * x2 - x * x);
        b = y / n - k * x / n;

        r = k / (float)Math.PI * 180;
    }
    public int getX(int myY)
    {
        X = (int)((myY - b) / k);
        return X;
    }
    public int getY(int myX)
    {
        Y = (int)(myX * k + b);
        return Y;
    }

    /// <summary>
    /// 合并所有 x 轴相同的点
    /// </summary>
    /// <param name="dot"></param>
    public void BindDot_X()
    {
        ArrayList al = new ArrayList();

        for (int i = 0; i < Al_dot.Count; i++)
        {
            myDot dot = (myDot)Al_dot[i];
            bool isadd = true;
            for (int j = 0; j < al.Count; j++)
            {
                myDot d = (myDot)al[j];
                if (d.x == dot.x)
                {
                    isadd = false;
                    break;
                }
            }
            if (isadd)
            {
                al.Add(dot);
            }
        }
        Al_dot = al;
        iNum = Al_dot.Count;
    }

    /// <summary>
    /// 合并所有 y 轴相同的点
    /// </summary>
    /// <param name="dot"></param>
    public void BindDot_Y()
    {
        ArrayList al = new ArrayList();

        for (int i = 0; i < Al_dot.Count; i++)
        {
            myDot dot = (myDot)Al_dot[i];
            bool isadd = true;
            for (int j = 0; j < al.Count; j++)
            {
                myDot d = (myDot)al[j];
                if (d.y == dot.y)
                {
                    isadd = false;
                    break;
                }
            }
            if (isadd)
            {
                al.Add(dot);
            }
        }
        Al_dot = al;
        iNum = Al_dot.Count;
    }
}

using System;
using System.Collections.Generic;
using System.Text;

public class myPoint
{
    public uint x = 0;
    public uint y = 0;
    public uint ib = 0;
    public uint w = 0;
    public myPoint(uint ix, uint iy, uint iwidth)
    {
        x = ix;
        y = iy;
        w = iwidth;
        ib = y * w + x * 3;
    }
}

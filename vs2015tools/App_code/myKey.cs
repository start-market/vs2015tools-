using System;
using System.Collections;
using System.Text;
using System.Drawing;

public class myKey
{
    /// <summary>ҳ���� Ĭ��Ϊ 800</summary>
    public int width = 500;
    /// <summary>��ֵ����ֵ Ĭ��Ϊ 245</summary>
    public uint toTwo_value = 220;

    /// <summary>�γ̼����е��۵�Ͽ��� Ĭ��Ϊ 2</summary>
    public int toDotLine_dot = 3;
    /// <summary>�γ̼����еļ����С���߶εķ�ֵ Ĭ��Ϊ 10 ���� width/80</summary>
    public int toDotLine_line = 5;
    /// <summary>�γ̼�����ȥ��ֵ�ķ�ֵ Ĭ��Ϊ 10 ���� width/100</summary>
    public int toDotLine_T = 10;
    /// <summary>�γ̼�����ȥ��ֱ�ߺϲ������С�ڸ�ֵ�ĵ� ��ֱ�� Ĭ��Ϊ 500 ���� width/2</summary>
    public int toDotLine_end = 300;

    /// <summary>�γ̼�����б�� ���ڸõ�Ĳż�����бȨ�� Ĭ��Ϊ 50 ���� width/40</summary>
    public int toDotLine_R_1 = 50;
    /// <summary>�γ̼�����б�� б��С�ڸöȵ�ֱ�� Ĭ��Ϊ 15 ��</summary>
    public int toDotLine_R_2 = 15;
    /// <summary>��ͨ����ָ��㷨�� ��ͨ����С�ڸõ���(25��)�Ĳ����������Ч���� width/40</summary>
    public int cutArea_dot = 0;
    /// <summary>��ͨ����ָ��㷨�� �ָ���Ŀ�� Ĭ��Ϊ 400 ���ϼ� width/2</summary>
    public int cutArea_table = 0;


    ///<summary>��Ч�������</summary>
    public Rect xRect = null;

    /// <summary>�������ݷ�ֵ</summary>
    public myKey()
    {
        toDotLine_line = width / 120;

        toDotLine_R_1 = width / 40;

        cutArea_dot = width / 40;
        cutArea_table = width / 20;
    }
    /// <summary>
    /// ������Ч����
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

using System;
using System.Collections.Generic;
using System.Text;

public class tableCheck
{
    /// <summary>
    /// ֱ����
    /// </summary>
    public int iTableLine = 0;
    /// <summary>
    /// ����������
    /// </summary>
    public Rect maxRect;
    /// <summary>
    /// �ⳤ����ͨ��������
    /// </summary>
    public int countRect = 0;
    /// <summary>
    /// �ļ���С
    /// </summary>
    public long fileSize = 0;
    /// <summary>
    /// ������ͨ����
    /// </summary>
    public int countRectAll = 0;
    /// <summary>
    /// ��Ч��ͨ�������ռȫ�������İٷֱ�
    /// </summary>
    public double allRectDot = 0;
    /// <summary>
    /// ��ƽ���Ҷ�60% ������ϵĵ�ĸ���
    /// </summary>
    public int iUpDot = 0;

    public tableCheck()
    {
        maxRect = new Rect();
    }
}

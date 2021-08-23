using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

public unsafe class LineRow
{
    public int iCenterH = 0;
    public int iCenterV = 0;

    public List<Rect> AL_Rect = new List<Rect>();
    /// <summary>0 ��ʶ������1 ȥ������2 ��������3 ������ĸ��������</summary>
    int[] arrRemove = null;

    public Rect[] arrRectAll = null;
    public Rect[] arrRect = null;

    /// <summary>��</summary>
    public uint t;
    /// <summary>��</summary>
    public uint b;
    /// <summary>��</summary>
    public uint l;
    /// <summary>��</summary>
    public uint r;
    /// <summary>��</summary>
    public int width;
    /// <summary>��</summary>
    public int height;

    /// <summary>����Rect ��ƽ�����</summary>
    public int widthPer;
    /// <summary>����Rect ��ƽ���߶�</summary>
    public int heightPer;

    /// <summary>��ǰ�ڼ���</summary>
    public int Index;
    /// <summary>�ܼ�����</summary>
    public int Count;
    /// <summary>��ɢ�̶�  getD() ���</summary>
    public float D;

    public myOcrWord ocrWord = null;

    public LineRow(myOcrWord ocrword)
    {
        ocrWord = ocrword;
    }

    /// <summary>
    /// �ñ�Ե �� ���� �Ƚ����е����ɢ�̶� һ���� 0.2����Ϊʵ��
    /// </summary>
    public double getD(FileInfo file, int iline, bool show)
    {
        float id_all = 0;
        float id_count = 0;
        for (int i = 0; i < AL_Rect.Count; i++)
        {
            Rect rect = AL_Rect[i];
            rect.getD(file, iline, i, show);
            id_all += rect.D * rect.Al_dot.Count;
            id_count += rect.Al_dot.Count;
        }
        D = id_all / id_count;
        return D;
    }

    /// <summary>
    /// ����A �� ��B �Ƿ��а�����ϵ û�з��� null �з��� ��İ�����
    /// </summary>
    /// <param name="RectA"></param>
    /// <param name="RectB"></param>
    /// <returns></returns>
    public static LineRow LineA_in_LineB(LineRow LineA, LineRow LineB)
    {
        if (LineA.l <= LineB.l && LineA.r >= LineB.r && LineA.b <= LineB.b && LineA.t >= LineB.t)
        {
            return LineA;
        }
        else if (LineB.l <= LineA.l && LineB.r >= LineA.r && LineB.b <= LineA.b && LineB.t >= LineA.t)
        {
            //�ཻ
            return LineB;
        }
        return null;
    }

    /// <summary>
    /// �� AL_Rect �������� ���� ������ arrRect
    /// </summary>
    public void RectSort()
    {
        arrRectAll = new Rect[AL_Rect.Count];
        for (int i = 0; i < AL_Rect.Count; i++)
        {
            arrRectAll[i] = AL_Rect[i];
        }
        Array.Sort(arrRectAll, new sortAL_RecrX());
    }
    public void createBmp(OcrResult result, int index)
    {
        if (result != null)
        {
            result.createBitmap(AciMath.setNumberLength(Index, 2) + "_" + AciMath.setNumberLength(index, 3) + "_ָ����_" + result.DebugResult());
        }
    }
    double iH_09 = 0;
    double iH_085 = 0;
    double iH_08 = 0;
    double iH_105 = 0;
    double iH_11 = 0;
    double iH_07 = 0;
    double iH_06 = 0;
    double iH_05 = 0;
    double iH_04 = 0;
    double iH_02 = 0;
    double iH_03 = 0;
    double iH_015 = 0;
    double iH_01 = 0;
    double iH_100 = 0;
    /// <summary>
    /// ����ϲ������е����֣������ţ����ֵ�
    /// </summary>
    public void RectReset(byte* p, Rect.OcrMode ocr_mode)
    {
        iH_09 = (float)height * 0.9;
        iH_08 = (float)height * 0.8;
        iH_085 = (float)height * 0.85;
        iH_105 = (float)height * 1.05;
        iH_11 = (float)height * 1.1;
        iH_07 = (float)height * 0.7;
        iH_06 = (float)height * 0.6;
        iH_05 = (float)height * 0.5;
        iH_04 = (float)height * 0.4;
        iH_03 = (float)height * 0.3;
        iH_02 = (float)height * 0.2;
        iH_015 = (float)height * 0.15;
        iH_01 = (float)height * 0.1;
        iH_100 = (float)height;
        arrRemove = new int[arrRectAll.Length];

        int ic = 0;

        int icAdd = 0;

        for (int i = 0; i < arrRectAll.Length; i++)
        {
            Rect rect = arrRectAll[i];
            rect.width_line = height;
            rect.height_line = height;
        }

        for (int i = 0; i < arrRectAll.Length; i++)
        {
            if (arrRemove[i] != 1)
            {
                Rect rect = arrRectAll[i];
                ////////////////////////////////////////////////////
                //����ֻ�������أ��ϲ����ϵĲ���
                ////////////////////////////////////////////////////

                for (int j = 0; j < arrRectAll.Length; j++)
                {
                    if (i != j && arrRemove[i] != 1)
                    {
                        Rect r = arrRectAll[j];
                        if (j < i)
                        {
                            if (rect.r - r.l > iH_105) continue;
                        }
                        else
                        {
                            if (r.r - rect.l > iH_105) continue;
                        }
                        
                        if ((rect.l <= r.l && r.l <= rect.r) || (rect.l <= r.r && r.r <= rect.r))
                        {
                            arrRemove[j] = 1;
                            ic++;
                            rect.AddRect(r);
                        }
                    }
                }
            }
        }
       
        if (ocr_mode == Rect.OcrMode.Chinese)
        {
            //ʶ��Ƿ����� ���ж��Ƿ���Ӣ�ġ����֡����š�����
            for (int i = 0; i < arrRectAll.Length; i++)
            {
                if (arrRemove[i] != 1)
                {
                    Rect rect = arrRectAll[i];
                    ////////////////////////////////////////////////////
                    //���С�� 70% �Ҹ߶ȴ���  40% ��Ԫ�� �Ҳ� ��һ��Ԫ�� ���С�� 80% �߶ȴ��� 50% �Ҳ��������ұ�һ��Ԫ�غϲ��Ļ�
                    ////////////////////////////////////////////////////
                    if (rect.width <= iH_07 && rect.height >= iH_04 && rect.width >= iH_02)
                    {
                        rect.iOcrMode = Rect.OcrMode.Code_Int_English_Bushou;
                        OcrResult result = ocrWord.OcrRect(p, rect, i, null, "С����(" + (i + 1) + "/" + arrRectAll.Length + ")������(" + (Index + 1) + "/" + Count + ")", false);
                        rect.iOcrMode = Rect.OcrMode.Chinese;

                        //createBmp(result, i);

                        if (result.arrPer[0] >= 0.85 && result.arrMin[0] < 200
                            || (result.arrMode[0] == Rect.OcrMode.Bushou && result.arrMode[1] == Rect.OcrMode.Bushou
                            && result.arrName[0].Substring(0, 2) == result.arrName[1].Substring(0, 2))
                            )
                        {
                            //ȷ��ʶ��
                            if (result.arrMode[0] == Rect.OcrMode.Bushou)
                            {
                                //����
                                arrRemove[i] = 2;
                                //MessageBox.Show("��" + i + "�������ס�" + result.DebugResult());
                            }
                            else
                            {
                                //��ĸ���ַ���
                                arrRemove[i] = 3;
                                //MessageBox.Show("��" + i + "�����ǲ��ס�" + result.DebugResult());
                            }
                            rect.result = result;
                        }
                        else
                        {
                            //MessageBox.Show("��" + i + "����������" + result.DebugResult());
                        }
                    }
                }
            }
            //MessageBox.Show("11");
            //�������еı��Բ��׺�ϲ�����һ�κϲ���
            bool isHB = true;
            while (isHB)
            {
                isHB = false;
                for (int i = 0; i < arrRectAll.Length; i++)
                {
                    if (arrRemove[i] == 2)
                    {
                        Rect rect = arrRectAll[i];
                        string sLR = rect.result.arrName[0].Substring(1, 1);

                        bool ishb = false;

                        if (sLR == "��")
                        {
                            int iNext = i + 1;//�����Һϲ�
                            while (iNext < arrRectAll.Length)
                            {
                                if (arrRemove[iNext] == 0 || arrRemove[iNext] == 2)
                                {
                                    Rect r = arrRectAll[iNext];
                                    //Ԫ�غϲ���С�� 110%
                                    if (r.r - rect.l < iH_105)
                                    {
                                        ishb = true;
                                        arrRemove[iNext] = 1;
                                        ic++;
                                        rect.AddRect(r);
                                        if (r.result != null) rect.result.sResultWord += r.result.sResultWord;
                                        //MessageBox.Show(rect.result.sResultWord + "-" + r.result.sResultWord);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                iNext++;
                            }
                        }
                        else if (sLR == "��")
                        {
                            int iNext = i - 1;//�����Һϲ�
                            while (iNext >= 0)
                            {
                                if (arrRemove[iNext] == 0 || arrRemove[iNext] == 2)
                                {
                                    Rect r = arrRectAll[iNext];
                                    //Ԫ�غϲ���С�� 110%
                                    if (rect.r - r.l < iH_105)
                                    {
                                        ishb = true;
                                        arrRemove[iNext] = 1;
                                        ic++;
                                        rect.AddRect(r);
                                        if (r.result != null) rect.result.sResultWord += r.result.sResultWord;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                iNext--;
                            }
                        }
                        //�ָ��ǲ���
                        if (ishb)
                        {
                            isHB = true;
                            if (rect.width <= iH_07)
                            {
                                //��ʶ��һ��
                                rect.iOcrMode = Rect.OcrMode.Bushou;
                                OcrResult result = ocrWord.OcrRect(p, rect, i, null, "С����(" + (i + 1) + "/" + arrRectAll.Length + ")������(" + (Index + 1) + "/" + Count + ")", false);
                                rect.iOcrMode = Rect.OcrMode.Chinese;
                                //createBmp(result, i);
                                //MessageBox.Show(i + " " + rect.DebugString());
                                //MessageBox.Show(rect.width + "  " + result.DebugResult() + " ");
                                if (result.arrPer[0] >= 0.85 && result.arrMin[0] < 200)
                                {
                                    //����
                                    arrRemove[i] = 2;
                                    rect.result = result;
                                }
                            }
                            else if ((double)rect.width >= iH_085)
                            {
                                arrRemove[i] = 0;
                                rect.result = null;
                            }
                        }
                    }
                }
            }
            //MessageBox.Show("22");
            //����ʣ���޷��ϲ�����ı��Բ��׺�ϲ� ��ʶ�����ĸ�����ַ����ڶ��κϲ���
            isHB = true;
            while (isHB)
            {
                isHB = false;
                for (int i = 0; i < arrRectAll.Length; i++)
                {
                    if (arrRemove[i] == 2)
                    {
                        Rect rect = arrRectAll[i];
                        string sLR = rect.result.arrName[0].Substring(1, 1);

                        bool ishb = false;

                        if (sLR == "��")
                        {
                            int iNext = i + 1;//�����Һϲ�
                            while (iNext < arrRectAll.Length)
                            {
                                if (arrRemove[iNext] == 3)
                                {
                                    Rect r = arrRectAll[iNext];
                                    //Ԫ�غϲ���С�� 110%
                                    if (r.r - rect.l < iH_105)
                                    {
                                        if (r.result.arrMin[0] < rect.result.arrMin[0])
                                        {
                                            iNext++;
                                            continue;
                                        }

                                        ishb = true;
                                        arrRemove[iNext] = 1;
                                        ic++;
                                        rect.AddRect(r);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                iNext++;
                            }
                        }
                        else if (sLR == "��")
                        {
                            int iNext = i - 1;//�����Һϲ�
                            while (iNext >= 0)
                            {
                                if (arrRemove[iNext] == 3)
                                {
                                    Rect r = arrRectAll[iNext];
                                    //Ԫ�غϲ���С�� 110%
                                    if (rect.r - r.l < iH_105)
                                    {
                                        if (r.result.arrMin[0] < rect.result.arrMin[0])
                                        {
                                            iNext--;
                                            continue;
                                        }

                                        ishb = true;
                                        arrRemove[iNext] = 1;
                                        ic++;
                                        rect.AddRect(r);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                iNext--;
                            }
                        }

                        if (!ishb)
                        {
                            //createBmp(arrRectAll[i - 1].result, i - 1);
                            //createBmp(arrRectAll[i + 1].result, i + 1);
                            //createBmp(arrRectAll[i].result, i);
                            //MessageBox.Show("С���򣺡�" + i + "�� ȱʧ���ף�������(" + (Index + 1) + "/" + Count + ")");
                        }
                        //�ָ��ǲ���
                        if ((double)rect.width >= iH_085)
                        {
                            arrRemove[i] = 0;
                            rect.result = null;
                        }
                    }
                }
            }

            //ʵ���޷�����Ļظ��ǲ���  �����⻯����
            for (int i = 0; i < arrRectAll.Length; i++)
            {
                if (arrRemove[i] == 2)
                {
                    //���⻯����
                    Rect rect = arrRectAll[i];
                    //MessageBox.Show(rect.width + " " + height + " " + rect.result.sResultWord);
                    if (rect.result.sResultWord.IndexOf("����") > -1 || rect.result.sResultWord.IndexOf("����") > -1)
                    {
                        int iNext = i + 1;//�����Һϲ�
                        while (iNext < arrRectAll.Length)
                        {
                            if (arrRemove[iNext] != 1)
                            {
                                Rect r = arrRectAll[iNext];
                                //Ԫ�غϲ���С�� 110%
                                if (r.r - rect.l < iH_105)
                                {
                                    //MessageBox.Show(r.width + " " + height + " " + r.result.DebugResult());
                                    if ("jJ]})".IndexOf(r.result.sResultWord) > -1)
                                    {
                                        arrRemove[iNext] = 1;
                                        ic++;
                                        rect.AddRect(r);
                                        //MessageBox.Show(rect.width + "");
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                            iNext++;
                        }
                    }
                    else if (rect.result.sResultWord.IndexOf("ˮ��") > -1)
                    {
                        int iNext = i + 1;//�����Һϲ�
                        while (iNext < arrRectAll.Length)
                        {
                            if (arrRemove[iNext] != 1)
                            {
                                Rect r = arrRectAll[iNext];
                                //Ԫ�غϲ���С�� 110%
                                if (r.r - rect.l < iH_105)
                                {
                                    //MessageBox.Show(r.width + " " + height + " " + r.result.DebugResult());
                                    if ("[".IndexOf(r.result.sResultWord) > -1)
                                    {
                                        arrRemove[iNext] = 1;
                                        ic++;
                                        rect.AddRect(r);
                                        //MessageBox.Show(rect.width + "");
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                            iNext++;
                        }
                    }
                    else if (rect.result.sResultWord.IndexOf("С��") > -1)
                    {
                        int iNext = i + 1;//�����Һϲ�
                        while (iNext < arrRectAll.Length)
                        {
                            if (arrRemove[iNext] != 1)
                            {
                                Rect r = arrRectAll[iNext];
                                //Ԫ�غϲ���С�� 110%
                                if (r.r - rect.l < iH_105)
                                {
                                    //MessageBox.Show(r.width + " " + height + " " + r.result.DebugResult());
                                    if ("��".IndexOf(r.result.sResultWord) > -1)
                                    {
                                        arrRemove[iNext] = 1;
                                        ic++;
                                        rect.AddRect(r);
                                        //MessageBox.Show(rect.width + "");
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                            iNext++;
                        }
                    }

                    arrRemove[i] = 0;
                    rect.result = null;
                }
            }


            //MessageBox.Show("33");
            ////����������ϲ� (�ϴ�����
            isHB = true;
            while (isHB)
            {
                isHB = false;
                for (int i = 0; i < arrRectAll.Length; i++)
                {
                    if (arrRemove[i] == 0)
                    {
                        Rect rect = arrRectAll[i];
                        ////////////////////////////////////////////////////
                        //���С�� 70% �Ҹ߶ȴ���  50% ��Ԫ�� �Ҳ� ��һ��Ԫ�� ���С�� 80% �߶ȴ��� 50% �Ҳ��������ұ�һ��Ԫ�غϲ��Ļ�
                        ////////////////////////////////////////////////////
                        if (rect.width <= iH_08 && rect.height >= iH_04)
                        {
                            int iNext = i + 1;
                            //�����Һϲ�
                            while (iNext < arrRectAll.Length)
                            {
                                if (arrRemove[iNext] == 0)
                                {
                                    Rect r = arrRectAll[iNext];
                                    //Ԫ�غϲ���С�� 105%
                                    if (r.r - rect.l <= iH_100)
                                    {
                                        //�ټ�����ұ�һ��Ԫ���Ƿ��ܺϲ��Ҹ���
                                        int iJu = chechHebingJuRight(r, iNext);
                                        if (iJu == -1 || r.l - rect.r < iJu)
                                        {
                                            arrRemove[iNext] = 1;
                                            ic++;
                                            rect.AddRect(r);
                                            isHB = true;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                iNext++;
                            }
                            iNext = i - 1;
                            //������ϲ�
                            while (iNext >= 0)
                            {
                                if (arrRemove[iNext] == 0)
                                {
                                    Rect r = arrRectAll[iNext];
                                    //Ԫ�غϲ���С�� 105%
                                    if (rect.r - r.l <= iH_100)
                                    {
                                        //�ټ�����ұ�һ��Ԫ���Ƿ��ܺϲ��Ҹ���
                                        int iJu = chechHebingJuLeft(r, iNext);
                                        if (iJu == -1 || rect.l - r.r < iJu)
                                        {
                                            arrRemove[iNext] = 1;
                                            ic++;
                                            rect.AddRect(r);
                                            isHB = true;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                iNext--;
                            }
                        }
                    }
                }
            }


            //����������ϲ� (��С����
            isHB = true;
            while (isHB)
            {
                isHB = false;
                for (int i = 0; i < arrRectAll.Length; i++)
                {
                    if (arrRemove[i] == 0)
                    {
                        Rect rect = arrRectAll[i];
                        ////////////////////////////////////////////////////
                        //���С�� 80% �Ҹ߶ȴ���  50% ��Ԫ�� �Ҳ� ��һ��Ԫ�� ���С�� 80% �߶ȴ��� 50% �Ҳ��������ұ�һ��Ԫ�غϲ��Ļ�
                        ////////////////////////////////////////////////////
                        if (rect.width <= iH_03)
                        {
                            int iNext = i + 1;
                            //�����Һϲ�
                            while (iNext < arrRectAll.Length)
                            {
                                if (arrRemove[iNext] == 0)
                                {
                                    Rect r = arrRectAll[iNext];
                                    //Ԫ�غϲ���С�� 105%
                                    if (r.r - rect.l <= iH_100 && ((r.width > iH_07 && r.height > iH_07) || (r.width <= iH_04 && r.height <= iH_04)))
                                    {
                                        //�ټ�����ұ�һ��Ԫ���Ƿ��ܺϲ��Ҹ���
                                        int iJu = chechHebingJuRight(r, iNext);
                                        if (iJu == -1 || r.l - rect.r < iJu)
                                        {
                                            arrRemove[iNext] = 1;
                                            ic++;
                                            rect.AddRect(r);
                                            isHB = true;
                                        }
                                    }
                                    break;
                                }
                                iNext++;
                            }

                            iNext = i - 1;
                            //������ϲ�
                            while (iNext >= 0)
                            {
                                if (arrRemove[iNext] == 0)
                                {
                                    Rect r = arrRectAll[iNext];
                                    //Ԫ�غϲ���С�� 105%
                                    if (rect.r - r.l <= iH_100 && ((r.width > iH_07 && r.height > iH_07) || (r.width <= iH_04 && r.height <= iH_04)))
                                    {
                                        //�ټ�����ұ�һ��Ԫ���Ƿ��ܺϲ��Ҹ���
                                        int iJu = chechHebingJuLeft(r, iNext);
                                        if (iJu == -1 || rect.l - r.r < iJu)
                                        {
                                            arrRemove[iNext] = 1;
                                            ic++;
                                            rect.AddRect(r);
                                            isHB = true;
                                        }
                                    }
                                    break;
                                }
                                iNext--;
                            }
                        }
                    }
                }
            }


            //MessageBox.Show("33");
            ////�������������������Һϲ�
            //////
            isHB = true;
            while (isHB)
            {
                isHB = false;
                for (int i = 0; i < arrRectAll.Length; i++)
                {
                    if (arrRemove[i] == 0)
                    {
                        Rect rect = arrRectAll[i];
                        ////////////////////////////////////////////////////
                        //���С�� 70% �Ҹ߶ȴ���  50% ��Ԫ�� �Ҳ� ��һ��Ԫ�� ���С�� 80% �߶ȴ��� 50% �Ҳ��������ұ�һ��Ԫ�غϲ��Ļ�
                        ////////////////////////////////////////////////////
                        if (rect.width > iH_11 && rect.height > iH_07)
                        {
                            int iNext = i + 1;
                            //�����Һϲ�
                            while (iNext < arrRectAll.Length)
                            {
                                if (arrRemove[iNext] == 0)
                                {
                                    Rect r = arrRectAll[iNext];
                                    //Ԫ�غϲ���С�� 105%
                                    if (r.r - rect.l <= iH_100 * 2)
                                    {
                                        //�ټ�����ұ�һ��Ԫ���Ƿ��ܺϲ��Ҹ���
                                        int iJu = chechHebingJuRight(r, iNext);
                                        if (iJu == -1 || r.l - rect.r < iJu)
                                        {
                                            arrRemove[iNext] = 1;
                                            ic++;
                                            rect.AddRect(r);
                                            isHB = true;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                iNext++;
                            }
                            iNext = i - 1;
                            //������ϲ�
                            while (iNext >= 0)
                            {
                                if (arrRemove[iNext] == 0)
                                {
                                    Rect r = arrRectAll[iNext];
                                    //Ԫ�غϲ���С�� 105%
                                    if (rect.r - r.l <= iH_100 * 2)
                                    {
                                        //�ټ�����ұ�һ��Ԫ���Ƿ��ܺϲ��Ҹ���
                                        int iJu = chechHebingJuLeft(r, iNext);
                                        if (iJu == -1 || rect.l - r.r < iJu)
                                        {
                                            arrRemove[iNext] = 1;
                                            ic++;
                                            rect.AddRect(r);
                                            isHB = true;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                iNext--;
                            }
                        }
                    }
                }
            }


            for (int i = 0; i < arrRectAll.Length; i++)
            {
                if (arrRemove[i] == 0)
                {
                    Rect rect = arrRectAll[i];
                    ////////////////////////////////////////////////////
                    //�����źϲ�  ������ С�� 40% ��ȸ߶�
                    ////////////////////////////////////////////////////
                    if (rect.height < iH_05 && rect.width < iH_07)
                    {
                        for (int j = 0; j < arrRectAll.Length; j++)
                        {
                            if (i != j && arrRemove[j] == 0)
                            {
                                Rect r = arrRectAll[j];
                                if ((rect.l <= r.l && r.l <= rect.r)
                                    || (rect.l <= r.r && r.r <= rect.r)
                                    || (r.l <= rect.l && rect.l <= r.r)
                                    || (r.l <= rect.r && rect.r <= r.r)
                                    )
                                {
                                    arrRemove[j] = 1;
                                    ic++;
                                    rect.AddRect(r);
                                }
                            }
                        }
                        if (Math.Abs((int)rect.b - (int)b) < iH_03)
                        {
                            rect.iOcrMode = Rect.OcrMode.CodeDown;
                        }
                        else if (Math.Abs((int)rect.t - (int)t) < iH_03)
                        {
                            rect.iOcrMode = Rect.OcrMode.CodeUp;
                        }
                        else
                        {
                            rect.iOcrMode = Rect.OcrMode.Code_English;
                        }
                    }
                    ////////////////////////////////////////////////////
                    //���ֻ���ĸ�����   ���ֻ���ĸ����� С���� 50% ��ȸ߶�
                    ////////////////////////////////////////////////////
                    else if (rect.height >= iH_05 && rect.width <= iH_05)
                    {
                        rect.iOcrMode = Rect.OcrMode.Code_Int_English;
                    }
                    ////////////////////////////////////////////////////
                    //���ֻ���ĸ�����   ���ֻ���ĸ����� С�� 70% ��ȸ߶�
                    ////////////////////////////////////////////////////
                    //else if (rect.height > iH_07 && rect.width < iH_07)
                    //{
                    //    rect.iOcrMode = Rect.OcrMode.Code_Int_English_ChineseOther;
                    //}
                }
            }

            ////////////////////////////////////////////////////
            //�ָ�౶����
            ////////////////////////////////////////////////////
            for (int i = 0; i < arrRectAll.Length; i++)
            {
                if (arrRemove[i] == 0)
                {
                    Rect rect = arrRectAll[i];
                    if (rect.height >= iH_05 && rect.width > iH_105)
                    {
                        double iper = (double)rect.width / (double)iH_100;
                        int icc = 0;
                        if (iper.ToString().IndexOf(".") > -1)
                        {
                            icc = (int)Math.Round(iper);
                        }
                        else
                        {
                            icc = (int)iper;
                        }
                        rect.icut = icc;
                        if (icc > 1)
                        {
                            icAdd += icc - 1;
                        }
                    }
                }
            }

        }
        else if (ocr_mode == Rect.OcrMode.Int_English || ocr_mode == Rect.OcrMode.Int || ocr_mode == Rect.OcrMode.English || ocr_mode == Rect.OcrMode.Code_Int_English)
        {
            for (int i = 0; i < arrRectAll.Length; i++)
            {
                if (arrRemove[i] == 0)
                {
                    Rect rect = arrRectAll[i];
                    rect.iOcrMode = ocr_mode;

                    ////////////////////////////////////////////////////
                    //�����źϲ�  ������ С�� 40% ��ȸ߶�
                    ////////////////////////////////////////////////////
                    if (rect.height < iH_05 && rect.width < iH_05)
                    {
                        for (int j = 0; j < arrRectAll.Length; j++)
                        {
                            if (i != j && arrRemove[j] == 0)
                            {
                                Rect r = arrRectAll[j];
                                if ((rect.l <= r.l && r.l <= rect.r)
                                    || (rect.l <= r.r && r.r <= rect.r)
                                    || (r.l <= rect.l && rect.l <= r.r)
                                    || (r.l <= rect.r && rect.r <= r.r)
                                    )
                                {
                                    arrRemove[j] = 1;
                                    ic++;
                                    rect.AddRect(r);
                                }
                            }
                        }
                        if (ocr_mode == Rect.OcrMode.Code_Int_English)
                        {
                            if (Math.Abs((int)rect.b - (int)b) < iH_03)
                            {
                                rect.iOcrMode = Rect.OcrMode.CodeDown;
                            }
                            else if (Math.Abs((int)rect.t - (int)t) < iH_03)
                            {
                                rect.iOcrMode = Rect.OcrMode.CodeUp;
                            }
                            else
                            {
                                rect.iOcrMode = Rect.OcrMode.Code_English;
                            }
                        }
                    }
                }
            }
            //////////////////////////////////////////////////////
            ////��Ӣ����ȷ���Ƿ��Ǵ�д��
            //////////////////////////////////////////////////////
            //if (ocr_mode == Rect.OcrMode.Int_English || ocr_mode == Rect.OcrMode.English || ocr_mode == Rect.OcrMode.Code_Int_English)
            //{
            //}
            ////////////////////////////////////////////////////
            //�ָ�౶����
            ////////////////////////////////////////////////////
            for (int i = 0; i < arrRectAll.Length; i++)
            {
                if (arrRemove[i] == 0)
                {
                    Rect rect = arrRectAll[i];
                    if (rect.height >= iH_05 && rect.width > iH_100)
                    {
                        double iper = (double)rect.width / (double)iH_06;
                        int icc = 0;
                        if (iper.ToString().IndexOf(".") > -1)
                        {
                            icc = (int)Math.Round(iper);
                        }
                        else
                        {
                            icc = (int)iper;
                        }
                        rect.icut = icc;
                        if (icc > 1)
                        {
                            icAdd += icc - 1;
                        }
                    }
                }
            }
        }
        
        //�������պϲ�����
        arrRect = new Rect[arrRectAll.Length - ic + icAdd];
        int it = 0;
        for (int i = 0; i < arrRectAll.Length; i++)
        {
            if (arrRemove[i] != 1)
            {
                Rect rect = arrRectAll[i];
                if (rect.icut <= 1)
                {
                    arrRect[it] = arrRectAll[i];
                    it++;
                }
                else
                {
                    //��ʼ����������
                    int iwidth = (int)Math.Ceiling((double)rect.width / (double)rect.icut);
                    for (int j = 1; j <= rect.icut; j++)
                    {
                        Rect r = new Rect(rect.t, (uint)(rect.r - iwidth * (rect.icut - j)), rect.b, (uint)(rect.l + iwidth * (j - 1)));
                        r.iOcrMode = rect.iOcrMode;
                        //MessageBox.Show(r.DebugString());
                        arrRect[it] = r;
                        it++;
                    }
                }
            }
        }
        //MessageBox.Show(arrRect.Length + " " + arrRectAll.Length);
    }
    /// <summary>
    /// �������Ƿ��кϲ��ҷ��ؾ����Ƕ���     -1 ʱû�кϲ� ���ڵ���0ʱΪ����
    /// </summary>
    int chechHebingJuLeft(Rect rect, int index)
    {
        int iJu = -1;
        int iNext = index - 1;
        //�����Һϲ�
        while (iNext >= 0)
        {
            if (arrRemove[iNext] == 0)
            {
                Rect r = arrRectAll[iNext];
                //���С�� 80% �߶ȴ��� 50%
                if (rect.width < iH_08 && rect.height > iH_05)
                {
                    //Ԫ�غϲ���С�� 105%
                    if (rect.r - r.l < iH_105)
                    {
                        iJu = (int)(rect.l - r.r);
                    }
                }
                break;
            }
            iNext--;
        }
        return iJu;
    }
    /// <summary>
    /// ����ұ��Ƿ��кϲ��ҷ��ؾ����Ƕ���     -1 ʱû�кϲ� ���ڵ���0ʱΪ����
    /// </summary>
    int chechHebingJuRight(Rect rect, int index)
    {
        int iJu = -1;
        int iNext = index + 1;
        //�����Һϲ�
        while (iNext < arrRectAll.Length)
        {
            if (arrRemove[iNext] == 0)
            {
                Rect r = arrRectAll[iNext];
                //���С�� 80% �߶ȴ��� 50%
                if (rect.width < iH_08 && rect.height > iH_05)
                {
                    //Ԫ�غϲ���С�� 105%
                    if (r.r - rect.l < iH_105)
                    {
                        iJu = (int)(r.l - rect.r);
                    }
                }
                break;
            }
            iNext++;
        }
        return iJu;
    }

    public void DrawRect(byte* p, uint w, uint h, uint iWidth)
    {
        for (int j = 0; j < arrRect.Length; j++)
        {
            arrRect[j].Draw(p, iWidth);
        }
    }
    public void DrawRect2(byte* p, uint w, uint h, uint iWidth)
    {
        for (int j = 0; j < arrRectAll.Length; j++)
        {
            arrRectAll[j].Draw(p, iWidth);
        }
    }
    public ArrayList Draw(byte* p, uint w, uint h, uint iWidth)
    {
        return Draw(p, w, h, iWidth, true, 0, 0, 255, 1f);
    }
    public ArrayList Draw(byte* p, uint w, uint h, uint iWidth, bool bCenterLine)
    {
        return Draw(p, w, h, iWidth, bCenterLine, 0, 0, 255, 1f);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="p"></param>
    /// <param name="w"></param>
    /// <param name="h"></param>
    /// <param name="iWidth"></param>
    /// <param name="bCenterLine"></param>
    /// <param name="iR"></param>
    /// <param name="iG"></param>
    /// <param name="iB"></param>
    /// <param name="alpha">0-1 ͸���� Ĭ��1</param>
    /// <returns></returns>
    public ArrayList Draw(byte* p, uint w, uint h, uint iWidth, bool bCenterLine, byte iR, byte iG, byte iB, float alpha)
    {
        ArrayList al = new ArrayList();

        if (bCenterLine)
        {
            for (uint x = 0; x < w; x++)
            {
                uint ib = (uint)(iCenterV * iWidth + x * 3);
                p[ib] = 0;
                p[ib + 1] = 0;
                p[ib + 2] = 255;
            }
        }

        uint thist = 0;
        if (t - 1 >= 0)
        {
            thist = t - 1;
        }

        uint thisl = 0;
        if (l - 1 >= 0)
        {
            thisl = l - 1;
        }
        for (uint y = thist; y <= b; y++)
        {
            for (uint x = thisl; x <= r; x++)
            {
                if (y == thist || y == b || x == thisl || x == r)
                {
                    uint ib = y * iWidth + x * 3;
                    myDot dot = new myDot(x, y, iWidth, p);
                    dot.iB = p[ib];
                    dot.iG = p[ib + 1];
                    dot.iR = p[ib + 2];
                    al.Add(dot);

                    p[ib] = (byte)((float)iB * alpha + (float)p[ib] * (1 - alpha));
                    p[ib + 1] = (byte)((float)iG * alpha + (float)p[ib + 1] * (1 - alpha));
                    p[ib + 2] = (byte)((float)iR * alpha + (float)p[ib + 2] * (1 - alpha));
                }
            }
        }
        return al;
    }
    ArrayList AL_drawOld = new ArrayList();
    /// <summary>
    /// �ú�� ��������
    /// </summary>
    /// <param name="p"></param>
    /// <param name="iWidth"></param>
    public ArrayList Draw(ref int[,] intBmp)
    {
        if (AL_drawOld == null) AL_drawOld = new ArrayList();

        uint thist = 0;
        if ((int)t - 1 >= 0)
        {
            thist = t - 1;
        }

        uint thisl = 0;
        if ((int)l - 1 >= 0)
        {
            thisl = l - 1;
        }

        for (uint y = thist; y <= b - 1; y++)
        {
            for (uint x = thisl; x <= r - 1; x++)
            {
                if (y == thist || y == b - 1 || x == thisl || x == r - 1)
                {
                    int ito = intBmp[x, y];
                    if (ito >= 0)
                    {
                        myDot dot = new myDot(x, y);
                        dot.iR = (byte)ito;
                        AL_drawOld.Add(dot);
                        intBmp[x, y] = -2;
                    }
                }
            }
        }
        return AL_drawOld;
    }
    /// <summary>
    /// �������� ��Ļָ�
    /// </summary>
    /// <param name="p"></param>
    /// <param name="iWidth"></param>
    public void DrawReturn(ref int[,] intBmp)
    {
        for (int i = 0; i < AL_drawOld.Count; i++)
        {
            myDot dot = (myDot)AL_drawOld[i];
            intBmp[dot.x, dot.y] = dot.iR;
        }
        AL_drawOld = new ArrayList();
    }

    public void Add(LineRow line)
    {
        for (int i = 0; i < line.AL_Rect.Count; i++)
        {
            Rect rect = (Rect)line.AL_Rect[i];
            if (AL_Rect.IndexOf(rect) < 0)
            {
                AL_Rect.Add(rect);

                if (l > rect.l) l = rect.l;
                if (r < rect.r) r = rect.r;
                if (t > rect.t) t = rect.t;
                if (b < rect.b) b = rect.b;
            }
        }

        double ic = 0;
        for (int i = 0; i < AL_Rect.Count; i++)
        {
            Rect rrect = AL_Rect[i];
            ic += (double)rrect.pCenter.Y;
        }
        iCenterV = (int)Math.Round(ic / (double)AL_Rect.Count);

        width = Math.Abs((int)l - (int)r) + 1;
        height = Math.Abs((int)t - (int)b) + 1;
    }
    public void Add(Rect rect)
    {
        AL_Rect.Add(rect);
        rect.line = this;
        if (AL_Rect.Count == 1)
        {
            heightPer = rect.height;
            widthPer = rect.width;
            iCenterH = rect.pCenter.X;
            iCenterV = rect.pCenter.Y;
            l = rect.l;
            r = rect.r;
            b = rect.b;
            t = rect.t;
            width = rect.width;
            height = rect.height;
        }
        else
        {
            if (l > rect.l) l = rect.l;
            if (r < rect.r) r = rect.r;
            if (t > rect.t) t = rect.t;
            if (b < rect.b) b = rect.b;

            //double ic = 0;
            //double ic2 = 0;
            double ic3 = 0;
            double ic4 = 0;
            for (int i = 0; i < AL_Rect.Count; i++)
            {
                Rect rrect = AL_Rect[i];
                //ic += (double)rrect.pCenter.Y;
                //ic2 += (double)rrect.pCenter.X;
                ic3 += rrect.height;
                ic4 += rrect.width;
            }
            //iCenterH = (int)Math.Round(ic / (double)AL_Rect.Count);
            //iCenterV = (int)Math.Round(ic2 / (double)AL_Rect.Count);
            iCenterH = ((int)l + (int)r) / 2;
            iCenterV = ((int)t + (int)b) / 2;
            heightPer = (int)Math.Round(ic3 / (double)AL_Rect.Count);
            widthPer = (int)Math.Round(ic4 / (double)AL_Rect.Count);

            width = Math.Abs((int)l - (int)r) + 1;
            height = Math.Abs((int)t - (int)b) + 1;
        }
    }

    /// <summary>
    /// �� X��С���� ����
    /// </summary>
    public class sortAL_RecrX : IComparer
    {
        public int Compare(object x, object y)
        {
            if (x == null && y == null)
            {
                return 0;
            }
            if (x == null)
            {
                return -1;
            }
            if (y == null)
            {
                return 1;
            }
            Rect xInfo = (Rect)x;
            Rect yInfo = (Rect)y;


            //�����Q����    
            return xInfo.l.CompareTo(yInfo.l);//�f��    
            //return yInfo.FullName.CompareTo(xInfo.FullName);//�f�p  
        }
    }
    public class RectangleL
    {
        public uint X = 0;
        public uint Y = 0;
        public uint Width = 0;
        public uint Height = 0;
        public RectangleL(uint x, uint y, uint width, uint height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        public void Debug()
        {
            AciDebug.Debug(X + " " + Y + " " + Width + " " + Height);
        }
        public string DebugString()
        {
            return "��" + X + " �ң�" + Y + "�ϣ�" + Width + " �£�" + Height;
        }
    }
}

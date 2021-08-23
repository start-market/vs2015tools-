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
    /// <summary>0 待识别区域，1 去除区域，2 部首区域，3 数字字母符号区域</summary>
    int[] arrRemove = null;

    public Rect[] arrRectAll = null;
    public Rect[] arrRect = null;

    /// <summary>上</summary>
    public uint t;
    /// <summary>下</summary>
    public uint b;
    /// <summary>左</summary>
    public uint l;
    /// <summary>右</summary>
    public uint r;
    /// <summary>宽</summary>
    public int width;
    /// <summary>高</summary>
    public int height;

    /// <summary>里面Rect 的平均宽度</summary>
    public int widthPer;
    /// <summary>里面Rect 的平均高度</summary>
    public int heightPer;

    /// <summary>当前第几排</summary>
    public int Index;
    /// <summary>总计排数</summary>
    public int Count;
    /// <summary>离散程度  getD() 获得</summary>
    public float D;

    public myOcrWord ocrWord = null;

    public LineRow(myOcrWord ocrword)
    {
        ocrWord = ocrword;
    }

    /// <summary>
    /// 用边缘 与 点数 比较所有点的离散程度 一般在 0.2以下为实心
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
    /// 区域A 与 行B 是否有包含关系 没有返回 null 有返回 大的包含体
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
            //相交
            return LineB;
        }
        return null;
    }

    /// <summary>
    /// 将 AL_Rect 进行排序 生成 排序后的 arrRect
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
            result.createBitmap(AciMath.setNumberLength(Index, 2) + "_" + AciMath.setNumberLength(index, 3) + "_指定的_" + result.DebugResult());
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
    /// 整理合并该行中的文字，标点符号，数字等
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
                //文字只有纵向半截，合并其上的部分
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
            //识别非方形字 以判断是否是英文、数字、符号、边旁
            for (int i = 0; i < arrRectAll.Length; i++)
            {
                if (arrRemove[i] != 1)
                {
                    Rect rect = arrRectAll[i];
                    ////////////////////////////////////////////////////
                    //宽度小于 70% 且高度大于  40% 的元素 右侧 有一个元素 宽度小于 80% 高度大于 50% 且不能与再右边一个元素合并的话
                    ////////////////////////////////////////////////////
                    if (rect.width <= iH_07 && rect.height >= iH_04 && rect.width >= iH_02)
                    {
                        rect.iOcrMode = Rect.OcrMode.Code_Int_English_Bushou;
                        OcrResult result = ocrWord.OcrRect(p, rect, i, null, "小区域：(" + (i + 1) + "/" + arrRectAll.Length + ")行数：(" + (Index + 1) + "/" + Count + ")", false);
                        rect.iOcrMode = Rect.OcrMode.Chinese;

                        //createBmp(result, i);

                        if (result.arrPer[0] >= 0.85 && result.arrMin[0] < 200
                            || (result.arrMode[0] == Rect.OcrMode.Bushou && result.arrMode[1] == Rect.OcrMode.Bushou
                            && result.arrName[0].Substring(0, 2) == result.arrName[1].Substring(0, 2))
                            )
                        {
                            //确认识别
                            if (result.arrMode[0] == Rect.OcrMode.Bushou)
                            {
                                //部首
                                arrRemove[i] = 2;
                                //MessageBox.Show("【" + i + "】【部首】" + result.DebugResult());
                            }
                            else
                            {
                                //字母数字符号
                                arrRemove[i] = 3;
                                //MessageBox.Show("【" + i + "】【非部首】" + result.DebugResult());
                            }
                            rect.result = result;
                        }
                        else
                        {
                            //MessageBox.Show("【" + i + "】【其他】" + result.DebugResult());
                        }
                    }
                }
            }
            //MessageBox.Show("11");
            //处理所有的边旁部首后合并【第一次合并】
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

                        if (sLR == "左")
                        {
                            int iNext = i + 1;//先往右合并
                            while (iNext < arrRectAll.Length)
                            {
                                if (arrRemove[iNext] == 0 || arrRemove[iNext] == 2)
                                {
                                    Rect r = arrRectAll[iNext];
                                    //元素合并后小于 110%
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
                        else if (sLR == "右")
                        {
                            int iNext = i - 1;//先往右合并
                            while (iNext >= 0)
                            {
                                if (arrRemove[iNext] == 0 || arrRemove[iNext] == 2)
                                {
                                    Rect r = arrRectAll[iNext];
                                    //元素合并后小于 110%
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
                        //恢复非部首
                        if (ishb)
                        {
                            isHB = true;
                            if (rect.width <= iH_07)
                            {
                                //再识别一次
                                rect.iOcrMode = Rect.OcrMode.Bushou;
                                OcrResult result = ocrWord.OcrRect(p, rect, i, null, "小区域：(" + (i + 1) + "/" + arrRectAll.Length + ")行数：(" + (Index + 1) + "/" + Count + ")", false);
                                rect.iOcrMode = Rect.OcrMode.Chinese;
                                //createBmp(result, i);
                                //MessageBox.Show(i + " " + rect.DebugString());
                                //MessageBox.Show(rect.width + "  " + result.DebugResult() + " ");
                                if (result.arrPer[0] >= 0.85 && result.arrMin[0] < 200)
                                {
                                    //部首
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
            //处理剩余无法合并处理的边旁部首后合并 或识别成字母数字字符【第二次合并】
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

                        if (sLR == "左")
                        {
                            int iNext = i + 1;//先往右合并
                            while (iNext < arrRectAll.Length)
                            {
                                if (arrRemove[iNext] == 3)
                                {
                                    Rect r = arrRectAll[iNext];
                                    //元素合并后小于 110%
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
                        else if (sLR == "右")
                        {
                            int iNext = i - 1;//先往右合并
                            while (iNext >= 0)
                            {
                                if (arrRemove[iNext] == 3)
                                {
                                    Rect r = arrRectAll[iNext];
                                    //元素合并后小于 110%
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
                            //MessageBox.Show("小区域：【" + i + "】 缺失部首，行数：(" + (Index + 1) + "/" + Count + ")");
                        }
                        //恢复非部首
                        if ((double)rect.width >= iH_085)
                        {
                            arrRemove[i] = 0;
                            rect.result = null;
                        }
                    }
                }
            }

            //实在无法处理的回复非部首  或特殊化处理
            for (int i = 0; i < arrRectAll.Length; i++)
            {
                if (arrRemove[i] == 2)
                {
                    //特殊化门左
                    Rect rect = arrRectAll[i];
                    //MessageBox.Show(rect.width + " " + height + " " + rect.result.sResultWord);
                    if (rect.result.sResultWord.IndexOf("门左") > -1 || rect.result.sResultWord.IndexOf("人左") > -1)
                    {
                        int iNext = i + 1;//先往右合并
                        while (iNext < arrRectAll.Length)
                        {
                            if (arrRemove[iNext] != 1)
                            {
                                Rect r = arrRectAll[iNext];
                                //元素合并后小于 110%
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
                    else if (rect.result.sResultWord.IndexOf("水左") > -1)
                    {
                        int iNext = i + 1;//先往右合并
                        while (iNext < arrRectAll.Length)
                        {
                            if (arrRemove[iNext] != 1)
                            {
                                Rect r = arrRectAll[iNext];
                                //元素合并后小于 110%
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
                    else if (rect.result.sResultWord.IndexOf("小左") > -1)
                    {
                        int iNext = i + 1;//先往右合并
                        while (iNext < arrRectAll.Length)
                        {
                            if (arrRemove[iNext] != 1)
                            {
                                Rect r = arrRectAll[iNext];
                                //元素合并后小于 110%
                                if (r.r - rect.l < iH_105)
                                {
                                    //MessageBox.Show(r.width + " " + height + " " + r.result.DebugResult());
                                    if ("、".IndexOf(r.result.sResultWord) > -1)
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
            ////距离最近法合并 (较大区域）
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
                        //宽度小于 70% 且高度大于  50% 的元素 右侧 有一个元素 宽度小于 80% 高度大于 50% 且不能与再右边一个元素合并的话
                        ////////////////////////////////////////////////////
                        if (rect.width <= iH_08 && rect.height >= iH_04)
                        {
                            int iNext = i + 1;
                            //先往右合并
                            while (iNext < arrRectAll.Length)
                            {
                                if (arrRemove[iNext] == 0)
                                {
                                    Rect r = arrRectAll[iNext];
                                    //元素合并后小于 105%
                                    if (r.r - rect.l <= iH_100)
                                    {
                                        //再检查再右边一个元素是否能合并且更近
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
                            //再往左合并
                            while (iNext >= 0)
                            {
                                if (arrRemove[iNext] == 0)
                                {
                                    Rect r = arrRectAll[iNext];
                                    //元素合并后小于 105%
                                    if (rect.r - r.l <= iH_100)
                                    {
                                        //再检查再右边一个元素是否能合并且更近
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


            //距离最近法合并 (较小区域）
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
                        //宽度小于 80% 且高度大于  50% 的元素 右侧 有一个元素 宽度小于 80% 高度大于 50% 且不能与再右边一个元素合并的话
                        ////////////////////////////////////////////////////
                        if (rect.width <= iH_03)
                        {
                            int iNext = i + 1;
                            //先往右合并
                            while (iNext < arrRectAll.Length)
                            {
                                if (arrRemove[iNext] == 0)
                                {
                                    Rect r = arrRectAll[iNext];
                                    //元素合并后小于 105%
                                    if (r.r - rect.l <= iH_100 && ((r.width > iH_07 && r.height > iH_07) || (r.width <= iH_04 && r.height <= iH_04)))
                                    {
                                        //再检查再右边一个元素是否能合并且更近
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
                            //再往左合并
                            while (iNext >= 0)
                            {
                                if (arrRemove[iNext] == 0)
                                {
                                    Rect r = arrRectAll[iNext];
                                    //元素合并后小于 105%
                                    if (rect.r - r.l <= iH_100 && ((r.width > iH_07 && r.height > iH_07) || (r.width <= iH_04 && r.height <= iH_04)))
                                    {
                                        //再检查再右边一个元素是否能合并且更近
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
            ////大于正常比文字与左右合并
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
                        //宽度小于 70% 且高度大于  50% 的元素 右侧 有一个元素 宽度小于 80% 高度大于 50% 且不能与再右边一个元素合并的话
                        ////////////////////////////////////////////////////
                        if (rect.width > iH_11 && rect.height > iH_07)
                        {
                            int iNext = i + 1;
                            //先往右合并
                            while (iNext < arrRectAll.Length)
                            {
                                if (arrRemove[iNext] == 0)
                                {
                                    Rect r = arrRectAll[iNext];
                                    //元素合并后小于 105%
                                    if (r.r - rect.l <= iH_100 * 2)
                                    {
                                        //再检查再右边一个元素是否能合并且更近
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
                            //再往左合并
                            while (iNext >= 0)
                            {
                                if (arrRemove[iNext] == 0)
                                {
                                    Rect r = arrRectAll[iNext];
                                    //元素合并后小于 105%
                                    if (rect.r - r.l <= iH_100 * 2)
                                    {
                                        //再检查再右边一个元素是否能合并且更近
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
                    //标点符号合并  标点符号 小于 40% 宽度高度
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
                    //数字或字母或符号   数字或字母或符号 小等于 50% 宽度高度
                    ////////////////////////////////////////////////////
                    else if (rect.height >= iH_05 && rect.width <= iH_05)
                    {
                        rect.iOcrMode = Rect.OcrMode.Code_Int_English;
                    }
                    ////////////////////////////////////////////////////
                    //数字或字母或符号   数字或字母或符号 小于 70% 宽度高度
                    ////////////////////////////////////////////////////
                    //else if (rect.height > iH_07 && rect.width < iH_07)
                    //{
                    //    rect.iOcrMode = Rect.OcrMode.Code_Int_English_ChineseOther;
                    //}
                }
            }

            ////////////////////////////////////////////////////
            //分割多倍区域
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
                    //标点符号合并  标点符号 小于 40% 宽度高度
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
            ////是英文先确定是否是大写的
            //////////////////////////////////////////////////////
            //if (ocr_mode == Rect.OcrMode.Int_English || ocr_mode == Rect.OcrMode.English || ocr_mode == Rect.OcrMode.Code_Int_English)
            //{
            //}
            ////////////////////////////////////////////////////
            //分割多倍区域
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
        
        //创建最终合并区域
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
                    //开始增加新区域
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
    /// 检查左边是否有合并且返回距离是多少     -1 时没有合并 大于等于0时为距离
    /// </summary>
    int chechHebingJuLeft(Rect rect, int index)
    {
        int iJu = -1;
        int iNext = index - 1;
        //先往右合并
        while (iNext >= 0)
        {
            if (arrRemove[iNext] == 0)
            {
                Rect r = arrRectAll[iNext];
                //宽度小于 80% 高度大于 50%
                if (rect.width < iH_08 && rect.height > iH_05)
                {
                    //元素合并后小于 105%
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
    /// 检查右边是否有合并且返回距离是多少     -1 时没有合并 大于等于0时为距离
    /// </summary>
    int chechHebingJuRight(Rect rect, int index)
    {
        int iJu = -1;
        int iNext = index + 1;
        //先往右合并
        while (iNext < arrRectAll.Length)
        {
            if (arrRemove[iNext] == 0)
            {
                Rect r = arrRectAll[iNext];
                //宽度小于 80% 高度大于 50%
                if (rect.width < iH_08 && rect.height > iH_05)
                {
                    //元素合并后小于 105%
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
    /// <param name="alpha">0-1 透明度 默认1</param>
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
    /// 用红框 画出区域
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
    /// 画出区域 后的恢复
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
    /// 按 X大小排序 排序
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


            //依名稱排序    
            return xInfo.l.CompareTo(yInfo.l);//遞增    
            //return yInfo.FullName.CompareTo(xInfo.FullName);//遞減  
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
            return "左：" + X + " 右：" + Y + "上：" + Width + " 下：" + Height;
        }
    }
}

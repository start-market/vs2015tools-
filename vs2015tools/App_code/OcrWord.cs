using System;
using System.Collections;
using System.Text;
using System.Windows.Forms;

public class OcrWords
{
    /// <summary>所有的行  内存 OcrResult[]</summary>
    public ArrayList AL_LineRow = new ArrayList();
    /// <summary>所有文字的字符串集合</summary>
    public string sALL_words = "";


    public int iWidth = 0;
    public int iHeight = 0;
    /// <summary>左,上,宽,高</summary>
    public string sRectString = "";

    public OcrWords()
    {

    }
    /// <summary>
    /// 创建一个新行用于 创建一行OcrResult[]
    /// </summary>
    /// <param name="icount">OcrResult[icount] 中的数量</param>
    /// <returns>返回创建的行号</returns>
    public int CreateLineRow(int icount)
    {
        OcrResult[] Result = new OcrResult[icount];
        AL_LineRow.Add(Result);
        return AL_LineRow.Count - 1;
    }
    /// <summary>
    /// 向行内添加内容结果result
    /// </summary>
    /// <param name="iRow">加入的行号</param>
    /// <param name="iIndex">行中的序号</param>
    /// <param name="result">结果</param>
    public void AddResult(int iRow, int iIndex, OcrResult result)
    {
        if (iRow < AL_LineRow.Count)
        {
            OcrResult[] Result = (OcrResult[])AL_LineRow[iRow];
            Result[iIndex] = result;
            //MessageBox.Show("aaa" + result.DebugResult());
            sALL_words += result.getResult();
            //System.Windows.Forms.MessageBox.Show("aaa");
        }
    }
    /// <summary>
    /// 返回信息格式 字,最小值,相似度,总点数;字,最小值,相似度,总点数\r\n
    /// </summary>
    /// <returns></returns>
    public string getResultString()
    {
        StringBuilder str = new StringBuilder();

        for (int i = 0; i < AL_LineRow.Count; i++)
        {
            OcrResult[] Result = (OcrResult[])AL_LineRow[i];
            StringBuilder words = new StringBuilder();

            for (int j = 0; j < Result.Length; j++)
            {
                if (Result[j] != null)
                {
                    if (words.Length != 0)
                    {
                        words.Append("><");
                    }
                    words.Append(Result[j].getResultString());
                }
            }

            if (str.Length != 0)
            {
                str.Append("<br/>");
            }
            str.Append(words.ToString());
        }

        return str.ToString();
    }
    /// <summary>
    /// 返回格式  上,右,下,左\r\n上,右,下,左
    /// </summary>
    /// <returns></returns>
    public string getRectString()
    {
        StringBuilder str = new StringBuilder();

        for (int i = 0; i < AL_LineRow.Count; i++)
        {
            OcrResult[] Result = (OcrResult[])AL_LineRow[i];
            StringBuilder words = new StringBuilder();

            for (int j = 0; j < Result.Length; j++)
            {
                if (Result[j] != null)
                {
                    if (words.Length != 0)
                    {
                        words.Append(";");
                    }
                    words.Append(Result[j].sRectString);
                }
            }

            if (str.Length != 0)
            {
                str.Append("<br/>");
            }
            str.Append(words.ToString());
        }

        return str.ToString();
    }
}

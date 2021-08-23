using System;
using System.Collections;
using System.Text;
using System.Windows.Forms;

public class OcrWords
{
    /// <summary>���е���  �ڴ� OcrResult[]</summary>
    public ArrayList AL_LineRow = new ArrayList();
    /// <summary>�������ֵ��ַ�������</summary>
    public string sALL_words = "";


    public int iWidth = 0;
    public int iHeight = 0;
    /// <summary>��,��,��,��</summary>
    public string sRectString = "";

    public OcrWords()
    {

    }
    /// <summary>
    /// ����һ���������� ����һ��OcrResult[]
    /// </summary>
    /// <param name="icount">OcrResult[icount] �е�����</param>
    /// <returns>���ش������к�</returns>
    public int CreateLineRow(int icount)
    {
        OcrResult[] Result = new OcrResult[icount];
        AL_LineRow.Add(Result);
        return AL_LineRow.Count - 1;
    }
    /// <summary>
    /// ������������ݽ��result
    /// </summary>
    /// <param name="iRow">������к�</param>
    /// <param name="iIndex">���е����</param>
    /// <param name="result">���</param>
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
    /// ������Ϣ��ʽ ��,��Сֵ,���ƶ�,�ܵ���;��,��Сֵ,���ƶ�,�ܵ���\r\n
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
    /// ���ظ�ʽ  ��,��,��,��\r\n��,��,��,��
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

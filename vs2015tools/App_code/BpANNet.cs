using System;
using System.IO;
using System.Text;
using System.Collections;

/// <summary>
/// BpNet ��ժҪ˵����
/// </summary>
public class BpNet
{
    /// <summary>����ڵ���</summary>
    public int inNum;
    /// <summary>����ڵ���</summary>
    int hideNum;//
    /// <summary>�����ڵ���</summary>
    public int outNum;//
    /// <summary>��������</summary>
    public int sampleNum;//

    Random R;
    /// <summary>����ڵ����������</summary>
    double[] x;//
    /// <summary>����ڵ�����</summary>
    double[] x1;//
    /// <summary>����ڵ�����</summary>
    public double[] x2;//

    /// <summary>����ڵ�����</summary>
    public ArrayList AL_x2;

    /// <summary>���������</summary>
    double[] o1;//
    /// <summary>����������</summary>
    double[] o2;//
    /// <summary>Ȩֵ����w</summary>
    public double[,] w;//
    /// <summary>Ȩֵ����V</summary>
    public double[,] v;//
    /// <summary>Ȩֵ����w</summary>
    public double[,] dw;//
    /// <summary>Ȩֵ����V</summary>
    public double[,] dv;//

    /// <summary>ѧϰ��</summary>
    public double rate;//
    /// <summary>������ֵ����</summary>
    public double[] b1;//
    /// <summary>�������ֵ����</summary>
    public double[] b2;//
    /// <summary>������ֵ����</summary>
    public double[] db1;//
    /// <summary>�������ֵ����</summary>
    public double[] db2;//

    /// <summary>���������</summary>
    double[] pp;//
    /// <summary>��������</summary>
    double[] qq;//
    /// <summary>�����Ľ�ʦ����</summary>
    double[] yd;//
    /// <summary>�������</summary>
    public double e;//
    /// <summary>��һ������ϵ��</summary>
    public double in_rate;//

    public int computeHideNum(int m, int n)
    {
        double s = Math.Sqrt(0.43 * m * n + 0.12 * n * n + 2.54 * m + 0.77 * n + 0.35) + 0.51;
        int ss = Convert.ToInt32(s);
        return ((s - (double)ss) > 0.5) ? ss + 1 : ss;

    }

    public BpNet(double[,] p, double[,] t)
    {

        // ���캯���߼�
        R = new Random(32); //����һ��������ʹ������α���������ͬ

        this.inNum = p.GetLength(1); //����ڶ�ά��СΪ ����ڵ���
        this.outNum = t.GetLength(1); //����ڵ���
        //this.hideNum = computeHideNum(inNum, outNum); //���ؽڵ�������֪��ԭ��
        this.hideNum=20;
        this.sampleNum = p.GetLength(0); //�����һά��С Ϊ

        //Console.WriteLine("����ڵ���Ŀ�� " + inNum);
        //Console.WriteLine("����ڵ���Ŀ��" + hideNum);
        //Console.WriteLine("�����ڵ���Ŀ��" + outNum);

        //Console.ReadLine();

        x = new double[inNum];
        x1 = new double[hideNum];
        x2 = new double[outNum];

        o1 = new double[hideNum];
        o2 = new double[outNum];

        w = new double[inNum, hideNum];
        v = new double[hideNum, outNum];
        dw = new double[inNum, hideNum];
        dv = new double[hideNum, outNum];

        b1 = new double[hideNum];
        b2 = new double[outNum];
        db1 = new double[hideNum];
        db2 = new double[outNum];

        pp = new double[hideNum];
        qq = new double[outNum];
        yd = new double[outNum];

        //��ʼ��w
        for (int i = 0; i < inNum; i++)
        {
            for (int j = 0; j < hideNum; j++)
            {
                w[i, j] = Math.Abs(R.NextDouble() * 2 - 1.0) / 2;

            }
        }

        //b1 = new double[] { 0.1, 0.3, 0.2 };
        //w = new double[,] { { 0.5, 0.3, 0.6 }, { 0.4, 0.5, 0.6 }, { 0.2, 0.4, 0.1 }, { 0.2, 0.3, 0.6 } };


        //��ʼ��v
        for (int i = 0; i < hideNum; i++)
        {
            for (int j = 0; j < outNum; j++)
            {
                v[i, j] = Math.Abs(R.NextDouble() * 2 - 1.0) / 2;
            }
        }

        //v = new double[,] { { 0.5, 0.3 }, { 0.4, 0.2 }, { 0.6, 0.8 } };
        //b2 = new double[] { 0.1, 0.3 };

        rate = 0.5;
        e = 0.0;
        in_rate = 1.0;
    }

    //ѵ������
    public void train(double[,] p, double[,] t)
    {
        e = 0.0;
        //��p��t�е����ֵ
        double pMax = 0.0;
        for (int isamp = 0; isamp < sampleNum; isamp++)
        {
            for (int i = 0; i < inNum; i++)
            {
                if (Math.Abs(p[isamp, i]) > pMax)
                {
                    pMax = Math.Abs(p[isamp, i]);
                }
            }

            for (int j = 0; j < outNum; j++)
            {
                if (Math.Abs(t[isamp, j]) > pMax)
                {
                    pMax = Math.Abs(t[isamp, j]);
                }
            }

            in_rate = pMax;
        }//end isamp

        AL_x2 = new ArrayList();

        for (int isamp = 0; isamp < sampleNum; isamp++)
        {
            //���ݹ�һ��
            for (int i = 0; i < inNum; i++)
            {
                x[i] = p[isamp, i];
            }
            for (int i = 0; i < outNum; i++)
            {
                yd[i] = t[isamp, i];
            }

            //�����������������
            for (int j = 0; j < hideNum; j++)
            {
                o1[j] = 0.0;
                for (int i = 0; i < inNum; i++)
                {
                    o1[j] += w[i, j] * x[i];
                }
                x1[j] = 1.0 / (1.0 + Math.Exp(-o1[j] - b1[j]));
            }

            //������������������
            for (int k = 0; k < outNum; k++)
            {
                o2[k] = 0.0;
                for (int j = 0; j < hideNum; j++)
                {
                    o2[k] += v[j, k] * x1[j];
                }
                x2[k] = 1.0 / (1.0 + Math.Exp(-o2[k] - b2[k]));
            }
            
            AL_x2.Add(x2.Clone());

            string s1 = "";

            //������������;�����

            for (int k = 0; k < outNum; k++)
            {
                qq[k] = (yd[k] - x2[k]) * x2[k] * (1.0 - x2[k]);
                e += (yd[k] - x2[k]) * (yd[k] - x2[k]);
                //����V
                for (int j = 0; j < hideNum; j++)
                {
                    v[j, k] += rate * qq[k] * x1[j];
                }
                string s = "";
            }

            //�����������

            for (int j = 0; j < hideNum; j++)
            {
                pp[j] = 0.0;
                for (int k = 0; k < outNum; k++)
                {
                    pp[j] += qq[k] * v[j, k];
                }
                pp[j] = pp[j] * x1[j] * (1 - x1[j]);

                //����W

                for (int i = 0; i < inNum; i++)
                {
                    double aa = rate * pp[j] * x[i];
                    w[i, j] += rate * pp[j] * x[i];
                }
                string s = "";
            }

            //����b2
            for (int k = 0; k < outNum; k++)
            {
                b2[k] += rate * qq[k];
            }

            //����b1
            for (int j = 0; j < hideNum; j++)
            {
                b1[j] += rate * pp[j];
            }

        }//end isamp

        string ss = "";
        //e = Math.Sqrt(e);
        //      adjustWV(w,dw);
        //      adjustWV(v,dv);


    }//end train

    public void adjustWV(double[,] w, double[,] dw)
    {
        for (int i = 0; i < w.GetLength(0); i++)
        {
            for (int j = 0; j < w.GetLength(1); j++)
            {
                w[i, j] += dw[i, j];
            }
        }

    }

    public void adjustWV(double[] w, double[] dw)
    {
        for (int i = 0; i < w.Length; i++)
        {

            w[i] += dw[i];

        }

    }


    public BpNet() //���溯�� �õĹ��캯��
    {

    }

    //���ݷ��溯��
    public double[] sim(double[] psim) //in_rate inNum HideNum outNum 
    {
        for (int i = 0; i < inNum; i++)
            x[i] = psim[i] / in_rate;

        for (int j = 0; j < hideNum; j++)
        {
            o1[j] = 0.0;
            for (int i = 0; i < inNum; i++)
                o1[j] = o1[j] + w[i, j] * x[i];
            x1[j] = 1.0 / (1.0 + Math.Exp(-o1[j] - b1[j]));
        }
        for (int k = 0; k < outNum; k++)
        {
            o2[k] = 0.0;
            for (int j = 0; j < hideNum; j++)
                o2[k] = o2[k] + v[j, k] * x1[j];
            x2[k] = 1.0 / (1.0 + Math.Exp(-o2[k] - b2[k]));

            x2[k] = in_rate * x2[k];

        }

        return x2;
    } //end sim

    //�������w,v
    public void saveMatrix(double[,] w, string filename)
    {
        StreamWriter sw = new StreamWriter(filename);
        for (int i = 0; i < w.GetLength(0); i++)
        {
            for (int j = 0; j < w.GetLength(1); j++)
            {
                sw.Write(w[i, j] + " ");
            }
            sw.WriteLine();
        }
        sw.Close();

    }

    //�������b1,b2
    public void saveMatrix(double[] b, string filename)
    {
        StreamWriter sw = new StreamWriter(filename);
        for (int i = 0; i < b.Length; i++)
        {
            sw.Write(b[i] + " ");
        }
        sw.Close();
    }

    //������� in_rate inNum HideNum outNum 
    public void saveParas(string filename)
    {
        try
        {
            StreamWriter sw = new StreamWriter(filename);
            string str = inNum.ToString() + " "
                    + hideNum.ToString() + " "
                    + outNum.ToString() + " "
                    + in_rate.ToString();
            sw.WriteLine(str);
            sw.Close();
        }
        catch (Exception e)
        {
            // Let the user know what went wrong.
            Console.WriteLine("The file could not be read:");
            Console.WriteLine(e.Message);
        }
    }

    //���ز��� in_rate inNum HideNum outNum, tjt Ԥ�������� 
    public void readParas(string filename)
    {
        StreamReader sr;
        try
        {
            sr = new StreamReader(filename);
            String line;
            if ((line = sr.ReadLine()) != null)
            {
                string[] strArr = line.Split(' ');
                this.inNum = Convert.ToInt32(strArr[0]);
                this.hideNum = Convert.ToInt32(strArr[1]);
                this.outNum = Convert.ToInt32(strArr[2]);
                this.in_rate = Convert.ToDouble(strArr[3]);
            }
            sr.Close();

        }
        catch (Exception e)
        {
            // Let the user know what went wrong.
            Console.WriteLine("The file could not be read:");
            Console.WriteLine(e.Message);
        }
    }

    public void initial() // ����һЩ�м����� tjt Ԥ��������
    {
        x = new double[inNum];
        x1 = new double[hideNum];
        x2 = new double[outNum];

        o1 = new double[hideNum];
        o2 = new double[outNum];

        w = new double[inNum, hideNum];
        v = new double[hideNum, outNum];
        dw = new double[inNum, hideNum];
        dv = new double[hideNum, outNum];

        b1 = new double[hideNum];
        b2 = new double[outNum];
        db1 = new double[hideNum];
        db2 = new double[outNum];

        pp = new double[hideNum];
        qq = new double[outNum];
        yd = new double[outNum];
    }

    //��ȡ����W,V
    public void readMatrixW(double[,] w, string filename)
    {

        StreamReader sr;
        try
        {

            sr = new StreamReader(filename);

            String line;
            int i = 0;

            while ((line = sr.ReadLine()) != null)
            {

                string[] s1 = line.Trim().Split(' ');
                for (int j = 0; j < s1.Length; j++)
                {
                    w[i, j] = Convert.ToDouble(s1[j]);
                }
                i++;
            }
            sr.Close();

        }
        catch (Exception e)
        {
            // Let the user know what went wrong.
            Console.WriteLine("The file could not be read:");
            Console.WriteLine(e.Message);
        }
    }


    //��ȡ����b1,b2
    public void readMatrixB(double[] b, string filename)
    {

        StreamReader sr;
        try
        {
            sr = new StreamReader(filename);
            String line;
            while ((line = sr.ReadLine()) != null)
            {
                int i = 0;
                string[] s1 = line.Trim().Split(' ');
                for (int j = 0; j < s1.Length; j++)
                {
                    b[i] = Convert.ToDouble(s1[j]);
                    i++;
                }
            }
            sr.Close();

        }
        catch (Exception e)
        {
            // Let the user know what went wrong.
            Console.WriteLine("The file could not be read:");
            Console.WriteLine(e.Message);
        }

    }

}//end bpnet
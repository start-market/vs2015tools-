using System;
using System.IO;
using System.Text;
using System.Collections;

/// <summary>
/// BpNet 的摘要说明。
/// </summary>
public class BpNet
{
    /// <summary>输入节点数</summary>
    public int inNum;
    /// <summary>隐层节点数</summary>
    int hideNum;//
    /// <summary>输出层节点数</summary>
    public int outNum;//
    /// <summary>样本总数</summary>
    public int sampleNum;//

    Random R;
    /// <summary>输入节点的输入数据</summary>
    double[] x;//
    /// <summary>隐层节点的输出</summary>
    double[] x1;//
    /// <summary>输出节点的输出</summary>
    public double[] x2;//

    /// <summary>输出节点的输出</summary>
    public ArrayList AL_x2;

    /// <summary>隐层的输入</summary>
    double[] o1;//
    /// <summary>输出层的输入</summary>
    double[] o2;//
    /// <summary>权值矩阵w</summary>
    public double[,] w;//
    /// <summary>权值矩阵V</summary>
    public double[,] v;//
    /// <summary>权值矩阵w</summary>
    public double[,] dw;//
    /// <summary>权值矩阵V</summary>
    public double[,] dv;//

    /// <summary>学习率</summary>
    public double rate;//
    /// <summary>隐层阈值矩阵</summary>
    public double[] b1;//
    /// <summary>输出层阈值矩阵</summary>
    public double[] b2;//
    /// <summary>隐层阈值矩阵</summary>
    public double[] db1;//
    /// <summary>输出层阈值矩阵</summary>
    public double[] db2;//

    /// <summary>输出层的误差</summary>
    double[] pp;//
    /// <summary>隐层的误差</summary>
    double[] qq;//
    /// <summary>输出层的教师数据</summary>
    double[] yd;//
    /// <summary>均方误差</summary>
    public double e;//
    /// <summary>归一化比例系数</summary>
    public double in_rate;//

    public int computeHideNum(int m, int n)
    {
        double s = Math.Sqrt(0.43 * m * n + 0.12 * n * n + 2.54 * m + 0.77 * n + 0.35) + 0.51;
        int ss = Convert.ToInt32(s);
        return ((s - (double)ss) > 0.5) ? ss + 1 : ss;

    }

    public BpNet(double[,] p, double[,] t)
    {

        // 构造函数逻辑
        R = new Random(32); //加了一个参数，使产生的伪随机序列相同

        this.inNum = p.GetLength(1); //数组第二维大小为 输入节点数
        this.outNum = t.GetLength(1); //输出节点数
        //this.hideNum = computeHideNum(inNum, outNum); //隐藏节点数，不知其原理
        this.hideNum=20;
        this.sampleNum = p.GetLength(0); //数组第一维大小 为

        //Console.WriteLine("输入节点数目： " + inNum);
        //Console.WriteLine("隐层节点数目：" + hideNum);
        //Console.WriteLine("输出层节点数目：" + outNum);

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

        //初始化w
        for (int i = 0; i < inNum; i++)
        {
            for (int j = 0; j < hideNum; j++)
            {
                w[i, j] = Math.Abs(R.NextDouble() * 2 - 1.0) / 2;

            }
        }

        //b1 = new double[] { 0.1, 0.3, 0.2 };
        //w = new double[,] { { 0.5, 0.3, 0.6 }, { 0.4, 0.5, 0.6 }, { 0.2, 0.4, 0.1 }, { 0.2, 0.3, 0.6 } };


        //初始化v
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

    //训练函数
    public void train(double[,] p, double[,] t)
    {
        e = 0.0;
        //求p，t中的最大值
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
            //数据归一化
            for (int i = 0; i < inNum; i++)
            {
                x[i] = p[isamp, i];
            }
            for (int i = 0; i < outNum; i++)
            {
                yd[i] = t[isamp, i];
            }

            //计算隐层的输入和输出
            for (int j = 0; j < hideNum; j++)
            {
                o1[j] = 0.0;
                for (int i = 0; i < inNum; i++)
                {
                    o1[j] += w[i, j] * x[i];
                }
                x1[j] = 1.0 / (1.0 + Math.Exp(-o1[j] - b1[j]));
            }

            //计算输出层的输入和输出
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

            //计算输出层误差和均方差

            for (int k = 0; k < outNum; k++)
            {
                qq[k] = (yd[k] - x2[k]) * x2[k] * (1.0 - x2[k]);
                e += (yd[k] - x2[k]) * (yd[k] - x2[k]);
                //更新V
                for (int j = 0; j < hideNum; j++)
                {
                    v[j, k] += rate * qq[k] * x1[j];
                }
                string s = "";
            }

            //计算隐层误差

            for (int j = 0; j < hideNum; j++)
            {
                pp[j] = 0.0;
                for (int k = 0; k < outNum; k++)
                {
                    pp[j] += qq[k] * v[j, k];
                }
                pp[j] = pp[j] * x1[j] * (1 - x1[j]);

                //更新W

                for (int i = 0; i < inNum; i++)
                {
                    double aa = rate * pp[j] * x[i];
                    w[i, j] += rate * pp[j] * x[i];
                }
                string s = "";
            }

            //更新b2
            for (int k = 0; k < outNum; k++)
            {
                b2[k] += rate * qq[k];
            }

            //更新b1
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


    public BpNet() //仿真函数 用的构造函数
    {

    }

    //数据仿真函数
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

    //保存矩阵w,v
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

    //保存矩阵b1,b2
    public void saveMatrix(double[] b, string filename)
    {
        StreamWriter sw = new StreamWriter(filename);
        for (int i = 0; i < b.Length; i++)
        {
            sw.Write(b[i] + " ");
        }
        sw.Close();
    }

    //保存参数 in_rate inNum HideNum outNum 
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

    //读回参数 in_rate inNum HideNum outNum, tjt 预测新数据 
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

    public void initial() // 建立一些中间数组 tjt 预测新数据
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

    //读取矩阵W,V
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


    //读取矩阵b1,b2
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
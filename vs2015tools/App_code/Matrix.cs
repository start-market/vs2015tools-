using System;
using System.Collections.Generic;
using System.Text;

public class Matrix
{

    public double[,] Mat;
    private long _m, _n;
    public long M
    {
        get
        {
            return _m;
        }
        private set
        {
            _m = value;
        }
    }
    public long N
    {
        get
        {
            return _n;
        }
        private set
        {
            _n = value;
        }
    }
    protected static Random rand = new Random((int)DateTime.Now.Ticks);

    public Matrix(long m, long n, bool isRandValue)
    {
        _m = m;
        _n = n;

        Mat = new double[m, n];
        for (int i = 0; i < m; i++)
            for (int j = 0; j < n; j++)
                if (isRandValue)
                    Mat[i, j] = rand.NextDouble();
                else Mat[i, j] = 0;
    }
    public Matrix(long m, long n)
    {
        bool isRandValue = false;
        _m = m;
        _n = n;

        Mat = new double[m, n];
        for (int i = 0; i < m; i++)
            for (int j = 0; j < n; j++)
                if (isRandValue)
                    Mat[i, j] = rand.NextDouble();
                else Mat[i, j] = 0;
    }
    public Matrix(double[,] m)
    {
        _m = m.GetLongLength(0);
        _n = m.GetLongLength(1);
        Mat = m;
    }


    public static Matrix operator +(Matrix M1, Matrix M2)
    {
        if (M1.M != M2.M
         || M1.N != M1.N)
            throw new Exception("矩阵不符合运算条件，2个矩阵必须完全一样的行和列");

        Matrix result = new Matrix(M1.M, M1.N);
        for (int i = 0; i < M1.M; i++)
            for (int j = 0; j < M1.N; j++)
            {
                result.Mat[i, j] = M1.Mat[i, j] + M2.Mat[i, j];
            }
        return result;
    }
    public static Matrix operator -(Matrix M1, Matrix M2)
    {
        if (M1.M != M2.M
         || M1.N != M1.N)
            throw new Exception("矩阵不符合运算条件，2个矩阵必须完全一样的行和列");

        Matrix result = new Matrix(M1.M, M1.N);
        for (int i = 0; i < M1.M; i++)
            for (int j = 0; j < M1.N; j++)
            {
                result.Mat[i, j] = M1.Mat[i, j] - M2.Mat[i, j];
            }
        return result;
    }
    public static Matrix operator ^(Matrix M1, Matrix M2)
    {
        if (M1.M != M2.M
         || M1.N != M1.N)
            throw new Exception("矩阵不符合运算条件，2个矩阵必须完全一样的行和列");

        Matrix result = new Matrix(M1.M, M1.N);
        for (int i = 0; i < M1.M; i++)
            for (int j = 0; j < M1.N; j++)
            {
                result.Mat[i, j] = M1.Mat[i, j]* M2.Mat[i, j];
            }
        return result;
    }

    public static Matrix operator *(Matrix M1, Matrix M2)
    {
        long m = M1.Mat.GetLongLength(0);//4   4
        long jW = M1.Mat.GetLongLength(1);//3   4

        long iH = M2.Mat.GetLongLength(0);//3   4
        long n = M2.Mat.GetLongLength(1);//4   1

        if (jW != iH)
            throw new Exception("矩阵不符合运算条件，W的行不等于H的列");
        Matrix result = new Matrix(m, n);

        for (int i = 0; i < m; i++)//W的行数
        {
            for (int j = 0; j < n; j++)//H的列数
            {
                for (int k = 0; k < jW; k++)
                {
                    double a = M2.Mat[k, j];
                    double b = M1.Mat[i, k];
                    double c = a * b;
                    result.Mat[i, j] += M2.Mat[k, j] * M1.Mat[i, k];
                }
            }
        }

        return result;
    }
    public static Matrix operator /(Matrix M1, Matrix M2)
    {
        long m = M1.Mat.GetLongLength(0);//4   4
        long jW = M1.Mat.GetLongLength(1);//3   4

        long iH = M2.Mat.GetLongLength(0);//3   4
        long n = M2.Mat.GetLongLength(1);//4   1

        if (jW != iH)
            throw new Exception("矩阵不符合运算条件，W的行不等于H的列");
        Matrix result = new Matrix(m, n);

        for (int i = 0; i < m; i++)//W的行数
        {
            for (int j = 0; j < n; j++)//H的列数
            {
                for (int k = 0; k < jW; k++)
                {
                    double a = M2.Mat[k, j];
                    double b = M1.Mat[i, k];
                    double c = a * b;
                    result.Mat[i, j] += M2.Mat[k, j] * M1.Mat[i, k];
                }
            }
        }

        return result;
    }
    public static Matrix operator *(Matrix M1, double ratio)
    {
        long m = M1.Mat.GetLongLength(0);
        long n = M1.Mat.GetLongLength(1);
        Matrix result = new Matrix(m, n);
        for (int i = 0; i < m; i++)
            for (int j = 0; j < n; j++)
                result.Mat[i, j] = M1.Mat[i, j] * ratio;
        return result;
    }

    public  Matrix Nonlin()
    {
        Matrix result = new Matrix(M,N);
        for (int i = 0; i < M; i++)
            for (int j = 0; j < N; j++)
                result.Mat[i, j] = Sigmoid(Mat[i, j]);
        return result;
    }
    public Matrix Derivative()
    {
        Matrix result = new Matrix(M, N);
        for (int i = 0; i < M; i++)
            for (int j = 0; j < N; j++)
                result.Mat[i, j] = Derivative(Mat[i, j]);
        return result;
    }
    public  double Sigmoid(double x)
    {
        return (1 / (1 + Math.Exp(-3 * x)));
    }

    //求导
    public  double Derivative(double x)
    {
        return (3 * x * (1 - x));
    }
    public Matrix T
    {
        get
        {
            Matrix result= new Matrix(N, M);

            //新矩阵生成规则： b[i,j]=a[j,i]
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < M; j++)
                {
                    result.Mat[i, j] = this.Mat[j, i];
                }
            }
            return result;
        }
    }
    public override string ToString()
    {
        StringBuilder sbd = new StringBuilder();
        for (int i = 0; i <this.M; i++)
        {
            for (int j = 0; j < this.N; j++)
            {
                sbd.Append(Mat[i, j].ToString("N10"));
                sbd.Append(",");
            }
            sbd.AppendLine();
        }
        return sbd.ToString();
    }
}

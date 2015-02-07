using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onigiri.Algorithm
{
    class ReducedLinearOptimizationKernel:ReducedLinearOptimization
    {
        double[][] kernelMatrix;
        Dictionary<int, double> memo;
        double gamma;
        double degree;
        Func<Data, Data, double> kernel;

        public ReducedLinearOptimizationKernel(int kernelOption,DataSet dataSet, int n, Dictionary<int, int> signMapping,int core,double gamma,double degree)
            : base(dataSet, n, signMapping,core)
        {
            this.gamma = gamma;
            this.degree = degree;
            SetKernelMatrix(kernelOption,dataSet);
        }

        internal double[][] KernelMatrix
        { get { return kernelMatrix; } }

        internal Func<Data, Data, double> Kernel
        { get { return kernel; } }

        private void SetKernelMatrix(int kernelOption,DataSet dataSet)
        {
            kernelMatrix = new double[N][];
            for (int i = 0; i < N; i++)
            {
                kernelMatrix[i] = new double[N];
            }
            memo = new Dictionary<int, double>();
             kernel = SetKernel(kernelOption);
            for (int i = 0; i < N; i++)
            {
                var a = dataSet[i];
                for (int j = i; j < N; j++)
                {
                    kernelMatrix[i][j] = kernelMatrix[j][i] = kernel.Invoke(a, dataSet[j]);
                }
            }
        }

        protected override void SetExtremeBase(double[] extremeBase, int pos, double coeff)
        {
            extremeBase[pos] += coeff;
        }

        protected override double CalcInnerProduct(double[] x, int index)
        {
            double res = 0;
            for (int i = 0; i < N; i++)
            {
                res += x[i] * kernelMatrix[i][index];
            }
            return res;
        }

        protected override double CalcNorm(int index)
        {
            return kernelMatrix[index][index];
        }

        private Func<Data, Data, double> SetKernel(int kernelOption)
        {
            return (x, y) =>
                {
                    double a;
                    double b;
                    double res = 0;
                    memo.Clear();
                    for (int i = 0; i < y.Length; i++)
                    {
                        memo[y.GetKey(i)] = y.GetValue(i);
                    }
                    for (int i = 0; i < x.Length; i++)
                    {
                        if (memo.TryGetValue(x.GetKey(i),out a))
                        {
                            b = x.GetValue(i);
                            switch (kernelOption)
                            {
                                case 1:
                                    a -= b;
                                    res += a * a;
                                    break;
                                case 2:
                                    res +=b* a;
                                    break;
                                default:
                                    throw new ArgumentException("Kernel option " + kernelOption + " is not supporteed.");
                            }

                        }
                    }
                    switch (kernelOption)
                    {
                        case 1:
                            res = Math.Pow(Math.E,-res * gamma);
                            break;
                        case 2:
                            res = Math.Pow(res, degree) * gamma;
                            break;
                        default:
                            throw new ArgumentException("Kernel option " + kernelOption + " is not supporteed.");
                    }
                    return res;
                };
        }


    }
}

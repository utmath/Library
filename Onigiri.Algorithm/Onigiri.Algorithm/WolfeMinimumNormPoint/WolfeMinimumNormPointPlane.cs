//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Onigiri.Algorithm
//{

//    public class WolfeMinimumNormPointPlane
//    {
//        protected const double DefaultAbsEps = 1e-12;
//        protected const double DefaultRelativeEps = 1e-12;
//        protected LinearOptimization LOAlgorithm;
//        protected double[] extremeBase;
//        double[] x;
//        double[] y;
//        protected double[] b;
//        protected double currentNorm;
//        protected ConvexComponents components;  //dummy

        
//        public WolfeMinimumNormPointPlane(LinearOptimization LOAlgorithm, double[][] kernelMatrix = null, double absEps = DefaultAbsEps, double relativeEps = DefaultRelativeEps)
//        {
//            this.AbsEps = absEps;
//            this.RelativeEps = relativeEps;
//            int n = LOAlgorithm.N;
//            N = n;
//            this.LOAlgorithm = LOAlgorithm;
//            extremeBase = new double[n];
//            b = new double[n];
//            x = new double[n];
//            y = Enumerable.Repeat(double.MinValue, N).ToArray();
//        }

//        public long Iteration
//        { get; set; }

//        public long MajorCycle
//        { get; set; }

//        public long MinorCycle
//        { get; set; }

//        double AbsEps
//        {
//            get;
//            set;
//        }

//        double RelativeEps
//        {
//            get;
//            set;
//        }

//        int N
//        {
//            get;
//            set;
//        }

//        public double[] X
//        {
//            get { return x; }
//        }

//        public double[] Lambdas
//        {
//            get
//            {
//                return null;
//            }
//        }

//        public object[] Data
//        {
//            get { return null; }
//        }

//        //System.Diagnostics.Stopwatch sw;
//        //System.IO.StreamWriter s;

//        public virtual double[] CalcMinimumNormPoint()
//        {
//            Initialize();
//            currentNorm = double.MaxValue;
//            while (true)
//            {
//                Iteration++;
//                double nextNorm = CalcInnerProduct(X, X);
//                //s.WriteLine(sw.ElapsedMilliseconds + " " + nextNorm);
//                if (currentNorm <= nextNorm)
//                {
//                    break;
//                }//if
//                currentNorm = nextNorm;
//                var data = LOAlgorithm.CalcLinearMinimizer(x, extremeBase);
//                if (IsMinimizer(currentNorm))
//                {
//                    break;
//                }//if
//                MinorCycle++;
//                PlaneOptimization(extremeBase);
//            }//while
//            //s.Close();
//            return (double[])X.Clone();
//        }

//        protected virtual bool HeuristicStop()
//        {
//            return true;
//        }

//        private void Initialize()
//        {
//            Iteration = MinorCycle = MajorCycle = 0;
//            var data = LOAlgorithm.GetInitialBase(x);
//        }

//        private bool IsMinimizer(double norm)
//        {
//            double innerProduct = CalcInnerProduct(X, extremeBase);
//            if (Math.Abs(norm - innerProduct) <= Math.Max(norm, Math.Abs(innerProduct)) * RelativeEps)
//            {
//                return true;
//            }//if
//            return norm <= innerProduct;
//        }

//        private void PlaneOptimization(double[] extremeBase)
//        {
//            if (y[0]==double.MinValue)
//            {
//                LineOptimization(extremeBase);
//            }
//            else
//            {
//                double a = CalcDiffInnerProduct(extremeBase, x);
//                double b = CalcDiffSquare( extremeBase,x);
//                double c = CalcDiffInnerProduct(extremeBase, x, y);
//                double e = CalcDiffInnerProduct(extremeBase, y);
//                double f = c;
//                double g = CalcDiffSquare(extremeBase, y);
//                double denominator = b * g - c * f;
//                double numeratorX = c * e - a * g;
//                double numeratorY = -e * b + a * f;
//                if (numeratorX<0)
//                {
//                    for (int i = 0; i < N; i++)
//                    {
//                        x[i] = y[i];
//                    }
//                    LineOptimization(extremeBase);
//                }
//                else if (numeratorY<0||Math.Abs(denominator)<AbsEps || Math.Abs(denominator)<Math.Max(Math.Abs(b*g),Math.Abs(c*f))*RelativeEps)
//                {
//                    LineOptimization(extremeBase);
//                }
//                else
//                {
//                    for (int i = 0; i < N; i++)
//                    {
//                        x[i] = extremeBase[i] + (numeratorX * (x[i] - extremeBase[i]) + numeratorY * (y[i] - extremeBase[i])) / denominator;
//                    }
//                }
//            }
//            for (int i = 0; i < N; i++)
//            {
//                y[i] = extremeBase[i];
//            }
//        }

//        private double CalcDiffInnerProduct(double[] extremeBase, double[] x)
//        {
//            double res = 0;
//            for (int i = 0; i < N; i++)
//            {
//                res += extremeBase[i] * (x[i] - extremeBase[i]);
//            }
//            return res;
//        }

//        private double CalcDiffInnerProduct(double[] extremeBase, double[] x, double[] y)
//        {
//            double res = 0;
//            for (int i = 0; i < N; i++)
//            {
//                res += (x[i] - extremeBase[i]) * (y[i] - extremeBase[i]);
//            }
//            return res;
//        }

//        private void LineOptimization(double[] extremeBase)
//        {
//            double numerator = CalcSquare(X) - CalcInnerProduct(extremeBase, X);
//            double denominator = CalcDiffSquare(extremeBase, X);
//            if (denominator > AbsEps)
//            {
//                double alpha = numerator / denominator;
//                alpha = Math.Min(1, alpha);
//                for (int i = 0; i < N; i++)
//                {
//                    x[i] = x[i] + alpha * (extremeBase[i] - x[i]);
//                }
//            }
//        }

//        private double CalcDiffSquare(double[] extremeBase, double[] X)
//        {
//            double res = 0;
//            double tmp;
//            for (int i = 0; i < N; i++)
//            {
//                tmp = extremeBase[i] - X[i];
//                res += tmp * tmp;
//            }
//            return res;
//        }

//        private double CalcSquare(double[] x, int length = -1)
//        {
//            if (length == -1)
//            {
//                length = x.Length;
//            }//if
//            double res = 0;
//            for (int i = 0; i < length; i++)
//            {
//                res += x[i] * x[i];
//            }//for i
//            return res;
//        }

//        private double CalcInnerProduct(double[] a, double[] b)
//        {
//            double res = 0;
//            for (int i = 0; i < N; i++)
//            {
//                res += a[i] * b[i];
//            }
//            return res;
//        }

//    }
//}

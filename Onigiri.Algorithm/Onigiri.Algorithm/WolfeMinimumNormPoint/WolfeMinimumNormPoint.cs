using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onigiri.Algorithm
{

    public class WolfeMinimumNormPoint
    {
        protected const double DefaultAbsEps = 1e-12;
        protected const double DefaultRelativeEps = 1e-10;
        protected ConvexComponents components;
        protected LinearOptimization LOAlgorithm;
        protected double[] extremeBase;
        protected double[] b;
        protected double currentNorm;
        
        public WolfeMinimumNormPoint(LinearOptimization LOAlgorithm,double[][]kernelMatrix=null, double absEps=DefaultAbsEps, double relativeEps=DefaultRelativeEps)
        {
            this.AbsEps = absEps;
            this.RelativeEps = relativeEps;
            int n = LOAlgorithm.N;
            this.LOAlgorithm = LOAlgorithm;
            extremeBase = new double[n];
            b = new double[n];
            //components = new ConvexComponentsImiroved(n, kernelMatrix, absEps, relativeEps);
            //components = new ConvexComponentsCholesky(n, kernelMatrix, absEps, relativeEps);
            components = new ConvexComponentsQR(n, kernelMatrix, absEps, relativeEps);
        }
        
        public long Iteration
        { get; set; }

        public long MinorCycle
        { get { return components.MinorCycle; } }

        double AbsEps
        {
            get;
            set;
        }

        double RelativeEps
        {
            get;
            set;
        }

        public double[] X
        {
            get { return components.X; }
        }

        public double[] Lambdas
        {
            get
            {
                return components.Lambdas;
            }
        }

        public object[] Data
        {
            get { return components.GetData(); }
        }

        //int minus = 0;
        public virtual double[] CalcMinimumNormPoint(long maxReputation = long.MaxValue)
        {
            //var sw = new System.IO.StreamWriter(@"C:\Users\onigiri\Desktop\Norm.txt");
            Initialize();
            currentNorm = double.MaxValue;
            while (Iteration<=maxReputation)
            {
                //minus = X.Count(x => x <= 0);
                Iteration++;
                //MajorCycle++;
                components.SetX();
                //CheckMinusComionets();
                //CheckOrder();

                if (HeuristicStop() || X.Length == 0)
                {
                    break;
                }
                
                double nextNorm = components.CalcSquareKernel(X);
                if (currentNorm <= nextNorm)
                {
                    break;
                }//if
                currentNorm = nextNorm;
                var data = LOAlgorithm.CalcLinearMinimizer(components.X, extremeBase);
                if (IsMinimizer(currentNorm))
                {
                    break;
                }//if
                if (!components.Add(extremeBase, data))
                {
                    break;
                }
            }//while
            //Console.WriteLine(MajorCycle+" "+MinorCycle);
            components.SetXWithoutError();
            //sw.Close();
            //s.Close();
            return (double[])X.Clone();
        }

        private void CheckOrder()
        {
            var order = Enumerable.Range(0, X.Length).ToArray();
            var copy = (double[])X.Clone();
            Array.Sort(copy, order);
            long sum = 0;
            for (int i = 0; i < order.Length; i++)
            {
                sum += order[i];  
            }
        }

        protected virtual bool HeuristicStop()
        {
            return false;
        }

        private void CheckMinusComionets()
        {
            var sw = new System.IO.StreamWriter(@"C:\Users\onigiri\Desktop\norm.txt", true);
            int cnt = 0;
            for (int i = 0; i < X.Length; i++)
            {
                if (X[i] < 0)
                {
                    cnt++;
                    sw.Write(i.ToString() + " ");
                }
            }
            sw.WriteLine();
            sw.WriteLine(cnt);
            sw.Close();
        }

        private bool IsMinimizer(double norm)
        {
            double innerProduct = components.CalcInnerProductKernel(components.X, extremeBase);
            if (Math.Abs(norm - innerProduct) <= Math.Max(norm, Math.Abs(innerProduct)) * RelativeEps)
            {
                return true;
            }//if
            return norm <= innerProduct;
        }

        protected void Initialize()
        {
            Iteration = 0;
            components.Clear();
            var data = LOAlgorithm.GetInitialBase(b);
            components.Add(b, data);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onigiri.Algorithm
{
    public class BruteForceLinearOptimization:LinearOptimization
    {
        double[][] extremeBases;

        public BruteForceLinearOptimization(int n,double[][] extremeBases):base(n)
        {
            this.extremeBases = extremeBases;
        }

        public override object CalcLinearMinimizer(double[] x, double[] extremeBase)
        {
            int minimizer = 0;
            double val = double.MaxValue;
            for (int i = 0; i < extremeBases.Length; i++)
            {
                double cur = 0;
                for (int k = 0; k < N; k++)
                {
                    cur += x[k] * extremeBases[i][k];
                }//for k
                if (cur<val)
                {
                    val = cur;
                    minimizer = i;
                }//if
            }//for i
            for (int i = 0; i < N; i++)
            {
                extremeBase[i] = extremeBases[minimizer][i];
            }//for i
            return null;
        }

        public override object GetInitialBase(double[] b)
        {
            for (int i = 0; i < N; i++)
            {
                b[i] = extremeBases[0][i];
            }//for i
            return null;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onigiri.Algorithm
{
    public abstract class LinearOptimization
    {
        protected LinearOptimization(int n)
        {
            this.N = n;
        }

        public abstract object CalcLinearMinimizer(double[] x, double[] extremeBase);
        public abstract object GetInitialBase(double[] b);

        public int N
        { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onigiri.Algorithm
{
    public class ConvexComponent
    {
        double[] b;
        object data;

        public double Lambda
        {
            get;
            set;
        }

        public double[] B
        {
            get { return b; }
            set { b = value; }
        }

        public object Data
        {
            get { return data; }
            set { data = value; }
        }

        public ConvexComponent(int n)
        {
            this.Lambda = 0;
            this.b = new double[n];
        }

        public virtual void Copy(ConvexComponent other,int n)
        {
            Lambda = 0;
            for (int i = 0; i < n; i++)
            {
                B[i] = other.B[i];
            }//for i
            data = other.Data;
        }


        internal void Set(int lambda, double[] extremeBase, object data)
        {
            Lambda = lambda;
            for (int i = 0; i < extremeBase.Length; i++)
            {
                B[i] = extremeBase[i];
            }//for i
            this.data = data;
        }
    }
}

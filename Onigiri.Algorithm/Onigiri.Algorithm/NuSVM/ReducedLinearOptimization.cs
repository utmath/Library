using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onigiri.Algorithm
{
    class ReducedLinearOptimization:LinearOptimization
    {
        const int Binary = 2;
        DataSet dataSet;
        int[][] indicesWithSign;
        double[][] signedInnerProducts;
        double[] etaList;
        Random rand;

        int Core
        { get; set; }

        double Eta
        { get; set; }

        int K
        { get; set; }

        public int[][] IndicesWithSign
        {
            get { return indicesWithSign; }
        }

        public ReducedLinearOptimization(DataSet dataSet,int n,Dictionary<int,int> signMapping,int core):base(n)
        {
            rand = new Random(0);
            this.dataSet = dataSet;
            this.Core = core;
            indicesWithSign =new int[Binary][];
            int cntZero = 0;
            for (int i = 0; i < dataSet.Samples; i++)
            {
                if (signMapping[dataSet[i].Label]==0)
                {
                    cntZero++;
                }
            }//for i
            indicesWithSign[0] = new int[cntZero];
            indicesWithSign[1] = new int[dataSet.Samples-cntZero];
            var indices = new int[Binary];
            for (int i = 0; i < dataSet.Samples; i++)
            {
                int sign = signMapping[dataSet[i].Label];
                indicesWithSign[sign][indices[sign]++] = i;
            }//for i
            signedInnerProducts = new double[Binary][];
            for (int i = 0; i < Binary; i++)
            {
                signedInnerProducts[i] = new double[indicesWithSign[i].Length];
            }//for i
        }

        public void ResetEta(double eta)
        {
            this.Eta = eta;
            K = (int)Math.Ceiling(1.0 / eta) - 1;
            etaList = new double[K + 1];
            for (int i = 0; i <= K; i++)
            {
                etaList[i] = (i == K ? 1 - K * Eta : Eta);
            }
        }

        public override object CalcLinearMinimizer(double[] x, double[] extremeBase)
        {
            SetInnerProducts(x, Core);
            var res = SetExtremeBase(extremeBase);
            return res;
        }
        
        protected virtual void SetInnerProducts(double[] x,int core)
        {
            for (int i = 0; i < Binary; i++)
            {
                var indices = indicesWithSign[i];
                var vector = signedInnerProducts[i];
                if (core==1)
                {
                    for (int k = 0; k < indices.Length; k++)
                    {
                        vector[k] = (i == 0 ? 1 : -1) * CalcInnerProduct(x,indices[k]);
                    }//for k
                }
                else
                {
                    Parallel.For(0, core, s =>
                       {
                           for (int k = s; k < indices.Length; k+=core)
                           {
                               vector[k] = (i == 0 ? 1 : -1) * CalcInnerProduct(x, indices[k]);
                           }
                       });
                }
            }//for i
        }
        
        public override object GetInitialBase(double[] b)
        {
            SetNorms();
            for (int i = 0; i < N; i++)
            {
                b[i] = 0;
            }//for i
            int[][] res = new int[Binary][];
            for (int i = 0; i < Binary; i++)
            {
                var order = new int[signedInnerProducts[i].Length];
                var indices =  indicesWithSign[i];
                for (int j = 0; j < order.Length; j++)
                {
                    order[j] = indices[j];
                }
                Array.Sort(signedInnerProducts[i], order);
                res[i] = new int[K + 1];
                for (int j = 0; j <= K; j++)
                {
                    res[i][j] = order[j];
                    SetExtremeBase(b, order[j], (i == 0 ? 1 : -1) * etaList[j]);
                }
            }
            return res;
        }

        private void SetNorms()
        {
            for (int i = 0; i < Binary; i++)
            {
                for (int k = 0; k < indicesWithSign[i].Length; k++)
                {
                    int index = indicesWithSign[i][k];
                    var vector = dataSet[index];
                    double norm = CalcNorm(index);
                    signedInnerProducts[i][k] = (i == 0 ? 1 : -1) * norm;
                }//for k
            }//for i
        }

        protected virtual double CalcInnerProduct(double[] x, int index)
        {
            var data = dataSet[index];
            double innerProduct = 0;
            for (int j = 0; j < data.Length; j++)
            {
                innerProduct += x[data.GetKey(j)] * data.GetValue(j);
            }
            return innerProduct;
        }

        protected virtual double CalcNorm(int index)
        {
            Data vector = dataSet[index];
            double norm = 0;
            double cur;
            for (int j = 0; j < vector.Length; j++)
            {
                cur = vector.GetValue(j);
                norm += cur * cur;
            }
            return norm;
        }

        //int iteration = 0;
        private int[][] SetExtremeBase(double[] extremeBase)
        {
            //iteration++;
            for (int i = 0; i < N; i++)
            {
                extremeBase[i] = 0;
            }//for i
            var res = new int[Binary][];
            int[] array;
            int[] indices;
            for (int i = 0; i < Binary; i++)
            {
                array = new int[K + 1];
                indices = indicesWithSign[i];
                var usedIndices = Algo.KthFinding(signedInnerProducts[i], K, rand);
                for (int p = 0; p <= K; p++)
                {
                    int index = indices[usedIndices[p]];
                    array[p] = index;
                    SetExtremeBase(extremeBase, index, (i == 0 ? 1 : -1) * etaList[p]);
                }//for i
                res[i] = array;
            }//for i
            return res;
        }

        protected virtual void SetExtremeBase(double[] extremeBase, int pos, double coeff)
        {
            var vector = dataSet[pos];
            for (int i = 0; i < vector.Length; i++)
            {
                extremeBase[vector.GetKey(i)] += coeff * vector.GetValue(i);
            }
        }

    }
}

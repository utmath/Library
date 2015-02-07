using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onigiri.Algorithm
{
    public class NuSVMResult
    {
        public const string standardOutput = "sfntivco";
        const int Binary = 2;
        double[] w;
        double[] lambdas;

        public string Message
        {
            get;
            set;
        }

        int Dimension
        { get; set; }

        int Samples
        { get; set; }

        public long ExecutionTime
        { get; private set; }

        public int CountSupportVectors
        {
            get;
            private set;
        }

        public double Accuracy
        {
            get { return TestSize==0?0:(double)CorrectSize / TestSize; }
        }

        public long CorrectSize
        {
            get;
            set;
        }

        public long TestSize
        {
            get;
            set;
        }

        public long Iteration
        { get; set; }

        public double[] W
        {
            get { return w; }
        }

        public double Nu
        { get; set; }

        public double B
        {
            get;
            private set;
        }

        double Rho
        { get; set; }

        public double C
        {
            get;
            private set;
        }

        public double OptValue
        {
            get;
            private set;
        }

        public int[] Labels
        {
            get;
            set;
        }

        public NuSVMResult(string message)
        {
            this.Message = message;
        }

        public NuSVMResult(int samples, int dimension, double nu, long executionTime, long Iteration,double[] w, object[] data, double[] lambdas, DataSet trainingData,DataSet testData,double[][]kernelMatrix,Func<Data,Data,double> kernel)
        {
            this.Message = null;
            this.Samples = samples;
            this.Dimension = dimension;
            this.Nu = nu;
            this.ExecutionTime = executionTime;
            this.Iteration = Iteration;
            SetNumberOfSupportVectors(data,lambdas);
            SetW(w,kernelMatrix);
            SetBAndRho(trainingData,kernelMatrix);
            SetC();
            SetOptValue(kernelMatrix);
            SetAccuracy(trainingData, testData,kernel);
        }

        private void SetBAndRho(DataSet dataSet,double[][]kernelMatrix)
        {
            const double AbsEps = 1e-12;
            double eta = 2.0 / (Nu * Samples);
            double plusValue = double.MinValue;
            double minusValue = double.MaxValue;
            for (int i = 0; i < lambdas.Length; i++)
            {
                double curVal = lambdas[i];
                double absVal = Math.Abs(curVal);
                if (absVal <= AbsEps || Math.Abs(absVal - eta) <= AbsEps)   //TODO : error
                {
                    continue;
                }//if
                double val = CalcInneerProduct(w, i, dataSet[i],kernelMatrix);
                if (curVal > 0)
                {
                    plusValue = Math.Max(plusValue, val);
                }//if
                if (curVal < 0)
                {
                    minusValue = Math.Min(minusValue, val);
                }
            }

            if (double.MinValue == plusValue || double.MaxValue == minusValue)
            {
                B = double.MaxValue;
                Rho = double.MaxValue;
            }
            else
            {
                B = -(plusValue + minusValue) / 2.0;
                Rho = (plusValue - minusValue) / 2.0;
            }
        }

        private double CalcInneerProduct(double[] w, int dataIndex,Data data, double[][] kernelMatrix)
        {
            double res = 0;
            if (kernelMatrix==null)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    res += w[data.GetKey(i)] * data.GetValue(i);
                }
            }
            else
            {
                for (int i = 0; i < Samples; i++)
                {
                    res += w[i] * kernelMatrix[i][dataIndex];
                }
            }
            return res;
        }

        private double InnerProduct(double[] w,int minFeatures, Data data)
        {
            double res = 0;
            for (int i = 0; i < data.Length; i++)
            {
                res += w[data.GetKey(i)] * data.GetValue(i);                
            }
            return res;
        }

        private void SetNumberOfSupportVectors(object[] data,double[]componentsLambdas)
        {
            var isSupportVector = new bool[Samples];
            lambdas = new double[Samples];
            double eta = 2.0 / (Nu * Samples);
            for(int k=0;k<data.Length;k++)
            {
                var usedIdicesList = (int[][])data[k];
                for (int i = 0; i < Binary; i++)
                {
                    for (int j = 0; j < usedIdicesList[i].Length; j++)
                    {
                        int index = usedIdicesList[i][j];
                        isSupportVector[index] = true;
                        double curVal = (j == usedIdicesList[i].Length - 1 ? 1 - eta * (usedIdicesList[i].Length - 1) : eta);
                        curVal *=(i==0?1:-1);
                        lambdas[index] += curVal*componentsLambdas[k];
                    }
                }
            }
            CountSupportVectors = isSupportVector.Count(x => x);
        }

        private void SetAccuracy(DataSet trainingData,DataSet testData, Func<Data, Data, double> kernel)
        {
            Labels = new int[0];
            if (testData!=null)
            {
                Labels = new int[testData.Samples];
                NuSVM.Predict(this,trainingData,testData,W, B, null, testData.signMapping,kernel);
            }
        }

        private void SetW(double[] w,double[][] kernelMatrix)
        {
            if (kernelMatrix ==null)
            {
                this.w = new double[w.Length];
                for (int i = 0; i < w.Length; i++)
                {
                    this.w[i] = w[i] * Nu * 0.5;
                }
            }
            else
            {
                this.w = (double[])w.Clone();
            }
        }

        private void SetOptValue(double[][] kernelMatrix)
        {
            OptValue = 0;
            if (kernelMatrix == null)
            {
                for (int i = 0; i < Dimension; i++)
                {
                    OptValue += W[i] * W[i];
                }//for i                
            }
            else
            {
                double[] tmp = new double[Samples];
                for (int i = 0; i < Samples; i++)
                {
                    for (int j = 0; j < Samples; j++)
                    {
                        tmp[i] += kernelMatrix[i][j] * W[j];
                    }
                }
                for (int i = 0; i < Samples; i++)
                {
                    OptValue += tmp[i] * W[i];
                }
            }
            OptValue = Math.Sqrt(OptValue);
        }

        private void SetC()
        {
            C = 1.0 / Rho / Samples;
        }

        public void Output(string outputParams,StreamWriter sw)
        {
            if (Message!=null)
            {
                WriteLine(sw,Message);
            }//if
            var dict = MakeDictionary();
            if (sw == null)
            {
                foreach (var c in standardOutput)
                {
                    Console.WriteLine(dict[c]);
                }//foreach c
                return;
            }
            else
            {
                string cur;
                foreach (var c in outputParams)
                {
                    if (dict.TryGetValue(c,out cur))
                    {
                        WriteLine(sw, cur);     
                    }
                }
            }//else

        }

        private Dictionary<char,string> MakeDictionary()
        {
            var dict = new Dictionary<char, string>();
            dict['s'] = "Sample " + Samples;
            dict['f'] = "Feature " + Dimension;
            dict['n'] = "Nu " + Nu;
            dict['t'] = "Time " + ExecutionTime;
            dict['i'] = "Iteration " + Iteration;
            dict['v'] = "#SupportVector " + CountSupportVectors;
            dict['w'] = MakeStringW();
            dict['b'] = "B "+B.ToString();
            dict['r'] ="Rho "+ Rho.ToString();
            dict['c'] = "C "+C.ToString();
            dict['o'] = "OptimalValue " + OptValue;
            dict['a'] = "Accuracy " + Accuracy;
            dict['l'] = MakeLable();
            return dict;
        }

        private string MakeLable()
        {
            var sbLabel = new StringBuilder();
            sbLabel.Append("Labels");
            for (int i = 0; i < Labels.Length; i++)
            {
                sbLabel.Append("\r\n" + Labels[i]);
            }
            return sbLabel.ToString();
        }

        private string MakeStringW()
        {
            var sbW = new StringBuilder();
            sbW.Append("W");
            for (int i = 0; i < W.Length; i++)
            {
                sbW.Append("\r\n" + W[i]);
            }
            return sbW.ToString();
        }

        public static void WriteLine<T>(StreamWriter sw, T val)
        {
            if (sw!=null)
            {
            string str = val.ToString();
            sw.WriteLine(str);
            Console.WriteLine(str);                
            }
        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Onigiri.Algorithm
{
    public class NuSVM
    {
        const int Binary = 2;
        DataSet trainingData;
        DataSet testingData;
        Dictionary<int, int> signMapping;
        ReducedLinearOptimization reducedLO;
        WolfeMinimumNormPoint wolfeMNP;
        Stopwatch timer;
        double[][] kernelMatrix;

        public int TrainingSamples
        {
            get { return trainingData.Samples; }
        }

        public int TestSamples
        {
            get { return testingData.Samples; }
        }

        public int Features
        {
            get;
            private set;
        }

        public int kernelOption
        {
            get;
            set;
        }

        public double[][] KernelMatrix
        {
            get { return kernelMatrix; }
        }

        public NuSVM(string trainingFile,int kernelOption,int core, string testingFile = null,double gamma=double.MinValue,double degree=double.MinValue)
        {
            this.kernelOption = kernelOption;
            signMapping = new Dictionary<int, int>();
            int maxFeature;
            trainingData = new DataSet(trainingFile, signMapping,out maxFeature);
            Features = maxFeature + 1;
            if (testingFile!=null)
            {
                int dummy;
                testingData = new DataSet(testingFile, signMapping, out dummy);                
            }//if

            if (degree==double.MinValue)
            {
                degree = 3.0;
            }
            if (gamma==double.MinValue)
            {
                gamma = 1.0 / Features;
            }

            reducedLO = (kernelOption == 0 ? new ReducedLinearOptimization(trainingData, Features, signMapping, core) :
                new ReducedLinearOptimizationKernel(kernelOption, trainingData, trainingData.Samples, signMapping, core
                    ,gamma,degree));
            kernelMatrix = (kernelOption==0 ? null : ((ReducedLinearOptimizationKernel)reducedLO).KernelMatrix);
            wolfeMNP = new WolfeMinimumNormPoint(reducedLO,kernelMatrix);
            timer = new Stopwatch();
        }

        public NuSVMResult Train(double nu,string outputParams = NuSVMResult.standardOutput, string outputFile = null)
        {
            var list = Enumerable.Repeat(nu, 1);
            var res = Train(outputParams,outputFile,list);
            if (res==null)
            {
                return null;
            }
            else
            {
                return res[0];
            }
        }

        public List<NuSVMResult> Train(double nuMin, double nuMax, double nuStep, string outputParams = NuSVMResult.standardOutput, string outputFile = null)
        {
            var list= GetEnumerator(nuMin, nuMax, nuStep);
            return Train(outputParams,outputFile, list);
        }

        public List<NuSVMResult> Train(List<double> nuList, string outputParams = NuSVMResult.standardOutput, string outputFile = null)
        {
            return Train(outputParams,outputFile, nuList);
        }

        private List<NuSVMResult> Train(string outputParams,string outputFile, IEnumerable<double> list)
        {
            if (signMapping.Count != Binary)
            {
                string message = "The number of labels must be 2 but this data set have " + Features + " labels.";
                return null;
            }//if
            StreamWriter sw = null;
            if (outputFile != null)
            {
                try
                {
                    sw = new StreamWriter(outputFile);
                }
                catch (Exception)
                {
                    Console.WriteLine("The file " + outputFile.ToString() + "can not be opened.");
                    return null;
                }
            }//if
            var res = new List<NuSVMResult>();
            foreach (var nu in list)
            {
                var result = Train(nu, outputParams,sw);
                res.Add(result);
            }//foreach nu
            if (sw != null)
            {
                sw.Close();
            }//if
            return res;
        }
        
        private IEnumerable<double> GetEnumerator(double min, double max, double step)
        {
            for (double nu = max; nu >= min; nu += step)
            {
                yield return nu;
            }
        }

        private NuSVMResult Train(double nu, string outputParams,StreamWriter sw)
        {
            NuSVMResult result;
            if (2 * trainingData.CalcLeastSamples(signMapping) < nu * trainingData.Samples + 1e-9)
            {
                double val = 2.0 * trainingData.CalcLeastSamples(signMapping) /trainingData.Samples-1e-9;
                string message = "The hyper-parameter nu must be smaller than 2 min{|m^+|,|m^-|} / m.";
                message += "In this case, nu must be smaller than " + val;
                result = new NuSVMResult(message);
            }//if
            else
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                double eta = 2.0 / (nu * trainingData.Samples);
                reducedLO.ResetEta(eta);
                timer.Restart();
                wolfeMNP.CalcMinimumNormPoint();
                timer.Stop();
                result = new NuSVMResult(trainingData.Samples, Features, nu, timer.ElapsedMilliseconds, wolfeMNP.Iteration, wolfeMNP.X, wolfeMNP.Data, wolfeMNP.Lambdas, trainingData, testingData,
                    (kernelOption == 0 ? null : ((ReducedLinearOptimizationKernel)reducedLO).KernelMatrix),
                    (kernelOption==0?null:((ReducedLinearOptimizationKernel)reducedLO).Kernel)
                    );
                if (testingData!=null&&sw!=null)
                {
                    Predict(result,trainingData, testingData, result.W, result.B, sw, signMapping
                        , (kernelOption == 0 ? null : ((ReducedLinearOptimizationKernel)reducedLO).Kernel));
                }
            }//else
            result.Output(outputParams, sw);
            return result;
        }


        internal static void Predict(NuSVMResult result,DataSet trainingData,DataSet testingData,double[] w, double b, StreamWriter sw, Dictionary<int, int> signMapping,Func<Data,Data,double> kernel)
        {
            int[] signs = new int[signMapping.Count];
            foreach (var pair in signMapping)
            {
                signs[pair.Value] = pair.Key;
            }//foreach pair
            long correct = 0;
            long all = 0;
            foreach (var data in testingData)
            {
                double value = b;
                if (kernel == null)
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        value += w[data.GetKey(i)] * data.GetValue(i);
                    }
                }
                else
                {
                    for (int i = 0; i < w.Length; i++)
                    {
                        value += w[i] * (data.Label == signs[1] ? -1 : 1) * kernel.Invoke(data, trainingData[i]);
                    }
                }
                int lable = (value >= 0 ? signs[0] : signs[1]);
                if (lable == data.Label)
                {
                    correct++;
                }
                result.Labels[all] = lable;
                all++;
            }//foreach data
            result.CorrectSize = correct;
            result.TestSize = all;
        }


    }

}

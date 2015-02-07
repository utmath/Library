using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Onigiri.Algorithm;

namespace ExecuteNuSVM
{
    class ExecuteNuSVM
    {

        public static void Main(string[] args)
        {
            string trainPath = null; string testPath = null; string output = null;
            double nuMin =1e-9; double nuMax = 1-1e-9; double nuStep = -0.01;
            double gamma = double.MinValue; double degree  = double.MinValue;
            string outputParams = "";
            int kernelOption = 0;
            int core = 1;
            List<double> nuList;
            bool good = SetParams(args, ref trainPath, ref testPath, ref outputParams, ref output, ref nuMin, ref nuMax, ref nuStep, ref kernelOption, ref gamma, ref degree, ref core, out nuList);
            if (!CanOpen(trainPath)||!CanOpen(testPath))
            {
                return;
            }
            if (good)
            {
                Execute(trainPath, testPath, outputParams, output, nuMin, nuMax, nuStep, kernelOption, gamma, degree, core, nuList);
            }//if
        }

        private static void Execute(string trainPath, string testPath, string outputParams,string output, double nuMin, double nuMax, double nuStep, int kernelOption,double gamma,double degree,int core,List<double>nuList)
        {
            if (nuStep>=0)
            {
                Console.WriteLine("The step size must be negative.");
                return;
            }//if
            NuSVM nuSVM = new NuSVM(trainPath, kernelOption,core, testPath,  gamma, degree);
            if (nuList!=null)
            {
                nuSVM.Train(nuList, outputParams,output);
            }
            else
            {
                nuSVM.Train(nuMin, nuMax, nuStep, outputParams, output);
            }
        }

        private static bool SetParams(string[] args, ref string trainPath, ref string testPath, ref string outputParams, ref string output, ref double nuMin, ref double nuMax, ref double nuStep, ref int kernelOption, ref double gamma, ref double degree, ref int core, out List<double> nuList)
        {
            var done = new HashSet<string>();
            nuList = null;
            for (int i = 0; i < args.Length; i += 2)
            {
                if (i + 1 >= args.Length)
                {
                    Console.WriteLine("The parameter " + args[i] + " is not defined");
                    return false;
                }//if
                if (done.Contains(args[i]))
                {
                    Console.WriteLine("The parameter "+args[i]+" is duplicate.");
                    return false;
                }
                done.Add(args[i]);
                if (!Update(args[i], args[i + 1], ref trainPath, ref testPath, ref outputParams, ref output, ref nuMin, ref nuMax, ref nuStep, ref kernelOption, ref core, out nuList))
                {
                    return false;
                }//if
            }//for i
            return true;
        }

        private static bool Update(string str, string val, ref string trainPath, ref string testPath, ref string outputParams, ref string output, ref double nuMin, ref double nuMax, ref double nuStep, ref int kernelOption, ref int core, out List<double> nuList)
        {
            nuList = null;
            if (str=="-n")
            {
                return ParseToDouble(ref nuMin, val);
            }//if
            if (str=="-N")
            {
                return ParseToDouble(ref nuMax, val);
            }//if
            if (str=="-t")
            {
                trainPath = val;
            }//if
            if (str=="-T")
            {
                testPath = val;
            }//if
            if (str=="-o")
            {
                output = val;
            }//if
            if (str=="-r")
            {
                outputParams = val;
            }
            if (str=="-v")
            {
                return ParseToDouble(ref nuStep, val);
            }//if
            if (str=="k")
            {
                return ParseToInt(ref kernelOption, val);
            }
            if (str=="-c")
            {
                return ParseToInt(ref core, val);
            }
            if (str=="-l")
            {
                if (!CanOpen(val))
                {
                    return false;
                }
                return ParseToDoubleList(out nuList,val);
            }
            return true;
        }

        private static bool ParseToDoubleList(out List<double> nuList, string val)
        {
            nuList = new List<double>();
            var sr = new StreamReader(val);
            string line;
            while (!string.IsNullOrEmpty(line=sr.ReadLine()))
            {
                double cur = -1;
                if (!ParseToDouble(ref cur, line))
                {
                    return false;
                }
                nuList.Add(cur);
            }
            return true;
        }

        private static bool CanOpen(string path)
        {
            StreamReader sr;
            try
            {
                sr = new StreamReader(path);
            }
            catch (Exception)
            {
                Console.WriteLine("The file "+path +" does not exist.");
                return false;
            }
            sr.Close();
            return true;
        }

        private static bool ParseToInt(ref int feature, string val)
        {
            try
            {
                feature = int.Parse(val);
            }
            catch (Exception)
            {
                Console.WriteLine(val+" can not be converted to integer.");
                return false;
            }
            return true;
        }

        private static bool ParseToDouble(ref double nu, string val)
        {
            try
            {
                nu = double.Parse(val);
            }
            catch (Exception)
            {
                Console.WriteLine(val + " can not be converted to number.");
                return false;
            }
            return true;
        }


    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Onigiri.Algorithm;
using System.Collections.Generic;

namespace TestWolfe
{
    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void TestMethod0()
        {
            int N = 4;
            int cnt = 3;

            var bases = Make(N, cnt);
            Test(N, bases);
        }

        [TestMethod]
        public void TestMethod1()
        {
            const int N = 3;
            const int cnt = 3;

            var bases = new List<double[]>(cnt);
            bases.Add(new double[N] { 1, 0, 0 });
            bases.Add(new double[N] { 0, 1, 0 });
            bases.Add(new double[N] { 0, 0, 1 });
            Test(N, bases);
        }

        [TestMethod]
        public void TestMethod2()
        {
            const int N = 3;
            const int cnt = 3;

            var bases = new List<double[]>(cnt);
            bases.Add(new double[N] { 1, 0, -1 });
            bases.Add(new double[N] { 0, 1, 0 });
            bases.Add(new double[N] { 0, 0, 1 });
            Test(N, bases);
        }

        private static void Test(int N, List<double[]> bases)
        {
            BruteForceLinearOptimization bflo = new BruteForceLinearOptimization(N, bases);
            WolfeMinimumNormPoint wmnp = new WolfeMinimumNormPoint();
            var res = wmnp.CalcMinimumNormPoint(bflo);
            foreach (var item in res)
            {
                Console.WriteLine(item);
            }//foreach item
        }

        private List<double[]> Make(int N, int cnt)
        {
            var rand = new Random(0);
            var res = new List<double[]>();
            for (int i = 0; i < cnt; i++)
            {
                var cur = new double[N];
                for (int k = 0; k < N; k++)
                {
                    cur[k] = rand.NextDouble();
                }//for k
                res.Add(cur);
            }//for i
            return res;
        }
    }
}

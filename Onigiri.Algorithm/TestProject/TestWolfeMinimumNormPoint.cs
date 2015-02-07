using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Onigiri.Algorithm;
using System.Collections.Generic;

namespace TestWolfe
{
    [TestClass]
    public class TestWolfeMinimumNormPoint
    {

        [TestMethod]
        public void TestWolfeMinimumNormPoint0()
        {
            int N = 4;
            int cnt = 3;

            var bases = Make(N, cnt);
            Test(N, bases);
        }

        [TestMethod]
        public void TestWolfeMinimumNormPoint1()
        {
            const int N = 3;
            const int cnt = 3;

            var bases = new double[cnt][];
            bases[0]=(new double[N] { 1, 0, 0 });
            bases[1]=(new double[N] { 0, 1, 0 });
            bases[2]=(new double[N] { 0, 0, 1 });
            Test(N, bases);
        }

        [TestMethod]
        public void TestWolfeMinimumNormPoint2()
        {
            const int N = 3;
            const int cnt = 3;

            var bases = new double[cnt][];
            bases[0] = (new double[N] { 1, 0, -1 });
            bases[1] = (new double[N] { 0, 1, 0 });
            bases[2] = (new double[N] { 0, 0, 1 });
            Test(N, bases);
        }

        private static void Test(int N, double[][] bases)
        {
            var bflo = new BruteForceLinearOptimization(N, bases);
            var wmnp = new WolfeMinimumNormPoint(bflo);
            var res = wmnp.CalcMinimumNormPoint();
            foreach (var item in res)
            {
                Console.WriteLine(item);
            }//foreach item
        }

        private double[][] Make(int N, int cnt)
        {
            var rand = new Random(0);
            var res = new double[cnt][];
            for (int i = 0; i < cnt; i++)
            {
                var cur = new double[N];
                for (int k = 0; k < N; k++)
                {
                    cur[k] = rand.NextDouble();
                }//for k
                res[i] = cur;
            }//for i
            return res;
        }
    }
}

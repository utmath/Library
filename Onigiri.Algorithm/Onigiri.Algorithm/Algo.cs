using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onigiri.Algorithm
{
    public static class Algo
    {
        public static int[] KthFinding(double[] vals, int k, Random rand)
        {
            int done;
            var res = new int[k + 1];
            int len = vals.Length;
            //int cut = (int)(Math.Log(vals.Length, 2));
            int cut = 4;
            if (vals.Length < 100)
            {
                int[] indices = new int[len];
                for (int i = 0; i < len; i++)
                {
                    indices[i] = i;
                }//for i
                Array.Sort(vals, indices);
                for (int i = 0; i <= k; i++)
                {
                    res[i] = indices[i];
                }//for i
                return res;
            }
            else if (k < cut)
            {
                done = 0;
                for (int i = 0; i < len; i++)
                {
                    int cur = i;
                    for (int index = 0; index < done; index++)
                    {
                        if (vals[cur] < vals[res[index]])
                        {
                            int memo = cur;
                            cur = res[index];
                            res[index] = memo;
                        }//if
                    }//for index
                    if (done<= k)
                    {
                        res[done++] = cur;
                    }//if
                }//for i
                return res;
            }//if
            else if (len - k <= cut)
            {
                List<int> list = new List<int>();
                for (int i = 0; i < len; i++)
                {
                    int cur = i;
                    for (int index = 0; index < list.Count; index++)
                    {
                        if (vals[cur] > vals[list[index]])
                        {
                            int memo = cur;
                            cur = list[index];
                            list[index] = memo;
                        }//if
                    }//for index
                    if (list.Count < len - k)
                    {
                        list.Add(cur);
                    }//if
                }//for i
                done = 0;
                for (int i = 0; i < len; i++)
                {
                    int j = 0;
                    for (; j < list.Count; j++)
                    {
                        if (i == list[j])
                        {
                            break;
                        }//if
                    }//for j
                    if (j == list.Count)
                    {
                        res[done++] = i;
                    }//if
                }//for i
                res[done++] = list[list.Count - 1];
                return res;
            }//if

            double pow = Math.Pow(len, 0.75);
            int num = (int)(pow + 1);
            double[] sub = new double[num];

        Again:

            for (int i = 0; i < num; i++)
            {
                sub[i] = vals[rand.Next(len)];
            }
            Array.Sort(sub);
            int x = (int)(k / Math.Pow(len, 0.25));
            double down = sub[Math.Max(0, (int)(x - Math.Sqrt(len)))];
            double up = sub[Math.Min(num - 1, (int)(x + Math.Sqrt(len)))];
            List<double> C = new List<double>();
            int cntDown = 0; int cntUp = 0;
            for (int i = 0; i < len; i++)
            {
                double now = vals[i];
                if (now < down) cntDown++;
                else if (now > up) cntUp++;
                else C.Add(now);
            }//for i
            if (cntDown > k || cntUp >= len - k || C.Count > 4 * pow + 2)
            {
                goto Again;
            }
            int cnt = 0;
            double[] values = new double[C.Count];
            int[] numbers = new int[C.Count];
            done = 0;
            for (int i = 0; i < len; i++)
            {
                double now = vals[i];
                if (now < down)
                {
                    res[done++] = i;
                }//if
                else if (now <= up)
                {
                    numbers[cnt] = i;
                    values[cnt++] = now;
                }//if
            }//for i
            Array.Sort(values, numbers);
            for (int i = 0; i <= k - cntDown; i++)
            {
                res[done++] = numbers[i];
            }//for i
            return res;
        }
    }
}

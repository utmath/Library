using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onigiri.Algorithm
{
    public class Data
    {
        const int NoLabel = int.MinValue;
        const int Binary = 2;
        int[] keys;
        double[] values;

        public int Label
        {
            get;
            private set;
        }

        public int Length
        {
            get { return keys.Length; }
        }

        internal void AdjustKey(int offset)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] -= offset;
            }
        }

        public int GetKey(int index)
        {
            return keys[index];
        }

        public double GetValue(int index)
        {
            return values[index];
        }

        public Data(string line, Dictionary<int, int> signMapping,ref int minFeature,ref int maxFeatrue)
        {
            Label = NoLabel;
            string[] split = line.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
            int ptr = ReadSign(split, signMapping);
            keys = new int[split.Length - ptr];
            values = new double[split.Length - ptr];
            ReadVector(split, ptr,ref  minFeature,ref  maxFeatrue);
        }

        private void ReadVector(string[] split, int ptr,ref int minFeature,ref int maxFeatrue)
        {
            for (int i = ptr; i < split.Length; i++)
            {
                string[] tmp = split[i].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                int feature = int.Parse(tmp[0]);
                maxFeatrue = Math.Max(maxFeatrue, feature);
                minFeature = Math.Min(minFeature, feature);
                double value = double.Parse(tmp[1]);
                keys[i - ptr] = feature;
                values[i - ptr] =  value;
            }//for i
        }

        private int ReadSign(string[] split, Dictionary<int, int> signMapping)
        {
            int ptr = 0;
            if (split.Length > 0 && !split[0].Contains(':'))
            {
                Label = int.Parse(split[0]);
                ptr++;
                if (!signMapping.ContainsKey(Label))
                {
                    signMapping[Label] = signMapping.Count;
                }//if
            }//if
            return ptr;
        }

    }
}

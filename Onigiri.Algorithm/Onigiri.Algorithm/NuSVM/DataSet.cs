using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Onigiri.Algorithm
{
    public class DataSet : IEnumerable
    {
        Data[] dataSet;
        public Dictionary<int, int> signMapping;

        public DataSet(string filePath, Dictionary<int, int> signMapping, out int maxFeature)
        {
            this.signMapping = signMapping;
            var listData = new List<Data>();
            maxFeature = int.MinValue;
            int minFeature = int.MaxValue;
            StreamReader sr = OpenFile(filePath);
            string line;
            while (!string.IsNullOrEmpty(line = sr.ReadLine()))
            {
                listData.Add(new Data(line, signMapping, ref minFeature,ref maxFeature));
            }//while
            dataSet = listData.ToArray();
            foreach (var data in dataSet)
            {
                data.AdjustKey(minFeature);
            }
            sr.Close();
        }

        private int CountSampleSize(string filePath)
        {
            var sr = OpenFile(filePath);
            string line;
            int cnt = 0;
            while (!string.IsNullOrEmpty(line = sr.ReadLine()))
            {
                cnt++;
            }//while
            sr.Close();
            return cnt;
        }

        private static StreamReader OpenFile(string filePath)
        {
            StreamReader sr;
            try
            {
                sr = new StreamReader(filePath);
            }
            catch (Exception)
            {
                Console.WriteLine("The file " + filePath + " can not be opened.");
                return null;
            }
            return sr;
        }
        
        public int Samples
        {
            get { return dataSet.Length; }
        }

        public int CalcLeastSamples(Dictionary<int, int> signMapping)
        {
            var cnt = new int[2];
            foreach (var data in dataSet)
            {
                cnt[signMapping[data.Label]]++;
            }//foreach data
            return Math.Min(cnt[0], cnt[1]);
        }

        public IEnumerator<Data> GetEnumerator()
        {
            foreach (var data in dataSet)
            {
                yield return data;
            }//foreach data
        }

        public Data this[int index]
        {
            get { return dataSet[index]; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onigiri.Algorithm
{
    class ConvexComponentsQR : ConvexComponents
    {
        public ConvexComponentsQR(int n, double[][] kernelMatrix, double absEps, double relativeEps)
            : base(n, kernelMatrix, absEps, relativeEps)
        {
            SetInitialQ(N+1);
        }

        protected override bool AddToMatrix(double[] extremeBase)
        {
            if (Count==0)
            {
                for (int i = 0; i < N; i++)
                {
                    Q[0][i] = extremeBase[i];
                }
                Q[0][N] = 1;
            }
            else
            {
                MultipleyByQTransposeWithOne(extremeBase, matrix[Count]);
                MultipleyByQ(matrix[Count], Q[Count]);
                MinusByBWithOne(extremeBase, Q[Count]);
            }
            matrix[Count][Count] = Normalize(Q[Count]);
            return matrix[Count][Count] >= Math.Abs(matrix[0][0]) * RelativeEps;
        }

        private double Normalize(double[] vector)
        {
            double res = 0;
            for (int i = 0; i <= N; i++)
            {
                res += vector[i] * vector[i];
            }
            res = Math.Sqrt(res);
            for (int i = 0; i <= N; i++)
            {
                vector[i] /= res;
            }
            return res;
        }

        private void MinusByBWithOne(double[] extremeBase, double[] q)
        {
            for (int i = 0; i < N; i++)
            {
                q[i] = extremeBase[i] - q[i];
            }
            q[N] = 1 - q[N];
        }

        private void MultipleyByQ(double[] cur, double[] res)
        {
            for (int i = 0; i <= N; i++)
            {
                res[i] = 0;
                for (int j = 0; j < Count; j++)
                {
                    res[i] += Q[j][i] * cur[j];
                }
            }
        }

        private void MultipleyByQTransposeWithOne(double[] cur, double[] res)
        {
            for (int i = 0; i < Count; i++)
            {
                res[i] = Q[i][N];
                for (int j = 0; j < N; j++)
                {
                    res[i] += cur[j] * Q[i][j];
                }
            }
        }

        protected override void CalcBestLambdas()
        {
            for (int i = 0; i < Count; i++)
            {
                vector[i] = Q[i][N];
            }
            SolveLinearEquation(vector, r, Count);
            double sum = 0;
            for (int i = 0; i < Count; i++)
            {
                sum += r[i];
            }
            for (int i = 0; i < Count; i++)
            {
                bestLambdas[i] = r[i] / sum;
            }
        }

        protected override void Delete(int index)
        {
            for (int i = index; i < Count-1; i++)
            {
                var tmpVariable = matrix[i];
                matrix[i] = matrix[i+1];
                matrix[i+1] = tmpVariable;
                var tmp = components[i];
                components[i] = components[i+1];
                components[i+1] = tmp;
            }
            for (int i = index; i < Count-1; i++)
            {
                Elimination(i, i + 1, Count - 1);
            }
            Count--;
        }

        protected override void Elimination(int pivotRow, int eraseRow, int endCol)
        {
            double a = matrix[pivotRow][pivotRow];
            double b = matrix[pivotRow][eraseRow];
            double sqrt = Math.Sqrt((a * a + b * b));
            if (sqrt == 0)
            {
                return;
            }
            double cos = a / sqrt;
            double sin = b / sqrt;
            for (int j = pivotRow; j < endCol; j++)
            {
                double pivot = matrix[j][pivotRow];
                double erase = matrix[j][eraseRow];
                //TODO: set tolerance
                matrix[j][pivotRow] = pivot * cos + erase * sin;
                matrix[j][eraseRow] = -pivot * sin + erase * cos;
            }//for j
            matrix[pivotRow][eraseRow] = 0;

            for (int j = 0; j <= N; j++)
            {
                double pivot = Q[pivotRow][j];
                double erase = Q[eraseRow][j];
                Q[pivotRow][j] = pivot * cos + erase * sin;
                Q[eraseRow][j] = -pivot * sin + erase * cos;
            }//for j
        }



    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onigiri.Algorithm
{
    class ConvexComponentsImiroved:ConvexComponents
    {
        double[] diff;

        public ConvexComponentsImiroved(int n, double[][] kernelMatrix, double absEps, double relativeEps)
            : base(n, kernelMatrix, absEps, relativeEps)
        {
            diff = new double[N];
            SetInitialQ(N);
        }

        protected override bool AddToMatrix(double[] extremeBase)
        {
            if (Count == 0)
            {
                return true;
            }
            SetDiffVector(extremeBase);
            MultipleyByQTranspose(diff, matrix[Count - 1]);
            for (int i = Count; i < N; i++)
            {
                Elimination(Count - 1, i, Count);
            }
            return matrix[Count - 1][Count - 1] >= Math.Abs(matrix[0][0]) * RelativeEps;
        }

        //private void Check()
        //{
        //    double[][] c = new double[N][];
        //    for (int p = 0; p < N; p++)
        //    {
        //        c[p] = new double[N];
        //    }
        //    for (int p = 0; p < N; p++)
        //    {
        //        for (int j = 0; j < N; j++)
        //        {
        //            for (int k = 0; k < N; k++)
        //            {
        //                c[p][j] += Q[k][p] * matrix[j][k];
        //            }
        //        }
        //    }
        //    for (int p = 0; p < N; p++)
        //    {
        //        for (int q = 0; q < N; q++)
        //        {
        //            if (Math.Abs(c[p][q] + 1e9 - (long)(c[p][q] + 1e-9 + 1e9)) > 1e-3)
        //            {
        //                Console.WriteLine();
        //                return;
        //            }
        //        }
        //    }
        //}

        private void SetDiffVector(double[] extremeBase)
        {
            for (int i = 0; i < N; i++)
            {
                diff[i] = components[Count - 1].B[i] - extremeBase[i];
            }
        }

        protected override void CalcBestLambdas()
        {
            for (int i = 0; i < N; i++)
            {
                diff[i] = -components[0].B[i];
            }
            MultipleyByQTranspose(diff, vector);
            SolveLinearEquation(vector, r, Count - 1);
            for (int i = 0; i < Count; i++)
            {
                bestLambdas[i] = 0;
            }
            bestLambdas[0] = 1;
            for (int i = 0; i < Count-1; i++)
            {
                bestLambdas[i] += r[i];
                bestLambdas[i+1] -= r[i];
            }
        }

        private void MultipleyByQTranspose(double[] diff, double[] vector)
        {
            for (int i = 0; i < N; i++)
            {
                vector[i] = 0;
                for (int j = 0; j < N; j++)
                {
                    vector[i] += diff[j] * Q[i][j];
                }
            }
        }

        protected override void Delete(int index)
        {
            //int eraseCol0; int eraseCol1;
            //if (index==0||index==Count-1)
            //{
            //    int cur = eraseCol0 != -1 ? eraseCol0 : eraseCol1;
            //    SwapMatrix(cur);
            //    Elimination(cur, Count - 2);
            //}
            //else
            //{
            //    for (int i = 0; i < Count-1; i++)
            //    {
            //        matrix[eraseCol1][i] += matrix[eraseCol0][i];
            //    }
            //    SwapMatrix(eraseCol1);
            //    SwapMatrix(eraseCol0 - (eraseCol1 < eraseCol0 ? 1 : 0));
            //    int min = Math.Min(eraseCol0, eraseCol1);
            //    int max = Math.Max(eraseCol0, eraseCol1);
            //    int endCol = Count - 2;
            //    for (int i = min; i < endCol-1; i++)
            //    {
            //        Elimination(i, i + 1, endCol);
            //        if (i>=max-1)
            //        {
            //            Elimination(i, i + 2, endCol);
            //        }
            //    }
            //    for (int i = endCol; i < Count - 1; i++)
            //    {
            //        Elimination(endCol - 1, i, endCol);
            //    }
            //}

            if (index<Count-1)
            {
                if (index>0)
                {
                    for (int i = 0; i <= index; i++)
                    {
                        matrix[index - 1][i] += matrix[index][i];
                    }
                }
                SwapMatrix(index);
                int endCol = Count - 2;
                for (int i = Math.Max(0,index-1); i < endCol; i++)
                {
                    Elimination(i, i + 1, endCol);
                }
            }

            DeleteFromComponents(index);
            Count--;
        }

        private void SwapMatrix(int eraseCol)
        {
            for (int i = eraseCol; i < Count - 2; i++)
            {
                var tmp = matrix[i];
                matrix[i] = matrix[i + 1];
                matrix[i + 1] = tmp;
            }
        }

        private void DeleteFromComponents(int index)
        {
            for (int i = index; i < Count - 1; i++)
            {
                var tmpVariable = components[i];
                components[i] = components[i + 1];
                components[i + 1] = tmpVariable;
            }
        }

        //private void GetDeleteCols(int index, out int eraseCol0, out int eraseCol1)
        //{
        //    eraseCol0 = -1;
        //    eraseCol1 = -1;
        //    for (int i = 0; i < Count - 1; i++)
        //    {
        //        if (C[i] == index)
        //        {
        //            eraseCol0 = i;
        //        }
        //        else if (C[i] == index - 1)
        //        {
        //            eraseCol1 = i;
        //        }

        //        if (C[i] > index)
        //        {
        //            C[i]--;
        //        }
        //    }
        //}

        protected override void Elimination(int pivotRow, int eraseRow, int endCol)
        {
            double a = matrix[pivotRow][pivotRow];
            double b = matrix[pivotRow][eraseRow];
            double sqrt = Math.Sqrt((a * a + b * b));
            if (sqrt==0)
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

            for (int j = 0; j < N; j++)
            {
                double pivot = Q[pivotRow][j];
                double erase = Q[eraseRow][j];
                Q[pivotRow][j] = pivot * cos + erase * sin;
                Q[eraseRow][j] = -pivot * sin + erase * cos;
            }//for j
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onigiri.Algorithm
{
    class ConvexComponentsCholesky:ConvexComponents
    {
        double[] unitVector;

        public ConvexComponentsCholesky(int n, double[][] kernelMatrix, double absEps, double relativeEps)
            : base(n, kernelMatrix, absEps, relativeEps)
        {
            unitVector = new double[N + 1];
            for (int i = 0; i < N + 1; i++)
            {
                unitVector[i] = 1;
            }
        }

        protected override void Elimination(int pivotRow, int eraseRow, int endCol)
        {
            double a = matrix[pivotRow][pivotRow];
            double b = matrix[pivotRow][eraseRow];
            double sqrt = Math.Sqrt((a * a + b * b));
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
        }

        protected override void CalcBestLambdas()
        {
            SolveLinearEquationByTranspose(unitVector, vector);
            SolveLinearEquation(vector, bestLambdas, Count);
            Normalize(bestLambdas);
        }


        private void SolveLinearEquationByTranspose(double[] c, double[] vector)
        {
            for (int i = 0; i < Count; i++)
            {
                double sum = 0;
                for (int j = 0; j < i; j++)
                {
                    sum += matrix[i][j] * vector[j];
                }//for j
                double current = c[i] - sum;
                vector[i] = current / matrix[i][i];
            }//for i
        }


        private void Normalize(double[] r)
        {
            double sum = 0;
            for (int i = 0; i < Count; i++)
            {
                sum += r[i];
            }//for i
            double inverse = 1.0 / sum;
            for (int i = 0; i < Count; i++)
            {
                r[i] *= inverse;
            }//for i
        }

        protected override bool AddToMatrix(double[] extremeBase)
        {
            MultiplyByTransposeAndAddOne(extremeBase, vector);
            SolveLinearEquationByTranspose(vector, r);
            double rho = CalcRho(extremeBase);
            if (rho == 0)
            {
                return false;
            }//if
            double maxAbsValue = -1;
            for (int i = 0; i < Count; i++)
            {
                matrix[Count][i] = r[i];
                maxAbsValue = Math.Max(maxAbsValue, matrix[i][i]);
            }//for i
            if (rho < maxAbsValue * RelativeEps)
            {
                return false;
            }//if
            matrix[Count][Count] = rho;
            return true;
        }

        protected override void Delete(int index)
        {
            int endCol = Count - 1;
            for (int i = index; i < endCol; i++)
            {
                Swap(i, i + 1);
            }//for i
            Elimination(index, endCol);
            Count--;
        }

        private double CalcRho(double[] extremeBase)
        {
            double rho = 1;
            rho += CalcSquareKernel(extremeBase);
            rho -= CalcSquare(r, Count);
            rho = Math.Max(0, rho);
            rho = Math.Sqrt(rho);
            return rho;
        }
        
        private double CalcSquare(double[] x, int length = -1)
        {
            return CalcInnerProduct(x, x, length);
        }

    }
}

using System;

namespace SelfOrganizingMap
{
    public class Matrix
    {
        protected double[][] Values;

        public Matrix(int rows, int cols)
        {
            Values = new double[rows][];
            for (int i = 0; i < rows; i++)
            {
                Values[i] = new double[cols];
            }
        }

        public Matrix(int[][] values)
        {
            Values = Array.ConvertAll(values, (a) =>
                {
                    return Array.ConvertAll<int, double>(a, y => y);
                });
        }

        public Matrix(double[][] values)
        {
            Values = values;
        }

        public Matrix(double[] values)
        {
            Values = new double[1][]
            {
                values
            };
        }

        public static Matrix ForAll(Matrix a, Func<double, double> func)
        {
            var result = new Matrix(a.Rows, a.Cols);

            for (var row = 0; row < a.Rows; row++)
            {
                for (var column = 0; column < a.Cols; column++)
                {
                    result[row][column] = func(a[row][column]);
                }
            }

            return result;
        }

        public static Matrix ForAll(Matrix a, double b, Func<double, double, double> func)
        {
            var result = new Matrix(a.Rows, a.Cols);

            for (var row = 0; row < a.Rows; row++)
            {
                for (var column = 0; column < a.Cols; column++)
                {
                    result[row][column] = func(a[row][column], b);
                }
            }

            return result;
        }

        public static Matrix ForAll(Matrix a, Matrix b, Func<double, double, double> func)
        {
            var result = new Matrix(a.Rows, b.Cols);

            for (var row = 0; row < a.Rows; row++)
            {
                for (var column = 0; column < b.Cols; column++)
                {
                    result[row][column] = func(a[row][column], b[row][column]);
                }
            }

            return result;
        }

        public Matrix Mul(Matrix b)
        {
            var result = new Matrix(Rows, b.Cols);

            for (var i = 0; i < Rows; i++)
            {
                for (var j = 0; j < b.Cols; j++)
                {
                    var temp = 0.0;
                    for (var k = 0; k < Cols; k++)
                    {
                        temp += this[i][k] * b[k][j];
                    }
                    result[i][j] = temp;
                }
            }

            return result;
        }

        public static Matrix operator *(double b, Matrix a) => ForAll(a, b, (x, y) => x * y);
        public static Matrix operator *(Matrix a, double b) => ForAll(a, b, (x, y) => x * y);

        public static Matrix operator *(Matrix a, Matrix b)
        {
            var result = new Matrix(a.Rows, a.Cols);

            for (var row = 0; row < a.Rows; row++)
            {
                for (var column = 0; column < a.Cols; column++)
                {
                    result[row][column] = a[row][column] * b[row][column];
                }
            }

            return result;
        }

        public static Matrix operator +(Matrix a, Matrix b) => ForAll(a, b, (x, y) => x + y);

        public static Matrix operator -(Matrix a, Matrix b) => ForAll(a, b, (x, y) => x - y);
        
        public Matrix Pow(double x)
        {
            for (var row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    Values[row][col] = Math.Pow(Values[row][col], x);
                }
            }

            return this;
        }

        public static Matrix operator -(Matrix a) => ForAll(a, x => -x);

        public virtual double[] this[in int row]
        {
            get => Values[row];
            set => Values[row] = value;
        }

        public int Rows => Values.Length;

        public int Cols => Values[0].Length;

        public double Sum()
        {
            var result = 0.0;
            for (var row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    result += Values[row][col];
                }
            }

            return result;
        }

        public Vector ToVector()
        {
            return new Vector(Values[0]);
        }

        public static Matrix Sigmoid(Matrix x)
        {
            var result = new Matrix(x.Rows, x.Cols);
            for (var row = 0; row < x.Rows; row++)
            {
                for (var column = 0; column < x.Cols; column++)
                {
                    result[row][column] = Sigmoid(x[row][column]);
                }
            }

            return result;
        }

        public static double Sigmoid(double x)
        {
            return 1.0 / (1.0 + Math.Exp(-x));
        }

        public static Matrix SigmoidDerivative(Matrix x)
        {
            var result = new Matrix(x.Rows, x.Cols);
            for (var row = 0; row < x.Rows; row++)
            {
                for (var column = 0; column < x.Cols; column++)
                {
                    result[row][column] = SigmoidDerivative(x[row][column]);
                }

            }

            return result;
        }

        public static double SigmoidDerivative(double x)
        {
            return x * (1 - x);
        }

        public static double None(double x)
        {
            return x;
        }
    }

    public class Vector : Matrix
    {
        public Vector(int cols) : base(1, cols)
        {
        }

        public Vector(int[] values) : base(new[] { values })
        {
        }

        public Vector(params double[] values) : base(new[] { values })
        {
        }

        public new virtual double this[in int row]
        {
            get => Values[0][row];
            set => Values[0][row] = value;
        }

        public static Vector operator -(Vector a)
        {
            return (-(Matrix)a).ToVector();
        }

        public static Vector operator -(Vector a, Vector b)
        {
            return ((Matrix)a - (Matrix)b).ToVector();
        }

        public Matrix Transpose()
        {
            var result = new Matrix(Cols, 1);
            for (int i = 0; i < Cols; i++)
            {
                result[i][0] = Values[0][i];
            }

            return result;
        }
    }
}

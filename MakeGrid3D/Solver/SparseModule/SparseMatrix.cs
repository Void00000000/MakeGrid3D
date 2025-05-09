using System.Collections.Generic;

namespace MakeGrid3D.Solver.SparseModule
{
    /// <summary>
    /// Матрица в разреженном строчно-столбцовом формате. 
    /// </summary>
    public class SparseMatrix
    {
        /// <summary>
        /// Целочисленный массив ig. 
        /// Элемент ig(k) равен индексу, с которого начинаются элементы k -й строки (столбца) в массивах jg, gl и gu.
        /// ig(k + 1) - ig(k) равна количеству хранимых внедиагональных элементов 
        /// i-й строки(столбца) нижнего(верхнего) треугольника
        /// Размерность ij равна N + 1.
        /// </summary>
        public List<int> Ig { get; private set; }

        /// <summary>
        /// Целочисленный массив jg содержит номера столбцов (строк) хранимых 
        /// внедиагональных элементов нижнего(верхнего) треугольника матрицы.
        /// Размерность jg равна размерности gl и gu.
        /// </summary>
        public List<int> Jg { get; private set; }

        /// <summary>
        /// Вещественный массив di содержит последовательно диагональные элементы матрицы.
        /// Размерность di равна размерности N.
        /// </summary>
        public List<double> Di { get; private set; }

        /// <summary>
        /// Вещественный массив gl для хранения внедиагональных элементов матрицы
        /// нижнего по строкам треугольника.
        /// </summary>
        public List<double> Gl { get; private set; }

        /// <summary>
        /// Вещественный массив gu для хранения внедиагональных элементов матрицы
        /// верхнего по стообцам треугольника.
        /// </summary>
        public List<double> Gu { get; private set; }

        /// <summary>
        /// Размерность матрицы.
        /// </summary>
        public int N { get; }

        /// <summary>
        /// Размерность массивов jg, gl, gu.
        /// </summary>
        public int Ng { get; }

        /// <summary>
        /// Инициализирует <see cref="SparseMatrix"/>.
        /// </summary>
        /// <param name="n">Размерность матрицы.</param>
        public SparseMatrix(int n)
        {
            N = n;
            Di = new List<double>(N);
            Ig = new List<int>(N + 1);
            Jg = new List<int>();
            Gu = new List<double>();
            Gl = new List<double>();
        }

        /// <summary>
        /// Получает элемент в i строке, j столбце (нумерация с 0).
        /// </summary>
        public double this[int i, int j]
        {
            get
            {
                if (i >= N || j >= N) 
                {
                    return 0;
                }

                if (i == j)
                {
                    return Di[i];
                }

                for (int k = Ig[i]; k < Ig[i+1]; k++)
                {
                    if (Jg[k] == j)
                    {
                        return i > j ? Gl[i] : Gu[i];
                    }
                }
                return 0;
            }

            set 
            { 
            }
        }
    }
}

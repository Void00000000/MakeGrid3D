global using GTree = System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<(int, double)>>;
using System.Collections.Generic;

namespace MakeGrid3D.Solver.SparseModule
{
    /// <summary>
    /// Матрица в разреженном строчно-столбцовом формате. 
    /// </summary>
    public class SparseMatrix
    {
        #region Public Properties

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
        public int Ng { get; private set; }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Инициализирует <see cref="SparseMatrix"/>.
        /// </summary>
        /// <param name="n">Размерность матрицы.</param>
        public SparseMatrix(int n)
        {
            N = n;

            Di = new List<double>(N);
            for (int i = 0; i < N; i++)
                Di.Add(0);
            Ig = new List<int>(N + 1);
            for (int i = 0; i < N + 1; i++)
                Ig.Add(0);
        }

        /// <summary>
        /// Выделяет память для массивов jg, gl, gu.
        /// </summary>
        /// <param name="ng">Размерность массивов jg,gl,gu.</param>
        public void Alloc(int ng)
        {
            Ng = ng;
            
            Jg = new List<int>(Ng);
            for (int i = 0; i < Ng; i++)
                Jg.Add(0);

            Gu = new List<double>(Ng);
            for (int i = 0; i < Ng; i++)
                Gu.Add(0);

            Gl = new List<double>(Ng);
            for (int i = 0; i < Ng; i++)
                Gl.Add(0);
        }

        #endregion Constructors
    }
}

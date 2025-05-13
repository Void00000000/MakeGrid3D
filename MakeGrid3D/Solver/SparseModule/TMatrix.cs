namespace MakeGrid3D.Solver.SparseModule
{
    using System.Collections.Generic;

    /// <summary>
    /// T матрица в разреженном столбцовом формате.
    /// Первые Nc столбцом образуют единичную подматрицу (Nc = кол-во регулряных узлов).
    /// </summary>
    public class TMatrix
    {
        #region Public Properties

        /// <summary>
        /// Целочисленный массив ig. 
        /// Элемент ig(k) равен индексу, с которого начинаются элементы (Nc + 1 + k)-го стобца (столбца) в массивах jg, gl и gu.
        /// ig(k + 1) - ig(k) равна количеству ненулевых елементов в k-ом столбце 
        /// Размерность ij равна N - Nc + 1.
        /// </summary>
        public List<int> Ig { get; private set; }

        /// <summary>
        /// Целочисленный массив jg содержит номера строк ненулевых элементов
        /// Размерность jg равна размерности gl и gu.
        /// </summary>
        public List<int> Jg { get; private set; }

        /// <summary>
        /// Ненулевые элементы матрицы (единичная подматрица сюда не входит)
        /// </summary>
        public List<double> Gg { get; private set; }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Инициализирует <see cref="TMatrix"/>.
        /// </summary>
        /// <param name="n">Размерность матрицы.</param>
        /// <param name="nc">Количество регулярных узлов.</param>
        public TMatrix(int n, int nc)
        {
            Ig = new List<int>(n - nc + 1);
            Jg = new List<int>();
            Gg = new List<double>();
        }

        #endregion Constructors
    }
}
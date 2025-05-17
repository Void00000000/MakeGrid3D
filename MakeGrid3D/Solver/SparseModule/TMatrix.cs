namespace MakeGrid3D.Solver.SparseModule
{
    using System.Collections.Generic;

    /// <summary>
    /// T матрица в разреженном столбцовом формате.
    /// Первые Nc столбцом образуют единичную подматрицу (Nc = кол-во регулряных узлов).
    /// </summary>
    public class TMatrix
    {
        #region Private Fields

        /// <summary>
        /// Количество столбцов матрицы.
        /// </summary>
        private int _n;

        /// <summary>
        /// Количество строк матрицы.
        /// </summary>
        private int _nc;

        #endregion Private Fields 

        #region Public Properties

        /// <summary>
        /// Целочисленный массив ig. 
        /// Элемент ig(k) равен индексу, с которого начинаются элементы (Nc + 1 + k)-го стобца (столбца) в массивах jg, gl и gu.
        /// ig(k + 1) - ig(k) равна количеству ненулевых елементов в k-ом столбце 
        /// Размерность ij равна N - Nc + 1.
        /// </summary>
        public List<int> Ig { get; set; }

        /// <summary>
        /// Целочисленный массив jg содержит номера строк ненулевых элементов
        /// Размерность jg равна размерности gl и gu.
        /// </summary>
        public List<int> Jg { get; set; }

        /// <summary>
        /// Ненулевые элементы матрицы (единичная подматрица сюда не входит)
        /// </summary>
        public List<double> Gg { get; set; }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Инициализирует <see cref="TMatrix"/>.
        /// </summary>
        /// <param name="n">Размерность матрицы.</param>
        /// <param name="nc">Количество регулярных узлов.</param>
        public TMatrix(int n, int nc)
        {
            _n = n;
            _nc = nc;
            Ig = new List<int>(n - nc + 1);
            Jg = new List<int>();
            Gg = new List<double>();
        }

        #endregion Constructors

        #region Public Indexers

        /// <summary>
        /// Возвращает элемент T[i][j].
        /// </summary>
        public double this[int i, int j]
        {
            get
            {
                if (i >= _nc || j >= _n || i < 0 || j < 0)
                    return 0;

                if (j >= _nc)
                {
                    int jc = j - _nc;
                    int jbeg = Ig[jc];
                    int jend = Ig[jc + 1] - 1;
                    int max_count = Ig[jc + 1] - Ig[jc];
                    int count = 0;
                    int med;
                    while (jbeg < Jg.Count && Jg[jbeg] != i && count <= max_count)
                    {
                        count++;
                        med = (jbeg + jend) / 2;
                        if (Jg[med] < i)
                            jbeg = med + 1;
                        else
                            jend = med;
                    }

                    if (count >= max_count || jbeg >= Jg.Count)
                        return 0;
                    return Gg[jbeg];
                }
                else 
                {
                    if (i == j)
                        return 1;
                    return 0;
                }
            }
        }

        #endregion Public Indexers

        #region Public Methods

        /// <summary>
        /// Умножает транспонированную матрицу T на вектор v.
        /// </summary>
        /// <returns>Вектор размерностью равным количеству стобцов T матрицы.
        /// Если не удалось выполнить операцию, то нулевой вектор.</returns>
        public List<double> TMultiplyByVector(List<double> v) 
        {
            List<double> result = new List<double>(_n);
            for (int i = 0; i < _n; i++)
                result.Add(0);

            if (v.Count != _nc)
                return result;

            for (int i = 0; i < _n; i++) 
            {
                if (i < _nc)
                    result[i] = v[i];
                else 
                {
                    for (int k = Ig[i - _nc]; k < Ig[i - _nc + 1]; k++) 
                    {
                        int j = Jg[k];
                        result[i] += Gg[k] * v[j];
                    }
                }
            }
            return result;
        }

        #endregion Public Methods
    }
}
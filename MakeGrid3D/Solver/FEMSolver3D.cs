using MakeGrid3D.Helpers;
using MakeGrid3D.Solver.SparseModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace MakeGrid3D.Solver
{
    /// <summary>
    /// Решатель метода конечных элементов трехмерный (с использованием T технологии). 
    /// </summary>
    public class FEMSolver3D
    {
        #region Private Fields

        /// <summary>
        /// Единственный экземпляр ленивого синглтона <see cref="FEMSolver3D"/>. 
        /// </summary>
        private static readonly Lazy<FEMSolver3D> _instance = new Lazy<FEMSolver3D>(() => new FEMSolver3D());

        /// <summary>
        /// Конечноэлементная сетка. 
        /// </summary>
        private Grid3D _grid;

        /// <summary>
        /// Глобальная матрица. 
        /// </summary>
        private SparseMatrix _matrix;

        /// <summary>
        /// T матрица. 
        /// </summary>
        private TMatrix _tMatrix;

        /// <summary>
        /// Параметры краевой задачи. 
        /// </summary>
        private FEMParams3D _params;

        /// <summary>
        /// Границы первого краевого условия. 
        /// </summary>
        private List<Boundary3D> _bc1;

        /// <summary>
        /// Границы второго краевого условия. 
        /// </summary>
        private List<Boundary3D> _bc2;

        /// <summary>
        /// Границы третьего краевого условия. 
        /// </summary>
        private List<Boundary3D> _bc3;

        /// <summary>
        /// Локальная матрицы жесткости. 
        /// </summary>
        private double[][] _g = new double[][]
                                {
                                new double[] {0, 0, 0, 0, 0, 0, 0, 0},
                                new double[] {0, 0, 0, 0, 0, 0, 0, 0},
                                new double[] {0, 0, 0, 0, 0, 0, 0, 0},
                                new double[] {0, 0, 0, 0, 0, 0, 0, 0},
                                new double[] {0, 0, 0, 0, 0, 0, 0, 0},
                                new double[] {0, 0, 0, 0, 0, 0, 0, 0},
                                new double[] {0, 0, 0, 0, 0, 0, 0, 0},
                                new double[] {0, 0, 0, 0, 0, 0, 0, 0}
                                };

        private double[][] _g1 = new double[][]
                                {
                                new double[] {1, -1},
                                new double[] {-1, 1}
                                };


        private double[][] _gx = new double[][]
                                {
                                new double[] {0, 0},
                                new double[] {0, 0}
                                };

        private double[][] _gy = new double[][]
                                {
                                new double[] {0, 0},
                                new double[] {0, 0}
                                };

        private double[][] _gz = new double[][]
                                {
                                new double[] {0, 0},
                                new double[] {0, 0}
                                };

        /// <summary>
        /// Локальная матрицы массы. 
        /// </summary>
        private double[][] _m = new double[][]
                                {
                                new double[] {0, 0, 0, 0, 0, 0, 0, 0},
                                new double[] {0, 0, 0, 0, 0, 0, 0, 0},
                                new double[] {0, 0, 0, 0, 0, 0, 0, 0},
                                new double[] {0, 0, 0, 0, 0, 0, 0, 0},
                                new double[] {0, 0, 0, 0, 0, 0, 0, 0},
                                new double[] {0, 0, 0, 0, 0, 0, 0, 0},
                                new double[] {0, 0, 0, 0, 0, 0, 0, 0},
                                new double[] {0, 0, 0, 0, 0, 0, 0, 0}
                                };

        
        private double[][] _m1 = new double[][]
                                {
                                new double[] {2.0/6.0, 1.0/6.0},
                                new double[] {1.0/6.0, 2.0/6.0}
                                };


        private double[][] _mx = new double[][]
                                {
                                new double[] {0, 0},
                                new double[] {0, 0}
                                };

        private double[][] _my = new double[][]
                                {
                                new double[] {0, 0},
                                new double[] {0, 0}
                                };

        private double[][] _mz = new double[][]
                                {
                                new double[] {0, 0},
                                new double[] {0, 0}
                                };

        /// <summary>
        /// Локальная матрица для учета второго и третьего краевых условий. 
        /// </summary>
        private double[][] _c = new double[][]
                                {
                                new double[] {4, 2, 2, 1},
                                new double[] {2, 4, 1, 2},
                                new double[] {2, 1, 4, 2},
                                new double[] {1, 2, 2, 4},
                                };

        /// <summary>
        /// Вектор правой части. 
        /// </summary>
        private List<double> _b;

        /// <summary>
        /// Размерность матрицы. 
        /// Для нерегулярной сетки размерность матрицы равна количеству регулярных элемент.
        /// </summary>
        private int _n;

        #endregion Private Fields

        #region Public Properties

        /// <inheritdoc cref="_instance"/>
        public static FEMSolver3D Instance => _instance.Value;

        #endregion Public Properties 

        #region Constructors

        private FEMSolver3D()
        {
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Инициализирует решатель. 
        /// </summary>
        /// <param name="grid">Конечноэлементная сетка.</param>
        /// <param name="gridParams">Параметры краевой задачи.</param>
        /// <param name="bc1">Список границ с первым к.у.</param>
        /// <param name="bc2">Список границ со вторым к.у.</param>
        /// <param name="bc3">Список границ с третьим к.у.</param>
        /// <returns>true, если успешно удалось проинициализировать решатель.</returns>
        public bool Initialize(Grid3D grid, FEMParams3D gridParams, List<Boundary3D> bc1, List<Boundary3D> bc2, List<Boundary3D> bc3)
        {
            _grid = grid;
            _params = gridParams;
            _bc1 = bc1;
            _bc2 = bc2;
            _bc3 = bc3;
            if (_grid.Nnodes > _grid.Nc)
            {
                GTree G = GenerateGTree();
                bool isSuccess = GenerateTMatrix(G);
                if (!isSuccess) return false;
            }

            _n = _grid.Nc;
            _b = new List<double>(_n);
            for (int i = 0; i < _n; i++)
                _b.Add(0);
            GeneratePortrait();
            return true;
        }

        /// <summary>
        /// Решает краевую задачу.
        /// </summary>
        /// <returns>Вектор решения.</returns>
        public List<double> Solve()
        {
            AssemblyG();
            AssemblyM();
            ApplyBc(2);
            ApplyBc(3);
            ApplyBc(1);

            //_matrix.Di[2] = 1;

            List<double> qc = LOSSolver.Instance.LOS_DI(_matrix, _b);
            if (_n == _grid.Nnodes)
                return qc;

            List<double> q = _tMatrix.TMultiplyByVector(qc);
            return q;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Создает портрет матрицы. 
        /// </summary>
        private void GeneratePortrait()
        {
            /// <summary>
            /// Добавляет номера глобальных узлов в массив global_nodes. 
            /// </summary>
            void AddNodes(int n, List<int> global_nodes)
            {
                if (n < _n)
                {
                    if (!global_nodes.Contains(n))
                        global_nodes.Add(n);
                }
                else
                {
                    int k = n - _n;
                    for (int i = _tMatrix.Ig[k]; i < _tMatrix.Ig[k + 1]; i++)
                        if (!global_nodes.Contains(_tMatrix.Jg[i]))
                            global_nodes.Add(_tMatrix.Jg[i]);
                }
            }

            List<List<int>> list = new List<List<int>>(_n);
            for (int i = 0; i < _n; i++)
                list.Add(new List<int>());
            list[0].Add(0);
            int g1, g2;  // Глобальные номера базисных функций
            bool not_in;
            List<int> global_nodes = new List<int>(); // Массив глобальных узлов элемента.
            // Цикл по конечным элементам
            foreach (Elem3D elem in _grid.Elems)
            {
                AddNodes(elem.n1, global_nodes);
                AddNodes(elem.n2, global_nodes);
                AddNodes(elem.n3, global_nodes);
                AddNodes(elem.n4, global_nodes);
                AddNodes(elem.n5, global_nodes);
                AddNodes(elem.n6, global_nodes);
                AddNodes(elem.n7, global_nodes);
                AddNodes(elem.n8, global_nodes);
                global_nodes.Sort();

                // Цикл по ненулевым базисным функциям
                for (int i_n = 0; i_n < global_nodes.Count; i_n++)
                {
                    g1 = global_nodes[i_n];
                    for (int j_n = i_n + 1; j_n < global_nodes.Count; j_n++)
                    {
                        // g2 > g1
                        g2 = global_nodes[j_n];
                        // Перед добавлением проверяем наличие элемента в списке
                        not_in = true;
                        for (int l = 0; l < list[g2].Count && not_in; l++)
                            if (g1 == list[g2][l])
                                not_in = false;

                        // Добавляем
                        if (not_in)
                            list[g2].Add(g1);
                    }
                }
                global_nodes.Clear();
            }


            // Сортировка списков по возрастанию
            for (int i = 0; i < _n; i++)
                list[i].Sort();

            _matrix = new SparseMatrix(_n);
            // Формирование вектора ig
            _matrix.Ig[0] = 0;
            for (int i = 0; i < list.Count; i++)
                _matrix.Ig[i + 1] = _matrix.Ig[i] + list[i].Count;

            for (int i = 1; i < _n + 1; i++)
                _matrix.Ig[i] -= 1;

            int ng = _matrix.Ig[_n];
            _matrix.Alloc(ng);
            // Формирование вектора jg
            for (int i = 1, j = 0; i < _n; i++)
                for (int k = 0; k < list[i].Count; k++, j++)
                    _matrix.Jg[j] = list[i][k];
        }

        /// <summary>
        /// Генерирует структуру данных G для построения T матрицы.
        /// </summary>
        /// <returns>Дерево G, где G[i] содержит пары (j, t), где 
        /// i - номер терминальная узла, j - номера регулярных узлов, которые лежат на ребре с узлом i,
        /// t - значение T[i][j] матрицы. </returns>
        private GTree GenerateGTree()
        {
            // Вначале нужно построить структуру данных G.
            GTree G = new();
            foreach (Elem3D elem in _grid.Elems)
            {
                foreach (int nc in elem.n_uc)
                {
                    // В 3D ключ может повторяться.
                    if (!G.ContainsKey(nc))
                    {
                        G.Add(nc, new List<(int, double)>());
                    }
                    int n1 = elem.n1;
                    int n2 = elem.n2;
                    int n3 = elem.n3;
                    int n4 = elem.n4;
                    int n5 = elem.n5;
                    int n6 = elem.n6;
                    int n7 = elem.n7;
                    int n8 = elem.n8;

                    double xc = _grid.XYZ[nc].X;
                    double yc = _grid.XYZ[nc].Y;
                    double zc = _grid.XYZ[nc].Z;
                    double xmin = _grid.XYZ[n1].X;
                    double xmax = _grid.XYZ[n8].X;
                    double ymin = _grid.XYZ[n1].Y;
                    double ymax = _grid.XYZ[n8].Y;
                    double zmin = _grid.XYZ[n1].Z;
                    double zmax = _grid.XYZ[n8].Z;
                    double hx = xmax - xmin;
                    double hy = ymax - ymin;
                    double hz = zmax - zmin;

                    // NOTE: В 3D терминальный узел может лежат на нескольких граней

                    // Узел лежит на передней грани.
                    if (MathsHelper.IsEqual(yc, ymin))
                    {
                        double psiX1 = BasicFunc1(xc, xmax, hx);
                        double psiX2 = BasicFunc2(xc, xmin, hx);
                        double psiZ1 = BasicFunc1(zc, zmax, hz);
                        double psiZ2 = BasicFunc2(zc, zmin, hz);
                        double Telem1 = psiX1 * psiZ1;
                        double Telem2 = psiX2 * psiZ1;
                        double Telem3 = psiX1 * psiZ2;
                        double Telem4 = psiX2 * psiZ2;

                        if (!MathsHelper.IsEqual(Telem1, 0) && !G[nc].Any(tuple => tuple.Item1 == n1))
                        {
                            G[nc].Add((n1, Telem1));
                        }
                        if (!MathsHelper.IsEqual(Telem2, 0) && !G[nc].Any(tuple => tuple.Item1 == n2))
                        {
                            G[nc].Add((n2, Telem2));
                        }
                        if (!MathsHelper.IsEqual(Telem3, 0) && !G[nc].Any(tuple => tuple.Item1 == n5))
                        {
                            G[nc].Add((n5, Telem3));
                        }
                        if (!MathsHelper.IsEqual(Telem4, 0) && !G[nc].Any(tuple => tuple.Item1 == n6))
                        {
                            G[nc].Add((n6, Telem4));
                        }
                    }

                    // Узел лежит на правой грани.
                    if (MathsHelper.IsEqual(xc, xmax))
                    {
                        double psiY1 = BasicFunc1(yc, ymax, hy);
                        double psiY2 = BasicFunc2(yc, ymin, hy);
                        double psiZ1 = BasicFunc1(zc, zmax, hz);
                        double psiZ2 = BasicFunc2(zc, zmin, hz);
                        double Telem1 = psiY1 * psiZ1;
                        double Telem2 = psiY2 * psiZ1;
                        double Telem3 = psiY1 * psiZ2;
                        double Telem4 = psiY2 * psiZ2;

                        if (!MathsHelper.IsEqual(Telem1, 0) && !G[nc].Any(tuple => tuple.Item1 == n2))
                        {
                            G[nc].Add((n2, Telem1));
                        }
                        if (!MathsHelper.IsEqual(Telem2, 0) && !G[nc].Any(tuple => tuple.Item1 == n4))
                        {
                            G[nc].Add((n4, Telem2));
                        }
                        if (!MathsHelper.IsEqual(Telem3, 0) && !G[nc].Any(tuple => tuple.Item1 == n6))
                        {
                            G[nc].Add((n6, Telem3));
                        }
                        if (!MathsHelper.IsEqual(Telem4, 0) && !G[nc].Any(tuple => tuple.Item1 == n8))
                        {
                            G[nc].Add((n8, Telem4));
                        }
                    }

                    // Узел лежит на задней грани.
                    if (MathsHelper.IsEqual(yc, ymax))
                    {
                        double psiX1 = BasicFunc1(xc, xmax, hx);
                        double psiX2 = BasicFunc2(xc, xmin, hx);
                        double psiZ1 = BasicFunc1(zc, zmax, hz);
                        double psiZ2 = BasicFunc2(zc, zmin, hz);
                        double Telem1 = psiX1 * psiZ1;
                        double Telem2 = psiX2 * psiZ1;
                        double Telem3 = psiZ1 * psiZ2;
                        double Telem4 = psiZ2 * psiZ2;

                        if (!MathsHelper.IsEqual(Telem1, 0) && !G[nc].Any(tuple => tuple.Item1 == n2))
                        {
                            G[nc].Add((n2, Telem1));
                        }
                        if (!MathsHelper.IsEqual(Telem2, 0) && !G[nc].Any(tuple => tuple.Item1 == n3))
                        {
                            G[nc].Add((n3, Telem2));
                        }
                        if (!MathsHelper.IsEqual(Telem3, 0) && !G[nc].Any(tuple => tuple.Item1 == n7))
                        {
                            G[nc].Add((n7, Telem3));
                        }
                        if (!MathsHelper.IsEqual(Telem4, 0) && !G[nc].Any(tuple => tuple.Item1 == n8))
                        {
                            G[nc].Add((n8, Telem4));
                        }
                    }

                    // Узел лежит на левой грани.
                    if (MathsHelper.IsEqual(xc, xmin))
                    {
                        double psiY1 = BasicFunc1(yc, ymax, hy);
                        double psiY2 = BasicFunc2(yc, ymin, hy);
                        double psiZ1 = BasicFunc1(zc, zmax, hz);
                        double psiZ2 = BasicFunc2(zc, zmin, hz);
                        double Telem1 = psiY1 * psiZ1;
                        double Telem2 = psiY2 * psiZ1;
                        double Telem3 = psiZ1 * psiZ2;
                        double Telem4 = psiZ2 * psiZ2;

                        if (!MathsHelper.IsEqual(Telem1, 0) && !G[nc].Any(tuple => tuple.Item1 == n1))
                        {
                            G[nc].Add((n1, Telem1));
                        }
                        if (!MathsHelper.IsEqual(Telem2, 0) && !G[nc].Any(tuple => tuple.Item1 == n3))
                        {
                            G[nc].Add((n3, Telem2));
                        }
                        if (!MathsHelper.IsEqual(Telem3, 0) && !G[nc].Any(tuple => tuple.Item1 == n5))
                        {
                            G[nc].Add((n5, Telem3));
                        }
                        if (!MathsHelper.IsEqual(Telem4, 0) && !G[nc].Any(tuple => tuple.Item1 == n7))
                        {
                            G[nc].Add((n7, Telem4));
                        }
                    }

                    // Узел лежит на нижней грани.
                    if (MathsHelper.IsEqual(zc, zmin))
                    {
                        double psiX1 = BasicFunc1(xc, xmax, hx);
                        double psiX2 = BasicFunc2(xc, xmin, hx);
                        double psiY1 = BasicFunc1(yc, ymax, hy);
                        double psiY2 = BasicFunc2(yc, ymin, hy);
                        double Telem1 = psiX1 * psiY1;
                        double Telem2 = psiX2 * psiY1;
                        double Telem3 = psiX1 * psiY2;
                        double Telem4 = psiX2 * psiY2;

                        if (!MathsHelper.IsEqual(Telem1, 0) && !G[nc].Any(tuple => tuple.Item1 == n1))
                        {
                            G[nc].Add((n1, Telem1));
                        }
                        if (!MathsHelper.IsEqual(Telem2, 0) && !G[nc].Any(tuple => tuple.Item1 == n2))
                        {
                            G[nc].Add((n2, Telem2));
                        }
                        if (!MathsHelper.IsEqual(Telem3, 0) && !G[nc].Any(tuple => tuple.Item1 == n3))
                        {
                            G[nc].Add((n3, Telem3));
                        }
                        if (!MathsHelper.IsEqual(Telem4, 0) && !G[nc].Any(tuple => tuple.Item1 == n4))
                        {
                            G[nc].Add((n4, Telem4));
                        }
                    }

                    // Узел лежит на верхней грани.
                    if (MathsHelper.IsEqual(zc, zmax))
                    {
                        double psiX1 = BasicFunc1(xc, xmax, hx);
                        double psiX2 = BasicFunc2(xc, xmin, hx);
                        double psiY1 = BasicFunc1(yc, ymax, hy);
                        double psiY2 = BasicFunc2(yc, ymin, hy);
                        double Telem1 = psiX1 * psiY1;
                        double Telem2 = psiX2 * psiY1;
                        double Telem3 = psiX1 * psiY2;
                        double Telem4 = psiX2 * psiY2;

                        if (!MathsHelper.IsEqual(Telem1, 0) && !G[nc].Any(tuple => tuple.Item1 == n5))
                        {
                            G[nc].Add((n5, Telem1));
                        }
                        if (!MathsHelper.IsEqual(Telem2, 0) && !G[nc].Any(tuple => tuple.Item1 == n6))
                        {
                            G[nc].Add((n6, Telem2));
                        }
                        if (!MathsHelper.IsEqual(Telem3, 0) && !G[nc].Any(tuple => tuple.Item1 == n7))
                        {
                            G[nc].Add((n7, Telem3));
                        }
                        if (!MathsHelper.IsEqual(Telem4, 0) && !G[nc].Any(tuple => tuple.Item1 == n8))
                        {
                            G[nc].Add((n8, Telem4));
                        }
                    }
                }
            }
            return G;
        }

        /// <summary>
        /// Генерирует T матрицу.
        /// </summary>
        /// <param name="G">Структура данных G</param>
        /// <returns>true, если удалось успено создать матрицу.</returns>
        private bool GenerateTMatrix(GTree G)
        {
            _tMatrix = new TMatrix(_grid.Nnodes, _grid.Nc);
            _tMatrix.Ig.Add(0);
            // Обработанные узлы.
            HashSet<int> processed_nodes = new();
            // Массив, содержащий пары элементов jg и gg.
            List<(int, double)> jg_gg = new();
            for (int j = _grid.Nc; j < _grid.Nnodes; j++)
            {
                processed_nodes.Clear();
                jg_gg.Clear();
                int elems_count = 0; // Количество элементов с столбце.
                double m = 1;
                bool isSuccess = GenerateTMatrixChain(G, j, m, ref elems_count, jg_gg, processed_nodes);
                if (!isSuccess)
                    return false;

                jg_gg.Sort((x, y) => x.Item1.CompareTo(y.Item1));
                foreach ((int, double) pair in jg_gg)
                {
                    _tMatrix.Jg.Add(pair.Item1);
                    _tMatrix.Gg.Add(pair.Item2);
                }

                int igCount = _tMatrix.Ig.Count;
                int igElem = elems_count + _tMatrix.Ig[igCount - 1];
                _tMatrix.Ig.Add(igElem);
            }
            return true;
        }

        /// <summary>
        /// Рекурсивная итерация генерации T матрицы.
        /// </summary>
        /// <param name="G">Структура данных G.</param>
        /// <param name="j">Номер терминального узла.</param>
        /// <param name="m">Ячейка памяти, хранящаяя произведение текущей цепочки.</param>
        /// <param name="elems_count">Счетчик количества элементов в столбце.</param>
        /// <param name="elems_count">Список обработанных узлов.</param>
        /// <param name="ig_gg">Массив, содержаший пары элементов массивов jg и gg</param>
        /// <returns>true, если удалось успешно создать цепочку.</returns>
        private bool GenerateTMatrixChain(GTree G, int j, double m, ref int elems_count, List<(int, double)> jg_gg, HashSet<int> processed_nodes)
        {
            foreach ((int, double) treeNode in G[j])
            {
                int i = treeNode.Item1;
                double Telem = treeNode.Item2;

                if (i < _grid.Nc)
                {
                    if (!processed_nodes.Contains(i))
                    {
                        jg_gg.Add((i, m * Telem));
                        elems_count++;
                    }
                    processed_nodes.Add(i);
                }
                else
                {
                    if (processed_nodes.Contains(i))
                        return false;
                    processed_nodes.Add(i);
                    bool isSuccess = GenerateTMatrixChain(G, i, m * Telem, ref elems_count, jg_gg, processed_nodes);
                    if (!isSuccess) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Первая локальная базисная функция X1. 
        /// </summary>
        /// <param name="x">Переменное значение.</param>
        /// <param name="xmax">Максимальное значение x на элементе.</param>
        /// <param name="h">Длина значения по x</param>
        private double BasicFunc1(double x, double xmax, double h)
        {
            return (xmax - x) / h;
        }

        /// <summary>
        /// Вторая локальная базисная функция X2. 
        /// </summary>
        /// <param name="x">Переменное значение.</param>
        /// <param name="xmin">Минимальное значение x на элементе.</param>
        /// <param name="h">Длина значения по x</param>
        private double BasicFunc2(double x, double xmin, double h)
        {
            return (x - xmin) / h;
        }

        private int mu(int i)
        {
            i++;
            return ((i - 1) % 2) + 1 - 1;
        }
        private int nu(int i)
        {
            i++;
            return (((i - 1) / 2) % 2) + 1 - 1;
        }
        private int v(int i)
        {
            i++;
            return (i - 1) / 4 + 1 - 1;
        }

        /// <summary>
        /// Добавляет элемент локальной матрицы в глобальную с учётом T матрицы. 
        /// </summary>
        /// <param name="a">Элемент локальной матрицы.</param>
        /// <param name="i">Глобальный номер, соответствующей строке.</param>
        /// <param name="j">Глобальный номер, соответствующей столбцу.</param>
        private void AddLocalMatrixElement(double a, int i, int j)
        {
            if (i < _n && j < _n)
            {
                AddToGlobalMatrix(a, i, j);
            }
            else if (i >= _n && j < _n)
            {
                i = i - _n;
                for (int mu = _tMatrix.Ig[i]; mu < _tMatrix.Ig[i + 1]; mu++)
                    AddToGlobalMatrix(a * _tMatrix.Gg[mu], _tMatrix.Jg[mu], j);
            }
            else if (i < _n && j >= _n)
            {
                j = j - _n;
                for (int nu = _tMatrix.Ig[j]; nu < _tMatrix.Ig[j + 1]; nu++)
                    AddToGlobalMatrix(a * _tMatrix.Gg[nu], i, _tMatrix.Jg[nu]);
            }
            else
            {
                i = i - _n;
                j = j - _n;
                for (int mu = _tMatrix.Ig[i]; mu < _tMatrix.Ig[i + 1]; mu++)
                    for (int nu = _tMatrix.Ig[j]; nu < _tMatrix.Ig[j + 1]; nu++)
                        AddToGlobalMatrix(a * _tMatrix.Gg[nu] * _tMatrix.Gg[mu], _tMatrix.Jg[mu], _tMatrix.Jg[nu]);
            }
        }

        /// <summary>
        /// Добавляет значение a в элемент Aij глобальной матрицы. 
        /// </summary>
        private void AddToGlobalMatrix(double a, int i, int j)
        {
            if (i == j)
            {
                _matrix.Di[i] += a;
                return;
            }

            int n1, n2;
            if (i > j)
            {
                n1 = i;
                n2 = j;
            }
            else
            {
                n1 = j;
                n2 = i;
            }

            int beg = _matrix.Ig[n1];
            int end = _matrix.Ig[n1 + 1] - 1;
            if (end < 0)
                return;

            int max_count = _matrix.Ig[n1 + 1] - _matrix.Ig[n1];
            int count = 0;
            int med;
            while (beg < _matrix.Jg.Count && _matrix.Jg[beg] != n2 && count <= max_count)
            {
                count++;
                med = (beg + end) / 2;
                if (_matrix.Jg[med] < n2)
                    beg = med + 1;
                else
                    end = med;
            }

            if (count >= max_count || beg >= _matrix.Jg.Count)
                return;

            if (i > j)
                _matrix.Gl[beg] += a;
            else
                _matrix.Gu[beg] += a;
        }

        /// <summary>
        /// Собирает матрицу жесткости и добавляет её в глобальную матрицу. 
        /// </summary>
        private void AssemblyG()
        {
            foreach (Elem3D elem in _grid.Elems)
            {
                double x1 = _grid.XYZ[elem.n1].X;
                double x2 = _grid.XYZ[elem.n8].X;
                double y1 = _grid.XYZ[elem.n1].Y;
                double y2 = _grid.XYZ[elem.n8].Y;
                double z1 = _grid.XYZ[elem.n1].Z;
                double z2 = _grid.XYZ[elem.n8].Z;
                double hx = x2 - x1;
                double hy = y2 - y1;
                double hz = z2 - z1;
                double lambda = _params.Lambda(elem.wi);

                for (int il = 0; il < 2; il++)
                    for (int jl = 0; jl < 2; jl++)
                    {
                        _gx[il][jl] = _g1[il][jl] / hx;
                        _gy[il][jl] = _g1[il][jl] / hy;
                        _gz[il][jl] = _g1[il][jl] / hz;
                        _mx[il][jl] = _m1[il][jl] * hx;
                        _my[il][jl] = _m1[il][jl] * hy;
                        _mz[il][jl] = _m1[il][jl] * hz;
                    }

                for (int il = 0; il < 8; il++)
                    for (int jl = 0; jl < 8; jl++)
                    {
                        _g[il][jl] = lambda * (_gx[mu(il)][mu(jl)] * _my[nu(il)][nu(jl)] * _mz[v(il)][v(jl)] +
                                            _mx[mu(il)][mu(jl)] * _gy[nu(il)][nu(jl)] * _mz[v(il)][v(jl)] +
                                            _mx[mu(il)][mu(jl)] * _my[nu(il)][nu(jl)] * _gz[v(il)][v(jl)]);
                    }

                int[] global_nodes = { elem.n1, elem.n2, elem.n3, elem.n4, elem.n5, elem.n6, elem.n7, elem.n8 };
                for (int i = 0; i < 8; i++)
                    for (int j = 0; j < 8; j++)
                        AddLocalMatrixElement(_g[i][j], global_nodes[i], global_nodes[j]);
            }
        }

        /// <summary>
        /// Собирает матрицу масс и добавляет её в глобальную матрицу. 
        /// </summary>
        private void AssemblyM()
        {
            foreach (Elem3D elem in _grid.Elems)
            {
                double x1 = _grid.XYZ[elem.n1].X;
                double x2 = _grid.XYZ[elem.n8].X;
                double y1 = _grid.XYZ[elem.n1].Y;
                double y2 = _grid.XYZ[elem.n8].Y;
                double z1 = _grid.XYZ[elem.n1].Z;
                double z2 = _grid.XYZ[elem.n8].Z;
                double hx = x2 - x1;
                double hy = y2 - y1;
                double hz = z2 - z1;
                double sigma = _params.Sigma(elem.wi);

                for (int il = 0; il < 2; il++)
                    for (int jl = 0; jl < 2; jl++)
                    {
                        _mx[il][jl] = _m1[il][jl] * hx;
                        _my[il][jl] = _m1[il][jl] * hy;
                        _mz[il][jl] = _m1[il][jl] * hz;
                    }

                for (int il = 0; il < 8; il++)
                    for (int jl = 0; jl < 8; jl++)
                    {
                        _m[il][jl] = sigma * (_mx[mu(il)][mu(jl)] * _my[nu(il)][nu(jl)] * _mz[v(il)][v(jl)]);
                    }

                int[] global_nodes = { elem.n1, elem.n2, elem.n3, elem.n4, elem.n5, elem.n6, elem.n7, elem.n8 };
                for (int i = 0; i < 8; i++)
                    for (int j = 0; j < 8; j++)
                        AddLocalMatrixElement(_m[i][j], global_nodes[i], global_nodes[j]);

                double f1 = _params.F(elem.wi, x1, y1, z1);
                double f2 = _params.F(elem.wi, x2, y1, z1);
                double f3 = _params.F(elem.wi, x1, y2, z1);
                double f4 = _params.F(elem.wi, x2, y2, z1);
                double f5 = _params.F(elem.wi, x1, y1, z2);
                double f6 = _params.F(elem.wi, x2, y1, z2);
                double f7 = _params.F(elem.wi, x1, y2, z2);
                double f8 = _params.F(elem.wi, x2, y2, z2);

                for (int i_node = 0; i_node < 8; i_node++)
                {
                    int i;
                    if (global_nodes[i_node] < _n)
                    {
                        i = global_nodes[i_node];
                        _b[i] += (_m[i_node][0] * f1 + _m[i_node][1] * f2 +_m[i_node][2] * f3 + _m[i_node][3] * f4 + _m[i_node][4] * f5 + _m[i_node][5] * f6 + _m[i_node][6] * f7 + _m[i_node][7] * f8) / sigma;
                    }
                    else
                    {
                        int k = global_nodes[i_node] - _n;
                        for (int j = _tMatrix.Ig[k]; j < _tMatrix.Ig[k + 1]; j++)
                        {
                            i = _tMatrix.Jg[j];
                            _b[i] += (_m[i_node][0] * f1 + _m[i_node][1] * f2 + _m[i_node][2] * f3 + _m[i_node][3] * f4 + _m[i_node][4] * f5 + _m[i_node][5] * f6 + _m[i_node][6] * f7 + _m[i_node][7] * f8) / sigma;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Применяет краевое условие для узла. 
        /// </summary>
        /// <param name="bcNum">Номер краевого условия (1,2,3).</param>
        private void ApplyBc(int bcNum)
        {
            switch (bcNum)
            {
                case 1:
                    foreach (Boundary3D boundary in _bc1)
                        ApplyBc1(boundary);
                    break;
                case 2:
                    foreach (Boundary3D boundary in _bc2)
                        ApplyBc2Node(boundary);
                    break;
                case 3:
                    foreach (Boundary3D boundary in _bc3)
                        ApplyBc3Node(boundary);
                    break;
                default:
                    LogService.LogWarning("Указано неверное краевое условие");
                    break;
            }
        }

        /// <summary>
        /// Применяет первое краевое условие. 
        /// </summary>
        private void ApplyBc1(Boundary3D boundary)
        {
            int p = boundary.Si;
            List<int> global_nodes = new List<int>() { boundary.N1, boundary.N2, boundary.N3, boundary.N4};
            foreach (int l in global_nodes)
            {
                if (l >= _n)
                    continue;
                double x = _grid.XYZ[l].X;
                double y = _grid.XYZ[l].Y;
                double z = _grid.XYZ[l].Z;
                _matrix.Di[l] = 1;
                for (int s = _matrix.Ig[l]; s < _matrix.Ig[l + 1]; s++)
                    _matrix.Gl[s] = 0;
                for (int s = 0; s < _matrix.Ng; s++)
                    if (_matrix.Jg[s] == l)
                        _matrix.Gu[s] = 0;
                _b[l] = _params.Ug(p, x, y, z);
            }
        }

        /// <summary>
        /// Применяет второе краевое условие. 
        /// </summary>
        private void ApplyBc2Node(Boundary3D boundary)
        {
            int p = boundary.Si;
            int l1 = boundary.N1;
            int l2 = boundary.N2;
            int l3 = boundary.N3;
            int l4 = boundary.N4;

            double x1 = _grid.XYZ[l1].X;
            double x2 = _grid.XYZ[l4].X;
            double y1 = _grid.XYZ[l1].Y;
            double y2 = _grid.XYZ[l4].Y;
            double z1 = _grid.XYZ[l1].Z;
            double z2 = _grid.XYZ[l4].Z;

            double theta1 = 0;
            double theta2 = 0;
            double theta3 = 0;
            double theta4 = 0;
            double area = 0;

            if (MathsHelper.IsEqual(x1, x2))
            {
                theta1 = _params.Theta(p, x1, y1, z1);
                theta2 = _params.Theta(p, x2, y1, z1);
                theta3 = _params.Theta(p, x1, y2, z1);
                theta4 = _params.Theta(p, x2, y2, z1);
                area = (x2 - x1) * (y2 - y1);
            }
            else if (MathsHelper.IsEqual(y1, y2))
            {
                theta1 = _params.Theta(p, x1, y1, z1);
                theta2 = _params.Theta(p, x1, y2, z1);
                theta3 = _params.Theta(p, x1, y1, z2);
                theta4 = _params.Theta(p, x1, y2, z2);
                area = (y2 - y1) * (z2 - z1);
            }
            else
            {
                theta1 = _params.Theta(p, x1, y1, z1);
                theta2 = _params.Theta(p, x2, y1, z1);
                theta3 = _params.Theta(p, x1, y1, z2);
                theta4 = _params.Theta(p, x2, y1, z2);
                area = (x2 - x1) * (z2 - z1);
            }

            int[] global_nodes = new int[4] {l1,l2,l3,l4 };
            for (int l = 0; l < 4; l++)
                _b[global_nodes[l]] += area * (theta1 * _c[l][0] + theta2 * _c[l][1] + theta3 * _c[l][2] + theta4 * _c[l][3]) / 36.0;
        }

        /// <summary>
        /// Применяет третье краевое условие. 
        /// </summary>
        private void ApplyBc3Node(Boundary3D boundary)
        {
            int p = boundary.Si;
            int l1 = boundary.N1;
            int l2 = boundary.N2;
            int l3 = boundary.N3;
            int l4 = boundary.N4;

            double x1 = _grid.XYZ[l1].X;
            double x2 = _grid.XYZ[l4].X;
            double y1 = _grid.XYZ[l1].Y;
            double y2 = _grid.XYZ[l4].Y;
            double z1 = _grid.XYZ[l1].Z;
            double z2 = _grid.XYZ[l4].Z;

            double ubeta1 = 0;
            double ubeta2 = 0;
            double ubeta3 = 0;
            double ubeta4 = 0;
            double area = 0;
            double beta = _params.Beta(p);

            if (MathsHelper.IsEqual(x1, x2))
            {
                ubeta1 = _params.Theta(p, x1, y1, z1);
                ubeta2 = _params.Theta(p, x2, y1, z1);
                ubeta3 = _params.Theta(p, x1, y2, z1);
                ubeta4 = _params.Theta(p, x2, y2, z1);
                area = (x2 - x1) * (y2 - y1);
            }
            else if (MathsHelper.IsEqual(y1, y2))
            {
                ubeta1 = _params.Theta(p, x1, y1, z1);
                ubeta2 = _params.Theta(p, x1, y2, z1);
                ubeta3 = _params.Theta(p, x1, y1, z2);
                ubeta4 = _params.Theta(p, x1, y2, z2);
                area = (y2 - y1) * (z2 - z1);
            }
            else
            {
                ubeta1 = _params.Theta(p, x1, y1, z1);
                ubeta2 = _params.Theta(p, x2, y1, z1);
                ubeta3 = _params.Theta(p, x1, y1, z2);
                ubeta4 = _params.Theta(p, x2, y1, z2);
                area = (x2 - x1) * (z2 - z1);
            }

            int[] global_nodes = new int[4] { l1, l2, l3, l4 };

            for (int ic = 0; ic < 4; ic++)
                for (int jc = 0; jc < 4; jc++)
                    AddLocalMatrixElement(_c[ic][jc] * area * beta / 36.0, global_nodes[ic], global_nodes[jc]);

           
            for (int l = 0; l < 4; l++)
                _b[global_nodes[l]] += beta * area * (ubeta1 * _c[l][0] + ubeta2 * _c[l][1] + ubeta3 * _c[l][2] + ubeta4 * _c[l][3]) / 36.0;
        }

        #endregion Private Methods
    }
}
using MakeGrid3D.Helpers;
using MakeGrid3D.Solver.SparseModule;
using System;
using System.Collections.Generic;
using System.Linq;

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
        /// Список локальный матриц масс. 
        /// </summary>
        private List<double[][]> _allM;

        /// <summary>
        /// Локальная матрицы масс. 
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
        /// Список временных слоев. 
        /// </summary>
        private List<double> _t;

        /// <summary>
        /// Вектор решения на (i - 3) временном слое.
        /// Размерность равна кол-во регулярных узлов.
        /// </summary>
        private List<double> q_3;

        /// <summary>
        /// Вектор решения на (i - 2) временном слое. 
        /// Размерность равна кол-во регулярных узлов.
        /// </summary>
        private List<double> q_2;

        /// <summary>
        /// Вектор решения на (i - 1) временном слое. 
        /// Размерность равна кол-во регулярных узлов.
        /// </summary>
        private List<double> q_1;

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
        /// <param name="t">Список временных слоев.</param>
        /// <returns>true, если успешно удалось проинициализировать решатель.</returns>
        public bool Initialize(Grid3D grid, FEMParams3D gridParams, List<Boundary3D> bc1, List<Boundary3D> bc2, List<Boundary3D> bc3, List<double> t)
        {
            _grid = grid;
            _params = gridParams;
            _bc1 = bc1;
            _bc2 = bc2;
            _bc3 = bc3;
            _t = t;
            _n = _grid.Nc;
            _b = new List<double>(_n);
            for (int i = 0; i < _n; i++)
                _b.Add(0);

            if (_grid.Nnodes > _n)
            {
                GTree G = GenerateGTree();
                bool isSuccess = GenerateTMatrix(G);
                if (!isSuccess) return false;
            }

            GeneratePortrait();
            GenerateMmatrices();
            InitPrevQ_3_2();
            q_1 = SolveBy3Layers(_t[0], _t[1], _t[2]);
            return true;
        }

        /// <summary>
        /// Решает краевую задачу.
        /// </summary>
        /// <returns>Вектор решения.</returns>
        public List<List<double>> Solve()
        {
            List<List<double>> Q = new List<List<double>>(_t.Count)
            {
                q_3,
                q_2,
                q_1
            };

            for (int i = 3; i < _t.Count; i++)
            {
                ResetToZero();
                double t_3 = _t[i - 3];
                double t_2 = _t[i - 2];
                double t_1 = _t[i - 1];
                double t = _t[i];

                double delta_t = t - t_2;
                double delta_t1 = t_1 - t_2;
                double delta_t0 = t - t_1;
                double delta_t2 = t_2 - t_3;
                double delta_t3 = t_1 - t_3;
                double delta_t4 = t - t_3;
                double d1_eta3 = -(delta_t0 * delta_t) / (delta_t2 * delta_t3 * delta_t4);
                double d1_eta2 = (delta_t0 * delta_t4) / (delta_t * delta_t1 * delta_t2);
                double d1_eta1 = -(delta_t4 * delta_t) / (delta_t0 * delta_t1 * delta_t3);
                double d1_eta0 = (delta_t * delta_t4 + delta_t0 * delta_t4 + delta_t0 * delta_t) / (delta_t * delta_t0 * delta_t4);
                double d2_eta3 = -2 * (2 * t - t_2 - t_1) / (delta_t2 * delta_t3 * delta_t4);
                double d2_eta2 = 2 * (2 * t - t_3 - t_1) / (delta_t * delta_t1 * delta_t2);
                double d2_eta1 = -2 * (2 * t - t_3 - t_2) / (delta_t0 * delta_t1 * delta_t3);
                double d2_eta0 = 2 * (3 * t - t_3 - t_2 - t_1) / (delta_t * delta_t0 * delta_t4);

                AssemblyG();
                AssemblyM(1, _params.Gamma);
                AssemblyM(d1_eta0, _params.Sigma);
                AssemblyM(d2_eta0, _params.Chi);
                AssemblyB(t, true);
                AssemblyB(t, false, -d1_eta1, _params.Sigma, q_1);
                AssemblyB(t, false, -d1_eta2, _params.Sigma, q_2);
                AssemblyB(t, false, -d1_eta3, _params.Sigma, q_3);
                AssemblyB(t, false, -d2_eta1, _params.Chi, q_1);
                AssemblyB(t, false, -d2_eta2, _params.Chi, q_2);
                AssemblyB(t, false, -d2_eta3, _params.Chi, q_3);

                ApplyBc(2, t);
                ApplyBc(3, t);
                ApplyBc(1, t);

                //_matrix.Di[2] = 1;

                List<double> qc = LOSSolver.Instance.LOS_DI(_matrix, _b);
                List<double> q;
                if (_n == _grid.Nnodes)
                {
                    Q.Add(qc);
                    q = qc;
                }
                else
                {
                    q = _tMatrix.TMultiplyByVector(qc);
                    Q.Add(q);
                }

                if (i < _t.Count - 1)
                {
                    q_3 = new List<double>(q_2);
                    q_2 = new List<double>(q_1);
                    q_1 = new List<double>(q);
                }
            }
            return Q;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Вычисляет значения на предыдущих двух временных слоях (для 1-ой итерации). 
        /// </summary>
        private void InitPrevQ_3_2()
        {
            q_3 = new List<double>(_grid.Nnodes);
            q_2 = new List<double>(_grid.Nnodes);
            for (int i = 0; i < _grid.Nnodes; i++)
            {
                q_3.Add(0);
                q_2.Add(0);
            }

            for (int i = 0; i < _grid.Nnodes; i++)
            {
                double x = _grid.XYZ[i].X;
                double y = _grid.XYZ[i].Y;
                double z = _grid.XYZ[i].Z;
                int p = _grid.Area.FindSubArea(x, x, y, y, z, z);
                if (p < -1)
                    continue;

                double u0 = _params.U0(p, x, y, z);
                q_3[i] = u0;

                if (_params.U1 != null)
                {
                    double u1 = _params.U1(p, x, y, z);
                    q_2[i] = u1;
                }
                else
                {
                    double d_u1 = _params.DU0(p, x, y, z);
                    q_2[i] = u0 + d_u1 * (_t[1] - _t[0]);
                }
            }
        }

        /// <summary>
        /// Вычисляет вектор решения через трехслойную схему.
        /// </summary>
        private List<double> SolveBy3Layers(double t_2, double t_1, double t)
        {
            double delta_t = t - t_2;
            double delta_t1 = t_1 - t_2;
            double delta_t0 = t - t_1;

            AssemblyG();
            AssemblyM(1, _params.Gamma);
            AssemblyM(2 / (delta_t * delta_t0), _params.Chi);
            AssemblyM((delta_t + delta_t0) / (delta_t * delta_t0), _params.Sigma);
            AssemblyB(t, true);
            AssemblyB(t, false, -2 / (delta_t1 * delta_t), _params.Chi, q_3);
            AssemblyB(t, false, 2 / (delta_t1 * delta_t0), _params.Chi, q_2);
            AssemblyB(t, false, -delta_t0 / (delta_t1 * delta_t), _params.Sigma, q_3);
            AssemblyB(t, false, delta_t / (delta_t1 * delta_t0), _params.Sigma, q_2);
            ApplyBc(2, t);
            ApplyBc(3, t);
            ApplyBc(1, t);

            //_matrix.Di[2] = 1;

            List<double> qc = LOSSolver.Instance.LOS_DI(_matrix, _b);
            if (_n == _grid.Nnodes)
                return qc;
            else
                return _tMatrix.TMultiplyByVector(qc);
        }

        /// <summary>
        /// Обнуляет значение матрицы и вектора правой части. 
        /// </summary>
        private void ResetToZero()
        {
            for (int i = 0; i < _n; i++)
            {
                _b[i] = 0;
                _matrix.Di[i] = 0;
            }
            for (int i = 0; i < _matrix.Ng; i++)
            {
                _matrix.Gl[i] = 0;
                _matrix.Gu[i] = 0;
            }
        }

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
                        double Telem3 = psiX1 * psiZ2;
                        double Telem4 = psiX2 * psiZ2;

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
                        double Telem3 = psiY1 * psiZ2;
                        double Telem4 = psiY2 * psiZ2;

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
                        processed_nodes.Add(i);
                    }
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
        /// Собирает глобальную матрицу масс. 
        /// </summary>
        private void GenerateMmatrices() 
        {
            _allM = new List<double[][]>(_grid.Nelems);
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

                for (int il = 0; il < 2; il++)
                    for (int jl = 0; jl < 2; jl++)
                    {
                        _mx[il][jl] = _m1[il][jl] * hx;
                        _my[il][jl] = _m1[il][jl] * hy;
                        _mz[il][jl] = _m1[il][jl] * hz;
                    }

                double[][] m = new double[][]
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

                for (int il = 0; il < 8; il++)
                    for (int jl = 0; jl < 8; jl++)
                    {
                        m[il][jl] = _mx[mu(il)][mu(jl)] * _my[nu(il)][nu(jl)] * _mz[v(il)][v(jl)];
                    }
                _allM.Add(m);
            }
        }

        /// <summary>
        /// Собирает матрицу масс и добавляет её в глобальную матрицу. 
        /// </summary>
        /// <param name="m">Множитель.</param>
        /// <param name="param">Функция параметра.</param>
        private void AssemblyM(double m, Function param)
        {
            for (int k = 0; k < _grid.Nelems; k++)
            {
                Elem3D elem = _grid.Elems[k];
                double gamma = param(elem.wi);
                for (int i = 0; i < 8; i++)
                    for (int j = 0; j < 8; j++)
                    {
                        _m[i][j] = m * gamma * _allM[k][i][j];
                    }

                int[] global_nodes = { elem.n1, elem.n2, elem.n3, elem.n4, elem.n5, elem.n6, elem.n7, elem.n8 };
                for (int i = 0; i < 8; i++)
                    for (int j = 0; j < 8; j++)
                        AddLocalMatrixElement(_m[i][j], global_nodes[i], global_nodes[j]);
            }
        }

        /// <summary>
        /// Собирает вектор правой части. 
        /// </summary>
        /// <param name="t">Значение на текущем временном слое.</param>
        /// <param name="isF">Сборка происходит через вектор f, иначе через q_i. Если isF = true, то умножается на 1</param>
        /// <param name="m">Множитель.</param>
        /// <param name="param">Функция параметра.</param>
        /// <param name="q_i">Вектор значений на i-ом временном слое.</param>
        private void AssemblyB(double t, bool isF, double m = 1, Function param=null, List<double> q_i = null) 
        {
            for (int k = 0; k < _grid.Nelems; k++)
            {
                Elem3D elem = _grid.Elems[k];
                double x1 = _grid.XYZ[elem.n1].X;
                double x2 = _grid.XYZ[elem.n8].X;
                double y1 = _grid.XYZ[elem.n1].Y;
                double y2 = _grid.XYZ[elem.n8].Y;
                double z1 = _grid.XYZ[elem.n1].Z;
                double z2 = _grid.XYZ[elem.n8].Z;

                double f1, f2, f3, f4, f5, f6, f7, f8;
                double gamma = 0;
                f1 = f2 = f3 = f4 = f5 = f6 = f7 = f8 = 0;

                if (isF)
                {
                    f1 = _params.F(elem.wi, x1, y1, z1, t);
                    f2 = _params.F(elem.wi, x2, y1, z1, t);
                    f3 = _params.F(elem.wi, x1, y2, z1, t);
                    f4 = _params.F(elem.wi, x2, y2, z1, t);
                    f5 = _params.F(elem.wi, x1, y1, z2, t);
                    f6 = _params.F(elem.wi, x2, y1, z2, t);
                    f7 = _params.F(elem.wi, x1, y2, z2, t);
                    f8 = _params.F(elem.wi, x2, y2, z2, t);
                    gamma = 1;
                }
                else if (param != null)
                {
                    f1 = q_i[elem.n1];
                    f2 = q_i[elem.n2];
                    f3 = q_i[elem.n3];
                    f4 = q_i[elem.n4];
                    f5 = q_i[elem.n5];
                    f6 = q_i[elem.n6];
                    f7 = q_i[elem.n7];
                    f8 = q_i[elem.n8];
                    gamma = param(elem.wi);
                }

                for (int i = 0; i < 8; i++)
                    for (int j = 0; j < 8; j++)
                    {
                        _m[i][j] = m * _allM[k][i][j];
                    }

                int[] global_nodes = { elem.n1, elem.n2, elem.n3, elem.n4, elem.n5, elem.n6, elem.n7, elem.n8 };

                for (int i = 0; i < 8; i++)
                {
                    if (global_nodes[i] < _n)
                    {
                        CalcB(gamma, global_nodes[i], i, gamma, f1, f2, f3, f4, f5, f6, f7, f8);
                    }
                    else
                    {
                        int d = global_nodes[i] - _n;
                        for (int j = _tMatrix.Ig[d]; j < _tMatrix.Ig[d + 1]; j++)
                        {
                            CalcB(gamma * _tMatrix.Gg[j], _tMatrix.Jg[j], i, gamma, f1, f2, f3, f4, f5, f6, f7, f8);
                        }
                    }
                }

                void CalcB(double mult, int l, int i, double gamma, double f1, double f2, double f3, double f4, double f5, double f6, double f7, double f8)
                {
                    _b[l] += mult * (_m[i][0] * f1 + _m[i][1] * f2 + _m[i][2] * f3 + _m[i][3] * f4 + _m[i][4] * f5 + _m[i][5] * f6 + _m[i][6] * f7 + _m[i][7] * f8);
                }
            }
        }

        /// <summary>
        /// Применяет краевое условие для узла. 
        /// </summary>
        /// <param name="bcNum">Номер краевого условия (1,2,3).</param>
        private void ApplyBc(int bcNum, double t)
        {
            switch (bcNum)
            {
                case 1:
                    foreach (Boundary3D boundary in _bc1)
                        ApplyBc1(boundary, t);
                    break;
                case 2:
                    foreach (Boundary3D boundary in _bc2)
                        ApplyBc2Node(boundary, t);
                    break;
                case 3:
                    foreach (Boundary3D boundary in _bc3)
                        ApplyBc3Node(boundary, t);
                    break;
                default:
                    LogService.LogWarning("Указано неверное краевое условие");
                    break;
            }
        }

        /// <summary>
        /// Применяет первое краевое условие. 
        /// </summary>
        private void ApplyBc1(Boundary3D boundary, double t)
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
                _b[l] = _params.Ug(p, x, y, z, t);
            }
        }

        /// <summary>
        /// Применяет второе краевое условие. 
        /// </summary>
        private void ApplyBc2Node(Boundary3D boundary, double t)
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
                theta1 = _params.Theta(p, x1, y1, z1, t);
                theta2 = _params.Theta(p, x1, y2, z1, t);
                theta3 = _params.Theta(p, x1, y1, z2, t);
                theta4 = _params.Theta(p, x1, y2, z2, t);
                area = (y2 - y1) * (z2 - z1);
            }
            else if (MathsHelper.IsEqual(y1, y2))
            {
                theta1 = _params.Theta(p, x1, y1, z1, t);
                theta2 = _params.Theta(p, x2, y1, z1, t);
                theta3 = _params.Theta(p, x1, y1, z2, t);
                theta4 = _params.Theta(p, x2, y1, z2, t);
                area = (x2 - x1) * (z2 - z1);
            }
            else
            {
                theta1 = _params.Theta(p, x1, y1, z1, t);
                theta2 = _params.Theta(p, x2, y1, z1, t);
                theta3 = _params.Theta(p, x1, y2, z1, t);
                theta4 = _params.Theta(p, x2, y2, z1, t);
                area = (x2 - x1) * (y2 - y1);
            }

            int[] global_nodes = new int[4] {l1,l2,l3,l4 };
            for (int i = 0; i < 4; i++)
            {
                if (global_nodes[i] < _n)
                    _b[global_nodes[i]] += area * (theta1 * _c[i][0] + theta2 * _c[i][1] + theta3 * _c[i][2] + theta4 * _c[i][3]) / 36.0;
                else
                {
                    int d = global_nodes[i] - _n;
                    for (int j = _tMatrix.Ig[d]; j < _tMatrix.Ig[d + 1]; j++)
                    {
                        _b[_tMatrix.Jg[j]] += _tMatrix.Gg[j] * area * (theta1 * _c[i][0] + theta2 * _c[i][1] + theta3 * _c[i][2] + theta4 * _c[i][3]) / 36.0;
                    }
                }
            }
        }

        /// <summary>
        /// Применяет третье краевое условие. 
        /// </summary>
        private void ApplyBc3Node(Boundary3D boundary, double t)
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
                ubeta1 = _params.Ubeta(p, x1, y1, z1, t);
                ubeta2 = _params.Ubeta(p, x1, y2, z1, t);
                ubeta3 = _params.Ubeta(p, x1, y1, z2, t);
                ubeta4 = _params.Ubeta(p, x1, y2, z2, t);
                area = (y2 - y1) * (z2 - z1);
            }
            else if (MathsHelper.IsEqual(y1, y2))
            {
                ubeta1 = _params.Ubeta(p, x1, y1, z1, t);
                ubeta2 = _params.Ubeta(p, x2, y1, z1, t);
                ubeta3 = _params.Ubeta(p, x1, y1, z2, t);
                ubeta4 = _params.Ubeta(p, x2, y1, z2, t);
                area = (x2 - x1) * (z2 - z1);
            }
            else
            {
                ubeta1 = _params.Ubeta(p, x1, y1, z1, t);
                ubeta2 = _params.Ubeta(p, x2, y1, z1, t);
                ubeta3 = _params.Ubeta(p, x1, y2, z1, t);
                ubeta4 = _params.Ubeta(p, x2, y2, z1, t);
                area = (x2 - x1) * (y2 - y1);
            }

            int[] global_nodes = new int[4] { l1, l2, l3, l4 };

            for (int ic = 0; ic < 4; ic++)
                for (int jc = 0; jc < 4; jc++)
                    AddLocalMatrixElement(_c[ic][jc] * area * beta / 36.0, global_nodes[ic], global_nodes[jc]);


            for (int i = 0; i < 4; i++) 
            {
                if (global_nodes[i] < _n)
                    _b[global_nodes[i]] += beta * area * (ubeta1 * _c[i][0] + ubeta2 * _c[i][1] + ubeta3 * _c[i][2] + ubeta4 * _c[i][3]) / 36.0;
                else
                {
                    int d = global_nodes[i] - _n;
                    for (int j = _tMatrix.Ig[d]; j < _tMatrix.Ig[d + 1]; j++)
                    {
                        _b[_tMatrix.Jg[j]] += _tMatrix.Gg[j] * beta * area * (ubeta1 * _c[i][0] + ubeta2 * _c[i][1] + ubeta3 * _c[i][2] + ubeta4 * _c[i][3]) / 36.0;
                    }
                }
            }
        }

        #endregion Private Methods
    }
}
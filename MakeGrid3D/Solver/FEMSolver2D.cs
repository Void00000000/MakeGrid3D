namespace MakeGrid3D.Solver
{
    using MakeGrid3D.Helpers;
    using MakeGrid3D.Solver.SparseModule;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Решатель метода конечных элементов двумерный (с использованием T технологии). 
    /// Для решения гиперболическиз задач с 4-ех слойной неявной схемой.
    /// </summary>
    public class FEMSolver2D
    {
        #region Private Fields

        /// <summary>
        /// Единственный экземпляр ленивого синглтона <see cref="FEMSolver2D"/>. 
        /// </summary>
        private static readonly Lazy<FEMSolver2D> _instance = new Lazy<FEMSolver2D>(() => new FEMSolver2D());

        /// <summary>
        /// Конечноэлементная сетка. 
        /// </summary>
        private Grid2D _grid;

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
        private FEMParams2D _params;

        /// <summary>
        /// Границы первого краевого условия. 
        /// </summary>
        private List<Boundary2D> _bc1;

        /// <summary>
        /// Границы второго краевого условия. 
        /// </summary>
        private List<Boundary2D> _bc2;

        /// <summary>
        /// Границы третьего краевого условия. 
        /// </summary>
        private List<Boundary2D> _bc3;

        /// <summary>
        /// Первое слагаемое локальной матрицы жесткости. 
        /// </summary>
        private double[][] _g1 = new double[][]
                                { 
                                new double[] {2, -2, 1, -1},
                                new double[] {-2, 2, -1, 1},
                                new double[] {1, -1, 2, -2},
                                new double[] {-1, 1, -2, 2} 
                                };

        /// <summary>
        /// Второе слагаемое локальной матрицы жесткости. 
        /// </summary>
        private double[][] _g2 = new double[][]
                                {
                                new double[] {2, 1, -2, -1},
                                new double[] {1, 2, -1, -2},
                                new double[] {-2, -1, 2, 1},
                                new double[] {-1, -2, 1, 2}
                                };

        /// <summary>
        /// Локальная матрицы массы без умножения на коэффициент. 
        /// </summary>
        private double[][] _c = new double[][]
                                {
                                new double[] {4, 2, 2, 1},
                                new double[] {2, 4, 1, 2},
                                new double[] {2, 1, 4, 2},
                                new double[] {1, 2, 2, 4}
                                };

        /// <summary>
        /// Локальная матрицы жесткости. 
        /// </summary>
        private double[][] _g = new double[][]
                                {
                                new double[] {0, 0, 0, 0},
                                new double[] {0, 0, 0, 0},
                                new double[] {0, 0, 0, 0},
                                new double[] {0, 0, 0, 0 }
                                };

        /// <summary>
        /// Локальная матрицы массы. 
        /// </summary>
        private double[][] _m = new double[][]
                                {
                                new double[] {0, 0, 0, 0},
                                new double[] {0, 0, 0, 0},
                                new double[] {0, 0, 0, 0},
                                new double[] {0, 0, 0, 0 }
                                };


        /// <summary>
        /// Локальная матрица учета третьего краевого условия. 
        /// </summary>
        private double[][] _as3 = new double[][]
                                {
                                new double[] {0, 0},
                                new double[] {0, 0}
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
        public static FEMSolver2D Instance => _instance.Value;

        #endregion Public Properties 

        #region Constructors

        private FEMSolver2D()
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
        public bool Initialize(Grid2D grid, FEMParams2D gridParams, List<Boundary2D> bc1, List<Boundary2D> bc2, List<Boundary2D> bc3, List<double> t)
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

            if (_grid.Nnodes > _grid.Nc)
            {
                GTree G = GenerateGTree();
                bool isSuccess =  GenerateTMatrix(G);
                if (!isSuccess) return false;
            }
            GeneratePortrait();

            InitPrevQ_3_2();
            q_1 = SolveBy3Layers(_t[0], _t[1], _t[2]);
            return true;
        }

        /// <summary>
        /// Решает краевую задачу.
        /// </summary>
        /// <returns>Список векторов решения для каждого временного слоя.</returns>
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
                double x = _grid.XY[i].X;
                double y = _grid.XY[i].Y;
                int p = _grid.Area.FindSubArea(x, x, y, y);
                if (p < -1)
                    continue;

                double u0 = _params.U0(p, x, y);
                q_3[i]= u0;

                if (_params.U1 != null) 
                { 
                    double u1 = _params.U1(p, x, y);
                    q_2[i] = u1;
                }
                else 
                {
                    double d_u1 = _params.DU0(p, x, y);
                    q_2[i] = d_u1;
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
            foreach (Elem2D elem in _grid.Elems)
            {
                AddNodes(elem.n1, global_nodes);
                AddNodes(elem.n2, global_nodes);
                AddNodes(elem.n3, global_nodes);
                AddNodes(elem.n4, global_nodes);
                global_nodes.Sort();

                // Цикл по ненулевым базисным функциям
                for (int i_n = 0; i_n < global_nodes.Count; i_n++) {
                    g1 = global_nodes[i_n];
                    for (int j_n = i_n + 1; j_n < global_nodes.Count; j_n++) {
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
            foreach (Elem2D elem in _grid.Elems) 
            {
                foreach (int nc in elem.n_uc) 
                {
                    G.Add(nc, new List<(int, double)>(2));
                    int n1 = elem.n1;
                    int n2 = elem.n2;
                    int n3 = elem.n3;
                    int n4 = elem.n4;
                    double xc = _grid.XY[nc].X;
                    double yc = _grid.XY[nc].Y;
                    double xmin = _grid.XY[n1].X;
                    double xmax = _grid.XY[n4].X;
                    double ymin = _grid.XY[n1].Y;
                    double ymax = _grid.XY[n4].Y;
                    double hx = xmax - xmin;
                    double hy = ymax - ymin;

                    // Узел лежит на нижней стороне
                    if (MathsHelper.IsEqual(yc, ymin)) 
                    {
                        double Telem1 = BasicFunc1(xc, xmax, hx);
                        double Telem2 = BasicFunc2(xc, xmin, hx);
                        G[nc].Add((n1, Telem1));
                        G[nc].Add((n2, Telem2));
                    }
                    // Узел лежит на правой стороне
                    else if (MathsHelper.IsEqual(xc, xmax))
                    {
                        double Telem1 = BasicFunc1(yc, ymax, hy);
                        double Telem2 = BasicFunc2(yc, ymin, hy);
                        G[nc].Add((n2, Telem1));
                        G[nc].Add((n4, Telem2));
                    }
                    // Узел лежит на верхней стороне
                    else if (MathsHelper.IsEqual(yc, ymax))
                    {
                        double Telem1 = BasicFunc1(xc, xmax, hx);
                        double Telem2 = BasicFunc2(xc, xmin, hx);
                        G[nc].Add((n2, Telem1));
                        G[nc].Add((n4, Telem2));
                    }
                    // Узел лежит на левой стороне
                    else if (MathsHelper.IsEqual(xc, xmin)) 
                    {
                        double Telem1 = BasicFunc1(yc, ymax, hy);
                        double Telem2 = BasicFunc2(yc, ymin, hy);
                        G[nc].Add((n1, Telem1));
                        G[nc].Add((n3, Telem2));
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

                if (i < _grid.Nc && !processed_nodes.Contains(i))
                {
                    jg_gg.Add((i, m * Telem));
                    elems_count++;
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
        double BasicFunc1(double x, double xmax, double h) 
        {
            return (xmax - x) / h;
        }

        /// <summary>
        /// Вторая локальная базисная функция X2. 
        /// </summary>
        /// <param name="x">Переменное значение.</param>
        /// <param name="xmin">Минимальное значение x на элементе.</param>
        /// <param name="h">Длина значения по x</param>
        double BasicFunc2(double x, double xmin, double h)
        {
            return (x - xmin) / h;
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
            foreach (Elem2D elem in _grid.Elems) 
            {
                double x1 = _grid.XY[elem.n1].X;
                double x2 = _grid.XY[elem.n4].X;
                double y1 = _grid.XY[elem.n1].Y;
                double y2 = _grid.XY[elem.n4].Y;
                double hx = x2 - x1;
                double hy = y2 - y1;
                double lambda = _params.Lambda(elem.wi);
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                    {
                        _g[i][j] = _g1[i][j] * hy / hx + _g2[i][j] * hx / hy;
                        _g[i][j] *= lambda / 6;
                    }

                int[] global_nodes = { elem.n1, elem.n2, elem.n3, elem.n4 };
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                        AddLocalMatrixElement(_g[i][j], global_nodes[i], global_nodes[j]);
            }
        }

        /// <summary>
        /// Собирает матрицу масс и добавляет её в глобальную матрицу. 
        /// </summary>
        /// <param name="m">Множитель.</param>
        /// <param name="isChi">Множитель хи, иначе множитель сигма.</param>
        private void AssemblyM(double m, Function param) 
        {
            foreach (Elem2D elem in _grid.Elems)
            {
                double x1 = _grid.XY[elem.n1].X;
                double x2 = _grid.XY[elem.n4].X;
                double y1 = _grid.XY[elem.n1].Y;
                double y2 = _grid.XY[elem.n4].Y;
                double hx = x2 - x1;
                double hy = y2 - y1;
                double gamma = param(elem.wi);
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                    {
                        _m[i][j] = m * gamma * hx * hy * _c[i][j] / 36.0;
                    }

                int[] global_nodes = { elem.n1, elem.n2, elem.n3, elem.n4 };
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                        AddLocalMatrixElement(_m[i][j], global_nodes[i], global_nodes[j]);
            }
        }

        /// <summary>
        /// Собирает вектор правой части. 
        /// </summary>
        /// <param name="t">Значение на текущем временном слое.</param>
        /// <param name="isF">Сборка происходит через вектор f, иначе через q_i. Если isF = true, то умножается на 1</param>
        /// <param name="m">Множитель.</param>
        /// <param name="isChi">Множитель хи, иначе множитель сигма.</param>
        /// <param name="q_i">Вектор значений на i-ом временном слое.</param>
        private void AssemblyB(double t, bool isF, double m = 1, Function param = null, List<double> q_i = null)
        {
            foreach (Elem2D elem in _grid.Elems)
            {
                double x1 = _grid.XY[elem.n1].X;
                double x2 = _grid.XY[elem.n4].X;
                double y1 = _grid.XY[elem.n1].Y;
                double y2 = _grid.XY[elem.n4].Y;
                double hx = x2 - x1;
                double hy = y2 - y1;

                double f1, f2, f3, f4;
                double gamma = 0;
                f1 = f2 = f3 = f4 = 0;
                if (isF)
                {
                    f1 = _params.F(elem.wi, x1, y1, t);
                    f2 = _params.F(elem.wi, x2, y1, t);
                    f3 = _params.F(elem.wi, x1, y2, t);
                    f4 = _params.F(elem.wi, x2, y2, t);
                    gamma = 1;
                }
                else if (q_i != null)
                {
                    f1 = q_i[elem.n1];
                    f2 = q_i[elem.n2];
                    f3 = q_i[elem.n3];
                    f4 = q_i[elem.n4];
                    if (param != null)
                        gamma = param(elem.wi);
                }

                
                int[] global_nodes = { elem.n1, elem.n2, elem.n3, elem.n4 };
                for (int i = 0; i < 4; i++)
                {
                    if (global_nodes[i] < _n)
                    {
                        CalcB(m * gamma, global_nodes[i], i, hx, hy, f1, f2, f3, f4);
                    }
                    else
                    {
                        int k = global_nodes[i] - _n;
                        for (int j = _tMatrix.Ig[k]; j < _tMatrix.Ig[k + 1]; j++)
                        {
                            CalcB(m * gamma * _tMatrix.Gg[j], _tMatrix.Jg[j], i, hx, hy, f1, f2, f3, f4);
                        }
                    }  
                }
            }

            void CalcB(double mult, int l, int i, double hx, double hy, double f1, double f2, double f3, double f4) 
            {
                _b[l] += mult * hx * hy * (_c[i][0] * f1 + _c[i][1] * f2 + _c[i][2] * f3 + _c[i][3] * f4) / 36.0;
            }
        }


        /// <summary>
        /// Применяет краевое условие. 
        /// </summary>
        /// <param name="bc">Номер краевого услоивя (1,2,3).</param>
        private void ApplyBc(int bcNum, double t) 
        {
            switch (bcNum) 
            {
                case 1:
                    foreach (Boundary2D boundary in _bc1)
                        ApplyBc1(boundary, t);
                    break;
                case 2:
                    foreach (Boundary2D boundary in _bc2)
                        ApplyBc2(boundary, t);
                    break;
                case 3:
                    foreach (Boundary2D boundary in _bc3)
                        ApplyBc3(boundary, t);
                    break;
                default:
                    LogService.LogWarning("Указано неверное краевое условие");
                    break;
            }
        }

        /// <summary>
        /// Применяет первое краевое условие. 
        /// </summary>
        /// <param name="boundary">Граница.</param>
        /// <param name="t">Временной слой.</param>
        private void ApplyBc1(Boundary2D boundary, double t) 
        {
            int p = boundary.Si;
            List<int> global_nodes = new List<int>() { boundary.N1, boundary.N2};

            foreach (int l in global_nodes)
            {
                double x = _grid.XY[l].X;
                double y = _grid.XY[l].Y;
                _matrix.Di[l] = 1;
                for (int k = _matrix.Ig[l]; k < _matrix.Ig[l + 1]; k++)
                    _matrix.Gl[k] = 0;
                for (int k = 0; k < _matrix.Ng; k++)
                    if (_matrix.Jg[k] == l)
                        _matrix.Gu[k] = 0;
                _b[l] = _params.Ug(p, x, y, t);
            }
        }

        /// <summary>
        /// Применяет второе краевое условие. 
        /// </summary>
        /// <param name="boundary">Граница.</param>
        /// <param name="t">Временной слой.</param>
        private void ApplyBc2(Boundary2D boundary, double t)
        {
            int p = boundary.Si;
            int l1 = boundary.N1;
            int l2 = boundary.N2;
            double x1 = _grid.XY[l1].X;
            double x2 = _grid.XY[l2].X;
            double y1 = _grid.XY[l1].Y;
            double y2 = _grid.XY[l2].Y;

            double theta1 = _params.Theta(p, x1, y1, t);
            double theta2 = _params.Theta(p, x2, y2, t);
            double h = MathsHelper.IsEqual(y1, y2) ? x2 - x1 : y2 - y1;
            _b[l1] += h * (2 * theta1 + theta2) / 6.0;
            _b[l2] += h * (theta1 + 2 * theta2) / 6.0;
        }

        /// <summary>
        /// Применяет третье краевое условие. 
        /// </summary>
        /// <param name="boundary">Граница.</param>
        /// <param name="t">Временной слой.</param>
        private void ApplyBc3(Boundary2D boundary, double t)
        {
            int p = boundary.Si;
            int l1 = boundary.N1;
            int l2 = boundary.N2;
            double x1 = _grid.XY[l1].X;
            double x2 = _grid.XY[l2].X;
            double y1 = _grid.XY[l1].Y;
            double y2 = _grid.XY[l2].Y;
            double beta_const = _params.Beta(p);
            double u_beta1 = _params.Ubeta(p, x1, y1, t);
            double u_beta2 = _params.Ubeta(p, x2, y2, t);
            double h = MathsHelper.IsEqual(y1, y2) ? x2 - x1 : y2 - y1;

            _as3[0][1] = beta_const * h / 6.0;
            _as3[0][0] = 2 * _as3[0][1];
            _as3[1][0] = _as3[0][1];
            _as3[1][1] = _as3[0][0];
            int[] global_nodes = new int[2] { l1, l2 };
            for (int row = 0; row < 2; row++)
                for (int column = 0; column < 2; column++)
                    AddLocalMatrixElement(_as3[row][column], global_nodes[row], global_nodes[column]);

            _b[l1] += beta_const * h * (2 * u_beta1 + u_beta2) / 6.0;
            _b[l2] += beta_const * h * (u_beta1 + 2 * u_beta2) / 6.0;
        }

        #endregion Private Methods
    }
}

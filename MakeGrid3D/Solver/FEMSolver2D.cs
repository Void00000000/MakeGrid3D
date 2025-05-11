namespace MakeGrid3D.Solver
{
    using MakeGrid3D.Solver.SparseModule;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Решатель метода конечных элементов двумерный. 
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
        /// Параметры краевой задачи. 
        /// </summary>
        private FEMParams _params;

        /// <summary>
        /// Границы первого краевого условия. 
        /// </summary>
        private List<Boundary> _bc1;

        /// <summary>
        /// Границы второго краевого условия. 
        /// </summary>
        private List<Boundary> _bc2;

        /// <summary>
        /// Границы третьего краевого условия. 
        /// </summary>
        private List<Boundary> _bc3;

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
        /// Размерность матрицы. 
        /// </summary>
        private int _nNodes;

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
        public void Initialize(Grid2D grid, FEMParams gridParams, List<Boundary> bc1, List<Boundary> bc2, List<Boundary> bc3)
        {
            _grid = grid;
            _params = gridParams;
            _bc1 = bc1;
            _bc2 = bc2;
            _bc3 = bc3;
            _nNodes = _grid.Nnodes;
            GeneratePortrait();
        }

        /// <summary>
        /// Решает краевую задачу.
        /// </summary>
        /// <returns>Вектор решения.</returns>
        public List<double> Solve() 
        {
            AssemblyG();
            AssemblyM();
            AssemblyB();
            ApplyBc(2);
            ApplyBc(3);
            ApplyBc(1);
            return LOSSolver.Instance.LOS(_matrix, _b);
        } 

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Создает портрет матрицы. 
        /// </summary>
        private void GeneratePortrait() 
        {
            List<List<int>> list = new List<List<int>>(_nNodes);
            list[0].Add(0);
            int g1, g2;  // Глобальные номера базисных функций
            bool not_in;
            int[] global_nodes = new int[4]; // Массив глобальных узлов элемента.
            // Цикл по конечным элементам
            for (int j = 0; j < _grid.Ny - 1; j++)
                for (int i = 0; i < _grid.Nx - 1; i++)
                {
                    global_nodes[0] = _grid.global_num(i, j);
                    global_nodes[1] = _grid.global_num(i + 1, j);
                    global_nodes[2] = _grid.global_num(i, j + 1);
                    global_nodes[3] = _grid.global_num(i + 1, j + 1);
                    // Цикл по ненулевым базисным функциям
                    for (int i_n = 0; i_n < 4; i_n++) {
                        g1 = global_nodes[i_n];
                        for (int j_n = i_n +1; j_n < 4; j_n++) {
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
                }


            // Сортировка списков по возрастанию
            for (int i = 0; i < _nNodes; i++)
                list[i].Sort();

            _matrix = new SparseMatrix(_nNodes);
            // Формирование вектора ig
            _matrix.Ig[0] = 0;
            for (int i = 0; i < list.Count; i++)
                _matrix.Ig[i + 1] = _matrix.Ig[i] + list[i].Count;

            for (int i = 1; i < _nNodes + 1; i++)
                _matrix.Ig[i] -= 1;

            int ng = _matrix.Ig[_nNodes];
            _matrix.Alloc(ng);
            // Формирование вектора jg
            for (int i = 1, j = 0; i < _nNodes; i++)
                for (int k = 0; k < list[i].Count; k++, j++)
                    _matrix.Jg[j] = list[i][k];
        }

        /// <summary>
        /// Добавляет локальную матрицу в глобальную. 
        /// </summary>
        /// <param name="l">Локальная матрица.</param>
        /// <param name="n">Размерность локальной матрицы.</param>
        /// <param name="global_nodes">Массив глобальных номеров конечного элемента.</param>
        private void AddLocalMatrix(double[][] l, int n, int[] global_nodes) 
        {
            int ibeg, iend, med;
            for (int i = 0; i < n; i++)
                _matrix.Di[global_nodes[i]] = _matrix.Di[global_nodes[i]] + l[i][i];

            for (int i = 0; i < n; i++)
            {
                ibeg = _matrix.Ig[global_nodes[i]];
                for (int j = 0; j <= i - 1; j++)
                {
                    iend = _matrix.Ig[global_nodes[i] + 1] - 1;
                    while (_matrix.Jg[ibeg] != global_nodes[j])
                    {
                        med = (ibeg + iend) / 2;
                        if (_matrix.Jg[med] < global_nodes[j])
                            ibeg = med + 1;
                        else
                            iend = med;
                    }
                    _matrix.Gl[ibeg] += l[i][j];
                    _matrix.Gu[ibeg] += l[j][i];
                    ibeg++;
                }
            }
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
                AddLocalMatrix(_g, 4, global_nodes);
            }
        }

        /// <summary>
        /// Собирает матрицу масс и добавляет её в глобальную матрицу. 
        /// </summary>
        private void AssemblyM() 
        {
            foreach (Elem2D elem in _grid.Elems)
            {
                double x1 = _grid.XY[elem.n1].X;
                double x2 = _grid.XY[elem.n4].X;
                double y1 = _grid.XY[elem.n1].Y;
                double y2 = _grid.XY[elem.n4].Y;
                double hx = x2 - x1;
                double hy = y2 - y1;
                double sigma = _params.Sigma(elem.wi);
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                    {
                        _m[i][j] = sigma * hx * hy * _c[i][j] / 36.0;
                    }

                int[] global_nodes = { elem.n1, elem.n2, elem.n3, elem.n4 };
                AddLocalMatrix(_m, 4, global_nodes);
            }
        }

        /// <summary>
        /// Собирает вектор правой части. 
        /// </summary>
        private void AssemblyB()
        {
            foreach (Elem2D elem in _grid.Elems)
            {
                double x1 = _grid.XY[elem.n1].X;
                double x2 = _grid.XY[elem.n4].X;
                double y1 = _grid.XY[elem.n1].Y;
                double y2 = _grid.XY[elem.n4].Y;
                double hx = x2 - x1;
                double hy = y2 - y1;
                
                double f1 = _params.F(elem.wi, x1, y1);
                double f2 = _params.F(elem.wi, x2, y1);
                double f3 = _params.F(elem.wi, x1, y2);
                double f4 = _params.F(elem.wi, x2, y2);

                int[] global_nodes = { elem.n1, elem.n2, elem.n3, elem.n4 };
                for (int i = 0; i < 4; i++)
                    _b[global_nodes[i]] += hx * hy * (_c[i][0] * f1 + _c[i][1] * f2 + _c[i][2] * f3 + _c[i][3] * f4) / 36.0;
            }
        }

        /// <summary>
        /// Применяет краевое условие. 
        /// </summary>
        /// <param name="bc">Номер краевого услоивя (1,2,3).</param>
        private void ApplyBc(int bc) 
        {
            int p;
            int i_beg, i_end, j_beg, j_end;
            for (int s = 0; s < _bc1.Count; s++)
            {
                i_beg = _grid.IXw[_bc1[s].Nx1];
                i_end = _grid.IXw[_bc1[s].Nx2];
                j_beg = _grid.IYw[_bc1[s].Ny1];
                j_end = _grid.IYw[_bc1[s].Ny2];
                p = _bc1[s].Si;
                if (i_beg == i_end)
                {
                    int i = i_beg;
                    for (int j = j_beg; j <= j_end; j++)
                    {
                        ApplyBcNode(bc, i, j, p, true);
                    }
                }
                else
                {
                    int j = j_beg;
                    for (int i = i_beg; i <= i_end; i++)
                    {
                        ApplyBcNode(bc, i, j, p, false);
                    }
                }
            }
        }

        /// <summary>
        /// Применяет краевое условие для узла. 
        /// </summary>
        /// <param name="bc">Номер краевого условия (1,2,3).</param>
        /// <param name="i">Порядкой номер узла по горизонтали (нумерация с 0).</param>
        /// <param name="j">Порядкой номер узла по горизонтали (нумерация с 0).</param>
        /// <param name="p">Номер границы.</param>
        /// <param name="isX">true, если граница горизонтальная, вертикальная иначе.</param>
        private void ApplyBcNode(int bc, int i, int j, int p, bool isX) 
        {
            switch (bc) 
            {
                case 1:
                    ApplyBc1Node(i, j, p);
                    break;
                case 2:
                    ApplyBc2Node(i, j, p, isX);
                    break;
                case 3:
                    ApplyBc3Node(i, j, p, isX);
                    break;
                default:
                    LogService.LogWarning("Указано неверное краевое условие");
                    break;
            }
        }

        /// <summary>
        /// Применяет первое краевое условие для узла. 
        /// </summary>
        private void ApplyBc1Node(int i, int j, int p) 
        {
            int l = _grid.global_num(i, j);
            double x = _grid.XY[l].X;
            double y = _grid.XY[l].Y;
            _matrix.Di[l] = 1;
            for (int k = _matrix.Ig[l]; k < _matrix.Ig[l + 1]; k++)
                _matrix.Gl[k] = 0;
            for (int k = 0; k < _matrix.Ng; k++)
                if (_matrix.Jg[k] == l)
                    _matrix.Gu[k] = 0;
            _b[l] = _params.Ug(p, x, y);
        }

        /// <summary>
        /// Применяет второе краевое условие для узла. 
        /// </summary>
        private void ApplyBc2Node(int i, int j, int p, bool isX)
        {
            int l1 = _grid.global_num(i, j);
            int l2 = _grid.global_num(i, j + 1);
            double x1 = _grid.XY[l1].X;
            double x2 = _grid.XY[l2].X;
            double y1 = _grid.XY[l1].Y;
            double y2 = _grid.XY[l2].Y;

            double theta1 = _params.Theta(p, x1, y1);
            double theta2 = _params.Theta(p, x2, y2);
            double h = isX ? x2 - x1 : y2 - y1;
            _b[l1] += h * (2 * theta1 + theta2) / 6.0;
            _b[l2] += h * (theta1 + 2 * theta2) / 6.0;
        }

        /// <summary>
        /// Применяет третье краевое условие для узла. 
        /// </summary>
        private void ApplyBc3Node(int i, int j, int p, bool isX)
        {
            int l1 = _grid.global_num(i, j);
            int l2 = _grid.global_num(i, j + 1);
            double x1 = _grid.XY[l1].X;
            double x2 = _grid.XY[l2].X;
            double y1 = _grid.XY[l1].Y;
            double y2 = _grid.XY[l2].Y;
            double beta_const = _params.Beta(p);
            double u_beta1 = _params.Ubeta(p, x1, y1);
            double u_beta2 = _params.Ubeta(p, x2, y2);
            double h = isX ? x2 - x1 : y2 - y1;

            _as3[0][1] = beta_const * h / 6.0;
            _as3[0][0] = 2 * _as3[0][1];
            _as3[1][0] = _as3[0][1];
            _as3[1][1] = _as3[0][0];
            int[] global_noses = new int[2] { l1, l2 };
            AddLocalMatrix(_as3, 2, global_noses);

            _b[l1] += beta_const * h * (2 * u_beta1 + u_beta2) / 6.0;
            _b[l2] += beta_const * h * (u_beta1 + 2 * u_beta2) / 6.0;
        }

        #endregion Private Methods
    }
}

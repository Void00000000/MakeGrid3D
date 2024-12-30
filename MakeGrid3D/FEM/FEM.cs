//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Controls;
//using System.Windows.Documents;

//namespace MakeGrid3D.FEM
//{
//    class FEM2D
//    {
//        Grid2D grid;
//        private int N;  // Количество узлов
//        private int n; // Количество конечных элементов
//        public List<float> q { get; }
//        private List<float> b;
//        private int n_jgg;            // Размерность векторов ggl, ggu и jg
//        // Матрица A--------------
//        private List<float> di;
//        private List<float> ggl;
//        private List<float> ggu;
//        private List<int> ig;
//        private List<int> jg;
//        // -----------------------

//        // Краевые условия-----------------
//        private int ns1; // Количество границ с первым краевым условием
//        private List<(int, int)> bc1;  // Номера границ с первым кравевым условием
//        private int[] L;    // Вектор для хранения номеров глобальных базисных функций
//        Numeric numeric;

//        List<List<float>> G = new List<List<float>>(5);
//        List<List<float>> C = new List<List<float>>(5);

//        public FEM2D(Grid2D grid)
//        {
//            this.grid = grid;
//            N = grid.Nnodes;
//            n = grid.Nelems;
//            for (int i = 0; i < 5; i++)
//            {
//                G.Add(new List<float>(5));
//                resize(G[i], 5);
//                C.Add(new List<float>(5));
//                resize(C[i], 5);
//            }

//            generate_portrait();
//        }

//        public void solve()
//        {
//            G_matrix_assembly();
//            M_matrix_assembly();
//            b_vector_assembly();
//            use_bc1();
//            LOS_solve(q, di, ggl, ggu, b, ig, jg);
//        }


//        void resize<T>(List<T> list, int size)
//        {
//            list = new List<T>(size);
//            for (int i = 0; i < size; i++)
//                list.Add(default);
//        }

//        int check_edge(Elem2D elem)
//        {
//            if (elem.n5 < 0) return 0;
//            float xmin = grid.XY[elem.n1].X;
//            float ymin = grid.XY[elem.n1].Y;
//            float xmax = grid.XY[elem.n4].X;
//            float ymax = grid.XY[elem.n4].Y;
//            float xm = grid.XY[elem.n5].X;
//            float ym = grid.XY[elem.n5].Y;
//            if (xm > xmin && xm < xmax && MathF.Abs(ym - ymin) < 10e-14) return 1;
//            if (ym > ymin && ym < ymax && MathF.Abs(xm - xmax) < 10e-14) return 2;
//            if (xm > xmin && xm < xmax && MathF.Abs(ym - ymax) < 10e-14) return 3;
//            if (ym > ymin && ym < ymax && MathF.Abs(xm - xmin) < 10e-14) return 4;
//            return 0;
//        }

//        void generate_portrait()
//        {
//            List<List<int>> list = new List<List<int>>(N);
//            list[0].Add(0);
//            int g1, g2;  // Глобальные номера базисных функций
//            bool not_in;
//            int edge_num;
//            // Цикл по конечным элементам
//            for (int i = 0; i < n; i++)
//            {
//                int k;
//                Elem2D elem = grid.Elems[i];
//                edge_num = check_edge(elem);
//                L[0] = elem.n1;
//                L[1] = elem.n2;
//                L[2] = elem.n3;
//                L[3] = elem.n4;
//                L[4] = elem.n5;
//                if (edge_num > 0) k = 5; else k = 4;
//                // Цикл по ненулевым базисным функциям
//                for (int in_ = 0; in_ < k; in_++) {
//                    g1 = L[in_];
//                    for (int jn = in_ + 1; jn < k; jn++) {
//                        // g2 > g1
//                        g2 = L[jn];
//                        // Перед добавлением проверяем наличие элемента в списке
//                        not_in = true;
//                        for (int l = 0; l < list[g2].Count && not_in; l++)
//                            if (g1 == list[g2][l])
//                                not_in = false;

//                        // Добавляем
//                        if (not_in)
//                            list[g2].Add(g1);
//                    }
//                }
//            }
//            // Сортировка списков по возрастанию
//            for (int i = 0; i<N; i++)
//                list[i].OrderBy(o => o).ToList();
//            // Формирование вектора ig
//            resize(ig, N + 1);
//            for (int i = 0; i< list.Count; i++)
//                ig[i + 1] = ig[i] + list[i].Count;

//            for (int i = 1; i<N + 1; i++)
//                ig[i] -= 1;

//            n_jgg = ig[N];
//            resize(jg, n_jgg);
//            resize(ggl, n_jgg);
//            resize(ggu, n_jgg);
//            // Формирование вектора jg
//            for (int i = 1, j = 0; i<N; i++)
//                for (int k = 0; k<list[i].Count; k++, j++)
//                    jg[j] = list[i][k];
//        }   

        


//        // Занесение локальной матрицы размером k на k в глобальную матрицу A
//        void add_local_matrix(List<List<float>> local_matrix, int k)
//        {
//            int ibeg, iend, med;
//            for (int i = 0; i < k; i++)
//                di[L[i]] = di[L[i]] + local_matrix[i][i];

//            for (int i = 0; i < k; i++)
//            {
//                ibeg = ig[L[i]];
//                for (int j = 0; j <= i - 1; j++)
//                {
//                    iend = ig[L[i] + 1] - 1;
//                    while (jg[ibeg] != L[j])
//                    {
//                        med = (ibeg + iend) / 2;
//                        if (jg[med] < L[j])
//                            ibeg = med + 1;
//                        else
//                            iend = med;
//                    }
//                    ggl[ibeg] += local_matrix[i][j];
//                    ggu[ibeg] += local_matrix[j][i];
//                    ibeg++;
//                }
//            }

//        }

//        void G_matrix_assembly()
//        {
//            int k; // Размерность локальных матриц
//            int edge_num;
//            float h_x, h_y;
//            float xmin, xmax, ymin, ymax;
//            float xm = 0;
//            float ym = 0;
//            for (int i = 0; i < n; i++)
//            {
//                Elem2D elem = grid.Elems[i];
//                edge_num = check_edge(elem);
//                L[0] = elem.n1;
//                L[1] = elem.n2;
//                L[2] = elem.n3;
//                L[3] = elem.n4;
//                L[4] = elem.n5;
//                xmin = grid.XY[L[0]].X;
//                ymin = grid.XY[L[0]].Y;
//                xmax = grid.XY[L[3]].X;
//                ymax = grid.XY[L[3]].Y;
//                if (edge_num > 0)
//                {
//                    k = 5;
//                    xm = grid.XY[L[4]].X;
//                    ym = grid.XY[L[4]].Y;
//                }
//                else k = 4;

//                for (int il = 0; il < k; il ++)
//                    for (int jl = 0; jl < k; jl++)
//                    {
//                        G[il][jl] = lambda(1) * numeric.doubleIntegral(xmin, xmax, ymin, ymax, xm, ym, psi_G[edge_num][il][jl]);
//                        //G[i][j] = G1[i][j]* h_y / h_x + G2[i][j] * h_x / h_y;
//                        //G[i][j] *= lambda / 6;
//                    }
//                add_local_matrix(G, k);
//            }
//        }

//        void M_matrix_assembly()
//        {
//            int k; // Размерность локальных матриц
//            int edge_num;
//            float h_x, h_y;
//            float xmin, xmax, ymin, ymax;
//            float xm = 0;
//            float ym = 0;
//            for (int i = 0; i < n; i++)
//            {
//                Elem2D elem = grid.Elems[i];
//                edge_num = check_edge(elem);
//                L[0] = elem.n1;
//                L[1] = elem.n2;
//                L[2] = elem.n3;
//                L[3] = elem.n4;
//                L[4] = elem.n5;
//                xmin = grid.XY[L[0]].X;
//                ymin = grid.XY[L[0]].Y;
//                xmax = grid.XY[L[3]].X;
//                ymax = grid.XY[L[3]].Y;
//                if (edge_num > 0)
//                {
//                    k = 5;
//                    xm = grid.XY[L[4]].X;
//                    ym = grid.XY[L[4]].Y;
//                }
//                else k = 4;

//                for (int il = 0; il < k; il++)
//                    for (int jl = 0; jl < k; jl++)
//                    {
//                        C[il][jl] = lambda(1) * numeric.doubleIntegral(xmin, xmax, ymin, ymax, xm, ym, psi_M[edge_num][il][jl]);
//                    }
//                add_local_matrix(C, k);
//            }
//        }


//        void b_vector_assembly()
//        {
//            float f0, f1, f2, f3, f4;
//            int k; // Размерность локальных матриц
//            int edge_num;
//            float h_x, h_y;
//            float xmin, xmax, ymin, ymax;
//            float xm = 0;
//            float ym = 0;
//            for (int i = 0; i < n; i++)
//            {
//                Elem2D elem = grid.Elems[i];
//                edge_num = check_edge(elem);
//                L[0] = elem.n1;
//                L[1] = elem.n2;
//                L[2] = elem.n3;
//                L[3] = elem.n4;
//                L[4] = elem.n5;
//                xmin = grid.XY[L[0]].X;
//                ymin = grid.XY[L[0]].Y;
//                xmax = grid.XY[L[3]].X;
//                ymax = grid.XY[L[3]].Y;
//                xm = 0;
//                ym = 0;

//                f0 = f(1, xmin, ymin);
//                f1 = f(1, xmax, ymin);
//                f2 = f(1, xmin, ymax);
//                f3 = f(1, xmax, ymax);
//                f4 = 0;
//                if (edge_num > 0) { 
//                    k = 5;
//                    xm = grid.XY[L[4]].X;
//                    ym = grid.XY[L[4]].Y;
//                    f4 = f(1, xm, ym);
//                }
//                else k = 4;


//                for (int il = 0; il < k; il++)
//                    for (int jl = 0; jl < k; jl++)
//                    {
//                        //M[i][j] = gamma1 * M1[i][j] + gamma2 * M2[i][j] + gamma3 * M3[i][j] + gamma4 * M4[i][j];
//                        //M[i][j] *= h_x * h_y / 144;
//                        C[il][jl] = numeric.floatIntegral(xmin, xmax, ymin, ymax, xm, ym, psi_M[edge_num][il][jl]);
//                    }

//                for (int il = 0; il < k; il++)
//                    b[L[i]] += (C[il][0] * f0 + C[il][1] * f1 + C[il][2] * f2 + C[il][3] * f3 + C[il][4] * f4);
//            }
//        }


//        // Учёт краевых условий 1-го рода
//        void use_bc1()
//        {
//            int si, l;
//            float x, y;
//            for (int i = 0; i < ns1; i += 1)
//            {
//                si = bc1[i][0];
//                l = bc1[i][1];
//                x = grid.getX(l);
//                y = grid.getY(l);
//                di[l] = 1;
//                b[l] = u_g(si, x, y);
//                for (int j = ig[l]; j < ig[l + 1]; j++)
//                    ggl[j] = 0;
//                for (int j = 0; j < n_jgg; j++)
//                    if (jg[j] == l)
//                        ggu[j] = 0;
//            }
//        }
//};
//}

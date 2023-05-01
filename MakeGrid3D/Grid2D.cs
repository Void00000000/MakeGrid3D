using OpenTK.Mathematics;
using MakeGrid3D;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace MakeGrid3D
{
    struct SubArea
    {
        public int wi; // Номер подобласти
        // Индексы в массивах Xw и Yw
        public int nx1, nx2;
        public int ny1, ny2;

        public SubArea(int wi, int nx1, int nx2, int ny1, int ny2)
        {
            this.wi = wi; 
            this.nx1 = nx1;
            this.nx2 = nx2; 
            this.ny1 = ny1;
            this.ny2 = ny2;
        }
    }

    enum NodeType : byte
    {
        Regular,
        Left,
        Right,
        Top,
        Bottom,
        Removed
    }

    // Прямоугольный конечный элемент
    struct Elem
    {
        public int wi;
        public int n1; public int n2; public int n3; public int n4;

        public Elem(int wi, int n1, int n2, int n3, int n4)
        {
            this.wi = wi;
            this.n1 = n1;
            this.n2 = n2;
            this.n3 = n3;
            this.n4 = n4;
        }
    }

    // Расчётная область
    class Area
    {
        // Количество подобластей
        public int Nareas { get; }
        // Количество различных номеров подобластей(материалов)
        public int Nmats { get; }
        // Границы области
        public float X0 { get; }
        public float Xn { get; }
        public float Y0 { get; }
        public float Yn { get; }
        public int NXw { get; }
        public int NYw { get; }
        // Массивы содержащие координаты подобластей
        public List<float> Xw { get; }
        public List<float> Yw { get; }
        // Массив, содержащий подобласти
        public List<SubArea> Mw { get; }

        public Area(string path)
        {
            try
            {
                using (TextReader reader = File.OpenText(path))
                {
                    string nXw_txt = reader.ReadLine();
                    NXw = int.Parse(nXw_txt);
                    Xw = new List<float>(NXw);
                    string Xwi_txt = reader.ReadLine();
                    string[] Xwi = Xwi_txt.Split(' ');
                    for (int i = 0; i < NXw; i++)
                        Xw.Add(float.Parse(Xwi[i], CultureInfo.InvariantCulture));

                    string nYw_txt = reader.ReadLine();
                    NYw = int.Parse(nYw_txt);
                    Yw = new List<float>(NYw);
                    string Ywi_txt = reader.ReadLine();
                    string[] Ywi = Ywi_txt.Split(' ');
                    for (int i = 0; i < NYw; i++)
                        Yw.Add(float.Parse(Ywi[i], CultureInfo.InvariantCulture));

                    X0 = Xw[0];
                    Xn = Xw[NXw - 1];
                    Y0 = Yw[0];
                    Yn = Yw[NYw - 1];

                    string nmat_nw_txt = reader.ReadLine();
                    string[] nmat_nw = nmat_nw_txt.Split(' ');
                    Nmats = int.Parse(nmat_nw[0]);
                    Nareas = int.Parse(nmat_nw[1]);
                    Mw = new List<SubArea>(Nareas);

                    for (int i = 0; i < Nareas; i++)
                    {
                        string Mwi_txt = reader.ReadLine();
                        string[] Mwi = Mwi_txt.Split(' ');
                        int wi = int.Parse(Mwi[0]) - 1;
                        int nx1 = int.Parse(Mwi[1]) - 1;
                        int nx2 = int.Parse(Mwi[2]) - 1;
                        int ny1 = int.Parse(Mwi[3]) - 1;
                        int ny2 = int.Parse(Mwi[4]) - 1;
                        Mw.Add(new SubArea(wi, nx1, nx2, ny1, ny2));
                    }
                }
            }
            catch (Exception e)
            {
                if (e is DirectoryNotFoundException || e is FileNotFoundException)
                {
                    ErrorHandler.FileReadingErrorMessage("Не удалось найти файл с сеткой");
                }
                else if (e is FormatException)
                {
                    ErrorHandler.FileReadingErrorMessage("Некорректный формат файла");
                }
                else
                {
                    ErrorHandler.FileReadingErrorMessage("Не удалось прочитать файл");
                }
            }
        }

        public int FindSubArea(float xElemMin, float xElemMax, float yElemMin, float yElemMax)
        {
            foreach (SubArea subArea in Mw)
            {
                float xAreaMin = Xw[subArea.nx1];
                float yAreaMin = Yw[subArea.ny1];
                float xAreaMax = Xw[subArea.nx2];
                float yAreaMax = Yw[subArea.ny2];

                if (xAreaMin <= xElemMin && xElemMin <= xAreaMax &&
                    yAreaMin <= yElemMin && yElemMin <= yAreaMax &&
                    xAreaMin <= xElemMax && xElemMax <= xAreaMax &&
                    yAreaMin <= yElemMax && yElemMax <= yAreaMax)
                {
                    return subArea.wi;
                }
            }
            return -1;
        }
    }

    class Grid2D
    {
        public Area Area { get; }
        public int Nnodes { get; }
        public int Nelems { get; }
        public int Nx { get; private set; }
        public int Ny { get; private set; }

        public List<Elem> Elems { get; }
        public List<Vector2> XY { get; }
        public List<List<NodeType>> IJ { get; }
        public List<int> removedNodes;

        public Grid2D(Area area, List<Vector2> XY, List<Elem> elems, List<List<NodeType>> IJ)
        {
            Area = area;
            this.XY = XY;
            Nnodes = XY.Count;
            Nelems = elems.Count;
            Elems = elems;
            this.IJ = IJ;
            Nx = IJ.Count;
            Ny = IJ[0].Count;
            removedNodes = new List<int>();
            for (int j = 0; j < Ny; j++)
                for (int i = 0; i < Nx; i++)
                {
                    if (IJ[i][j] == NodeType.Removed)
                        removedNodes.Add(j * Nx + i);
                }
        }

        // Создание регулярной сетки
        public Grid2D(string path, Area area) 
        {
            Area = area;
            List<float> X = new List<float>();
            List<float> Y = new List<float>();
            ReadGrid2D(path, X, Y);

            Nnodes = Nx * Ny;
            Nelems = (Nx - 1) * (Ny - 1);

            Elems = new List<Elem>(Nelems);
            XY = new List<Vector2>(Nnodes);
            removedNodes = new List<int>();

            for (int j = 0; j < Ny; j++)
                for (int i = 0; i < Nx; i++)
                {
                    Vector2 Vector2 = new Vector2(X[i], Y[j]);
                    XY.Add(Vector2);
                };
            
            for (int j = 0; j < Ny - 1; j++)
                for (int i = 0; i < Nx - 1; i++)
                {
                    int n1 = global_num(i, j);
                    int n2 = global_num(i + 1, j);
                    int n3 = global_num(i, j + 1);
                    int n4 = global_num(i + 1, j + 1);
                    float xmin = XY[n1].X; float xmax = XY[n4].X;
                    float ymin = XY[n1].Y; float ymax = XY[n4].Y;
                    int wi = area.FindSubArea(xmin, xmax, ymin, ymax);
                    Elem elem = new Elem(wi, n1, n2, n3, n4);
                    Elems.Add(elem);
                }

            IJ = new List<List<NodeType>>(Nx);
            for (int i = 0; i < Nx; i++)
            {
                IJ.Add(new List<NodeType>(Ny));
                for (int j = 0; j < Ny; j++)
                    IJ[i].Add(NodeType.Regular);
            }
        }

        private void ReadGrid2D(string path, List<float> X, List<float> Y)
        {
            List<int> nx, ny;
            List<float> qx, qy;
            Nx = 0; Ny = 0;
            try
            {
                using (TextReader reader = File.OpenText(path))
                {
                    string line = "*";
                    while (line != "")
                        line = reader.ReadLine();
                    nx = new List<int>(Area.NXw - 1);
                    string nxi_txt = reader.ReadLine();
                    string[] nxi = nxi_txt.Split(' ');
                    for (int i = 0; i < Area.NXw - 1; i++)
                    {
                        int current_nx = int.Parse(nxi[i]);
                        Nx += current_nx;
                        nx.Add(current_nx);
                    }
                    Nx++;

                    qx = new List<float>(Area.NXw - 1);
                    string qxi_txt = reader.ReadLine();
                    string[] qxi = qxi_txt.Split(' ');
                    for (int i = 0; i < Area.NXw - 1; i++)
                        qx.Add(float.Parse(qxi[i], CultureInfo.InvariantCulture));

                    ny = new List<int>(Area.NYw - 1);
                    string nyi_txt = reader.ReadLine();
                    string[] nyi = nyi_txt.Split(' ');
                    for (int i = 0; i < Area.NYw - 1; i++)
                    {
                        int current_ny = int.Parse(nyi[i]);
                        Ny += current_ny;
                        ny.Add(current_ny);
                    }
                    Ny++;

                    qy = new List<float>(Area.NYw - 1);
                    string qyi_txt = reader.ReadLine();
                    string[] qyi = qyi_txt.Split(' ');
                    for (int i = 0; i < Area.NYw - 1; i++)
                        qy.Add(float.Parse(qyi[i], CultureInfo.InvariantCulture));


                    X.Capacity = Nx;
                    Y.Capacity = Ny;
                    for (int i = 0; i < X.Capacity; i++)
                        X.Add(0);
                    for (int i = 0; i < Y.Capacity; i++)
                        Y.Add(0);

                    int ix0 = 0, iy0 = 0;
                    int jx = 0, jy = 0;

                    for (int i = 0; i < Area.NXw - 1; i++)
                        MakeGrid1D(X, Area.Xw[i], Area.Xw[i + 1], nx[i], qx[i], ref ix0, ref jx);
                    for (int i = 0; i < Area.NYw - 1; i++)
                        MakeGrid1D(Y, Area.Yw[i], Area.Yw[i + 1], ny[i], qy[i], ref iy0, ref jy);
                    X[Nx - 1] = Area.Xw[Area.NXw - 1];
                    Y[Ny - 1] = Area.Yw[Area.NYw - 1];
                }
            }
            catch (Exception e)
            {
                if (e is DirectoryNotFoundException || e is FileNotFoundException)
                {
                    ErrorHandler.FileReadingErrorMessage("Не удалось найти файл с сеткой");
                }
                else if (e is FormatException)
                {
                    ErrorHandler.FileReadingErrorMessage("Некорректный формат файла");
                }
                else
                {
                    ErrorHandler.FileReadingErrorMessage("Не удалось прочитать файл");
                }
            }
        }

        private void MakeGrid1D(List<float> X_Y, float left, float right, int n, float qxy, ref int i0, ref int j)
        {
            float h0;
            if (qxy - 1 < 1E-16)
                h0 = (right - left) / n;
            else
                h0 = (right - left) * (1 - qxy) / (1 - MathF.Pow(qxy, n));

            X_Y[i0] = left;
            j++;
            for (int i = i0 + 1; i < n + i0; i++)
            {
                X_Y[i] = X_Y[i - 1] + h0;
                h0 *= qxy;
            }
            i0 = n + i0;
        }

        public int global_num(int i, int j)
        {
            int l = j * Nx + i;
            if (removedNodes.Count == 0)
                return l;

            for (int k = 0; k < removedNodes.Count; k++)
                if (l < removedNodes[k]) return l - k;
            return l - removedNodes.Count;
            
        }
    }

    class IrregularGridMaker
    {
        public Grid2D Grid2D { get; set; }
        int Nx, Ny;
        public int StepSize { get; set; } = 1;
        public bool AllSteps { get; set; } = true;
        public float MaxAR { get; set; } = (float)Default.maxAR_width / Default.maxAR_height;

        public int CurrentI { get; set; } = 1;
        public int CurrentJ { get; set; } = 1;
        public IrregularGridMaker(Grid2D grid2D)
        {
            Grid2D = grid2D;
            Nx = grid2D.Nx;
            Ny = grid2D.Ny;
        }

        // Calculate Aspect Ratio
        private float CalcAR(int n1, int n4)
        {
            float x1 = Grid2D.XY[n1].X;
            float y1 = Grid2D.XY[n1].Y;

            float x2 = Grid2D.XY[n4].X;
            float y2 = Grid2D.XY[n4].Y;

            float width = (x2 - x1);
            float height = (y2 - y1);
            return width / height;
        }

        // a1 >= a2
        private bool CompareAR(float a1, float a2)
        {
            if (a2 < 1f) a2 = 1f / a2;
            if (a1 < 1f) a1 = 1f / a1;
            return a1 >= a2;
        }

        private List<List<NodeType>> MakeUnStructedMatrix()
        {
            List<List<NodeType>> IJ_new = new List<List<NodeType>>(Nx);
            for (int i = 0; i < Nx; i++)
            {
                IJ_new.Add(new List<NodeType>(Ny));
                for (int j = 0; j < Ny; j++)
                    IJ_new[i].Add(Grid2D.IJ[i][j]);
            }
            int removedCount = 0;
            bool full_loop = true;
            // Изменение матрицы IJ
            for (int j = CurrentJ; j < Ny - 1; j++)
            {
                for (int i = CurrentI; i < Nx - 1; i++)
                {
                    if (!AllSteps && removedCount >= StepSize) { CurrentI = i; CurrentJ = j; full_loop = false; goto EXIT_LOOP; }
                    if (IJ_new[i][j] != NodeType.Regular)
                        continue;
                    int n = Grid2D.global_num(i, j);
                    int nlb = Grid2D.global_num(i - 1, j - 1);
                    int nb = Grid2D.global_num(i, j - 1);
                    int nr = Grid2D.global_num(i + 1, j);
                    int nru = Grid2D.global_num(i + 1, j + 1);
                    int nl = Grid2D.global_num(i - 1, j);
                    int nu = Grid2D.global_num(i, j + 1);

                    float alb = CalcAR(nlb, n);
                    float arb = CalcAR(nb, nr);
                    float ab = CalcAR(nlb, nr);
                    float aru = CalcAR(n, nru);
                    float ar = CalcAR(nb, nru);
                    float alu = CalcAR(nl, nu);
                    float au = CalcAR(nl, nru);
                    float al = CalcAR(nlb, nu);

                    // bottom
                    if (CompareAR(alb, MaxAR) && CompareAR(arb, MaxAR) &&
                        CompareAR(alb, ab) && CompareAR(arb, ab))
                    {
                        IJ_new[i][j] = NodeType.Top;
                        for (int jk = j - 1; jk >= 0; jk--)
                        {
                            IJ_new[i][jk] = NodeType.Removed;
                            removedCount++;
                        }
                        continue;
                    }
                    // top
                    if (CompareAR(alu, MaxAR) && CompareAR(aru, MaxAR) &&
                        CompareAR(alu, au) && CompareAR(aru, au))
                    {
                        IJ_new[i][j] = NodeType.Bottom;
                        for (int jk = j + 1; jk < Ny; jk++)
                        {   
                            IJ_new[i][jk] = NodeType.Removed;
                            removedCount++;
                        }
                        continue;
                    }
                    // left
                    if (CompareAR(alb, MaxAR) && CompareAR(alu, MaxAR) &&
                        CompareAR(alb, al) && CompareAR(alu, al))
                    {
                        IJ_new[i][j] = NodeType.Right;
                        for (int ik = i - 1; ik >= 0; ik--)
                        {
                            IJ_new[ik][j] = NodeType.Removed;
                            removedCount++;
                        }
                        continue;
                    }
                    // right
                    if (CompareAR(arb, MaxAR) && CompareAR(aru, MaxAR) &&
                        CompareAR(arb, ar) && CompareAR(aru, ar))
                    {
                        IJ_new[i][j] = NodeType.Left;
                        for (int ik = i + 1; ik < Nx; ik++)
                        {
                            IJ_new[ik][j] = NodeType.Removed;
                            removedCount++;
                        }
                        continue;
                    }
                }
                CurrentI = 1;
            }
            EXIT_LOOP:
            if (full_loop)
            {
                CurrentI = Nx - 1;
                CurrentJ = Ny - 1;
            }
            return IJ_new;
        }

        public Grid2D MakeUnStructedGrid()
        {
            List<List<NodeType>> IJ_new = MakeUnStructedMatrix();
            List<Vector2> XY_new = new List<Vector2>();
            List<Elem> Elems_new = new List<Elem>();

            for (int j = 0; j < Ny; j++)
                for (int i = 0; i < Nx; i++)
                {
                    if (IJ_new[i][j] == NodeType.Removed)
                        continue;
                    int n = Grid2D.global_num(i, j);
                    XY_new.Add(new Vector2(Grid2D.XY[n].X, Grid2D.XY[n].Y));
                }
            Elems_new = new List<Elem>();

            for (int j = 0; j < Ny - 1; j++)
                for (int i = 0; i < Nx - 1; i++)
                {
                    if (IJ_new[i][j] == NodeType.Removed || IJ_new[i][j] == NodeType.Left || IJ_new[i][j] == NodeType.Bottom)
                        continue;
                    int ik;
                    int ik1 = i + 1;
                    while (ik1 < Nx && (IJ_new[ik1][j] == NodeType.Removed || IJ_new[ik1][j] == NodeType.Bottom))
                    {
                        ik1++;
                    }
                    int ik2 = i + 1;
                    while (ik2 < Nx && IJ_new[ik2][j] == NodeType.Removed)
                    {
                        ik2++;
                    }
                    if (ik1 > ik2) { ik = ik1; } else { ik = ik2; }
                    if (ik >= Nx) { ik = i + 1; }


                    int jk;
                    int jk1 = j + 1;
                    while (jk1 < Ny && IJ_new[i][jk1] == NodeType.Removed)
                    {
                        jk1++;
                    }
                    int jk2 = j + 1;
                    while (jk2 < Ny && IJ_new[ik][jk2] == NodeType.Removed)
                    {
                        jk2++;
                    }
                    if (jk1 > jk2) { jk = jk1; } else { jk = jk2; }
                    if (jk >= Ny) { jk = j + 1; }
                    int n1 = Grid2D.global_num(i, j);
                    int n2 = Grid2D.global_num(ik, j);
                    int n3 = Grid2D.global_num(i, jk);
                    int n4 = Grid2D.global_num(ik, jk);

                    // TODO: OPTIMIZE THAT
                    int n1_new = XY_new.FindIndex(v => MathF.Abs(v.X - Grid2D.XY[n1].X) < 1e-14f && MathF.Abs(v.Y - Grid2D.XY[n1].Y) < 1e-14f);
                    int n2_new = XY_new.FindIndex(v => MathF.Abs(v.X - Grid2D.XY[n2].X) < 1e-14f && MathF.Abs(v.Y - Grid2D.XY[n2].Y) < 1e-14f);
                    int n3_new = XY_new.FindIndex(v => MathF.Abs(v.X - Grid2D.XY[n3].X) < 1e-14f && MathF.Abs(v.Y - Grid2D.XY[n3].Y) < 1e-14f);
                    int n4_new = XY_new.FindIndex(v => MathF.Abs(v.X - Grid2D.XY[n4].X) < 1e-14f && MathF.Abs(v.Y - Grid2D.XY[n4].Y) < 1e-14f);
                    float xmin = XY_new[n1_new].X;
                    float xmax = XY_new[n4_new].X;
                    float ymin = XY_new[n1_new].Y;
                    float ymax = XY_new[n4_new].Y;
                    int wi = Grid2D.Area.FindSubArea(xmin, xmax, ymin, ymax);
                    Elems_new.Add(new Elem(wi, n1_new, n2_new, n3_new, n4_new));
                }
            return new Grid2D(Grid2D.Area, XY_new, Elems_new, IJ_new);
        }
    }
}
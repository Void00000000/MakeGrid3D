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

    enum Direction
    {
        Left,
        Right,
        Top,
        Bottom,
    }

    // Прямоугольный конечный элемент
    struct Elem
    {
        public int wi;
        public int n1; public int n2; public int n3; public int n4; // граничные узлы
        public int n5 = -1;

        public Elem(int wi, int n1, int n2, int n3, int n4)
        {
            this.wi = wi;
            this.n1 = n1;
            this.n2 = n2;
            this.n3 = n3;
            this.n4 = n4;
        }

        public Elem(int wi, int n1, int n2, int n3, int n4, int n5) : this(wi, n1, n2, n3, n4)
        {
            this.n5 = n5;
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

        public Vector2i global_ij(int node_num)
        {
            int i, j;
            if (removedNodes.Count == 0)
            {
                i = node_num % Nx;
                j = node_num / Nx;
                return new Vector2i(i, j);
            }

            for (int k = 0; k < removedNodes.Count; k++)
            {
                int reg_i = (removedNodes[k]) % Nx;
                int reg_j = (removedNodes[k]) / Nx;
                // TODO: Скорее всего неправильно
                if (node_num < global_num(reg_i, reg_j) + 1)
                {
                    i = (node_num + k) % Nx;
                    j = (node_num + k) / Nx;
                    return new Vector2i(i, j);
                }
            }
            i = (node_num + removedNodes.Count) % Nx;
            j = (node_num + removedNodes.Count) / Nx;
            return new Vector2i(i, j);
        }

        public bool FindElem(float x, float y, ref Elem foundElem)
        {
            foreach (Elem elem in Elems)
            {
                float xElemMin = XY[elem.n1].X;
                float yElemMin = XY[elem.n1].Y;
                float xElemMax = XY[elem.n4].X;
                float yElemMax = XY[elem.n4].Y;
                if (x >= xElemMin && x <= xElemMax && y >= yElemMin && y <= yElemMax)
                {
                    foundElem = elem;
                    return true;
                }
            }
            return false;
        }
    }

    class IrregularGridMaker
    {
        public Grid2D Grid2D { get; set; }
        int Nx, Ny;
        public int StepSize { get; set; } = 1;
        public float MaxAR { get; set; } = (float)Default.maxAR_width / Default.maxAR_height;

        public int NodeI { get; set; } = Default.I;
        public int NodeJ { get; set; } = Default.J;

        public int I { get; set; } = Default.I;
        public int J { get; set; } = Default.J;
        
        // По приоритету (от высшего к низшему ^ слева направо)
        public Direction[] Dirs = { Default.dir1, Default.dir2, Default.dir3, Default.dir4 };
        private int dirs_index = 0;
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
 

        private bool MergeLeft(List<List<NodeType>> IJ_new)
        {

        }

        private void MakeUnStructedMatrix(List<List<NodeType>> IJ_new)
        {
            bool end;
            switch (Dirs[dirs_index])
            {
                case Direction.Left:
                    end = MergeLeft(IJ_new); 
                    break;
                case Direction.Right:
                    end = MergeRight(IJ_new);
                    break;
                case Direction.Bottom:
                    end = MergeBottom(IJ_new);
                    break;
                case Direction.Top:
                    end = MergeTop(IJ_new);
                    break;
            }
            if (end)
                dirs_index++;
            if (dirs_index >= Dirs.Length) { 
                dirs_index = 0;
                do
                {
                    NodeI++;
                    if (NodeI >= Nx - 1)
                    {
                        NodeI = 0;
                        NodeJ++;
                    }
                }
                while (IJ_new[NodeI][NodeJ] == NodeType.Removed);
            }
            I = NodeI;
            J = NodeJ;
        }

        public Grid2D MakeUnStructedGrid()
        {
            List<List<NodeType>> IJ_new = new List<List<NodeType>>(Nx);
            for (int i = 0; i < Nx; i++)
            {
                IJ_new.Add(new List<NodeType>(Ny));
                for (int j = 0; j < Ny; j++)
                    IJ_new[i].Add(Grid2D.IJ[i][j]);
            }
            MakeUnStructedMatrix(IJ_new);
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

            int n1, n2, n3, n4, n5;
            for (int j = 0; j < Ny - 1; j++)
                for (int i = 0; i < Nx - 1; i++)
                {
                    if (IJ_new[i][j] == NodeType.Removed || IJ_new[i][j] == NodeType.Left || IJ_new[i][j] == NodeType.Bottom)
                        continue;
                    n5 = -1;
                    n1 = Grid2D.global_num(i, j);

                    int ik = i + 1;
                    // Bottom line
                    while (IJ_new[ik][j] == NodeType.Removed || IJ_new[ik][j] == NodeType.Bottom)
                    {
                        if (IJ_new[ik][j] == NodeType.Bottom)
                            n5 = Grid2D.global_num(ik, j);
                        ik++;
                    }
                    n2 = Grid2D.global_num(ik, j);

                    int jk = j + 1;
                    // Left line
                    while (IJ_new[i][jk] == NodeType.Removed || IJ_new[i][jk] == NodeType.Left) {
                        if (IJ_new[i][jk] == NodeType.Left)
                            n5 = Grid2D.global_num(i, jk);
                        jk++;
                    }
                    n3 = Grid2D.global_num(i, jk);
                    n4 = Grid2D.global_num(ik, jk);

                    // Top line
                    for (int ikk = i; ikk < ik; ikk++)
                    {
                        if (IJ_new[ikk][jk] == NodeType.Top)
                            n5 = Grid2D.global_num(ikk, jk);
                    }

                    // Right line
                    for (int jkk = j; jkk < jk; jkk++)
                    {
                        if (IJ_new[ik][jkk] == NodeType.Right)
                            n5 = Grid2D.global_num(ik, jkk);
                    }

                    // TODO: OPTIMIZE THAT
                    int n1_new, n2_new, n3_new, n4_new, n5_new;
                    n1_new = XY_new.FindIndex(v => MathF.Abs(v.X - Grid2D.XY[n1].X) < 1e-14f && MathF.Abs(v.Y - Grid2D.XY[n1].Y) < 1e-14f);
                    n2_new = XY_new.FindIndex(v => MathF.Abs(v.X - Grid2D.XY[n2].X) < 1e-14f && MathF.Abs(v.Y - Grid2D.XY[n2].Y) < 1e-14f);
                    n3_new = XY_new.FindIndex(v => MathF.Abs(v.X - Grid2D.XY[n3].X) < 1e-14f && MathF.Abs(v.Y - Grid2D.XY[n3].Y) < 1e-14f);
                    n4_new = XY_new.FindIndex(v => MathF.Abs(v.X - Grid2D.XY[n4].X) < 1e-14f && MathF.Abs(v.Y - Grid2D.XY[n4].Y) < 1e-14f);
                    if (n5 >= 0)
                        n5_new = XY_new.FindIndex(v => MathF.Abs(v.X - Grid2D.XY[n5].X) < 1e-14f && MathF.Abs(v.Y - Grid2D.XY[n5].Y) < 1e-14f);
                    else
                        n5_new = -1;
                    float xmin = XY_new[n1_new].X;
                    float xmax = XY_new[n4_new].X;
                    float ymin = XY_new[n1_new].Y;
                    float ymax = XY_new[n4_new].Y;
                    int wi = Grid2D.Area.FindSubArea(xmin, xmax, ymin, ymax);
                    Elems_new.Add(new Elem(wi, n1_new, n2_new, n3_new, n4_new, n5_new));
                }
            return new Grid2D(Grid2D.Area, XY_new, Elems_new, IJ_new);
        }
    }
}
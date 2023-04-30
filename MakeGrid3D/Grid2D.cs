using OpenTK.Mathematics;
using MakeGrid3D;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Reflection.PortableExecutable;

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
        public int nXw { get; }
        public int nYw { get; }
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
                    nXw = int.Parse(nXw_txt);
                    Xw = new List<float>(nXw);
                    string Xwi_txt = reader.ReadLine();
                    string[] Xwi = Xwi_txt.Split(' ');
                    for (int i = 0; i < nXw; i++)
                        Xw.Add(float.Parse(Xwi[i], CultureInfo.InvariantCulture));

                    string nYw_txt = reader.ReadLine();
                    nYw = int.Parse(nYw_txt);
                    Yw = new List<float>(nYw);
                    string Ywi_txt = reader.ReadLine();
                    string[] Ywi = Ywi_txt.Split(' ');
                    for (int i = 0; i < nYw; i++)
                        Yw.Add(float.Parse(Ywi[i], CultureInfo.InvariantCulture));

                    X0 = Xw[0];
                    Xn = Xw[nXw - 1];
                    Y0 = Yw[0];
                    Yn = Yw[nYw - 1];

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
        public Area area { get; }
        public int Nnodes { get; }
        public int Nelems { get; }
        public int Nx { get; private set; }
        public int Ny { get; private set; }

        public List<Elem> Elems { get; }
        public List<Vector2> XY { get; }
        public List<List<NodeType>> IJ { get; }

        public Grid2D(Area area, List<Vector2> XY, List<Elem> elems, List<List<NodeType>> IJ)
        {
            this.area = area;
            this.XY = XY;
            Nnodes = XY.Count;
            Nelems = elems.Count;
            this.Elems = elems;
            this.IJ = IJ;
            Nx = IJ.Count;
            Ny = IJ[0].Count;
        }

        // Создание регулярной сетки
        public Grid2D(string path, Area area) 
        {
            this.area = area;
            List<float> X = new List<float>();
            List<float> Y = new List<float>();
            ReadGrid2D(path, X, Y);

            Nnodes = Nx * Ny;
            Nelems = (Nx - 1) * (Ny - 1);

            Elems = new List<Elem>(Nelems);
            XY = new List<Vector2>(Nnodes);

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
                    nx = new List<int>(area.nXw - 1);
                    string nxi_txt = reader.ReadLine();
                    string[] nxi = nxi_txt.Split(' ');
                    for (int i = 0; i < area.nXw - 1; i++)
                    {
                        int current_nx = int.Parse(nxi[i]);
                        Nx += current_nx;
                        nx.Add(current_nx);
                    }
                    Nx++;

                    qx = new List<float>(area.nXw - 1);
                    string qxi_txt = reader.ReadLine();
                    string[] qxi = qxi_txt.Split(' ');
                    for (int i = 0; i < area.nXw - 1; i++)
                        qx.Add(float.Parse(qxi[i], CultureInfo.InvariantCulture));

                    ny = new List<int>(area.nYw - 1);
                    string nyi_txt = reader.ReadLine();
                    string[] nyi = nyi_txt.Split(' ');
                    for (int i = 0; i < area.nYw - 1; i++)
                    {
                        int current_ny = int.Parse(nyi[i]);
                        Ny += current_ny;
                        ny.Add(current_ny);
                    }
                    Ny++;

                    qy = new List<float>(area.nYw - 1);
                    string qyi_txt = reader.ReadLine();
                    string[] qyi = qyi_txt.Split(' ');
                    for (int i = 0; i < area.nYw - 1; i++)
                        qy.Add(float.Parse(qyi[i], CultureInfo.InvariantCulture));


                    X.Capacity = Nx;
                    Y.Capacity = Ny;
                    for (int i = 0; i < X.Capacity; i++)
                        X.Add(0);
                    for (int i = 0; i < Y.Capacity; i++)
                        Y.Add(0);

                    int ix0 = 0, iy0 = 0;
                    int jx = 0, jy = 0;

                    for (int i = 0; i < area.nXw - 1; i++)
                        MakeGrid1D(X, area.Xw[i], area.Xw[i + 1], nx[i], qx[i], ref ix0, ref jx);
                    for (int i = 0; i < area.nYw - 1; i++)
                        MakeGrid1D(Y, area.Yw[i], area.Yw[i + 1], ny[i], qy[i], ref iy0, ref jy);
                    X[Nx - 1] = area.Xw[area.nXw - 1];
                    Y[Ny - 1] = area.Yw[area.nYw - 1];
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
            return j * Nx + i;
        }
    }

    class IrregularGridMaker
    {
        public Grid2D grid2D;
        int Nx, Ny;
        public IrregularGridMaker(Grid2D grid2D)
        {
            this.grid2D = grid2D;
            Nx = grid2D.Nx;
            Ny = grid2D.Ny;
        }

        // Calculate Aspect Ratio
        private float CalcAR(int n1, int n4)
        {
            float x1 = grid2D.XY[n1].X;
            float y1 = grid2D.XY[n1].Y;

            float x2 = grid2D.XY[n4].X;
            float y2 = grid2D.XY[n4].Y;

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
                    IJ_new[i].Add(grid2D.IJ[i][j]);
            }
            // Изменение матрицы IJ
            for (int j = 1; j < Ny - 1; j++)
            {
                for (int i = 1; i < Nx - 1; i++)
                {
                    if (IJ_new[i][j] != NodeType.Regular)
                        continue;
                    int n = grid2D.global_num(i, j);
                    int nlb = grid2D.global_num(i - 1, j - 1);
                    int nb = grid2D.global_num(i, j - 1);
                    int nr = grid2D.global_num(i + 1, j);
                    int nru = grid2D.global_num(i + 1, j + 1);
                    int nl = grid2D.global_num(i - 1, j);
                    int nu = grid2D.global_num(i, j + 1);

                    float alb = CalcAR(nlb, n);
                    float arb = CalcAR(nb, nr);
                    float ab = CalcAR(nlb, nr);
                    float aru = CalcAR(n, nru);
                    float ar = CalcAR(nb, nru);
                    float alu = CalcAR(nl, nu);
                    float au = CalcAR(nl, nru);
                    float al = CalcAR(nlb, nu);

                    // bottom
                    if (CompareAR(alb, BufferClass.maxAR) && CompareAR(arb, BufferClass.maxAR) &&
                        CompareAR(alb, ab) && CompareAR(arb, ab))
                    {
                        IJ_new[i][j] = NodeType.Top;
                        for (int jk = j - 1; jk >= 0; jk--)
                        {
                            IJ_new[i][jk] = NodeType.Removed;
                        }
                        continue;
                    }
                    // top
                    if (CompareAR(alu, BufferClass.maxAR) && CompareAR(aru, BufferClass.maxAR) &&
                        CompareAR(alu, au) && CompareAR(aru, au))
                    {
                        IJ_new[i][j] = NodeType.Bottom;
                        for (int jk = j + 1; jk < Ny; jk++)
                        {   
                            IJ_new[i][jk] = NodeType.Removed;
                        }
                        continue;
                    }
                    // left
                    if (CompareAR(alb, BufferClass.maxAR) && CompareAR(alu, BufferClass.maxAR) &&
                        CompareAR(alb, al) && CompareAR(alu, al))
                    {
                        IJ_new[i][j] = NodeType.Right;
                        for (int ik = i - 1; ik >= 0; ik--)
                        {
                            IJ_new[ik][j] = NodeType.Removed;
                        }
                        continue;
                    }
                    // right
                    if (CompareAR(arb, BufferClass.maxAR) && CompareAR(aru, BufferClass.maxAR) &&
                        CompareAR(arb, ar) && CompareAR(aru, ar))
                    {
                        IJ_new[i][j] = NodeType.Left;
                        for (int ik = i + 1; ik < Nx; ik++)
                        {
                            IJ_new[ik][j] = NodeType.Removed;
                        }
                        continue;
                    }
                }
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
                    int n = grid2D.global_num(i, j);
                    XY_new.Add(new Vector2(grid2D.XY[n].X, grid2D.XY[n].Y));
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
                    int n1 = grid2D.global_num(i, j);
                    int n2 = grid2D.global_num(ik, j);
                    int n3 = grid2D.global_num(i, jk);
                    int n4 = grid2D.global_num(ik, jk);

                    // TODO: OPTIMIZE THAT
                    int n1_new = XY_new.FindIndex(v => MathF.Abs(v.X - grid2D.XY[n1].X) < 1e-14f && MathF.Abs(v.Y - grid2D.XY[n1].Y) < 1e-14f);
                    int n2_new = XY_new.FindIndex(v => MathF.Abs(v.X - grid2D.XY[n2].X) < 1e-14f && MathF.Abs(v.Y - grid2D.XY[n2].Y) < 1e-14f);
                    int n3_new = XY_new.FindIndex(v => MathF.Abs(v.X - grid2D.XY[n3].X) < 1e-14f && MathF.Abs(v.Y - grid2D.XY[n3].Y) < 1e-14f);
                    int n4_new = XY_new.FindIndex(v => MathF.Abs(v.X - grid2D.XY[n4].X) < 1e-14f && MathF.Abs(v.Y - grid2D.XY[n4].Y) < 1e-14f);
                    float xmin = XY_new[n1_new].X;
                    float xmax = XY_new[n4_new].X;
                    float ymin = XY_new[n1_new].Y;
                    float ymax = XY_new[n4_new].Y;
                    int wi = grid2D.area.FindSubArea(xmin, xmax, ymin, ymax);
                    Elems_new.Add(new Elem(wi, n1_new, n2_new, n3_new, n4_new));
                }
            return new Grid2D(grid2D.area, XY_new, Elems_new, IJ_new);
        }
    }
}
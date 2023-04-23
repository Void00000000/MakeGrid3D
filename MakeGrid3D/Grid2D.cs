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

    struct Elem5
    {
        public NodeType e;
        public int n1; public int n2; public int n3; public int n4; public int n5;

        public Elem5(NodeType e, int n1, int n2, int n3, int n4, int n5=-1)
        {
            this.e = e;
            this.n1 = n1;
            this.n2 = n2;
            this.n3 = n3;
            this.n4 = n4;
            this.n5 = n5;
        }
    }


    class Grid2D
    {
        public int Nareas { get; private set; }
        public int Nnodes { get;}
        public int Nelems { get;}
        public int UnStrNnodes { get; private set; }
        public int UnStrNelems { get; private set; }
        public int Nx { get; private set; }
        public int Ny { get; private set; }

        public float X0 { get;}
        public float Xn { get;}
        public float Y0 { get;}
        public float Yn { get;}

        public List<Elem5> Elems { get; private set; }
        public List<Elem5> UnStrElems { get; private set; }
        public List<Vector2> XY { get; private set; }
        public List<Vector2> UnStrXY { get; private set; }
        private List<float> Xw, Yw; // Массивы содержащие координаты подобластей
        public List<SubArea> Mw { get; private set; } // Массив, содержащий подобласти
        public List<int> IXw { get; private set; }
        public List<int> IYw { get; private set; }
        public List<List<NodeType>> IJ { get; private set; }

        public Grid2D(string path) 
        {
            List<float> X = new List<float>();
            List<float> Y = new List<float>();
            ReadGrid2D(path, X, Y);

            Nnodes = Nx * Ny;
            Nelems = (Nx - 1) * (Ny - 1);

            Elems = new List<Elem5>(Nelems);
            XY = new List<Vector2>(Nnodes);

            for (int j = 0; j < Ny - 1; j++)
                for (int i = 0; i < Nx - 1; i++)
                {
                    int n1 = global_num(i, j);
                    int n2 = global_num(i + 1, j);
                    int n3 = global_num(i, j + 1);
                    int n4 = global_num(i + 1, j + 1);
                    Elem5 elem = new Elem5(NodeType.Regular, n1, n2, n3, n4);
                    Elems.Add(elem);
                }

            for (int j = 0; j < Ny; j++)
                for (int i = 0; i < Nx; i++)
                {
                    Vector2 Vector2 = new Vector2(X[i], Y[j]);
                    XY.Add(Vector2);
                }


            X0 = XY[0].X;
            Xn = XY[XY.Count - 1].X;
            Y0 = XY[0].Y;
            Yn = XY[XY.Count - 1].Y;

            IJ = new List<List<NodeType>>(Nx);
            for (int i = 0; i < Nx; i++)
            {
                IJ.Add(new List<NodeType>(Ny));
                for (int j = 0; j < Ny; j++)
                    IJ[i].Add(NodeType.Regular);
            }
        }

        private void MakeGrid1D(List<float> X_Y, float left, float right, int n, float qxy, ref int i0, ref int j,
            List<int> IXYw)
        {
            float h0;
            if (qxy - 1 < 1E-16)
                h0 = (right - left) / n;
            else
                h0 = (right - left) * (1 - qxy) / (1 - MathF.Pow(qxy, n));

            X_Y[i0] = left;
            IXYw[j] = i0; j++;
            for (int i = i0 + 1; i < n + i0; i++)
            {
                X_Y[i] = X_Y[i - 1] + h0;
                h0 *= qxy;
            }
            i0 = n + i0;
        }



        private void ReadGrid2D(string path, List<float> X, List<float> Y)
        {
            int nXw, nYw;
            List<int> nx, ny;
            List<float> qx, qy;
            Nx = 0; Ny = 0;
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

                    string nw_txt = reader.ReadLine();
                    Nareas = int.Parse(nw_txt);
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

                    nx = new List<int>(nXw - 1);
                    string nxi_txt = reader.ReadLine();
                    string[] nxi = nxi_txt.Split(' ');
                    for (int i = 0; i < nXw - 1; i++)
                    {
                        int current_nx = int.Parse(nxi[i]);
                        Nx += current_nx;
                        nx.Add(current_nx);
                    }
                    Nx++;

                    qx = new List<float>(nXw - 1);
                    string qxi_txt = reader.ReadLine();
                    string[] qxi = qxi_txt.Split(' ');
                    for (int i = 0; i < nXw - 1; i++)
                        qx.Add(float.Parse(qxi[i], CultureInfo.InvariantCulture));

                    ny = new List<int>(nYw - 1);
                    string nyi_txt = reader.ReadLine();
                    string[] nyi = nyi_txt.Split(' ');
                    for (int i = 0; i < nYw - 1; i++)
                    {
                        int current_ny = int.Parse(nyi[i]);
                        Ny += current_ny;
                        ny.Add(current_ny);
                    }
                    Ny++;

                    qy = new List<float>(nYw - 1);
                    string qyi_txt = reader.ReadLine();
                    string[] qyi = qyi_txt.Split(' ');
                    for (int i = 0; i < nYw - 1; i++)
                        qy.Add(float.Parse(qyi[i], CultureInfo.InvariantCulture));


                    X.Capacity = Nx;
                    Y.Capacity = Ny;
                    for (int i = 0; i < X.Capacity; i++)
                        X.Add(0);
                    for (int i = 0; i < Y.Capacity; i++)
                        Y.Add(0);

                    int ix0 = 0, iy0 = 0;
                    int jx = 0, jy = 0;
                    IXw = new List<int>(nXw);
                    IYw = new List<int>(nYw);
                    for (int i = 0; i < IXw.Capacity; i++)
                        IXw.Add(0);
                    for (int i = 0; i < IYw.Capacity; i++)
                        IYw.Add(0);

                    for (int i = 0; i < nXw - 1; i++)
                        MakeGrid1D(X, Xw[i], Xw[i + 1], nx[i], qx[i], ref ix0, ref jx, IXw);
                    for (int i = 0; i < nYw - 1; i++)
                        MakeGrid1D(Y, Yw[i], Yw[i + 1], ny[i], qy[i], ref iy0, ref jy, IYw);
                    X[Nx - 1] = Xw[nXw - 1];
                    Y[Ny - 1] = Yw[nYw - 1];
                    IXw[nXw - 1] = ix0;
                    IYw[nYw - 1] = iy0;

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

        public int global_num(int i, int j)
        {
            return j * Nx + i;
        }

        // Calculate Aspect Ratio
        private float CalcAR(int n1, int n4)
        {
            float x1 = XY[n1].X;
            float y1 = XY[n1].Y;

            float x2 = XY[n4].X;
            float y2 = XY[n4].Y;

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

        private void MakeUnStructedMatrix()
        {
            IJ = new List<List<NodeType>>(Nx);
            for (int i = 0; i < Nx; i++)
            {
                IJ.Add(new List<NodeType>(Ny));
                for (int j = 0; j < Ny; j++)
                    IJ[i].Add(NodeType.Regular);
            }
            // Изменение матрицы IJ
            for (int j = 1; j < Ny - 1; j++)
            {
                for (int i = 1; i < Nx - 1; i++)
                {
                    if (IJ[i][j] != NodeType.Regular)
                        continue;
                    int n = global_num(i, j);
                    int nlb = global_num(i - 1, j - 1);
                    int nb = global_num(i, j - 1);
                    int nr = global_num(i + 1, j);
                    int nru = global_num(i + 1, j + 1);
                    int nl = global_num(i - 1, j);
                    int nu = global_num(i, j + 1);

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
                        IJ[i][j] = NodeType.Top;
                        for (int jk = j - 1; jk >=0; jk--)
                        {
                            IJ[i][jk] = NodeType.Removed;
                        }
                        continue;
                    }
                    // top
                    if (CompareAR(alu, BufferClass.maxAR) && CompareAR(aru, BufferClass.maxAR) &&
                        CompareAR(alu, au) && CompareAR(aru, au))
                    {
                        IJ[i][j] = NodeType.Bottom;
                        for (int jk = j + 1; jk < Ny; jk++)
                        {
                            IJ[i][jk] = NodeType.Removed;
                        }
                        continue;
                    }
                    // left
                    if (CompareAR(alb, BufferClass.maxAR) && CompareAR(alu, BufferClass.maxAR) &&
                        CompareAR(alb, al) && CompareAR(alu, al))
                    {
                        IJ[i][j] = NodeType.Right;
                        for (int ik = i - 1; ik >= 0; ik--)
                        {
                            IJ[ik][j] = NodeType.Removed;
                        }
                        continue;
                    }
                    // right
                    if (CompareAR(arb, BufferClass.maxAR) && CompareAR(aru, BufferClass.maxAR) &&
                        CompareAR(arb, ar) && CompareAR(aru, ar))
                    {
                        IJ[i][j] = NodeType.Left;
                        for (int ik = i + 1; ik < Nx; ik++)
                        {
                            IJ[ik][j] = NodeType.Removed;
                        }
                        continue;
                    }
                }
            }
        }

        public void MakeUnStructedGrid()
        {
            MakeUnStructedMatrix();
            UnStrXY = new List<Vector2>();
            for (int j = 0; j < Ny; j++)
                for (int i = 0; i < Nx; i++)
                {
                    if (IJ[i][j] == NodeType.Removed)
                        continue;
                    int n = global_num(i, j);
                    UnStrXY.Add(new Vector2(XY[n].X, XY[n].Y));
                }
            UnStrElems = new List<Elem5>();
            
            for (int j = 0; j < Ny - 1; j++)
                for (int i = 0; i < Nx - 1; i++)
                {
                    if (IJ[i][j] == NodeType.Removed || IJ[i][j] == NodeType.Left || IJ[i][j] == NodeType.Bottom)
                        continue;
                    int ik;
                    int ik1 = i + 1;
                    while (ik1 < Nx && (IJ[ik1][j] == NodeType.Removed || IJ[ik1][j] == NodeType.Bottom))
                    {
                        ik1++;
                    }
                    int ik2 = i + 1;
                    while (ik2 < Nx && IJ[ik2][j] == NodeType.Removed)
                    {
                        ik2++;
                    }
                    if (ik1 > ik2) { ik = ik1; } else { ik = ik2; }
                    if (ik >= Nx ) { ik = i + 1; }


                    int jk;
                    int jk1 = j + 1;
                    while (jk1 < Ny && IJ[i][jk1] == NodeType.Removed)
                    {
                        jk1++;
                    }
                    int jk2 = j + 1;
                    while (jk2 < Ny && IJ[ik][jk2] == NodeType.Removed)
                    {
                        jk2++;
                    }
                    if (jk1 > jk2) { jk = jk1; } else { jk= jk2; }
                    if (jk >= Ny) { jk = j + 1; }
                    int n1 = global_num(i, j);
                    int n2 = global_num(ik, j);
                    int n3 = global_num(i, jk);
                    int n4 = global_num(ik, jk);

                    // TODO: OPTIMIZE THAT
                    int n1_new = UnStrXY.FindIndex(v => MathF.Abs(v.X - XY[n1].X) < 1e-14f && MathF.Abs(v.Y - XY[n1].Y) < 1e-14f);
                    int n2_new = UnStrXY.FindIndex(v => MathF.Abs(v.X - XY[n2].X) < 1e-14f && MathF.Abs(v.Y - XY[n2].Y) < 1e-14f);
                    int n3_new = UnStrXY.FindIndex(v => MathF.Abs(v.X - XY[n3].X) < 1e-14f && MathF.Abs(v.Y - XY[n3].Y) < 1e-14f);
                    int n4_new = UnStrXY.FindIndex(v => MathF.Abs(v.X - XY[n4].X) < 1e-14f && MathF.Abs(v.Y - XY[n4].Y) < 1e-14f);
                    UnStrElems.Add(new Elem5(NodeType.Regular, n1_new, n2_new, n3_new, n4_new));
                }
            UnStrNnodes = UnStrXY.Count;
            UnStrNelems = UnStrElems.Count;
        }
    }
}
using OpenTK.Mathematics;
using MakeGrid3D;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace MakeGrid3D
{
    using ByteMat2D = List<List<NodeType>>;
    struct SubArea2D
    {
        public int wi; // Номер подобласти
        // Индексы в массивах Xw и Yw
        public int nx1, nx2;
        public int ny1, ny2;

        public SubArea2D(int wi, int nx1, int nx2, int ny1, int ny2)
        {
            this.wi = wi; 
            this.nx1 = nx1;
            this.nx2 = nx2; 
            this.ny1 = ny1;
            this.ny2 = ny2;
        }
    }

    // Прямоугольный конечный элемент
    struct Elem2D
    {
        public int wi;
        // Граничные узлы
        public int n1; public int n2; public int n3; public int n4;
        // Терминальный узел
        public int n5 = -1;

        public Elem2D(int wi, int n1, int n2, int n3, int n4)
        {
            this.wi = wi;
            this.n1 = n1;
            this.n2 = n2;
            this.n3 = n3;
            this.n4 = n4;
        }

        public Elem2D(int wi, int n1, int n2, int n3, int n4, int n5) : this(wi, n1, n2, n3, n4)
        {
            this.n5 = n5;
        }
    }

    // Расчётная область
    class Area2D
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
        public List<SubArea2D> Mw { get; }

        public Area2D(string path)
        {
            try
            {
                using (TextReader reader = File.OpenText(path))
                {
                    string line = "*";
                    while (line != "")
                        line = reader.ReadLine();
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
                    Mw = new List<SubArea2D>(Nareas);

                    for (int i = 0; i < Nareas; i++)
                    {
                        string Mwi_txt = reader.ReadLine();
                        string[] Mwi = Mwi_txt.Split(' ');
                        int wi = int.Parse(Mwi[0]) - 1;
                        int nx1 = int.Parse(Mwi[1]) - 1;
                        int nx2 = int.Parse(Mwi[2]) - 1;
                        int ny1 = int.Parse(Mwi[3]) - 1;
                        int ny2 = int.Parse(Mwi[4]) - 1;
                        Mw.Add(new SubArea2D(wi, nx1, nx2, ny1, ny2));
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

        // Сечение
        public Area2D(Area3D area3D, Plane plane, float value)
        {
            Mw = new List<SubArea2D>();
            List<int> Wi = new List<int>();
            Nareas = 0;
            Nmats = 0;
            switch (plane)
            {
                case Plane.XY:
                    X0 = area3D.X0;
                    Y0 = area3D.Y0;
                    Xn = area3D.Xn;
                    Yn = area3D.Yn;
                    NXw = area3D.NXw;
                    NYw = area3D.NYw;
                    Xw = area3D.Xw;
                    Yw = area3D.Yw;
                    foreach (SubArea3D subArea3D in area3D.Mw)
                    {
                        float zAreaMin = area3D.Zw[subArea3D.nz1];
                        float zAreaMax = area3D.Zw[subArea3D.nz2];
                        if (value >= zAreaMin && value <= zAreaMax)
                        {
                            if (!Wi.Contains(subArea3D.wi))
                            {
                                Wi.Add(subArea3D.wi);
                                Nmats++;
                            }
                            Mw.Add(new SubArea2D(subArea3D.wi, subArea3D.nx1, subArea3D.nx2, subArea3D.ny1, subArea3D.ny2));
                            Nareas++;
                        }
                    }
                    break;
                case Plane.XZ:
                    X0 = area3D.X0;
                    Y0 = area3D.Z0;
                    Xn = area3D.Xn;
                    Yn = area3D.Zn;
                    NXw = area3D.NXw;
                    NYw = area3D.NZw;
                    Xw = area3D.Xw;
                    Yw = area3D.Zw;
                    foreach (SubArea3D subArea3D in area3D.Mw)
                    {
                        float yAreaMin = area3D.Yw[subArea3D.ny1];
                        float yAreaMax = area3D.Yw[subArea3D.ny2];
                        if (value >= yAreaMin && value <= yAreaMax)
                        {
                            if (!Wi.Contains(subArea3D.wi))
                            {
                                Wi.Add(subArea3D.wi);
                                Nmats++;
                            }
                            Mw.Add(new SubArea2D(subArea3D.wi, subArea3D.nx1, subArea3D.nx2, subArea3D.nz1, subArea3D.nz2));
                            Nareas++;
                        }
                    }
                    break;
                case Plane.YZ:
                    X0 = area3D.Y0;
                    Y0 = area3D.Z0;
                    Xn = area3D.Yn;
                    Yn = area3D.Zn;
                    NXw = area3D.NYw;
                    NYw = area3D.NZw;
                    Xw = area3D.Yw;
                    Yw = area3D.Zw;
                    foreach (SubArea3D subArea3D in area3D.Mw)
                    {
                        float xAreaMin = area3D.Xw[subArea3D.nx1];
                        float xAreaMax = area3D.Xw[subArea3D.nx2];
                        if (value >= xAreaMin && value <= xAreaMax)
                        {
                            if (!Wi.Contains(subArea3D.wi))
                            {
                                Wi.Add(subArea3D.wi);
                                Nmats++;
                            }
                            Mw.Add(new SubArea2D(subArea3D.wi, subArea3D.ny1, subArea3D.ny2, subArea3D.nz1, subArea3D.nz2));
                            Nareas++;
                        }
                    }
                    break;
            }
        }

        public int FindSubArea(float xElemMin, float xElemMax, float yElemMin, float yElemMax)
        {
            foreach (SubArea2D subArea in Mw)
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

    class Grid2D : IGrid
    {
        public Area2D Area { get; }
        public int Nnodes { get; }
        public int Nelems { get; private set; }
        public int Nmats { get; }
        public float MeanAR { get; set; } = 0;
        public float WorstAR { get; set; } = 0;

        public int Nx { get; private set; }
        public int Ny { get; private set; }

        public List<Elem2D> Elems { get; }
        public List<Vector2> XY { get; }
        public ByteMat2D IJ { get; }
        public List<int> removedNodes;

        public Grid2D(Area2D area, List<Vector2> XY, List<Elem2D> elems, ByteMat2D IJ)
        {
            Area = area;
            Nmats = area.Nmats;
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
            CalcAR();
        }

        // Создание регулярной сетки
        public Grid2D(string path, Area2D area) 
        {
            Area = area;
            Nmats = area.Nmats;
            List<float> X = new List<float>();
            List<float> Y = new List<float>();
            ReadGrid2D(path, X, Y);

            Nnodes = Nx * Ny;
            Nelems = (Nx - 1) * (Ny - 1);

            Elems = new List<Elem2D>(Nelems);
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
                    Elem2D elem = new Elem2D(wi, n1, n2, n3, n4);
                    Elems.Add(elem);
                }

            IJ = new ByteMat2D(Nx);
            for (int i = 0; i < Nx; i++)
            {
                IJ.Add(new List<NodeType>(Ny));
                for (int j = 0; j < Ny; j++)
                    IJ[i].Add(NodeType.Regular);
            }
            CalcAR();
        }

        // Сечение
        public Grid2D(Grid3D grid3D, Plane plane, int index, float value)
        {
            Area = new Area2D(grid3D.Area, plane, value);
            Nmats = Area.Nmats;
            switch (plane) 
            {
                case Plane.XY:
                    Nx = grid3D.Nx; Ny = grid3D.Ny; break;
                case Plane.XZ:
                    Nx = grid3D.Nx; Ny = grid3D.Nz; break;
                case Plane.YZ: 
                    Nx = grid3D.Ny; Ny = grid3D.Nz; break;
            }
            // СБОРКА IJ
            IJ = new ByteMat2D(Nx);
            for (int i = 0; i < Nx; i++)
            {
                IJ.Add(new List<NodeType>(Ny));
                for (int j = 0; j < Ny; j++)
                    switch (plane)
                    {
                        case Plane.XY:
                            IJ[i].Add(grid3D.IJK[i][j][index]); break;
                        case Plane.XZ:
                            IJ[i].Add(grid3D.IJK[i][index][j]); break;
                        case Plane.YZ:
                            IJ[i].Add(grid3D.IJK[index][i][j]); break; ;
                    }
            }
            // СБОРКА removedNodes
            removedNodes = new List<int>();
            for (int j = 0; j < Ny; j++)
                for (int i = 0; i < Nx; i++)
                {
                    if (IJ[i][j] == NodeType.Removed)
                        removedNodes.Add(j * Nx + i);
                }
            // СБОРКА МАССИВА УЗЛОВ
            Nnodes = 0;
            XY = new List<Vector2>();
            for (int j = 0; j < Ny; j++)
                for (int i = 0; i < Nx; i++)
                {
                    if (IJ[i][j] != NodeType.Removed)
                    {
                        switch (plane)
                        {
                            case Plane.XY:
                                int nxy = grid3D.global_num(i, j, index);
                                XY.Add(new Vector2(grid3D.XYZ[nxy].X, grid3D.XYZ[nxy].Y)); break;
                            case Plane.XZ:
                                int nxz = grid3D.global_num(i, index, j);
                                XY.Add(new Vector2(grid3D.XYZ[nxz].X, grid3D.XYZ[nxz].Z)); break;
                            case Plane.YZ:
                                int nyz = grid3D.global_num(index, i, j);
                                XY.Add(new Vector2(grid3D.XYZ[nyz].Y, grid3D.XYZ[nyz].Z)); break;
                        }
                        Nnodes++;
                    }
                }
            Elems = new List<Elem2D>();
            BuildElemsFromByteMat();
            CalcAR();
        }

        private void BuildElemsFromByteMat()
        {
            Nelems = 0;
            int n1, n2, n3, n4, n5;
            for (int j = 0; j < Ny - 1; j++)
                for (int i = 0; i < Nx - 1; i++)
                {
                    if (IJ[i][j] == NodeType.Removed || IJ[i][j] == NodeType.Left || IJ[i][j] == NodeType.Bottom)
                        continue;
                    n5 = -1;
                    n1 = global_num(i, j);

                    int ik = i + 1;
                    // Bottom line
                    while (IJ[ik][j] == NodeType.Removed || IJ[ik][j] == NodeType.Bottom)
                    {
                        if (IJ[ik][j] == NodeType.Bottom)
                            n5 = global_num(ik, j);
                        ik++;
                    }
                    n2 = global_num(ik, j);

                    int jk = j + 1;
                    // Left line
                    while (IJ[i][jk] == NodeType.Removed || IJ[i][jk] == NodeType.Left)
                    {
                        if (IJ[i][jk] == NodeType.Left)
                            n5 = global_num(i, jk);
                        jk++;
                    }
                    n3 = global_num(i, jk);
                    n4 = global_num(ik, jk);

                    // Top line
                    for (int ikk = i; ikk < ik; ikk++)
                    {
                        if (IJ[ikk][jk] == NodeType.Top)
                            n5 = global_num(ikk, jk);
                    }

                    // Right line
                    for (int jkk = j; jkk < jk; jkk++)
                    {
                        if (IJ[ik][jkk] == NodeType.Right)
                            n5 = global_num(ik, jkk);
                    }
                    float xmin = XY[n1].X;
                    float xmax = XY[n4].X;
                    float ymin = XY[n1].Y;
                    float ymax = XY[n4].Y;
                    int wi = Area.FindSubArea(xmin, xmax, ymin, ymax);
                    Elems.Add(new Elem2D(wi, n1, n2, n3, n4, n5));
                    Nelems++;
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
                    line = "*";
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
            if (MathF.Abs(qxy - 1) < 1E-16)
                h0 = (right - left) / n;
            else if (qxy > 0)
                h0 = (right - left) * (1 - qxy) / (1 - MathF.Pow(qxy, n));
            else
            {
                qxy *= -1;
                h0 = (right - left) * (1 - qxy) / (1 - MathF.Pow(qxy, n)) * MathF.Pow(qxy, n - 1);
                qxy *= -1;
            }

            X_Y[i0] = left;
            j++;
            for (int i = i0 + 1; i < n + i0; i++)
            {
                X_Y[i] = X_Y[i - 1] + h0;
                if (qxy > 0)
                    h0 *= qxy;
                else
                    h0 /= MathF.Abs(qxy);
            }
            i0 = n + i0;
        }

        public int global_num(int i, int j, bool removedNode = false)
        {
            int l = j * Nx + i;
            if (removedNodes.Count == 0)
                return l;

            for (int k = 0; k < removedNodes.Count; k++)
                if (!removedNode && l == removedNodes[k]) return -1;
                else if (l < removedNodes[k]) return l - k;
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
                if (node_num < global_num(reg_i, reg_j, true) + 1)
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

        public bool FindElem(float x, float y, ref Elem2D foundElem)
        {
            foreach (Elem2D elem in Elems)
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

        public void CalcAR()
        {
            foreach (Elem2D elem in Elems)
            {
                float xmin = XY[elem.n1].X;
                float ymin = XY[elem.n1].Y;
                float xmax = XY[elem.n4].X;
                float ymax = XY[elem.n4].Y;
                float ar = (xmax - xmin) / (ymax - ymin);
                if (ar < 1f) ar = 1f / ar;
                if (ar > WorstAR) WorstAR = ar;
                MeanAR += ar;
            }
            MeanAR /= Nelems;
        }
    }
}
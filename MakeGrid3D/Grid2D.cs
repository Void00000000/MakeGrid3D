using OpenTK.Mathematics;
using MakeGrid3D;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace MakeGrid3D
{
    using ByteMat2D = List<List<NodeType>>;
    public struct SubArea2D
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
    public struct Elem2D
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

        public Area2D(List<float> xw, List<float> yw, List<SubArea2D> mw, int nmats)
        {
            Xw = xw; Yw = yw;
            NXw = xw.Count; NYw = yw.Count; Nareas = mw.Count;
            X0 = xw[0]; Xn = xw[NXw - 1]; Y0 = yw[0]; Yn = yw[NYw - 1];
            Nmats = nmats;
            Mw = mw;
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
                    X0 = area3D.Z0;
                    Y0 = area3D.Y0;
                    Xn = area3D.Zn;
                    Yn = area3D.Yn;
                    NXw = area3D.NZw;
                    NYw = area3D.NYw;
                    Xw = area3D.Zw;
                    Yw = area3D.Yw;
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
                            Mw.Add(new SubArea2D(subArea3D.wi, subArea3D.nz1, subArea3D.nz2, subArea3D.ny1, subArea3D.ny2));
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

        // Создание регулярной сетки не через файл
        public Grid2D(GridParams gridParams) 
        {
            List<SubArea2D> Mw2D = new List<SubArea2D>(gridParams.Mw.Count);
            foreach (SubArea3D subArea3D in gridParams.Mw)
            {
                SubArea2D subArea2D = new SubArea2D(subArea3D.wi, subArea3D.nx1, subArea3D.nx2, subArea3D.ny1, subArea3D.ny2);
                Mw2D.Add(subArea2D);
            }
            Area = new Area2D(gridParams.Xw, gridParams.Yw, Mw2D, gridParams.Mats.Count);
            Nmats = Area.Nmats;
            Default.areaColors = gridParams.Mats;
            List<int> nx = gridParams.NX;
            List<int> ny = gridParams.NY;
            List<float> qx = gridParams.QX;
            List<float> qy = gridParams.QY;
            List<float> X = new List<float>();
            List<float> Y = new List<float>();
            Nx = 0; Ny = 0;
            foreach (int nxi in nx)
                Nx += nxi;
            Nx++;
            foreach (int nyi in ny)
                Ny += nyi;
            Ny++;

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
                    int wi = Area.FindSubArea(xmin, xmax, ymin, ymax);
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

        // Создание сетки через файл
        public Grid2D(string fName)
        {
            try
            {
                using (TextReader reader = File.OpenText(fName))
                {
                    reader.ReadLine(); reader.ReadLine();

                    string NXw_txt = reader.ReadLine();
                    int NXw = int.Parse(NXw_txt);
                    List<float> Xw = new List<float>(NXw);
                    string[] Xw_txt = reader.ReadLine().Split('|');
                    for (int i = 0; i < NXw; i++)
                    {
                        Xw.Add(float.Parse(Xw_txt[i]));
                    }

                    string NYw_txt = reader.ReadLine();
                    int NYw = int.Parse(NYw_txt);
                    List<float> Yw = new List<float>(NYw);
                    string[] Yw_txt = reader.ReadLine().Split('|');
                    for (int i = 0; i < NYw; i++)
                    {
                        Yw.Add(float.Parse(Yw_txt[i]));
                    }

                    string Nmats_txt = reader.ReadLine();
                    Nmats = int.Parse(Nmats_txt);
                    Default.areaColors = new List<Color4>(Nmats);
                    for (int i = 0; i < Nmats; i++)
                    {
                        string[] areaColor_txt = reader.ReadLine().Split('|');
                        Color4 color4 = new Color4(float.Parse(areaColor_txt[0]), float.Parse(areaColor_txt[1]),
                                                   float.Parse(areaColor_txt[2]), float.Parse(areaColor_txt[3]));
                        Default.areaColors.Add(color4);
                    }

                    string Nareas_txt = reader.ReadLine();
                    int Nareas = int.Parse(Nareas_txt);
                    List<SubArea2D> Mw = new List<SubArea2D>(Nareas);
                    for (int i = 0; i < Nareas; i++)
                    {
                        string[] subArea_txt = reader.ReadLine().Split('|');
                        SubArea2D subArea2D = new SubArea2D(int.Parse(subArea_txt[0]), int.Parse(subArea_txt[1]),
                                                   int.Parse(subArea_txt[2]), int.Parse(subArea_txt[3]), int.Parse(subArea_txt[4]));
                        Mw.Add(subArea2D);
                    }

                    Area = new Area2D(Xw, Yw, Mw, Nmats);
                    reader.ReadLine();
                    string Nnodes_txt = reader.ReadLine();
                    Nnodes = int.Parse(Nnodes_txt);
                    XY = new List<Vector2>(Nnodes);
                    for (int i = 0; i < Nnodes; i++)
                    {
                        string[] node_txt = reader.ReadLine().Split('|');
                        Vector2 node = new Vector2(float.Parse(node_txt[0]), float.Parse(node_txt[1]));
                        XY.Add(node);
                    }
                    reader.ReadLine();
                    string Nelems_txt = reader.ReadLine();
                    Nelems = int.Parse(Nelems_txt);
                    Elems = new List<Elem2D>(Nelems);
                    for (int i = 0; i < Nelems; i++)
                    {
                        string[] elem_txt = reader.ReadLine().Split('|');
                        Elem2D elem = new Elem2D(int.Parse(elem_txt[0]), int.Parse(elem_txt[1]),
                                                   int.Parse(elem_txt[2]), int.Parse(elem_txt[3]), int.Parse(elem_txt[4]), int.Parse(elem_txt[5]));
                        Elems.Add(elem);
                    }
                    reader.ReadLine();
                    string[] NxNy_txt = reader.ReadLine().Split('|');
                    Nx = int.Parse(NxNy_txt[0]); Ny = int.Parse(NxNy_txt[1]);
                    IJ = new ByteMat2D(Nx);
                    for (int i = 0; i < Nx; i++)
                    {
                        string[] row = reader.ReadLine().Split('|');
                        IJ.Add(new List<NodeType>(Ny));
                        for (int j = 0; j < Ny; j++)
                        {
                            Enum.TryParse(row[j], out NodeType nodeType);
                            IJ[i].Add(nodeType);
                        }
                    }
                    reader.ReadLine();
                    int removedNodesNum = int.Parse(reader.ReadLine());
                    removedNodes = new List<int>(removedNodesNum);
                    if (removedNodesNum > 0)
                    {
                        string[] removedNodes_txt = reader.ReadLine().Split('|');
                        for (int i = 0; i < removedNodesNum; i++)
                        {
                            removedNodes.Add(int.Parse(removedNodes_txt[i]));
                        }
                    }
                    CalcAR();
                }
            }
            catch (Exception e)
            {
                if (e is DirectoryNotFoundException || e is FileNotFoundException)
                    ErrorHandler.FileReadingErrorMessage("Не удалось найти файл с сеткой");
                else if (e is FormatException)
                    ErrorHandler.FileReadingErrorMessage("Некорректный формат файла");
                else
                    ErrorHandler.FileReadingErrorMessage("Не удалось прочитать файл");
            }
        }

        // Сечение
        public Grid2D(Grid3D grid3D, Plane plane, int index, float value, bool isQ, List<float> q, out List<float> q_new)
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
                    Nx = grid3D.Nz; Ny = grid3D.Ny; break;
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
                            IJ[i].Add(grid3D.IJK[index][j][i]); break; ;
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
            q_new = new List<float>();
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
                                XY.Add(new Vector2(grid3D.XYZ[nxy].X, grid3D.XYZ[nxy].Y));
                                if (isQ) q_new.Add(q[nxy]); break;
                            case Plane.XZ:
                                int nxz = grid3D.global_num(i, index, j);
                                XY.Add(new Vector2(grid3D.XYZ[nxz].X, grid3D.XYZ[nxz].Z));
                                if (isQ) q_new.Add(q[nxz]); break;
                            case Plane.YZ:
                                int nyz = grid3D.global_num(index, j, i);
                                XY.Add(new Vector2(grid3D.XYZ[nyz].Z, grid3D.XYZ[nyz].Y));
                                if (isQ) q_new.Add(q[nyz]); break;
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
                    try
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
                    } catch (ArgumentOutOfRangeException) { continue; }
                }
        }

        public static void MakeGrid1D(List<float> X_Y, float left, float right, int n, float qxy, ref int i0, ref int j)
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

        public bool FindElem(float x, float y, ref int num)
        {
            for (int i = 0; i < Nelems; i++)
            {
                Elem2D elem = Elems[i];
                float xElemMin = XY[elem.n1].X;
                float yElemMin = XY[elem.n1].Y;
                float xElemMax = XY[elem.n4].X;
                float yElemMax = XY[elem.n4].Y;
                if (x >= xElemMin && x <= xElemMax && y >= yElemMin && y <= yElemMax)
                {
                    num = i;
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

        public string PrintInfo()
        {
            string infoText = "2D\n";
            infoText += "*** AREA ***\n";
            infoText += $"{Area.NXw}\n";
            for (int i = 0; i < Area.NXw; i++)
            {
                infoText += $"{Area.Xw[i]}|";
            }
            infoText += $"\n{Area.NYw}\n";
            for (int i = 0; i < Area.NYw; i++)
            {
                infoText += $"{Area.Yw[i]}|";
            }
            infoText += $"\n{Nmats}\n";
            for (int i = 0; i < Nmats; i++)
            {
                infoText += $"{Default.areaColors[i].R}|{Default.areaColors[i].G}|{Default.areaColors[i].B}|{Default.areaColors[i].A}\n";
            }
            infoText += $"{Area.Nareas}\n";
            for (int i = 0; i < Area.Nareas; i++)
            {
                infoText += $"{Area.Mw[i].wi}|{Area.Mw[i].nx1}|{Area.Mw[i].nx2}|{Area.Mw[i].ny1}|{Area.Mw[i].ny2}\n";
            }

            infoText += "*** NODES ***\n";
            infoText += $"{Nnodes}\n";
            for (int i = 0; i < Nnodes; i++)
            {
                infoText += $"{XY[i].X}|{XY[i].Y}\n";
            }
            infoText += "*** ELEMS ***\n";
            infoText += $"{Nelems}\n";
            for (int i = 0; i < Nelems; i++)
            {
                infoText += $"{Elems[i].wi}|{Elems[i].n1}|{Elems[i].n2}|{Elems[i].n3}|{Elems[i].n4}|{Elems[i].n5}\n";
            }
            infoText += "*** NODE TYPES MATRIX ***\n";
            infoText += $"{Nx}|{Ny}\n";
            for (int i = 0; i < Nx; i++)
            {
                for (int j = 0; j < Ny; j++)
                    infoText += $"{IJ[i][j]}|";
                infoText += "\n";
            }
            infoText += "*** REMOVED NODES ***\n";
            infoText += $"{removedNodes.Count}\n";
            for (int i = 0; i < removedNodes.Count; i++)
            {
                infoText += $"{removedNodes[i]}|";
            }

            // КРАЕВЫЕ ДЛЯ МКЭ --------------------------
            infoText += "\n";
            infoText += "*** BOTTOM LINE ***\n";
            for (int i = 0; i < Nx; i++)
            {
                if (global_num(i, 0) < 0) continue;
                infoText += $"{global_num(i, 0)}|";
            }
            infoText += "\n";
            infoText += "*** RIGHT LINE ***\n";
            for (int j = 1; j < Ny; j++)
            {
                if (global_num(Nx - 1, j) < 0) continue;
                infoText += $"{global_num(Nx - 1, j)}|";
            }
            infoText += "\n";
            infoText += "*** TOP LINE ***\n";
            for (int i = 0; i < Nx - 1; i++)
            {
                if (global_num(i, Ny - 1) < 0) continue;
                infoText += $"{global_num(i, Ny - 1)}|";
            }
            infoText += "\n";
            infoText += "*** LEFT LINE ***\n";
            for (int j = 1; j < Ny - 1; j++)
            {
                if (global_num(0, j) < 0) continue;
                infoText += $"{global_num(0, j)}|";
            }
            // --------------------------------------------------
            return infoText;
        }
    }
}
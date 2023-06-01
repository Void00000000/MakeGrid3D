using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MakeGrid3D
{
    using ByteMat3D = List<List<List<NodeType>>>;
    using ByteMat2D = List<List<NodeType>>;
    struct SubArea3D
    {
        public int wi; // Номер подобласти
        // Индексы в массивах Xw и Yw
        public int nx1, nx2;
        public int ny1, ny2;
        public int nz1, nz2;

        public SubArea3D(int wi, int nx1, int nx2, int ny1, int ny2, int nz1, int nz2)
        {
            this.wi = wi;
            this.nx1 = nx1;
            this.nx2 = nx2;
            this.ny1 = ny1;
            this.ny2 = ny2;
            this.nz1 = nz1;
            this.nz2 = nz2;
        }
    }

    // Прямоугольный конечный элемент
    struct Elem3D
    {
        public int wi;
        // Граничные узлы
        public int n1; public int n2; public int n3; public int n4;
        public int n5; public int n6; public int n7; public int n8;
        // Терминальные узлы
        public int n9 = -1; public int n10 = -1;

        public Elem3D(int wi, int n1, int n2, int n3, int n4, int n5, int n6, int n7, int n8)
        {
            this.wi = wi;
            this.n1 = n1;
            this.n2 = n2;
            this.n3 = n3;
            this.n4 = n4;
            this.n5 = n5;
            this.n6 = n6;
            this.n7 = n7;
            this.n8 = n8;
        }

        public Elem3D(int wi, int n1, int n2, int n3, int n4, int n5, int n6, int n7, int n8, int n9, int n10)
            : this(wi, n1, n2, n3, n4, n5, n6, n7, n8)
        {
            this.n9 = n9;
            this.n10 = n10;
        }
    }

    // Расчётная область
    class Area3D
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
        public float Z0 { get; }
        public float Zn { get; }
        public int NXw { get; }
        public int NYw { get; }
        public int NZw { get; }
        // Массивы содержащие координаты подобластей
        public List<float> Xw { get; }
        public List<float> Yw { get; }
        public List<float> Zw { get; }
        // Массив, содержащий подобласти
        public List<SubArea3D> Mw { get; }

        public Area3D(string path)
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

                    string nZw_txt = reader.ReadLine();
                    NZw = int.Parse(nZw_txt);
                    Zw = new List<float>(NZw);
                    string Zwi_txt = reader.ReadLine();
                    string[] Zwi = Zwi_txt.Split(' ');
                    for (int i = 0; i < NZw; i++)
                        Zw.Add(float.Parse(Zwi[i], CultureInfo.InvariantCulture));

                    X0 = Xw[0];
                    Xn = Xw[NXw - 1];
                    Y0 = Yw[0];
                    Yn = Yw[NYw - 1];
                    Z0 = Zw[0];
                    Zn = Zw[NZw - 1];

                    string nmat_nw_txt = reader.ReadLine();
                    string[] nmat_nw = nmat_nw_txt.Split(' ');
                    Nmats = int.Parse(nmat_nw[0]);
                    Nareas = int.Parse(nmat_nw[1]);
                    Mw = new List<SubArea3D>(Nareas);

                    for (int i = 0; i < Nareas; i++)
                    {
                        string Mwi_txt = reader.ReadLine();
                        string[] Mwi = Mwi_txt.Split(' ');
                        int wi = int.Parse(Mwi[0]) - 1;
                        int nx1 = int.Parse(Mwi[1]) - 1;
                        int nx2 = int.Parse(Mwi[2]) - 1;
                        int ny1 = int.Parse(Mwi[3]) - 1;
                        int ny2 = int.Parse(Mwi[4]) - 1;
                        int nz1 = int.Parse(Mwi[5]) - 1;
                        int nz2 = int.Parse(Mwi[6]) - 1;
                        Mw.Add(new SubArea3D(wi, nx1, nx2, ny1, ny2, nz1, nz2));
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

        // Тиражирование сечений
        public Area3D(Area2D area2D, List<float> z)
        {
            Nareas = area2D.Nareas;
            Nmats = area2D.Nmats;
            X0 = area2D.X0;
            Xn = area2D.Xn;
            Y0 = area2D.Y0;
            Yn = area2D.Yn;
            Z0 = z[0];
            Zn = z[z.Count - 1];
            NXw = area2D.NXw;
            NYw = area2D.NYw;
            NZw = 2;
            Xw = area2D.Xw;
            Yw = area2D.Yw;
            Zw = new List<float>{ Z0, Zn };
            Mw = new List<SubArea3D>(Nareas);
            foreach (SubArea2D subArea2D in area2D.Mw)
            {
                int wi = subArea2D.wi;
                int nx1 = subArea2D.nx1;
                int nx2 = subArea2D.nx2;
                int ny1 = subArea2D.ny1;
                int ny2 = subArea2D.ny2;
                SubArea3D subArea3D = new SubArea3D(wi, nx1, nx2, ny1, ny2, 0, 1);
                Mw.Add(subArea3D);
            }
        }

        public int FindSubArea(float xElemMin, float xElemMax, float yElemMin, float yElemMax,
            float zElemMin, float zElemMax)
        {
            foreach (SubArea3D subArea in Mw)
            {
                float xAreaMin = Xw[subArea.nx1];
                float yAreaMin = Yw[subArea.ny1];
                float zAreaMin = Zw[subArea.nz1];
                float xAreaMax = Xw[subArea.nx2];
                float yAreaMax = Yw[subArea.ny2];
                float zAreaMax = Zw[subArea.nz2];

                if (xAreaMin <= xElemMin && xElemMin <= xAreaMax &&
                    yAreaMin <= yElemMin && yElemMin <= yAreaMax &&
                    zAreaMin <= zElemMin && zElemMin <= zAreaMax &&
                    xAreaMin <= xElemMax && xElemMax <= xAreaMax &&
                    yAreaMin <= yElemMax && yElemMax <= yAreaMax &&
                    zAreaMin <= zElemMax && zElemMax <= zAreaMax)
                {
                    return subArea.wi;
                }
            }
            return -1;
        }
    }

    class Grid3D : IGrid
    {
        public Area3D Area { get; }
        public int Nnodes { get; }
        public int Nelems { get; }
        public int Nmats { get; }
        public float MeanAR { get; set; } = 0;
        public float WorstAR { get; set; } = 0;
        public int Nx { get; private set; }
        public int Ny { get; private set; }
        public int Nz { get; private set; }

        public List<Elem3D> Elems { get; }
        public List<Vector3> XYZ { get; }
        public ByteMat3D IJK { get; }
        public List<int> removedNodes;

        public Grid3D(Area3D area, List<Vector3> XYZ, List<Elem3D> elems, ByteMat3D IJK)
        {
            Area = area;
            Nmats = area.Nmats;
            this.XYZ = XYZ;
            Nnodes = XYZ.Count;
            Nelems = elems.Count;
            Elems = elems;
            this.IJK = IJK;
            Nx = IJK.Count;
            Ny = IJK[0].Count;
            Nz = IJK[0][0].Count;
            removedNodes = new List<int>();
            for (int k = 0; k < Nz; k++)
                for (int j = 0; j < Ny; j++)
                    for (int i = 0; i < Nx; i++)
                    {
                        if (IJK[i][j][k] == NodeType.Removed)
                            removedNodes.Add(j * Nx + i + k * Nx * Ny);
                    }
        }

        // Тиражирование сечений
        public Grid3D(Grid2D grid2D, List<float> z)
        {
            Area = new Area3D(grid2D.Area, z);
            Nnodes = grid2D.Nnodes * z.Count;
            Nelems = grid2D.Nelems * (z.Count - 1);
            Nmats = Area.Nmats;
            Nx = grid2D.Nx;
            Ny = grid2D.Ny;
            Nz = z.Count;
            // Заполнение массива узлов
            XYZ = new List<Vector3>(Nnodes);
            foreach (float zi in z)
            {
                foreach (Vector2 xy in grid2D.XY)
                {
                    XYZ.Add(new Vector3(xy.X, xy.Y, zi));
                }
            }
            // Заполнение байтовой матрицы
            IJK = new ByteMat3D(Nx);
            for (int i = 0; i < Nx; i++)
            {
                IJK.Add(new ByteMat2D(Ny));
                for (int j = 0; j < Ny; j++)
                {
                    IJK[i].Add(new List<NodeType>(Nz));
                    for (int k = 0; k < Nz; k++)
                        IJK[i][j].Add(NodeType.Regular);
                }
            }
            removedNodes = new List<int>();
            for (int k = 0; k < Nz; k++)
                for (int j = 0; j < Ny; j++)
                    for (int i = 0; i < Nx; i++)
                    {
                        IJK[i][j][k] = grid2D.IJ[i][j];
                        if (IJK[i][j][k] == NodeType.Removed)
                            removedNodes.Add(j * Nx + i + k * Nx * Ny);
                    }
            // Заполнения массива элементов
            Elems = new List<Elem3D>(Nelems);
            for (int i = 0; i < z.Count - 1; i++)
            {
                foreach (Elem2D elem2D in grid2D.Elems)
                {
                    int n1 = elem2D.n1 + i * grid2D.Nnodes;
                    int n2 = elem2D.n2 + i * grid2D.Nnodes;
                    int n3 = elem2D.n3 + i * grid2D.Nnodes;
                    int n4 = elem2D.n4 + i * grid2D.Nnodes;
                    int n5 = elem2D.n1 + (i + 1) * grid2D.Nnodes;
                    int n6 = elem2D.n2 + (i + 1) * grid2D.Nnodes;
                    int n7 = elem2D.n3 + (i + 1) * grid2D.Nnodes;
                    int n8 = elem2D.n4 + (i + 1) * grid2D.Nnodes;
                    if (elem2D.n5 < 0)
                        Elems.Add(new Elem3D(elem2D.wi, n1, n2, n3, n4, n5, n6, n7, n8));
                    else
                    {
                        int n9 = elem2D.n5 + i * grid2D.Nnodes;
                        int n10 = elem2D.n5 + (i + 1) * grid2D.Nnodes;
                        Elems.Add(new Elem3D(elem2D.wi, n1, n2, n3, n4, n5, n6, n7, n8, n9, n10));
                    }
                }
            }
        }

        // Создание регулярной сетки
        public Grid3D(string path, Area3D area)
        {
            Area = area;
            Nmats = area.Nmats;
            List<float> X = new List<float>();
            List<float> Y = new List<float>();
            List<float> Z = new List<float>();
            ReadGrid3D(path, X, Y, Z);

            Nnodes = Nx * Ny * Nz;
            Nelems = (Nx - 1) * (Ny - 1) * (Nz - 1);

            Elems = new List<Elem3D>(Nelems);
            XYZ = new List<Vector3>(Nnodes);
            removedNodes = new List<int>();

            for (int k = 0; k < Nz; k++)
                for (int j = 0; j < Ny; j++)
                    for (int i = 0; i < Nx; i++)
                    {
                        Vector3 Vector3 = new Vector3(X[i], Y[j], Z[k]);
                        XYZ.Add(Vector3);
                    };

            for (int k = 0; k < Nz - 1; k++)
                for (int j = 0; j < Ny - 1; j++)
                    for (int i = 0; i < Nx - 1; i++)
                    {
                        int n1 = global_num(i, j, k);
                        int n2 = global_num(i + 1, j, k);
                        int n3 = global_num(i, j + 1, k);
                        int n4 = global_num(i + 1, j + 1, k);
                        int n5 = global_num(i, j, k + 1);
                        int n6 = global_num(i + 1, j, k + 1);
                        int n7 = global_num(i, j + 1, k + 1);
                        int n8 = global_num(i + 1, j + 1, k + 1);
                        float xmin = XYZ[n1].X; float xmax = XYZ[n4].X;
                        float ymin = XYZ[n1].Y; float ymax = XYZ[n4].Y;
                        float zmin = XYZ[n1].Z; float zmax = XYZ[n4].Z;
                        int wi = area.FindSubArea(xmin, xmax, ymin, ymax, zmin, zmax);
                        Elem3D elem = new Elem3D(wi, n1, n2, n3, n4, n5, n6, n7, n8);
                        Elems.Add(elem);
                    }

            IJK = new ByteMat3D(Nx);
            for (int i = 0; i < Nx; i++)
            {
                IJK.Add(new ByteMat2D(Ny));
                for (int j = 0; j < Ny; j++)
                {
                    IJK[i].Add(new List<NodeType>(Nz));
                    for (int k = 0; k < Nz; k++)
                        IJK[i][j].Add(NodeType.Regular);
                }
            }
        }

        private void ReadGrid3D(string path, List<float> X, List<float> Y, List<float> Z)
        {
            List<int> nx, ny, nz;
            List<float> qx, qy, qz;
            Nx = 0; Ny = 0; Nz = 0;
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

                    nz = new List<int>(Area.NZw - 1);
                    string nzi_txt = reader.ReadLine();
                    string[] nzi = nzi_txt.Split(' ');
                    for (int i = 0; i < Area.NZw - 1; i++)
                    {
                        int current_nz = int.Parse(nzi[i]);
                        Nz += current_nz;
                        nz.Add(current_nz);
                    }
                    Nz++;

                    qz = new List<float>(Area.NZw - 1);
                    string qzi_txt = reader.ReadLine();
                    string[] qzi = qzi_txt.Split(' ');
                    for (int i = 0; i < Area.NZw - 1; i++)
                        qz.Add(float.Parse(qzi[i], CultureInfo.InvariantCulture));

                    X.Capacity = Nx;
                    Y.Capacity = Ny;
                    Z.Capacity = Nz;
                    for (int i = 0; i < X.Capacity; i++)
                        X.Add(0);
                    for (int i = 0; i < Y.Capacity; i++)
                        Y.Add(0);
                    for (int i = 0; i < Z.Capacity; i++)
                        Z.Add(0);

                    int ix0 = 0, iy0 = 0, iz0 = 0;
                    int jx = 0, jy = 0, jz = 0;

                    for (int i = 0; i < Area.NXw - 1; i++)
                        MakeGrid1D(X, Area.Xw[i], Area.Xw[i + 1], nx[i], qx[i], ref ix0, ref jx);
                    for (int i = 0; i < Area.NYw - 1; i++)
                        MakeGrid1D(Y, Area.Yw[i], Area.Yw[i + 1], ny[i], qy[i], ref iy0, ref jy);
                    for (int i = 0; i < Area.NZw - 1; i++)
                        MakeGrid1D(Z, Area.Zw[i], Area.Zw[i + 1], nz[i], qz[i], ref iz0, ref jz);
                    X[Nx - 1] = Area.Xw[Area.NXw - 1];
                    Y[Ny - 1] = Area.Yw[Area.NYw - 1];
                    Z[Nz - 1] = Area.Zw[Area.NZw - 1];
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

        public int global_num(int i, int j, int k, bool removedNode = false)
        {
            int l = j * Nx + i + k * Nx * Ny;
            if (removedNodes.Count == 0)
                return l;

            for (int s = 0; s < removedNodes.Count; s++)
                if (!removedNode && l == removedNodes[s]) return -1;
                else if (l < removedNodes[s]) return l - s;
            return l - removedNodes.Count;
        }

        // TODO: Доделать
        public Vector3i global_ijk(int node_num)
        {
            int i, j, k;
            if (removedNodes.Count == 0)
            {
                i = node_num % Nx;
                j = node_num / Nx;
                k = node_num / Nx;
                return new Vector3i(i, j, k);
            }

            for (int s = 0; s < removedNodes.Count; s++)
            {
                int reg_i = (removedNodes[s]) % Nx;
                int reg_j = (removedNodes[s]) / Nx;
                int reg_k = (removedNodes[s]) / Nx;
                // TODO: Скорее всего неправильно
                if (node_num < global_num(reg_i, reg_j, reg_k) + 1)
                {
                    i = (node_num + s) % Nx;
                    j = (node_num + s) / Nx;
                    k = (node_num + s) / Nx;
                    return new Vector3i(i, j, k);
                }
            }
            i = (node_num + removedNodes.Count) % Nx;
            j = (node_num + removedNodes.Count) / Nx;
            k = (node_num + removedNodes.Count) / Nx;
            return new Vector3i(i, j, k);
        }

        public bool FindElem(float x, float y, float z, ref int num)
        {
            for (int i = 0; i < Nelems; i++)
            {
                Elem3D elem = Elems[i];
                float xElemMin = XYZ[elem.n1].X;
                float yElemMin = XYZ[elem.n1].Y;
                float zElemMin = XYZ[elem.n1].Z;
                float xElemMax = XYZ[elem.n4].X;
                float yElemMax = XYZ[elem.n4].Y;
                float zElemMax = XYZ[elem.n8].Z;
                if (x >= xElemMin && x <= xElemMax && y >= yElemMin && y <= yElemMax && z >= zElemMin && z <= zElemMax)
                {
                    num = i;
                    return true;
                }
            }
            return false;
        }
    }
}
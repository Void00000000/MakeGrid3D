using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeGrid3D
{
    using ByteMat3D = List<List<List<NodeType>>>;
    using ByteMat2D = List<List<NodeType>>;

    class IrregularGridMaker
    {
        public IGrid Grid { 
            get
            {
                if (grid2D != null && grid3D == null) { return grid2D; }
                else return grid3D;
            }
            set
            {
                if (value is Grid2D)
                {
                    grid2D = (Grid2D)value;
                    grid3D = null;
                }
                else
                {
                    grid2D = null;
                    grid3D = (Grid3D)value;
                }
            }
        }
        private Grid2D? grid2D;
        private Grid3D? grid3D;
        public int Nx { get; private set; }
        public int Ny { get; private set; }
        public int Nz { get; private set; }
        public float MaxAR { get; set; } = (float)Default.maxAR_width / Default.maxAR_height;

        public int NodeI { get; set; } = 1;
        public int NodeJ { get; set; } = 1;
        public int NodeK { get; set; } = 1;

        public int I { get; set; } = 1;
        public int J { get; set; } = 1;
        public int K { get; set; } = 1;

        // По приоритету (от высшего к низшему ^ слева направо)
        public Direction[] Dirs = { Default.dir1, Default.dir2, Default.dir3, Default.dir4 };
        public int DirIndex = 0;
        public IrregularGridMaker(IGrid grid)
        {
            if (grid is Grid2D)
            {
                grid2D = (Grid2D)grid;
                grid3D = null;
                Nx = grid2D.Nx;
                Ny = grid2D.Ny;
            }
            else
            {
                grid2D = null;
                grid3D = (Grid3D)grid;
                Nx = grid3D.Nx;
                Ny = grid3D.Ny;
                Nz = grid3D.Nz;
            }
            
        }

        // Calculate Aspect Ratio
        private float CalcAR2D(int n1, int n4)
        {
            float x1 = grid2D.XY[n1].X;
            float y1 = grid2D.XY[n1].Y;

            float x2 = grid2D.XY[n4].X;
            float y2 = grid2D.XY[n4].Y;

            float width = (x2 - x1);
            float height = (y2 - y1);
            return width / height;
        }

        // a1 > a2
        private bool CompareAR(float a1, float a2)
        {
            if (a2 < 1f) a2 = 1f / a2;
            if (a1 < 1f) a1 = 1f / a1;
            return a1 > a2;
        }

        private bool MoveRight2D(ByteMat2D IJ_new)
        {
            do
            {
                I++;
            } while (IJ_new[I][J] == NodeType.Removed && I < Nx - 1);
            if (I >= Nx - 1)
                return true;
            return false;
        }

        private bool MoveLeft2D(ByteMat2D IJ_new)
        {
            do
            {
                I--;
            } while (IJ_new[I][J] == NodeType.Removed && I > 0);
            if (I <= 0)
                return true;
            return false;
        }

        private bool MoveTop2D(ByteMat2D IJ_new)
        {
            do
            {
                J++;
            } while (IJ_new[I][J] == NodeType.Removed && J < Ny - 1);
            if (J >= Ny - 1)
                return true;
            return false;
        }

        private bool MoveBottom2D(ByteMat2D IJ_new)
        {
            do
            {
                J--;
            } while (IJ_new[I][J] == NodeType.Removed && J > 0);
            if (J <= 0)
                return true;
            return false;
        }

        private void MoveNode2D(ByteMat2D IJ_new)
        {
            do
            {
                NodeI++;
                if (NodeI >= Nx - 1)
                {
                    NodeI = 1;
                    NodeJ++;
                }
            }
            while (IJ_new[NodeI][NodeJ] == NodeType.Removed && NodeI < Nx - 1 && NodeJ < Ny - 1);
        }

        private bool MergeRight2D(ByteMat2D IJ_new, out bool merged)
        {
            merged = false;
            if (IJ_new[I][J] == NodeType.Regular || IJ_new[I][J] == NodeType.Right)
            {
                int n = grid2D.global_num(I, J);
                int nb = grid2D.global_num(I, J - 1);
                int nr = grid2D.global_num(I + 1, J);
                int nt = grid2D.global_num(I, J + 1);
                int nrt = grid2D.global_num(I + 1, J + 1);
                int nrb = grid2D.global_num(I + 1, J - 1);

                if (IJ_new[I][J - 1] != NodeType.Removed && IJ_new[I + 1][J] != NodeType.Removed &&
                    IJ_new[I][J + 1] != NodeType.Removed && IJ_new[I + 1][J + 1] != NodeType.Removed &&
                    IJ_new[I + 1][J - 1] != NodeType.Removed)
                {
                    float art = CalcAR2D(n, nrt);
                    float arb = CalcAR2D(nb, nr);
                    float ar = CalcAR2D(nb, nrt);
                    if (CompareAR(art, MaxAR) && CompareAR(arb, MaxAR) && CompareAR(art, ar) && CompareAR(arb, ar))
                    {
                        merged = true;
                        if (IJ_new[I][J] == NodeType.Right)
                        {
                            IJ_new[I][J] = NodeType.Removed;
                            if (I + 1 == Nx - 1)
                                IJ_new[I + 1][J] = NodeType.Removed;
                            else
                                IJ_new[I + 1][J] = NodeType.Right;
                        }
                        else
                        {
                            if (IJ_new[I][J] == NodeType.Regular)
                                IJ_new[I][J] = NodeType.Left;
                            else
                                IJ_new[I][J] = NodeType.Removed;
                            IJ_new[I + 1][J] = NodeType.Removed;


                            bool end = MoveRight2D(IJ_new);
                            if (!end)
                                IJ_new[I][J] = NodeType.Right;
                            else
                                IJ_new[I][J] = NodeType.Removed;
                            return end;
                        }
                    }
                }
            }
            return MoveRight2D(IJ_new);
        }

        private bool MergeLeft2D(ByteMat2D IJ_new, out bool merged)
        {
            merged = false;
            if (IJ_new[I][J] == NodeType.Regular || IJ_new[I][J] == NodeType.Left)
            {
                int n = grid2D.global_num(I, J);
                int nb = grid2D.global_num(I, J - 1);
                int nl = grid2D.global_num(I - 1, J);
                int nt = grid2D.global_num(I, J + 1);
                int nlt = grid2D.global_num(I - 1, J + 1);
                int nlb = grid2D.global_num(I - 1, J - 1);

                if (IJ_new[I][J - 1] != NodeType.Removed && IJ_new[I - 1][J] != NodeType.Removed &&
                    IJ_new[I][J + 1] != NodeType.Removed && IJ_new[I - 1][J + 1] != NodeType.Removed &&
                    IJ_new[I - 1][J - 1] != NodeType.Removed)
                {
                    float alt = CalcAR2D(nl, nt);
                    float alb = CalcAR2D(nlb, n);
                    float al = CalcAR2D(nlb, nt);
                    if (CompareAR(alt, MaxAR) && CompareAR(alb, MaxAR) && CompareAR(alt, al) && CompareAR(alb, al))
                    {
                        merged = true;
                        if (IJ_new[I][J] == NodeType.Left)
                        {
                            IJ_new[I][J] = NodeType.Removed;
                            if (I - 1 == 0)
                                IJ_new[I - 1][J] = NodeType.Removed;
                            else
                                IJ_new[I - 1][J] = NodeType.Left;
                        }
                        else
                        {
                            if (IJ_new[I][J] == NodeType.Regular)
                                IJ_new[I][J] = NodeType.Right;
                            else
                                IJ_new[I][J] = NodeType.Removed;
                            IJ_new[I - 1][J] = NodeType.Removed;


                            bool end = MoveLeft2D(IJ_new);
                            if (!end)
                                IJ_new[I][J] = NodeType.Left;
                            else
                                IJ_new[I][J] = NodeType.Removed;
                            return end;
                        }
                    }
                }
            }
            return MoveLeft2D(IJ_new);
        }

        private bool MergeTop2D(ByteMat2D IJ_new, out bool merged)
        {
            merged = false;
            if (IJ_new[I][J] == NodeType.Regular || IJ_new[I][J] == NodeType.Top)
            {
                int n = grid2D.global_num(I, J);
                int nl = grid2D.global_num(I - 1, J);
                int nr = grid2D.global_num(I + 1, J);
                int nt = grid2D.global_num(I, J + 1);
                int nlt = grid2D.global_num(I - 1, J + 1);
                int nrt = grid2D.global_num(I + 1, J + 1);

                if (IJ_new[I - 1][J] != NodeType.Removed && IJ_new[I + 1][J] != NodeType.Removed &&
                    IJ_new[I][J + 1] != NodeType.Removed && IJ_new[I - 1][J + 1] != NodeType.Removed &&
                    IJ_new[I + 1][J + 1] != NodeType.Removed)
                {
                    float alt = CalcAR2D(nl, nt);
                    float art = CalcAR2D(n, nrt);
                    float at = CalcAR2D(nl, nrt);
                    if (CompareAR(art, MaxAR) && CompareAR(alt, MaxAR) && CompareAR(alt, at) && CompareAR(art, at))
                    {
                        merged = true;
                        if (IJ_new[I][J] == NodeType.Top)
                        {
                            IJ_new[I][J] = NodeType.Removed;
                            if (J + 1 == Ny - 1)
                                IJ_new[I][J + 1] = NodeType.Removed;
                            else
                                IJ_new[I][J + 1] = NodeType.Top;
                        }
                        else
                        {
                            if (IJ_new[I][J] == NodeType.Regular)
                                IJ_new[I][J] = NodeType.Bottom;
                            else
                                IJ_new[I][J] = NodeType.Removed;
                            IJ_new[I][J + 1] = NodeType.Removed;


                            bool end = MoveTop2D(IJ_new);
                            if (!end)
                                IJ_new[I][J] = NodeType.Top;
                            else
                                IJ_new[I][J] = NodeType.Removed;
                            return end;
                        }
                    }
                }
            }
            return MoveTop2D(IJ_new);
        }

        private bool MergeBottom2D(ByteMat2D IJ_new, out bool merged)
        {
            merged = false;
            if (IJ_new[I][J] == NodeType.Regular || IJ_new[I][J] == NodeType.Bottom)
            {
                int n = grid2D.global_num(I, J);
                int nl = grid2D.global_num(I - 1, J);
                int nr = grid2D.global_num(I + 1, J);
                int nb = grid2D.global_num(I, J - 1);
                int nlb = grid2D.global_num(I - 1, J - 1);
                int nrb = grid2D.global_num(I + 1, J - 1);

                if (IJ_new[I - 1][J] != NodeType.Removed && IJ_new[I + 1][J] != NodeType.Removed &&
                    IJ_new[I][J - 1] != NodeType.Removed && IJ_new[I - 1][J - 1] != NodeType.Removed &&
                    IJ_new[I + 1][J - 1] != NodeType.Removed)
                {
                    float alb = CalcAR2D(nlb, n);
                    float arb = CalcAR2D(nb, nr);
                    float ab = CalcAR2D(nlb, nr);
                    if (CompareAR(alb, MaxAR) && CompareAR(arb, MaxAR) && CompareAR(alb, ab) && CompareAR(arb, ab))
                    {
                        merged = true;
                        if (IJ_new[I][J] == NodeType.Bottom)
                        {
                            IJ_new[I][J] = NodeType.Removed;
                            if (J - 1 == 0)
                                IJ_new[I][J - 1] = NodeType.Removed;
                            else
                                IJ_new[I][J - 1] = NodeType.Bottom;
                        }
                        else
                        {
                            if (IJ_new[I][J] == NodeType.Regular)
                                IJ_new[I][J] = NodeType.Top;
                            else
                                IJ_new[I][J] = NodeType.Removed;
                            IJ_new[I][J - 1] = NodeType.Removed;


                            bool end = MoveBottom2D(IJ_new);
                            if (!end)
                                IJ_new[I][J] = NodeType.Bottom;
                            else
                                IJ_new[I][J] = NodeType.Removed;
                            return end;
                        }
                    }
                }
            }
            return MoveBottom2D(IJ_new);
        }

        private void MakeUnStructedMatrix2D(ByteMat2D IJ_new)
        {
            bool end = true;
            bool merged = false;
            while (!merged && NodeI < Nx - 1 && NodeJ < Ny - 1)
            {
                switch (Dirs[DirIndex])
                {
                    case Direction.Left:
                        end = MergeLeft2D(IJ_new, out merged);
                        break;
                    case Direction.Right:
                        end = MergeRight2D(IJ_new, out merged);
                        break;
                    case Direction.Bottom:
                        end = MergeBottom2D(IJ_new, out merged);
                        break;
                    case Direction.Top:
                        end = MergeTop2D(IJ_new, out merged);
                        break;
                }
                if (IJ_new[NodeI][NodeJ] == NodeType.Removed)
                    MoveNode2D(IJ_new);
                if (end)
                {
                    DirIndex++;
                    I = NodeI;
                    J = NodeJ;
                }
                if (DirIndex >= Dirs.Length)
                {
                    DirIndex = 0;
                    MoveNode2D(IJ_new);
                    I = NodeI;
                    J = NodeJ;
                }
            }
        }

        public Grid2D MakeUnStructedGrid2D()
        {
            ByteMat2D IJ_new = new ByteMat2D(Nx);
            for (int i = 0; i < Nx; i++)
            {
                IJ_new.Add(new List<NodeType>(Ny));
                for (int j = 0; j < Ny; j++)
                    IJ_new[i].Add(grid2D.IJ[i][j]);
            }
            MakeUnStructedMatrix2D(IJ_new);
            List<Vector2> XY_new = new List<Vector2>();
            List<Elem2D> Elems_new = new List<Elem2D>();

            for (int j = 0; j < Ny; j++)
                for (int i = 0; i < Nx; i++)
                {
                    if (IJ_new[i][j] == NodeType.Removed)
                        continue;
                    int n = grid2D.global_num(i, j);
                    XY_new.Add(new Vector2(grid2D.XY[n].X, grid2D.XY[n].Y));
                }

            int n1, n2, n3, n4, n5;
            for (int j = 0; j < Ny - 1; j++)
                for (int i = 0; i < Nx - 1; i++)
                {
                    if (IJ_new[i][j] == NodeType.Removed || IJ_new[i][j] == NodeType.Left || IJ_new[i][j] == NodeType.Bottom)
                        continue;
                    n5 = -1;
                    n1 = grid2D.global_num(i, j);

                    int ik = i + 1;
                    // Bottom line
                    while (IJ_new[ik][j] == NodeType.Removed || IJ_new[ik][j] == NodeType.Bottom)
                    {
                        if (IJ_new[ik][j] == NodeType.Bottom)
                            n5 = grid2D.global_num(ik, j);
                        ik++;
                    }
                    n2 = grid2D.global_num(ik, j);

                    int jk = j + 1;
                    // Left line
                    while (IJ_new[i][jk] == NodeType.Removed || IJ_new[i][jk] == NodeType.Left)
                    {
                        if (IJ_new[i][jk] == NodeType.Left)
                            n5 = grid2D.global_num(i, jk);
                        jk++;
                    }
                    n3 = grid2D.global_num(i, jk);
                    n4 = grid2D.global_num(ik, jk);

                    // Top line
                    for (int ikk = i; ikk < ik; ikk++)
                    {
                        if (IJ_new[ikk][jk] == NodeType.Top)
                            n5 = grid2D.global_num(ikk, jk);
                    }

                    // Right line
                    for (int jkk = j; jkk < jk; jkk++)
                    {
                        if (IJ_new[ik][jkk] == NodeType.Right)
                            n5 = grid2D.global_num(ik, jkk);
                    }

                    // TODO: OPTIMIZE THAT
                    int n1_new, n2_new, n3_new, n4_new, n5_new;
                    n1_new = XY_new.FindIndex(v => MathF.Abs(v.X - grid2D.XY[n1].X) < 1e-14f && MathF.Abs(v.Y - grid2D.XY[n1].Y) < 1e-14f);
                    n2_new = XY_new.FindIndex(v => MathF.Abs(v.X - grid2D.XY[n2].X) < 1e-14f && MathF.Abs(v.Y - grid2D.XY[n2].Y) < 1e-14f);
                    n3_new = XY_new.FindIndex(v => MathF.Abs(v.X - grid2D.XY[n3].X) < 1e-14f && MathF.Abs(v.Y - grid2D.XY[n3].Y) < 1e-14f);
                    n4_new = XY_new.FindIndex(v => MathF.Abs(v.X - grid2D.XY[n4].X) < 1e-14f && MathF.Abs(v.Y - grid2D.XY[n4].Y) < 1e-14f);
                    if (n5 >= 0)
                        n5_new = XY_new.FindIndex(v => MathF.Abs(v.X - grid2D.XY[n5].X) < 1e-14f && MathF.Abs(v.Y - grid2D.XY[n5].Y) < 1e-14f);
                    else
                        n5_new = -1;
                    float xmin = XY_new[n1_new].X;
                    float xmax = XY_new[n4_new].X;
                    float ymin = XY_new[n1_new].Y;
                    float ymax = XY_new[n4_new].Y;
                    int wi = grid2D.Area.FindSubArea(xmin, xmax, ymin, ymax);
                    Elems_new.Add(new Elem2D(wi, n1_new, n2_new, n3_new, n4_new, n5_new));
                }
            return new Grid2D(grid2D.Area, XY_new, Elems_new, IJ_new);
        }

        public Grid3D MakeUnStructedGrid3D()
        {
            return grid3D;
        }

        public IGrid MakeUnStructedGrid()
        {
            if (grid3D == null && grid2D != null)
            {
                return MakeUnStructedGrid2D();
            }
            else
                return MakeUnStructedGrid3D();
        }
    }
}
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

        public int MidI { get; set; } = 1;
        public int MidJ { get; set; } = 1;
        public int MidK { get; set; } = 1;

        public int I { get; set; } = 1;
        public int J { get; set; } = 1;
        public int K { get; set; } = 1;

        public bool End { get; private set; } = false; 

        // По приоритету (от высшего к низшему ^ слева направо)
        public Quadrant[] Quadrants = { Quadrant.RightTop, Quadrant.LeftTop, Quadrant.LeftBottom, Quadrant.RightBottom };
        Dictionary<Quadrant, Direction[]> Directions = new Dictionary<Quadrant, Direction[]>()
        {
            { Quadrant.RightTop, new Direction[]{Direction.Right, Direction.Top} },
            { Quadrant.LeftTop, new Direction[]{Direction.Left, Direction.Top} },
            { Quadrant.LeftBottom, new Direction[]{Direction.Left, Direction.Bottom} },
            { Quadrant.RightBottom, new Direction[]{Direction.Right, Direction.Bottom} }
        };
        public int DirIndex = 0;
        public int QuadIndex = 0;
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

        private void MergeRight2D(ByteMat2D IJ_new, ref bool merged)
        
        {
            merged = true;
            int i = I; int j = J;
            MoveBottom2D(IJ_new);
            int bottom = J;
            J = j;
            MoveTop2D(IJ_new);
            int top = J;
            J = j;
            if (IJ_new[I][bottom] == NodeType.Left || IJ_new[I][J] == NodeType.Left || IJ_new[I][top] == NodeType.Left)
                return;

            MoveBottom2D(IJ_new);
            int jb = J;
            J = j;

            MoveRight2D(IJ_new);
            int ir = I;
            MoveTop2D(IJ_new);
            int jt = J;

            I = i; J = j;

            int n = grid2D.global_num(i, j);
            int nb = grid2D.global_num(i, jb);
            int nr = grid2D.global_num(ir, j);
            int nrt = grid2D.global_num(ir, jt);

            float arb = CalcAR2D(nb, nr);
            float art = CalcAR2D(n, nrt);
            float ar = CalcAR2D(nb, nrt);

            if ((CompareAR(arb, MaxAR) || CompareAR(art, MaxAR)) &&
                 CompareAR(arb, ar) && CompareAR(art, ar))
            {
                IJ_new[i][j] = NodeType.Left;
                for (int ik = i + 1; ik < Nx; ik++)
                {
                    IJ_new[ik][j] = NodeType.Removed;
                }
                merged = true;
            }
        }

        private void MergeLeft2D(ByteMat2D IJ_new, ref bool merged)
        {
            merged = true;
            int i = I; int j = J;
            MoveBottom2D(IJ_new);
            int bottom = J;
            J = j;
            MoveTop2D(IJ_new);
            int top = J;
            J = j;
            if (IJ_new[I][bottom] == NodeType.Right || IJ_new[I][J] == NodeType.Right || IJ_new[I][top] == NodeType.Right)
                return;

            MoveTop2D(IJ_new);
            int jt = J;
            J = j;

            MoveLeft2D(IJ_new);
            int il = I;
            MoveBottom2D(IJ_new);
            int jb = J;

            I = i; J = j;

            int n = grid2D.global_num(i, j);
            int nlb = grid2D.global_num(il, jb);
            int nl = grid2D.global_num(il, j);
            int nt = grid2D.global_num(i, jt);

            float alb = CalcAR2D(nlb, n);
            float alt = CalcAR2D(nl, nt);
            float al = CalcAR2D(nlb, nt);

            if ((CompareAR(alb, MaxAR) || CompareAR(alt, MaxAR)) &&
                 CompareAR(alb, al) && CompareAR(alt, al))
            {
                IJ_new[i][j] = NodeType.Right;
                for (int ik = i - 1; ik >= 0; ik--)
                {
                    IJ_new[ik][j] = NodeType.Removed;
                }
                merged = true;
            }
        }

        private void MergeTop2D(ByteMat2D IJ_new, ref bool merged)
        {
            merged = true;
            int i = I; int j = J;
            MoveLeft2D(IJ_new);
            int left = I;
            I = i;
            MoveRight2D(IJ_new);
            int right = I;
            I = i;
            if (IJ_new[left][J] == NodeType.Bottom || IJ_new[I][J] == NodeType.Bottom || IJ_new[right][J] == NodeType.Bottom)
                return;

            MoveLeft2D(IJ_new);
            int il = I;
            I = i;

            MoveTop2D(IJ_new);
            int jt = J;
            MoveRight2D(IJ_new);
            int ir = I;

            I = i; J = j;

            int n = grid2D.global_num(i, j);
            int nl = grid2D.global_num(il, j);
            int nt = grid2D.global_num(i, jt);
            int nrt = grid2D.global_num(ir, jt);

            float alt = CalcAR2D(nl, nt);
            float art = CalcAR2D(n, nrt);
            float at = CalcAR2D(nl, nrt);

            if ((CompareAR(alt, MaxAR) || CompareAR(art, MaxAR)) &&
                 CompareAR(alt, at) && CompareAR(art, at))
            {
                IJ_new[i][j] = NodeType.Bottom;
                for (int jk = j + 1; jk < Ny; jk++)
                {
                    IJ_new[i][jk] = NodeType.Removed;
                }
                merged = true;
            }
        }

        private void MergeBottom2D(ByteMat2D IJ_new, ref bool merged)
        {
            merged = true;
            int i = I; int j = J;
            MoveLeft2D(IJ_new);
            int left = I;
            I = i;
            MoveRight2D(IJ_new);
            int right = I;
            I = i;
            if (IJ_new[left][J] == NodeType.Top || IJ_new[I][J] == NodeType.Top || IJ_new[right][J] == NodeType.Top)
                return;

            MoveRight2D(IJ_new);
            int ir = I;
            I = i;

            MoveBottom2D(IJ_new);
            int jb = J;
            MoveLeft2D(IJ_new);
            int il = I;

            I = i; J = j;

            int n = grid2D.global_num(i, j);
            int nb = grid2D.global_num(i, jb);
            int nlb = grid2D.global_num(il, jb);
            int nr = grid2D.global_num(ir, j);

            float alb = CalcAR2D(nlb, n);
            float arb = CalcAR2D(nb, nr);
            float ab = CalcAR2D(nlb, nr);

            if ((CompareAR(alb, MaxAR) || CompareAR(arb, MaxAR)) &&
                 CompareAR(alb, ab) && CompareAR(arb, ab))
            {
                IJ_new[i][j] = NodeType.Top;
                for (int jk = j - 1; jk >= 0; jk--)
                {
                    IJ_new[i][jk] = NodeType.Removed;
                }
                merged = true;
            }
        }

        private void MakeUnStructedMatrix2D(ByteMat2D IJ_new)
        {
            bool merged = false;
            bool end = false;
            Quadrant current_quad;
            while (!merged && !End)
            {
                current_quad = Quadrants[QuadIndex];
                switch (current_quad)
                {
                    case Quadrant.RightTop:
                        switch (Directions[current_quad][DirIndex])
                        {
                            case Direction.Right:
                                if (!MoveRight2D(IJ_new))
                                    MergeTop2D(IJ_new, ref merged);
                                else {
                                    I = NodeI;
                                    end = MoveTop2D(IJ_new);
                                    if (!end) MergeTop2D(IJ_new, ref merged);
                                }
                                break;
                            case Direction.Top:
                                if (!MoveTop2D(IJ_new))
                                    MergeRight2D(IJ_new, ref merged);
                                else
                                {
                                    J = MidJ;
                                    end = MoveRight2D(IJ_new);
                                    if (!end) MergeRight2D(IJ_new, ref merged);
                                    else
                                    {
                                        I = NodeI; J = NodeJ;
                                        while (IJ_new[I][J] != NodeType.Removed && I < Nx - 1)
                                            I++;
                                        if (I < Nx - 1)
                                        {
                                            end = false;
                                            MoveTop2D(IJ_new);
                                            MidI = I; MidJ = J;
                                        }
                                        else
                                            end = true;
                                    }
                                }
                                break;
                        }
                        break;

                    case Quadrant.LeftTop:
                        switch (Directions[current_quad][DirIndex])
                        {
                            case Direction.Left:
                                if (!MoveLeft2D(IJ_new))
                                    MergeTop2D(IJ_new, ref merged);
                                else
                                {
                                    I = NodeI;
                                    end = MoveTop2D(IJ_new);
                                    if (!end) MergeTop2D(IJ_new, ref merged);
                                }
                                break;
                            case Direction.Top:
                                if (!MoveTop2D(IJ_new))
                                    MergeLeft2D(IJ_new, ref merged);
                                else
                                {
                                    J = NodeJ;
                                    end = MoveLeft2D(IJ_new);
                                    if (!end) MergeLeft2D(IJ_new, ref merged);
                                }
                                break;
                        }
                        break;

                    case Quadrant.LeftBottom:
                        switch (Directions[current_quad][DirIndex])
                        {
                            case Direction.Left:
                                if (!MoveLeft2D(IJ_new))
                                    MergeBottom2D(IJ_new, ref merged);
                                else
                                {
                                    I = NodeI;
                                    end = MoveBottom2D(IJ_new);
                                    if (!end) MergeBottom2D(IJ_new, ref merged);
                                }
                                break;
                            case Direction.Bottom:
                                if (!MoveBottom2D(IJ_new))
                                    MergeLeft2D(IJ_new, ref merged);
                                else
                                {
                                    J = NodeJ;
                                    end = MoveLeft2D(IJ_new);
                                    if (!end) MergeLeft2D(IJ_new, ref merged);
                                }
                                break;
                        }
                        break;

                    case Quadrant.RightBottom:
                        switch (Directions[current_quad][DirIndex])
                        {
                            case Direction.Right:
                                if (!MoveRight2D(IJ_new))
                                    MergeBottom2D(IJ_new, ref merged);
                                else
                                {
                                    I = NodeI;
                                    end = MoveBottom2D(IJ_new);
                                    if (!end) MergeBottom2D(IJ_new, ref merged);
                                }
                                break;
                            case Direction.Bottom:
                                if (!MoveBottom2D(IJ_new))
                                    MergeRight2D(IJ_new, ref merged);
                                else
                                {
                                    J = NodeJ;
                                    end = MoveRight2D(IJ_new);
                                    if (!end) MergeRight2D(IJ_new, ref merged);
                                }
                                break;
                        }
                        break;
                }
                if (end)
                {
                    end = false;
                    DirIndex++;
                    if (DirIndex >= 2)
                    {
                        DirIndex = 0;
                        QuadIndex++;
                        if (QuadIndex >= 4)
                        {
                            QuadIndex = 0;
                            End = true;
                        }
                    }
                    I = NodeI; J = NodeJ;
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
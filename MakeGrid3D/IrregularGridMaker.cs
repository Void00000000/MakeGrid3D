using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Documents;
using OpenTK.Audio.OpenAL;

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
        public bool AllSteps { get; set; } = false;
        private bool smartMerge = Default.smartMerge;
        public bool SmartMerge
        {
            get => smartMerge;
            set
            {
                if (value)
                {
                    aspect_ratios = new List<Tuple<float, int>>();
                    calcAROnly = true;
                }
                else calcAROnly = false;
                smartMerge = value;
            }
        }

        private List<Tuple<float, int>> aspect_ratios = new List<Tuple<float, int>>();
        private bool calcAROnly = Default.smartMerge;
        
        public int NodeI { get; set; } = 1;
        public int NodeJ { get; set; } = 1;
        public int NodeK { get; set; } = 1;

        public int MidI { get; set; } = 1;
        public int MidJ { get; set; } = 1;
        public int MidK { get; set; } = 1;

        public int I { get; set; } = 1;
        public int J { get; set; } = 1;
        public int K { get; set; } = 1;

        public bool End { get; set; } = false; 

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
        public void Reset()
        {
            I = 1;
            J = 1;
            K = 1;
            MidI = 1;
            MidJ = 1;
            MidK = 1;
            NodeI = 1;
            NodeJ = 1;
            NodeK = 1;
            QuadIndex = 0;
            DirIndex = 0;
            End = false;
            if (aspect_ratios != null) aspect_ratios.Clear();
            if (smartMerge) calcAROnly = true; else calcAROnly = false;
        }

        // TODO: В GridState нет инфы связанной с SmartMerge
        public void Set(GridState gridState)
        {
            Grid = gridState.Grid;
            I = gridState.I;
            J = gridState.J;
            K = gridState.K;
            MidI = gridState.MidI;
            MidJ = gridState.MidJ;
            MidK = gridState.MidK;
            NodeI = gridState.NodeI;
            NodeJ = gridState.NodeJ;
            NodeK = gridState.NodeK;
            QuadIndex = gridState.QuadIndex;
            DirIndex = gridState.DirIndex;
            End = gridState.End;
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
            } while (I < Nx - 1 && IJ_new[I][J] == NodeType.Removed);
            if (I >= Nx - 1)
                return true;
            return false;
        }

        private bool MoveLeft2D(ByteMat2D IJ_new)
        {
            do
            {
                I--;
            } while (I > 0 && IJ_new[I][J] == NodeType.Removed);
            if (I <= 0)
                return true;
            return false;
        }

        private bool MoveTop2D(ByteMat2D IJ_new)
        {
            do
            {
                J++;
            } while (J < Ny - 1 && IJ_new[I][J] == NodeType.Removed);
            if (J >= Ny - 1)
                return true;
            return false;
        }

        private bool MoveBottom2D(ByteMat2D IJ_new)
        {
            do
            {
                J--;
            } while (J > 0 && IJ_new[I][J] == NodeType.Removed);
            if (J <= 0)
                return true;
            return false;
        }

        private void GetNearIJ(Direction dir, out int ij, ByteMat2D IJ_new)
        {
            int i0 = I; int j0 = J;
            Move(dir, IJ_new);
            if (dir == Direction.Bottom || dir == Direction.Top)
                ij = J;
            else ij = I;
            I = i0; J = j0;
        }

        private void MergeRight2D(ByteMat2D IJ_new, ref bool merged)
        {
            merged = true;
            int right, bottom, top;
            GetNearIJ(Direction.Right, out right, IJ_new);
            GetNearIJ(Direction.Bottom, out bottom, IJ_new);
            GetNearIJ(Direction.Top, out top, IJ_new);

            if (IJ_new[I][bottom] == NodeType.Left || IJ_new[I][top] == NodeType.Left || IJ_new[I][J] != NodeType.Regular)
                return;

            int n = grid2D.global_num(I, J);
            int nb = grid2D.global_num(I, bottom);
            int nt = grid2D.global_num(I, top);
            int nr = grid2D.global_num(right, J);
            int nrb = grid2D.global_num(right, bottom);
            int nrt = grid2D.global_num(right, top);

            if (nrt < 0 || nrb < 0) return;

            float xmin = grid2D.XY[nb].X;
            float ymin = grid2D.XY[nb].Y;
            float xmax = grid2D.XY[nrt].X;
            float ymax = grid2D.XY[nrt].Y;
            if (grid2D.Area.FindSubArea(xmin, xmax, ymin, ymax) < 0) return;

            float arb = CalcAR2D(nb, nr);
            float art = CalcAR2D(n, nrt);
            float ar = CalcAR2D(nb, nrt);

            if ((CompareAR(arb, MaxAR) || CompareAR(art, MaxAR)) &&
                 CompareAR(arb, ar) && CompareAR(art, ar))
            {
                for (int ik = I + 1; ik < Nx; ik++)
                {
                    if (IJ_new[ik][J] == NodeType.Bottom || IJ_new[ik][J] == NodeType.Top) return;
                }

                if (calcAROnly)
                {
                    if (arb < 1f) arb = 1f / arb;
                    if (art < 1f) art = 1f / art;
                    float a = (arb > art) ? arb : art;
                    aspect_ratios.Add(Tuple.Create(a, J));
                    return;
                }

                IJ_new[I][J] = NodeType.Left;
                for (int ik = I + 1; ik < Nx; ik++)
                {
                    IJ_new[ik][J] = NodeType.Removed;
                }
                merged = true;
            }
        }

        private void MergeLeft2D(ByteMat2D IJ_new, ref bool merged)
        {
            merged = true;
            int left, bottom, top;
            GetNearIJ(Direction.Left, out left, IJ_new);
            GetNearIJ(Direction.Bottom, out bottom, IJ_new);
            GetNearIJ(Direction.Top, out top, IJ_new);
            if (IJ_new[I][bottom] == NodeType.Right || IJ_new[I][top] == NodeType.Right || IJ_new[I][J] != NodeType.Regular)
                return;

            int n = grid2D.global_num(I, J);
            int nl = grid2D.global_num(left, J);
            int nb = grid2D.global_num(I, bottom);
            int nt = grid2D.global_num(I, top);
            int nlb = grid2D.global_num(left, bottom);
            int nlt = grid2D.global_num(left, top);

            if (nlb < 0 || nlt < 0) return;

            float xmin = grid2D.XY[nlb].X;
            float ymin = grid2D.XY[nlb].Y;
            float xmax = grid2D.XY[nt].X;
            float ymax = grid2D.XY[nt].Y;
            if (grid2D.Area.FindSubArea(xmin, xmax, ymin, ymax) < 0) return;

            float alb = CalcAR2D(nlb, n);
            float alt = CalcAR2D(nl, nt);
            float al = CalcAR2D(nlb, nt);

            if ((CompareAR(alb, MaxAR) || CompareAR(alt, MaxAR)) &&
                 CompareAR(alb, al) && CompareAR(alt, al))
            {
                for (int ik = I - 1; ik >= 0; ik--)
                {
                    if (IJ_new[ik][J] == NodeType.Bottom || IJ_new[ik][J] == NodeType.Top) return;
                }

                if (calcAROnly)
                {
                    if (alb < 1f) alb = 1f / alb;
                    if (alt < 1f) alt = 1f / alt;
                    float a = (alb > alt) ? alb : alt;
                    aspect_ratios.Add(Tuple.Create(a, J));
                    return;
                }

                IJ_new[I][J] = NodeType.Right;
                for (int ik = I - 1; ik >= 0; ik--)
                {
                    IJ_new[ik][J] = NodeType.Removed;
                }
                merged = true;
            }
        }

        private void MergeTop2D(ByteMat2D IJ_new, ref bool merged)
        {
            merged = true;
            int top, left, right;
            GetNearIJ(Direction.Left, out left, IJ_new);
            GetNearIJ(Direction.Right, out right, IJ_new);
            GetNearIJ(Direction.Top, out top, IJ_new);
            if (IJ_new[left][J] == NodeType.Bottom || IJ_new[right][J] == NodeType.Bottom || IJ_new[I][J] != NodeType.Regular)
                return;

            int n = grid2D.global_num(I, J);
            int nl = grid2D.global_num(left, J);
            int nr = grid2D.global_num(right, J);
            int nt = grid2D.global_num(I, top);
            int nlt = grid2D.global_num(left, top);
            int nrt = grid2D.global_num(right, top);

            if (nlt < 0 || nrt < 0) return;

            float xmin = grid2D.XY[nl].X;
            float ymin = grid2D.XY[nl].Y;
            float xmax = grid2D.XY[nrt].X;
            float ymax = grid2D.XY[nrt].Y;
            if (grid2D.Area.FindSubArea(xmin, xmax, ymin, ymax) < 0) return;

            float alt = CalcAR2D(nl, nt);
            float art = CalcAR2D(n, nrt);
            float at = CalcAR2D(nl, nrt);

            if ((CompareAR(alt, MaxAR) || CompareAR(art, MaxAR)) &&
                 CompareAR(alt, at) && CompareAR(art, at))
            {
                for (int jk = J + 1; jk < Ny; jk++)
                {
                    if (IJ_new[I][jk] == NodeType.Left || IJ_new[I][jk] == NodeType.Right) return;
                }

                if (calcAROnly)
                {
                    if (alt < 1f) alt = 1f / alt;
                    if (art < 1f) art = 1f / art;
                    float a = (alt > art) ? alt : art;
                    aspect_ratios.Add(Tuple.Create(a, I));
                    return;
                }

                IJ_new[I][J] = NodeType.Bottom;
                for (int jk = J + 1; jk < Ny; jk++)
                {
                    IJ_new[I][jk] = NodeType.Removed;
                }
                merged = true;
            }
        }

        private void MergeBottom2D(ByteMat2D IJ_new, ref bool merged)
        {
            merged = true;
            int bottom, left, right;
            GetNearIJ(Direction.Left, out left, IJ_new);
            GetNearIJ(Direction.Right, out right, IJ_new);
            GetNearIJ(Direction.Bottom, out bottom, IJ_new);
            if (IJ_new[left][J] == NodeType.Top || IJ_new[right][J] == NodeType.Top || IJ_new[I][J] != NodeType.Regular)
                return;

            int n = grid2D.global_num(I, J);
            int nl = grid2D.global_num(left, J);
            int nr = grid2D.global_num(right, J);
            int nb = grid2D.global_num(I, bottom);
            int nlb = grid2D.global_num(left, bottom);
            int nrb = grid2D.global_num(right, bottom);

            if (nlb < 0 || nrb < 0) return;

            float xmin = grid2D.XY[nlb].X;
            float ymin = grid2D.XY[nlb].Y;
            float xmax = grid2D.XY[nr].X;
            float ymax = grid2D.XY[nr].Y;
            if (grid2D.Area.FindSubArea(xmin, xmax, ymin, ymax) < 0) return;

            float alb = CalcAR2D(nlb, n);
            float arb = CalcAR2D(nb, nr);
            float ab = CalcAR2D(nlb, nr);

            if ((CompareAR(alb, MaxAR) || CompareAR(arb, MaxAR)) &&
                 CompareAR(alb, ab) && CompareAR(arb, ab))
            {
                for (int jk = J - 1; jk >= 0; jk--)
                {
                    if (IJ_new[I][jk] == NodeType.Left || IJ_new[I][jk] == NodeType.Right) return;
                }

                if (calcAROnly)
                {
                    if (alb < 1f) alb = 1f / alb;
                    if (arb < 1f) arb = 1f / arb;
                    float a = (alb > arb) ? alb : arb;
                    aspect_ratios.Add(Tuple.Create(a, I));
                    return;
                }

                IJ_new[I][J] = NodeType.Top;
                for (int jk = J - 1; jk >= 0; jk--)
                {
                    IJ_new[I][jk] = NodeType.Removed;
                }
                merged = true;
            }
        }

        private void Merge(Direction dir, ByteMat2D IJ_new, ref bool merged)
        {
            if (I < Nx - 1 && I > 0 && J < Ny - 1 && J > 0)
            {
                switch (dir)
                {
                    case Direction.Top:
                        MergeTop2D(IJ_new, ref merged); break;
                    case Direction.Bottom:
                        MergeBottom2D(IJ_new, ref merged); break;
                    case Direction.Left:
                        MergeLeft2D(IJ_new, ref merged); break;
                    case Direction.Right:
                        MergeRight2D(IJ_new, ref merged); break;
                }
            }
        }

        private bool Move(Direction dir, ByteMat2D IJ_new)
        {
            switch (dir)
            {
                case Direction.Top:
                    return MoveTop2D(IJ_new);
                case Direction.Bottom:
                    return MoveBottom2D(IJ_new);
                case Direction.Left:
                    return MoveLeft2D(IJ_new);
                case Direction.Right:
                    return MoveRight2D(IJ_new);
                default:
                    return false;
            }
        }

        private void ProcessNodeBasic(Direction nodeDir, Direction mergeDir, ByteMat2D IJ_new, ref bool merged)
        {
            bool end = false;
            Merge(mergeDir, IJ_new, ref merged);
            if (Move(nodeDir, IJ_new))
            {
                if (mergeDir == Direction.Top || mergeDir == Direction.Bottom) I = MidI; else J = MidJ;

                if (Move(mergeDir, IJ_new))
                {
                    // Переход на новую строку
                    I = MidI; J = MidJ;
                    bool end_cond = true;
                    switch (mergeDir) {
                        case Direction.Top:
                            while (IJ_new[I][J] != NodeType.Removed && J < Ny - 1)
                                J++; 
                            if (J < Ny - 1) end_cond = false; break;
                        case Direction.Bottom:
                            while (IJ_new[I][J] != NodeType.Removed && J > 0)
                                J--;
                            if (J > 0) end_cond = false; break;
                        case Direction.Right:
                            while (IJ_new[I][J] != NodeType.Removed && I < Nx - 1)
                                I++;
                            if (I < Nx - 1) end_cond = false; break;
                        case Direction.Left:
                            while (IJ_new[I][J] != NodeType.Removed && I > 0)
                                I--;
                            if (I > 0) end_cond = false; break;
                    }
                    if (!end_cond)
                    {
                        Move(nodeDir, IJ_new);
                        MidI = I; MidJ = J;
                    }
                    else end = true;
                }
            }
            if (end)
            {
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
                MidI = NodeI; MidJ = NodeJ;
            }
        }

        private void ProcessNodeSmart(Direction nodeDir, Direction mergeDir, ByteMat2D IJ_new, ref bool merged)
        {
            merged = true;
            bool end = false;
            bool end_row = false;
            if (!calcAROnly)
            {
                if (aspect_ratios.Count != 0)
                {
                    var index = aspect_ratios[aspect_ratios.Count - 1].Item2;
                    aspect_ratios.RemoveAt(aspect_ratios.Count - 1);
                    if (nodeDir == Direction.Left || nodeDir == Direction.Right) I = index; else J = index;
                }
                else end_row = true;
            }
            if (!end_row) Merge(mergeDir, IJ_new, ref merged);
            if (calcAROnly) end_row = Move(nodeDir, IJ_new);
            if (end_row)
            {
                if (calcAROnly)
                {
                    aspect_ratios.Sort((a, b) => a.Item1.CompareTo(b.Item1));
                    calcAROnly = false;
                    return;
                }
                else
                {
                    calcAROnly = true;
                    aspect_ratios.Clear();
                }
                if (mergeDir == Direction.Top || mergeDir == Direction.Bottom) I = MidI; else J = MidJ;

                if (Move(mergeDir, IJ_new))
                {
                    // Переход на новую строку
                    I = MidI; J = MidJ;
                    bool end_cond = true;
                    switch (mergeDir)
                    {
                        case Direction.Top:
                            while (IJ_new[I][J] != NodeType.Removed && J < Ny - 1)
                                J++;
                            if (J < Ny - 1) end_cond = false; break;
                        case Direction.Bottom:
                            while (IJ_new[I][J] != NodeType.Removed && J > 0)
                                J--;
                            if (J > 0) end_cond = false; break;
                        case Direction.Right:
                            while (IJ_new[I][J] != NodeType.Removed && I < Nx - 1)
                                I++;
                            if (I < Nx - 1) end_cond = false; break;
                        case Direction.Left:
                            while (IJ_new[I][J] != NodeType.Removed && I > 0)
                                I--;
                            if (I > 0) end_cond = false; break;
                    }
                    if (!end_cond)
                    {
                        Move(nodeDir, IJ_new);
                        MidI = I; MidJ = J;
                    }
                    else end = true;
                }
            }
            if (end)
            {
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
                MidI = NodeI; MidJ = NodeJ;
            }
        }

        private void ProcessNode(Direction nodeDir, Direction mergeDir, ByteMat2D IJ_new, ref bool merged)
        {
            if (SmartMerge)
                ProcessNodeSmart(nodeDir, mergeDir, IJ_new, ref merged);
            else
                ProcessNodeBasic(nodeDir, mergeDir, IJ_new, ref merged);
        }

        private void MakeUnStructedMatrix2D(ByteMat2D IJ_new)
        {
            bool merged = false;
            Quadrant current_quad;
            while ((AllSteps || !merged) && !End)
            {
                current_quad = Quadrants[QuadIndex];
                switch (current_quad)
                {
                    case Quadrant.RightTop:
                        switch (Directions[current_quad][DirIndex])
                        {
                            case Direction.Right:
                                ProcessNode(Direction.Right, Direction.Top, IJ_new, ref merged);
                                break;
                            case Direction.Top:
                                ProcessNode(Direction.Top, Direction.Right, IJ_new, ref merged);
                                break;
                        }
                        break;

                    case Quadrant.LeftTop:
                        switch (Directions[current_quad][DirIndex])
                        {
                            case Direction.Left:
                                ProcessNode(Direction.Left, Direction.Top, IJ_new, ref merged);
                                break;
                            case Direction.Top:
                                ProcessNode(Direction.Top, Direction.Left, IJ_new, ref merged);
                                break;
                        }
                        break;

                    case Quadrant.LeftBottom:
                        switch (Directions[current_quad][DirIndex])
                        {
                            case Direction.Left:
                                ProcessNode(Direction.Left, Direction.Bottom, IJ_new, ref merged);
                                break;
                            case Direction.Bottom:
                                ProcessNode(Direction.Bottom, Direction.Left, IJ_new, ref merged);
                                break;
                        }
                        break;

                    case Quadrant.RightBottom:
                        switch (Directions[current_quad][DirIndex])
                        {
                            case Direction.Right:
                                ProcessNode(Direction.Right, Direction.Bottom, IJ_new, ref merged);
                                break;
                            case Direction.Bottom:
                                ProcessNode(Direction.Bottom, Direction.Right, IJ_new, ref merged);
                                break;
                        }
                        break;
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
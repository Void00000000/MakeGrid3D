using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeGrid3D
{
    // TODO: в нерегулярных сетках не все слои выделяются
    class CrossSections
    {
        // XY - 0; XZ - 1; YZ - 2;
        private List<Tuple<Mesh, double>>[] planes = new List<Tuple<Mesh, double>>[3];
        private int indexPlane = 0;
        private int indexPlaneSec = -1;
        public int MaxPlaneIndex { get; private set; } = int.MinValue;
        public bool Active { get; set; } = false;
        public double CurrentValue { get; private set; }
        public Plane CurrentPlane
        {
            get { return (Plane)indexPlane; }
            set
            {
                try
                {
                    indexPlane = (int)value;
                    MaxPlaneIndex = planes[indexPlane].Count - 1;
                    indexPlaneSec = -1;
                    Active = false;
                }
                catch (InvalidCastException) { Active = false; }
            }
        }

        public int CurrentPlaneSec
        {
            get => indexPlaneSec;
            set
            {
                if (value < 0)
                {
                    indexPlaneSec = -1;
                    Active = false;
                }
                else if (value > MaxPlaneIndex)
                {
                    indexPlaneSec = MaxPlaneIndex + 1;
                    Active = false;
                }
                else
                {
                    Active = true;
                    indexPlaneSec = value;
                    CurrentValue = planes[indexPlane][indexPlaneSec].Item2;
                }
            }
        }

        public Color4 PlaneColor { get; set; } = new Color4(50 / 255f, 168 / 255f, 107 / 255f, 0.75f);
        public CrossSections(Grid3D grid3D)
        {
            double xmin = grid3D.Area.X0;
            double ymin = grid3D.Area.Y0;
            double zmin = grid3D.Area.Z0;
            double xmax = grid3D.Area.Xn;
            double ymax = grid3D.Area.Yn;
            double zmax = grid3D.Area.Zn;
            // XY
            planes[0] = new List<Tuple<Mesh, double>>(grid3D.Nz);
            for (int k = 0; k < grid3D.Nz; k++)
            {
                int n = -1;
                for (int j = 0; j < grid3D.Ny; j++)
                    for (int i = 0; i < grid3D.Nx; i++)
                    {
                        n = grid3D.global_num(i, j, k);
                        if (n >= 0)
                            break;
                    }
                if (n >= 0)
                {
                    double z = grid3D.XYZ[n].Z;
                    double[] vertices = { xmin, ymin, z,
                                         xmax, ymin, z,
                                         xmin, ymax, z,
                                         xmax, ymax, z};
                    uint[] indices = { 0, 2, 3, 0, 1, 3 };
                    Mesh mesh = new Mesh(vertices, indices);
                    planes[0].Add(Tuple.Create(mesh, z));
                }
            }
            // XZ
            planes[1] = new List<Tuple<Mesh, double>>(grid3D.Ny);
            for (int j = 0; j < grid3D.Ny; j++)
            {
                int n = -1;
                for (int k = 0; k < grid3D.Nz; k++)
                    for (int i = 0; i < grid3D.Nx; i++)
                    {
                        n = grid3D.global_num(i, j, k);
                        if (n >= 0)
                            break;
                    }
                if (n >= 0)
                {
                    double y = grid3D.XYZ[n].Y;
                    double[] vertices = { xmin, y, zmin,
                                         xmax, y, zmin,
                                         xmin, y, zmax,
                                         xmax, y, zmax};
                    uint[] indices = { 0, 2, 3, 0, 1, 3 };
                    Mesh mesh = new Mesh(vertices, indices);
                    planes[1].Add(Tuple.Create(mesh, y));
                }
            }
            // YZ
            planes[2] = new List<Tuple<Mesh, double>>(grid3D.Nx);
            for (int i = 0; i < grid3D.Nx; i++)
            {
                int n = -1;
                for (int k = 0; k < grid3D.Nz; k++)
                    for (int j = 0; j < grid3D.Ny; j++)
                    {
                        n = grid3D.global_num(i, j, k);
                        if (n >= 0)
                            break;
                    }
                if (n >= 0)
                {
                    double x = grid3D.XYZ[n].X;
                    double[] vertices = { x, ymin, zmin,
                                     x, ymax, zmin,
                                     x, ymin, zmax,
                                     x, ymax, zmax};
                    uint[] indices = { 0, 2, 3, 0, 1, 3 };
                    Mesh mesh = new Mesh(vertices, indices);
                    planes[2].Add(Tuple.Create(mesh, x));
                }
            }
        }

        public void DrawPlane(Shader shader)
        {
            if (Active)
            {
                shader.SetColor4("current_color", PlaneColor);
                planes[indexPlane][indexPlaneSec].Item1.DrawElems(6, 0, PrimitiveType.Triangles);
            }
        }
    }
}

using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeGrid3D
{
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

    enum Quadrant
    {
        RightTop,
        LeftTop,
        LeftBottom,
        RightBottom
    }

    enum Plane
    {
        XY,
        XZ,
        YZ,
    }
    interface IGrid
    {
        public int Nnodes { get; }
        public int Nelems { get; }
        public int Nmats { get; }
        public float MeanAR { get; set; }
        public float WorstAR { get; set; }
        public string PrintInfo();
    }

    class GridState
    {
        public IGrid Grid { get; }
        public int I { get; } = 1;
        public int J { get; } = 1;
        public int K { get; } = 1;
        public int MidI { get; } = 1;
        public int MidJ { get; } = 1;
        public int MidK { get; } = 1;
        public int NodeI { get; } = 1;
        public int NodeJ { get; } = 1;
        public int NodeK { get; } = 1;
        public int QuadIndex { get; } = 0;
        public int DirIndex { get; } = 0;
        public bool End { get; } = false;
        public GridState(IGrid grid, IrregularGridMaker irregularGridMaker)
        {
            Grid = grid;
            I = irregularGridMaker.I;
            J = irregularGridMaker.J;
            K = irregularGridMaker.K;
            MidI = irregularGridMaker.MidI;
            MidJ = irregularGridMaker.MidJ;
            MidK = irregularGridMaker.MidK;
            NodeI = irregularGridMaker.NodeI;
            NodeJ = irregularGridMaker.NodeJ;
            NodeK = irregularGridMaker.NodeK;
            QuadIndex = irregularGridMaker.QuadIndex;
            DirIndex = irregularGridMaker.DirIndex;
            End = irregularGridMaker.End;
        }
        public GridState(IGrid grid)
        {
            Grid = grid;
        }
    }

    // TODO: В UI значения по умолчаниюплохо ставятся при загрузке новой сетки
    class Mesh
    {
        public int Vao { get; private set; }
        public int Vbo { get; private set; }
        public int Ebo { get; private set; }
        public int VLen { get; private set; }
        public int ILen { get; private set; }
        public Mesh(int vbo, uint[] indices, int vLen)
        {
            Vbo = vbo;
            Vao = GL.GenVertexArray();
            GL.BindVertexArray(Vao);
            Ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
            VLen = vLen;
            ILen = indices.Length;
            GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.BindVertexArray(0);
        }

        public Mesh(float[] vertices, uint[] indices, bool IsTexture = false)
        {
            Vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            Vao = GL.GenVertexArray();
            GL.BindVertexArray(Vao);
            Ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
            VLen = vertices.Length;
            ILen = indices.Length;
            if (!IsTexture)
            {
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);
            }
            else
            {
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);

                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
                GL.EnableVertexAttribArray(1);
            }
            GL.BindVertexArray(0);
        }

        public Mesh(Mesh mesh)
        {
            Vao = mesh.Vao;
            Vbo = mesh.Vbo;
            Ebo = mesh.Ebo;
            VLen = mesh.VLen;
            ILen = mesh.ILen;
        }

        public void Use()
        {
            GL.BindVertexArray(Vao);
        }

        public void DrawElems(int count, int offset, PrimitiveType type)
        {
            GL.BindVertexArray(Vao);
            GL.DrawElements(type, count, DrawElementsType.UnsignedInt, offset * sizeof(uint));
        }

        public void DrawElems(PrimitiveType type)
        {
            GL.BindVertexArray(Vao);
            GL.DrawElements(type, ILen, DrawElementsType.UnsignedInt, 0);
        }

        public void DrawVerices(int count, int offset, PrimitiveType type)
        {
            GL.BindVertexArray(Vao);
            GL.DrawArrays(type, offset, count);
        }

        public void DrawVerices(PrimitiveType type)
        {
            GL.BindVertexArray(Vao);
            GL.DrawArrays(type, 0, VLen);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(Vbo);
            GL.DeleteVertexArray(Vao);
            GL.DeleteBuffer(Ebo);
        }
    }

    public class GridParams
    {
        public bool TwoD { get; }
        public List<float> Xw { get; }
        public List<float> Yw {get;}
        public List<float> Zw {get;}
        public List<SubArea3D> Mw { get; }
        public List<int> NX { get; }
        public List<int> NY { get; }
        public List<int> NZ { get; }
        public List<float> QX { get; }
        public List<float> QY { get; }
        public List<float> QZ { get; }
        public List<Color4> Mats { get; }

        public GridParams(bool twoD, List<float> xw, List<float> yw, List<float> zw, List<SubArea3D> mw, List<int> nx, List<int> ny, List<int> nz, List<float> qx, List<float> qy, List<float> qz, List<Color4> mats)
        {
            TwoD = twoD;
            Xw = xw;
            Yw = yw;
            Zw = zw;
            Mw = mw;
            NX = nx;
            NY = ny;
            NZ = nz;
            QX = qx;
            QY = qy;
            QZ = qz;
            Mats = mats;
        }
    }
}

using OpenTK.Compute.OpenCL;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Input;
using OpenTK.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml.Linq;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MakeGrid3D
{
    // Wrapper class around a shader
    public class Shader
    {
        // the location of a final shader program
        public int Handle { get; }

        public Shader(string vertexPath, string fragmentPath)
        {
            string VertexShaderSource = "";
            string FragmentShaderSource = "";
            try
            {
                string workingDirectory = Environment.CurrentDirectory;
                string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
                string VertexPath = projectDirectory + vertexPath;
                string FragmentPath = projectDirectory + fragmentPath;
                VertexShaderSource = File.ReadAllText(VertexPath);
                FragmentShaderSource = File.ReadAllText(FragmentPath);
            }
            catch (Exception e)
            {
                if (e is DirectoryNotFoundException || e is FileNotFoundException)
                {
                    ErrorHandler.FileReadingErrorMessage("Не удалось найти шейдеры");
                }
                else
                {
                    ErrorHandler.FileReadingErrorMessage("Не удалось прочитать файлы шейдеров");
                }
            }

            int VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);

            int FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);

            // Compiling shaders-----------------------------------------------------
            GL.CompileShader(VertexShader);

            GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out int success_v);
            if (success_v == 0)
            {
                string infoLog = "Ошибка компиляции vertex шейдера\n" + GL.GetShaderInfoLog(VertexShader);
                ErrorHandler.BuildingErrorMessage(infoLog);
            }

            GL.CompileShader(FragmentShader);

            GL.GetShader(FragmentShader, ShaderParameter.CompileStatus, out int success_f);
            if (success_f == 0)
            {
                string infoLog = "Ошибка компиляции fragment шейдера\n" + GL.GetShaderInfoLog(FragmentShader);
                ErrorHandler.BuildingErrorMessage(infoLog); ;
            }

            // Linking shaders-----------------------------------------------------
            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, VertexShader);
            GL.AttachShader(Handle, FragmentShader);

            GL.LinkProgram(Handle);

            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = "Ошибка связывания шейдеров\n" + GL.GetProgramInfoLog(Handle);
                ErrorHandler.BuildingErrorMessage(infoLog);
            }

            // Clenup
            GL.DetachShader(Handle, VertexShader);
            GL.DetachShader(Handle, FragmentShader);
            GL.DeleteShader(FragmentShader);
            GL.DeleteShader(VertexShader);
        }

        // Bind the shader
        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public void SetVector2(string name, Vector2 vector)
        {
            Use();
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform2(location, vector.X, vector.Y);
        }

        public void SetFloat(string name, float f)
        {
            Use();
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform1(location, f);
        }

        public void SetMatrix4(string name, ref Matrix4 matrix)
        {
            Use();
            int location = GL.GetUniformLocation(Handle, name);
            GL.UniformMatrix4(location, true, ref matrix);
        }

        public void SetColor4(string name, Color4 vector)
        {
            Use();
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform4(location, vector.R, vector.G, vector.B, vector.A);
        }


        public void DashedLines(bool dash, float dash_size=0.2f, float gap_size=0.5f)
        {
            if (dash)
            {
                SetFloat("u_dashSize", dash_size);
                SetFloat("u_gapSize", gap_size);
            }
            else
            {
                SetFloat("u_dashSize", BufferClass.linesSize);
                SetFloat("u_gapSize", 0f);
            }
            Use();
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(Handle);

                disposedValue = true;
            }
        }

        ~Shader()
        {
            GL.DeleteProgram(Handle);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    class Mesh
    {
        public int Vao { get; }
        private int vbo;
        private int ebo;
        public Mesh(int vbo, int vao, int ebo)
        {
            this.vbo = vbo;
            Vao = vao;
            this.ebo = ebo;
            GL.BindVertexArray(Vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vbo);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.ebo);
            GL.BindVertexArray(0);
        }

        public Mesh(float[] vertices, uint[] indices) {
            
        }

        public void Dispose()
        {
            GL.DeleteBuffer(vbo);
            GL.DeleteVertexArray(Vao);
            GL.DeleteBuffer(ebo);
        }
    }


    class RenderGrid
    {
        Mesh regularGrid;
        Mesh irregularGrid;
        Mesh area;

        public readonly Shader shader;
        private float[] vertices;
        private float[] vertices_unstr;
        private uint[] indices;
        private uint[] indices_area;
        private uint[] indices_unstr;
        private Matrix4 projection;
        private Matrix4 model;
        private List<Color4> AreaColors;

        public readonly Grid2D grid2D;
        public float Left { get; private set; }
        public float Right { get; private set; }
        public float Bottom { get; private set; }
        public float Top { get; private set; }

        public RenderGrid(Grid2D grid2D)
        {
            this.grid2D = grid2D;
            AreaColors = new List<Color4>()
            {
                Default.area1Color,
                Default.area2Color,
                Default.area3Color
            };
            AssembleVertices();
            FillBuffers();
            shader = new Shader("\\Shaders\\shader.vert", "\\Shaders\\shader.frag");

            float left = grid2D.X0;
            float right = grid2D.Xn;
            float bottom = grid2D.Y0;
            float top = grid2D.Yn;

            float width = right - left;
            float height = top - bottom;

            float indent = 0.2f;
            float hor_offset = width * indent;
            float ver_offset = height * indent;

            float left_ = left - hor_offset;
            float right_ = right + hor_offset;
            float bottom_ = bottom - ver_offset;
            float top_ = top + ver_offset;

            float w;
            if ((right_ - left_) >= (top_ - bottom_))
            {
                Left = left_;
                Right = right_;
                w = ((Right - Left) - (top_ - bottom_)) / 2;
                Top = top_ + w;
                Bottom = bottom_ - w;
            }
            else
            {
                Top = top_;
                Bottom = bottom_;
                w = ((Top - Bottom) - (right_ - left_)) / 2;
                Left = left_ - w;
                Right = right_ + w;
            }
            projection = Matrix4.CreateOrthographicOffCenter(Left, Right, Bottom, Top, -0.1f, 100.0f);
        }


        private void AssembleVertices(bool re=true, bool irre=true)
        {
            int Nelems = grid2D.Nelems;
            int Nnodes = grid2D.Nnodes;
            int Nareas = grid2D.Nareas;

            if (re)
            {
                vertices = new float[Nnodes * 3];
                int n = 0;
                foreach (Vector2 node in grid2D.XY)
                {
                    vertices[n] = node.X; vertices[n + 1] = node.Y; vertices[n + 2] = 0;
                    n += 3;
                }

                indices = new uint[Nelems * 4 * 2];
                int e = 0;
                foreach (Elem5 elem5 in grid2D.Elems)
                {
                    uint n1 = (uint)elem5.n1;
                    uint n2 = (uint)elem5.n2;
                    uint n3 = (uint)elem5.n3;
                    uint n4 = (uint)elem5.n4;
                    indices[e] = n1; indices[e + 1] = n2; indices[e + 2] = n2; indices[e + 3] = n4;
                    indices[e + 4] = n3; indices[e + 5] = n4; indices[e + 6] = n1; indices[e + 7] = n3;
                    e += 8;
                }

                indices_area = new uint[Nareas * 6];
                int s = 0;
                foreach (SubArea subArea in grid2D.Mw)
                {
                    int ix1 = grid2D.IXw[subArea.nx1];
                    int iy1 = grid2D.IYw[subArea.ny1];
                    int ix2 = grid2D.IXw[subArea.nx2];
                    int iy2 = grid2D.IYw[subArea.ny2];

                    uint n1 = (uint)grid2D.global_num(ix1, iy1);
                    uint n2 = (uint)grid2D.global_num(ix2, iy1);
                    uint n3 = (uint)grid2D.global_num(ix1, iy2);
                    uint n4 = (uint)grid2D.global_num(ix2, iy2);

                    indices_area[s] = n1; indices_area[s + 1] = n2; indices_area[s + 2] = n4;
                    indices_area[s + 3] = n1; indices_area[s + 4] = n3; indices_area[s + 5] = n4;
                    s += 6;
                }
            }

            if (irre)
            {
                vertices_unstr = new float[grid2D.UnStrXY.Count * 3];
                int un = 0;
                foreach (Vector2 node in grid2D.UnStrXY)
                {
                    vertices_unstr[un] = node.X; vertices_unstr[un + 1] = node.Y; vertices_unstr[un + 2] = 0;
                    un += 3;
                }

                indices_unstr = new uint[grid2D.UnStrElems.Count * 4 * 2];
                int u = 0;
                foreach (Elem5 uelem5 in grid2D.UnStrElems)
                {
                    uint n1 = (uint)uelem5.n1;
                    uint n2 = (uint)uelem5.n2;
                    uint n3 = (uint)uelem5.n3;
                    uint n4 = (uint)uelem5.n4;
                    indices_unstr[u] = n1; indices_unstr[u + 1] = n2; indices_unstr[u + 2] = n2; indices_unstr[u + 3] = n4;
                    indices_unstr[u + 4] = n3; indices_unstr[u + 5] = n4; indices_unstr[u + 6] = n1; indices_unstr[u + 7] = n3;
                    u += 8;
                }
            }
        }


        private void FillBuffers(bool re = true, bool irre = true)
        {
            int vbo1, vbo2;
            int vao1, vao2, vao3;
            int ebo1, ebo2, ebo3;
            if (re)
            {
                vbo1 = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo1);
                GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
                vao1 = GL.GenVertexArray();
                GL.BindVertexArray(vao1);
                ebo1 = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo1);
                GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
                regularGrid = new Mesh(vbo1, vao1, ebo1);

                vao3 = GL.GenVertexArray();
                GL.BindVertexArray(vao3);
                ebo3 = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo3);
                GL.BufferData(BufferTarget.ElementArrayBuffer, indices_area.Length * sizeof(uint), indices_area, BufferUsageHint.StaticDraw);
                area = new Mesh(vbo1, vao3, ebo3);
            }
            if (irre)
            {
                vbo2 = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo2);
                GL.BufferData(BufferTarget.ArrayBuffer, vertices_unstr.Length * sizeof(float), vertices_unstr, BufferUsageHint.StaticDraw);
                vao2 = GL.GenVertexArray();
                GL.BindVertexArray(vao2);
                ebo2 = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo2);
                GL.BufferData(BufferTarget.ElementArrayBuffer, indices_unstr.Length * sizeof(uint), indices_unstr, BufferUsageHint.StaticDraw);
                irregularGrid = new Mesh(vbo2, vao2, ebo2);
            }
        }

        public void RebuildUnStructedGrid()
        {
            grid2D.MakeUnStructedGrid();
            AssembleVertices(re:false);
            FillBuffers(re: false);
        }

        private void DrawLines(int vao, int iLength, int iOffset) {
            GL.BindVertexArray(vao);
            shader.SetColor4("current_color", BufferClass.linesColor);
            GL.DrawElements(PrimitiveType.Lines, iLength, DrawElementsType.UnsignedInt, iOffset);
        }

        public void DrawNodes(int vao, int vLength, int vOffset)
        {
            GL.BindVertexArray(vao);
            shader.SetColor4("current_color", BufferClass.pointsColor);
            GL.DrawArrays(PrimitiveType.Points, vOffset, vLength);
        }

        public void RenderFrame()
        {
            GL.ClearColor(BufferClass.bgColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            // Чтобы работали прозрачные цвета
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend);

            shader.Use();
            shader.SetMatrix4("projection", ref projection);
            model = BufferClass.translate * BufferClass.scale;
            shader.SetMatrix4("model", ref model);
            shader.SetVector2("u_resolution", new Vector2(Right - Left, Top - Bottom));
            GL.LineWidth(BufferClass.linesSize);
            GL.PointSize(BufferClass.pointsSize);

            if (!BufferClass.wireframeMode)
            {
                int s = 0;
                GL.BindVertexArray(area.Vao);
                foreach (SubArea subArea in grid2D.Mw)
                {
                    shader.SetColor4("current_color", AreaColors[subArea.wi]);
                    GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, s * sizeof(uint));
                    s += 6;
                }
            }
            if (BufferClass.unstructedGridMode)
            {
                if (BufferClass.drawRemovedLinesMode)
                {
                    shader.DashedLines(true);
                    DrawLines(regularGrid.Vao, indices.Length, 0);
                    shader.DashedLines(false);
                }
                DrawLines(irregularGrid.Vao, indices_unstr.Length, 0);
                DrawNodes(irregularGrid.Vao, vertices_unstr.Length, 0);
            }
            else
            {
                DrawLines(regularGrid.Vao, indices.Length, 0);
                DrawNodes(regularGrid.Vao, vertices.Length, 0);
            }

            
        }

        // When application exists OS and GPU drives handle cleaning up but closing the GraphicsWindow
        // is not exiting the application because user may want to input area data again
        public void CleanUp()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            regularGrid.Dispose();
            irregularGrid.Dispose();
            area.Dispose();
            shader.Dispose();
        }
    }
}

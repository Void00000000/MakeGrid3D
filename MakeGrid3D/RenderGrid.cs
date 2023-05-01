using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

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


        public void DashedLines(bool dash, float linesSize=0f, float dash_size=0.2f, float gap_size=0.5f)
        {
            if (dash)
            {
                SetFloat("u_dashSize", dash_size);
                SetFloat("u_gapSize", gap_size);
            }
            else
            {
                SetFloat("u_dashSize", linesSize);
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
        public int Vbo { get; }
        public int Ebo { get; }
        public int VLen { get; }
        public int ILen { get; }
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

        public Mesh(float[] vertices, uint[] indices) {
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
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.BindVertexArray(0);
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


    class RenderGrid
    {
        Mesh gridMesh;
        Mesh areaMesh;

        public readonly Shader shader;
        private float[] vertices;
        private uint[] indices;
        private float[] vertices_area;
        private uint[] indices_area;
        private Matrix4 projection;

        public Matrix4 Translate { get; set; } = Matrix4.Identity;
        public Matrix4 Scale { get; set; } = Matrix4.Identity;
        public float Indent { get; set; } = Default.indent;
        public float LinesSize { get; set; } = Default.linesSize;
        public float PointsSize { get; set; } = Default.pointsSize;
        public Color4 LinesColor { get; set; } = Default.linesColor;
        public Color4 PointsColor { get; set; } = Default.pointsColor;
        public bool WireframeMode { get; set; } = Default.wireframeMode;
        public bool ShowGrid { get; set; } = Default.showGrid;
        public bool DrawRemovedLinesMode { get; set; } = Default.drawRemovedLinesMode;

        private Grid2D grid2D;
        public Grid2D Grid2D
        {
            get { return grid2D; }
            set 
            {
                grid2D = value; 
                AssembleVertices(false);
            }
        }
        public float Left { get; private set; }
        public float Right { get; private set; }
        public float Bottom { get; private set; }
        public float Top { get; private set;}

        public float WindowWidth { get; set; }
        public float WindowHeight { get; set; }

        public RenderGrid(Grid2D grid2D, float windowWidth, float windowHeight)
        {
            this.grid2D = grid2D;
            AssembleVertices(true);
            WindowWidth= windowWidth;
            WindowHeight = windowHeight; 
            shader = new Shader("\\Shaders\\shader.vert", "\\Shaders\\shader.frag");
            SetSize();
        }

        public void SetSize()
        {
            float left = grid2D.Area.X0;
            float right = grid2D.Area.Xn;
            float bottom = grid2D.Area.Y0;
            float top = grid2D.Area.Yn;

            float width = right - left;
            float height = top - bottom;

            float hor_offset = width * Indent;
            float ver_offset = height * Indent;

            float left_ = left - hor_offset;
            float right_ = right + hor_offset;
            float bottom_ = bottom - ver_offset;
            float top_ = top + ver_offset;

            float w;
            if ((right_ - left_) >= (top_ - bottom_))
            {
                Left = left_;
                Right = right_;
                w = (WindowHeight / WindowWidth * (Right - Left) - (top - bottom)) / 2;
                Top = top + w;
                Bottom = bottom - w;
            }
            else
            {
                Top = top_;
                Bottom = bottom_;
                w = (WindowWidth / WindowHeight * (Top - Bottom) - (right - left)) / 2;
                Right = right + w;
                Left = left - w;
            }
           projection = Matrix4.CreateOrthographicOffCenter(Left, Right, Bottom, Top, -0.1f, 100.0f);
        }

        private void AssembleVertices(bool area)
        {
            int Nelems = grid2D.Nelems;
            int Nnodes = grid2D.Nnodes;
            int Nareas = grid2D.Area.Nareas;

            vertices = new float[Nnodes * 3];
            int n = 0;
            foreach (Vector2 node in grid2D.XY)
            {
                vertices[n] = node.X; vertices[n + 1] = node.Y; vertices[n + 2] = 0;
                n += 3;
            }

            indices = new uint[Nelems * 4 * 2];
            int e = 0;
            foreach (Elem elem in grid2D.Elems)
            {
                uint n1 = (uint)elem.n1;
                uint n2 = (uint)elem.n2;
                uint n3 = (uint)elem.n3;
                uint n4 = (uint)elem.n4;
                indices[e] = n1; indices[e + 1] = n2; indices[e + 2] = n2; indices[e + 3] = n4;
                indices[e + 4] = n3; indices[e + 5] = n4; indices[e + 6] = n1; indices[e + 7] = n3;
                e += 8;
            }
            gridMesh = new Mesh(vertices, indices);

            if (area)
            {
                vertices_area = new float[grid2D.Area.NXw * grid2D.Area.NYw * 3];
                int yx = 0;
                foreach (float y in grid2D.Area.Yw)
                    foreach (float x in grid2D.Area.Xw)
                    {
                        vertices_area[yx] = x; vertices_area[yx + 1] = y; vertices_area[yx + 2] = 0;
                        yx += 3;
                    }

                indices_area = new uint[Nareas * 6];
                int s = 0;
                foreach (SubArea subArea in grid2D.Area.Mw)
                {
                    int ix1 = subArea.nx1;
                    int iy1 = subArea.ny1;
                    int ix2 = subArea.nx2;
                    int iy2 = subArea.ny2;

                    uint n1 = (uint)(iy1 * grid2D.Area.NXw + ix1);
                    uint n2 = (uint)(iy1 * grid2D.Area.NXw + ix2);
                    uint n3 = (uint)(iy2 * grid2D.Area.NXw + ix1);
                    uint n4 = (uint)(iy2 * grid2D.Area.NXw + ix2);

                    indices_area[s] = n1; indices_area[s + 1] = n2; indices_area[s + 2] = n4;
                    indices_area[s + 3] = n1; indices_area[s + 4] = n3; indices_area[s + 5] = n4;
                    s += 6;
                }
                areaMesh = new Mesh(vertices_area, indices_area);
            }
        }

        private void DrawLines(Mesh mesh) {
            shader.SetColor4("current_color", LinesColor);
            mesh.DrawElems(PrimitiveType.Lines);
        }

        private void DrawNodes(Mesh mesh)
        {
            shader.SetColor4("current_color", PointsColor);
            mesh.DrawVerices(PrimitiveType.Points);
        }

        public void RenderFrame(bool drawArea=true, bool drawNodes=true, bool drawLines=true)
        {
            shader.Use();
            shader.SetMatrix4("projection", ref projection);
            Matrix4 model = Translate * Scale;
            shader.SetMatrix4("model", ref model);
            shader.SetVector2("u_resolution", new Vector2(Right - Left, Top - Bottom));
            GL.LineWidth(LinesSize);
            GL.PointSize(PointsSize);

            if (!WireframeMode && drawArea)
            {
                int s = 0;
                areaMesh.Use();
                foreach (SubArea subArea in grid2D.Area.Mw)
                {
                    shader.SetColor4("current_color", Default.areaColors[subArea.wi]);
                    areaMesh.DrawElems(6, s, PrimitiveType.Triangles);
                    s += 6;
                }
            }
            if (ShowGrid)
            {
                if (drawLines)
                    DrawLines(gridMesh);
                if (drawNodes)
                    DrawNodes(gridMesh);
            }
        }

        // When application exists OS and GPU drives handle cleaning up but closing the GraphicsWindow
        // is not exiting the application because user may want to input area data again
        public void CleanUp()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            gridMesh.Dispose();
            areaMesh.Dispose();
            shader.Dispose();
        }
    }
}

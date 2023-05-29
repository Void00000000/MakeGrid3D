using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Printing;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

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

        public void SetInt(string name, int i)
        {
            Use();
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform1(location, i);
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
        private Matrix4 view;

        public Camera Camera { get; private set; }
        public Matrix4 Translate { get; set; } = Matrix4.Identity;
        public Matrix4 Scale { get; set; } = Matrix4.Identity;
        public Matrix4 Rotate { get; set; } = Matrix4.Identity;
        public float Indent { get; set; } = Default.indent;
        public float LinesSize { get; set; } = Default.linesSize;
        public float PointsSize { get; set; } = Default.pointsSize;
        public Color4 LinesColor { get; set; } = Default.linesColor;
        public Color4 PointsColor { get; set; } = Default.pointsColor;
        public bool WireframeMode { get; set; } = Default.wireframeMode;
        public bool ShowGrid { get; set; } = Default.showGrid;
        public bool DrawRemovedLinesMode { get; set; } = Default.drawRemovedLinesMode;

        private IGrid grid;
        // TODO: Area собирается всегда
        public IGrid Grid
        {
            get { return grid; }
            set 
            {
                grid = value;
                if (grid is Grid2D) AssembleVertices2D(true); else AssembleVertices3D(true);
                SetSize();
            }
        }
        public float Left { get; private set; }
        public float Right { get; private set; }
        public float Bottom { get; private set; }
        public float Top { get; private set;}
        public float Front { get; private set; } = 0;
        public float Back { get; private set; } = 0;

        public float WindowWidth { get; set; }
        public float WindowHeight { get; set; }

        public RenderGrid(IGrid grid, float windowWidth, float windowHeight)
        {
            this.grid = grid;
            WindowWidth= windowWidth;
            WindowHeight = windowHeight;
            shader = new Shader("\\Shaders\\shader.vert", "\\Shaders\\shader.frag");
            if (grid is Grid2D) { AssembleVertices2D(true); Camera = new Camera(); } 
            else AssembleVertices3D(true);
            SetSize();
        }

        private void SetSize2D()
        {
            Grid2D grid2D = (Grid2D)grid;
            // TODO: может не влезать
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

        private void SetSize3D()
        {
            Grid3D grid3D = (Grid3D)grid;
            Left = grid3D.Area.X0;
            Right = grid3D.Area.Xn;
            Bottom = grid3D.Area.Y0;
            Top = grid3D.Area.Yn;
            Front = grid3D.Area.Z0;
            Back = grid3D.Area.Zn;
            Camera = new Camera(new Vector3(0, 0, Back), WindowWidth / WindowHeight);
        }

        public void SetSize()
        {
            if (grid is Grid2D) SetSize2D(); else SetSize3D();
        }

        private void AssembleVertices2D(bool area)
        {
            Grid2D grid2D = (Grid2D)grid;
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
            foreach (Elem2D elem in grid2D.Elems)
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
                foreach (SubArea2D subArea in grid2D.Area.Mw)
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

        private void AssembleVertices3D(bool area)
        {
            Grid3D grid3D = (Grid3D)grid;
            int Nelems = grid3D.Nelems;
            int Nnodes = grid3D.Nnodes;
            int Nareas = grid3D.Area.Nareas;

            vertices = new float[Nnodes * 3];
            int n = 0;
            foreach (Vector3 node in grid3D.XYZ)
            {
                vertices[n] = node.X; vertices[n + 1] = node.Y; vertices[n + 2] = node.Z;
                n += 3;
            }

            indices = new uint[Nelems * 12 * 2];
            int e = 0;
            foreach (Elem3D elem in grid3D.Elems)
            {
                uint n1 = (uint)elem.n1;
                uint n2 = (uint)elem.n2;
                uint n3 = (uint)elem.n3;
                uint n4 = (uint)elem.n4;
                uint n5 = (uint)elem.n5;
                uint n6 = (uint)elem.n6;
                uint n7 = (uint)elem.n7;
                uint n8 = (uint)elem.n8;
                indices[e] = n1; indices[e + 1] = n2; indices[e + 2] = n2; indices[e + 3] = n4;
                indices[e + 4] = n3; indices[e + 5] = n4; indices[e + 6] = n1; indices[e + 7] = n3;
                indices[e + 8] = n1; indices[e + 9] = n5; indices[e + 10] = n3; indices[e + 11] = n7;
                indices[e + 12] = n7; indices[e + 13] = n8; indices[e + 14] = n7; indices[e + 15] = n5;
                indices[e + 16] = n5; indices[e + 17] = n6; indices[e + 18] = n6; indices[e + 19] = n8;
                indices[e + 20] = n2; indices[e + 21] = n6; indices[e + 22] = n4; indices[e + 23] = n8;
                e += 24;
            }
            gridMesh = new Mesh(vertices, indices);

            if (area)
            {
                vertices_area = new float[grid3D.Area.NXw * grid3D.Area.NYw * grid3D.Area.NZw * 3];
                int zyx = 0;
                foreach (float z in grid3D.Area.Zw)
                    foreach (float y in grid3D.Area.Yw)
                        foreach (float x in grid3D.Area.Xw)
                        {
                            vertices_area[zyx] = x; vertices_area[zyx + 1] = y; vertices_area[zyx + 2] = z;
                            zyx += 3;
                        }

                indices_area = new uint[Nareas * 12 * 3];
                int s = 0;
                foreach (SubArea3D subArea in grid3D.Area.Mw)
                {
                    int ix1 = subArea.nx1;
                    int iy1 = subArea.ny1;
                    int iz1 = subArea.nz1;
                    int ix2 = subArea.nx2;
                    int iy2 = subArea.ny2;
                    int iz2 = subArea.nz2;

                    uint n1 = (uint)(iy1 * grid3D.Area.NXw + ix1 + grid3D.Area.NXw * grid3D.Area.NYw * iz1);
                    uint n2 = (uint)(iy1 * grid3D.Area.NXw + ix2 + grid3D.Area.NXw * grid3D.Area.NYw * iz1);
                    uint n3 = (uint)(iy2 * grid3D.Area.NXw + ix1 + grid3D.Area.NXw * grid3D.Area.NYw * iz1);
                    uint n4 = (uint)(iy2 * grid3D.Area.NXw + ix2 + grid3D.Area.NXw * grid3D.Area.NYw * iz1);
                    uint n5 = (uint)(iy1 * grid3D.Area.NXw + ix1 + grid3D.Area.NXw * grid3D.Area.NYw * iz2);
                    uint n6 = (uint)(iy1 * grid3D.Area.NXw + ix2 + grid3D.Area.NXw * grid3D.Area.NYw * iz2);
                    uint n7 = (uint)(iy2 * grid3D.Area.NXw + ix1 + grid3D.Area.NXw * grid3D.Area.NYw * iz2);
                    uint n8 = (uint)(iy2 * grid3D.Area.NXw + ix2 + grid3D.Area.NXw * grid3D.Area.NYw * iz2);
                    
                    // Front face
                    indices_area[s] = n1; indices_area[s + 1] = n3; indices_area[s + 2] = n4;
                    indices_area[s + 3] = n1; indices_area[s + 4] = n4; indices_area[s + 5] = n2;
                    // Right face
                    indices_area[s + 6] = n2; indices_area[s + 7] = n4; indices_area[s + 8] = n8;
                    indices_area[s + 9] = n2; indices_area[s + 10] = n8; indices_area[s + 11] = n6;
                    // Back face
                    indices_area[s + 12] = n6; indices_area[s + 13] = n8; indices_area[s + 14] = n7;
                    indices_area[s + 15] = n6; indices_area[s + 16] = n7; indices_area[s + 17] = n5;
                    // Left face
                    indices_area[s + 18] = n5; indices_area[s + 19] = n7; indices_area[s + 20] = n3;
                    indices_area[s + 21] = n5; indices_area[s + 22] = n3; indices_area[s + 23] = n1;
                    // Top face
                    indices_area[s + 24] = n3; indices_area[s + 25] = n7; indices_area[s + 26] = n8;
                    indices_area[s + 27] = n3; indices_area[s + 28] = n8; indices_area[s + 29] = n4;
                    // Bottom face
                    indices_area[s + 30] = n6; indices_area[s + 31] = n5; indices_area[s + 32] = n1;
                    indices_area[s + 33] = n6; indices_area[s + 34] = n1; indices_area[s + 35] = n2;
                    s += 36;
                }
                areaMesh = new Mesh(vertices_area, indices_area);
            }
        }

        public void DrawLines(Mesh mesh, Color4 color4) {
            shader.SetColor4("current_color", color4);
            mesh.DrawElems(PrimitiveType.Lines);
        }

        public void DrawNodes(Mesh mesh, Color4 color4)
        {
            shader.SetColor4("current_color", color4);
            // 0.5f - это центр квадрата(точки)
            shader.SetFloat("pointRadius", 0.5f);
            shader.SetInt("isPoint", 1);
            mesh.DrawVerices(PrimitiveType.Points);
            shader.SetInt("isPoint", 0);
        }

        private void DrawArea2D()
        {
            Grid2D grid2D = (Grid2D)grid;
            int s = 0;
            foreach (SubArea2D subArea in grid2D.Area.Mw)
            {
                shader.SetColor4("current_color", Default.areaColors[subArea.wi]);
                areaMesh.DrawElems(6, s, PrimitiveType.Triangles);
                s += 6;
            }
        }

        private void DrawArea3D()
        {
            Grid3D grid3D = (Grid3D)grid;
            int s = 0;
            foreach (SubArea3D subArea in grid3D.Area.Mw)
            {
                shader.SetColor4("current_color", Default.areaColors[subArea.wi]);
                areaMesh.DrawElems(12 * 3, s, PrimitiveType.Triangles);
                s += 12 * 3;
            }
        }

        public void RenderFrame(bool drawArea=true, bool drawNodes=true, bool drawLines=true)
        {
            shader.Use();
            if (grid is Grid3D)
                projection = Camera.GetProjectionMatrix();
            shader.SetMatrix4("projection", ref projection);
            float center_x = (Left + Right) / 2;
            float center_y = (Top + Bottom) / 2;
            float center_z = (Front + Back) / 2;
            if (grid is Grid3D)
                Translate = Matrix4.CreateTranslation(-center_x, -center_y, -center_z);
            Matrix4 model = Translate * Scale * Rotate;
            shader.SetMatrix4("model", ref model);
            if (grid is Grid2D)
                view = Matrix4.Identity;
            else
                view = Camera.GetViewMatrix();
            shader.SetMatrix4("view", ref view);
            shader.SetVector2("u_resolution", new Vector2(Right - Left, Top - Bottom));
            GL.LineWidth(LinesSize);
            GL.PointSize(PointsSize);

            if (!WireframeMode && drawArea)
            {
                if (grid is Grid2D) DrawArea2D();
                else DrawArea3D();
            }
            if (ShowGrid)
            {
                if (drawLines)
                    DrawLines(gridMesh, LinesColor);
                if (drawNodes)
                    DrawNodes(gridMesh, PointsColor);
                
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

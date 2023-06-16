using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Printing;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace MakeGrid3D
{
    class RenderGrid
    {
        Mesh gridMesh;
        Mesh areaMesh;
        public List<Mesh> GradientMeshes;
        public List<Mesh> LinesMeshes;

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
        public List<Color4> GridColors { get; set; }
        private float minGradValue;
        private float maxGradValue;
        public Color4 LinesColor { get; set; } = Default.linesColor;
        public Color4 PointsColor { get; set; } = Default.pointsColor;
        public Color4 MinColor { get; set; } = Default.MinColor;
        public Color4 MaxColor { get; set; } = Default.MaxColor;
        public bool WireframeMode { get; set; } = Default.wireframeMode;
        public bool ShowQGradient { get; set; } = false;
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
            //------------------------------------------------
            GradientMeshes = new List<Mesh>(Nelems);
            LinesMeshes = new List<Mesh>(Nelems);
            foreach (Elem2D elem in grid2D.Elems)
            {
                float xmin = grid2D.XY[elem.n1].X; float ymin = grid2D.XY[elem.n1].Y;
                float xmax = grid2D.XY[elem.n4].X; float ymax = grid2D.XY[elem.n4].Y;
                float[] verticesGradElem = { xmin, ymin, 0, 0, 0, // 0
                                          xmax, ymin, 0, 1, 0, // 1
                                          xmin, ymax, 0, 0, 1, // 2
                                          xmax, ymax, 0, 1, 1}; // 3
                float xt, yt;
                if (elem.n5 < 0) { xt = xmin; yt = ymin; } else { xt = grid2D.XY[elem.n5].X; yt = grid2D.XY[elem.n5].Y; }
                float[] verticesElem = {  xmin, ymin, 0, // 0
                                          xmax, ymin, 0, // 1
                                          xmin, ymax, 0, // 2
                                          xmax, ymax, 0, // 3
                                          xt,   yt,   0, };
                uint[] indices_elem = {0, 1, 3, 0, 2, 3};
                uint[] indices_lines = { 0, 1, 1, 3, 2, 3, 0, 2 };
                GradientMeshes.Add(new Mesh(verticesGradElem, indices_elem, true));
                LinesMeshes.Add(new Mesh(verticesElem, indices_lines));
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
            //------------------------------------------------
            GradientMeshes = new List<Mesh>(Nelems * 6);
            LinesMeshes = new List<Mesh>(Nelems);
            foreach (Elem3D elem in grid3D.Elems)
            {
                float left = grid3D.XYZ[elem.n1].X;
                float right = grid3D.XYZ[elem.n8].X;
                float bottom = grid3D.XYZ[elem.n1].Y;
                float top = grid3D.XYZ[elem.n8].Y;
                float front = grid3D.XYZ[elem.n1].Z;
                float back = grid3D.XYZ[elem.n8].Z;

                float[] verticesElem =     {left,  bottom, front, // 0
                                            right, bottom, front, // 1
                                            left,  top,    front, // 2
                                            right, top,    front, // 3
                                            left,  bottom, back,  // 4
                                            right, bottom, back,  // 5
                                            left,  top,    back,  // 6
                                            right, top,    back,  // 7
                                            };
                uint n1 = 0; uint n2 = 1; uint n3 = 2; uint n4 = 3; uint n5 = 4; uint n6 = 5; uint n7 = 6; uint n8 = 7;

                // Для каждой грани
                uint[] indicesFace = { 0, 2, 3, 0, 3, 1 };
                float[] verticesFrontFace = {
                    left,  bottom, front, 0, 0,  // 0|n1
                    right, bottom, front, 0, 1,  // 1|n2
                    left,  top,    front, 1, 0,  // 2|n3
                    right, top,    front, 1, 1}; // 3|n4
                Mesh meshFrontFace = new Mesh(verticesFrontFace, indicesFace, true);
                GradientMeshes.Add(meshFrontFace);

                float[] verticesRightFace = {
                    right,  bottom, front, 0, 0,  // 0|n2
                    right,  bottom, back,  0, 1,  // 1|n6
                    right,  top,   front,  1, 0,  // 2|n4
                    right,  top,   back,   1, 1}; // 3|n8
                Mesh meshRightFace = new Mesh(verticesRightFace, indicesFace, true);
                GradientMeshes.Add(meshRightFace);

                float[] verticesBackFace = {
                    right, bottom, back,  0, 0,  // 0|n6
                    left,  bottom, back,  0, 1,  // 1|n5
                    right, top,    back,  1, 0,  // 2|n8
                    left,  top,    back,  1, 1}; // 3|n7
                Mesh meshBackFace = new Mesh(verticesBackFace, indicesFace, true);
                GradientMeshes.Add(meshBackFace);

                float[] verticesLeftFace = {
                    left, bottom, back,   0, 0,  // 0|n5
                    left, bottom, front,  0, 1,  // 1|n1
                    left, top,    back,   1, 0,  // 2|n7
                    left, top,    front,  1, 1}; // 3|n3
                Mesh meshLeftFace = new Mesh(verticesLeftFace, indicesFace, true);
                GradientMeshes.Add(meshLeftFace);

                float[] verticesTopFace = {
                    left,  top,  front,  0, 0,  // 0|n3
                    right, top,  front,  0, 1,  // 1|n4
                    left,  top,  back,   1, 0,  // 2|n7
                    right, top,  back,   1, 1}; // 3|n8
                Mesh meshTopFace = new Mesh(verticesTopFace, indicesFace, true);
                GradientMeshes.Add(meshTopFace);

                float[] verticesBottomFace = {
                    right, bottom, back,  0, 0,  // 0|n6
                    right, bottom, front, 0, 1,  // 1|n2
                    left,  bottom, back,  1, 0,  // 2|n5
                    left,  bottom, front, 1, 1}; // 3|n1
                Mesh meshBottomFace = new Mesh(verticesBottomFace, indicesFace, true);
                GradientMeshes.Add(meshBottomFace);

                uint[] indices_lines = new uint[24];
                indices_lines[0] = n1; indices_lines[1] = n2; indices_lines[2] = n2; indices_lines[3] = n4;
                indices_lines[4] = n3; indices_lines[5] = n4; indices_lines[6] = n1; indices_lines[7] = n3;
                indices_lines[8] = n1; indices_lines[9] = n5; indices_lines[10] = n3; indices_lines[11] = n7;
                indices_lines[12] = n7; indices_lines[13] = n8; indices_lines[14] = n7; indices_lines[15] = n5;
                indices_lines[16] = n5; indices_lines[17] = n6; indices_lines[18] = n6; indices_lines[19] = n8;
                indices_lines[20] = n2; indices_lines[21] = n6; indices_lines[22] = n4; indices_lines[23] = n8;
                LinesMeshes.Add(new Mesh(verticesElem, indices_lines));
            }
        }

        public void DrawLines(Mesh mesh, Color4 color4) {
            shader.SetColor4("current_color", color4);
            mesh.DrawElems(PrimitiveType.Lines);
        }

        public void DrawNodes(Mesh mesh, Color4 color4)
        {
            shader.SetColor4("current_color", color4);
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
                GL.DepthMask(false);
            }

            // TODO: Для 3D сильные лаги так как много вызовов
            if (WireframeMode && ShowQGradient)
            {
                shader.SetInt("isGradient", 1);
                int ci = 0;
                foreach (Mesh elemMesh in GradientMeshes)
                {
                    shader.SetColor4("color1", GridColors[ci]);
                    shader.SetColor4("color2", GridColors[ci + 1]);
                    shader.SetColor4("color3", GridColors[ci + 2]);
                    shader.SetColor4("color4", GridColors[ci + 3]);
                    ci += 4;
                    elemMesh.DrawElems(6, 0, PrimitiveType.Triangles);
                }
                GL.DepthMask(false);
                shader.SetInt("isGradient", 0);
            }

            if (ShowGrid)
            {
                GL.DepthMask(true);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                if (drawLines)
                    DrawLines(gridMesh, LinesColor);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Point);
                if (drawNodes)
                    DrawNodes(gridMesh, PointsColor);
            }
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.DepthMask(true);
        }

        public void SetGridColors(List<float> q, bool changeMinMaxQ = true)
        {
            if (changeMinMaxQ)
            {
                minGradValue = q.Min();
                maxGradValue = q.Max();
            }
            if (grid is Grid2D)
            {
                Grid2D grid2D = (Grid2D)grid;
                GridColors = new List<Color4>(grid2D.Nelems * 4);
                for (int i = 0; i < grid2D.Nelems; i++)
                {
                    int n1 = grid2D.Elems[i].n1;
                    int n2 = grid2D.Elems[i].n2;
                    int n3 = grid2D.Elems[i].n3;
                    int n4 = grid2D.Elems[i].n4;
                    GridColors.Add(CalcGradientColor(q[n1]));
                    GridColors.Add(CalcGradientColor(q[n2]));
                    GridColors.Add(CalcGradientColor(q[n3]));
                    GridColors.Add(CalcGradientColor(q[n4]));
                }
            }
            else
            {
                Grid3D grid3D = (Grid3D)grid;
                GridColors = new List<Color4>(grid3D.Nelems * 6 * 4);
                for (int i = 0; i < grid3D.Nelems; i++)
                {
                    int n1 = grid3D.Elems[i].n1;
                    int n2 = grid3D.Elems[i].n2;
                    int n3 = grid3D.Elems[i].n3;
                    int n4 = grid3D.Elems[i].n4;
                    int n5 = grid3D.Elems[i].n5;
                    int n6 = grid3D.Elems[i].n6;
                    int n7 = grid3D.Elems[i].n7;
                    int n8 = grid3D.Elems[i].n8;
                    // Front face
                    GridColors.Add(CalcGradientColor(q[n1]));
                    GridColors.Add(CalcGradientColor(q[n2]));
                    GridColors.Add(CalcGradientColor(q[n3]));
                    GridColors.Add(CalcGradientColor(q[n4]));
                    // Right face
                    GridColors.Add(CalcGradientColor(q[n2]));
                    GridColors.Add(CalcGradientColor(q[n6]));
                    GridColors.Add(CalcGradientColor(q[n4]));
                    GridColors.Add(CalcGradientColor(q[n8]));
                    // Back face
                    GridColors.Add(CalcGradientColor(q[n6]));
                    GridColors.Add(CalcGradientColor(q[n5]));
                    GridColors.Add(CalcGradientColor(q[n8]));
                    GridColors.Add(CalcGradientColor(q[n7]));
                    // Left face
                    GridColors.Add(CalcGradientColor(q[n5]));
                    GridColors.Add(CalcGradientColor(q[n1]));
                    GridColors.Add(CalcGradientColor(q[n7]));
                    GridColors.Add(CalcGradientColor(q[n3]));
                    // Top face
                    GridColors.Add(CalcGradientColor(q[n3]));
                    GridColors.Add(CalcGradientColor(q[n4]));
                    GridColors.Add(CalcGradientColor(q[n7]));
                    GridColors.Add(CalcGradientColor(q[n8]));
                    // Bottom face
                    GridColors.Add(CalcGradientColor(q[n6]));
                    GridColors.Add(CalcGradientColor(q[n2]));
                    GridColors.Add(CalcGradientColor(q[n5]));
                    GridColors.Add(CalcGradientColor(q[n1]));
                }
            }
        }

        private Color4 CalcGradientColor(float qi)
        {
            float h = maxGradValue - minGradValue;
            float r = MinColor.R * (maxGradValue - qi) / h + MaxColor.R * (qi - minGradValue) / h;
            float g = MinColor.G * (maxGradValue - qi) / h + MaxColor.G * (qi - minGradValue) / h;
            float b = MinColor.B * (maxGradValue - qi) / h + MaxColor.B * (qi - minGradValue) / h;
            return new Color4(r, g, b, 1);
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

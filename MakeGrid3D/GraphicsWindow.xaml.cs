using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.IO;
using OpenTK.Windowing.Common;
using System.Windows.Controls;

namespace MakeGrid3D
{
    /// <summary>
    /// Interaction logic for GraphicsWindow.xaml
    /// </summary>
    public partial class GraphicsWindow : Window
    {
        RenderGrid renderGrid;
        IrregularGridMaker irregularGridMaker;
        IGrid regularGrid;
        Grid3D? prevGrid3D; // Чтобы после сечения вернутся к трёхмерной сетке
        CrossSections crossSections;
        LinkedList<GridState> gridList;
        LinkedListNode<GridState> currentNode;

        Mesh axis;
        Mesh? selectedElemMesh = null;
        Mesh? selectedElemLines = null;
        Mesh currentNodeMesh;
        Mesh currentPosMesh;

        bool twoD;
        Plane currentPlane;

        Matrix4 projectionSelectedElem = Matrix4.Identity;
        Matrix4 rtranslate = Matrix4.Identity;
        Matrix4 rscale = Matrix4.Identity; // TODO: не используется вообще все матрицы scale
        float horOffset = 0;
        float verOffset = 0;
        float scaleX = 1;
        float scaleY = 1;
        float angleX = 0;
        float angleY = 0;
        float angleZ = 0;
        float mouse_horOffset = 0;
        float mouse_verOffset = 0;
        float mouse_scaleX = 1;
        float mouse_scaleY = 1;
        float speedMove = Default.speedMove;
        float speedZoom = Default.speedZoom;
        float speedRotate = Default.speedRotate;
        float speedHor = 0;
        float speedVer = 0;
        Color4 bgColor = Default.bgColor;
        string fileName = "C:\\Users\\artor\\OneDrive\\Рабочий стол\\ДИПЛОМ ТЕСТЫ\\TEST2.txt";
        int currentElemIndex = -1;
        bool showCurrentUnstructedNode = Default.showCurrentUnstructedNode;
        Color4 currentUnstructedNodeColor = Default.currentUnstructedNodeColor;

        Vector2 lastMousePos;
        bool firstMove = true;
        bool rotateState = false;
        const float sensitivity = 0.2f;

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        public GraphicsWindow()
        {
            InitializeComponent();
            var settings = new GLWpfControlSettings
            {
                MajorVersion = 3,
                MinorVersion = 3
            };
            OpenTkControl.Start(settings);
            AxisOpenTkControl.Start(settings);
            SelectedElemOpenTkControl.Start(settings);
            ResetUI();
        }

        private void SetRenderGrid()
        {
            if (renderGrid == null)
                renderGrid = new RenderGrid(regularGrid, (float)OpenTkControl.ActualWidth, (float)OpenTkControl.ActualHeight);
            else
                renderGrid.Grid = regularGrid;
            irregularGridMaker = new IrregularGridMaker(regularGrid);
            gridList = new LinkedList<GridState>();
            SetCurrentNodeMesh();
            gridList.AddLast(new GridState(regularGrid));
            currentNode = gridList.Last;
            // Множители скорости = 1 процент от ширины(высоты) мира
            speedHor = (renderGrid.Right - renderGrid.Left) * 0.01f;
            speedVer = (renderGrid.Top - renderGrid.Bottom) * 0.01f;
            SetAxis();
        }

        private void InitRegularGrid()
        {
            try
            {
                using (TextReader reader = File.OpenText(fileName))
                {
                    string dim_txt = reader.ReadLine();
                    if (dim_txt == "2D") twoD = true; else twoD = false; 
                }
            }
            catch (Exception e)
            {
                if (e is DirectoryNotFoundException || e is FileNotFoundException)
                {
                    ErrorHandler.FileReadingErrorMessage("Не удалось найти файл с сеткой");
                }
            }
            BlockCurrentMode.Text = twoD ? "Режим: 2D" : "Режим: 3D";
            if (twoD)
            {
                Area2D area = new Area2D(fileName);
                regularGrid = new Grid2D(fileName, area);
                currentPlane = Plane.XY;
                prevGrid3D = null;
            }
            else
            {
                Area3D area = new Area3D(fileName);
                regularGrid = new Grid3D(fileName, area);
                crossSections = new CrossSections((Grid3D)regularGrid);
                prevGrid3D = (Grid3D)regularGrid;
            }
        }

        private void OpenTkControl_OnLoad(object sender, RoutedEventArgs e)
        {
            InitRegularGrid();
            SetRenderGrid();
            // если удалить отсюда то не будет показываться в статус баре
            BlockAreaCount.Text = "Количество подобластей: " + renderGrid.Grid.Nmats;
            BlockNodesCount.Text = "Количество узлов: " + renderGrid.Grid.Nnodes;
            BlockElemsCount.Text = "Количество элементов: " + renderGrid.Grid.Nelems;
            BlockMeanAR.Text = "Среднее соотношение сторон: " + renderGrid.Grid.MeanAR.ToString("0.00");
            BlockWorstAR.Text = "Худшее соотношение сторон: " + renderGrid.Grid.WorstAR.ToString("0.00");
            //----------------------------------------------------------------------------
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.PointSmooth);
            GL.Enable(EnableCap.PolygonOffsetFill);
            GL.PolygonOffset(3.0f, 3.0f);
        }

        private void OpenTkControl_Resize(object sender, SizeChangedEventArgs e)
        {
            if (renderGrid == null)
                return;
            renderGrid.WindowWidth = (float)OpenTkControl.ActualWidth;
            renderGrid.WindowHeight = (float)OpenTkControl.ActualHeight;
            if (twoD)
                renderGrid.SetSize();
            else renderGrid.Camera.AspectRatio = (float)OpenTkControl.ActualWidth / (float)OpenTkControl.ActualHeight;
        }

        private void OpenTkControl_OnRender(TimeSpan obj)
        {
            if (renderGrid == null)
                return;
            BlockAreaCount.Text = "Количество подобластей: " + renderGrid.Grid.Nmats;
            BlockNodesCount.Text = "Количество узлов: " + renderGrid.Grid.Nnodes;
            BlockElemsCount.Text = "Количество элементов: " + renderGrid.Grid.Nelems;
            BlockMeanAR.Text = "Среднее соотношение сторон: " + renderGrid.Grid.MeanAR.ToString("0.00");
            BlockWorstAR.Text = "Худшее соотношение сторон: " + renderGrid.Grid.WorstAR.ToString("0.00");

            //------------------------------
            if (!twoD)
            {
                CameraPositionBlock.Text = "X: " + renderGrid.Camera.Position.X.ToString("0.00") +
                    ", Y: " + renderGrid.Camera.Position.Y.ToString("0.00") +
                    ", Z: " + renderGrid.Camera.Position.Z.ToString("0.00");
                CameraDirectionBlock.Text = "X: " + (renderGrid.Camera.Position + renderGrid.Camera.Front).X.ToString("0.00") +
                    ", Y: " + (renderGrid.Camera.Position + renderGrid.Camera.Front).Y.ToString("0.00") +
                    ", Z: " + (renderGrid.Camera.Position + renderGrid.Camera.Front).Z.ToString("0.00");
            }
            //------------------------------

            if (twoD) GL.Disable(EnableCap.DepthTest); else GL.Enable(EnableCap.DepthTest);

            GL.ClearColor(bgColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            // Чтобы работали прозрачные цвета
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend);

            if (renderGrid.DrawRemovedLinesMode)
            {
                renderGrid.Grid = regularGrid;
                renderGrid.RenderFrame(drawLines:false, drawNodes:false);
                renderGrid.shader.DashedLines(true);
                renderGrid.RenderFrame(drawArea:false, drawNodes:false);
                renderGrid.shader.DashedLines(false, renderGrid.LinesSize);

                renderGrid.Grid = currentNode.ValueRef.Grid;
                renderGrid.RenderFrame(drawArea:false);
            }
            else
            {
                renderGrid.RenderFrame();
            }

            if (crossSections != null)
                crossSections.DrawPlane(renderGrid.shader);

            if (selectedElemMesh != null)
            {
                if (!renderGrid.WireframeMode)
                    renderGrid.shader.SetColor4("current_color", new Color4(86 / 255f, 89 / 255f, 88 / 255f, 0.75f));
                else
                    renderGrid.shader.SetColor4("current_color", Color4.Red);
                int count = twoD ? 6 : 36;
                selectedElemMesh.DrawElems(count, 0, PrimitiveType.Triangles);
            }

            if (showCurrentUnstructedNode)
            {
                renderGrid.DrawNodes(currentPosMesh, Color4.Orange);
                renderGrid.DrawNodes(currentNodeMesh, currentUnstructedNodeColor);
                // первый узел почему то тоже красится...
                // -----------------------------------2D-------------------------------------
                if (twoD)
                {
                    Grid2D grid2D = (Grid2D)renderGrid.Grid;
                    float[] vert = { grid2D.XY[0].X, grid2D.XY[0].Y, 0 };
                    uint[] index = { 0 };
                    Mesh firstNode = new Mesh(vert, index);
                    renderGrid.DrawNodes(firstNode, renderGrid.PointsColor);
                    firstNode.Dispose();
                    //----------------------
                    int node = grid2D.global_num(irregularGridMaker.I, irregularGridMaker.J);
                    if (node >= 0)
                    {
                        float x = grid2D.XY[node].X;
                        float y = grid2D.XY[node].Y;
                        CurrentUnstructedNodeBlock.Text = $"Номер узла: {node} | X: " + x.ToString("0.00") + ", " + y.ToString("0.00");
                    }
                }
                // -----------------------------------3D------------------------------------ -
                else
                {

                }
                switch (irregularGridMaker.DirIndex)
                {
                    case 0:
                        CurrentUnstructedNodeBlock.Text += "| Влево"; break;
                    case 1:
                        CurrentUnstructedNodeBlock.Text += "| Вправо"; break;
                    case 2:
                        CurrentUnstructedNodeBlock.Text += "| Вниз"; break;
                    case 3:
                        CurrentUnstructedNodeBlock.Text += "| Вверх"; break;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (renderGrid != null)
            {
                renderGrid.CleanUp();
                axis.Dispose();
                ResetSelectedElem();
            }
        }

        private void ResetPosition()
        {
            renderGrid.Translate = Matrix4.Identity;
            renderGrid.Scale = Matrix4.Identity;
            renderGrid.Rotate = Matrix4.Identity;
            rtranslate = Matrix4.Identity;
            rscale = Matrix4.Identity;
            horOffset = 0;
            verOffset = 0;
            scaleX = 1;
            scaleY = 1;
            angleX = 0;
            angleY = 0;
            angleZ = 0;
            mouse_horOffset = 0;
            mouse_verOffset = 0;
            mouse_scaleX = 1;
            mouse_scaleY = 1;
            renderGrid.Indent = Default.indent;
            renderGrid.SetSize();
            renderGrid.Camera.Reset();
        }

        private Point MouseMap(Point pos)
        {
            float left = renderGrid.Left;
            float bottom = renderGrid.Bottom;
            float right = renderGrid.Right;
            float top = renderGrid.Top;

            double width = OpenTkControl.ActualWidth;
            double height = OpenTkControl.ActualHeight;

            double x = pos.X * (right - left) / width + left;
            double y = pos.Y * (bottom - top) / height + top;
            Vector4 v = new Vector4((float)x, (float)y, 0, 1);
            Vector4 u = v * rscale * rtranslate;

            return new Point(u.X, u.Y);
        }

        private void OpenTkControl_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(OpenTkControl);
            // ------------------------------------- 2D -------------------------------------
            if (twoD)
            {
                Point new_position = MouseMap(position);
                double x = new_position.X;
                double y = new_position.Y;
                string axis1 = ""; string axis2 = "";
                switch (currentPlane)
                {
                    case Plane.XY: axis1 = "X"; axis2 = "Y"; break;
                    case Plane.XZ: axis1 = "X"; axis2 = "Z"; break;
                    case Plane.YZ: axis1 = "Z"; axis2 = "Y"; break;
                }
                BlockCoordinates.Text = $"{axis1}: {x.ToString("0.00")}, {axis2}: {y.ToString("0.00")}";
            }
            // ------------------------------------- 3D -------------------------------------
            else
            {
                BlockCoordinates.Text = "";
                float x = (float)position.X;
                float y = (float)position.Y;
                if (firstMove)
                {
                    lastMousePos = new Vector2(x, y);
                    firstMove = false;
                }
                else if (rotateState)
                {
                    float deltaX, deltaY;
                    OpenTkControl.CaptureMouse();
                    if (x < 0)
                    {
                        lastMousePos = new Vector2(x, y);
                        x = 0;
                        Point screenPoint = OpenTkControl.PointToScreen(new Point(x, y));
                        SetCursorPos((int)screenPoint.X, (int)screenPoint.Y);
                        deltaX = lastMousePos.X - x;
                        deltaY = y - lastMousePos.Y;
                    }
                    else if (x > OpenTkControl.ActualWidth)
                    {
                        lastMousePos = new Vector2(x, y);
                        x = (float)OpenTkControl.ActualWidth;
                        Point screenPoint = OpenTkControl.PointToScreen(new Point(x, y));
                        SetCursorPos((int)screenPoint.X, (int)screenPoint.Y);
                        deltaX = lastMousePos.X - x;
                        deltaY = y - lastMousePos.Y;
                    }
                    else if (y < 0)
                    {
                        lastMousePos = new Vector2(x, y);
                        y = 0;
                        Point screenPoint = OpenTkControl.PointToScreen(new Point(x, y));
                        SetCursorPos((int)screenPoint.X, (int)screenPoint.Y);
                        deltaX = x - lastMousePos.X;
                        deltaY = lastMousePos.Y - y;
                    }
                    else if (y > OpenTkControl.ActualHeight)
                    {
                        lastMousePos = new Vector2(x, y);
                        y = (float)OpenTkControl.ActualHeight;
                        Point screenPoint = OpenTkControl.PointToScreen(new Point(x, y));
                        SetCursorPos((int)screenPoint.X, (int)screenPoint.Y);
                        deltaX = x - lastMousePos.X;
                        deltaY = lastMousePos.Y - y;
                    }
                    else
                    { 
                        deltaX = x - lastMousePos.X;
                        deltaY = y - lastMousePos.Y;
                        lastMousePos = new Vector2(x, y);
                    }
                    

                    renderGrid.Camera.Yaw += deltaX * sensitivity;
                    renderGrid.Camera.Pitch -= deltaY * sensitivity; // Reversed since y-coordinates range from bottom to top
                }
            }
        }

        private void SelectedElemOpenTkControl_OnRender(TimeSpan obj)
        {
            if (renderGrid == null)
                return;
            GL.ClearColor(new Color4(bgColor.R / 2, bgColor.G / 2, bgColor.B / 2, bgColor.A));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            // Чтобы работали прозрачные цвета
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend);

            if (selectedElemMesh != null && selectedElemLines != null)
            {
                renderGrid.shader.Use();
                renderGrid.shader.SetMatrix4("projection", ref projectionSelectedElem);
                Matrix4 model = Matrix4.Identity;
                renderGrid.shader.SetMatrix4("model", ref model);
                if (!renderGrid.WireframeMode)
                {
                    int count;
                    if (twoD)
                    {
                        Grid2D grid2D = (Grid2D)renderGrid.Grid;
                        Elem2D selectedElem2D = grid2D.Elems[currentElemIndex];
                        renderGrid.shader.SetColor4("current_color", Default.areaColors[selectedElem2D.wi]);
                        count = 6;
                    }
                    else
                    {
                        Grid3D grid3D = (Grid3D)renderGrid.Grid;
                        Elem3D selectedElem3D = grid3D.Elems[currentElemIndex];
                        renderGrid.shader.SetColor4("current_color", Default.areaColors[selectedElem3D.wi]);
                        count = 36;
                    }
                    selectedElemMesh.DrawElems(count, 0, PrimitiveType.Triangles);
                }
                Matrix4 view = Matrix4.Identity;
                renderGrid.shader.SetMatrix4("view", ref view);
                GL.LineWidth(renderGrid.LinesSize);
                renderGrid.DrawLines(selectedElemLines, renderGrid.LinesColor);
                renderGrid.DrawNodes(selectedElemLines, renderGrid.PointsColor);
            }
        }

        private void SetSelectedElemWindowSize()
        {
            // -----------------------------------2D------------------------------------ -
            if (twoD)
            {
                Grid2D grid2D = (Grid2D)renderGrid.Grid;
                Elem2D selectedElem2D = grid2D.Elems[currentElemIndex];
                float left = grid2D.XY[selectedElem2D.n1].X;
                float right = grid2D.XY[selectedElem2D.n4].X;
                float bottom = grid2D.XY[selectedElem2D.n1].Y;
                float top = grid2D.XY[selectedElem2D.n4].Y;

                float width = right - left;
                float height = top - bottom;

                float indent = 0.1f;
                float hor_offset = width * indent;
                float ver_offset = height * indent;

                float left_ = left - hor_offset;
                float right_ = right + hor_offset;
                float bottom_ = bottom - ver_offset;
                float top_ = top + ver_offset;

                float w, left__, right__, bottom__, top__;
                float windowWidth = (float)SelectedElemOpenTkControl.ActualWidth;
                float windowHeight = (float)SelectedElemOpenTkControl.ActualHeight;
                if ((right_ - left_) >= (top_ - bottom_))
                {
                    left__ = left_;
                    right__ = right_;
                    w = (windowHeight / windowWidth * (right__ - left__) - (top - bottom)) / 2;
                    top__ = top + w;
                    bottom__ = bottom - w;
                }
                else
                {
                    top__ = top_;
                    bottom__ = bottom_;
                    w = (windowWidth / windowHeight * (top__ - bottom__) - (right - left)) / 2;
                    right__ = right + w;
                    left__ = left - w;
                }
                projectionSelectedElem = Matrix4.CreateOrthographicOffCenter(left__, right__, bottom__, top__, -0.1f, 100.0f);
            }
            // -----------------------------------3D------------------------------------ -
            else
            {
                
            }
        }

        private void SelectedElemOpenTkControl_Resize(object sender, SizeChangedEventArgs e)
        {
            if (selectedElemMesh != null && selectedElemLines!= null)
                SetSelectedElemWindowSize();
        }

        private void SetSelectedElem2DMeshes()
        {
            SetSelectedElemWindowSize();
            Grid2D grid2D = (Grid2D)renderGrid.Grid;
            Elem2D selectedElem2D = grid2D.Elems[currentElemIndex];
            float left = grid2D.XY[selectedElem2D.n1].X;
            float right = grid2D.XY[selectedElem2D.n4].X;
            float bottom = grid2D.XY[selectedElem2D.n1].Y;
            float top = grid2D.XY[selectedElem2D.n4].Y;
            float xt, yt;
            if (selectedElem2D.n5 >= 0)
            {
                xt = grid2D.XY[selectedElem2D.n5].X;
                yt = grid2D.XY[selectedElem2D.n5].Y;
            }
            else
            {
                xt = left;
                yt = bottom;
            }
            float[] vertices = { left,  bottom, 0,  // 0
                                     right, bottom, 0,  // 1
                                     left,  top,    0,  // 2
                                     right, top,    0,  // 3
                                     xt,    yt,     0};
            uint[] indices = { 0, 1, 3, 0, 2, 3 };
            uint[] indices_lines = { 0, 1, 1, 3, 2, 3, 0, 2 };

            selectedElemMesh = new Mesh(vertices, indices);
            selectedElemLines = new Mesh(selectedElemMesh.Vbo, indices_lines, vertices.Length);

            ElemNumBlock.Text = $"№ элемента: {currentElemIndex} |";
            BlockSubAreaNum.Text = "Номер подобласти: " + (selectedElem2D.wi + 1).ToString();
            BlockNodesNum1.Text = "Л.Н. №: " + selectedElem2D.n1;
            BlockNodesCoords1.Text = "x: " + grid2D.XY[selectedElem2D.n1].X.ToString("0.00")
                                   + " y: " + grid2D.XY[selectedElem2D.n1].Y.ToString("0.00");
            BlockNodesNum2.Text = "П.Н. №: " + selectedElem2D.n2;
            BlockNodesCoords2.Text = "x: " + grid2D.XY[selectedElem2D.n2].X.ToString("0.00")
                                   + " y: " + grid2D.XY[selectedElem2D.n2].Y.ToString("0.00");
            BlockNodesNum3.Text = "Л.В. №: " + selectedElem2D.n3;
            BlockNodesCoords3.Text = "x: " + grid2D.XY[selectedElem2D.n3].X.ToString("0.00")
                                   + " y: " + grid2D.XY[selectedElem2D.n3].Y.ToString("0.00");
            BlockNodesNum4.Text = "П.В. №: " + selectedElem2D.n4;
            BlockNodesCoords4.Text = "x: " + grid2D.XY[selectedElem2D.n4].X.ToString("0.00")
                                   + " y: " + grid2D.XY[selectedElem2D.n4].Y.ToString("0.00");
            if (selectedElem2D.n5 >= 0)
            {
                BlockNodesNum5.Text = "ДОП. №: " + selectedElem2D.n5;
                BlockNodesCoords5.Text = "x: " + grid2D.XY[selectedElem2D.n5].X.ToString("0.00")
                                       + " y: " + grid2D.XY[selectedElem2D.n5].Y.ToString("0.00");
            }
            else
            {
                BlockNodesNum5.Text = "";
                BlockNodesCoords5.Text = "";
            }
        }

        private void SetSelectedElem3DMeshes()
        {
            SetSelectedElemWindowSize();
            Grid3D grid3D = (Grid3D)renderGrid.Grid;
            Elem3D selectedElem3D = grid3D.Elems[currentElemIndex];
            float left = grid3D.XYZ[selectedElem3D.n1].X;
            float right = grid3D.XYZ[selectedElem3D.n8].X;
            float bottom = grid3D.XYZ[selectedElem3D.n1].Y;
            float top = grid3D.XYZ[selectedElem3D.n8].Y;
            float front = grid3D.XYZ[selectedElem3D.n1].Z;
            float back = grid3D.XYZ[selectedElem3D.n8].Z;

            float[] vertices = {     left,  bottom, front,  // 0
                                     right, bottom, front,  // 1
                                     left,  top,    front,  // 2
                                     right, top,    front,  // 3
                                     left,  bottom, back,   // 4
                                     right, bottom, back,   // 5
                                     left,  top,    back,   // 6
                                     right, top,    back,   // 7
                               };
            uint n1 = 0; uint n2 = 1; uint n3 = 2; uint n4 = 3; uint n5 = 4; uint n6 = 5; uint n7 = 6; uint n8 = 7;
            
            uint[] indices = new uint[36];
            // Front face
            indices[0] = n1; indices[1] = n3; indices[2] = n4;
            indices[3] = n1; indices[4] = n4; indices[5] = n2;
            // Right face
            indices[6] = n2; indices[7] = n4; indices[8] = n8;
            indices[9] = n2; indices[10] = n8; indices[11] = n6;
            // Back face
            indices[12] = n6; indices[13] = n8; indices[14] = n7;
            indices[15] = n6; indices[16] = n7; indices[17] = n5;
            // Left face
            indices[18] = n5; indices[19] = n7; indices[20] = n3;
            indices[21] = n5; indices[22] = n3; indices[23] = n1;
            // Top face
            indices[24] = n3; indices[25] = n7; indices[26] = n8;
            indices[27] = n3; indices[28] = n8; indices[29] = n4;
            // Bottom face
            indices[30] = n6; indices[31] = n5; indices[32] = n1;
            indices[33] = n6; indices[34] = n1; indices[35] = n2;

            uint[] indices_lines = new uint[24];
            indices_lines[0] = n1; indices_lines[1] = n2; indices_lines[2] = n2; indices_lines[3] = n4;
            indices_lines[4] = n3; indices_lines[5] = n4; indices_lines[6] = n1; indices_lines[7] = n3;
            indices_lines[8] = n1; indices_lines[9] = n5; indices_lines[10] = n3; indices_lines[11] = n7;
            indices_lines[12] = n7; indices_lines[13] = n8; indices_lines[14] = n7; indices_lines[15] = n5;
            indices_lines[16] = n5; indices_lines[17] = n6; indices_lines[18] = n6; indices_lines[19] = n8;
            indices_lines[20] = n2; indices_lines[21] = n6; indices_lines[22] = n4; indices_lines[23] = n8;

            selectedElemMesh = new Mesh(vertices, indices);
            selectedElemLines = new Mesh(selectedElemMesh.Vbo, indices_lines, vertices.Length);

            ElemNumBlock.Text = $"№ элемента: {currentElemIndex} |";
            BlockSubAreaNum.Text = "Номер подобласти: " + (selectedElem3D.wi + 1).ToString();
            BlockNodesNum1.Text = "Л.Н. №: " + selectedElem3D.n1;
            BlockNodesCoords1.Text = "x: " + grid3D.XYZ[selectedElem3D.n1].X.ToString("0.00")
                                   + " y: " + grid3D.XYZ[selectedElem3D.n1].Y.ToString("0.00");
            BlockNodesNum2.Text = "П.Н. №: " + selectedElem3D.n2;
            BlockNodesCoords2.Text = "x: " + grid3D.XYZ[selectedElem3D.n2].X.ToString("0.00")
                                   + " y: " + grid3D.XYZ[selectedElem3D.n2].Y.ToString("0.00");
            BlockNodesNum3.Text = "Л.В. №: " + selectedElem3D.n3;
            BlockNodesCoords3.Text = "x: " + grid3D.XYZ[selectedElem3D.n3].X.ToString("0.00")
                                   + " y: " + grid3D.XYZ[selectedElem3D.n3].Y.ToString("0.00");
            BlockNodesNum4.Text = "П.В. №: " + selectedElem3D.n4;
            BlockNodesCoords4.Text = "x: " + grid3D.XYZ[selectedElem3D.n4].X.ToString("0.00")
                                   + " y: " + grid3D.XYZ[selectedElem3D.n4].Y.ToString("0.00");
        }

        private void SetSelectedElemMeshes()
        {
            if (twoD) SetSelectedElem2DMeshes(); else SetSelectedElem3DMeshes();
        }


        private void OpenTkControl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (renderGrid == null)
                return;
            var position = e.GetPosition(OpenTkControl);
            Point new_position = MouseMap(position);
            float x = (float)new_position.X;
            float y = (float)new_position.Y;
            if (twoD)
            {
                // ----------------------------------------- 2D -----------------------------------------
                Grid2D grid2D = (Grid2D)renderGrid.Grid;
                if (grid2D.FindElem(x, y, ref currentElemIndex))
                {
                    SetSelectedElemWindowSize();
                    SetSelectedElem2DMeshes();
                }
                else
                    ResetSelectedElem();
            }
            else
            {
                // ----------------------------------------- 3D -----------------------------------------
            }
        }

        private void SetCurrentNodeMesh()
        {
            if (twoD)
            {
                // ----------------------------------------- 2D -----------------------------------------
                Grid2D grid2D = (Grid2D)renderGrid.Grid;
                int node_num_1 = grid2D.global_num(irregularGridMaker.I, irregularGridMaker.J);
                int node_num_2 = grid2D.global_num(irregularGridMaker.NodeI, irregularGridMaker.NodeJ);
                if (node_num_1 < 0 || node_num_2 < 0) { return; }
                float x1 = grid2D.XY[node_num_1].X;
                float y1 = grid2D.XY[node_num_1].Y;
                float[] vertices1 = { x1, y1, 0 };
                uint[] indices1 = { 0 };
                currentPosMesh = new Mesh(vertices1, indices1);

                float x2 = grid2D.XY[node_num_2].X;
                float y2 = grid2D.XY[node_num_2].Y;
                float[] vertices2 = { x2, y2, 0 };
                uint[] indices2 = { 0 };
                currentNodeMesh = new Mesh(vertices2, indices2);
            }
            // ----------------------------------------- 3D -----------------------------------------
            else
            {

            }
            ResetSelectedElem();
        }

        private void OpenTkControl_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (renderGrid == null)
                return;
            var position = e.GetPosition(OpenTkControl);
            Point new_position = MouseMap(position);
            float x = (float)new_position.X;
            float y = (float)new_position.Y;

            if (twoD)
            {
                // ----------------------------------------- 2D -----------------------------------------
                Grid2D grid2D = (Grid2D)renderGrid.Grid;
                int num = 0;
                if (grid2D.FindElem(x, y, ref num))
                {
                    Elem2D selectedElem2D = grid2D.Elems[num];
                    float x1 = grid2D.XY[selectedElem2D.n1].X;
                    float x2 = grid2D.XY[selectedElem2D.n4].X;
                    float y1 = grid2D.XY[selectedElem2D.n1].Y;
                    float y2 = grid2D.XY[selectedElem2D.n4].Y;

                    // TODO: Что за...
                    Tuple<int, float> distance_lb = new Tuple<int, float>(1, MathF.Sqrt(MathF.Pow(x - x1, 2) + MathF.Pow(y - y1, 2)));
                    Tuple<int, float> distance_rb = new Tuple<int, float>(2, MathF.Sqrt(MathF.Pow(x - x2, 2) + MathF.Pow(y - y1, 2)));
                    Tuple<int, float> distance_lu = new Tuple<int, float>(3, MathF.Sqrt(MathF.Pow(x - x1, 2) + MathF.Pow(y - y2, 2)));
                    Tuple<int, float> distance_ru = new Tuple<int, float>(4, MathF.Sqrt(MathF.Pow(x - x2, 2) + MathF.Pow(y - y2, 2)));

                    List<Tuple<int, float>> distances = new List<Tuple<int, float>> { distance_lb, distance_rb, distance_lu, distance_ru };
                    int id = distances.MinBy(t => t.Item2).Item1;
                    int node = 0;
                    int i = 0; int j = 0;
                    switch (id)
                    {
                        case 1:
                            node = selectedElem2D.n1;
                            i = grid2D.global_ij(node).X;
                            j = grid2D.global_ij(node).Y;
                            break;
                        case 2:
                            node = selectedElem2D.n2;
                            i = grid2D.global_ij(node).X;
                            j = grid2D.global_ij(node).Y;
                            break;
                        case 3:
                            node = selectedElem2D.n3;
                            i = grid2D.global_ij(node).X;
                            j = grid2D.global_ij(node).Y;
                            break;
                        case 4:
                            node = selectedElem2D.n4;
                            i = grid2D.global_ij(node).X;
                            j = grid2D.global_ij(node).Y;
                            break;
                    }
                    if (i < grid2D.Nx - 1 && j < grid2D.Ny - 1 && i > 0 && j > 0)
                    {
                        irregularGridMaker.I = i;
                        irregularGridMaker.J = j;
                        irregularGridMaker.MidI = i;
                        irregularGridMaker.MidJ = j;
                        irregularGridMaker.NodeI = i;
                        irregularGridMaker.NodeJ = j;
                        SetCurrentNodeMesh();
                    }
                }
            }
            // ----------------------------------------- 3D -----------------------------------------
            else
            {

            }
        }

        private void AxisOpenTkControl_OnRender(TimeSpan obj)
        {
            if (renderGrid == null)
                return;
            GL.ClearColor(Color4.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            renderGrid.shader.Use();
            GL.LineWidth(2);
            float fov = renderGrid.Camera.Fov;
            if (twoD)
            {
                Matrix4 projection = Matrix4.CreateOrthographicOffCenter(-1f, 1f, -1f, 1f, -0.01f, 100f);
                renderGrid.shader.SetMatrix4("projection", ref projection);
                Matrix4 model = Matrix4.Identity;
                renderGrid.shader.SetMatrix4("model", ref model);
            }
            else
            {
                renderGrid.Camera.Fov /= 1.5f;
                Matrix4 axis_proj3d = renderGrid.Camera.GetProjectionMatrix();
                renderGrid.shader.SetMatrix4("projection", ref axis_proj3d);
                float width = renderGrid.Right - renderGrid.Left;
                float height = renderGrid.Top - renderGrid.Bottom;
                float depth = renderGrid.Back - renderGrid.Front;
                float min, max;
                if (width >= height && width >= depth)
                {
                    min = renderGrid.Left; max = renderGrid.Right;
                }
                else if (height >= width && height >= depth)
                {
                    min = renderGrid.Bottom; max = renderGrid.Top;
                }
                else
                {
                    min = renderGrid.Front; max = renderGrid.Back;
                }
                float center = (min + max) / 2;
                Matrix4 translate = Matrix4.CreateTranslation(-center, -center, -center);
                Matrix4 model = translate * renderGrid.Rotate;
                renderGrid.shader.SetMatrix4("model", ref model);
            }
            Color4 x_color = Color4.Red;
            Color4 y_color = new Color4(78 / 255f, 252 / 255f, 3 / 255f, 1);
            Color4 z_color = Color4.Blue;
            if (twoD)
                switch (currentPlane)
                {
                    case Plane.XZ: y_color = z_color; break;
                    case Plane.YZ: x_color = z_color; break;
                }
            renderGrid.shader.SetColor4("current_color", y_color);
            if (twoD) axis.DrawElems(6, 0, PrimitiveType.Lines); else axis.DrawElems(10, 0, PrimitiveType.Lines);
            renderGrid.shader.SetColor4("current_color", x_color);
            if (twoD) axis.DrawElems(6, 6, PrimitiveType.Lines); else axis.DrawElems(10, 10, PrimitiveType.Lines);
            if (!twoD)
            {
                renderGrid.shader.SetColor4("current_color", Color4.Blue);
                axis.DrawElems(10, 20, PrimitiveType.Lines);

                renderGrid.Camera.Fov = fov;
                Matrix4 axis_proj3d = renderGrid.Camera.GetProjectionMatrix();
                renderGrid.shader.SetMatrix4("projection", ref axis_proj3d);
            }
        }

        private void SetAxis()
        {
            // ----------------------------------- 2D -------------------------------------------
            if (twoD)
            {
                float offset = 2 * 0.05f;
                float[] vertices = {
                                                    // x
                                 0, -1, 0, //0
                                 0, 1, 0, // 1
                                 0 - offset, 1 - offset, 0, // 2
                                 0 + offset, 1 - offset, 0, // 3
                                                    // y
                                 -1, 0, 0, // 4
                                 1, 0, 0,// 5
                                 1 - offset, 0 + offset, 0, // 6
                                 1 - offset, 0 - offset, 0 }; // 7
                uint[] indices = { 0, 1, 1, 2, 1, 3, 4, 5, 5, 6, 5, 7 };
                if (axis != null)
                    axis.Dispose();
                axis = new Mesh(vertices, indices);
            }
            // ----------------------------------- 3D -------------------------------------------
            else
            {
                float width = renderGrid.Right - renderGrid.Left;
                float height = renderGrid.Top - renderGrid.Bottom;
                float depth = renderGrid.Back - renderGrid.Front;
                float min, max;
                if (width >= height && width >= depth)
                {
                    min = renderGrid.Left; max = renderGrid.Right;
                }
                else if (height >= width && height >= depth)
                {
                    min = renderGrid.Bottom; max = renderGrid.Top;
                }
                else
                {
                    min = renderGrid.Front; max = renderGrid.Back;
                }

                float mid = (max + min) / 2f;
                float offset = (max - min) * 0.05f;
                float[] vertices = {
                                                    // y
                                 mid, min, mid, //0
                                 mid, max, mid, // 1
                                 mid - offset, max - offset, mid, // 2
                                 mid + offset, max - offset, mid, // 3
                                 mid, max - offset, mid - offset, // 4
                                 mid, max - offset, mid + offset, // 5
                                                    // x
                                 min, mid, mid, // 6
                                 max, mid, mid,// 7
                                 max - offset, mid + offset, mid, // 8
                                 max - offset, mid - offset, mid, // 9
                                 max - offset, mid, mid + offset, // 10
                                 max - offset, mid, mid - offset, // 11
                                                    // z
                                 mid, mid, min, // 12
                                 mid, mid, max, // 13
                                 mid + offset, mid, max - offset, // 14
                                 mid - offset, mid, max - offset, // 15
                                 mid, mid + offset, max - offset, // 16
                                 mid, mid - offset, max - offset, // 17
                                 };
                uint[] indices = { 0, 1, 1, 2, 1, 3, 1, 4, 1, 5, 6, 7, 7, 8, 7, 9, 7, 10, 7, 11, 12, 13, 13, 14, 13, 15, 13, 16, 13, 17 };
                if (axis != null)
                    axis.Dispose();
                axis = new Mesh(vertices, indices);
            }
        }
        private void OpenFileClick(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".txt"; // Default file extension
            dialog.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                fileName = dialog.FileName;
                ResetSelectedElem();
                ResetPosition();
                ResetUI();
                InitRegularGrid();
                SetRenderGrid();
            }
        }

        private Color ColorFloatToByte(Color4 color4)
        {
            Color color = new Color();
            color.R = (byte)(color4.R * 255);
            color.G = (byte)(color4.G * 255);
            color.B = (byte)(color4.B * 255);
            color.A = (byte)(color4.A * 255);
            return color;
        }
        private Color4 ColorByteToFloat(Color color)
        {
            Color4 color4 = new Color4();
            color4.R = color.R / 255f;
            color4.G = color.G / 255f;
            color4.B = color.B / 255f;
            color4.A = color.A / 255f;
            return color4;
        }
        private void ResetUI()
        {
            LinesSizeSlider.Value = Default.linesSize;
            PointsSizeSlider.Value = Default.pointsSize;
            SpeedMoveSlider.Value = Default.speedMove;
            SpeedZoomSlider.Value = Default.speedZoom;
            SpeedRotateSlider.Value = Default.speedRotate;

            PointsColorPicker.SelectedColor = ColorFloatToByte(Default.pointsColor);
            LinesColorPicker.SelectedColor = ColorFloatToByte(Default.linesColor);
            BgColorPicker.SelectedColor = ColorFloatToByte(Default.bgColor);
            WiremodeCheckBox.IsChecked = Default.wireframeMode;
            ShowGridCheckBox.IsChecked = Default.showGrid;
            SmartMergeCheckBox.IsChecked = Default.smartMerge;

            DrawRemovedLinesCheckBox.IsChecked = Default.drawRemovedLinesMode;
            WidthInput.Text = Default.maxAR_width.ToString();
            HeightInput.Text = Default.maxAR_height.ToString();
            if (irregularGridMaker != null)
            {
                irregularGridMaker.MaxAR = Default.maxAR_width / Default.maxAR_height;
            }

            ShowCurrentUnstructedNodeCheckBox.IsChecked = Default.showCurrentUnstructedNode;
            CurrentUnstructedNodeColorPicker.SelectedColor = ColorFloatToByte(Default.currentUnstructedNodeColor);
        }

        private void ResetSelectedElem()
        {
            if (selectedElemMesh != null)
            {
                selectedElemMesh.Dispose();
                selectedElemMesh = null;
            }
            if (selectedElemLines != null)
            {
                selectedElemLines.Dispose();
                selectedElemLines = null;
            }
            currentElemIndex = -1;
            ElemNumBlock.Text = "№ элемента: |";
            BlockSubAreaNum.Text = "";
            BlockNodesNum1.Text = "";
            BlockNodesCoords1.Text = "";
            BlockNodesNum2.Text = "";
            BlockNodesCoords2.Text = "";
            BlockNodesNum3.Text = "";
            BlockNodesCoords3.Text = "";
            BlockNodesNum4.Text = "";
            BlockNodesCoords4.Text = "";
            BlockNodesNum5.Text = "";
            BlockNodesCoords5.Text = "";
        }

        private void RollLeftClick(object sender, RoutedEventArgs e)
        {
            if (!twoD)
            {
                angleZ += speedRotate;
                Matrix4 rotateX = Matrix4.CreateRotationX(angleX);
                Matrix4 rotateY = Matrix4.CreateRotationY(angleY);
                Matrix4 rotateZ = Matrix4.CreateRotationZ(angleZ);
                renderGrid.Rotate = rotateX * rotateY * rotateZ;
            }
            else MessageBox.Show("Вращение не доступно в 2D режиме");
        }
        private void RollRightClick(object sender, RoutedEventArgs e)
        {
            if (!twoD)
            {
                angleZ -= speedRotate;
                Matrix4 rotateX = Matrix4.CreateRotationX(angleX);
                Matrix4 rotateY = Matrix4.CreateRotationY(angleY);
                Matrix4 rotateZ = Matrix4.CreateRotationZ(angleZ);
                renderGrid.Rotate = rotateX * rotateY * rotateZ;
            }
            else MessageBox.Show("Вращение не доступно в 2D режиме");
        }
        private void YawLeftClick(object sender, RoutedEventArgs e)
        {
            if (!twoD)
            {
                angleY += speedRotate;
                Matrix4 rotateX = Matrix4.CreateRotationX(angleX);
                Matrix4 rotateY = Matrix4.CreateRotationY(angleY);
                Matrix4 rotateZ = Matrix4.CreateRotationZ(angleZ);
                renderGrid.Rotate = rotateX * rotateY * rotateZ;
            }
            else MessageBox.Show("Вращение не доступно в 2D режиме");
        }
        private void YawRightClick(object sender, RoutedEventArgs e)
        {
            if (!twoD)
            {
                angleY -= speedRotate;
                Matrix4 rotateX = Matrix4.CreateRotationX(angleX);
                Matrix4 rotateY = Matrix4.CreateRotationY(angleY);
                Matrix4 rotateZ = Matrix4.CreateRotationZ(angleZ);
                renderGrid.Rotate = rotateX * rotateY * rotateZ;
            }
            else MessageBox.Show("Вращение не доступно в 2D режиме");
        }
        private void PitchBottomClick(object sender, RoutedEventArgs e)
        {
            if (!twoD)
            {
                angleX += speedRotate;
                Matrix4 rotateX = Matrix4.CreateRotationX(angleX);
                Matrix4 rotateY = Matrix4.CreateRotationY(angleY);
                Matrix4 rotateZ = Matrix4.CreateRotationZ(angleZ);
                renderGrid.Rotate = rotateX * rotateY * rotateZ;
            }
            else MessageBox.Show("Вращение не доступно в 2D режиме");
        }
        private void PitchTopClick(object sender, RoutedEventArgs e)
        {
            if (!twoD)
            {
                angleX -= speedRotate;
                Matrix4 rotateX = Matrix4.CreateRotationX(angleX);
                Matrix4 rotateY = Matrix4.CreateRotationY(angleY);
                Matrix4 rotateZ = Matrix4.CreateRotationZ(angleZ);
                renderGrid.Rotate = rotateX * rotateY * rotateZ;
            }
            else MessageBox.Show("Вращение не доступно в 2D режиме");
        }

        private void MoveLeft2D()
        {
            horOffset += speedHor * speedMove;
            renderGrid.Translate = Matrix4.CreateTranslation(horOffset, verOffset, 0);

            mouse_horOffset -= speedHor * speedMove;
            rtranslate = Matrix4.CreateTranslation(mouse_horOffset, mouse_verOffset, 0);
        }

        private void MoveRight2D()
        {
            horOffset -= speedHor * speedMove;
            renderGrid.Translate = Matrix4.CreateTranslation(horOffset, verOffset, 0);

            mouse_horOffset += speedHor * speedMove;
            rtranslate = Matrix4.CreateTranslation(mouse_horOffset, mouse_verOffset, 0);
        }

        private void MoveDown2D()
        {
            verOffset += speedVer * speedMove;
            renderGrid.Translate = Matrix4.CreateTranslation(horOffset, verOffset, 0);

            mouse_verOffset -= speedVer * speedMove;
            rtranslate = Matrix4.CreateTranslation(mouse_horOffset, mouse_verOffset, 0);
        }

        private void MoveUp2D()
        {
            verOffset -= speedVer * speedMove;
            renderGrid.Translate = Matrix4.CreateTranslation(horOffset, verOffset, 0);

            mouse_verOffset += speedVer * speedMove;
            rtranslate = Matrix4.CreateTranslation(mouse_horOffset, mouse_verOffset, 0);
        }

        private void ZoomIn2D(float delta = 1f)
        {
            if (renderGrid.Indent >= -0.5f)
            {
                renderGrid.Indent -= delta * speedZoom;
                renderGrid.SetSize();
            }

            //MessageBox.Show(BufferClass.indent.ToString());
            //BufferClass.scaleX *= BufferClass.speedZoom;
            //BufferClass.scaleY *= BufferClass.speedZoom;
            //BufferClass.scale = Matrix4.CreateScale(BufferClass.scaleX, BufferClass.scaleY, 1);

            //BufferClass.mouse_scaleX /= BufferClass.speedZoom;
            //BufferClass.mouse_scaleY /= BufferClass.speedZoom;
        }

        private void ZoomOut2D(float delta = 1f)
        {
            renderGrid.Indent += speedZoom;
            renderGrid.SetSize();

            //BufferClass.scaleX /= BufferClass.speedZoom;
            //BufferClass.scaleY /= BufferClass.speedZoom;
            //BufferClass.scale = Matrix4.CreateScale(BufferClass.scaleX, BufferClass.scaleY, 1);

            //BufferClass.mouse_scaleX *= BufferClass.speedZoom;
            //BufferClass.mouse_scaleY *= BufferClass.speedZoom;
        }

        private void MoveLeftClick(object sender, RoutedEventArgs e)
        {
            if (twoD) MoveLeft2D(); else renderGrid.Camera.MoveLeft();
        }

        private void MoveRightClick(object sender, RoutedEventArgs e)
        {
            if (twoD) MoveRight2D(); else renderGrid.Camera.MoveRight();
        }

        private void MoveDownClick(object sender, RoutedEventArgs e)
        {
            if (twoD) MoveDown2D(); else renderGrid.Camera.MoveDown();
        }

        private void MoveUpClick(object sender, RoutedEventArgs e)
        {
            if (twoD) MoveUp2D(); else renderGrid.Camera.MoveUp();
        }

        private void ZoomInClick(object sender, RoutedEventArgs e)
        {
            if (twoD) ZoomIn2D(); else renderGrid.Camera.Zoom(speedZoom * 100);
        }

        private void ZoomOutClick(object sender, RoutedEventArgs e)
        {
            if (twoD) ZoomOut2D(); else renderGrid.Camera.Zoom(-speedZoom * 100);
        }

        private void ResetPositionClick(object sender, RoutedEventArgs e)
        {
            ResetPosition();
        }

        private void LinesSizeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (renderGrid == null) return;
            renderGrid.LinesSize = (float)e.NewValue;
        }

        private void PointsSizeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (renderGrid == null) return;
            renderGrid.PointsSize = (float)e.NewValue;
        }

        private void PointsColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (renderGrid == null) return;
            renderGrid.PointsColor = ColorByteToFloat((Color)e.NewValue);
        }

        private void LinesColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (renderGrid == null) return;
            renderGrid.LinesColor = ColorByteToFloat((Color)e.NewValue);
        }

        private void BgColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            bgColor = ColorByteToFloat((Color)e.NewValue);
        }

        private void SpeedMoveChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            speedMove = (float)e.NewValue;
            if (!twoD && renderGrid != null) renderGrid.Camera.Speed = speedMove;
        }

        private void SpeedZoomChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            speedZoom = (float)e.NewValue;
        }

        private void SpeedRotateChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            speedRotate = MathHelper.DegreesToRadians((float)e.NewValue);
        }

        private void ResetSettingsClick(object sender, RoutedEventArgs e)
        {
            ResetUI();
        }

        private void WiremodeChecked(object sender, RoutedEventArgs e)
        {
            if (renderGrid == null) return;
            renderGrid.WireframeMode = true;
        }

        private void WiremodeUnChecked(object sender, RoutedEventArgs e)
        {
            if (renderGrid == null) return;
            renderGrid.WireframeMode = false;
        }

        private void DrawRemovedLinesChecked(object sender, RoutedEventArgs e)
        {
            if (renderGrid == null) return;
            renderGrid.DrawRemovedLinesMode = true;
        }

        private void DrawRemovedLinesUnChecked(object sender, RoutedEventArgs e)
        {
            if (renderGrid == null) return;
            renderGrid.DrawRemovedLinesMode = false;
        }

        private void MaxARClick(object sender, RoutedEventArgs e)
        {
            try
            {
                int w = int.Parse(WidthInput.Text);
                int h = int.Parse(HeightInput.Text);
                if (w <= 0 || h <= 0)
                {
                    throw new BelowZeroException("Числа в полях меньше или равны нулю");
                }
                float maxAr = (float)w / h;
                if (maxAr < 1f) maxAr = 1f / maxAr;
                irregularGridMaker.MaxAR = maxAr;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentNullException || ex is FormatException)
                {
                    ErrorHandler.DataErrorMessage("Данные в полях должны быть целыми положительными числами", false);
                }
                else if (ex is BelowZeroException)
                {
                    ErrorHandler.DataErrorMessage(ex.Message, false);
                }
                else
                {
                    ErrorHandler.DataErrorMessage("Не удалось прочитать данные из полей", false);
                }
                WidthInput.Text = "";
                HeightInput.Text = "";
            }
        }

        private void ShowGridChecked(object sender, RoutedEventArgs e)
        {
            if (renderGrid == null) return;
            renderGrid.ShowGrid = true;
        }

        private void UndoClick(object sender, RoutedEventArgs e)
        {
            if (currentNode != null && currentNode.Previous != null)
            {
                currentNode = currentNode.Previous;
                renderGrid.Grid = currentNode.ValueRef.Grid;
                irregularGridMaker.Set(currentNode.ValueRef);
                SetCurrentNodeMesh();
            }
        }

        private void DoClick(object sender, RoutedEventArgs e)
        {
            if (currentNode != null && currentNode.Next != null)
            {
                currentNode = currentNode.Next;
                renderGrid.Grid = currentNode.ValueRef.Grid;
                irregularGridMaker.Set(currentNode.ValueRef);
                SetCurrentNodeMesh();
            }
        }

        private void RemoveElemsClick(object sender, RoutedEventArgs e)
        {
            if (!irregularGridMaker.End)
            {
                irregularGridMaker.AllSteps = false;
                IGrid grid_new = irregularGridMaker.MakeUnStructedGrid();
                while (currentNode != null && currentNode.Next != null)
                    gridList.Remove(currentNode.Next);
                SetCurrentNodeMesh();
                gridList.AddLast(new GridState(grid_new, irregularGridMaker));
                currentNode = gridList.Last;
                renderGrid.Grid = grid_new;
                irregularGridMaker.Grid = grid_new;
            }
        }

        private void RemoveAllElemsClick(object sender, RoutedEventArgs e)
        {
            if (!irregularGridMaker.End)
            {
                irregularGridMaker.AllSteps = true;
                IGrid grid_new = irregularGridMaker.MakeUnStructedGrid();
                irregularGridMaker.Grid = grid_new;
                while (currentNode != null && currentNode.Next != null)
                    gridList.Remove(currentNode.Next);
                SetCurrentNodeMesh();
                gridList.AddLast(new GridState(grid_new, irregularGridMaker));
                currentNode = gridList.Last;
                renderGrid.Grid = grid_new;
            }
        }

        private void ShowGridUnChecked(object sender, RoutedEventArgs e)
        {
            if (renderGrid == null) return;
            renderGrid.ShowGrid = false;
        }

        private void MakeRegularGridClick(object sender, RoutedEventArgs e)
        {
            if (renderGrid == null) return;
            renderGrid.Grid = regularGrid;
            irregularGridMaker.Grid = regularGrid;
            irregularGridMaker.Reset();
            gridList.Clear();
            SetCurrentNodeMesh();
            gridList.AddLast(new GridState(regularGrid));
            currentNode = gridList.Last;
        }

        private void SmartMergeChecked(object sender, RoutedEventArgs e)
        {
            irregularGridMaker.SmartMerge = true;
        }

        private void SmartMergeUnChecked(object sender, RoutedEventArgs e)
        {
            irregularGridMaker.SmartMerge = false;
        }

        private void ShowCurrentUnstructedNodeChecked(object sender, RoutedEventArgs e)
        {
            showCurrentUnstructedNode = true;
        }

        private void ShowCurrentUnstructedNodeUnChecked(object sender, RoutedEventArgs e)
        {
            showCurrentUnstructedNode = false;
        }

        private void CurrentUnstructedNodeColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            currentUnstructedNodeColor = ColorByteToFloat((Color)e.NewValue);
        }

        private void LeftDirClick(object sender, RoutedEventArgs e)
        {
            irregularGridMaker.DirIndex = 0;
        }

        private void RightDirClick(object sender, RoutedEventArgs e)
        {
            irregularGridMaker.DirIndex = 1;
        }

        private void BottomDirClick(object sender, RoutedEventArgs e)
        {
            irregularGridMaker.DirIndex = 2;
        }

        private void TopDirClick(object sender, RoutedEventArgs e)
        {
            irregularGridMaker.DirIndex = 3;
        }
        
        private void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab && !twoD)
            {
                if (rotateState)
                {
                    rotateState = false;
                    firstMove = true;
                    OpenTkControl.ReleaseMouseCapture();
                }
                else rotateState = true;
            }
            else if (e.Key == Key.W)
                if (!twoD) renderGrid.Camera.MoveForward(); else MoveUp2D();
            else if (e.Key == Key.S)
                if (!twoD) renderGrid.Camera.MoveBackwards(); else MoveDown2D();
            else if (e.Key == Key.A)
                if (!twoD) renderGrid.Camera.MoveLeft(); else MoveLeft2D();
            else if (e.Key == Key.D)
                if (!twoD) renderGrid.Camera.MoveRight(); else MoveRight2D();
            else if (e.Key == Key.LeftCtrl && !twoD)
                renderGrid.Camera.MoveDown();
            else if (e.Key == Key.LeftShift && !twoD)
                renderGrid.Camera.MoveUp();
        }

        private void OpenTkControl_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (twoD)
            {
                if ((float)e.Delta > 0) ZoomIn2D((float)e.Delta / 100);
                else ZoomOut2D(-(float)e.Delta / 100);
            }
            else renderGrid.Camera.Zoom(speedZoom * e.Delta);
        }

        private void ReplicateCrossSectionsClick(object sender, RoutedEventArgs e)
        {
            if (twoD)
            {
                twoD = false;
                BlockCurrentMode.Text = "Режим: 3D";
                List<float> z = new List<float>{ 1f, 2f, 3f };
                regularGrid = new Grid3D((Grid2D)renderGrid.Grid, z);
                crossSections = new CrossSections((Grid3D)regularGrid);
                prevGrid3D = (Grid3D)regularGrid;
                SetRenderGrid();
                ResetPosition();
                ResetSelectedElem();
            }
            else MessageBox.Show("Тиражирование сечения не доступно в режиме 3D");
        }

        private void PlaneSectionChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (crossSections == null) return;
            crossSections.CurrentPlaneSec = (int)e.NewValue;
        }

        private void PlaneChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!twoD && crossSections != null)
            {
                ComboBox comboBox = (ComboBox)sender;
                string selectedItem = comboBox.SelectedItem.ToString();
                string plane_str = selectedItem.Substring(selectedItem.Length - 2);
                switch (plane_str)
                {
                    case "XY":
                        crossSections.CurrentPlane = Plane.XY;
                        CurrentPlaneBlock.Text = "Z = ";
                        break;
                    case "XZ":
                        crossSections.CurrentPlane = Plane.XZ;
                        CurrentPlaneBlock.Text = "Y = ";
                        break;
                    case "YZ":
                        crossSections.CurrentPlane = Plane.YZ;
                        CurrentPlaneBlock.Text = "X = ";
                        break;
                }
            }
            else MessageBox.Show("Выбор плоскости сечения не доступно в режиме 2D");
        }

        private void PrevPlaneCLick(object sender, RoutedEventArgs e)
        {
            if (!twoD && crossSections != null)
            {
                crossSections.CurrentPlaneSec--;
                CurrentPlaneSecBlock.Text = crossSections.CurrentValue.ToString("0.00");
            }
            else MessageBox.Show("Выбор плоскости сечения не доступно в режиме 2D");
        }

        private void NextPlaneCLick(object sender, RoutedEventArgs e)
        {
            if (!twoD && crossSections != null)
            {
                crossSections.CurrentPlaneSec++;
                CurrentPlaneSecBlock.Text = crossSections.CurrentValue.ToString("0.00");
            }
            else MessageBox.Show("Выбор плоскости сечения не доступно в режиме 2D");
        }

        // TODO: Для некоторых сечений не рисуются рёбра
        private void DrawCrossSectionClick(object sender, RoutedEventArgs e)
        {
            if (twoD)
            {
                MessageBox.Show("Не доступно в режиме 2D");
            }
            else if (crossSections == null || !crossSections.Active)
            {
                MessageBox.Show("Не удалось построить сечение");
            }
            else
            {
                Grid3D grid3D = (Grid3D)renderGrid.Grid;
                Plane plane = crossSections.CurrentPlane;
                int index = crossSections.CurrentPlaneSec;
                float value = crossSections.CurrentValue;
                regularGrid = new Grid2D(grid3D, plane, index, value);
                twoD = true;
                BlockCurrentMode.Text = twoD ? "Режим: 2D" : "Режим: 3D";
                SetRenderGrid();
                ResetPosition();
                ResetSelectedElem();
                crossSections.Active = false;
                currentPlane = plane;
            }
        }

        private void Return3DClick(object sender, RoutedEventArgs e)
        {
            if (!twoD)
            {
                MessageBox.Show("Не доступно в режиме 3D");
            }
            else
            {
                if (prevGrid3D != null)
                {
                    twoD = false;
                    BlockCurrentMode.Text = "Режим: 3D";
                    regularGrid = prevGrid3D;
                    renderGrid.Grid = regularGrid;
                    crossSections.Active = true;
                    SetRenderGrid();
                    ResetPosition();
                    ResetSelectedElem();
                }
            }
        }

        private void PrevElemCLick(object sender, RoutedEventArgs e)
        {
            int step;
            if (int.TryParse(StepSelectorInput.Text, out step) && step > 0)
            {
                currentElemIndex -= step;
                if (currentElemIndex < 0)
                    currentElemIndex = 0;
                SetSelectedElemMeshes();
            }
            else ErrorHandler.DataErrorMessage("Некорректный шаг", false);
        }

        private void NextElemCLick(object sender, RoutedEventArgs e)
        {
            int step;
            if (int.TryParse(StepSelectorInput.Text, out step) && step > 0)
            {
                currentElemIndex += step;
                if (currentElemIndex >= renderGrid.Grid.Nelems)
                    currentElemIndex = renderGrid.Grid.Nelems - 1;
                SetSelectedElemMeshes();
            }
            else ErrorHandler.DataErrorMessage("Некорректный шаг", false);
        }

        private void ResetElemClick(object sender, RoutedEventArgs e)
        {
            currentElemIndex = 0;
            ResetSelectedElem();
        }
    }
}
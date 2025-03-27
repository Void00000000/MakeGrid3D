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
using Xceed.Wpf.AvalonDock.Themes;
using System.Xml.Serialization;
using MakeGrid3D.FEM;

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
        GridParams gridParams;

        Mesh axis;
        bool isElemSelected = false;
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
        bool gridFromFile;
        int currentElemIndex = -1;
        bool showCurrentUnstructedNode = Default.showCurrentUnstructedNode;
        Color4 currentUnstructedNodeColor = Default.currentUnstructedNodeColor;

        bool isQFileLoaded = false;
        List<float> q;
        Fem fem;

        Vector2 lastMousePos;
        bool firstMove = true;
        bool rotateState = false;
        const float sensitivity = 0.2f;

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        public GraphicsWindow(GridParams gridParams)
        {
            InitializeComponent();
            var settings = new GLWpfControlSettings
            {
                MajorVersion = 3,
                MinorVersion = 3
            };
            this.gridParams = gridParams;
            gridFromFile = false;
            OpenTkControl.Start(settings);
            AxisOpenTkControl.Start(settings);
            SelectedElemOpenTkControl.Start(settings);
            FillComboBoxes();
            ResetUI();
        }

        public GraphicsWindow(string fileName)
        {
            InitializeComponent();
            var settings = new GLWpfControlSettings
            {
                MajorVersion = 3,
                MinorVersion = 3
            };
            this.fileName = fileName;
            gridFromFile = true;
            OpenTkControl.Start(settings);
            AxisOpenTkControl.Start(settings);
            SelectedElemOpenTkControl.Start(settings);
            FillComboBoxes();
            ResetUI();
        }

        private void SetRenderGrid(bool removeQ = true)
        {
            if (renderGrid == null)
                renderGrid = new RenderGrid(regularGrid, (float)OpenTkControl.ActualWidth, (float)OpenTkControl.ActualHeight);
            else
            {
                renderGrid.Grid = regularGrid;
                renderGrid.AreaColors = new List<Color4>(Default.areaColors);
            }
            irregularGridMaker = new IrregularGridMaker(regularGrid);
            ResetComboBoxes();
            gridList = new LinkedList<GridState>();
            SetCurrentNodeMesh(removeQ);
            gridList.AddLast(new GridState(regularGrid));
            currentNode = gridList.Last;
            // Множители скорости = 1 процент от ширины(высоты) мира
            speedHor = (renderGrid.Right - renderGrid.Left) * 0.01f;
            speedVer = (renderGrid.Top - renderGrid.Bottom) * 0.01f;
            SetAxis();
            SubAreaNumDownMenu.Items.Clear();
            for (int i = 0; i < renderGrid.Grid.Nmats; i++)
            {
                SubAreaNumDownMenu.Items.Add($"{i + 1}");
            }
            if (removeQ)
                RemoveQFile();
        }

        private void InitRegularGrid()
        {
            if (gridFromFile)
                using (TextReader reader = File.OpenText(fileName))
                {
                    string dim_txt = reader.ReadLine();
                    if (dim_txt == "2D") twoD = true; else twoD = false;
                }
            else twoD = gridParams.TwoD;
            BlockCurrentMode.Text = twoD ? "Режим: 2D" : "Режим: 3D";
            if (twoD)
            {
                if (gridFromFile) regularGrid = new Grid2D(fileName); else regularGrid = new Grid2D(gridParams);
                currentPlane = Plane.XY;
                prevGrid3D = null;
            }
            else
            {
                if (gridFromFile) regularGrid = new Grid3D(fileName); else regularGrid = new Grid3D(gridParams);
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

            if (isElemSelected)
            {
                if (twoD)
                {
                    if (!renderGrid.WireframeMode)
                        renderGrid.shader.SetColor4("current_color", new Color4(86 / 255f, 89 / 255f, 88 / 255f, 0.75f));
                    else if (renderGrid.WireframeMode && isQFileLoaded)
                    {
                        renderGrid.shader.SetInt("isGradient", 1);
                        renderGrid.shader.SetColor4("color1", renderGrid.GridColors[currentElemIndex * 4]);
                        renderGrid.shader.SetColor4("color2", renderGrid.GridColors[currentElemIndex * 4 + 1]);
                        renderGrid.shader.SetColor4("color3", renderGrid.GridColors[currentElemIndex * 4 + 2]);
                        renderGrid.shader.SetColor4("color4", renderGrid.GridColors[currentElemIndex * 4 + 3]);
                        renderGrid.shader.SetInt("isGradient", 0);
                    }
                    else
                        renderGrid.shader.SetColor4("current_color", Color4.Red);
                    if (renderGrid.WireframeMode && isQFileLoaded) renderGrid.shader.SetInt("isGradient", 1);
                    renderGrid.GradientMeshes[currentElemIndex].DrawElems(6, 0, PrimitiveType.Triangles);
                    renderGrid.shader.SetInt("isGradient", 0);
                }
                // ------------------------------------ 3D --------------------------------------------
                else
                {
                    if (renderGrid.WireframeMode && isQFileLoaded) renderGrid.shader.SetInt("isGradient", 1);
                    // 1 face
                    if (renderGrid.WireframeMode && isQFileLoaded)
                    {
                        renderGrid.shader.SetColor4("color1", renderGrid.GridColors[currentElemIndex * 24]);
                        renderGrid.shader.SetColor4("color2", renderGrid.GridColors[currentElemIndex * 24 + 1]);
                        renderGrid.shader.SetColor4("color3", renderGrid.GridColors[currentElemIndex * 24 + 2]);
                        renderGrid.shader.SetColor4("color4", renderGrid.GridColors[currentElemIndex * 24 + 3]);
                    } else if (!renderGrid.WireframeMode) renderGrid.shader.SetColor4("current_color", new Color4(86 / 255f, 89 / 255f, 88 / 255f, 0.75f));
                    else renderGrid.shader.SetColor4("current_color", Color4.Red);
                    renderGrid.GradientMeshes[currentElemIndex * 6].DrawElems(6, 0, PrimitiveType.Triangles);
                    // 2 face
                    if (renderGrid.WireframeMode && isQFileLoaded)
                    {
                        renderGrid.shader.SetColor4("color1", renderGrid.GridColors[currentElemIndex * 24 + 4]);
                        renderGrid.shader.SetColor4("color2", renderGrid.GridColors[currentElemIndex * 24 + 5]);
                        renderGrid.shader.SetColor4("color3", renderGrid.GridColors[currentElemIndex * 24 + 6]);
                        renderGrid.shader.SetColor4("color4", renderGrid.GridColors[currentElemIndex * 24 + 7]);
                    }
                    else if (!renderGrid.WireframeMode) renderGrid.shader.SetColor4("current_color", new Color4(86 / 255f, 89 / 255f, 88 / 255f, 0.75f));
                    else renderGrid.shader.SetColor4("current_color", Color4.Red);
                    renderGrid.GradientMeshes[currentElemIndex * 6 + 1].DrawElems(6, 0, PrimitiveType.Triangles);
                    // 3 face
                    if (renderGrid.WireframeMode && isQFileLoaded)
                    {
                        renderGrid.shader.SetColor4("color1", renderGrid.GridColors[currentElemIndex * 24 + 8]);
                        renderGrid.shader.SetColor4("color2", renderGrid.GridColors[currentElemIndex * 24 + 9]);
                        renderGrid.shader.SetColor4("color3", renderGrid.GridColors[currentElemIndex * 24 + 10]);
                        renderGrid.shader.SetColor4("color4", renderGrid.GridColors[currentElemIndex * 24 + 11]);
                    }
                    else if (!renderGrid.WireframeMode) renderGrid.shader.SetColor4("current_color", new Color4(86 / 255f, 89 / 255f, 88 / 255f, 0.75f));
                    else renderGrid.shader.SetColor4("current_color", Color4.Red);
                    renderGrid.GradientMeshes[currentElemIndex * 6 + 2].DrawElems(6, 0, PrimitiveType.Triangles);
                    // 4 face
                    if (renderGrid.WireframeMode && isQFileLoaded)
                    {
                        renderGrid.shader.SetColor4("color1", renderGrid.GridColors[currentElemIndex * 24 + 12]);
                        renderGrid.shader.SetColor4("color2", renderGrid.GridColors[currentElemIndex * 24 + 13]);
                        renderGrid.shader.SetColor4("color3", renderGrid.GridColors[currentElemIndex * 24 + 14]);
                        renderGrid.shader.SetColor4("color4", renderGrid.GridColors[currentElemIndex * 24 + 15]);
                    }
                    else if (!renderGrid.WireframeMode) renderGrid.shader.SetColor4("current_color", new Color4(86 / 255f, 89 / 255f, 88 / 255f, 0.75f));
                    else renderGrid.shader.SetColor4("current_color", Color4.Red);
                    renderGrid.GradientMeshes[currentElemIndex * 6 + 3].DrawElems(6, 0, PrimitiveType.Triangles);
                    // 5 face
                    if (renderGrid.WireframeMode && isQFileLoaded)
                    {
                        renderGrid.shader.SetColor4("color1", renderGrid.GridColors[currentElemIndex * 24 + 16]);
                        renderGrid.shader.SetColor4("color2", renderGrid.GridColors[currentElemIndex * 24 + 17]);
                        renderGrid.shader.SetColor4("color3", renderGrid.GridColors[currentElemIndex * 24 + 18]);
                        renderGrid.shader.SetColor4("color4", renderGrid.GridColors[currentElemIndex * 24 + 19]);
                    }
                    else if (!renderGrid.WireframeMode) renderGrid.shader.SetColor4("current_color", new Color4(86 / 255f, 89 / 255f, 88 / 255f, 0.75f));
                    else renderGrid.shader.SetColor4("current_color", Color4.Red);
                    renderGrid.GradientMeshes[currentElemIndex * 6 + 4].DrawElems(6, 0, PrimitiveType.Triangles);
                    // 6 face
                    if (renderGrid.WireframeMode && isQFileLoaded)
                    {
                        renderGrid.shader.SetColor4("color1", renderGrid.GridColors[currentElemIndex * 24 + 20]);
                        renderGrid.shader.SetColor4("color2", renderGrid.GridColors[currentElemIndex * 24 + 21]);
                        renderGrid.shader.SetColor4("color3", renderGrid.GridColors[currentElemIndex * 24 + 22]);
                        renderGrid.shader.SetColor4("color4", renderGrid.GridColors[currentElemIndex * 24 + 23]);
                    }
                    else if (!renderGrid.WireframeMode) renderGrid.shader.SetColor4("current_color", new Color4(86 / 255f, 89 / 255f, 88 / 255f, 0.75f));
                    else renderGrid.shader.SetColor4("current_color", Color4.Red);
                    renderGrid.GradientMeshes[currentElemIndex * 6 + 5].DrawElems(6, 0, PrimitiveType.Triangles);
                    renderGrid.shader.SetInt("isGradient", 0);
                }
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
                        CurrentUnstructedNodeBlock1.Text = $"Номер узла: {node} | X: " + x.ToString("0.00") + ", Y: " + y.ToString("0.00");
                    }
                }
                // -----------------------------------3D------------------------------------ -
                else
                {
                }
                CurrentUnstructedNodeBlock2.Text = irregularGridMaker.PrintInfo();
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

            if (isElemSelected)
            {
                renderGrid.shader.Use();
                renderGrid.shader.SetMatrix4("projection", ref projectionSelectedElem);
                Matrix4 model = Matrix4.Identity;
                renderGrid.shader.SetMatrix4("model", ref model);
                if (!renderGrid.WireframeMode)
                {
                    if (twoD)
                    {
                        Grid2D grid2D = (Grid2D)renderGrid.Grid;
                        Elem2D selectedElem2D = grid2D.Elems[currentElemIndex];
                        renderGrid.shader.SetColor4("current_color", renderGrid.AreaColors[selectedElem2D.wi]);
                        renderGrid.GradientMeshes[currentElemIndex].DrawElems(6, 0, PrimitiveType.Triangles);
                    }
                    else
                    {
                        //Grid3D grid3D = (Grid3D)renderGrid.Grid;
                        //Elem3D selectedElem3D = grid3D.Elems[currentElemIndex];
                        //renderGrid.shader.SetColor4("current_color", Default.areaColors[selectedElem3D.wi]);
                        //count = 36;
                    }
                }
                else if (renderGrid.WireframeMode && renderGrid.ShowQGradient)
                {
                    if (twoD)
                    {
                        renderGrid.shader.SetInt("isGradient", 1);
                        renderGrid.shader.SetColor4("color1", renderGrid.GridColors[currentElemIndex * 4]);
                        renderGrid.shader.SetColor4("color2", renderGrid.GridColors[currentElemIndex * 4 + 1]);
                        renderGrid.shader.SetColor4("color3", renderGrid.GridColors[currentElemIndex * 4 + 2]);
                        renderGrid.shader.SetColor4("color4", renderGrid.GridColors[currentElemIndex * 4 + 3]);
                        renderGrid.GradientMeshes[currentElemIndex].DrawElems(6, 0, PrimitiveType.Triangles);
                        renderGrid.shader.SetInt("isGradient", 0);
                    }
                }
                //Matrix4 view = Matrix4.Identity;
                //renderGrid.shader.SetMatrix4("view", ref view);
                GL.LineWidth(renderGrid.LinesSize);
                renderGrid.DrawLines(renderGrid.LinesMeshes[currentElemIndex], renderGrid.LinesColor);
                renderGrid.DrawNodes(renderGrid.LinesMeshes[currentElemIndex], renderGrid.PointsColor); ;
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
            if (isElemSelected)
                SetSelectedElemWindowSize();
        }

        private void SetSelected2DElemInfo()
        {
            isElemSelected = true;
            Grid2D grid2D = (Grid2D)renderGrid.Grid;
            Elem2D selectedElem2D = grid2D.Elems[currentElemIndex];
            ElemNumBlock.Text = $"№ элемента: {currentElemIndex} |";
            BlockSubAreaNum.Text = "Номер подобласти: " + (selectedElem2D.wi + 1).ToString();
            BlockNodesNum1.Text = "Л.Н. №: " + selectedElem2D.n1;
            BlockNodesCoords1.Text = "\tx: " + grid2D.XY[selectedElem2D.n1].X.ToString("0.00")
                                   + "\ty: " + grid2D.XY[selectedElem2D.n1].Y.ToString("0.00");
            BlockNodesNum2.Text = "П.Н. №: " + selectedElem2D.n2;
            BlockNodesCoords2.Text = "\tx: " + grid2D.XY[selectedElem2D.n2].X.ToString("0.00")
                                   + "\ty: " + grid2D.XY[selectedElem2D.n2].Y.ToString("0.00");
            BlockNodesNum3.Text = "Л.В. №: " + selectedElem2D.n3;
            BlockNodesCoords3.Text = "\tx: " + grid2D.XY[selectedElem2D.n3].X.ToString("0.00")
                                   + "\ty: " + grid2D.XY[selectedElem2D.n3].Y.ToString("0.00");
            BlockNodesNum4.Text = "П.В. №: " + selectedElem2D.n4;
            BlockNodesCoords4.Text = "\tx: " + grid2D.XY[selectedElem2D.n4].X.ToString("0.00")
                                   + "\ty: " + grid2D.XY[selectedElem2D.n4].Y.ToString("0.00");
            if (isQFileLoaded)
            {
                BlockNodesCoords1.Text += "\tq: " + q[selectedElem2D.n1].ToString("0.00");
                BlockNodesCoords2.Text += "\tq: " + q[selectedElem2D.n2].ToString("0.00");
                BlockNodesCoords3.Text += "\tq: " + q[selectedElem2D.n3].ToString("0.00");
                BlockNodesCoords4.Text += "\tq: " + q[selectedElem2D.n4].ToString("0.00");
            }
            if (selectedElem2D.n5 >= 0)
            {
                BlockNodesNum5.Text = "ДОП. №: " + selectedElem2D.n5;
                BlockNodesCoords5.Text = "\tx: " + grid2D.XY[selectedElem2D.n5].X.ToString("0.00")
                                       + "\ty: " + grid2D.XY[selectedElem2D.n5].Y.ToString("0.00");
                if (isQFileLoaded)
                    BlockNodesCoords5.Text += "\tq: " + q[selectedElem2D.n5].ToString("0.00");
            }
            else
            {
                BlockNodesNum5.Text = "";
                BlockNodesCoords5.Text = "";
            }
            BlockNodesCoords6.Text = ""; BlockNodesCoords6.Text = "";
            BlockNodesCoords7.Text = ""; BlockNodesCoords7.Text = "";
            BlockNodesCoords8.Text = ""; BlockNodesCoords8.Text = "";
            BlockNodesCoords9.Text = ""; BlockNodesCoords9.Text = "";
            BlockNodesCoords10.Text = ""; BlockNodesCoords10.Text = "";
        }

        private void SetSelected3DElemInfo()
        {
            isElemSelected = true;
            Grid3D grid3D = (Grid3D)renderGrid.Grid;
            Elem3D selectedElem3D = grid3D.Elems[currentElemIndex];
            ElemNumBlock.Text = $"№ элемента: {currentElemIndex} |";
            BlockSubAreaNum.Text = "Номер подобласти: " + (selectedElem3D.wi + 1).ToString();
            BlockNodesNum1.Text = "Л.Н.Б. №: " + selectedElem3D.n1;
            BlockNodesCoords1.Text = "\tx: " + grid3D.XYZ[selectedElem3D.n1].X.ToString("0.00")
                                   + "\ty: " + grid3D.XYZ[selectedElem3D.n1].Y.ToString("0.00") +
                                   "\tz: " + grid3D.XYZ[selectedElem3D.n1].Z.ToString("0.00");
            BlockNodesNum2.Text = "П.Н.Б. №: " + selectedElem3D.n2;
            BlockNodesCoords2.Text = "\tx: " + grid3D.XYZ[selectedElem3D.n2].X.ToString("0.00")
                                   + "\ty: " + grid3D.XYZ[selectedElem3D.n2].Y.ToString("0.00") +
                                   "\tz: " + grid3D.XYZ[selectedElem3D.n2].Z.ToString("0.00");
            BlockNodesNum3.Text = "Л.В.Б. №: " + selectedElem3D.n3;
            BlockNodesCoords3.Text = "\tx: " + grid3D.XYZ[selectedElem3D.n3].X.ToString("0.00")
                                   + "\ty: " + grid3D.XYZ[selectedElem3D.n3].Y.ToString("0.00") +
                                   "\tz: " + grid3D.XYZ[selectedElem3D.n3].Z.ToString("0.00");
            BlockNodesNum4.Text = "П.В.Б. №: " + selectedElem3D.n4;
            BlockNodesCoords4.Text = "\tx: " + grid3D.XYZ[selectedElem3D.n4].X.ToString("0.00")
                                   + "\ty: " + grid3D.XYZ[selectedElem3D.n4].Y.ToString("0.00") +
                                   "\tz: " + grid3D.XYZ[selectedElem3D.n4].Z.ToString("0.00");
            BlockNodesNum5.Text = "Л.Н.Д. №: " + selectedElem3D.n5;
            BlockNodesCoords5.Text = "\tx: " + grid3D.XYZ[selectedElem3D.n5].X.ToString("0.00")
                                   + "\ty: " + grid3D.XYZ[selectedElem3D.n5].Y.ToString("0.00") +
                                   "\tz: " + grid3D.XYZ[selectedElem3D.n5].Z.ToString("0.00");
            BlockNodesNum6.Text = "П.Н.Д. №: " + selectedElem3D.n6;
            BlockNodesCoords6.Text = "\tx: " + grid3D.XYZ[selectedElem3D.n6].X.ToString("0.00")
                                   + "\ty: " + grid3D.XYZ[selectedElem3D.n6].Y.ToString("0.00") +
                                   "\tz: " + grid3D.XYZ[selectedElem3D.n6].Z.ToString("0.00");
            BlockNodesNum7.Text = "Л.В.Д. №: " + selectedElem3D.n7;
            BlockNodesCoords7.Text = "\tx: " + grid3D.XYZ[selectedElem3D.n7].X.ToString("0.00")
                                   + "\ty: " + grid3D.XYZ[selectedElem3D.n7].Y.ToString("0.00") +
                                   "\tz: " + grid3D.XYZ[selectedElem3D.n7].Z.ToString("0.00");
            BlockNodesNum8.Text = "П.В.Д. №: " + selectedElem3D.n8;
            BlockNodesCoords8.Text = "\tx: " + grid3D.XYZ[selectedElem3D.n8].X.ToString("0.00")
                                   + "\ty: " + grid3D.XYZ[selectedElem3D.n8].Y.ToString("0.00") +
                                   "\tz: " + grid3D.XYZ[selectedElem3D.n8].Z.ToString("0.00");
            if (isQFileLoaded)
            {
                BlockNodesCoords1.Text += "\tq: " + q[selectedElem3D.n1].ToString("0.00");
                BlockNodesCoords2.Text += "\tq: " + q[selectedElem3D.n2].ToString("0.00");
                BlockNodesCoords3.Text += "\tq: " + q[selectedElem3D.n3].ToString("0.00");
                BlockNodesCoords4.Text += "\tq: " + q[selectedElem3D.n4].ToString("0.00");
                BlockNodesCoords5.Text += "\tq: " + q[selectedElem3D.n5].ToString("0.00");
                BlockNodesCoords6.Text += "\tq: " + q[selectedElem3D.n6].ToString("0.00");
                BlockNodesCoords7.Text += "\tq: " + q[selectedElem3D.n7].ToString("0.00");
                BlockNodesCoords8.Text += "\tq: " + q[selectedElem3D.n8].ToString("0.00");
            }
            if (selectedElem3D.n9 > 0 && selectedElem3D.n10 > 0)
            {
                BlockNodesNum9.Text = "ДОП1. №: " + selectedElem3D.n9;
                BlockNodesCoords9.Text = "\tx: " + grid3D.XYZ[selectedElem3D.n9].X.ToString("0.00")
                                       + "\ty: " + grid3D.XYZ[selectedElem3D.n9].Y.ToString("0.00") +
                                       "\tz: " + grid3D.XYZ[selectedElem3D.n9].Z.ToString("0.00");
                BlockNodesNum10.Text = "ДОП2. №: " + selectedElem3D.n10;
                BlockNodesCoords10.Text = "\tx: " + grid3D.XYZ[selectedElem3D.n10].X.ToString("0.00")
                                       + "\ty: " + grid3D.XYZ[selectedElem3D.n10].Y.ToString("0.00") +
                                       "\tz: " + grid3D.XYZ[selectedElem3D.n10].Z.ToString("0.00");
                if (isQFileLoaded)
                {
                    BlockNodesCoords9.Text += "\tq: " + q[selectedElem3D.n9].ToString("0.00");
                    BlockNodesCoords10.Text += "\tq: " + q[selectedElem3D.n10].ToString("0.00");
                }
            }
        }

        private void SetSelectedElemInfo()
        {
            if (twoD) SetSelected2DElemInfo(); else SetSelected3DElemInfo();
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
                    SetSelected2DElemInfo();
                }
                else
                    ResetSelectedElem();
            }
        }

        private void SetCurrentNodeMesh(bool removeQ = true)
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
            if (removeQ)
                RemoveQFile();
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

        static public Color ColorFloatToByte(Color4 color4)
        {
            Color color = new Color();
            color.R = (byte)(color4.R * 255);
            color.G = (byte)(color4.G * 255);
            color.B = (byte)(color4.B * 255);
            color.A = (byte)(color4.A * 255);
            return color;
        }
        static public Color4 ColorByteToFloat(Color color)
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
            MinColorPicker.SelectedColor = ColorFloatToByte(Default.MinColor);
            MaxColorPicker.SelectedColor = ColorFloatToByte(Default.MaxColor);

            if (renderGrid != null)
            {
                for (int i = 0; i < renderGrid.Grid.Nmats; i++)
                    renderGrid.AreaColors[i] = Default.areaColors[i];
            }

            WiremodeCheckBox.IsChecked = Default.wireframeMode;
            ShowGridCheckBox.IsChecked = Default.showGrid;
            ShowQGradientCheckbox.IsChecked = false;
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

        void FillComboBoxes()
        {
            RT1QuadDirDropDownMenu.Items.Add(StringConstants.right_top);
            RT1QuadDirDropDownMenu.Items.Add(StringConstants.top_right);
            RT1QuadDirDropDownMenu.Items.Add(StringConstants.skip);
            RT2QuadDirDropDownMenu.Items.Add(StringConstants.right_top);
            RT2QuadDirDropDownMenu.Items.Add(StringConstants.top_right);
            RT2QuadDirDropDownMenu.Items.Add(StringConstants.skip);

            LT1QuadDirDropDownMenu.Items.Add(StringConstants.left_top);
            LT1QuadDirDropDownMenu.Items.Add(StringConstants.top_left);
            LT1QuadDirDropDownMenu.Items.Add(StringConstants.skip);
            LT2QuadDirDropDownMenu.Items.Add(StringConstants.left_top);
            LT2QuadDirDropDownMenu.Items.Add(StringConstants.top_left);
            LT2QuadDirDropDownMenu.Items.Add(StringConstants.skip);

            LB1QuadDirDropDownMenu.Items.Add(StringConstants.left_bottom);
            LB1QuadDirDropDownMenu.Items.Add(StringConstants.bottom_left);
            LB1QuadDirDropDownMenu.Items.Add(StringConstants.skip);
            LB2QuadDirDropDownMenu.Items.Add(StringConstants.left_bottom);
            LB2QuadDirDropDownMenu.Items.Add(StringConstants.bottom_left);
            LB2QuadDirDropDownMenu.Items.Add(StringConstants.skip);

            RB1QuadDirDropDownMenu.Items.Add(StringConstants.right_bottom);
            RB1QuadDirDropDownMenu.Items.Add(StringConstants.bottom_right);
            RB1QuadDirDropDownMenu.Items.Add(StringConstants.skip);
            RB2QuadDirDropDownMenu.Items.Add(StringConstants.right_bottom);
            RB2QuadDirDropDownMenu.Items.Add(StringConstants.bottom_right);
            RB2QuadDirDropDownMenu.Items.Add(StringConstants.skip);
        }

        private void ResetComboBoxes()
        {
            RT1QuadDirDropDownMenu.SelectedItem = StringConstants.right_top;
            RT2QuadDirDropDownMenu.SelectedItem = StringConstants.top_right;
            LT1QuadDirDropDownMenu.SelectedItem = StringConstants.left_top;
            LT2QuadDirDropDownMenu.SelectedItem = StringConstants.top_left;
            LB1QuadDirDropDownMenu.SelectedItem = StringConstants.left_bottom;
            LB2QuadDirDropDownMenu.SelectedItem = StringConstants.bottom_left;
            RB1QuadDirDropDownMenu.SelectedItem = StringConstants.right_bottom;
            RB2QuadDirDropDownMenu.SelectedItem = StringConstants.bottom_right;
        }

        private void ResetSelectedElem()
        {
            isElemSelected = false;
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
                SmartMergeCheckBox.IsChecked = irregularGridMaker.SmartMerge;
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
                SmartMergeCheckBox.IsChecked = irregularGridMaker.SmartMerge;
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
            ResetComboBoxes();
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

        private void SetLTQuadDir(string dir_txt, int index)
        {
            switch (dir_txt)
            {
                case StringConstants.left_top:
                    irregularGridMaker.Directions[Quadrant.LeftTop][index] = Direction.Left; break;
                case StringConstants.top_left:
                    irregularGridMaker.Directions[Quadrant.LeftTop][index] = Direction.Top; break;
                case StringConstants.skip:
                    irregularGridMaker.Directions[Quadrant.LeftTop][index] = Direction.None; break;
            }
        }

        private void SetRTQuadDir(string dir_txt, int index)
        {
            switch (dir_txt)
            {
                case StringConstants.right_top:
                    irregularGridMaker.Directions[Quadrant.RightTop][index] = Direction.Right; break;
                case StringConstants.top_right:
                    irregularGridMaker.Directions[Quadrant.RightTop][index] = Direction.Top; break;
                case StringConstants.skip:
                    irregularGridMaker.Directions[Quadrant.RightTop][index] = Direction.None; break;
            }
        }

        private void SetLBQuadDir(string dir_txt, int index)
        {
            switch (dir_txt)
            {
                case StringConstants.left_bottom:
                    irregularGridMaker.Directions[Quadrant.LeftBottom][index] = Direction.Left; break;
                case StringConstants.bottom_left:
                    irregularGridMaker.Directions[Quadrant.LeftBottom][index] = Direction.Bottom; break;
                case StringConstants.skip:
                    irregularGridMaker.Directions[Quadrant.LeftBottom][index] = Direction.None; break;
            }
        }

        private void SetRBQuadDir(string dir_txt, int index)
        {
            switch (dir_txt)
            {
                case StringConstants.right_bottom:
                    irregularGridMaker.Directions[Quadrant.RightBottom][index] = Direction.Right; break;
                case StringConstants.bottom_right:
                    irregularGridMaker.Directions[Quadrant.RightBottom][index] = Direction.Bottom; break;
                case StringConstants.skip:
                    irregularGridMaker.Directions[Quadrant.RightBottom][index] = Direction.None; break;
            }
        }

        private void LT1QuadDirChanged(object sender, SelectionChangedEventArgs e)
        {
            string dir_txt = LT1QuadDirDropDownMenu.SelectedItem.ToString();
            SetLTQuadDir(dir_txt, 0);
        }

        private void LT2QuadDirChanged(object sender, SelectionChangedEventArgs e)
        {
            string dir_txt = LT2QuadDirDropDownMenu.SelectedItem.ToString();
            SetLTQuadDir(dir_txt, 1);
        }

        private void RT1QuadDirChanged(object sender, SelectionChangedEventArgs e)
        {
            string dir_txt = RT1QuadDirDropDownMenu.SelectedItem.ToString();
            SetRTQuadDir(dir_txt, 0);
        }

        private void RT2QuadDirChanged(object sender, SelectionChangedEventArgs e)
        {
            string dir_txt = RT2QuadDirDropDownMenu.SelectedItem.ToString();
            SetRTQuadDir(dir_txt, 1);
        }

        private void LB1QuadDirChanged(object sender, SelectionChangedEventArgs e)
        {
            string dir_txt = LB1QuadDirDropDownMenu.SelectedItem.ToString();
            SetLBQuadDir(dir_txt, 0);
        }

        private void LB2QuadDirChanged(object sender, SelectionChangedEventArgs e)
        {
            string dir_txt = LB2QuadDirDropDownMenu.SelectedItem.ToString();
            SetLBQuadDir(dir_txt, 1);
        }

        private void RB1QuadDirChanged(object sender, SelectionChangedEventArgs e)
        {
            string dir_txt = RB1QuadDirDropDownMenu.SelectedItem.ToString();
            SetRBQuadDir(dir_txt, 0);
        }

        private void RB2QuadDirChanged(object sender, SelectionChangedEventArgs e)
        {
            string dir_txt = RB2QuadDirDropDownMenu.SelectedItem.ToString();
            SetRBQuadDir(dir_txt, 1);
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
                float z0, zn, qz;
                int nz;
                if (float.TryParse(Z0Block.Text, out z0) && float.TryParse(ZnBlock.Text, out zn) &&
                    int.TryParse(NZBlock.Text, out nz) && float.TryParse(QZBlock.Text, out qz) &&
                    zn > z0 && nz > 0)
                {
                    twoD = false;
                    BlockCurrentMode.Text = "Режим: 3D";
                    List<float> Z = new List<float>(new float[nz + 1]);
                    int i0 = 0, j0 = 0;
                    Grid2D.MakeGrid1D(Z, z0, zn, nz, qz, ref i0, ref j0);
                    Z[nz] = zn;
                    regularGrid = new Grid3D((Grid2D)renderGrid.Grid, Z);
                    crossSections = new CrossSections((Grid3D)regularGrid);
                    prevGrid3D = (Grid3D)regularGrid;
                    SetRenderGrid();
                    ResetPosition();
                    ResetSelectedElem();
                }
                else ErrorHandler.DataErrorMessage("Введены некорректные данные для тирожирования сечения", false);
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
                List<float> q_new;
                if (isQFileLoaded) regularGrid = new Grid2D(grid3D, plane, index, value, true, q, out q_new);
                else regularGrid = new Grid2D(grid3D, plane, index, value, false, q, out q_new);
                q = q_new;
                twoD = true;
                BlockCurrentMode.Text = twoD ? "Режим: 2D" : "Режим: 3D";
                SetRenderGrid(false);
                ResetPosition();
                ResetSelectedElem();
                crossSections.Active = false;
                currentPlane = plane;
                if (isQFileLoaded) renderGrid.SetGridColors(q, false);
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
                SetSelectedElemWindowSize();
                SetSelectedElemInfo();
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
                SetSelectedElemWindowSize();
                SetSelectedElemInfo();
            }
            else ErrorHandler.DataErrorMessage("Некорректный шаг", false);
        }

        private void ResetElemClick(object sender, RoutedEventArgs e)
        {
            currentElemIndex = 0;
            ResetSelectedElem();
        }

        private void UploadQFileClick(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".txt"; // Default file extension
            dialog.Filter = "Text format (.txt)|*.txt"; // Filter files by extension

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            { 
                string qfileName = dialog.FileName;
                List<float> q_temp;
                try
                {
                    using (TextReader reader = File.OpenText(qfileName))
                    {
                        q_temp = new List<float>(renderGrid.Grid.Nnodes);
                        for (int i = 0; i < q_temp.Capacity; i++)
                        {
                            float qi =  float.Parse(reader.ReadLine());
                            q_temp.Add(qi);
                        }
                    }
                    q = q_temp;
                    renderGrid.SetGridColors(q);
                    QFileNameBlock.Text = "Файл: " + Path.GetFileName(qfileName);
                    isQFileLoaded = true;
                }
                catch (Exception ex)
                {
                    if (ex is DirectoryNotFoundException || ex is FileNotFoundException)
                        ErrorHandler.FileReadingErrorMessage("Не удалось найти файл с сеткой", false);
                    else if (ex is FormatException)
                        ErrorHandler.FileReadingErrorMessage("Некорректный формат файла", false);
                    else
                        ErrorHandler.FileReadingErrorMessage("Не удалось прочитать файл", false);
                }
            }
        }

        private void RemoveQFile()
        {
            isQFileLoaded = false;
            QFileNameBlock.Text = "Файл не загружен";
            renderGrid.ShowQGradient = false;
            ShowQGradientCheckbox.IsChecked = false;
        }

        private void RemoveQFileClick(object sender, RoutedEventArgs e)
        {
            RemoveQFile();
        }

        private void ShowQGradientChecked(object sender, RoutedEventArgs e)
        {
            if (isQFileLoaded)
            {
                renderGrid.ShowQGradient = true;
            }
            else 
            { 
                MessageBox.Show("Файл с решением не загружен", "Не удалось выполнить действие");
                ShowQGradientCheckbox.IsChecked = false;
            }
        }

        private void ShowQGradientUnChecked(object sender, RoutedEventArgs e)
        {
            renderGrid.ShowQGradient = false;
        }

        private void MinColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (renderGrid == null) return;
            renderGrid.MinColor = ColorByteToFloat((Color)e.NewValue);
            if (isQFileLoaded)
                renderGrid.SetGridColors(q);
        }

        private void MaxColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (renderGrid == null) return;
            renderGrid.MaxColor = ColorByteToFloat((Color)e.NewValue);
            if (isQFileLoaded)
                renderGrid.SetGridColors(q);
        }

        private void CreateNewGridClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Текущая сетка не будет сохранена. Продолжить?", "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.No)
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                Close();
            }
        }

        private void OpenFileClick(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".mkgrid"; // Default file extension
            dialog.Filter = "MakeGrid format (.mkgrid)|*.mkgrid"; // Filter files by extension

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                fileName = dialog.FileName;
                gridFromFile = true;
                ResetSelectedElem();
                ResetPosition();
                ResetUI();
                InitRegularGrid();
                SetRenderGrid();
            }
        }

        private void SaveFileClick(object sender, RoutedEventArgs e)
        {
            // Configure save file dialog box
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "New Grid"; // Default file name
            dlg.DefaultExt = ".mkgrid"; // Default file extension
            dlg.Filter = "MakeGrid format (.mkgrid)|*.mkgrid"; // Filter files by extension

            // Show save file dialog box
            bool? result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                File.WriteAllText(dlg.FileName, renderGrid.Grid.PrintInfo());
            }
        }

        private void SubAreaNumChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            string subAreaNum_txt = "";
            if (comboBox != null && comboBox.SelectedItem != null)
                subAreaNum_txt = comboBox.SelectedItem.ToString();
            int subAreaNum;
            if (int.TryParse(subAreaNum_txt, out subAreaNum))
            {
                SubAreaColorPicker.SelectedColor = ColorFloatToByte(renderGrid.AreaColors[subAreaNum - 1]);
            }
        }
        private void SubAreaColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (renderGrid == null) return;
            string subAreaNum_txt = "";
            if (SubAreaNumDownMenu != null && SubAreaNumDownMenu.SelectedItem != null)
                subAreaNum_txt = SubAreaNumDownMenu.SelectedItem.ToString();
            int subAreaNum;
            if (int.TryParse(subAreaNum_txt, out subAreaNum))
            {
                renderGrid.AreaColors[subAreaNum - 1] = ColorByteToFloat((Color)e.NewValue);
            }
        }

        private void DoubleOrHalfGrid(bool half)
        {
            if (gridParams != null && SubAreaAxisDownMenu != null && SubAreaAxisDownMenu.SelectedItem != null)
            {
                string raw_txt = SubAreaAxisDownMenu.SelectedItem.ToString();
                string axis_txt = raw_txt.Substring(raw_txt.Length - 1);
                switch (axis_txt)
                {
                    case "X":
                        for (int i = 0; i < gridParams.NX.Count; i++)
                        {
                            if (!half) gridParams.NX[i] *= 2;
                            else if (gridParams.NX[i] / 2 >= 1)
                                gridParams.NX[i] /= 2;
                        }
                        break;
                    case "Y":
                        for (int i = 0; i < gridParams.NY.Count; i++)
                        {
                            if (!half) gridParams.NY[i] *= 2;
                            else if (gridParams.NY[i] / 2 >= 1)
                                gridParams.NY[i] /= 2;
                        }
                        break;
                    case "Z":
                        if (twoD) return;
                        for (int i = 0; i < gridParams.NZ.Count; i++)
                        {
                            if (!half) gridParams.NZ[i] *= 2;
                            else if (gridParams.NZ[i] / 2 >= 1)
                                gridParams.NZ[i] /= 2;
                        }
                        break;
                }
                regularGrid = (twoD) ? new Grid2D(gridParams) : new Grid3D(gridParams);
            SetRenderGrid();
            ResetSelectedElem();
            }
        }

        private void DoubleGridClick(object sender, RoutedEventArgs e)
        {
            DoubleOrHalfGrid(false);
        }

        private void HalfGridClick(object sender, RoutedEventArgs e)
        {
            DoubleOrHalfGrid(true);
        }

        private void FEMSubAreaNumChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            string subAreaNum_txt = "";
            if (comboBox != null && comboBox.SelectedItem != null)
                subAreaNum_txt = comboBox.SelectedItem.ToString();
            int subAreaNum;
            if (int.TryParse(subAreaNum_txt, out subAreaNum))
            {
                ;
            }
        }

        private void FEMLambdaChanged(object sender, RoutedEventArgs e)
        {

        }

        private void FEMSigmaChanged(object sender, RoutedEventArgs e)
        {

        }

        private void FEMChiChanged(object sender, RoutedEventArgs e)
        {

        }

        private void FEMFChanged(object sender, RoutedEventArgs e)
        {

        }

        private void FEM1BCLeftChanged(object sender, RoutedEventArgs e)
        {

        }

        private void FEM1BCRightChanged(object sender, RoutedEventArgs e)
        {

        }

        private void FEM1BCTopChanged(object sender, RoutedEventArgs e)
        {

        }

        private void FEM1BCBottomChanged(object sender, RoutedEventArgs e)
        {

        }

        private void FEMT0Changed(object sender, RoutedEventArgs e)
        {

        }

        private void FEMTnChanged(object sender, RoutedEventArgs e)
        {

        }

        private void FEMNtChanged(object sender, RoutedEventArgs e)
        {

        }

        private void FEMQtChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MakeGrid3D
{
    /// <summary>
    /// Interaction logic for GraphicsWindow.xaml
    /// </summary>
    class GridState
    {
        public Grid2D Grid2D { get; }
        public int I { get; } = Default.I;
        public int J { get; } = Default.J;
        public int NodeI { get; } = Default.I;
        public int NodeJ { get; } = Default.J;
        public int DirIndex { get; } = 0;
        public GridState(Grid2D grid2D, int i, int j, int nodeI, int nodeJ, int dirIndex)
        {
            Grid2D = grid2D;
            I = i;
            J = j;
            NodeI = nodeI;
            NodeJ = nodeJ;
            DirIndex = dirIndex;
        }
        public GridState(Grid2D grid2D)
        {
            Grid2D = grid2D;
        }
    }

    public partial class GraphicsWindow : Window
    {
        RenderGrid renderGrid;
        IrregularGridMaker irregularGridMaker;
        Grid2D regularGrid2D;
        LinkedList<GridState> grid2DList;
        LinkedListNode<GridState> currentNode;

        Mesh axis;
        Mesh? selectedElemMesh = null;
        Mesh? selectedElemLines = null;
        Mesh currentNodeMesh;
        Mesh currentPosMesh;

        Matrix4 projectionSelectedElem = Matrix4.Identity;
        Matrix4 rtranslate = Matrix4.Identity;
        Matrix4 rscale = Matrix4.Identity; // TODO: не используется вообще все матрицы scale
        float horOffset = 0;
        float verOffset = 0;
        float scaleX = 1;
        float scaleY = 1;
        float mouse_horOffset = 0;
        float mouse_verOffset = 0;
        float mouse_scaleX = 1;
        float mouse_scaleY = 1;
        float speedTranslate = Default.speedTranslate;
        float speedZoom = Default.speedZoom;
        float speedHor = 0;
        float speedVer = 0;
        Color4 bgColor = Default.bgColor;
        string fileName = "C:\\Users\\artor\\OneDrive\\Рабочий стол\\тесты на практику\\TEST2.txt";
        Elem selectedElem;
        bool showCurrentUnstructedNode = Default.showCurrentUnstructedNode;
        Color4 currentUnstructedNodeColor = Default.currentUnstructedNodeColor;

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

        private void Init()
        {
            Area area = new Area(fileName);
            regularGrid2D = new Grid2D(fileName, area);
            renderGrid = new RenderGrid(regularGrid2D, (float)OpenTkControl.ActualWidth, (float)OpenTkControl.ActualHeight);
            irregularGridMaker = new IrregularGridMaker(regularGrid2D);
            grid2DList = new LinkedList<GridState>();
            SetCurrentNodeMesh();
            grid2DList.AddLast(new GridState(regularGrid2D));
            currentNode = grid2DList.Last;
            // Множители скорости = 1 процент от ширины(высоты) мира
            speedHor = (renderGrid.Right - renderGrid.Left) * 0.01f;
            speedVer = (renderGrid.Top - renderGrid.Bottom) * 0.01f;
            SetAxis();
        }

        private void OpenTkControl_OnLoad(object sender, RoutedEventArgs e)
        {
            Init();
            // если удалить отсюда то не будет показываться в статус баре
            BlockAreaCount.Text = "Количество подобластей: " + renderGrid.Grid2D.Area.Nmats;
            BlockNodesCount.Text = "Количество узлов: " + renderGrid.Grid2D.Nnodes;
            BlockElemsCount.Text = "Количество элементов: " + renderGrid.Grid2D.Nelems;
            BlockRemovedNodesCount.Text = "***";
            BlockRemovedElemsCount.Text = "***";
            //-----------------------------------------------------------------------------
        }

        private void OpenTkControl_Resize(object sender, SizeChangedEventArgs e)
        {
            if (renderGrid == null)
                return;
            renderGrid.WindowWidth = (float)OpenTkControl.ActualWidth;
            renderGrid.WindowHeight = (float)OpenTkControl.ActualHeight;
            renderGrid.SetSize();
        }

        private void OpenTkControl_OnRender(TimeSpan obj)
        {
            if (renderGrid == null)
                return;
            BlockAreaCount.Text = "Количество подобластей: " + renderGrid.Grid2D.Area.Nmats;
            BlockNodesCount.Text = "Количество узлов: " + renderGrid.Grid2D.Nnodes;
            BlockElemsCount.Text = "Количество элементов: " + renderGrid.Grid2D.Nelems;

            GL.ClearColor(bgColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            // Чтобы работали прозрачные цвета
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend);
            if (renderGrid.DrawRemovedLinesMode)
            {
                renderGrid.Grid2D = regularGrid2D;
                renderGrid.RenderFrame(drawLines:false, drawNodes:false);
                renderGrid.shader.DashedLines(true);
                renderGrid.RenderFrame(drawArea:false, drawNodes:false);
                renderGrid.shader.DashedLines(false, renderGrid.LinesSize);

                renderGrid.Grid2D = currentNode.ValueRef.Grid2D;
                renderGrid.RenderFrame(drawArea:false);
            }
            else
            {
                renderGrid.RenderFrame();
            }

            if (showCurrentUnstructedNode)
            {
                renderGrid.DrawNodes(currentPosMesh, Color4.Orange);
                renderGrid.DrawNodes(currentNodeMesh, currentUnstructedNodeColor);
                // первый узел почему то тоже красится...
                float[] vert = { renderGrid.Grid2D.XY[0].X, renderGrid.Grid2D.XY[0].Y, 0 };
                uint[] index = { 0 };
                Mesh firstNode = new Mesh(vert, index);
                renderGrid.DrawNodes(firstNode, renderGrid.PointsColor);
                firstNode.Dispose();
                //----------------------
                int node = renderGrid.Grid2D.global_num(irregularGridMaker.I, irregularGridMaker.J);
                float x = renderGrid.Grid2D.XY[node].X;
                float y = renderGrid.Grid2D.XY[node].Y;
                CurrentUnstructedNodeBlock.Text = $"Номер узла: {node} | X: " + x.ToString("0.00") + ", " + y.ToString("0.00");
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
            }
        }

        private void Reset()
        {
            speedTranslate = Default.speedTranslate;
            speedZoom = Default.speedZoom;
            renderGrid.LinesSize = Default.linesSize;
            renderGrid.PointsSize = Default.pointsSize;
            renderGrid.LinesColor = Default.linesColor;
            renderGrid.PointsColor = Default.pointsColor;
            bgColor = Default.bgColor;
            renderGrid.WireframeMode = Default.wireframeMode;
            renderGrid.ShowGrid = Default.showGrid;
            renderGrid.DrawRemovedLinesMode = Default.drawRemovedLinesMode;
            irregularGridMaker.MaxAR = (float)Default.maxAR_width / Default.maxAR_height;
            showCurrentUnstructedNode = Default.showCurrentUnstructedNode;
            currentUnstructedNodeColor = Default.currentUnstructedNodeColor;
        }

        private void ResetPosition()
        {
            renderGrid.Translate = Matrix4.Identity;
            renderGrid.Scale = Matrix4.Identity;
            rtranslate = Matrix4.Identity;
            rscale = Matrix4.Identity;
            horOffset = 0;
            verOffset = 0;
            scaleX = 1;
            scaleY = 1;
            mouse_horOffset = 0;
            mouse_verOffset = 0;
            mouse_scaleX = 1;
            mouse_scaleY = 1;
            renderGrid.Indent = Default.indent;
            renderGrid.SetSize();
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
            Point new_position = MouseMap(position);
            double x = new_position.X;
            double y = new_position.Y;
            BlockCoordinates.Text = "X: " + x.ToString("0.00") + ", Y: " + y.ToString("0.00");
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
                    renderGrid.shader.SetColor4("current_color", Default.areaColors[selectedElem.wi]);
                    selectedElemMesh.DrawElems(6, 0, PrimitiveType.Triangles);
                }
                renderGrid.DrawLines(selectedElemLines, renderGrid.LinesColor);
                renderGrid.DrawNodes(selectedElemLines, renderGrid.PointsColor);
            }
        }

        private void SetSelectedElemWindowSize()
        {
            float left = renderGrid.Grid2D.XY[selectedElem.n1].X;
            float right = renderGrid.Grid2D.XY[selectedElem.n4].X;
            float bottom = renderGrid.Grid2D.XY[selectedElem.n1].Y;
            float top = renderGrid.Grid2D.XY[selectedElem.n4].Y;

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

        private void SelectedElemOpenTkControl_Resize(object sender, SizeChangedEventArgs e)
        {
            if (selectedElemMesh != null && selectedElemLines!= null)
                SetSelectedElemWindowSize();
        }

        private void OpenTkControl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (renderGrid == null)
                return;
            var position = e.GetPosition(OpenTkControl);
            Point new_position = MouseMap(position);
            float x = (float)new_position.X;
            float y = (float)new_position.Y;

            if (renderGrid.Grid2D.FindElem(x, y, ref selectedElem))
            {
                SetSelectedElemWindowSize();
                float left = renderGrid.Grid2D.XY[selectedElem.n1].X;
                float right = renderGrid.Grid2D.XY[selectedElem.n4].X;
                float bottom = renderGrid.Grid2D.XY[selectedElem.n1].Y;
                float top = renderGrid.Grid2D.XY[selectedElem.n4].Y;
                float xt, yt;
                if (selectedElem.n5 >= 0) {
                    xt = renderGrid.Grid2D.XY[selectedElem.n5].X;
                    yt = renderGrid.Grid2D.XY[selectedElem.n5].Y;
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

                if (selectedElemMesh != null)
                    selectedElemMesh.Dispose();
                if (selectedElemLines != null)
                    selectedElemLines.Dispose();
                selectedElemMesh = new Mesh(vertices, indices);
                selectedElemLines = new Mesh(selectedElemMesh.Vbo, indices_lines, vertices.Length);

                BlockSubAreaNum.Text = "Номер подобласти: " + (selectedElem.wi + 1).ToString();
                BlockNodesNum1.Text = "Л.Н. №: " + selectedElem.n1;
                BlockNodesCoords1.Text = "x: " + renderGrid.Grid2D.XY[selectedElem.n1].X.ToString("0.00")
                                       + " y: " + renderGrid.Grid2D.XY[selectedElem.n1].Y.ToString("0.00");
                BlockNodesNum2.Text = "П.Н. №: " + selectedElem.n2;
                BlockNodesCoords2.Text = "x: " + renderGrid.Grid2D.XY[selectedElem.n2].X.ToString("0.00")
                                       + " y: " + renderGrid.Grid2D.XY[selectedElem.n2].Y.ToString("0.00");
                BlockNodesNum3.Text = "Л.В. №: " + selectedElem.n3;
                BlockNodesCoords3.Text = "x: " + renderGrid.Grid2D.XY[selectedElem.n3].X.ToString("0.00")
                                       + " y: " + renderGrid.Grid2D.XY[selectedElem.n3].Y.ToString("0.00");
                BlockNodesNum4.Text = "П.В. №: " + selectedElem.n4;
                BlockNodesCoords4.Text = "x: " + renderGrid.Grid2D.XY[selectedElem.n4].X.ToString("0.00")
                                       + " y: " + renderGrid.Grid2D.XY[selectedElem.n4].Y.ToString("0.00");
                if (selectedElem.n5 >= 0)
                {
                    BlockNodesNum5.Text = "ДОП. №: " + selectedElem.n5;
                    BlockNodesCoords5.Text = "x: " + renderGrid.Grid2D.XY[selectedElem.n5].X.ToString("0.00")
                                           + " y: " + renderGrid.Grid2D.XY[selectedElem.n5].Y.ToString("0.00");
                }
                else
                {
                    BlockNodesNum5.Text = "";
                    BlockNodesCoords5.Text = "";
                }
            }
            else
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
        }

        private void SetCurrentNodeMesh()
        {
            int node_num_1 = renderGrid.Grid2D.global_num(irregularGridMaker.I, irregularGridMaker.J);
            int node_num_2 = renderGrid.Grid2D.global_num(irregularGridMaker.NodeI, irregularGridMaker.NodeJ);
            float x1 = renderGrid.Grid2D.XY[node_num_1].X;
            float y1 = renderGrid.Grid2D.XY[node_num_1].Y;
            float[] vertices1 = { x1, y1, 0 };
            uint[] indices1 = { 0 };
            currentPosMesh = new Mesh(vertices1, indices1);

            float x2 = renderGrid.Grid2D.XY[node_num_2].X;
            float y2 = renderGrid.Grid2D.XY[node_num_2].Y;
            float[] vertices2 = { x2, y2, 0 };
            uint[] indices2 = { 0 };
            currentNodeMesh = new Mesh(vertices2, indices2);
        }

        private void OpenTkControl_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (renderGrid == null)
                return;
            var position = e.GetPosition(OpenTkControl);
            Point new_position = MouseMap(position);
            float x = (float)new_position.X;
            float y = (float)new_position.Y;

            if (renderGrid.Grid2D.FindElem(x, y, ref selectedElem))
            {  
                float x1 = renderGrid.Grid2D.XY[selectedElem.n1].X;
                float x2 = renderGrid.Grid2D.XY[selectedElem.n4].X;
                float y1 = renderGrid.Grid2D.XY[selectedElem.n1].Y;
                float y2 = renderGrid.Grid2D.XY[selectedElem.n4].Y;
                
                // TODO: Что за...
                Tuple<int, float> distance_lb = new Tuple<int, float>(1, MathF.Sqrt(MathF.Pow(x - x1, 2) + MathF.Pow(y - y1, 2)));
                Tuple<int, float> distance_rb = new Tuple<int, float>(2, MathF.Sqrt(MathF.Pow(x - x2, 2) + MathF.Pow(y - y1, 2)));
                Tuple<int, float> distance_lu = new Tuple<int, float>(3, MathF.Sqrt(MathF.Pow(x - x1, 2) + MathF.Pow(y - y2, 2)));
                Tuple<int, float> distance_ru = new Tuple<int, float>(4, MathF.Sqrt(MathF.Pow(x - x2, 2) + MathF.Pow(y - y2, 2)));

                List<Tuple<int, float>> distances = new List<Tuple<int, float>> { distance_lb, distance_rb, distance_lu, distance_ru};
                int id = distances.MinBy(t => t.Item2).Item1;
                int node = 0;
                int i = 0; int j = 0;
                switch (id)
                {
                    case 1:
                        node = selectedElem.n1;
                        i = renderGrid.Grid2D.global_ij(node).X;
                        j = renderGrid.Grid2D.global_ij(node).Y;
                        break;
                    case 2:
                        node = selectedElem.n2;
                        i = renderGrid.Grid2D.global_ij(node).X;
                        j = renderGrid.Grid2D.global_ij(node).Y;
                        break;
                    case 3:
                        node = selectedElem.n3;
                        i = renderGrid.Grid2D.global_ij(node).X;
                        j = renderGrid.Grid2D.global_ij(node).Y;
                        break;
                    case 4:
                        node = selectedElem.n4;
                        i = renderGrid.Grid2D.global_ij(node).X;
                        j = renderGrid.Grid2D.global_ij(node).Y;
                        break;
                }
                if (i < renderGrid.Grid2D.Nx - 1 && j < renderGrid.Grid2D.Ny - 1 && i > 0 && j > 0)
                {
                    irregularGridMaker.I = i;
                    irregularGridMaker.J = j;
                    irregularGridMaker.NodeI = i;
                    irregularGridMaker.NodeJ = j;
                    SetCurrentNodeMesh();
                }
            }
        }

        private void AxisOpenTkControl_OnRender(TimeSpan obj)
        {
            if (renderGrid == null)
                return;
            GL.ClearColor(Color4.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            renderGrid.shader.Use();
            renderGrid.shader.SetColor4("current_color", new Color4(78 / 255f, 252 / 255f, 3 / 255f, 1));
            axis.DrawElems(6, 0, PrimitiveType.Lines);
            renderGrid.shader.SetColor4("current_color", Color4.Red);
            axis.DrawElems(6, 6, PrimitiveType.Lines);
        }

        private void SetAxis()
        {
            float mid_x = (renderGrid.Right + renderGrid.Left) / 2f;
            float mid_y = (renderGrid.Top + renderGrid.Bottom) / 2f;
            float offset_x = (renderGrid.Right - renderGrid.Left) * 0.05f;
            float offset_y = (renderGrid.Top - renderGrid.Bottom) * 0.1f;
            float[] vertices = {
                                                    // x
                                 mid_x, renderGrid.Bottom, 0, //0
                                 mid_x, renderGrid.Top, 0, // 1
                                 mid_x - offset_x, renderGrid.Top - offset_y, 0, // 2
                                 mid_x + offset_x, renderGrid.Top - offset_y, 0, // 3
                                                    // y
                                 renderGrid.Left, mid_y, 0, // 4
                                 renderGrid.Right, mid_y, 0,// 5
                                 renderGrid.Right - offset_x, mid_y + offset_y, 0, // 6
                                 renderGrid.Right - offset_x, mid_y - offset_y, 0 }; // 7
            uint[] indices = { 0, 1, 1, 2, 1, 3, 4, 5, 5, 6, 5, 7 };
            if (axis != null)
                axis.Dispose();
            axis = new Mesh(vertices, indices);
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
            }
            renderGrid.CleanUp();
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
            ResetPosition();
            Reset();
            ResetUI();
            Init();
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
            SpeedTranslateSlider.Value = Default.speedTranslate;
            SpeedZoomSlider.Value = Default.speedZoom;

            PointsColorPicker.SelectedColor = ColorFloatToByte(Default.pointsColor);
            LinesColorPicker.SelectedColor = ColorFloatToByte(Default.linesColor);
            BgColorPicker.SelectedColor = ColorFloatToByte(Default.bgColor);
            WiremodeCheckBox.IsChecked = Default.wireframeMode;
            ShowGridCheckBox.IsChecked = Default.showGrid;

            DrawRemovedLinesCheckBox.IsChecked = Default.drawRemovedLinesMode;
            WidthInput.Text = Default.maxAR_width.ToString();
            HeightInput.Text = Default.maxAR_height.ToString();
            ShowCurrentUnstructedNodeCheckBox.IsChecked = Default.showCurrentUnstructedNode;
            CurrentUnstructedNodeColorPicker.SelectedColor = ColorFloatToByte(Default.currentUnstructedNodeColor);
        }

        private void RotateLeftClick(object sender, RoutedEventArgs e)
        {

        }

        private void MoveLeftClick(object sender, RoutedEventArgs e)
        {

            horOffset += speedHor * speedTranslate;
            renderGrid.Translate = Matrix4.CreateTranslation(horOffset, verOffset, 0);

            mouse_horOffset -= speedHor * speedTranslate;
            rtranslate = Matrix4.CreateTranslation(mouse_horOffset, mouse_verOffset, 0);
        }

        private void MoveRightClick(object sender, RoutedEventArgs e)
        {
            horOffset -= speedHor * speedTranslate;
            renderGrid.Translate = Matrix4.CreateTranslation(horOffset, verOffset, 0);

            mouse_horOffset += speedHor * speedTranslate;
            rtranslate = Matrix4.CreateTranslation(mouse_horOffset, mouse_verOffset, 0);
        }

        private void MoveDownClick(object sender, RoutedEventArgs e)
        {
            verOffset += speedVer * speedTranslate;
            renderGrid.Translate = Matrix4.CreateTranslation(horOffset, verOffset, 0);

            mouse_verOffset -= speedVer * speedTranslate;
            rtranslate = Matrix4.CreateTranslation(mouse_horOffset, mouse_verOffset, 0);
        }

        private void MoveUpClick(object sender, RoutedEventArgs e)
        {
            verOffset -= speedVer * speedTranslate;
            renderGrid.Translate = Matrix4.CreateTranslation(horOffset, verOffset, 0);

            mouse_verOffset += speedVer * speedTranslate;
            rtranslate = Matrix4.CreateTranslation(mouse_horOffset, mouse_verOffset, 0);
        }

        private void ZoomInClick(object sender, RoutedEventArgs e)
        {
            if (renderGrid.Indent >= -0.5f)
            {
                renderGrid.Indent -= speedZoom;
                renderGrid.SetSize();
            }
            //MessageBox.Show(BufferClass.indent.ToString());
            //BufferClass.scaleX *= BufferClass.speedZoom;
            //BufferClass.scaleY *= BufferClass.speedZoom;
            //BufferClass.scale = Matrix4.CreateScale(BufferClass.scaleX, BufferClass.scaleY, 1);

            //BufferClass.mouse_scaleX /= BufferClass.speedZoom;
            //BufferClass.mouse_scaleY /= BufferClass.speedZoom;
        }

        private void ZoomOutClick(object sender, RoutedEventArgs e)
        {
            renderGrid.Indent += speedZoom;
            renderGrid.SetSize();
            //BufferClass.scaleX /= BufferClass.speedZoom;
            //BufferClass.scaleY /= BufferClass.speedZoom;
            //BufferClass.scale = Matrix4.CreateScale(BufferClass.scaleX, BufferClass.scaleY, 1);

            //BufferClass.mouse_scaleX *= BufferClass.speedZoom;
            //BufferClass.mouse_scaleY *= BufferClass.speedZoom;
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

        private void SpeedTranslateChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            speedTranslate = (float)e.NewValue;
        }

        private void SpeedZoomChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            speedZoom = (float)e.NewValue;
        }

        private void ResetSettingsClick(object sender, RoutedEventArgs e)
        {
            Reset();
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
                renderGrid.Grid2D = currentNode.ValueRef.Grid2D;
                irregularGridMaker.Grid2D = currentNode.ValueRef.Grid2D;
                int i = currentNode.Value.I;
                int j = currentNode.Value.J;
                int nodeI = currentNode.Value.NodeI;
                int nodeJ = currentNode.Value.NodeJ;
                int dirIndex = currentNode.Value.DirIndex;
                irregularGridMaker.I = i;
                irregularGridMaker.J = j;
                irregularGridMaker.NodeI = nodeI;
                irregularGridMaker.NodeJ = nodeJ;
                irregularGridMaker.DirIndex = dirIndex;
                SetCurrentNodeMesh();
            }
        }

        private void DoClick(object sender, RoutedEventArgs e)
        {
            if (currentNode != null && currentNode.Next != null)
            {
                currentNode = currentNode.Next;
                renderGrid.Grid2D = currentNode.ValueRef.Grid2D;
                irregularGridMaker.Grid2D = currentNode.ValueRef.Grid2D;
                int i = currentNode.Value.I;
                int j = currentNode.Value.J;
                int nodeI = currentNode.Value.NodeI;
                int nodeJ = currentNode.Value.NodeJ;
                int dirIndex = currentNode.Value.DirIndex;
                irregularGridMaker.I = i;
                irregularGridMaker.J = j;
                irregularGridMaker.NodeI = nodeI;
                irregularGridMaker.NodeJ = nodeJ;
                irregularGridMaker.DirIndex = dirIndex;
                SetCurrentNodeMesh();
            }
        }

        private void RemoveElemsClick(object sender, RoutedEventArgs e)
        {
            if (irregularGridMaker.NodeI < irregularGridMaker.Grid2D.Nx - 1 &&
                irregularGridMaker.NodeJ < irregularGridMaker.Grid2D.Ny - 1) 
            {
                Grid2D grid2D_new = irregularGridMaker.MakeUnStructedGrid();
                while (currentNode != null && currentNode.Next != null)
                    grid2DList.Remove(currentNode.Next);
                int nodeI = irregularGridMaker.NodeI;
                int nodeJ = irregularGridMaker.NodeJ;
                int i = irregularGridMaker.I;
                int j = irregularGridMaker.J;
                int dirIndex = irregularGridMaker.DirIndex;
                SetCurrentNodeMesh();
                grid2DList.AddLast(new GridState(grid2D_new, i, j, nodeI, nodeJ, dirIndex));
                currentNode = grid2DList.Last;
                renderGrid.Grid2D = grid2D_new;
                irregularGridMaker.Grid2D = grid2D_new; 
            }
        }

        private void RemoveAllElemsClick(object sender, RoutedEventArgs e)
        {
            // TODO: Соптимизировать
            if (irregularGridMaker.NodeI < irregularGridMaker.Grid2D.Nx - 1 &&
                irregularGridMaker.NodeJ < irregularGridMaker.Grid2D.Ny - 1)
            {
                Grid2D grid2D_new = irregularGridMaker.MakeUnStructedGrid();
                irregularGridMaker.Grid2D = grid2D_new;
                while (irregularGridMaker.NodeI < irregularGridMaker.Grid2D.Nx - 1 &&
                    irregularGridMaker.NodeJ < irregularGridMaker.Grid2D.Ny - 1)
                {
                    grid2D_new = irregularGridMaker.MakeUnStructedGrid();
                    irregularGridMaker.Grid2D = grid2D_new;
                }
                while (currentNode != null && currentNode.Next != null)
                    grid2DList.Remove(currentNode.Next);
                SetCurrentNodeMesh();
                int dirIndex = irregularGridMaker.DirIndex;
                grid2DList.AddLast(new GridState(grid2D_new, grid2D_new.Nx - 1, grid2D_new.Ny - 1, grid2D_new.Nx - 1, grid2D_new.Ny - 1, dirIndex));
                currentNode = grid2DList.Last;
                renderGrid.Grid2D = grid2D_new;
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
            renderGrid.Grid2D = regularGrid2D;
            irregularGridMaker.Grid2D = regularGrid2D;
            irregularGridMaker.I = 1;
            irregularGridMaker.J = 1;
            irregularGridMaker.NodeI = 1;
            irregularGridMaker.NodeJ = 1;
            irregularGridMaker.DirIndex = 0;
            grid2DList.Clear();
            SetCurrentNodeMesh();
            grid2DList.AddLast(new GridState(regularGrid2D));
            currentNode = grid2DList.Last;
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

    }
}
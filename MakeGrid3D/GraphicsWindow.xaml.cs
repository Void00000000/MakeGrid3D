using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using System.Reflection;
using static System.Formats.Asn1.AsnWriter;

namespace MakeGrid3D
{
    /// <summary>
    /// Interaction logic for GraphicsWindow.xaml
    /// </summary>

    public partial class GraphicsWindow : Window
    {
        RenderGrid renderGrid;
        IrregularGridMaker irregularGridMaker;
        Grid2D regularGrid2D, irregularGrid2D;
        Mesh axis;
        Mesh? selectedElemMesh = null;
        Mesh? selectedElemLines = null;

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
        bool unstructedGridMode = Default.unstructedGridMode;
        string fileName = "C:\\Users\\artor\\OneDrive\\Рабочий стол\\тесты на практику\\TEST2.txt";

        Elem selectedElem;
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

        private void OpenTkControl_OnLoad(object sender, RoutedEventArgs e)
        {
            Area area = new Area(fileName);
            regularGrid2D = new Grid2D(fileName, area);
            irregularGridMaker = new IrregularGridMaker(regularGrid2D);
            irregularGrid2D = irregularGridMaker.MakeUnStructedGrid();
            renderGrid = new RenderGrid(regularGrid2D, (float)OpenTkControl.ActualWidth, (float)OpenTkControl.ActualHeight);
            // если удалить отсюда то не будет показываться в статус баре
            BlockAreaCount.Text = "Количество подобластей: " + renderGrid.Grid2D.area.Nmats;
            BlockNodesCount.Text = "Количество узлов: " + renderGrid.Grid2D.Nnodes;
            BlockElemsCount.Text = "Количество элементов: " + renderGrid.Grid2D.Nelems;
            BlockRemovedNodesCount.Text = "***";
            BlockRemovedElemsCount.Text = "***";
            //-----------------------------------------------------------------------------
            // Множители скорости = 1 процент от ширины(высоты) мира
            speedHor = (renderGrid.Right - renderGrid.Left) * 0.01f;
            speedVer = (renderGrid.Top - renderGrid.Bottom) * 0.01f;
            SetAxis();
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
            BlockAreaCount.Text = "Количество подобластей: " + renderGrid.Grid2D.area.Nareas;
            BlockNodesCount.Text = "Количество узлов: " + renderGrid.Grid2D.Nnodes;
            BlockElemsCount.Text = "Количество элементов: " + renderGrid.Grid2D.Nelems;

            GL.ClearColor(bgColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            // Чтобы работали прозрачные цвета
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend);
            if (renderGrid.drawRemovedLinesMode)
            {
                renderGrid.Grid2D = regularGrid2D;
                renderGrid.shader.DashedLines(true);
                renderGrid.RenderFrame(drawArea:false, drawNodes:false);
                renderGrid.shader.DashedLines(false, renderGrid.linesSize);
                renderGrid.Grid2D = irregularGrid2D;
            }
            renderGrid.RenderFrame();
            if (unstructedGridMode)
            {
                renderGrid.Grid2D = irregularGrid2D;
                unstructedGridMode = false;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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

        private void Reset()
        {
            speedTranslate = Default.speedTranslate;
            speedZoom = Default.speedZoom;
            renderGrid.linesSize = Default.linesSize;
            renderGrid.pointsSize = Default.pointsSize;
            renderGrid.linesColor = Default.linesColor;
            renderGrid.pointsColor = Default.pointsColor;
            bgColor = Default.bgColor;
            renderGrid.wireframeMode = Default.wireframeMode;
            renderGrid.showGrid = Default.showGrid;
            renderGrid.drawRemovedLinesMode = Default.drawRemovedLinesMode;
            irregularGridMaker.maxAR = (float)Default.maxAR_width / Default.maxAR_height;
        }

        private void ResetPosition()
        {
            renderGrid.translate = Matrix4.Identity;
            renderGrid.scale = Matrix4.Identity;
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
            renderGrid.indent = Default.indent;
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
                if (!renderGrid.wireframeMode)
                {
                    selectedElemMesh.Use();
                    renderGrid.shader.SetColor4("current_color", Default.areaColors[selectedElem.wi]);
                    selectedElemMesh.DrawElems(6, 0, PrimitiveType.Triangles);
                }

                selectedElemLines.Use();
                renderGrid.shader.SetColor4("current_color", renderGrid.linesColor);
                selectedElemLines.DrawElems(8, 0, PrimitiveType.Lines);
                renderGrid.shader.SetColor4("current_color", renderGrid.pointsColor);
                selectedElemLines.DrawVerices(4 * 3, 0, PrimitiveType.Points);
            }
        }

        private void OpenTkControl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (renderGrid == null)
                return;
            var position = e.GetPosition(OpenTkControl);
            Point new_position = MouseMap(position);
            float x = (float)new_position.X;
            float y = (float)new_position.Y;
            
            foreach (Elem elem in renderGrid.Grid2D.Elems)
            {
                float xAreaMin = renderGrid.Grid2D.XY[elem.n1].X;
                float yAreaMin = renderGrid.Grid2D.XY[elem.n1].Y;
                float xAreaMax = renderGrid.Grid2D.XY[elem.n4].X;
                float yAreaMax = renderGrid.Grid2D.XY[elem.n4].Y;
                if (x >= xAreaMin && x <= xAreaMax && y >= yAreaMin && y <= yAreaMax)
                {
                    selectedElem = elem;

                    float left = renderGrid.Grid2D.XY[selectedElem.n1].X;
                    float right = renderGrid.Grid2D.XY[selectedElem.n4].X;
                    float bottom = renderGrid.Grid2D.XY[selectedElem.n1].Y;
                    float top = renderGrid.Grid2D.XY[selectedElem.n4].Y;

                    float width = right - left;
                    float height = top - bottom;

                    float indent = 0.2f;
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
                    float[] vertices = { left,  bottom, 0,  // 0
                                         right, bottom, 0,  // 1
                                         left,  top,    0,  // 2
                                         right, top,    0}; // 3
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
                    return;
                }
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
            }
        }
        private void AxisOpenTkControl_OnRender(TimeSpan obj)
        {
            if (renderGrid == null)
                return;
            GL.ClearColor(Color4.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            renderGrid.shader.Use();
            axis.Use();
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
            unstructedGridMode = false;
            // TODO:
            // При загрузке новой сетки название кнопки не меняется
            ResetPosition();
            Reset();
            ResetUI();
            Area area = new Area(fileName);
            regularGrid2D = new Grid2D(fileName, area);
            irregularGridMaker = new IrregularGridMaker(regularGrid2D);
            irregularGrid2D = irregularGridMaker.MakeUnStructedGrid();
            renderGrid = new RenderGrid(regularGrid2D, (float)OpenTkControl.ActualWidth, (float)OpenTkControl.ActualHeight);
            SetAxis();
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
        }

        private void RotateLeftClick(object sender, RoutedEventArgs e)
        {

        }

        private void MoveLeftClick(object sender, RoutedEventArgs e)
        {

            horOffset += speedHor * speedTranslate;
            renderGrid.translate = Matrix4.CreateTranslation(horOffset, verOffset, 0);

            mouse_horOffset -= speedHor * speedTranslate;
            rtranslate = Matrix4.CreateTranslation(mouse_horOffset, mouse_verOffset, 0);
        }

        private void MoveRightClick(object sender, RoutedEventArgs e)
        {
            horOffset -= speedHor * speedTranslate;
            renderGrid.translate = Matrix4.CreateTranslation(horOffset, verOffset, 0);

            mouse_horOffset += speedHor * speedTranslate;
            rtranslate = Matrix4.CreateTranslation(mouse_horOffset, mouse_verOffset, 0);
        }

        private void MoveDownClick(object sender, RoutedEventArgs e)
        {
            verOffset += speedVer * speedTranslate;
            renderGrid.translate = Matrix4.CreateTranslation(horOffset, verOffset, 0);

            mouse_verOffset -= speedVer * speedTranslate;
            rtranslate = Matrix4.CreateTranslation(mouse_horOffset, mouse_verOffset, 0);
        }

        private void MoveUpClick(object sender, RoutedEventArgs e)
        {
            verOffset -= speedVer * speedTranslate;
            renderGrid.translate = Matrix4.CreateTranslation(horOffset, verOffset, 0);

            mouse_verOffset += speedVer * speedTranslate;
            rtranslate = Matrix4.CreateTranslation(mouse_horOffset, mouse_verOffset, 0);
        }

        private void ZoomInClick(object sender, RoutedEventArgs e)
        {
            if (renderGrid.indent >= -0.5f)
                renderGrid.indent -= speedZoom;
            //MessageBox.Show(BufferClass.indent.ToString());
            //BufferClass.scaleX *= BufferClass.speedZoom;
            //BufferClass.scaleY *= BufferClass.speedZoom;
            //BufferClass.scale = Matrix4.CreateScale(BufferClass.scaleX, BufferClass.scaleY, 1);

            //BufferClass.mouse_scaleX /= BufferClass.speedZoom;
            //BufferClass.mouse_scaleY /= BufferClass.speedZoom;
        }

        private void ZoomOutClick(object sender, RoutedEventArgs e)
        {
            renderGrid.indent += speedZoom;
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
            renderGrid.linesSize = (float)e.NewValue;
        }

        private void PointsSizeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (renderGrid == null) return;
            renderGrid.pointsSize = (float)e.NewValue;
        }

        private void PointsColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (renderGrid == null) return;
            renderGrid.pointsColor = ColorByteToFloat((Color)e.NewValue);
        }

        private void LinesColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (renderGrid == null) return;
            renderGrid.linesColor = ColorByteToFloat((Color)e.NewValue);
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
            renderGrid.wireframeMode = true;
        }

        private void WiremodeUnChecked(object sender, RoutedEventArgs e)
        {
            if (renderGrid == null) return;
            renderGrid.wireframeMode = false;
        }

        private void DrawRemovedLinesChecked(object sender, RoutedEventArgs e)
        {
            if (renderGrid == null) return;
            renderGrid.drawRemovedLinesMode = true;
        }

        private void DrawRemovedLinesUnChecked(object sender, RoutedEventArgs e)
        {
            if (renderGrid == null) return;
            renderGrid.drawRemovedLinesMode = false;
        }

        private void MakeUnstructedGridClick(object sender, RoutedEventArgs e)
        {
            if (!unstructedGridMode)
            {
                unstructedGridMode = true;
                BuildGridButton.Content = "Построить регулярную сетку";
            }
            else
            {
                unstructedGridMode = false;
                BuildGridButton.Content = "Построить нерегулярную сетку";
            }
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
                irregularGridMaker.maxAR = maxAr;
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
            renderGrid.showGrid = true;
        }

        private void ShowGridUnChecked(object sender, RoutedEventArgs e)
        {
            if (renderGrid == null) return;
            renderGrid.showGrid = false;
        }
    }
}
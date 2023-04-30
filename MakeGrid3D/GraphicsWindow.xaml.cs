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
using MakeGrid3D.Pages;
using System.Windows.Media.Media3D;
using System.Reflection;

namespace MakeGrid3D
{
    /// <summary>
    /// Interaction logic for GraphicsWindow.xaml
    /// </summary>

    static public class BufferClass
    {
        static public Matrix4 translate = Matrix4.Identity;
        static public Matrix4 scale = Matrix4.Identity;
        static public Matrix4 rtranslate = Matrix4.Identity;
        static public Matrix4 rscale = Matrix4.Identity; // TODO: не используется вообще все матрицы scale

        static public float horOffset = 0;
        static public float verOffset = 0;
        static public float scaleX = 1;
        static public float scaleY = 1;

        static public float mouse_horOffset = 0;
        static public float mouse_verOffset = 0;
        static public float mouse_scaleX = 1;
        static public float mouse_scaleY = 1;

        static public float speedTranslate = Default.speedTranslate;
        static public float speedZoom = Default.speedZoom;
        static public float speedHor = 0;
        static public float speedVer = 0;

        static public float linesSize = Default.linesSize;
        static public float pointsSize = Default.pointsSize;
        static public Color4 linesColor = Default.linesColor;
        static public Color4 pointsColor = Default.pointsColor;
        static public Color4 bgColor = Default.bgColor;
        static public bool wireframeMode = Default.wireframeMode;
        static public bool showGrid = Default.showGrid;
        static public bool drawRemovedLinesMode = Default.drawRemovedLinesMode;
        static public bool unstructedGridMode = Default.unstructedGridMode;

        static public string fileName = "C:\\Users\\artor\\OneDrive\\Рабочий стол\\тесты на практику\\TEST2.txt";
        static public float maxAR = (float)Default.maxAR_width / Default.maxAR_height;
        static public bool rebuildUnStructedGrid = false;
        static public float indent = Default.indent;

        static public void Reset()
        {
            speedTranslate = Default.speedTranslate;
            speedZoom = Default.speedZoom;
            linesSize = Default.linesSize;
            pointsSize = Default.pointsSize;
            linesColor = Default.linesColor;
            pointsColor = Default.pointsColor;
            bgColor = Default.bgColor;
            wireframeMode = Default.wireframeMode;
            showGrid = Default.showGrid;
            drawRemovedLinesMode = Default.drawRemovedLinesMode;
            maxAR = (float)Default.maxAR_width / Default.maxAR_height;
        }
        static public void ResetPosition()
        {
            translate = Matrix4.Identity;
            scale = Matrix4.Identity;
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
            indent = Default.indent;
        }
    }

    public partial class GraphicsWindow : Window
    {
        RenderGrid renderGrid;
        IrregularGridMaker irregularGridMaker;
        Grid2D regularGrid2D, irregularGrid2D;
        Mesh axis;
        Mesh? selectedElemMesh = null;
        Mesh? selectedElemLines = null;
        Matrix4 projectionSelectedElem = Matrix4.Identity;
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
        }

        private void OpenTkControl_OnLoad(object sender, RoutedEventArgs e)
        {
            Area area = new Area(BufferClass.fileName);
            regularGrid2D = new Grid2D(BufferClass.fileName, area);
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
            BufferClass.speedHor = (renderGrid.Right - renderGrid.Left) * 0.01f;
            BufferClass.speedVer = (renderGrid.Top - renderGrid.Bottom) * 0.01f;
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

            GL.ClearColor(BufferClass.bgColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            // Чтобы работали прозрачные цвета
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend);
            renderGrid.RenderFrame();
            if (BufferClass.unstructedGridMode)
            {
                renderGrid.Grid2D = irregularGrid2D;
                BufferClass.unstructedGridMode = false;
            }
            if (BufferClass.drawRemovedLinesMode)
            {
                renderGrid.Grid2D = regularGrid2D;
                renderGrid.shader.DashedLines(true);
                renderGrid.RenderFrame(drawArea:false, drawNodes:false);
                renderGrid.shader.DashedLines(false);
                renderGrid.Grid2D = irregularGrid2D;
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
            Vector4 u = v * BufferClass.rscale * BufferClass.rtranslate;

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
            GL.ClearColor(new Color4(BufferClass.bgColor.R / 2, BufferClass.bgColor.G / 2, BufferClass.bgColor.B / 2, BufferClass.bgColor.A));
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
                if (!BufferClass.wireframeMode)
                {
                    selectedElemMesh.Use();
                    renderGrid.shader.SetColor4("current_color", Default.areaColors[selectedElem.wi]);
                    selectedElemMesh.DrawElems(6, 0, PrimitiveType.Triangles);
                }

                selectedElemLines.Use();
                renderGrid.shader.SetColor4("current_color", BufferClass.linesColor);
                selectedElemLines.DrawElems(8, 0, PrimitiveType.Lines);
                renderGrid.shader.SetColor4("current_color", BufferClass.pointsColor);
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
                string fileName = dialog.FileName;
                BufferClass.fileName = fileName;
            }
            renderGrid.CleanUp();
            BufferClass.unstructedGridMode = false;
            // TODO:
            // При загрузке новой сетки название кнопки не меняется
            BufferClass.ResetPosition();
            Area area = new Area(BufferClass.fileName);
            regularGrid2D = new Grid2D(BufferClass.fileName, area);
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
    }
}
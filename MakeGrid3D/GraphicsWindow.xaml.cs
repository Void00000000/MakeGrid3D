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

namespace MakeGrid3D
{
    /// <summary>
    /// Interaction logic for GraphicsWindow.xaml
    /// </summary>

    static public class BufferClass
    {
        static public Matrix4 translate = Matrix4.Identity;
        static public Matrix4 scale = Matrix4.Identity;
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
        static public bool drawRemovedLinesMode = Default.drawRemovedLinesMode;
        static public bool unstructedGridMode = Default.unstructedGridMode;

        static public string fileName = "C:\\Users\\artor\\OneDrive\\Рабочий стол\\тесты на практику\\Grid2D_1.txt";
        static public float maxAR = (float)Default.maxAR_width / Default.maxAR_height;
        static public bool rebuildUnStructedGrid = false;

        static public void Reset()
        {
            speedTranslate = Default.speedTranslate;
            speedZoom = Default.speedZoom;
            linesSize = Default.linesSize;
            pointsSize = Default.pointsSize;
            linesColor = Default.linesColor;
            pointsColor = Default.pointsColor;
            bgColor = Default.bgColor;
            wireframeMode= Default.wireframeMode;
        }
    }

    public partial class GraphicsWindow : Window
    {
        private RenderGrid renderGrid;
        public GraphicsWindow()
        {
            InitializeComponent();
            var settings = new GLWpfControlSettings
            {
                MajorVersion = 3,
                MinorVersion = 3
            };
            OpenTkControl.Start(settings);

            Grid2D grid2D = new Grid2D(BufferClass.fileName);
            grid2D.MakeUnStructedGrid();
            renderGrid = new RenderGrid(grid2D);
            // Множители скорости = 1 процент от ширины(высоты) мира
            BufferClass.speedHor = (renderGrid.Right - renderGrid.Left) * 0.01f;
            BufferClass.speedVer = (renderGrid.Top - renderGrid.Bottom) * 0.01f;
        }

        private void OpenTkControl_OnRender(TimeSpan obj)
        {
            if (BufferClass.rebuildUnStructedGrid)
            {
                renderGrid.RebuildUnStructedGrid();
                BufferClass.rebuildUnStructedGrid = false;
            }
            renderGrid.RenderFrame();
        }

        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            renderGrid.CleanUp();
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
            
            Matrix4 rscale = Matrix4.CreateScale(BufferClass.mouse_scaleX, BufferClass.mouse_scaleY, 1);
            Matrix4 rtranslate = Matrix4.CreateTranslation(BufferClass.mouse_horOffset, BufferClass.mouse_verOffset, 0); 
            Vector4 u = v * rscale * rtranslate;

            return new Point(u.X, u.Y);
        }

        private void OpenTkControl_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(OpenTkControl);
            Point new_position = MouseMap(position);
            double x = new_position.X;
            double y = new_position.Y;
            coordinatesTextBlock.Header = "X: " + x.ToString("0.00") + ", Y: " + y.ToString("0.00");
            
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
            Grid2D grid2D = new Grid2D(BufferClass.fileName);
            grid2D.MakeUnStructedGrid();
            renderGrid = new RenderGrid(grid2D);
        }
    }
}
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;

namespace MakeGrid3D.Pages
{
    /// <summary>
    /// Interaction logic for InstrumentPanel.xaml
    /// </summary>
    public partial class InstrumentPanel : Page
    {
        public InstrumentPanel()
        {
            InitializeComponent();
            ResetUI();
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
            
            BufferClass.horOffset += BufferClass.speedHor * BufferClass.speedTranslate;
            BufferClass.translate = Matrix4.CreateTranslation(BufferClass.horOffset, BufferClass.verOffset, 0);

            BufferClass.mouse_horOffset -= BufferClass.speedHor * BufferClass.speedTranslate;
            BufferClass.rtranslate = Matrix4.CreateTranslation(BufferClass.mouse_horOffset, BufferClass.mouse_verOffset, 0);
        }

        private void MoveRightClick(object sender, RoutedEventArgs e)
        {
            BufferClass.horOffset -= BufferClass.speedHor * BufferClass.speedTranslate;
            BufferClass.translate = Matrix4.CreateTranslation(BufferClass.horOffset, BufferClass.verOffset, 0);

            BufferClass.mouse_horOffset += BufferClass.speedHor * BufferClass.speedTranslate;
            BufferClass.rtranslate = Matrix4.CreateTranslation(BufferClass.mouse_horOffset, BufferClass.mouse_verOffset, 0);
        }

        private void MoveDownClick(object sender, RoutedEventArgs e)
        {
            BufferClass.verOffset += BufferClass.speedVer * BufferClass.speedTranslate;
            BufferClass.translate = Matrix4.CreateTranslation(BufferClass.horOffset, BufferClass.verOffset, 0);

            BufferClass.mouse_verOffset -= BufferClass.speedVer * BufferClass.speedTranslate;
            BufferClass.rtranslate = Matrix4.CreateTranslation(BufferClass.mouse_horOffset, BufferClass.mouse_verOffset, 0);
        }

        private void MoveUpClick(object sender, RoutedEventArgs e)
        {
            BufferClass.verOffset -= BufferClass.speedVer * BufferClass.speedTranslate;
            BufferClass.translate = Matrix4.CreateTranslation(BufferClass.horOffset, BufferClass.verOffset, 0);

            BufferClass.mouse_verOffset += BufferClass.speedVer * BufferClass.speedTranslate;
            BufferClass.rtranslate = Matrix4.CreateTranslation(BufferClass.mouse_horOffset, BufferClass.mouse_verOffset, 0);
        }

        private void ZoomInClick(object sender, RoutedEventArgs e)
        {
            if (BufferClass.indent >= -0.5f)
                BufferClass.indent -= BufferClass.speedZoom;
            //MessageBox.Show(BufferClass.indent.ToString());
            //BufferClass.scaleX *= BufferClass.speedZoom;
            //BufferClass.scaleY *= BufferClass.speedZoom;
            //BufferClass.scale = Matrix4.CreateScale(BufferClass.scaleX, BufferClass.scaleY, 1);

            //BufferClass.mouse_scaleX /= BufferClass.speedZoom;
            //BufferClass.mouse_scaleY /= BufferClass.speedZoom;
        }

        private void ZoomOutClick(object sender, RoutedEventArgs e)
        {
            BufferClass.indent += BufferClass.speedZoom;
            //BufferClass.scaleX /= BufferClass.speedZoom;
            //BufferClass.scaleY /= BufferClass.speedZoom;
            //BufferClass.scale = Matrix4.CreateScale(BufferClass.scaleX, BufferClass.scaleY, 1);

            //BufferClass.mouse_scaleX *= BufferClass.speedZoom;
            //BufferClass.mouse_scaleY *= BufferClass.speedZoom;
        }

        private void ResetPositionClick(object sender, RoutedEventArgs e)
        {
            BufferClass.ResetPosition();
        }

        private void LinesSizeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BufferClass.linesSize = (float)e.NewValue;
        }

        private void PointsSizeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BufferClass.pointsSize = (float)e.NewValue;
        }

        private void PointsColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            BufferClass.pointsColor = ColorByteToFloat((Color)e.NewValue);
        }

        private void LinesColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            BufferClass.linesColor = ColorByteToFloat((Color)e.NewValue);
        }

        private void BgColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            BufferClass.bgColor = ColorByteToFloat((Color)e.NewValue);
        }

        private void SpeedTranslateChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BufferClass.speedTranslate = (float)e.NewValue;
        }

        private void SpeedZoomChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BufferClass.speedZoom = (float)e.NewValue;
        }

        private void ResetSettingsClick(object sender, RoutedEventArgs e)
        {
            BufferClass.Reset();
            ResetUI();
        }

        private void WiremodeChecked(object sender, RoutedEventArgs e)
        {
            BufferClass.wireframeMode = true;
        }

        private void WiremodeUnChecked(object sender, RoutedEventArgs e)
        {
            BufferClass.wireframeMode = false;
        }

        private void DrawRemovedLinesChecked(object sender, RoutedEventArgs e)
        {
            BufferClass.drawRemovedLinesMode = true;
        }

        private void DrawRemovedLinesUnChecked(object sender, RoutedEventArgs e)
        {
            BufferClass.drawRemovedLinesMode = false;
        }

        private void MakeUnstructedGridClick(object sender, RoutedEventArgs e)
        {
            if (!BufferClass.unstructedGridMode)
            {
                BufferClass.unstructedGridMode = true;
                BuildGridButton.Content = "Построить регулярную сетку";
            }
            else
            {
                BufferClass.unstructedGridMode = false;
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
                BufferClass.maxAR = maxAr;
                BufferClass.rebuildUnStructedGrid = true;
            }
            catch (Exception ex){
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
            BufferClass.showGrid = true;
        }

        private void ShowGridUnChecked(object sender, RoutedEventArgs e)
        {
            BufferClass.showGrid = false;
        }
    }
}

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

namespace MakeGrid3D.Pages
{
    /// <summary>
    /// Interaction logic for Page5.xaml
    /// </summary>
    public partial class Page5 : Page
    {
        Page4 prevPage;
        List<int> nx, ny, nz;
        List<float> qx, qy, qz;
        int indexX = 0, indexY = 0, indexZ = 0;
        bool TwoD { get; set; }
        public Page5(Page4 page4)
        {
            InitializeComponent();
            prevPage = page4;
            TwoD = prevPage.TwoD;
            if (TwoD)
            {
                NZBlock.IsEnabled = false;
                QZBlock.IsEnabled = false;
                PrevZButton.IsEnabled = false;
                NextZButton.IsEnabled = false;
                ReverseZCheckBox.IsEnabled = false;
            }

            nx = new List<int>(new int[prevPage.prevPage.prevPage.NXw - 1]);
            qx = new List<float>(new float[nx.Count]);
            ny = new List<int>(new int[prevPage.prevPage.prevPage.NYw - 1]);
            qy = new List<float>(new float[ny.Count]);
            if (!TwoD)
            {
                nz = new List<int>(new int[prevPage.prevPage.prevPage.NZw - 1]);
                qz = new List<float>(new float[nz.Count]);
            }
            XIntervalsCounterBlock.Text = $"1/{nx.Count}";
            YIntervalsCounterBlock.Text = $"1/{ny.Count}";
            if (!TwoD)
                ZIntervalsCounterBlock.Text = $"1/{nz.Count}";
        }

        private void PrevPageClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(prevPage);
        }

        private void CreateGridClick(object sender, RoutedEventArgs e)
        {
            bool TwoD = prevPage.TwoD;
            var Xw = prevPage.prevPage.Xw;
            var Yw = prevPage.prevPage.Yw;
            var Zw = prevPage.prevPage.Zw;
            var Mw = prevPage.Mw;
            var nx_ = nx;
            var ny_ = ny;
            var nz_ = nz;
            var qx_ = qx;
            var qy_ = qy;
            var qz_ = qz;
            var mats = prevPage.prevPage.AreaColors;
            GridParams gridParams = new GridParams(TwoD, Xw, Yw, Zw, Mw, nx, ny, nz, qx, qy, qz, mats);
            GraphicsWindow graphicsWindow = new GraphicsWindow(gridParams);
            graphicsWindow.Show();
            Window.GetWindow(this).Close();
        }

        private void NXChanged(object sender, TextChangedEventArgs e)
        {
            int nxi;
            bool success = int.TryParse(NXBlock.Text, out nxi);
            if (success && nxi > 0)
            {
                nx[indexX] = nxi;
            }
        }

        private void QXChanged(object sender, TextChangedEventArgs e)
        {
            float qxi;
            bool success = float.TryParse(QXBlock.Text, out qxi);
            if (success)
            {
                if (ReverseXCheckBox.IsChecked == true) qx[indexX] = -MathF.Abs(qxi);
                else qx[indexX] = MathF.Abs(qxi);
            }
        }

        private void ReverseXChecked(object sender, RoutedEventArgs e)
        {
            qx[indexX] = -MathF.Abs(qx[indexX]);
        }

        private void ReverseXUnChecked(object sender, RoutedEventArgs e)
        {
            qx[indexX] = MathF.Abs(qx[indexX]);
        }

        private void PrevXClick(object sender, RoutedEventArgs e)
        {
            indexX--;
            if (indexX < 0)
            {
                indexX = nx.Count - 1;
            }
            NXBlock.Text = nx[indexX].ToString();
            if (qx[indexX] < 0) ReverseXCheckBox.IsChecked = true; else ReverseXCheckBox.IsChecked = false;
            QXBlock.Text = MathF.Abs(qx[indexX]).ToString();
            XIntervalsCounterBlock.Text = $"{indexX + 1}/{nx.Count}";
        }

        private void NextXClick(object sender, RoutedEventArgs e)
        {
            indexX++;
            if (indexX >= nx.Count)
            {
                indexX = 0;
            }
            NXBlock.Text = nx[indexX].ToString();
            if (qx[indexX] < 0)  ReverseXCheckBox.IsChecked = true; else ReverseXCheckBox.IsChecked = false;
            QXBlock.Text = MathF.Abs(qx[indexX]).ToString();
            XIntervalsCounterBlock.Text = $"{indexX + 1}/{nx.Count}";
        }

        private void NYChanged(object sender, TextChangedEventArgs e)
        {
            int nyi;
            bool success = int.TryParse(NYBlock.Text, out nyi);
            if (success && nyi > 0)
            {
                ny[indexY] = nyi;
            }
        }

        private void QYChanged(object sender, TextChangedEventArgs e)
        {
            float qyi;
            bool success = float.TryParse(QYBlock.Text, out qyi);
            if (success)
            {
                if (ReverseYCheckBox.IsChecked == true) qy[indexY] = -MathF.Abs(qyi);
                else qy[indexY] = MathF.Abs(qyi);
            }
        }

        private void ReverseYChecked(object sender, RoutedEventArgs e)
        {
            qy[indexY] = -MathF.Abs(qy[indexY]);
        }

        private void ReverseYUnChecked(object sender, RoutedEventArgs e)
        {
            qy[indexY] = MathF.Abs(qy[indexY]);
        }

        private void PrevYClick(object sender, RoutedEventArgs e)
        {
            indexY--;
            if (indexY < 0)
            {
                indexY = ny.Count - 1;
            }
            NYBlock.Text = ny[indexY].ToString();
            if (qy[indexY] < 0) ReverseYCheckBox.IsChecked = true; else ReverseYCheckBox.IsChecked = false;
            QYBlock.Text = MathF.Abs(qy[indexY]).ToString();
            YIntervalsCounterBlock.Text = $"{indexY + 1}/{ny.Count}";
        }

        private void NextYClick(object sender, RoutedEventArgs e)
        {
            indexY++;
            if (indexY >= ny.Count)
            {
                indexY = 0;
            }
            NYBlock.Text = ny[indexY].ToString();
            if (qy[indexY] < 0) ReverseYCheckBox.IsChecked = true; else ReverseYCheckBox.IsChecked = false;
            QYBlock.Text = MathF.Abs(qy[indexY]).ToString();
            YIntervalsCounterBlock.Text = $"{indexY + 1}/{ny.Count}";
        }

        private void NZChanged(object sender, TextChangedEventArgs e)
        {
            int nzi;
            bool success = int.TryParse(NZBlock.Text, out nzi);
            if (success && nzi > 0)
            {
                nz[indexZ] = nzi;
            }
        }

        private void QZChanged(object sender, TextChangedEventArgs e)
        {
            float qzi;
            bool success = float.TryParse(QZBlock.Text, out qzi);
            if (success)
            {
                if (ReverseZCheckBox.IsChecked == true) qz[indexZ] = -MathF.Abs(qzi);
                else qz[indexZ] = MathF.Abs(qzi);
            }
        }

        private void ReverseZChecked(object sender, RoutedEventArgs e)
        {
            qz[indexZ] = -MathF.Abs(qz[indexZ]);
        }

        private void ReverseZUnChecked(object sender, RoutedEventArgs e)
        {
            qz[indexZ] = MathF.Abs(qz[indexZ]);
        }

        private void PrevZClick(object sender, RoutedEventArgs e)
        {
            indexZ--;
            if (indexZ < 0)
            {
                indexZ = nz.Count - 1;
            }
            NZBlock.Text = nz[indexZ].ToString();
            if (qz[indexZ] < 0) ReverseZCheckBox.IsChecked = true; else ReverseZCheckBox.IsChecked = false;
            QZBlock.Text = MathF.Abs(qz[indexZ]).ToString();
            ZIntervalsCounterBlock.Text = $"{indexZ + 1}/{nz.Count}";
        }

        private void NextZClick(object sender, RoutedEventArgs e)
        {
            indexZ++;
            if (indexZ >= nz.Count)
            {
                indexZ = 0;
            }
            NZBlock.Text = nz[indexZ].ToString();
            if (qz[indexZ] < 0) ReverseZCheckBox.IsChecked = true; else ReverseZCheckBox.IsChecked = false;
            QZBlock.Text = MathF.Abs(qz[indexZ]).ToString();
            ZIntervalsCounterBlock.Text = $"{indexZ + 1}/{nz.Count}";
        }
    }
}

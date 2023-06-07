using OpenTK.Mathematics;
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
    /// Interaction logic for Page3.xaml
    /// </summary>

    public partial class Page3 : Page
    {
        public Page2 prevPage { get; }
        public List<float> Xw { get; set; }
        public List<float> Yw { get; set; }
        public List<float> Zw { get; set; }
        public bool TwoD { get; set; }
        public List<Color4> AreaColors { get; set; }
        int indexXw = 0, indexYw = 0, indexZw = 0, indexAreaColors = 0;
        public Page3(Page2 page2)
        {
            InitializeComponent();
            prevPage = page2;
            TwoD = prevPage.TwoD;
            if (TwoD)
            {
                ZwBlock.IsEnabled = false;
                PrevZwButton.IsEnabled = false;
                NextZwButton.IsEnabled = false;
            }

            Xw = new List<float>(new float[prevPage.NXw]);
            Yw = new List<float>(new float[prevPage.NYw]);
            Zw = new List<float>(new float[prevPage.NZw]);
            AreaColors = new List<Color4>(prevPage.Nmats);
            for (int i = 0; i < prevPage.Nmats; i++)
                AreaColors.Add(Color4.White);
            XwCounterBlock.Text = $"1/{prevPage.NXw}";
            YwCounterBlock.Text = $"1/{prevPage.NYw}";
            if (!TwoD)
                ZwCounterBlock.Text = $"1/{prevPage.NZw}";
            MatColorCounterBlock.Text = $"1/{prevPage.Nmats}";
        }

        private void PrevPageClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(prevPage);
        }

        private void NextPageClick(object sender, RoutedEventArgs e)
        {
            bool correct_data = true;
            for (int i = 0; i < prevPage.NXw - 1; i++)
                if (Xw[i + 1] <= Xw[i]) correct_data = false;
            for (int i = 0; i < prevPage.NYw - 1; i++)
                if (Yw[i + 1] <= Yw[i]) correct_data = false;
            if (!TwoD)
                for (int i = 0; i < prevPage.NZw - 1; i++)
                    if (Zw[i + 1] <= Zw[i]) correct_data = false;
            if (correct_data)
            {
                Page4 page4 = new Page4(this);
                NavigationService.Navigate(page4);
            }
            else ErrorHandler.DataErrorMessage("Введены некорректные данные", false);
        }

        private void XwChanged(object sender, TextChangedEventArgs e)
        {
            float xwi;
            if (float.TryParse(XwBlock.Text, out xwi))
            {
                Xw[indexXw] = xwi;
            }
        }

        private void PrevXwClick(object sender, RoutedEventArgs e)
        {
            indexXw--;
            if (indexXw < 0)
            {
                indexXw = prevPage.NXw - 1;
            }
            XwBlock.Text = Xw[indexXw].ToString();
            XwCounterBlock.Text = $"{indexXw + 1}/{prevPage.NXw}";
        }

        private void NextXwClick(object sender, RoutedEventArgs e)
        {
            indexXw++;
            if (indexXw >= prevPage.NXw)
            {
                indexXw = 0;
            }
            XwBlock.Text = Xw[indexXw].ToString();
            XwCounterBlock.Text = $"{indexXw + 1}/{prevPage.NXw}";
        }

        private void YwChanged(object sender, TextChangedEventArgs e)
        {
            float ywi;
            if (float.TryParse(YwBlock.Text, out ywi))
            {
                Yw[indexYw] = ywi;
            }
        }

        private void PrevYwClick(object sender, RoutedEventArgs e)
        {
            indexYw--;
            if (indexYw < 0)
            {
                indexYw = prevPage.NYw - 1;
            }
            YwBlock.Text = Yw[indexYw].ToString();
            YwCounterBlock.Text = $"{indexYw + 1}/{prevPage.NYw}";
        }

        private void NextYwClick(object sender, RoutedEventArgs e)
        {
            indexYw++;
            if (indexYw >= prevPage.NYw)
            {
                indexYw = 0;
            }
            YwBlock.Text = Yw[indexYw].ToString();
            YwCounterBlock.Text = $"{indexYw + 1}/{prevPage.NYw}";
        }

        private void ZwChanged(object sender, TextChangedEventArgs e)
        {
            float zwi;
            if (float.TryParse(ZwBlock.Text, out zwi))
            {
                Zw[indexZw] = zwi;
            }
        }

        private void PrevZwClick(object sender, RoutedEventArgs e)
        {
            indexZw--;
            if (indexZw < 0)
            {
                indexZw = prevPage.NZw - 1;
            }
            ZwBlock.Text = Zw[indexZw].ToString();
            ZwCounterBlock.Text = $"{indexZw + 1}/{prevPage.NZw}";
        }


        private void NextZwClick(object sender, RoutedEventArgs e)
        {
            indexZw++;
            if (indexZw >= prevPage.NZw)
            {
                indexZw = 0;
            }
            ZwBlock.Text = Zw[indexZw].ToString();
            ZwCounterBlock.Text = $"{indexZw + 1}/{prevPage.NZw}";
        }

        private void MatColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            AreaColors[indexAreaColors] = GraphicsWindow.ColorByteToFloat((Color)e.NewValue);
        }

        private void PrevMatColorClick(object sender, RoutedEventArgs e)
        {
            indexAreaColors--;
            if (indexAreaColors < 0)
            {
                indexAreaColors = prevPage.Nmats - 1;
            }
            MatColorPicker.SelectedColor = GraphicsWindow.ColorFloatToByte(AreaColors[indexAreaColors]);
            MatColorCounterBlock.Text = $"{indexAreaColors + 1}/{prevPage.Nmats}";
        }

        private void NextMatColorClick(object sender, RoutedEventArgs e)
        {
            indexAreaColors++;
            if (indexAreaColors >= prevPage.Nmats)
            {
                indexAreaColors = 0;
            }
            MatColorPicker.SelectedColor = GraphicsWindow.ColorFloatToByte(AreaColors[indexAreaColors]);
            MatColorCounterBlock.Text = $"{indexAreaColors + 1}/{prevPage.Nmats}";
        }
    }
}

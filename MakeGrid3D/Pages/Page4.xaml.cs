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
    /// Interaction logic for Page4.xaml
    /// </summary>
    public partial class Page4 : Page
    {
        public Page3 prevPage { get; }
        public List<SubArea3D> Mw { get; set; }
        int indexArea = 0;
        public bool TwoD { get; set; }
        public Page4(Page3 page3)
        {
            InitializeComponent();
            prevPage = page3;
            TwoD = prevPage.TwoD;
            if (TwoD)
            {
                NZ1DownMenu.IsEnabled = false;
                NZ2DownMenu.IsEnabled = false;
            }
            Mw = new List<SubArea3D>(prevPage.prevPage.Nareas);
            for (int i = 0; i < prevPage.prevPage.Nareas; i++)
                Mw.Add(new SubArea3D(-1, -1, -1, -1, -1, -1, -1));

            for (int i = 0; i < prevPage.Xw.Count; i++)
                NX1DownMenu.Items.Add($"{i + 1}| {prevPage.Xw[i]}");
            for (int i = 0; i < prevPage.Xw.Count; i++)
                NX2DownMenu.Items.Add($"{i + 1}| {prevPage.Xw[i]}");
            for (int i = 0; i < prevPage.Yw.Count; i++)
                NY1DownMenu.Items.Add($"{i + 1}| {prevPage.Yw[i]}");
            for (int i = 0; i < prevPage.Yw.Count; i++)
                NY2DownMenu.Items.Add($"{i + 1}| {prevPage.Yw[i]}");
            for (int i = 0; i < prevPage.Zw.Count; i++)
                NZ1DownMenu.Items.Add($"{i + 1}| {prevPage.Zw[i]}");
            for (int i = 0; i < prevPage.Zw.Count; i++)
                NZ2DownMenu.Items.Add($"{i + 1}| {prevPage.Zw[i]}");
            for (int i = 0; i < prevPage.AreaColors.Count; i++)
                WiDownMenu.Items.Add($"{i + 1}");
            NX1DownMenu.Items.Add(string.Empty);
            NX2DownMenu.Items.Add(string.Empty);
            NY1DownMenu.Items.Add(string.Empty);
            NY2DownMenu.Items.Add(string.Empty);
            NZ1DownMenu.Items.Add(string.Empty);
            NZ2DownMenu.Items.Add(string.Empty);
            WiDownMenu.Items.Add(string.Empty);
            AreasCounterBlock.Text = $"1/{prevPage.prevPage.Nareas}";
        }

        private void PrevPageClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(prevPage);
        }

        private void NextPageClick(object sender, RoutedEventArgs e)
        {
            foreach (SubArea3D subArea3D in Mw)
            {
                if (subArea3D.wi < 0 || subArea3D.nx1 < 0 || subArea3D.nx2 < 0 || subArea3D.ny1 < 0
                    || subArea3D.ny2 < 0 || (!TwoD && (subArea3D.nz1 < 0 || subArea3D.nz2 < 0)))
                {
                    ErrorHandler.DataErrorMessage("Введены некорректные данные", false);
                    return;
                } 
            }
            Page5 page5 = new Page5(this);
            NavigationService.Navigate(page5);
        }

        public string GetUntilOrEmpty(string text, string stopAt = "-")
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

                if (charLocation > 0)
                {
                    return text.Substring(0, charLocation);
                }
            }
            return String.Empty;
        }

        private void NX1Changed(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            string selectedItem = comboBox.SelectedItem.ToString();
            int nx1;
            if (selectedItem == string.Empty) 
                nx1 = -1;
            else
                nx1 = int.Parse(GetUntilOrEmpty(selectedItem, "|")) - 1;
            Mw[indexArea] = new SubArea3D(Mw[indexArea].wi, nx1, Mw[indexArea].nx2, Mw[indexArea].ny1, Mw[indexArea].ny2,
                                          Mw[indexArea].nz1, Mw[indexArea].nz2);
        }

        private void NX2Changed(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            string selectedItem = comboBox.SelectedItem.ToString();
            int nx2;
            if (selectedItem == string.Empty)
                nx2 = -1;
            else
                nx2 = int.Parse(GetUntilOrEmpty(selectedItem, "|")) - 1;
            Mw[indexArea] = new SubArea3D(Mw[indexArea].wi, Mw[indexArea].nx1, nx2, Mw[indexArea].ny1, Mw[indexArea].ny2,
                                          Mw[indexArea].nz1, Mw[indexArea].nz2);
        }
        private void NY1Changed(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            string selectedItem = comboBox.SelectedItem.ToString();
            int ny1;
            if (selectedItem == string.Empty)
                ny1 = -1;
            else
                ny1 = int.Parse(GetUntilOrEmpty(selectedItem, "|")) - 1;
            Mw[indexArea] = new SubArea3D(Mw[indexArea].wi, Mw[indexArea].nx1, Mw[indexArea].nx2, ny1, Mw[indexArea].ny2,
                                          Mw[indexArea].nz1, Mw[indexArea].nz2);
        }
        private void NY2Changed(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            string selectedItem = comboBox.SelectedItem.ToString();
            int ny2;
            if (selectedItem == string.Empty)
                ny2 = -1;
            else
                ny2 = int.Parse(GetUntilOrEmpty(selectedItem, "|")) - 1;
            Mw[indexArea] = new SubArea3D(Mw[indexArea].wi, Mw[indexArea].nx1, Mw[indexArea].nx2, Mw[indexArea].ny1, ny2,
                                          Mw[indexArea].nz1, Mw[indexArea].nz2);
        }
        private void NZ1Changed(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            string selectedItem = comboBox.SelectedItem.ToString();
            int nz1;
            if (selectedItem == string.Empty)
                nz1 = -1;
            else
                nz1 = int.Parse(GetUntilOrEmpty(selectedItem, "|")) - 1;
            Mw[indexArea] = new SubArea3D(Mw[indexArea].wi, Mw[indexArea].nx1, Mw[indexArea].nx2, Mw[indexArea].ny1, Mw[indexArea].ny2,
                                          nz1, Mw[indexArea].nz2);
        }
        private void NZ2Changed(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            string selectedItem = comboBox.SelectedItem.ToString();
            int nz2;
            if (selectedItem == string.Empty)
                nz2 = -1;
            else
                nz2 = int.Parse(GetUntilOrEmpty(selectedItem, "|")) - 1;
            Mw[indexArea] = new SubArea3D(Mw[indexArea].wi, Mw[indexArea].nx1, Mw[indexArea].nx2, Mw[indexArea].ny1, Mw[indexArea].ny2,
                                          Mw[indexArea].nz1, nz2);
        }
       
        private void WiChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            string selectedItem = comboBox.SelectedItem.ToString();
            int wi;
            if (selectedItem == string.Empty)
                wi = -1;
            else
                wi = int.Parse(selectedItem) - 1;
            Mw[indexArea] = new SubArea3D(wi, Mw[indexArea].nx1, Mw[indexArea].nx2, Mw[indexArea].ny1, Mw[indexArea].ny2,
                                          Mw[indexArea].nz1, Mw[indexArea].nz2);
        }

        private void SetSubAreaToComboBoxes()
        {
            int nx1 = Mw[indexArea].nx1;
            if (nx1 < 0) NX1DownMenu.SelectedItem = string.Empty;
            else
            {
                float x1 = prevPage.Xw[nx1];
                NX1DownMenu.SelectedItem = $"{nx1 + 1}| {x1}";
            }
            int nx2 = Mw[indexArea].nx2;
            if (nx2 < 0) NX2DownMenu.SelectedItem = string.Empty;
            else
            {
                float x2 = prevPage.Xw[nx2];
                NX2DownMenu.SelectedItem = $"{nx2 + 1}| {x2}";
            }
            int ny1 = Mw[indexArea].ny1;
            if (ny1 < 0) NY1DownMenu.SelectedItem = string.Empty;
            else
            {
                float y1 = prevPage.Yw[ny1];
                NY1DownMenu.SelectedItem = $"{ny1 + 1}| {y1}";
            }
            int ny2 = Mw[indexArea].ny2;
            if (ny2 < 0) NY2DownMenu.SelectedItem = string.Empty;
            else
            {
                float y2 = prevPage.Yw[ny2];
                NY2DownMenu.SelectedItem = $"{ny2 + 1}| {y2}";
            }
            int nz1 = Mw[indexArea].nz1;
            if (nz1 < 0) NZ1DownMenu.SelectedItem = string.Empty;
            else
            {
                float z1 = prevPage.Zw[nz1];
                NZ1DownMenu.SelectedItem = $"{nz1 + 1}| {z1}";
            }
            int nz2 = Mw[indexArea].nz2;
            if (nz2 < 0) NZ2DownMenu.SelectedItem = string.Empty;
            else
            {
                float z2 = prevPage.Zw[nz2];
                NZ2DownMenu.SelectedItem = $"{nz2 + 1}| {z2}";
            }
            int wi = Mw[indexArea].wi;
            if (wi < 0) WiDownMenu.SelectedItem = string.Empty;
            else
            {
                WiDownMenu.SelectedItem = $"{wi + 1}";
            }
        }

        private void PrevAreaClick(object sender, RoutedEventArgs e)
        {
            indexArea--;
            if (indexArea < 0)
            {
                indexArea = prevPage.prevPage.Nareas - 1;
            }
            SetSubAreaToComboBoxes();
            AreasCounterBlock.Text = $"{indexArea + 1}/{prevPage.prevPage.Nareas}";
        }

        private void NextAreaClick(object sender, RoutedEventArgs e)
        {
            indexArea++;
            if (indexArea >= prevPage.prevPage.Nareas)
            {
                indexArea = 0;
            }
            SetSubAreaToComboBoxes();
            AreasCounterBlock.Text = $"{indexArea + 1}/{prevPage.prevPage.Nareas}";
        }
    }
}

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
    /// Interaction logic for Page2.xaml
    /// </summary>
    public partial class Page2 : Page
    {
        public int NXw { get; set; }
        public int NYw { get; set; }
        public int NZw { get; set; }
        public int Nareas { get; set; }
        public int Nmats { get; set; }
        public bool TwoD { get; set; }
        public Page2()
        {
            InitializeComponent();
            TwoD = false;
        }

        private void PrevPageClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Page1.xaml", UriKind.Relative));
        }

        private void NextPageClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NXw = int.Parse(NXwBlock.Text);
                NYw = int.Parse(NYwBlock.Text);
                if (!TwoD)
                    NZw = int.Parse(NZwBlock.Text);
                Nareas = int.Parse(NareasBlock.Text);
                Nmats = int.Parse(NmatsBlock.Text);
                if (NXw < 2 || NYw < 2 ||  ( !TwoD && NZw < 2) || Nareas < 1 || Nmats < 1)
                    throw new Exception();
            }
            catch { ErrorHandler.DataErrorMessage("Введены некорректные данные", false); return; }
            Page3 page3 = new Page3(this);
            NavigationService.Navigate(page3);
        }

        private void Mode3DChecked(object sender, RoutedEventArgs e)
        {
            TwoD = false;
            if (NZwBlock != null)
                NZwBlock.IsEnabled = true;
        }

        private void Mode3DUnChecked(object sender, RoutedEventArgs e)
        {
            TwoD = true;
            if (NZwBlock != null)
                NZwBlock.IsEnabled = false;
        }
    }
}

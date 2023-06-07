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
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        public Page1()
        {
            InitializeComponent();
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
                string fileName = dialog.FileName;
                GraphicsWindow graphicsWindow = new GraphicsWindow(fileName);
                graphicsWindow.Show();
                Window.GetWindow(this).Close();
            }
        }

        private void NextPageClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Page2.xaml", UriKind.Relative));
        }
    }
}

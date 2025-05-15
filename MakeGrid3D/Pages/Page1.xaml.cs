using MakeGrid3D.Solver;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

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
            Test1 test = new Test1();
            test.CreateTest();
            bool isSuccess = FEMSolver2D.Instance.Initialize(test.Grid, test.FemParams,
                test.Bc1, test.Bc2, test.Bc3);
            if (!isSuccess)
            {
                LogService.LogWarning("Не удалось создать T матрицу, т.к. есть перехлесты");
            }
            else
            {
                List<double> q = FEMSolver2D.Instance.Solve();
                LogService.LogVector(q);
            }
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

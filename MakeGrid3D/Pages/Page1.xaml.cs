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
            Test2_2D test = new Test2_2D();
            test.CreateTest();
            bool isSuccess = FEMSolver2D.Instance.Initialize(test.Grid, test.FemParams,
                test.Bc1, test.Bc2, test.Bc3, test.T);
            if (!isSuccess)
            {
                LogService.LogWarning("Не удалось создать T матрицу, т.к. есть перехлесты");
            }
            else
            {
                var qList = FEMSolver2D.Instance.Solve();
                int J = test.T.Count - 1;
                for (int i = 0; i < test.Grid.Nnodes; i++)
                {
                    double x = test.Grid.XY[i].X;
                    double y = test.Grid.XY[i].Y;
                    double uh = qList[J][i];
                    double u = test.U(x, y, test.T[J]);
                    double abs = Math.Abs(u - uh) / u * 100;

                    string frmt = "e15";
                    LogService.Log(i + 1 + " ");
                    LogService.Log(x.ToString() + " ");
                    LogService.Log(y.ToString() + " ");
                    LogService.Log(uh.ToString(frmt) + " ");
                    LogService.Log(u.ToString(frmt) + " ");
                    LogService.Log(abs.ToString("F3") + " ");
                    LogService.Log("\n");
                }
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

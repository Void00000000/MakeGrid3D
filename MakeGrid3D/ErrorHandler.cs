using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MakeGrid3D
{
    public class BelowZeroException : Exception
    {
        public BelowZeroException()
        {
        }
        public BelowZeroException(string message)
            : base(message)
        {
        }
    }

    internal static class ErrorHandler
    {
        public static void FileReadingErrorMessage(string message, bool shutdown=true)
        {
            MessageBox.Show(message, "Ошибка чтения файла");
            if (shutdown)
                Application.Current.Shutdown();
        }
        public static void BuildingErrorMessage(string message, bool shutdown=true)
        {
            MessageBox.Show(message, "Ошибка сборки");
            if (shutdown)
                Application.Current.Shutdown();
        }
        public static void DataErrorMessage(string message, bool shutdown = true)
        {
            MessageBox.Show(message, "Введены некорректные данные");
            if (shutdown)
                Application.Current.Shutdown();
        }
    }
}

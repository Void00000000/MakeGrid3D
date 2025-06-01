global using FunctionW = System.Func<int, double>;
global using FunctionWXY = System.Func<int, double, double, double>;
global using FunctionWXYT = System.Func<int, double, double, double, double>;
global using FunctionXYT = System.Func<double, double, double, double>;
global using FunctionXY = System.Func<double, double, double>;

namespace MakeGrid3D.Solver
{
    /// <summary>
    /// Параметры краевой задачи (лямбда, гамма, сигма, хи, параметры краевых условий). 
    /// </summary>
    public class FEMParams2D
    {
        /// <summary>
        /// Параметр лямбда. 
        /// </summary>
        public FunctionW Lambda { get; set; }

        /// <summary>
        /// Параметр гамма. 
        /// </summary>
        public FunctionW Gamma { get; set; }

        /// <summary>
        /// Параметр сигма. 
        /// </summary>
        public FunctionW Sigma { get; set; }

        /// <summary>
        /// Параметр хи. 
        /// </summary>
        public FunctionW Chi { get; set; }

        /// <summary>
        /// Функция правой части. 
        /// </summary>
        public FunctionWXYT F { get; set; }

        /// <summary>
        /// Функция первого краевого условия. 
        /// </summary>
        public FunctionWXYT Ug { get; set; }

        /// <summary>
        /// Функция второго краевого условия. 
        /// </summary>
        public FunctionWXYT Theta { get; set; }

        /// <summary>
        /// Параметр бета 3-го краевого условия. 
        /// </summary>
        public FunctionW Beta { get; set; }

        /// <summary>
        /// Функция третьего краевого условия. 
        /// </summary>
        public FunctionWXYT Ubeta { get; set; }

        /// <summary>
        /// Первое начальное условие (значение функции на нулевом слое). 
        /// </summary>
        public FunctionWXY U0 { get; set; }

        /// <summary>
        /// Второе начальное условие (значение производной функции на нулевом слое). 
        /// </summary>
        public FunctionWXY DU0 { get; set; }

        /// <summary>
        /// Второе начальное условие (значение функции на первом слое). 
        /// </summary>
        public FunctionWXY U1 { get; set; }
    }

    /// <summary>
    /// Граница сетки. 
    /// </summary>
    public struct Boundary2D
    {
        /// <summary>
        /// Номер границы. 
        /// </summary>
        public int Si { get; set; }

        // Глоабальные номер узлов грани
        public int N1 { get; set; }
        public int N2 { get; set; }

        public Boundary2D(int si, int n1, int n2) 
        {
            Si = si;
            N1 = n1;
            N2 = n2;
        }
    }
}

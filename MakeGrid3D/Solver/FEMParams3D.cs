global using FunctionWXYZ = System.Func<int, double, double, double, double>;
global using FunctionWXYZT = System.Func<int, double, double, double, double,double>;
global using FunctionXYZT = System.Func<double, double, double, double, double>;
global using FunctionXYZ = System.Func<double, double, double, double>;

namespace MakeGrid3D.Solver
{
    /// <summary>
    /// Параметры краевой задачи (лямбда, гамма, сигма, хи, параметры краевых условий). 
    /// </summary>
    public class FEMParams3D
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
        public FunctionWXYZT F { get; set; }

        /// <summary>
        /// Функция первого краевого условия. 
        /// </summary>
        public FunctionWXYZT Ug { get; set; }

        /// <summary>
        /// Функция второго краевого условия. 
        /// </summary>
        public FunctionWXYZT Theta { get; set; }

        /// <summary>
        /// Параметр бета 3-го краевого условия. 
        /// </summary>
        public FunctionW Beta { get; set; }

        /// <summary>
        /// Функция третьего краевого условия. 
        /// </summary>
        public FunctionWXYZT Ubeta { get; set; }

        /// <summary>
        /// Первое начальное условие (значение функции на нулевом слое). 
        /// </summary>
        public FunctionWXYZ U0 { get; set; }

        /// <summary>
        /// Второе начальное условие (значение производной функции на нулевом слое). 
        /// </summary>
        public FunctionWXYZ DU0 { get; set; }

        /// <summary>
        /// Второе начальное условие (значение функции на первом слое). 
        /// </summary>
        public FunctionWXYZ U1 { get; set; }
    }

    /// <summary>
    /// Граница сетки. 
    /// </summary>
    public struct Boundary3D
    {
        /// <summary>
        /// Номер границы. 
        /// </summary>
        public int Si { get; set; }

        // Глоабальные номер узлов грани
        public int N1 { get; set; }
        public int N2 { get; set; }
        public int N3 { get; set; }
        public int N4 { get; set; }

        public Boundary3D(int si, int n1, int n2, int n3, int n4)
        {
            Si = si;
            N1 = n1;
            N2 = n2;
            N3 = n3;
            N4 = n4;
        }
    }
}

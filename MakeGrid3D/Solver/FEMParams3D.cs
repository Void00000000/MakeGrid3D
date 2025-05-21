global using FunctionXYZ = System.Func<int, double, double, double, double>;
global using FunctionXYZT = System.Func<int, double, double, double, double,double>;

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
        public Function Lambda { get; set; }

        /// <summary>
        /// Параметр сигма. 
        /// </summary>
        public Function Sigma { get; set; }

        /// <summary>
        /// Параметр хи. 
        /// </summary>
        public Function Chi { get; set; }

        /// <summary>
        /// Функция правой части. 
        /// </summary>
        public FunctionXYZT F { get; set; }

        /// <summary>
        /// Функция первого краевого условия. 
        /// </summary>
        public FunctionXYZT Ug { get; set; }

        /// <summary>
        /// Функция второго краевого условия. 
        /// </summary>
        public FunctionXYZT Theta { get; set; }

        /// <summary>
        /// Параметр бета 3-го краевого условия. 
        /// </summary>
        public Function Beta { get; set; }

        /// <summary>
        /// Функция третьего краевого условия. 
        /// </summary>
        public FunctionXYZT Ubeta { get; set; }

        /// <summary>
        /// Первое начальное условие (значение функции на нулевом слое). 
        /// </summary>
        public FunctionXYZ U0 { get; set; }

        /// <summary>
        /// Второе начальное условие (значение производной функции на нулевом слое). 
        /// </summary>
        public FunctionXYZ DU0 { get; set; }

        /// <summary>
        /// Второе начальное условие (значение функции на первом слое). 
        /// </summary>
        public FunctionXYZ U1 { get; set; }
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

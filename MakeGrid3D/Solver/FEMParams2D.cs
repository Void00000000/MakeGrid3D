global using Function = System.Func<int, double>;
global using FunctionXY = System.Func<int, double, double, double>;
global using FunctionXYT = System.Func<int, double, double, double, double>;

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
        public Function Lambda { get; set; }

        /// <summary>
        /// Параметр гамма. 
        /// </summary>
        public Function Gamma { get; set; }

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
        public FunctionXYT F { get; set; }

        /// <summary>
        /// Функция первого краевого условия. 
        /// </summary>
        public FunctionXYT Ug { get; set; }

        /// <summary>
        /// Функция второго краевого условия. 
        /// </summary>
        public FunctionXYT Theta { get; set; }

        /// <summary>
        /// Параметр бета 3-го краевого условия. 
        /// </summary>
        public Function Beta { get; set; }

        /// <summary>
        /// Функция третьего краевого условия. 
        /// </summary>
        public FunctionXYT Ubeta { get; set; }

        /// <summary>
        /// Первое начальное условие (значение функции на нулевом слое). 
        /// </summary>
        public FunctionXY U0 { get; set; }

        /// <summary>
        /// Второе начальное условие (значение производной функции на нулевом слое). 
        /// </summary>
        public FunctionXY DU0 { get; set; }

        /// <summary>
        /// Второе начальное условие (значение функции на первом слое). 
        /// </summary>
        public FunctionXY U1 { get; set; }
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

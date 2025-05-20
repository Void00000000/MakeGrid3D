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

        /// <summary>
        /// Номер элемента в векторе Xw, 
        /// с которого начинается начало фрагмента границы.
        /// </summary>
        public int Nx1 { get; set; }

        /// <summary>
        /// Номер элемента в векторе Xw, 
        /// с которого заканчивается фрагмент границы.
        /// </summary>
        public int Nx2 { get; set; }

        /// <summary>
        /// Номер элемента в векторе Yw, 
        /// с которого начинается начало фрагмента границы.
        /// </summary>
        public int Ny1 { get; set; }

        /// <summary>
        /// Номер элемента в векторе Yw, 
        /// с которого заканчивается фрагмента границы.
        /// </summary>
        public int Ny2 { get; set; }

        public Boundary2D(int si, int nx1, int nx2, int ny1, int ny2) 
        {
            Si = si;
            Nx1 = nx1;
            Nx2 = nx2;
            Ny1 = ny1;
            Ny2 = ny2;
        }
    }
}

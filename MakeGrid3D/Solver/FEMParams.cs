global using Func1 = System.Func<int, double>;
global using Func4 = System.Func<int, double, double, double, double>;
using System;

namespace MakeGrid3D.Solver
{
    /// <summary>
    /// Параметры краевой задачи (лямбда, гамма, сигма, хи, параметры краевых условий). 
    /// </summary>
    public class FEMParams
    {
        /// <summary>
        /// Параметр лямбда. 
        /// </summary>
        public Func1 Lambda { get; set; }

        /// <summary>
        /// Параметр сигма. 
        /// </summary>
        public Func1 Sigma { get; set; }

        /// <summary>
        /// Параметр хи. 
        /// </summary>
        public Func1 Chi { get; set; }

        /// <summary>
        /// Функция правой части. 
        /// </summary>
        public Func4 F { get; set; }

        /// <summary>
        /// Функция первого краевого условия. 
        /// </summary>
        public Func4 Ug { get; set; }

        /// <summary>
        /// Функция второго краевого условия. 
        /// </summary>
        public Func4 Theta { get; set; }

        /// <summary>
        /// Параметр бета 3-го краевого условия. 
        /// </summary>
        public Func1 Beta { get; set; }

        /// <summary>
        /// Функция третьего краевого условия. 
        /// </summary>
        public Func4 Ubeta { get; set; }
    }

    /// <summary>
    /// Граница сетки. 
    /// </summary>
    public struct Boundary
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

        public Boundary(int si, int nx1, int nx2, int ny1, int ny2) 
        {
            Si = si;
            Nx1 = nx1;
            Nx2 = nx2;
            Ny1 = ny1;
            Ny2 = ny2;
        }
    }
}

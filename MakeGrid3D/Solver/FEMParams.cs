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
        /// <param name="wi">Номер подобласти.</param>
        public double Lambda(int wi) 
        {
            return 0;
        }

        /// <summary>
        /// Параметр сигма. 
        /// </summary>
        /// <param name="wi">Номер подобласти.</param>
        public double Sigma(int wi)
        {
            return 0;
        }

        /// <summary>
        /// Параметр хи. 
        /// </summary>
        /// <param name="wi">Номер подобласти.</param>
        public double Chi(int wi)
        {
            return 0;
        }

        /// <summary>
        /// Функция правой части. 
        /// </summary>
        /// <param name="si">Номер границы.</param>
        public double F(int si, double x, double y, double z = 0)
        {
            return 0;
        }

        /// <summary>
        /// Функция первого краевого условия. 
        /// </summary>
        /// <param name="si">Номер границы.</param>
        public double Ug(int si, double x, double y, double z=0) 
        {
            return 0;
        }

        /// <summary>
        /// Функция второго краевого условия. 
        /// </summary>
        /// <param name="si">Номер границы.</param>
        public double Theta(int si, double x, double y, double z = 0)
        {
            return 0;
        }

        /// <summary>
        /// Параметр бета 3-го краевого условия. 
        /// </summary>
        /// <param name="wi">Номер подобласти.</param>
        public double Beta(int wi)
        {
            return 0;
        }

        /// <summary>
        /// Функция третьего краевого условия. 
        /// </summary>
        /// <param name="si">Номер границы.</param>
        public double Ubeta(int si, double x, double y, double z = 0)
        {
            return 0;
        }
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

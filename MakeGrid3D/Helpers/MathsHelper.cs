using System;

namespace MakeGrid3D.Helpers
{
    /// <summary>
    /// Помощник по математическим операциям. 
    /// </summary>
    public static class MathsHelper
    {
        /// <summary>
        /// Проверяет два вещественных числа на равенство. 
        /// </summary>
        /// <returns>Возвращает true, если числа равны.</returns>
        public static bool IsEqual(double a, double b) 
        {
            if (Math.Abs(a - b) < 1e-16) 
            {
                return true;
            }
            return false;
        }
    }
}

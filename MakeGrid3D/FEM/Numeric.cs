using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeGrid3D.FEM
{
    using basic_function = Func<float, float, float, float, float, float, float, float, float>;
    class Numeric
    {
        private float h = 0.01f; // step for x
        private float k = 0.01f; // step for y
        private float hx = 0.001f; // diff step for x
        private float hy = 0.001f; // diff step for y

        // Function to find the double integral value
        public float doubleIntegral(float lx, float ux, float ly, float uy, float xm, float ym, basic_function givenFunction)
        {
            const int rows = 702;
            const int cols = 702;
            int nx, ny;
            // z stores the table
            // ax[] stores the integral wrt y
            // for all x points considered
            float answer;
            float[] ax = new float[cols];
            float[][] z = new float[rows][];
            for (int i = 0; i < rows; ++i)
                z[i] = new float[cols];

            // Calculating the number of points
            // in x and y integral
            nx = (int)((ux - lx) / h + 1);
            ny = (int)((uy - ly) / k + 1);

            // Calculating the values of the table
            for (int i = 0; i < nx; ++i)
            {
                for (int j = 0; j < ny; ++j)
                {
                    z[i][j] = givenFunction(
                        lx + i * h, ly + j * k,
                        lx, ux, ly, uy, xm, ym);
                }
            }

            // Calculating the integral value
            // wrt y at each point for x
            for (int i = 0; i < nx; ++i)
            {
                ax[i] = 0;
                for (int j = 0; j < ny; ++j)
                {
                    if (j == 0 || j == ny - 1)
                        ax[i] += z[i][j];
                    else if (j % 2 == 0)
                        ax[i] += 2 * z[i][j];
                    else
                        ax[i] += 4 * z[i][j];
                }
                ax[i] *= (k / 3);
            }

            answer = 0;

            // Calculating the final integral value
            // using the integral obtained in the above step
            for (int i = 0; i < nx; ++i)
            {
                if (i == 0 || i == nx - 1)
                    answer += ax[i];
                else if (i % 2 == 0)
                    answer += 2 * ax[i];
                else
                    answer += 4 * ax[i];
            }
            answer *= (h / 3);
            return answer;
        }

        public float diff_x(float x, float y, float lx, float ux, float ly, float uy, float xm, float ym,
            basic_function givenFunction)
        {
            float f1 = givenFunction(x + hx, y, lx, ux, ly, uy, xm, ym);
            float f2 = givenFunction(x - hx, y, lx, ux, ly, uy, xm, ym);
            return (f1 - f2) / (2 * hx);
        }
        public float diff_y(float x, float y, float lx, float ux, float ly, float uy, float xm, float ym,
            basic_function givenFunction)
        {
            float f1 = givenFunction(x, y + hy, lx, ux, ly, uy, xm, ym);
            float f2 = givenFunction(x, y - hy, lx, ux, ly, uy, xm, ym);
            return (f1 - f2) / (2 * hy);
        }
    };
}

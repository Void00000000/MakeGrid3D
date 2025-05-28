using System.Collections.Generic;
using System.Linq;

namespace MakeGrid3D.Solver
{
    /// <summary>
    /// Создатель функций для МКЭ решателя.
    /// </summary>
    static class FemParamsFactory3D
    {
        public static FEMParams3D CreateFemParams(int functions_num)
        {
            var FemParams = new FEMParams3D();
            switch (functions_num)
            {
                case 1:
                    FemParams.Lambda = Functions1_3D.lambda;
                    FemParams.Chi = Functions1_3D.chi;
                    FemParams.Sigma = Functions1_3D.sigma;
                    FemParams.Ubeta = Functions1_3D.u_beta;
                    FemParams.Theta = Functions1_3D.theta;
                    FemParams.Beta = Functions1_3D.beta;
                    FemParams.Ug = Functions1_3D.u_g;
                    FemParams.F = Functions1_3D.f;
                    FemParams.U0 = Functions1_3D.u0;
                    FemParams.U1 = Functions1_3D.u1;
                    FemParams.Gamma = Functions1_3D.gamma;
                    break;
            }
            return FemParams;
        }
    }

    /// <summary>
    /// Функция xyzt для регулярной сетки.
    /// </summary>
    static class Functions1_3D
    {
        static public double chi(int wi)
        {
            return 5;
        }

        static public double sigma(int wi)
        {
            return 4;
        }

        static public double gamma(int wi)
        {
            return 3;
        }

        static public double lambda(int wi)
        {
            return 2;
        }

        static public double f(int wi, double x, double y, double z, double t)
        {
            return gamma(wi) * u(x, y, z, t) + sigma(wi) * x * y * z;
        }

        static public double u_g(int si, double x, double y, double z, double t)
        {
            switch (si)
            {
                case 0:
                    return u(x, 1, z, t);
                case 1:
                    return u(4, y, z, t);
                case 2:
                    return u(x, 4, z, t);
                case 3:
                    return u(1, y, z, t); ;
                case 4:
                    return u(x, y, 1, t);
                case 5:
                    return u(x, y, 4, t);
            }
            return 0;
        }

        static public double theta(int si, double x, double y, double z, double t)
        {
            switch (si)
            {
                case 0:
                    return -2 * x * z;
            }
            return 0;
        }

        static public double beta(int si)
        {
            return 1;
        }

        static public double u_beta(int si, double x, double y, double z, double t)
        {
            switch (si)
            {
                case 0:
                    return -(-2 * x * z + 1 * x * z);
            }
            return 0;
        }

        static public double u0(int si, double x, double y, double z)
        {
            double t0 = 0;
            return u(x, y, z, t0);
        }

        static public double u1(int si, double x, double y, double z)
        {
            double t1 = 1;
            return u(x, y, z, t1);
        }

        static public double u(double x, double y, double z, double t)
        {
            return x * y * z * t;
        }
    }

    /// <summary>
    /// Тест сетки 3x3x3.
    /// </summary>
    public class Test1_3D
    {
        public Grid3D Grid;
        public FEMParams3D FemParams;
        public List<Boundary3D> Bc1;
        public List<Boundary3D> Bc2;
        public List<Boundary3D> Bc3;
        public List<double> T = new() { 0, 1, 2, 3, 4 };

        public void CreateTest()
        {
            List<double> xw = new List<double> { 1, 4 };
            List<double> yw = new List<double> { 1, 4 };
            List<double> zw = new List<double> { 1, 4 };
            SubArea3D sub1 = new SubArea3D(0, 0, 1, 0, 1, 0, 1);
            List<SubArea3D> subs = new List<SubArea3D> { sub1 };
            int nmats = 1;
            Area3D area = new Area3D(xw, yw, zw, subs, nmats);
            List<Vector3> XYZ = new List<Vector3>()
                {
                    new Vector3(1,1,1),
                    new Vector3(2,1,1),
                    new Vector3(4,1,1),
                    new Vector3(1,2,1),
                    new Vector3(2,2,1),
                    new Vector3(4,2,1),
                    new Vector3(1,4,1),
                    new Vector3(2,4,1),
                    new Vector3(4,4,1),

                    new Vector3(1,1,2),
                    new Vector3(2,1,2),
                    new Vector3(4,1,2),
                    new Vector3(1,2,2),
                    new Vector3(2,2,2),
                    new Vector3(4,2,2),
                    new Vector3(1,4,2),
                    new Vector3(2,4,2),
                    new Vector3(4,4,2),

                    new Vector3(1,1,4),
                    new Vector3(2,1,4),
                    new Vector3(4,1,4),
                    new Vector3(1,2,4),
                    new Vector3(2,2,4),
                    new Vector3(4,2,4),
                    new Vector3(1,4,4),
                    new Vector3(2,4,4),
                    new Vector3(4,4,4)
                };
            List<Elem3D> elems = new List<Elem3D>()
                {
                    new Elem3D(0,0,1,3,4,9,10,12,13),
                    new Elem3D(0,1,2,4,5,6,7,13,14),
                    new Elem3D(0,3,4,6,7,12,13,15,16),
                    new Elem3D(0,4,5,7,8,13,14,16,17),
                    new Elem3D(0,9,10,12,13,18,19,21,22),
                    new Elem3D(0,10,11,13,14,19,20,22,23),
                    new Elem3D(0,12,13,15,16,21,22,24,25),
                    new Elem3D(0,13,14,16,17,22,23,25,26)
                };
            int nx = 3;
            int ny = 3;
            int nz = 3;

            ByteMat3D IJK = new ByteMat3D(nx);
            for (int i = 0; i < nx; i++)
            {
                IJK.Add(new ByteMat2D(ny));
                for (int j = 0; j < ny; j++)
                {
                    IJK[i].Add(new List<NodeType>(nz));
                    for (int k = 0; k < nz; k++)
                        IJK[i][j].Add(NodeType.Regular);
                }
            }

            Grid = new Grid3D(area, XYZ, elems, IJK);
            Grid.Nc = Grid.Nnodes;
            List<Boundary3D> boundaries = Grid.GetBoundaries();

            Bc1 = boundaries;

            Bc2 = new List<Boundary3D>()
            {

            };

            Bc3 = new List<Boundary3D>()
            {

            };

            FemParams = FemParamsFactory3D.CreateFemParams(1);
        }

        public double u(double x, double y, double z, double t)
        {
            return Functions1_3D.u(x, y, z, t);
        }
    }

    /// <summary>
    /// Тест нерегулярной сетки из кирпича.
    /// </summary>
    public class Test2_3D
    {
        public Grid3D Grid;
        public FEMParams3D FemParams;
        public List<Boundary3D> Bc1;
        public List<Boundary3D> Bc2;
        public List<Boundary3D> Bc3;
        public List<double> T = new() { 0, 1, 2, 3, 4 };

        public void CreateTest()
        {
            double x1 = 0;
            double x2 = 3;
            double x3 = 5;
            double x4 = 8;

            double y1 = 0;
            double y2 = 3;
            double y3 = 5;

            double z1 = 0;
            double z2 = 1;
            double z3 = 4;
            double z4 = 8;

            List<double> xw = new List<double> { x1, x4 };
            List<double> yw = new List<double> { y1, y3 };
            List<double> zw = new List<double> { z1, z4 };
            SubArea3D sub1 = new SubArea3D(0, 0, 1, 0, 1, 0, 1);
            List<SubArea3D> subs = new List<SubArea3D> { sub1 };
            int nmats = 1;
            Area3D area = new Area3D(xw, yw, zw, subs, nmats);
            List<Vector3> XYZ = new List<Vector3>()
                {
                    new Vector3(x1,y1,z1), //1
                    new Vector3(x2,y1,z1), //2
                    new Vector3(x3,y1,z1), //3
                    new Vector3(x4,y1,z1), //4
                    new Vector3(x3,y2,z1), //5
                    new Vector3(x4,y2,z1), //6
                    new Vector3(x1,y3,z1), //7
                    new Vector3(x2,y3,z1), //8
                    new Vector3(x3,y3,z1), //9
                    new Vector3(x4,y3,z1), //10
                    new Vector3(x3,y1,z2), //11
                    new Vector3(x4,y1,z2), //12
                    new Vector3(x1,y1,z3), //13
                    new Vector3(x2,y1,z3), //14
                    new Vector3(x3,y1,z3), //15
                    new Vector3(x4,y1,z3), //16
                    new Vector3(x3,y2,z3), //17
                    new Vector3(x4,y2,z3), //18
                    new Vector3(x1,y3,z3), //19
                    new Vector3(x2,y3,z3), //20
                    new Vector3(x3,y3,z3), //21
                    new Vector3(x4,y3,z3), //22
                    new Vector3(x1,y1,z4), //23
                    new Vector3(x2,y1,z4), //24
                    new Vector3(x3,y1,z4), //25
                    new Vector3(x4,y1,z4), //26
                    new Vector3(x3,y2,z4), //27
                    new Vector3(x4,y2,z4), //28
                    new Vector3(x1,y3,z4), //29
                    new Vector3(x2,y3,z4), //30
                    new Vector3(x3,y3,z4), //31
                    new Vector3(x4,y3,z4), //32
                    new Vector3(x2,y2,z1), //33
                    new Vector3(x2,y1,z2), //34
                    new Vector3(x2,y2,z2), //35
                    new Vector3(x3,y2,z2), //36
                    new Vector3(x4,y2,z2), //37
                    new Vector3(x1,y2,z3), //38
                    new Vector3(x2,y2,z4), //39
                };
            List<Elem3D> elems = new List<Elem3D>()
                {
                    new Elem3D(0,0,1,4,5,8,9,12,13, new List<int>{15,16,17,19}),
                    new Elem3D(0,1,2,15,3,16,7,17,18),
                    new Elem3D(0,16,7,17,18,9,10,19,11),
                    new Elem3D(0,15,3,5,6,19,11,13,14, new List<int>{17,18})
                };
            int nx = 3;
            int ny = 3;
            int nz = 3;

            ByteMat3D IJK = new ByteMat3D(nx);
            for (int i = 0; i < nx; i++)
            {
                IJK.Add(new ByteMat2D(ny));
                for (int j = 0; j < ny; j++)
                {
                    IJK[i].Add(new List<NodeType>(nz));
                    for (int k = 0; k < nz; k++)
                        IJK[i][j].Add(NodeType.Regular);
                }
            }

            Grid = new Grid3D(area, XYZ, elems, IJK);
            Grid.Nc = Grid.Nnodes - 5;
            List<Boundary3D> boundaries = Grid.GetBoundaries();

            Bc1 = boundaries;

            Bc2 = new List<Boundary3D>()
            {

            };

            Bc3 = new List<Boundary3D>()
            {

            };

            FemParams = FemParamsFactory3D.CreateFemParams(1);
            FemParams.Ug = ug;
        }

        public double u(double x, double y, double z, double t) 
        {
            return Functions1_3D.u(x, y, z, t);
        }

        private double ug(int si, double x, double y, double z, double t)
        {
            double xmin = 0;
            double xmax = 5;
            switch (si)
            {
                case 0:
                    return u(x, 0, z, t);
                case 1:
                    return u(xmax, y, z, t);
                case 2:
                    return u(x, 5, z, t);
                case 3:
                    return u(xmin, y, z, t); ;
                case 4:
                    return u(x, y, 0, t);
                case 5:
                    return u(x, y, 4, t);
            }
            return 0;
        }
    }
}

using System;
using System.Collections.Generic;

namespace MakeGrid3D.Solver
{
    /// <summary>
    /// Создатель функций для МКЭ решателя.
    /// </summary>
    static class FemParamsFactory2D
    {
        public static FEMParams2D CreateFemParams(int functions_num)
        {
            var FemParams = new FEMParams2D();
            switch (functions_num)
            {
                case 1:
                    FemParams.Lambda = Functions1_2D.lambda;
                    FemParams.Chi = Functions1_2D.chi;
                    FemParams.Sigma = Functions1_2D.sigma;
                    FemParams.Ubeta = Functions1_2D.u_beta;
                    FemParams.Theta = Functions1_2D.theta;
                    FemParams.Beta = Functions1_2D.beta;
                    FemParams.Ug = Functions1_2D.u_g;
                    FemParams.F = Functions1_2D.f;
                    FemParams.U0 = Functions1_2D.u0;
                    FemParams.U1 = Functions1_2D.u1;
                    FemParams.DU0 = Functions1_2D.u0;
                    FemParams.Gamma = Functions2_2D.gamma;
                    break;
                case 2:
                    FemParams.Lambda = Functions2_2D.lambda;
                    FemParams.Chi = Functions2_2D.chi;
                    FemParams.Sigma = Functions2_2D.sigma;
                    FemParams.Ubeta = Functions2_2D.u_beta;
                    FemParams.Theta = Functions2_2D.theta;
                    FemParams.Beta = Functions2_2D.beta;
                    FemParams.Ug = Functions2_2D.u_g;
                    FemParams.F = Functions2_2D.f;
                    FemParams.U0 = Functions2_2D.u0;
                    FemParams.U1 = Functions2_2D.u1;
                    FemParams.DU0 = Functions2_2D.u0;
                    FemParams.Gamma = Functions2_2D.gamma;
                    break;
                case 3:
                    FemParams.Lambda = Functions3_2D.lambda;
                    FemParams.Chi = Functions3_2D.chi;
                    FemParams.Sigma = Functions3_2D.sigma;
                    FemParams.Ubeta = Functions3_2D.u_beta;
                    FemParams.Theta = Functions3_2D.theta;
                    FemParams.Beta = Functions3_2D.beta;
                    FemParams.Ug = Functions3_2D.u_g;
                    FemParams.F = Functions3_2D.f;
                    FemParams.U0 = Functions3_2D.u0;
                    FemParams.U1 = Functions3_2D.u1;
                    FemParams.DU0 = Functions3_2D.u0;
                    FemParams.Gamma = Functions3_2D.gamma;
                    break;
                case 4:
                    FemParams.Lambda = Functions4_2D.lambda;
                    FemParams.Chi = Functions4_2D.chi;
                    FemParams.Sigma = Functions4_2D.sigma;
                    FemParams.Ubeta = Functions4_2D.u_beta;
                    FemParams.Theta = Functions4_2D.theta;
                    FemParams.Beta = Functions4_2D.beta;
                    FemParams.Ug = Functions4_2D.u_g;
                    FemParams.F = Functions4_2D.f;
                    FemParams.U0 = Functions4_2D.u0;
                    FemParams.U1 = Functions4_2D.u1;
                    FemParams.DU0 = Functions4_2D.u0;
                    FemParams.Gamma = Functions4_2D.gamma;
                    break;
                case 5:
                    FemParams.Lambda = Functions5_2D.lambda;
                    FemParams.Chi = Functions5_2D.chi;
                    FemParams.Sigma = Functions5_2D.sigma;
                    FemParams.Ubeta = Functions5_2D.u_beta;
                    FemParams.Theta = Functions5_2D.theta;
                    FemParams.Beta = Functions5_2D.beta;
                    FemParams.Ug = Functions5_2D.u_g;
                    FemParams.F = Functions5_2D.f;
                    FemParams.U0 = Functions5_2D.u0;
                    FemParams.U1 = Functions5_2D.u1;
                    FemParams.DU0 = Functions5_2D.u0;
                    FemParams.Gamma = Functions5_2D.gamma;
                    break;
                case 6:
                    FemParams.Lambda = Functions6_2D.lambda;
                    FemParams.Chi = Functions6_2D.chi;
                    FemParams.Sigma = Functions6_2D.sigma;
                    FemParams.Ubeta = Functions6_2D.u_beta;
                    FemParams.Theta = Functions6_2D.theta;
                    FemParams.Beta = Functions6_2D.beta;
                    FemParams.Ug = Functions6_2D.u_g;
                    FemParams.F = Functions6_2D.f;
                    FemParams.U0 = Functions6_2D.u0;
                    FemParams.U1 = Functions6_2D.u1;
                    FemParams.DU0 = Functions6_2D.u0;
                    FemParams.Gamma = Functions6_2D.gamma;
                    break;
            }
            return FemParams;
        }
    }

    /// <summary>
    /// Функция x*y*t для функции testgrid1.
    /// </summary>
    static class Functions1_2D
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

        static public double f(int wi, double x, double y, double t)
        {
            return sigma(wi) * x * y + gamma(wi) * x * y * t;
        }

        static public double u_g(int si, double x, double y, double t)
        {
            switch (si)
            {
                case 0:
                    return x * t;
                case 1:
                    return 18 * y * t;
                case 2:
                    return x * 14 * t;
                case 3:
                    return y * t;
            }
            return 0;
        }

        static public double theta(int si, double x, double y, double t)
        {
            return 0;
        }

        static public double beta(int si)
        {
            return 0;
        }

        static public double u_beta(int si, double x, double y, double t)
        {
            return 0;
        }

        static public double u0(int wi, double x, double y)
        {
            double t0 = 0;
            return x * y * t0;
        }

        static public double u1(int wi, double x, double y)
        {
            double t1 = 1;
            return x * y * t1;
        }

        static public double u(double x, double y, double t) 
        {
            return x * y * t;
        }
    }

    /// <summary>
    /// Функция xy*t для сетки testgrid2.
    /// </summary>
    static class Functions2_2D
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

        static public double f(int wi, double x, double y, double t)
        {
            return sigma(wi) * x * y + gamma(wi) * x * y * t;
        }

        static public double u_g(int si, double x, double y, double t)
        {
            switch (si)
            {
                case 0:
                    return u(x, 1, t);
                case 1:
                    return u(9, y, t);
                case 2:
                    return u(x, 13, t);
                case 3:
                    return u(1, y, t);
            }
            return 0;
        }

        static public double theta(int si, double x, double y, double t)
        {
            switch (si)
            {
                case 1:
                    return 2 * y * t;
            }
            return 0;
        }

        static public double beta(int si)
        {
            return 2;
        }

        static public double u_beta(int si, double x, double y, double t)
        {
            switch (si)
            {
                case 2:
                    return 14 * x * t;
            }
            return 0;
        }

        static public double u0(int wi, double x, double y)
        {
            double t0 = 0;
            return u(x, y, t0);
        }

        static public double u1(int wi, double x, double y)
        {
            double t1 = 1;
            return u(x, y, t1);
        }

        static public double u(double x, double y, double t)
        {
            return x * y * t;
        }
    }

    /// <summary>
    /// Функция x^2*y^2*t для сетки testgrid2.
    /// </summary>
    static class Functions3_2D
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

        static public double f(int wi, double x, double y, double t)
        {
            return -lambda(wi) * 2*t*(x*x + y*y) + gamma(wi)*u(x,y,t) + sigma(wi)*x*x*y*y;
        }

        static public double u_g(int si, double x, double y, double t)
        {
            switch (si)
            {
                case 0:
                    return u(x,1,t);
                case 1:
                    return u(9, y, t);
                case 2:
                    return u(x, 13, t);
                case 3:
                    return u(1, y, t);
            }
            return 0;
        }

        static public double theta(int si, double x, double y, double t)
        {
            switch (si)
            {
                case 1:
                    return lambda(si) * 18 * y * y * t;
            }
            return 0;
        }

        static public double beta(int si)
        {
            return 2;
        }

        static public double u_beta(int si, double x, double y, double t)
        {
            switch (si)
            {
                case 2:
                    return (26 * lambda(si) * x * x * t + beta(si) * 169 * x * x * t) / beta(si);
            }
            return 0;
        }

        static public double u0(int wi, double x, double y)
        {
            double t0 = 0;
            return u(x, y, t0);
        }

        static public double u1(int wi, double x, double y)
        {
            double t1 = 1;
            return u(x,y,t1);
        }

        static public double u(double x, double y, double t)
        {
            return x*x * y*y * t;
        }
    }

    /// <summary>
    /// Функция x*y*t^2 для сетки testgrid2.
    /// </summary>
    static class Functions4_2D
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

        static public double f(int wi, double x, double y, double t)
        {
            return gamma(wi) * u(x,y,t) + sigma(wi) * 2*t*x*y + chi(wi) * 2 * x * y;
        }

        static public double u_g(int si, double x, double y, double t)
        {
            switch (si)
            {
                case 0:
                    return u(x, 1, t);
                case 1:
                    return u(9, y, t);
                case 2:
                    return u(x, 13, t);
                case 3:
                    return u(1, y, t);
            }
            return 0;
        }

        static public double theta(int si, double x, double y, double t)
        {
            switch (si)
            {
                case 1:
                    return 2 * y * t * t;
            }
            return 0;
        }

        static public double beta(int si)
        {
            return 2;
        }

        static public double u_beta(int si, double x, double y, double t)
        {
            switch (si)
            {
                case 2:
                    return 14 * x * t * t;
            }
            return 0;
        }

        static public double u0(int wi, double x, double y)
        {
            double t0 = 0;
            return u(x, y, t0);
        }

        static public double u1(int wi, double x, double y)
        {
            double t1 = 1;
            return u(x, y, t1);
        }

        static public double u(double x, double y, double t)
        {
            return x * y * t * t;
        }
    }

    /// <summary>
    /// Функция x*y*t^3 для сетки testgrid2.
    /// </summary>
    static class Functions5_2D
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

        static public double f(int wi, double x, double y, double t)
        {
            return gamma(wi) * u(x, y, t) + sigma(wi) * 3 * t * t * x * y + chi(wi) * 6 * t * x * y;
        }

        static public double u_g(int si, double x, double y, double t)
        {
            switch (si)
            {
                case 0:
                    return u(x, 1, t);
                case 1:
                    return u(9, y, t);
                case 2:
                    return u(x, 13, t);
                case 3:
                    return u(1, y, t);
            }
            return 0;
        }

        static public double theta(int si, double x, double y, double t)
        {
            switch (si)
            {
                case 1:
                    return 2 * y * t * t * t;
            }
            return 0;
        }

        static public double beta(int si)
        {
            return 2;
        }

        static public double u_beta(int si, double x, double y, double t)
        {
            switch (si)
            {
                case 2:
                    return 14 * x * t * t * t;
            }
            return 0;
        }

        static public double u0(int wi, double x, double y)
        {
            double t0 = 0;
            return u(x, y, t0);
        }

        static public double u1(int wi, double x, double y)
        {
            double t1 = 1;
            return u(x, y, t1);
        }

        static public double u2(int wi, double x, double y)
        {
            double t2 = 2;
            return u(x, y, t2);
        }

        static public double u(double x, double y, double t)
        {
            return x * y * t * t * t;
        }
    }

    /// <summary>
    /// Функция x*y*t^4 для сетки testgrid2.
    /// </summary>
    static class Functions6_2D
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

        static public double f(int wi, double x, double y, double t)
        {
            return gamma(wi) * u(x, y, t) + sigma(wi) * 4 * t * t * t * x * y + chi(wi) * 12 * t * t * x * y;
        }

        static public double u_g(int si, double x, double y, double t)
        {
            switch (si)
            {
                case 0:
                    return u(x, 1, t);
                case 1:
                    return u(9, y, t);
                case 2:
                    return u(x, 13, t);
                case 3:
                    return u(1, y, t);
            }
            return 0;
        }

        static public double theta(int si, double x, double y, double t)
        {
            switch (si)
            {
                case 1:
                    return 2 * y * t * t * t * t;
            }
            return 0;
        }

        static public double beta(int si)
        {
            return 2;
        }

        static public double u_beta(int si, double x, double y, double t)
        {
            switch (si)
            {
                case 2:
                    return 14 * x * t * t * t * t;
            }
            return 0;
        }

        static public double u0(int wi, double x, double y)
        {
            double t0 = 0;
            return u(x, y, t0);
        }

        static public double u1(int wi, double x, double y)
        {
            double t1 = 1;
            return u(x, y, t1);
        }

        static public double u2(int wi, double x, double y)
        {
            double t2 = 2;
            return u(x, y, t2);
        }

        static public double u(double x, double y, double t)
        {
            return x * y * t * t * t * t;
        }
    }

    /// <summary>
    /// Тест двумерной нерегулярной сетки рис 110.
    /// </summary>
    public class Test1_2D
    {
        public Grid2D Grid;
        public FEMParams2D FemParams;
        public List<double> T;
        public List<Boundary2D> Bc1;
        public List<Boundary2D> Bc2;
        public List<Boundary2D> Bc3;


        public void CreateTest()
        {
            List<double> xw = new List<double> { 1, 18 };
            List<double> yw = new List<double> { 1, 14 };
            SubArea2D sub1 = new SubArea2D(0, 0, 1, 0, 1);
            List<SubArea2D> subs = new List<SubArea2D> { sub1 };
            int nmats = 1;
            Area2D area = new Area2D(xw, yw, subs, nmats);
            List<Vector2> XY = new List<Vector2>()
            {
                new Vector2(1,1), //1
                new Vector2(3,1), //2
                new Vector2(7,1), //3 
                new Vector2(9.25,1), //4
                new Vector2(13,1), //5
                new Vector2(18,1), //6
                new Vector2(7,6), // 7
                new Vector2(1,9), //8
                new Vector2(3,9), //9
                new Vector2(13,11), //10
                new Vector2(18,11), //11
                new Vector2(1,14), //12
                new Vector2(3,14), //13
                new Vector2(13,14), //14
                new Vector2(18,14), //15
                new Vector2(3,6), //16
                new Vector2(9.25,6), //17
                new Vector2(13,6), //18
                new Vector2(7,9), //19
                new Vector2(3,11), //20
                new Vector2(7,11), //21
            };
            List<Elem2D> elems = new List<Elem2D>()
            {
                new Elem2D(0,0,1,7,8, new List<int>(){ 15 }),
                new Elem2D(0,1,2,15,6),
                new Elem2D(0,2,3,6,16),
                new Elem2D(0,3,4,16,17),
                new Elem2D(0,4,5,9,10, new List<int>(){ 17 }),
                new Elem2D(0,15,6,8,18),
                new Elem2D(0,6,17,20,9, new List<int>() { 16,18}),
                new Elem2D(0,7,8,11,12, new List<int>(){ 19 }),
                new Elem2D(0,8,18,19,20),
                new Elem2D(0,19,9,12,13, new List<int>(){ 20 }),
                new Elem2D(0,9,10,13,14),
            };
            int nx = 6;
            int ny = 5;
            ByteMat2D IJ = new ByteMat2D(nx);
            for (int i = 0; i < nx; i++)
            {
                IJ.Add(new List<NodeType>(ny));
                for (int j = 0; j < ny; j++)
                {
                    IJ[i].Add(NodeType.Regular);
                }
            }

            Grid = new Grid2D(area, XY, elems, IJ);
            Grid.Nc = Grid.Nnodes - 6;
            T = new List<double>() { 0, 1, 2, 3, 4 };


            Bc1 = Grid.GetBoundaries();

            Bc2 = new List<Boundary2D>()
            {

            };

            Bc3 = new List<Boundary2D>()
            {

            };

            FemParams = FemParamsFactory2D.CreateFemParams(1);
        }
    }

    /// <summary>
    /// Тест двумерной нерегулярной сетки рис 111.
    /// </summary>
    public class Test2_2D
    {
        public Grid2D Grid;
        public FEMParams2D FemParams;
        public List<double> T;
        public List<Boundary2D> Bc1;
        public List<Boundary2D> Bc2;
        public List<Boundary2D> Bc3;

        public void CreateTest()
        {
            List<double> xw = new List<double> { 1, 9 };
            List<double> yw = new List<double> { 1, 13 };
            SubArea2D sub1 = new SubArea2D(0, 0, 1, 0, 1);
            List<SubArea2D> subs = new List<SubArea2D> { sub1 };
            int nmats = 1;
            Area2D area = new Area2D(xw, yw, subs, nmats);
            List<Vector2> XY = new List<Vector2>()
            {
                new Vector2(1,1), //1
                new Vector2(3,1), //2
                new Vector2(6,1), //3 
                new Vector2(9,1), //4
                new Vector2(1,3), //5
                new Vector2(3,3), //6
                new Vector2(6,3), // 7
                new Vector2(9,3), //8
                new Vector2(1,5), //9
                new Vector2(3,5), //10
                new Vector2(1,8), //11
                new Vector2(3,8), //12
                new Vector2(1,11), //13
                new Vector2(6,11), //14
                new Vector2(9,11), //15
                new Vector2(1,13), //16
                new Vector2(6,13), //17
                new Vector2(9,13), //18
                new Vector2(6,5), //19
                new Vector2(6,8), //20
                new Vector2(3,11), //21
            };
            List<Elem2D> elems = new List<Elem2D>()
            {
                new Elem2D(0,0,1,4,5),
                new Elem2D(0,1,2,5,6),
                new Elem2D(0,2,3,6,7),
                new Elem2D(0,4,5,8,9),
                new Elem2D(0,5,6,9,18),
                new Elem2D(0,6,7,13,14, new List<int>() { 18,19}),
                new Elem2D(0,8,9,10,11),
                new Elem2D(0,9,18,11,19),
                new Elem2D(0,10,11,12,20),
                new Elem2D(0,11,19,20,13),
                new Elem2D(0,12,13,15,16, new List<int>() { 20}),
                new Elem2D(0,13,14,16,17)
            };
            int nx = 4;
            int ny = 6;
            ByteMat2D IG = new ByteMat2D(nx);
            for (int i = 0; i < nx; i++)
            {
                IG.Add(new List<NodeType>(ny));
                for (int j = 0; j < ny; j++)
                {
                    IG[i].Add(NodeType.Regular);
                }
            }

            Grid = new Grid2D(area, XY, elems, IG);
            Grid.Nc = Grid.Nnodes - 3;
            T = new List<double>() { 0, 1, 2, 3, 4 };
            var boundaries = Grid.GetBoundaries();

            Bc1 = new List<Boundary2D>();
            Bc2 = new List<Boundary2D>();
            Bc3 = new List<Boundary2D>();
            foreach (Boundary2D boundary in boundaries) 
            {
                switch (boundary.Si) 
                {
                    case 0: case 3:
                        Bc1.Add(boundary);
                        break;
                    case 1: 
                        Bc2.Add(boundary);
                        break;
                    case 2:
                        Bc3.Add(boundary);
                        break;
                }
            }

            FemParams = FemParamsFactory2D.CreateFemParams(6);
        }

        public double U(double x, double y, double t) 
        {
            return Functions6_2D.u(x, y, t);
        }
    }

    /// <summary>
    /// Тест двумерной нерегулярной сетки с перехлестами.
    /// </summary>
    public class Test3_2D
    {
        public Grid2D Grid;
        public FEMParams2D FemParams;
        public List<double> T;
        public List<Boundary2D> Bc1;
        public List<Boundary2D> Bc2;
        public List<Boundary2D> Bc3;

        public void CreateTest()
        {
            List<double> xw = new List<double> { 1, 9 };
            List<double> yw = new List<double> { 1, 13 };
            SubArea2D sub1 = new SubArea2D(0, 0, 1, 0, 1);
            List<SubArea2D> subs = new List<SubArea2D> { sub1 };
            int nmats = 1;
            Area2D area = new Area2D(xw, yw, subs, nmats);
            List<Vector2> XY = new List<Vector2>()
            {
                new Vector2(1,1), //1
                new Vector2(5,1), //2
                new Vector2(9,1), //3 
                new Vector2(1,3), //4
                new Vector2(9,6), //5
                new Vector2(1,7), //6
                new Vector2(9,9), // 7
                new Vector2(1,11), //8
                new Vector2(1,13), //9
                new Vector2(5,13), //10
                new Vector2(9,13), //11
                new Vector2(5,3), //12
                new Vector2(5,6), //13
                new Vector2(5,7), //14
                new Vector2(5,9), //15
                new Vector2(5,11) //16
            };
            List<Elem2D> elems = new List<Elem2D>()
            {
                new Elem2D(0,0,1,3,11),
                new Elem2D(0,1,2,12,4, new List<int>() { 11}),
                new Elem2D(0,3,11,5,13,new List<int>() { 12}),
                new Elem2D(0,12,4,14,6,new List<int>() { 13}),
                new Elem2D(0,5,13,7,15,new List<int>() { 14}),
                new Elem2D(0,14,6,9,10, new List<int>() { 15}),
                new Elem2D(0,7,15,8,9)
            };
            int nx = 3;
            int ny = 7;
            ByteMat2D IG = new ByteMat2D(nx);
            for (int i = 0; i < nx; i++)
            {
                IG.Add(new List<NodeType>(ny));
                for (int j = 0; j < ny; j++)
                {
                    IG[i].Add(NodeType.Regular);
                }
            }
            Grid = new Grid2D(area, XY, elems, IG);
            Grid.Nc = Grid.Nnodes - 5;
            T = new List<double>() { 1, 2, 3, 4, 5 };

            Bc1 = Grid.GetBoundaries();

            Bc2 = new List<Boundary2D>()
            {

            };

            Bc3 = new List<Boundary2D>()
            {

            };

            FemParams = FemParamsFactory2D.CreateFemParams(2);
        }
    }
}

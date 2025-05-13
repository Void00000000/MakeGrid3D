using System.Collections.Generic;

namespace MakeGrid3D.Solver
{
    /// <summary>
    /// Создатель функций для МКЭ решателя.
    /// </summary>
    static class FemParamsFactory
    {
        public static FEMParams CreateFemParams(int functions_num)
        {
            var FemParams = new FEMParams();
            switch (functions_num)
            {
                case 1:
                    FemParams.Lambda = Functions1.lambda;
                    FemParams.Chi = Functions1.chi;
                    FemParams.Sigma = Functions1.sigma;
                    FemParams.Ubeta = Functions1.u_beta;
                    FemParams.Theta = Functions1.theta;
                    FemParams.Beta = Functions1.beta;
                    FemParams.Ug = Functions1.u_g;
                    FemParams.F = Functions1.f;
                    break;
                case 2:
                    FemParams.Lambda = Functions2.lambda;
                    FemParams.Chi = Functions2.chi;
                    FemParams.Sigma = Functions2.sigma;
                    FemParams.Ubeta = Functions2.u_beta;
                    FemParams.Theta = Functions2.theta;
                    FemParams.Beta = Functions2.beta;
                    FemParams.Ug = Functions2.u_g;
                    FemParams.F = Functions2.f;
                    break;
            }
            return FemParams;
        }
    }

    /// <summary>
    /// Функция из кирпича с несколькоми подобластями.
    /// </summary>
    static class Functions1
    {
        static public double chi(int wi)
        {
            return 0;
        }

        static public double sigma(int wi)
        {
            switch (wi)
            {
                case 0:
                    return 2;
                case 1:
                    return 1;
                case 2:
                    return 0;
            }
            return 0;
        }

        static public double lambda(int wi)
        {
            switch (wi)
            {
                case 0:
                    return 1;
                case 1:
                    return 10;
                case 2:
                    return 10;
            }
            return 0;
        }

        static public double f(int wi, double x, double y)
        {
            switch (wi)
            {
                case 0:
                    return 2 * x;
                case 1:
                    return 1.8 + 0.1 * x;
                case 2:
                    return 0;
            }
            return 0;
        }

        static public double u_g(int si, double x, double y)
        {
            switch (si)
            {
                case 0:
                    return 2;
                case 1:
                    return 1.8 + 0.1 * x;
            }
            return 0;
        }

        static public double theta(int si, double x, double y)
        {
            switch (si)
            {
                case 0:
                    return 1;
                case 1:
                    return 0;
            }
            return 0;
        }

        static public double beta(int si)
        {
            switch (si)
            {
                case 0:
                    return 1;
                case 1:
                    return 2;
                case 2:
                    return 0.5;
            }
            return 0;
        }

        static public double u_beta(int si, double x, double y)
        {
            switch (si)
            {
                case 0:
                    return x;
                case 1:
                    return 1.8 + 0.1 * x;
                case 2:
                    return -1;
            }
            return 0;
        }
    }

    /// <summary>
    /// Функция xy.
    /// </summary>
    static class Functions2
    {
        static public double chi(int wi)
        {
            return 0;
        }

        static public double sigma(int wi)
        {
            return 3;
        }

        static public double lambda(int wi)
        {
            return 2;
        }

        static public double f(int wi, double x, double y)
        {
            return 3 * x * y;
        }

        static public double u_g(int si, double x, double y)
        {
            switch (si)
            {
                case 0:
                    return x;
                case 1:
                    return 18 * y;
                case 2:
                    return x * 14;
                case 3:
                    return y;
            }
            return 0;
        }

        static public double theta(int si, double x, double y)
        {
            return 0;
        }

        static public double beta(int si)
        {
            return 0;
        }

        static public double u_beta(int si, double x, double y)
        {
            return 0;
        }
    }

    /// <summary>
    /// Тест сетки со всеми краевыми из учебника.
    /// </summary>
    public class Test1
    {
        public Grid2D Grid;
        public FEMParams FemParams;
        public List<Boundary> Bc1;
        public List<Boundary> Bc2;
        public List<Boundary> Bc3;

        public void CreateTest()
        {
            List<double> xw = new List<double> { 1, 2, 6 };
            List<double> yw = new List<double> { 1, 2, 4, 5 };
            SubArea2D sub1 = new SubArea2D(0, 0, 1, 0, 3);
            SubArea2D sub2 = new SubArea2D(1, 1, 2, 1, 2);
            SubArea2D sub3 = new SubArea2D(2, 1, 2, 2, 3);
            List<SubArea2D> subs = new List<SubArea2D> { sub1, sub2, sub3 };
            Area2D area = new Area2D(xw, yw, subs, 3);
            List<Vector2> XY = new List<Vector2>()
            {
                new Vector2(1,1),
                new Vector2(2,1),
                new Vector2(2,2),
                new Vector2(1,2),
                new Vector2(2,2),
                new Vector2(6,2),
                new Vector2(1,4),
                new Vector2(2,4),
                new Vector2(6,4),
                new Vector2(1,5),
                new Vector2(2,5),
                new Vector2(6,5)
            };
            List<Elem2D> elems = new List<Elem2D>()
            {
                new Elem2D(0,0,1,3,4),
                new Elem2D(0,3,4,6,7),
                new Elem2D(1,4,5,7,8),
                new Elem2D(0,6,7,9,10),
                new Elem2D(2,7,8,10,11)
            };
            int nx = 3;
            int ny = 4;
            ByteMat2D IG = new ByteMat2D(nx);
            for (int i = 0; i < nx; i++) 
            {
                IG.Add(new List<NodeType>(ny));
                for (int j = 0; j< ny; j++) 
                {
                    IG[i].Add(NodeType.Regular);
                }
            }
            Grid = new Grid2D(area, XY, elems, IG);
            Grid.IXw = new List<int> { 0, 1, 2 };
            Grid.IYw = new List<int> { 0, 1, 2, 3 };

            Bc1 = new List<Boundary>()
            {
                new Boundary(0,1,1,0,1),
                new Boundary(1,1,2,1,1),
            };

            Bc2 = new List<Boundary>()
            {
                new Boundary(0,2,2,1,2),
                new Boundary(0,2,2,2,3),
                new Boundary(1,0,1,0,0),
            };

           Bc3 = new List<Boundary>()
            {
                new Boundary(0,0,1,3,3),
                new Boundary(1,1,2,3,3),
                new Boundary(2,0,0,0,3),
            };

            FemParams = FemParamsFactory.CreateFemParams(1);
        }
    }

    /// <summary>
    /// Тест сетки 3x3 с u(x,y) = xy.
    /// </summary>
    public class Test2
    {

        public Grid2D Grid;
        public FEMParams FemParams;
        public List<Boundary> Bc1;
        public List<Boundary> Bc2;
        public List<Boundary> Bc3;

        public void CreateTest()
        {
            List<double> xw = new List<double> { 1, 7 };
            List<double> yw = new List<double> { 1, 16 };
            SubArea2D sub1 = new SubArea2D(0, 0, 1, 0, 1);
            List<SubArea2D> subs = new List<SubArea2D> { sub1 };
            int nmats = 1;
            Area2D area = new Area2D(xw, yw, subs, nmats);
            List<Vector2> XY = new List<Vector2>()
            {
                new Vector2(1,1),
                new Vector2(2,1),
                new Vector2(4,1),
                new Vector2(7,1),
                new Vector2(1,5),
                new Vector2(2,5),
                new Vector2(4,5),
                new Vector2(7,5),
                new Vector2(1,10),
                new Vector2(2,10),
                new Vector2(4,10),
                new Vector2(7,10),
                new Vector2(1,16),
                new Vector2(2,16),
                new Vector2(4,16),
                new Vector2(7,16)
            };
            List<Elem2D> elems = new List<Elem2D>()
            {
                new Elem2D(0,0,1,4,5),
                new Elem2D(0,1,2,5,6),
                new Elem2D(0,2,3,6,7),
                new Elem2D(0,4,5,8,9),
                new Elem2D(0,5,6,9,10),
                new Elem2D(0,6,7,10,11),
                new Elem2D(0,8,9,12,13),
                new Elem2D(0,9,10,13,14),
                new Elem2D(0,10,11,14,15)
            };
            int nx = 4;
            int ny = 4;
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
            Grid.IXw = new List<int> { 0, 3 };
            Grid.IYw = new List<int> { 0, 3 };

            Bc1 = new List<Boundary>()
            {
                new Boundary(0,0,1,0,0),
                new Boundary(1,1,1,0,1),
                new Boundary(2,0,1,1,1),
                new Boundary(3,0,0,0,1),
            };

            Bc2 = new List<Boundary>()
            {
               
            };

            Bc3 = new List<Boundary>()
            {
                
            };

            FemParams = FemParamsFactory.CreateFemParams(2);
        }
    }

    /// <summary>
    /// Тест двумерной нерегулярной сетки рис 110.
    /// </summary>
    public class Test3
    {
        

        public Grid2D Grid;
        public FEMParams FemParams;
        public List<Boundary> Bc1;
        public List<Boundary> Bc2;
        public List<Boundary> Bc3;

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
            ByteMat2D IG = new ByteMat2D(nx);
            for (int i = 0; i < nx; i++)
            {
                IG.Add(new List<NodeType>(ny));
                for (int j = 0; j < ny; j++)
                {
                    IG[i].Add(NodeType.Regular);
                }
            }

            IG[1][1] = NodeType.Right;
            IG[3][1] = NodeType.Bottom;
            IG[4][1] = NodeType.Left;
            IG[2][2] = NodeType.Left;
            IG[1][3] = NodeType.Right;
            IG[2][3] = NodeType.Bottom;
            IG[0][1] = NodeType.Removed;
            IG[5][1] = NodeType.Removed;
            IG[4][2] = NodeType.Removed;
            IG[5][2] = NodeType.Removed;
            IG[0][3] = NodeType.Removed;

            Grid = new Grid2D(area, XY, elems, IG);
            Grid.IXw = new List<int> { 0, 5 };
            Grid.IYw = new List<int> { 0, 4 };
            Grid.Nс = Grid.Nnodes - 6;

            Bc1 = new List<Boundary>()
            {
                new Boundary(0,0,1,0,0),
                new Boundary(1,1,1,0,1),
                new Boundary(2,0,1,1,1),
                new Boundary(3,0,0,0,1),
            };

            Bc2 = new List<Boundary>()
            {

            };

            Bc3 = new List<Boundary>()
            {

            };

            FemParams = FemParamsFactory.CreateFemParams(2);
        }
    }

    /// <summary>
    /// Тест двумерной нерегулярной сетки рис 111.
    /// </summary>
    public class Test4
    {
        public Grid2D Grid;
        public FEMParams FemParams;
        public List<Boundary> Bc1;
        public List<Boundary> Bc2;
        public List<Boundary> Bc3;

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

            IG[3][2] = NodeType.Removed;
            IG[3][3] = NodeType.Removed;
            IG[2][2] = NodeType.Left;
            IG[2][3] = NodeType.Left;
            IG[1][4] = NodeType.Bottom;
            IG[1][5] = NodeType.Removed;

            Grid = new Grid2D(area, XY, elems, IG);
            Grid.IXw = new List<int> { 0, 3 };
            Grid.IYw = new List<int> { 0, 5 };
            Grid.Nс = Grid.Nnodes - 3;

            Bc1 = new List<Boundary>()
            {
                new Boundary(0,0,1,0,0),
                new Boundary(1,1,1,0,1),
                new Boundary(2,0,1,1,1),
                new Boundary(3,0,0,0,1),
            };

            Bc2 = new List<Boundary>()
            {

            };

            Bc3 = new List<Boundary>()
            {

            };

            FemParams = FemParamsFactory.CreateFemParams(2);
        }
    }
}

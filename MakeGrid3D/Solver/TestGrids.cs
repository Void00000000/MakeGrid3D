using System.Collections.Generic;

namespace MakeGrid3D.Solver
{
    public class Test1
    {
        static class Functions 
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

        public Grid2D Grid;
        public FEMParams FemParams;
        public List<Boundary> Bc1;
        public List<Boundary> Bc2;
        public List<Boundary> Bc3;

        public void CreateTest()
        {
            List<double> xw = new List<double> { 1, 2, 6 };
            List<double> yw = new List<double> { 1, 2, 4, 5 };
            SubArea2D sub1 = new SubArea2D(1, 1, 2, 1, 4);
            SubArea2D sub2 = new SubArea2D(2, 2, 3, 2, 3);
            SubArea2D sub3 = new SubArea2D(3, 2, 3, 3, 4);
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

            FemParams = new FEMParams();
            FemParams.Lambda = Functions.lambda;
            FemParams.Chi = Functions.chi;
            FemParams.Sigma = Functions.sigma;
            FemParams.Ubeta = Functions.u_beta;
            FemParams.Theta = Functions.theta;
            FemParams.Beta = Functions.beta;
            FemParams.Ug = Functions.u_g;
            FemParams.F = Functions.f;
        }
    }
}

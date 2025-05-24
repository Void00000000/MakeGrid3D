//using System.Collections.Generic;
//using System.Linq;

//namespace MakeGrid3D.Solver
//{
//    /// <summary>
//    /// Создатель функций для МКЭ решателя.
//    /// </summary>
//    static class FemParamsFactory3D
//    {
//        public static FEMParams3D CreateFemParams(int functions_num)
//        {
//            var FemParams = new FEMParams3D();
//            switch (functions_num)
//            {
//                case 1:
//                    FemParams.Lambda = Functions1_3D.lambda;
//                    FemParams.Chi = Functions1_3D.chi;
//                    FemParams.Sigma = Functions1_3D.sigma;
//                    FemParams.Ubeta = Functions1_3D.u_beta;
//                    FemParams.Theta = Functions1_3D.theta;
//                    FemParams.Beta = Functions1_3D.beta;
//                    FemParams.Ug = Functions1_3D.u_g;
//                    FemParams.F = Functions1_3D.f;
//                    break;
//            }
//            return FemParams;
//        }
//    }

//    /// <summary>
//    /// Функция xyz.
//    /// </summary>
//    static class Functions1_3D
//    {
//        static public double chi(int wi)
//        {
//            return 0;
//        }

//        static public double sigma(int wi)
//        {
//            return 3;
//        }

//        static public double lambda(int wi)
//        {
//            return 2;
//        }

//        static public double f(int wi, double x, double y, double z)
//        {
//            return 3 * x * y * z;
//        }

//        static public double u_g(int si, double x, double y, double z)
//        {
//            switch (si)
//            {
//                case 0:
//                    return x * z;
//                case 1:
//                    return 4 * y * z;
//                case 2:
//                    return 4 * x * z;
//                case 3:
//                    return y * z;
//                case 4:
//                    return x * y;
//                case 5:
//                    return 4 * x * y;
//            }
//            return 0;
//        }

//        static public double theta(int si, double x, double y, double z)
//        {
//            switch (si) 
//            {
//                case 0:
//                    return -2 * x * z;
//            }
//            return 0;
//        }

//        static public double beta(int si)
//        {
//            return 1;
//        }

//        static public double u_beta(int si, double x, double y, double z)
//        {
//            switch (si)
//            {
//                case 0:
//                    return -(-2 * x * z + 1 * x * z);
//            }
//            return 0;
//        }

//        /// <summary>
//        /// Тест сетки 3x3x3.
//        /// </summary>
//        public class Test1_3D
//        {
//            public Grid3D Grid;
//            public FEMParams3D FemParams;
//            public List<Boundary3D> Bc1;
//            public List<Boundary3D> Bc2;
//            public List<Boundary3D> Bc3;

//            public void CreateTest()
//            {
//                List<double> xw = new List<double> { 1, 4 };
//                List<double> yw = new List<double> { 1, 4 };
//                List<double> zw = new List<double> { 1, 4 };
//                SubArea3D sub1 = new SubArea3D(0, 0, 1, 0, 1, 0, 1);
//                List<SubArea3D> subs = new List<SubArea3D> { sub1 };
//                int nmats = 1;
//                Area3D area = new Area3D(xw, yw, zw, subs, nmats);
//                List<Vector3> XYZ = new List<Vector3>()
//                {
//                    new Vector3(1,1,1),
//                    new Vector3(2,1,1),
//                    new Vector3(4,1,1),
//                    new Vector3(1,2,1),
//                    new Vector3(2,2,1),
//                    new Vector3(4,2,1),
//                    new Vector3(1,4,1),
//                    new Vector3(2,4,1),
//                    new Vector3(4,4,1),

//                    new Vector3(1,1,2),
//                    new Vector3(2,1,2),
//                    new Vector3(4,1,2),
//                    new Vector3(1,2,2),
//                    new Vector3(2,2,2),
//                    new Vector3(4,2,2),
//                    new Vector3(1,4,2),
//                    new Vector3(2,4,2),
//                    new Vector3(4,4,2),

//                    new Vector3(1,1,4),
//                    new Vector3(2,1,4),
//                    new Vector3(4,1,4),
//                    new Vector3(1,2,4),
//                    new Vector3(2,2,4),
//                    new Vector3(4,2,4),
//                    new Vector3(1,4,4),
//                    new Vector3(2,4,4),
//                    new Vector3(4,4,4)
//                };
//                List<Elem3D> elems = new List<Elem3D>()
//                {
//                    new Elem3D(0,0,1,3,4,9,10,12,13),
//                    new Elem3D(0,1,2,4,5,6,7,13,14),
//                    new Elem3D(0,3,4,6,7,12,13,15,16),
//                    new Elem3D(0,4,5,7,8,13,14,16,17),
//                    new Elem3D(0,9,10,12,13,18,19,21,22),
//                    new Elem3D(0,10,11,13,14,19,20,22,23),
//                    new Elem3D(0,12,13,15,16,21,22,24,25),
//                    new Elem3D(0,13,14,16,17,22,23,25,26)
//                };
//                int nx = 3;
//                int ny = 3;
//                int nz = 3;

//                ByteMat3D IJK = new ByteMat3D(nx);
//                for (int i = 0; i < nx; i++)
//                {
//                    IJK.Add(new ByteMat2D(ny));
//                    for (int j = 0; j < ny; j++)
//                    {
//                        IJK[i].Add(new List<NodeType>(nz));
//                        for (int k = 0; k < nz; k++)
//                            IJK[i][j].Add(NodeType.Regular);
//                    }
//                }

//                Grid = new Grid3D(area, XYZ, elems, IJK);
//                Grid.Nc = Grid.Nnodes;
//                List<Boundary3D> boundaries = Grid.GetBoundaries();

//                Bc1 = boundaries;

//                Bc2 = new List<Boundary3D>()
//                {
                    
//                };

//                Bc3 = new List<Boundary3D>()
//                {
                    
//                };

//                FemParams = FemParamsFactory3D.CreateFemParams(1);
//            }
//        }

//        /// <summary>
//        /// Тест нерегулярной сетки из кирпича.
//        /// </summary>
//        public class Test2_3D
//        {
//            public Grid3D Grid;
//            public FEMParams3D FemParams;
//            public List<Boundary3D> Bc1;
//            public List<Boundary3D> Bc2;
//            public List<Boundary3D> Bc3;

//            public void CreateTest()
//            {
//                double x1 = 0;
//                double x2 = 3;
//                double x3 = 5;
//                List<double> xw = new List<double> {x1, x3 };
//                List<double> yw = new List<double> { 0, 5 };
//                List<double> zw = new List<double> { 0, 4 };
//                SubArea3D sub1 = new SubArea3D(0, 0, 1, 0, 1, 0, 1);
//                List<SubArea3D> subs = new List<SubArea3D> { sub1 };
//                int nmats = 1;
//                Area3D area = new Area3D(xw, yw, zw, subs, nmats);
//                List<Vector3> XYZ = new List<Vector3>()
//                {
//                    new Vector3(x1,0,0), //0
//                    new Vector3(x2,0,0), //1
//                    new Vector3(x3,0,0), //2
//                    new Vector3(x3,3,0), //3
//                    new Vector3(x1,5,0), //4
//                    new Vector3(x2,5,0), //5
//                    new Vector3(x3,5,0), //6
//                    new Vector3(x3,0,1), //7
//                    new Vector3(x1,0,4), //8
//                    new Vector3(x2,0,4), //9
//                    new Vector3(x3,0,4), //10
//                    new Vector3(x3,3,4), //11
//                    new Vector3(x1,5,4), //12
//                    new Vector3(x2,5,4), //13
//                    new Vector3(x3,5,4), //14
//                    new Vector3(x2,3,0), //15
//                    new Vector3(x2,0,1), //16
//                    new Vector3(x2,3,1), //17
//                    new Vector3(x3,3,1), //18
//                    new Vector3(x2,3,4), //19
//                };
//                List<Elem3D> elems = new List<Elem3D>()
//                {
//                    new Elem3D(0,0,1,4,5,8,9,12,13, new List<int>{15,16,17,19}),
//                    new Elem3D(0,1,2,15,3,16,7,17,18),
//                    new Elem3D(0,16,7,17,18,9,10,19,11),
//                    new Elem3D(0,15,3,5,6,19,11,13,14, new List<int>{17,18})
//                };
//                int nx = 3;
//                int ny = 3;
//                int nz = 3;

//                ByteMat3D IJK = new ByteMat3D(nx);
//                for (int i = 0; i < nx; i++)
//                {
//                    IJK.Add(new ByteMat2D(ny));
//                    for (int j = 0; j < ny; j++)
//                    {
//                        IJK[i].Add(new List<NodeType>(nz));
//                        for (int k = 0; k < nz; k++)
//                            IJK[i][j].Add(NodeType.Regular);
//                    }
//                }

//                Grid = new Grid3D(area, XYZ, elems, IJK);
//                Grid.Nc = Grid.Nnodes - 5;
//                List<Boundary3D> boundaries = Grid.GetBoundaries();

//                Bc1 = boundaries;

//                Bc2 = new List<Boundary3D>()
//                {

//                };

//                Bc3 = new List<Boundary3D>()
//                {

//                };

//                FemParams = FemParamsFactory3D.CreateFemParams(1);
//                FemParams.Ug = ug;
//            }

//            private double ug(int si, double x, double y, double z) 
//            {
//                double xmin = 0;
//                double xmax = 5;
//                switch (si)
//                {
//                    case 0:
//                        return 0;
//                    case 1:
//                        return xmax * y * z;
//                    case 2:
//                        return 5 * x * z;
//                    case 3:
//                        return xmin * y * z;
//                    case 4:
//                        return 0;
//                    case 5:
//                        return 4 * x * y;
//                }
//                return 0;
//            }
//        }
//    }
//}

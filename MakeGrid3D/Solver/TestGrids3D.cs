using System.Collections.Generic;

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
                    break;
            }
            return FemParams;
        }
    }

    /// <summary>
    /// Функция xyz.
    /// </summary>
    static class Functions1_3D
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

        static public double f(int wi, double x, double y, double z)
        {
            return 3 * x * y * z;
        }

        static public double u_g(int si, double x, double y, double z)
        {
            switch (si)
            {
                case 0:
                    return x * z;
                case 1:
                    return 4 * y * z;
                case 2:
                    return 4 * x * z;
                case 3:
                    return y * z;
                case 4:
                    return x * y;
                case 5:
                    return 4 * x * y;
            }
            return 0;
        }

        static public double theta(int si, double x, double y, double z)
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

        static public double u_beta(int si, double x, double y, double z)
        {
            switch (si)
            {
                case 0:
                    return -(-2 * x * z + 1 * x * z);
            }
            return 0;
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
                Grid.IXw = new List<int> { 0, 2 };
                Grid.IYw = new List<int> { 0, 2 };
                Grid.IZw = new List<int> { 0, 2 };
                Grid.CreateNXZ();

                Bc1 = new List<Boundary3D>()
                {
                    new Boundary3D(0,0,1,0,0,0,1),
                    new Boundary3D(1,1,1,0,1,0,1),
                    new Boundary3D(2,0,1,1,1,0,1),
                    new Boundary3D(3,0,0,0,1,0,1),
                    new Boundary3D(4,0,1,0,1,0,0),
                    new Boundary3D(5,0,1,0,1,1,1),
                };

                Bc2 = new List<Boundary3D>()
                {
                    
                };

                Bc3 = new List<Boundary3D>()
                {
                    
                };

                FemParams = FemParamsFactory3D.CreateFemParams(1);
            }
        }
    }
}

namespace MakeGrid3D.Solver
{
    using MakeGrid3D.Solver.SparseModule;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Решатель разрешенных СЛАУ с помощью локально-оптимальной схемы.
    /// </summary>
    public class LOSSolver
    {
        #region Private Fields

        /// <summary>
        /// Единственный экземпляр ленивого синглтона <see cref="LOSSolver"/>. 
        /// </summary>
        private static readonly Lazy<LOSSolver> _instance = new Lazy<LOSSolver>(() => new LOSSolver());

        /// <summary>
        /// Размерность матрицы.
        /// </summary>
        private int N;

        /// <summary>
        /// Невязка.
        /// </summary>
        private double nev;

        /// <summary>
        /// Вектор решения.
        /// </summary>
        private List<double> X;

        /// <summary>
        /// Вектор  di.
        /// </summary>
        private List<double> di;

        /// <summary>
        /// Вектор gl.
        /// </summary>
        private List<double> ggl;

        /// <summary>
        /// Вектор gu.
        /// </summary>
        private List<double> ggu;

        /// <summary>
        /// Вектор правой части.
        /// </summary>
        private List<double> pr;

        /// <summary>
        /// Вектор ig.
        /// </summary>
        private List<int> ig;

        /// <summary>
        /// Вектор jg.
        /// </summary>
        private List<int> jg;

        // Векторы для ЛОС
        private List<double> d;
        private List<double> r;
        private List<double> z;
        private List<double> p;
        private List<double> boof;
        private List<double> boof1;
        private List<double> L;
        private List<double> U;

        #endregion Private Fields

        #region Public Properties

        /// <inheritdoc cref="_instance"/>
        public static LOSSolver Instance => _instance.Value;

        /// <summary>
        /// Точность решения.
        /// </summary>
        public double Eps { get; set; } = 1e-16;

        /// <summary>
        /// Максимальное количество итераций.
        /// </summary>
        public int Maxiter { get; set; } = 1000;

        #endregion Public Properties

        #region Constructors

        private LOSSolver()
        {
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Решение СЛАУ с LU предобусловливанием.
        /// </summary>
        /// <param name="matrix">Разрешенная матрица.</param>
        /// <param name="b">Вектор правой части.</param>
        /// <returns>Вектор решения.</returns>
        public List<double> LOS_LU(SparseMatrix matrix, List<double> b)
        {
            Initialize(matrix, b);
            L = new List<double>(ig[N]);
            for (int i = 0; i < ig[N]; i++)
                L.Add(ggl[i]);
            U = new List<double>(ig[N]);
            for (int i = 0; i < ig[N]; i++)
                U.Add(ggu[i]);
            di = new List<double>(N);
            for (int i = 0; i < N; i++)
                di.Add(d[i]);
            LUS_factorisation();
            double a1, a2, nr, nf;
            r0_s();//r=f-A*x
            L_1(r, r);//r=(L^-1)*r
            U_1(r, z);//z=(U^-1)*r
            p0();//p=Az
            L_1(p, p);//p=(L^-1)*p
            nr = Norm(r);
            nf = Norm(pr);
            for (int i = 0; i < Maxiter; i++)
            {
                if (nr <= Eps * nf)
                    break;
                a1 = scalar_mult(p, r, N) / scalar_mult(p, p, N);//a=(p,r)/(p,p)  (3.35)
                X_k(a1, z);//x=x(k-1)+a*z  (3.36)
                R_k(a1, p);//r=r(k-1)+p*a  (3.37)
                U_1(r, boof);//boof=(U^-1)*r
                AVec(boof, boof1);//boof1=A*boof Все сделать по образцу этой функции
                L_1(boof1, boof);//boof=(L^-1)*boof1
                a2 = -scalar_mult(p, boof, N) / scalar_mult(p, p, N);//b=-(p,boof)/(p,p)  (3.38)
                U_1(r, boof1);//boof1=(U^-1)*r
                Z_k(boof1, a2, z, z);//z=boof1+b*z(k-1)
                Z_k(boof, a2, p, p);//p=длинное выражение+b*p  (3.40)
                nr = Norm(r);
            }
            nev = _nev();//nev=|f-Ax|/|f|
            return X;
        }

        /// <summary>
        /// Решение СЛАУ с диагональным предобусловливанием.
        /// </summary>
        /// <param name="matrix">Разрешенная матрица.</param>
        /// <param name="b">Вектор правой части.</param>
        /// <returns>Вектор решения.</returns>
        public List<double> LOS_DI(SparseMatrix matrix, List<double> b)
        {
            Initialize(matrix, b);
            double a1, a2, nr, nf;
            r0_s();//r=f-A*x
            vec_DI(r, r);//r=r/di
            z = new List<double>(r);
            p0();//p=Az
            vec_DI(p, p);//p=p/di
            nr = Norm(r);
            nf = Norm(pr);
            for (int i = 0; i < Maxiter; i++)
            {
                if (nr <= Eps * nf)
                    break;
                a1 = scalar_mult(p, r, N) / scalar_mult(p, p, N);//a=(p,r)/(p,p)  (3.35)
                X_k(a1, z);//x=x(k-1)+a*z  (3.36)
                R_k(a1, p);//r=r(k-1)+p*a  (3.37)
                AVec(r, boof);//boof=A*r 
                vec_DI(boof, boof);
                a2 = -scalar_mult(p, boof, N) / scalar_mult(p, p, N);//b=-(p,boof)/(p,p)  (3.38)
                Z_k(r, a2, z, z);//z=r+b*z(k-1)
                Z_k(boof, a2, p, p);//p=boof+b*p  (3.40)
                nr = Norm(r);
            }
            nev = _nev();//nev=|f-Ax|/|f|
            return X;
        }

        /// <summary>
        /// Решение СЛАУ без предобусловливания.
        /// </summary>
        /// <param name="matrix">Разрешенная матрица.</param>
        /// <param name="b">Вектор правой части.</param>
        /// <returns>Вектор решения.</returns>
        public List<double> LOS(SparseMatrix matrix, List<double> b)
        {
            Initialize(matrix, b);
            double a1, a2, nr, nf;
            r0_s();//r=f-A*x
            z = new List<double>(r);//z0=r0
            AVec(z, p);//p0=A*z0
            nr = Norm(r);
            nf = Norm(pr);
            for (int i = 0; i < Maxiter; i++)
            {
                if (nr <= Eps * nf)
                    break;
                a1 = scalar_mult(p, r, N) / scalar_mult(p, p, N);//a=(p,r)/(p,p)  (3.35)
                X_k(a1, z);//x=x(k-1)+a*z  (3.36)
                R_k(a1, p);//r=r(k-1)+p*a  (3.37)
                AVec(r, boof);//boof=A*r 
                a2 = -scalar_mult(p, boof, N) / scalar_mult(p, p, N);//b=-(p,boof)/(p,p)  (3.38)
                Z_k(r, a2, z, z);//z=r+b*z(k-1)
                Z_k(boof, a2, p, p);//p=boof+b*p  (3.40)
                nr = Norm(r);

            }
            nev = _nev();//nev=|f-Ax|/|f|
            return X;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Инициализирует решатель.
        /// </summary>
        private void Initialize(SparseMatrix matrix, List<double> b) 
        {
            N = matrix.N;
            X = InitializeVector(X);
            d = matrix.Di;
            ggl = matrix.Gl;
            ggu = matrix.Gu;
            pr = b;
            ig = matrix.Ig;
            jg = matrix.Jg;
            p = InitializeVector(p);
            r = InitializeVector(r);
            z = InitializeVector(z);
            boof = InitializeVector(boof);
            boof1 = InitializeVector(boof1);
        }

        private List<double> InitializeVector(List<double> v) 
        {
            v = new List<double>(N);
            for (int i = 0; i < N; i++)
                v.Add(0);
            return v;
        }

        private void LUS_factorisation()
        {
            int i, j, k;
            int i0, i1, ki, kj;
            double suml, sumu, sumd;

            for (i = 0; i < N; i++)
            {
                i0 = ig[i];
                i1 = ig[i + 1];
                sumd = 0;
                for (k = i0; k < i1; k++)
                {
                    j = jg[k];
                    ki = i0;
                    kj = ig[j];//что такое kj - соответсвующий номер элемента для домножения
                    suml = sumu = 0;
                    while (ki < k)
                        if (jg[kj] == jg[ki])
                        {
                            sumu += U[ki] * L[kj];
                            suml += U[kj] * L[ki];
                            kj++;
                            ki++;
                        }
                        else
                            if (jg[kj] < jg[ki])
                            kj++;
                        else
                            ki++;
                    U[k] = (ggu[k] - sumu) / di[j];
                    L[k] = (ggl[k] - suml) / di[j];
                    sumd += U[k] * L[k];
                }
                di[i] = Math.Sqrt(d[i] - sumd);
            }
        }

        private void r0_s()
        {
            int i, j, k;
            int i0, i1;
            for (i = 0; i < N; i++)
            {
                i0 = ig[i];
                i1 = ig[i + 1];
                r[i] = pr[i] - d[i] * X[i];//f-Ax0
                for (k = i0; k < i1; k++)
                {
                    j = jg[k];
                    r[i] -= ggl[k] * X[j];
                    r[j] -= ggu[k] * X[i];
                }
            }
        }

        private void L_1(List<double> pr, List<double> r)
        {
            double s;
            int i0, i1, k;
            for (int i = 0; i < N; i++)
            {
                s = pr[i];
                i0 = ig[i];
                i1 = ig[i + 1];
                for (k = i0; k < i1; k++)
                    s -= L[k] * r[jg[k]];
                r[i] = s / di[i];
            }
        }
        private void U_1(List<double> pr, List<double> z)
        {
            int i0, i1, j, k;
            z = new List<double>(pr);
            for (int i = N - 1; i >= 0; i--)
            {
                i0 = ig[i];
                i1 = ig[i + 1];
                z[i] = z[i] / di[i];
                for (k = i0; k < i1; k++)
                {
                    j = jg[k];
                    z[j] -= U[k] * z[i];
                }
            }
        }

        private void p0()
        {
            int i, k, j, i0, i1;
            for (i = 0; i < N; i++)
            {
                p[i] = d[i] * z[i];
                i0 = ig[i];
                i1 = ig[i + 1];
                for (k = i0; k < i1; k++)
                {
                    j = jg[k];
                    p[i] += ggl[k] * z[j];
                    p[j] += ggu[k] * z[i];
                }
            }
        }
        private double scalar_mult(List<double> v1, List<double> v2, int size)
        {
            double s = 0;
            for (int i = 0; i < size; i++)
                s += v1[i] * v2[i];
            return s;
        }

        private void X_k(double a, List<double> z)
        {
            for (int i = 0; i < N; i++)
                X[i] += a * z[i];
        }

        private void R_k(double a, List<double> p)
        {
            for (int i = 0; i < N; i++)
                r[i] -= a * p[i];
        }
        private double Norm(List<double> X)
        {
            double norma = 0;
            for (int i = 0; i < N; i++)
            {
                norma += X[i] * X[i];
            }
            norma = Math.Sqrt(norma);
            return norma;
        }

        private void AVec(List<double> x, List<double> y)
        {
            for (int i = 0; i < N; i++)
            {
                y[i] = d[i] * x[i];
                int i0 = ig[i];
                int i1 = ig[i + 1];
                for (int k = i0; k < i1; k++)
                {
                    int j = jg[k];
                    y[i] += ggl[k] * x[j];
                    y[j] += ggu[k] * x[i];
                }
            }
        }
        private void Z_k(List<double> dat, double b, List<double> d, List<double> z)
        {
            for (int i = 0; i < N; i++)
                z[i] = dat[i] + b * d[i];
        }
        private void vec_DI(List<double> vec, List<double> res)
        {
            for (int i = 0; i < N; i++)
                res[i] = vec[i] / d[i];
        }
        private double _nev()
        {

            int i, j, k;
            int i0, i1;
            for (i = 0; i < N; i++)
            {
                i0 = ig[i];
                i1 = ig[i + 1];
                boof1[i] = pr[i] - d[i] * X[i];//f-Ax0
                for (k = i0; k < i1; k++)
                {
                    j = jg[k];
                    boof1[i] -= ggl[k] * X[j];
                    boof1[j] -= ggu[k] * X[i];
                }
            }
            return (Norm(boof1) / Norm(pr));
        }
    }
        #endregion Private Methods
}

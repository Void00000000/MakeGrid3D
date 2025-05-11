using System.Collections.Generic;
using System.Reflection.PortableExecutable;

namespace MakeGrid3D.Solver
{
    public class Test1
    {
        public Grid2D Grid;
        public GridParams GridParams;

        public void CreateTest() {
            List<double> xw = new List<double> { 1, 2, 6 };
            List<double> yw = new List<double> { 1, 2, 4, 5 };
            SubArea2D sub1 = new SubArea2D(1, 1, 2, 1, 4);
            SubArea2D sub2 = new SubArea2D(2, 2, 3, 2, 3);
            SubArea2D sub3 = new SubArea2D(3, 2, 3, 3, 4);
            List<SubArea2D> subs = new List<SubArea2D> { sub1,sub2,sub3};
            Area2D area = new Area2D(xw,yw,subs,3);
    }
}

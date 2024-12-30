using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeGrid3D.FEM
{
    using basic_function = Func<float, float, float, float, float, float, float, float, float>;
    class Basic_Functions
    {
        float X1(float x, float xmin, float xmax)
        {
            return (xmax - x) / (xmax - xmin);
        }
        float X2(float x, float xmin, float xmax)
        {
            return (x - xmin) / (xmax - xmin);
        }
        float Y1(float y, float ymin, float ymax)
        {
            return (ymax - y) / (ymax - ymin);
        }
        float Y2(float y, float ymin, float ymax)
        {
            return (y - ymin) / (ymax - ymin);
        }
        float X1m(float x, float xmin, float xm)
        {
            return (xm - x) / (xm - xmin);
        }
        float X2m(float x, float xmax, float xm)
        {
            return (x - xm) / (xmax - xm);
        }
        float X3m(float x, float xmax, float xm)
        {
            return (xmax - x) / (xmax - xm);
        }
        float X4m(float x, float xmin, float xm)
        {
            return (x - xmin) / (xm - xmin);
        }
        float Y1m(float y, float ymin, float ym)
        {
            return (ym - y) / (ym - ymin);
        }
        float Y2m(float y, float ymax, float ym)
        {
            return (y - ym) / (ymax - ym);
        }
        float Y3m(float y, float ymax, float ym)
        {
            return (ymax - y) / (ymax - ym);
        }
        float Y4m(float y, float ymin, float ym)
        {
            return (y - ymin) / (ym - ymin);
        }


        //--------------------------------------    Ш        --------------------------------------
        //------------------------------------------------------------------------------------------
        float psi_01(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return X1(x, xmin, xmax) * Y1(y, ymin, ymax);
        }
        float psi_02(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return X2(x, xmin, xmax) * Y1(y, ymin, ymax);
        }
        float psi_03(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return X1(x, xmin, xmax) * Y2(y, ymin, ymax);
        }
        float psi_04(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return X2(x, xmin, xmax) * Y2(y, ymin, ymax);
        }


        float psi_0M11(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_01(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_01(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_0M12(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_01(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_02(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_0M13(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_01(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_03(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_0M14(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_01(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_04(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_0M22(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_02(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_02(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_0M23(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_02(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_03(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_0M24(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_02(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_04(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_0M33(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_03(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_03(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_0M34(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_03(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_04(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_0M44(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_04(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_04(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }


        float psi_0G11(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dx = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_01);
            float dy = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_01);
            return dx * dx + dy * dy;
        }
        float psi_0G12(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_01);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_02);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_01);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_02);
            return dxi * dxj + dyi * dyj;
        }
        float psi_0G13(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_01);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_03);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_01);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_03);
            return dxi * dxj + dyi * dyj;
        }
        float psi_0G14(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_01);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_04);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_01);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_04);
            return dxi * dxj + dyi * dyj;
        }
        float psi_0G22(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dx = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_02);
            float dy = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_02);
            return dx * dx + dy * dy;
        }
        float psi_0G23(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_02);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_03);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_02);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_03);
            return dxi * dxj + dyi * dyj;
        }
        float psi_0G24(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_02);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_04);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_02);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_04);
            return dxi * dxj + dyi * dyj;
        }
        float psi_0G33(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dx = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_03);
            float dy = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_03);
            return dx * dx + dy * dy;
        }
        float psi_0G34(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_03);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_04);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_03);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_04);
            return dxi * dxj + dyi * dyj;
        }
        float psi_0G44(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dx = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_04);
            float dy = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_04);
            return dx * dx + dy * dy;
        }
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------



        //--------------------------------------            --------------------------------------
        //------------------------------------------------------------------------------------------
        float psi_11(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            if (x <= xm)
                return X1m(x, xmin, xm) * Y1(y, ymin, ymax);
            else return 0;
        }
        float psi_12(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            if (x >= xm)
                return X3m(x, xmax, xm) * Y1(y, ymin, ymax);
            else
                return X4m(x, xmin, xm) * Y1(y, ymin, ymax);
        }
        float psi_13(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            if (x >= xm)
                return X2m(x, xmax, xm) * Y1(y, ymin, ymax);
            else return 0;
        }
        float psi_14(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return X1(x, xmin, xmax) * Y2(y, ymin, ymax);
        }
        float psi_15(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return X2(x, xmin, xmax) * Y2(y, ymin, ymax);
        }


        float psi_1M11(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_11(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_11(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_1M12(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_11(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_12(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_1M13(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_11(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_13(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_1M14(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_11(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_14(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_1M15(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_11(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_15(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_1M22(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_12(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_12(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_1M23(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_12(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_13(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_1M24(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_12(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_14(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_1M25(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_12(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_15(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_1M33(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_13(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_13(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_1M34(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_13(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_14(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_1M35(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_13(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_15(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_1M44(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_14(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_14(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_1M45(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_14(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_15(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_1M55(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_15(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_15(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }


        float psi_1G11(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dx = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_11);
            float dy = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_11);
            return dx * dx + dy * dy;
        }
        float psi_1G12(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_11);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_12);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_11);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_12);
            return dxi * dxj + dyi * dyj;
        }
        float psi_1G13(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_11);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_13);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_11);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_13);
            return dxi * dxj + dyi * dyj;
        }
        float psi_1G14(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_11);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_14);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_11);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_14);
            return dxi * dxj + dyi * dyj;
        }
        float psi_1G15(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_11);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_15);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_11);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_15);
            return dxi * dxj + dyi * dyj;
        }
        float psi_1G22(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dx = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_12);
            float dy = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_12);
            return dx * dx + dy * dy;
        }
        float psi_1G23(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_12);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_13);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_12);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_13);
            return dxi * dxj + dyi * dyj;
        }
        float psi_1G24(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_12);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_14);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_12);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_14);
            return dxi * dxj + dyi * dyj;
        }
        float psi_1G25(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_12);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_15);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_12);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_15);
            return dxi * dxj + dyi * dyj;
        }
        float psi_1G33(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dx = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_13);
            float dy = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_13);
            return dx * dx + dy * dy;
        }
        float psi_1G34(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_13);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_14);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_13);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_14);
            return dxi * dxj + dyi * dyj;
        }
        float psi_1G35(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_13);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_15);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_13);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_15);
            return dxi * dxj + dyi * dyj;
        }
        float psi_1G44(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dx = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_14);
            float dy = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_14);
            return dx * dx + dy * dy;
        }
        float psi_1G45(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_14);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_15);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_14);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_15);
            return dxi * dxj + dyi * dyj;
        }
        float psi_1G55(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dx = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_15);
            float dy = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_15);
            return dx * dx + dy * dy;
        }
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------


        //--------------------------------------            --------------------------------------
        //------------------------------------------------------------------------------------------
        float psi_21(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return X1(x, xmin, xmax) * Y1(y, ymin, ymax);
        }
        float psi_22(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            if (y <= ym)
                return Y1m(y, ymin, ym) * X2(x, xmin, xmax);
            else return 0;
        }
        float psi_23(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            if (y <= ym)
                return Y4m(y, ymin, ym) * X2(x, xmin, xmax);
            else
                return Y3m(y, ymax, ym) * X2(x, xmin, xmax);
        }
        float psi_24(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return X1(x, xmin, xmax) * Y2(y, ymin, ymax);
        }
        float psi_25(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            if (y >= ym)
                return Y2m(y, ymax, ym) * X2(x, xmin, xmax);
            else return 0;
        }


        float psi_2M11(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_21(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_21(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_2M12(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_21(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_22(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_2M13(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_21(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_23(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_2M14(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_21(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_24(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_2M15(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_21(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_25(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_2M22(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_22(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_22(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_2M23(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_22(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_23(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_2M24(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_22(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_24(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_2M25(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_22(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_25(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_2M33(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_23(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_23(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_2M34(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_23(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_24(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_2M35(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_23(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_25(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_2M44(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_24(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_24(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_2M45(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_24(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_25(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_2M55(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_25(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_25(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }


        float psi_2G11(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dx = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_21);
            float dy = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_21);
            return dx * dx + dy * dy;
        }
        float psi_2G12(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_21);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_22);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_21);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_22);
            return dxi * dxj + dyi * dyj;
        }
        float psi_2G13(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_21);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_23);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_21);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_23);
            return dxi * dxj + dyi * dyj;
        }
        float psi_2G14(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_21);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_24);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_21);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_24);
            return dxi * dxj + dyi * dyj;
        }
        float psi_2G15(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_21);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_25);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_21);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_25);
            return dxi * dxj + dyi * dyj;
        }
        float psi_2G22(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dx = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_22);
            float dy = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_22);
            return dx * dx + dy * dy;
        }
        float psi_2G23(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_22);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_23);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_22);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_23);
            return dxi * dxj + dyi * dyj;
        }
        float psi_2G24(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_22);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_24);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_22);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_24);
            return dxi * dxj + dyi * dyj;
        }
        float psi_2G25(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_22);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_25);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_22);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_25);
            return dxi * dxj + dyi * dyj;
        }
        float psi_2G33(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dx = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_23);
            float dy = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_23);
            return dx * dx + dy * dy;
        }
        float psi_2G34(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_23);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_24);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_23);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_24);
            return dxi * dxj + dyi * dyj;
        }
        float psi_2G35(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_23);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_25);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_23);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_25);
            return dxi * dxj + dyi * dyj;
        }
        float psi_2G44(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dx = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_24);
            float dy = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_24);
            return dx * dx + dy * dy;
        }
        float psi_2G45(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_24);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_25);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_24);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_25);
            return dxi * dxj + dyi * dyj;
        }
        float psi_2G55(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dx = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_25);
            float dy = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_25);
            return dx * dx + dy * dy;
        }
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------


        //--------------------------------------             --------------------------------------
        //------------------------------------------------------------------------------------------
        float psi_31(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return X1(x, xmin, xmax) * Y1(y, ymin, ymax);
        }
        float psi_32(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return X2(x, xmin, xmax) * Y1(y, ymin, ymax);
        }
        float psi_33(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            if (x <= xm)
                return X1m(x, xmin, xm) * Y2(y, ymin, ymax);
            else return 0;
        }
        float psi_34(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            if (x <= xm)
                return X4m(x, xmin, xm) * Y2(y, ymin, ymax);
            else
                return X3m(x, xmax, xm) * Y2(y, ymin, ymax);
        }
        float psi_35(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            if (x >= xm)
                return X2m(x, xmax, xm) * Y2(y, ymin, ymax);
            else return 0;
        }


        float psi_3M11(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_31(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_31(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_3M12(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_31(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_32(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_3M13(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_31(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_33(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_3M14(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_31(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_34(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_3M15(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_31(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_35(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_3M22(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_32(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_32(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_3M23(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_32(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_33(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_3M24(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_32(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_34(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_3M25(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_32(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_35(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_3M33(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_33(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_33(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_3M34(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_33(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_34(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_3M35(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_33(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_35(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_3M44(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_34(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_34(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_3M45(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_34(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_35(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_3M55(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_35(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_35(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }


        float psi_3G11(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dx = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_31);
            float dy = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_31);
            return dx * dx + dy * dy;
        }
        float psi_3G12(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_31);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_32);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_31);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_32);
            return dxi * dxj + dyi * dyj;
        }
        float psi_3G13(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_31);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_33);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_31);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_33);
            return dxi * dxj + dyi * dyj;
        }
        float psi_3G14(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_31);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_34);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_31);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_34);
            return dxi * dxj + dyi * dyj;
        }
        float psi_3G15(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_31);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_35);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_31);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_35);
            return dxi * dxj + dyi * dyj;
        }
        float psi_3G22(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dx = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_32);
            float dy = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_32);
            return dx * dx + dy * dy;
        }
        float psi_3G23(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_32);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_33);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_32);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_33);
            return dxi * dxj + dyi * dyj;
        }
        float psi_3G24(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_32);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_34);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_32);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_34);
            return dxi * dxj + dyi * dyj;
        }
        float psi_3G25(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_32);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_35);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_32);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_35);
            return dxi * dxj + dyi * dyj;
        }
        float psi_3G33(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dx = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_33);
            float dy = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_33);
            return dx * dx + dy * dy;
        }
        float psi_3G34(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_33);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_34);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_33);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_34);
            return dxi * dxj + dyi * dyj;
        }
        float psi_3G35(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_33);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_35);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_33);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_35);
            return dxi * dxj + dyi * dyj;
        }
        float psi_3G44(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dx = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_34);
            float dy = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_34);
            return dx * dx + dy * dy;
        }
        float psi_3G45(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_34);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_35);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_34);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_35);
            return dxi * dxj + dyi * dyj;
        }
        float psi_3G55(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dx = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_35);
            float dy = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_35);
            return dx * dx + dy * dy;
        }
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------


        //--------------------------------------           --------------------------------------
        //------------------------------------------------------------------------------------------
        float psi_41(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            if (y <= ym)
                return Y1m(y, ymin, ym) * X1(x, xmin, xmax);
            else return 0;
        }
        float psi_42(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return X2(x, xmin, xmax) * Y1(y, ymin, ymax);
        }
        float psi_43(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            if (y <= ym)
                return Y4m(y, ymin, ym) * X1(x, xmin, xmax);
            else
                return Y3m(y, ymax, ym) * X1(x, xmin, xmax);
        }
        float psi_44(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            if (y >= ym)
                return Y2m(y, ymax, ym) * X1(x, xmin, xmax);
            else return 0;
        }
        float psi_45(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return X2(x, xmin, xmax) * Y2(y, ymin, ymax);
        }


        float psi_4M11(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_41(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_41(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_4M12(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_41(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_42(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_4M13(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_41(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_43(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_4M14(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_41(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_44(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_4M15(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_41(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_45(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_4M22(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_42(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_42(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_4M23(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_42(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_43(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_4M24(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_42(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_44(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_4M25(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_42(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_45(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_4M33(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_43(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_43(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_4M34(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_43(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_44(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_4M35(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_43(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_45(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_4M44(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_44(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_44(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_4M45(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_44(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_45(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }
        float psi_4M55(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            return psi_45(x, y, xmin, xmax, ymin, ymax, xm, ym) * psi_45(x, y, xmin, xmax, ymin, ymax, xm, ym);
        }


        float psi_4G11(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dx = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_41);
            float dy = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_41);
            return dx * dx + dy * dy;
        }
        float psi_4G12(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_41);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_42);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_41);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_42);
            return dxi * dxj + dyi * dyj;
        }
        float psi_4G13(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_41);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_43);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_41);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_43);
            return dxi * dxj + dyi * dyj;
        }
        float psi_4G14(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_41);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_44);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_41);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_44);
            return dxi * dxj + dyi * dyj;
        }
        float psi_4G15(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_41);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_45);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_41);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_45);
            return dxi * dxj + dyi * dyj;
        }
        float psi_4G22(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dx = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_42);
            float dy = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_42);
            return dx * dx + dy * dy;
        }
        float psi_4G23(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_42);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_43);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_42);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_43);
            return dxi * dxj + dyi * dyj;
        }
        float psi_4G24(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_42);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_44);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_42);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_44);
            return dxi * dxj + dyi * dyj;
        }
        float psi_4G25(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_42);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_45);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_42);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_45);
            return dxi * dxj + dyi * dyj;
        }
        float psi_4G33(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dx = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_43);
            float dy = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_43);
            return dx * dx + dy * dy;
        }
        float psi_4G34(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_43);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_44);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_43);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_44);
            return dxi * dxj + dyi * dyj;
        }
        float psi_4G35(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_43);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_45);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_43);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_45);
            return dxi * dxj + dyi * dyj;
        }
        float psi_4G44(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dx = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_44);
            float dy = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_44);
            return dx * dx + dy * dy;
        }
        float psi_4G45(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dxi = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_44);
            float dxj = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_45);
            float dyi = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_44);
            float dyj = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_45);
            return dxi * dxj + dyi * dyj;
        }
        float psi_4G55(float x, float y, float xmin, float xmax, float ymin, float ymax, float xm, float ym)
        {
            Numeric numeric = new Numeric();
            float dx = numeric.diff_x(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_45);
            float dy = numeric.diff_y(x, y, xmin, xmax, ymin, ymax, xm, ym, psi_45);
            return dx * dx + dy * dy;
        }
        //------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------

        
    }
}
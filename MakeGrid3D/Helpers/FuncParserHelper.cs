using System;
using System.Text.RegularExpressions;
using NCalc;

namespace MakeGrid3D.Helpers
{
    /// <summary>
    /// Помощник для парсинга математических выражений
    /// </summary>
    public class FuncParserHelper
    {
        /// <summary>
        /// Парсит выражение с переменными xyt.
        /// </summary>
        /// <param name="exprStr">Текст выражения</param>
        /// <returns>Делегат Func, null если не удалось спарсить</returns>
        public static FunctionXYT ParseXYTExpression(string exprStr)
        {
            try
            {
                string patternPow = @"(\([^\)]+\)|\w+)\s*\^\s*(\([^\)]+\)|\w+)";
                exprStr = Regex.Replace(exprStr, patternPow, "Pow($1,$2)");

                exprStr = Regex.Replace(exprStr, @"\bsin\s*\(", "Sin(", RegexOptions.IgnoreCase);
                exprStr = Regex.Replace(exprStr, @"\bcos\s*\(", "Cos(", RegexOptions.IgnoreCase);
                exprStr = Regex.Replace(exprStr, @"\btan\s*\(", "Tan(", RegexOptions.IgnoreCase);
                exprStr = Regex.Replace(exprStr, @"\bexp\s*\(", "Exp(", RegexOptions.IgnoreCase);
                exprStr = Regex.Replace(exprStr, @"\blog\s*\(", "Log(", RegexOptions.IgnoreCase);
                
                var expression = new Expression(exprStr, EvaluateOptions.IgnoreCase);

                FunctionXYT func = (x, y, t) =>
                {
                    expression.Parameters["x"] = x;
                    expression.Parameters["y"] = y;
                    expression.Parameters["t"] = t;
                    object evalResult = expression.Evaluate();
                    return Convert.ToDouble(evalResult);
                };

                return func;
            }
            catch (Exception ex) 
            {
                LogService.LogError(ex);
                return null;
            }
        }

        /// <summary>
        /// Парсит выражение с переменными xyzt.
        /// </summary>
        /// <param name="exprStr">Текст выражения</param>
        /// <returns>Делегат Func, null если не удалось спарсить</returns>
        public static FunctionXYZT ParseXYZTExpression(string exprStr)
        {
            try
            {
                string patternPow = @"(\([^\)]+\)|\w+)\s*\^\s*(\([^\)]+\)|\w+)";
                exprStr = Regex.Replace(exprStr, patternPow, "Pow($1,$2)");

                exprStr = Regex.Replace(exprStr, @"\bsin\s*\(", "Sin(", RegexOptions.IgnoreCase);
                exprStr = Regex.Replace(exprStr, @"\bcos\s*\(", "Cos(", RegexOptions.IgnoreCase);
                exprStr = Regex.Replace(exprStr, @"\btan\s*\(", "Tan(", RegexOptions.IgnoreCase);
                exprStr = Regex.Replace(exprStr, @"\bexp\s*\(", "Exp(", RegexOptions.IgnoreCase);
                exprStr = Regex.Replace(exprStr, @"\blog\s*\(", "Log(", RegexOptions.IgnoreCase);

                var expression = new Expression(exprStr, EvaluateOptions.IgnoreCase);

                FunctionXYZT func = (x, y, z, t) =>
                {
                    expression.Parameters["x"] = x;
                    expression.Parameters["y"] = y;
                    expression.Parameters["z"] = y;
                    expression.Parameters["t"] = t;
                    object evalResult = expression.Evaluate();
                    return Convert.ToDouble(evalResult);
                };

                return func;
            }
            catch (Exception ex) 
            {
                LogService.LogError(ex);
                return null;
            }
        }

        /// <summary>
        /// Парсит выражение с переменными xy.
        /// </summary>
        /// <param name="exprStr">Текст выражения</param>
        /// <returns>Делегат Func, null если не удалось спарсить</returns>
        public static FunctionXY ParseXYExpression(string exprStr)
        {
            try
            {
                string patternPow = @"(\([^\)]+\)|\w+)\s*\^\s*(\([^\)]+\)|\w+)";
                exprStr = Regex.Replace(exprStr, patternPow, "Pow($1,$2)");

                exprStr = Regex.Replace(exprStr, @"\bsin\s*\(", "Sin(", RegexOptions.IgnoreCase);
                exprStr = Regex.Replace(exprStr, @"\bcos\s*\(", "Cos(", RegexOptions.IgnoreCase);
                exprStr = Regex.Replace(exprStr, @"\btan\s*\(", "Tan(", RegexOptions.IgnoreCase);
                exprStr = Regex.Replace(exprStr, @"\bexp\s*\(", "Exp(", RegexOptions.IgnoreCase);
                exprStr = Regex.Replace(exprStr, @"\blog\s*\(", "Log(", RegexOptions.IgnoreCase);

                var expression = new Expression(exprStr, EvaluateOptions.IgnoreCase);

                FunctionXY func = (x, y) =>
                {
                    expression.Parameters["x"] = x;
                    expression.Parameters["y"] = y;
                    object evalResult = expression.Evaluate();
                    return Convert.ToDouble(evalResult);
                };

                return func;
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return null;
            }
        }

        /// <summary>
        /// Парсит выражение с переменными xyz.
        /// </summary>
        /// <param name="exprStr">Текст выражения</param>
        /// <returns>Делегат Func, null если не удалось спарсить</returns>
        public static FunctionXYZ ParseXYZExpression(string exprStr)
        {
            try
            {
                string patternPow = @"(\([^\)]+\)|\w+)\s*\^\s*(\([^\)]+\)|\w+)";
                exprStr = Regex.Replace(exprStr, patternPow, "Pow($1,$2)");

                exprStr = Regex.Replace(exprStr, @"\bsin\s*\(", "Sin(", RegexOptions.IgnoreCase);
                exprStr = Regex.Replace(exprStr, @"\bcos\s*\(", "Cos(", RegexOptions.IgnoreCase);
                exprStr = Regex.Replace(exprStr, @"\btan\s*\(", "Tan(", RegexOptions.IgnoreCase);
                exprStr = Regex.Replace(exprStr, @"\bexp\s*\(", "Exp(", RegexOptions.IgnoreCase);
                exprStr = Regex.Replace(exprStr, @"\blog\s*\(", "Log(", RegexOptions.IgnoreCase);

                var expression = new Expression(exprStr, EvaluateOptions.IgnoreCase);

                FunctionXYZ func = (x, y, z) =>
                {
                    expression.Parameters["x"] = x;
                    expression.Parameters["y"] = y;
                    expression.Parameters["z"] = z;
                    object evalResult = expression.Evaluate();
                    return Convert.ToDouble(evalResult);
                };

                return func;
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return null;
            }
        }
    }
}

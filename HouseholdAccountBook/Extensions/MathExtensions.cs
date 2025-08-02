using System;

namespace HouseholdAccountBook.Extensions
{
    /// <summary>
    /// <see cref="Math"/> の拡張メソッドを提供します
    /// </summary>
    public static class MathExtensions
    {
        /// <summary>
        /// 指定した精度の値に切り上げます(0から遠ざかる)
        /// </summary>
        /// <param name="value">丸め対象の値</param>
        /// <param name="digits">戻り値の小数点以下の桁数</param>
        /// <returns>指定した桁数に切り上げた値</returns>
        public static double RoundUp(double value, int digits)
        {
            double coef = Math.Pow(10, digits);

            return value > 0 ? Math.Ceiling(value * coef) / coef : Math.Floor(value * coef) / coef;
        }

        /// <summary>
        /// 指定した精度の値に切り捨てます(0に近づく)
        /// </summary>
        /// <param name="value">丸め対象の値</param>
        /// <param name="digits">戻り値の小数点以下の桁数</param>
        /// <returns>指定した桁数に切り捨てた値</returns>
        public static double RoundDown(double value, int digits)
        {
            double coef = Math.Pow(10, digits);

            return value > 0 ? Math.Floor(value * coef) / coef : Math.Ceiling(value * coef) / coef;
        }

        /// <summary>
        /// 指定した基準値の倍数のうち、最も近い値に数値を切り上げます
        /// </summary>
        /// <param name="value">丸め対象の値</param>
        /// <param name="baseValue">基準値</param>
        /// <returns>指定した基準値の倍数のうち最も近い数値に切り上げた値</returns>
        public static double Ceiling(double value, double baseValue)
        {
            return Math.Ceiling(value / baseValue) * baseValue;
        }

        /// <summary>
        /// 指定した基準値の倍数のうち、最も近い値に数値を切り捨てます
        /// </summary>
        /// <param name="value">丸め対象の値</param>
        /// <param name="baseValue">基準値</param>
        /// <returns>指定した基準値の倍数のうち最も近い数値に切り捨てた値</returns>
        public static double Floor(double value, double baseValue)
        {
            return Math.Floor(value / baseValue) * baseValue;
        }

        /// <summary>
        /// 最大公約数を求めます
        /// </summary>
        /// <param name="value1">値1</param>
        /// <param name="value2">値2</param>
        /// <returns>最大公約数</returns>
        public static int Gcd(int value1, int value2)
        {
            value1 = Math.Abs(value1);
            value2 = Math.Abs(value2);
            static int gcd(int x, int y) => (y == 0 ? x : gcd(y, x % y));
            return value1 > value2 ? gcd(value1, value2) : gcd(value2, value1);
        }

        /// <summary>
        /// 最小公倍数を求めます
        /// </summary>
        /// <param name="value1">値1</param>
        /// <param name="value2">値2</param>
        /// <returns>最小公倍数</returns>
        public static int Lcm(int value1, int value2)
        {
            return (int)Math.Abs(value1 * ((double)value2 / Gcd(value1, value2)));
        }
    }
}

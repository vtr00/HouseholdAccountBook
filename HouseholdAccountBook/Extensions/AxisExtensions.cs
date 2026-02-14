using HouseholdAccountBook.Adapters.Logger;
using OxyPlot.Axes;
using System;
using System.Diagnostics;

namespace HouseholdAccountBook.Extensions
{
    public static class AxisExtensions
    {
        /// <summary>
        /// 軸の範囲を設定する
        /// </summary>
        /// <param name="axis">軸</param>
        /// <param name="minValue">最小値</param>
        /// <param name="maxValue">最大値</param>
        /// <param name="divNum">目盛幅分割数(基準値)</param>
        /// <param name="isDisplayZero">0を表示するか</param>
        public static void SetAxisRange(this Axis axis, double minValue, double maxValue, int divNum, bool isDisplayZero)
        {
            using FuncLog funcLog = new(new { minValue, maxValue, divNum, isDisplayZero }, Log.LogLevel.Trace);

            double unit = 0.25; // 最大値/最小値の求める単位(1以下の値)
            Debug.Assert(unit is > 0 and <= 1);

            // 0を表示範囲に含める
            if (isDisplayZero && !(minValue < 0 && 0 < maxValue)) {
                if (Math.Abs(minValue - 1) < Math.Abs(maxValue + 1)) { minValue = 0; }
                else { maxValue = 0; }
            }

            // マージンを設ける
            double tmpMin = minValue * (minValue < 0 ? 1.05 : 0.95);
            double tmpMax = maxValue * (0 < maxValue ? 1.05 : 0.95);
            // 0はログが計算できないので近い値に置き換える
            tmpMin = (tmpMin is 0 or 1) ? -1 : tmpMin - 1;
            tmpMax = (tmpMax is (-1) or 0) ? 1 : tmpMax + 1;

            double minDigit = Math.Floor(Math.Log10(Math.Abs(tmpMin))); // 最小値 の桁数
            double maxDigit = Math.Floor(Math.Log10(Math.Abs(tmpMax))); // 最大値 の桁数
            double diffDigit = Math.Max(minDigit, maxDigit);

            int minimum = (int)Math.Round(MathExtensions.Floor(tmpMin, Math.Pow(10, diffDigit) * unit)); // 軸の最小値
            int maximum = (int)Math.Round(MathExtensions.Ceiling(tmpMax, Math.Pow(10, diffDigit) * unit)); // 軸の最大値
            if (!(minValue == 0 && maxValue == 0)) {
                if (minValue == 0) { minimum = 0; }
                if (maxValue == 0) { maximum = 0; }
            }
            axis.Minimum = minimum;
            axis.Maximum = maximum;
            int majorStepBase = (int)(Math.Pow(10, diffDigit) * unit);
            axis.MajorStep = Math.Max((int)MathExtensions.Ceiling((double)(maximum - minimum) / divNum, majorStepBase), 1);
            axis.MinorStep = Math.Max(axis.MajorStep / 5, 1);
        }
    }
}

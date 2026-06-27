using System;
using System.Diagnostics;

namespace HouseholdAccountBook.Models.ValueObjects
{
    /// <summary>
    /// 金額VO
    /// </summary>
    [DebuggerDisplay("{Value:Scale}")]
    public struct AmountObj
    {
        /// <summary>
        /// 金額(主単位)
        /// </summary>
        public decimal MainValue { get; set; }
        /// <summary>
        /// 小数点以下桁数
        /// </summary>
        public int Scale { get; set; }
        /// <summary>
        /// 金額(補助単位)
        /// </summary>
        public readonly int SubValue => (int)(this.MainValue * (int)Math.Pow(10, this.Scale));

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="mainValue">金額(主単位)</param>
        public AmountObj(decimal mainValue)
        {
            this.MainValue = mainValue;
            this.Scale = 0;
        }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="mainValue">金額(主単位)</param>
        /// <param name="scale">小数点以下桁数</param>
        public AmountObj(decimal mainValue, int scale)
        {
            this.MainValue = mainValue;
            this.Scale = scale;
        }

        /// <summary>
        /// 金額(補助単位)から生成する
        /// </summary>
        /// <param name="subValue">金額(補助単位)</param>
        /// <param name="scale">小数点以下桁数</param>
        /// <returns></returns>
        public static AmountObj FromSubValue(int subValue, int scale) => new(subValue / (decimal)Math.Pow(10, scale), scale);

        // 単項演算子
        public static AmountObj operator +(AmountObj value) => value;

        public static AmountObj operator -(AmountObj value) => value with { MainValue = -value.MainValue };

        // 加算・減算
        public static AmountObj operator +(AmountObj left, AmountObj right) => new(left.MainValue + right.MainValue, Math.Max(left.Scale, right.Scale));

        public static AmountObj operator -(AmountObj left, AmountObj right) => new(left.MainValue - right.MainValue, Math.Max(left.Scale, right.Scale));

        // 乗算
        public static AmountObj operator *(AmountObj left, AmountObj right) => new(left.MainValue * right.MainValue, left.Scale + right.Scale);

        public static AmountObj operator *(AmountObj left, decimal right) => new(left.MainValue * right, left.Scale);

        public static AmountObj operator *(decimal left, AmountObj right) => new(left * right.MainValue, right.Scale);

        // 除算
        public static AmountObj operator /(AmountObj left, AmountObj right) => new(left.MainValue / right.MainValue, left.Scale);

        public static AmountObj operator /(AmountObj left, decimal right) => new(left.MainValue / right, left.Scale);

        // 剰余
        public static AmountObj operator %(AmountObj left, AmountObj right) => new(left.MainValue % right.MainValue, Math.Max(left.Scale, right.Scale));

        // インクリメント・デクリメント
        public static AmountObj operator ++(AmountObj value) => new(value.MainValue + 1, value.Scale);

        public static AmountObj operator --(AmountObj value) => new(value.MainValue - 1, value.Scale);

        // 比較演算子
        public static bool operator ==(AmountObj left, AmountObj right) => left.MainValue == right.MainValue;

        public static bool operator !=(AmountObj left, AmountObj right) => left.MainValue != right.MainValue;

        public static bool operator <(AmountObj left, AmountObj right) => left.MainValue < right.MainValue;
        public static bool operator <(AmountObj left, decimal right) => left.MainValue < right;
        public static bool operator <(decimal left, AmountObj right) => left < right.MainValue;

        public static bool operator <=(AmountObj left, AmountObj right) => left.MainValue <= right.MainValue;
        public static bool operator <=(AmountObj left, decimal right) => left.MainValue <= right;
        public static bool operator <=(decimal left, AmountObj right) => left <= right.MainValue;

        public static bool operator >(AmountObj left, AmountObj right) => left.MainValue > right.MainValue;
        public static bool operator >(decimal left, AmountObj right) => left > right.MainValue;
        public static bool operator >(AmountObj left, decimal right) => left.MainValue > right;

        public static bool operator >=(AmountObj left, AmountObj right) => left.MainValue >= right.MainValue;
        public static bool operator >=(AmountObj left, decimal right) => left.MainValue >= right;
        public static bool operator >=(decimal left, AmountObj right) => left >= right.MainValue;

        // 暗黙・明示変換
        public static explicit operator decimal(AmountObj value) => value.MainValue;

        public static explicit operator AmountObj(decimal value) => new(value);

        public override readonly string ToString() => this.MainValue.ToString($"F{this.Scale}");

        public override readonly bool Equals(object obj)
        {
            bool ret = false;
            if (obj is AmountObj value) {
                ret = this.MainValue == value.MainValue && this.Scale == value.Scale;
            }
            return ret;
        }

        public override readonly int GetHashCode() => HashCode.Combine(this.MainValue, this.Scale);
    }
}

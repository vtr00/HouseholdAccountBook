using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.UiDto;
using System;
using System.Diagnostics;

namespace HouseholdAccountBook.Models.ValueObjects
{
    /// <summary>
    /// 金額VO
    /// </summary>
    [DebuggerDisplay("{MainValue}")]
    public struct AmountObj
    {
        /// <summary>
        /// 金額(主単位)
        /// </summary>
        public decimal MainValue { get; set; }
        /// <summary>
        /// アセットID
        /// </summary>
        /// <remarks>初期化以降の書換は不可</remarks>
        public AssetIdObj AssetId { get; init; }

        /// <summary>
        /// 小数点以下桁数
        /// </summary>
        public readonly int Scale => AssetService.Instance.GetAssetModel(this.AssetId).Scale;
        /// <summary>
        /// 金額(補助単位)
        /// </summary>
        public readonly int SubValue => (int)(this.MainValue * (int)Math.Pow(10, this.Scale));

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="mainValue">金額(主単位)</param>
        /// <param name="assetId">アセットID</param>
        public AmountObj(decimal mainValue, AssetIdObj assetId)
        {
            this.MainValue = mainValue;
            this.AssetId = assetId;
        }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="mainValue">金額(主単位)</param>
        public AmountObj(decimal mainValue)
        {
            this.MainValue = mainValue;
            this.AssetId = AssetIdObj.System;
        }

        /// <summary>
        /// 金額(補助単位)から生成する
        /// </summary>
        /// <param name="subValue">金額(補助単位)</param>
        /// <param name="assetId">アセットID</param>
        /// <returns></returns>
        public static AmountObj FromSubValue(int subValue, AssetIdObj assetId)
        {
            AssetModel asset = AssetService.Instance.GetAssetModel(assetId);
            return new(subValue / (decimal)Math.Pow(10, asset.Scale), assetId);
        }

        // 単項演算子
        public static AmountObj operator +(AmountObj value) => value;

        public static AmountObj operator -(AmountObj value) => value with { MainValue = -value.MainValue };

        // 加算・減算
        public static AmountObj operator +(AmountObj left, AmountObj right) =>
            left.AssetId != right.AssetId ? throw new InvalidOperationException() : new(left.MainValue + right.MainValue, left.AssetId);

        public static AmountObj operator -(AmountObj left, AmountObj right) =>
            left.AssetId != right.AssetId ? throw new InvalidOperationException() : new(left.MainValue - right.MainValue, left.AssetId);

        // 乗算
        public static AmountObj operator *(AmountObj left, AmountObj right) => throw new InvalidOperationException();
        public static AmountObj operator *(AmountObj left, decimal right) => new(left.MainValue * right, left.AssetId);
        public static AmountObj operator *(decimal left, AmountObj right) => new(left * right.MainValue, right.AssetId);

        // 除算
        public static AmountObj operator /(AmountObj left, AmountObj right) => throw new InvalidOperationException();
        public static AmountObj operator /(AmountObj left, decimal right) => new(left.MainValue / right, left.AssetId);
        public static AmountObj operator /(decimal left, AmountObj right) => throw new InvalidOperationException();

        // 剰余
        public static AmountObj operator %(AmountObj left, AmountObj right) => throw new InvalidOperationException();
        public static AmountObj operator %(AmountObj left, decimal right) => new(left.MainValue % right, left.AssetId);

        // インクリメント・デクリメント
        public static AmountObj operator ++(AmountObj value) => new(value.MainValue + 1, value.AssetId);
        public static AmountObj operator --(AmountObj value) => new(value.MainValue - 1, value.AssetId);

        // 比較演算子
        public static bool operator ==(AmountObj left, AmountObj right) => left.AssetId == right.AssetId && left.MainValue == right.MainValue;

        public static bool operator !=(AmountObj left, AmountObj right) => left.AssetId != right.AssetId || left.MainValue != right.MainValue;

        public static bool operator <(AmountObj left, AmountObj right) => left.AssetId != right.AssetId ? throw new InvalidOperationException() : left.MainValue < right.MainValue;
        public static bool operator <(AmountObj left, decimal right) => left.MainValue < right;
        public static bool operator <(decimal left, AmountObj right) => left < right.MainValue;

        public static bool operator <=(AmountObj left, AmountObj right) => left.AssetId != right.AssetId ? throw new InvalidOperationException() : left.MainValue <= right.MainValue;
        public static bool operator <=(AmountObj left, decimal right) => left.MainValue <= right;
        public static bool operator <=(decimal left, AmountObj right) => left <= right.MainValue;

        public static bool operator >(AmountObj left, AmountObj right) => left.AssetId != right.AssetId ? throw new InvalidOperationException() : left.MainValue > right.MainValue;
        public static bool operator >(decimal left, AmountObj right) => left > right.MainValue;
        public static bool operator >(AmountObj left, decimal right) => left.MainValue > right;

        public static bool operator >=(AmountObj left, AmountObj right) => left.AssetId != right.AssetId ? throw new InvalidOperationException() : left.MainValue >= right.MainValue;
        public static bool operator >=(AmountObj left, decimal right) => left.MainValue >= right;
        public static bool operator >=(decimal left, AmountObj right) => left >= right.MainValue;

        // 暗黙・明示変換
        public static explicit operator decimal(AmountObj value) => value.MainValue;
        public static explicit operator AmountObj(decimal value) => new(value, AssetIdObj.System);

        public override readonly string ToString() => $"MainValue:{this.MainValue} AssetId:{this.AssetId}";

        public override readonly bool Equals(object obj)
        {
            bool ret = false;
            if (obj is AmountObj value) {
                ret = this.MainValue == value.MainValue && this.AssetId == value.AssetId;
            }
            return ret;
        }

        public override readonly int GetHashCode() => HashCode.Combine(this.MainValue, this.AssetId);
    }
}

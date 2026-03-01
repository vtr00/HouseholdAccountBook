using System;

namespace HouseholdAccountBook.Models.UiDto
{
    /// <summary>
    /// 店舗Model
    /// </summary>
    public class ShopModel(string name)
    {
        #region プロパティ
        /// <summary>
        /// 店舗
        /// </summary>
        public string Name { get; init; } = name;

        /// <summary>
        /// 使用回数
        /// </summary>
        public int UsedCount { get; init; }

        /// <summary>
        /// 最終使用日
        /// </summary>
        public DateTime? UsedTime { get; init; }
        #endregion

        public override string ToString() => this.Name;

        public static implicit operator string(ShopModel shop) => shop.Name;
        public static implicit operator ShopModel(string name) => new(name);
    }
}

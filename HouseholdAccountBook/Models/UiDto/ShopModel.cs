using HouseholdAccountBook.Models.ValueObjects;
using System;
using System.Diagnostics;

namespace HouseholdAccountBook.Models.UiDto
{
    /// <summary>
    /// 店舗Model
    /// </summary>
    /// <param name="Name">店名</param>
    [DebuggerDisplay("{Name}")]
    public record class ShopModel(string Name)
    {
        #region プロパティ
        /// <summary>
        /// 項目ID
        /// </summary>
        public ItemIdObj ItemId { get; set; }

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

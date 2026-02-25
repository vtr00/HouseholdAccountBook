using System;

namespace HouseholdAccountBook.Models.DomainModels
{
    /// <summary>
    /// 店舗Model
    /// </summary>
    public class ShopModel
    {
        #region プロパティ
        /// <summary>
        /// 店舗名
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// 使用回数
        /// </summary>
        public int UsedCount { get; init; }

        /// <summary>
        /// 最終使用日
        /// </summary>
        public DateTime? UsedTime { get; init; }
        #endregion
    }
}

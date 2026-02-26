using HouseholdAccountBook.Models.DomainModels;
using System;

namespace HouseholdAccountBook.ViewModels.Component
{
    /// <summary>
    /// 店舗VM
    /// </summary>
    public class ShopViewModel
    {
        #region プロパティ
        /// <summary>
        /// 店舗
        /// </summary>
        public ShopModel Shop { get; init; }

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

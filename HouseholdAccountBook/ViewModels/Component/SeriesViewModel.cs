using System;
using System.Collections.Generic;

namespace HouseholdAccountBook.ViewModels.Component
{
    /// <summary>
    /// 系列VM(一覧の行、グラフの系列データ)
    /// </summary>
    public class SeriesViewModel
    {
        #region プロパティ
        /// <summary>
        /// 収支種別
        /// </summary>
        public int BalanceKind { get; set; } = -1;
        /// <summary>
        /// 収支名
        /// </summary>
        public string BalanceName { get; set; } = string.Empty;
        /// <summary>
        /// 分類ID
        /// </summary>
        public int CategoryId { get; set; } = -1;
        /// <summary>
        /// 分類名
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;
        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; } = -1;
        /// <summary>
        /// 項目名
        /// </summary>
        public string ItemName { get; set; } = string.Empty;
        /// <summary>
        /// 値
        /// </summary>
        public List<int> Values { get; set; }
        /// <summary>
        /// 開始日
        /// </summary>
        public List<DateTime> StartDates { get; set; }
        /// <summary>
        /// 終了日
        /// </summary>
        public List<DateTime> EndDates { get; set; }

        /// <summary>
        /// 平均
        /// </summary>
        public int? Average { get; set; }
        /// <summary>
        /// 合計
        /// </summary>
        public int? Total { get; set; }

        /// <summary>
        /// その他名称
        /// </summary>
        public string OtherName { get; set; } = string.Empty;

        /// <summary>
        /// 表示名
        /// </summary>
        public string DisplayedName {
            get {
                return this.BalanceName != string.Empty && this.CategoryName != string.Empty
                    ? this.ItemName != string.Empty
                        ? $"{this.BalanceName} > {this.CategoryName} > {this.ItemName}"
                        : $"{this.BalanceName} > {this.CategoryName}"
                    : this.OtherName;
            }
            private set { }
        }
        /// <summary>
        /// 一覧表示名(サマリーや一覧に表示する名称)
        /// </summary>
        public string ListName {
            get {
                return this.ItemName != string.Empty
                    ? $"  {this.ItemName}"
                    : this.CategoryName != string.Empty
                        ? this.CategoryName
                        : this.BalanceName != string.Empty
                            ? this.BalanceName
                            : this.OtherName;
            }
            private set { }
        }
        #endregion

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public SeriesViewModel() { }
        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="summary">コピー元の概要VM</param>
        public SeriesViewModel(SummaryViewModel summary)
        {
            this.BalanceKind = summary.BalanceKind;
            this.BalanceName = summary.BalanceName;
            this.CategoryId = summary.CategoryId;
            this.CategoryName = summary.CategoryName;
            this.ItemId = summary.ItemId;
            this.ItemName = summary.ItemName;
            this.OtherName = summary.OtherName;

            this.Total = summary.Total;
        }
    }
}

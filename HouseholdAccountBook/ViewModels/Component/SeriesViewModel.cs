using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.DomainModels;
using HouseholdAccountBook.ViewModels.Abstract;
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
        public string BalanceName => this.BalanceKind == -1 ? string.Empty : UiConstants.BalanceKindStr[(BalanceKind)this.BalanceKind];

        /// <summary>
        /// 分類
        /// </summary>
        public CategoryModel Category { get; set; } = new CategoryModel();
        /// <summary>
        /// 項目
        /// </summary>
        public ItemModel Item { get; set; } = new ItemModel();

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
                return this.BalanceName != string.Empty && this.Category.Name != string.Empty
                    ? this.Item.Name != string.Empty
                        ? $"{this.BalanceName} > {this.Category.Name} > {this.Item.Name}"
                        : $"{this.BalanceName} > {this.Category.Name}"
                    : this.OtherName;
            }
        }
        /// <summary>
        /// 一覧表示名(サマリーや一覧に表示する名称)
        /// </summary>
        public string ListName {
            get {
                return this.Item.Name != string.Empty
                    ? $"  {this.Item.Name}"
                    : this.Category.Name != string.Empty
                        ? this.Category.Name
                        : this.BalanceName != string.Empty
                            ? this.BalanceName
                            : this.OtherName;
            }
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
            this.Category = summary.Category;
            this.Item = summary.Item;
            this.OtherName = summary.OtherName;

            this.Total = summary.Total;
        }
    }
}

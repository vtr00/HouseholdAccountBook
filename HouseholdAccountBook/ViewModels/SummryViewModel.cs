using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 合計VM
    /// </summary>
    public class SummaryViewModel
    {
        /// <summary>
        /// 収支種別
        /// </summary>
        public int BalanceKind { get; set; }
        /// <summary>
        /// カテゴリID
        /// </summary>
        public int CategoryId { get; set; }
        /// <summary>
        /// カテゴリ名
        /// </summary>
        public string CategoryName { get; set; }
        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; }
        /// <summary>
        /// 項目名
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 合計
        /// </summary>
        public int Summary { get; set; }

        /// <summary>
        /// 背景色
        /// </summary>
        public SolidColorBrush ColorBrush
        {
            get {
                if (BalanceKind == -1) {
                    return new SolidColorBrush(Color.FromRgb(0xFF, 0xB0, 0xB0));
                }
                if (CategoryId == -1) {
                    return new SolidColorBrush(Color.FromRgb(0xB0, 0xFF, 0xB0));
                }
                if (ItemId == -1) {
                    return new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xB0));
                }
                return new SolidColorBrush(Color.FromRgb(0xEE, 0xE3, 0xFB));
            }
        }
    }
}

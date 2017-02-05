using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 帳簿項目VM
    /// </summary>
    public class ActionViewModel
    {
        /// <summary>
        /// 帳簿項目ID
        /// </summary>
        public int ActionId { get; set; }
        /// <summary>
        /// 時刻
        /// </summary>
        public DateTime ActTime { get; set; }
        /// <summary>
        /// 項目名
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 収入
        /// </summary>
        public int? Income { get; set; }
        /// <summary>
        /// 支出
        /// </summary>
        public int? Outgo { get; set; }
        /// <summary>
        /// 残高
        /// </summary>
        public int Balance { get; set; }
        /// <summary>
        /// 店舗名
        /// </summary>
        public string ShopName { get; set; }
        /// <summary>
        /// 備考
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// グループID
        /// </summary>
        public int? GroupId { get; set; }

        /// <summary>
        /// 文字色
        /// </summary>
        public SolidColorBrush ColorBrush
        {
            get {
                if (ActTime <= DateTime.Now) {
                    return new SolidColorBrush(Colors.Black);
                }
                return new SolidColorBrush(Colors.Gray);
            }
        }
    }
}

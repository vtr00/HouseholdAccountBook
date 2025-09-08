using System.Windows.Controls;
using System.Windows.Input;

namespace HouseholdAccountBook.Views.UserControls
{
    public partial class NumericInputButton : UserControl, ICommandSource
    {
        /// <summary>
        /// 入力種別
        /// </summary>
        public enum InputKind
        {
            /// <summary>
            /// 未入力
            /// </summary>
            Unputed,
            /// <summary>
            /// 数値
            /// </summary>
            Number,
            /// <summary>
            /// バックスペース
            /// </summary>
            BackSpace,
            /// <summary>
            /// クリア
            /// </summary>
            Clear
        }
    }
}

using HouseholdAccountBook.UserControls;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// VM
    /// </summary>
    class NumericUpDownViewModel
    {
        /// <summary>
        /// 数値入力ボタンの入力値
        /// </summary>
        public int? InputedValue { get; set; }
        /// <summary>
        /// 数値入力ボタンの入力種別
        /// </summary>
        public NumericInputButton.InputKind InputedKind { get; set; }
    }
}

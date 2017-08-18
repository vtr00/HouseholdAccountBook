using HouseholdAccountBook.UserControls;
using Prism.Mvvm;
using System.Threading;
using System.Threading.Tasks;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// VM
    /// </summary>
    class NumericUpDownViewModel : BindableBase
    {
        /// <summary>
        /// IsOpenの瞬時値
        /// </summary>
        private bool localIsOpen = default(bool);

        #region プロパティ
        /// <summary>
        /// 数値入力ボタンの入力値
        /// </summary>
        #region InputedValue
        public int? InputedValue
        {
            get { return _InputedValue; }
            set { SetProperty(ref _InputedValue, value); }
        }
        private int? _InputedValue = default(int?);
        #endregion

        /// <summary>
        /// 数値入力ボタンの入力種別
        /// </summary>
        #region InputedKind
        public NumericInputButton.InputKind InputedKind
        {
            get { return _InputedKind; }
            set { SetProperty(ref _InputedKind, value); }
        }
        private NumericInputButton.InputKind _InputedKind = default(NumericInputButton.InputKind);
        #endregion

        /// <summary>
        /// <see cref="System.Windows.Controls.Primitives.Popup"/> の表示状態
        /// </summary>
        #region IsOpen
        public bool IsOpen
        {
            get { return _IsOpen; }
            set { SetProperty(ref _IsOpen, value); }
        }
        private bool _IsOpen = default(bool);
        #endregion

        /// <summary>
        /// <see cref="NumericUpDown"/> のフォーカスの状態
        /// </summary>
        #region NumericUpDownFocused
        public bool NumericUpDownFocused
        {
            get { return _NumericUpDownFocused; }
            set {
                _NumericUpDownFocused = value;
                SetIsOpen();
            }
        }
        private bool _NumericUpDownFocused = default(bool);
        #endregion

        /// <summary>
        /// <see cref="System.Windows.Controls.Primitives.Popup"/> のフォーカスの状態
        /// </summary>
        #region PopupFocused
        public bool PopupFocused
        {
            get { return _PopupFocused; }
            set {
                _PopupFocused = value;
                SetIsOpen();
            }
        }
        private bool _PopupFocused = default(bool);
        #endregion
        #endregion

        /// <summary>
        /// <see cref="System.Windows.Controls.Primitives.Popup"/> の表示状態を更新する
        /// </summary>
        private async void SetIsOpen()
        {
            this.localIsOpen = this.NumericUpDownFocused || this.PopupFocused;

            // true の場合は即座に更新し、false の場合は遅延して反映させる
            if (this.localIsOpen == true) {
                this.IsOpen = true;
            }
            else {
                await Task.Run(()=> { Thread.Sleep(10); });
                this.IsOpen = this.localIsOpen || false;
            }
        }
    }
}

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
        private bool localIsOpen = default;

        #region プロパティ
        /// <summary>
        /// 数値入力ボタンの入力値
        /// </summary>
        #region InputedValue
        public int? InputedValue
        {
            get => this._InputedValue;
            set => this.SetProperty(ref this._InputedValue, value);
        }
        private int? _InputedValue = default;
        #endregion

        /// <summary>
        /// 数値入力ボタンの入力種別
        /// </summary>
        #region InputedKind
        public NumericInputButton.InputKind InputedKind
        {
            get => this._InputedKind;
            set => this.SetProperty(ref this._InputedKind, value);
        }
        private NumericInputButton.InputKind _InputedKind = default;
        #endregion

        /// <summary>
        /// <see cref="System.Windows.Controls.Primitives.Popup"/> の表示状態
        /// </summary>
        #region IsOpen
        public bool IsOpen
        {
            get => this._IsOpen;
            set => this.SetProperty(ref this._IsOpen, value);
        }
        private bool _IsOpen = default;
        #endregion

        /// <summary>
        /// <see cref="NumericUpDown"/> のフォーカスの状態
        /// </summary>
        #region NumericUpDownFocused
        public bool NumericUpDownFocused
        {
            get => this._NumericUpDownFocused;
            set {
                this._NumericUpDownFocused = value;
                this.SetIsOpen();
            }
        }
        private bool _NumericUpDownFocused = default;
        #endregion

        /// <summary>
        /// <see cref="System.Windows.Controls.Primitives.Popup"/> のフォーカスの状態
        /// </summary>
        #region PopupFocused
        public bool PopupFocused
        {
            get => this._PopupFocused;
            set {
                this._PopupFocused = value;
                this.SetIsOpen();
            }
        }
        private bool _PopupFocused = default;
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
                await Task.Run(() => { Thread.Sleep(10); });
                this.IsOpen = this.localIsOpen || false;
            }
        }
    }
}

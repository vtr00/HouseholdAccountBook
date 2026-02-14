using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.Views.UserControls;
using System.Threading;
using System.Threading.Tasks;

namespace HouseholdAccountBook.ViewModels.UserControls
{
    /// <summary>
    /// <see cref="NumericInputButton"/> VM
    /// </summary>
    public class NumericUpDownViewModel : BindableBase
    {
        #region フィールド
        /// <summary>
        /// IsOpenの瞬時値
        /// </summary>
        private bool mLocalIsOpen;
        #endregion

        #region プロパティ
        /// <summary>
        /// 数値入力ボタンの入力値
        /// </summary>
        #region InputedValue
        public int? InputedValue {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 数値入力ボタンの入力種別
        /// </summary>
        #region InputedKind
        public NumericInputButton.InputKind InputedKind {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// <see cref="System.Windows.Controls.Primitives.Popup"/> の表示状態
        /// </summary>
        #region IsOpen
        public bool IsOpen {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// <see cref="NumericUpDown"/> のフォーカスの状態
        /// </summary>
        #region NumericUpDownFocused
        public bool NumericUpDownFocused {
            get;
            set {
                field = value;
                this.UpdateIsOpenProperty();
            }
        }
        #endregion

        /// <summary>
        /// <see cref="System.Windows.Controls.Primitives.Popup"/> のフォーカスの状態
        /// </summary>
        #region PopupFocused
        public bool PopupFocused {
            get;
            set {
                field = value;
                this.UpdateIsOpenProperty();
            }
        }
        #endregion
        #endregion

        /// <summary>
        /// <see cref="System.Windows.Controls.Primitives.Popup"/> の表示状態を更新する
        /// </summary>
        private async void UpdateIsOpenProperty()
        {
            this.mLocalIsOpen = this.NumericUpDownFocused || this.PopupFocused;

            // true の場合は即座に更新し、false の場合は遅延して反映させる
            if (this.mLocalIsOpen == true) {
                this.IsOpen = true;
            }
            else {
                await Task.Run(static () => Thread.Sleep(10));
                this.IsOpen = this.mLocalIsOpen || false;
            }
        }
    }
}

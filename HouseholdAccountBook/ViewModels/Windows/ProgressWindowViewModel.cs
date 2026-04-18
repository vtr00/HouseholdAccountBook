using HouseholdAccountBook.ViewModels.Abstract;
using System;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels.Windows
{
    /// <summary>
    /// 進捗ウィンドウVM
    /// </summary>
    public class ProgressWindowViewModel : BindableBase
    {
        /// <summary>
        /// キャンセル時イベント
        /// </summary>
        public event EventHandler Canceled;

        /// <summary>
        /// 進捗率[%]
        /// </summary>
        public int ProgressValue {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    this.RaisePropertyChanged(nameof(this.IsIndeterminate));
                }
            }
        }
        /// <summary>
        /// 進捗率が不確定か
        /// </summary>
        public bool IsIndeterminate => this.ProgressValue < 0;

        /// <summary>
        /// キャンセルコマンド
        /// </summary>
        public ICommand CancelCommand => field ??= new RelayCommand(() => this.Canceled?.Invoke(this, EventArgs.Empty), () => this.Canceled != null);
    }
}

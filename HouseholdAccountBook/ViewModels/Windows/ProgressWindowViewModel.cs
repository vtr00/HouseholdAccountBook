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
        public ICommand CancelCommand => new RelayCommand(this.CancelCommand_Execute, this.CancelCommand_CanExecute);

        /// <summary>
        /// キャンセルコマンド実行可能か
        /// </summary>
        /// <returns></returns>
        public bool CancelCommand_CanExecute() => this.Canceled != null;
        /// <summary>
        /// キャンセルコマンド処理
        /// </summary>
        public void CancelCommand_Execute() => this.Canceled?.Invoke(this, EventArgs.Empty);
    }
}

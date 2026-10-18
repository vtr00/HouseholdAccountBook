using HouseholdAccountBook.Models.Args;
using System;

namespace HouseholdAccountBook.ViewModels.Abstract
{
    /// <summary>
    /// 保存状態を保持するVMの基底クラス
    /// </summary>
    public abstract class DirtyStateViewModelBase : ViewModelBase, IDirtyState
    {
        #region イベント
        /// <summary>
        /// <see cref="IsDirty"/> 変更時に発火するイベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<bool>> IsDirtyChanged;
        #endregion

        #region Bindプロパティ
        /// <summary>
        /// 未保存か
        /// </summary>
        /// <remarks><see cref="EnableCheckDirty"/> = false の場合は、常に false を返す</remarks>
        public bool IsDirty {
            get => this.EnableCheckDirty && field;
            protected set {
                bool oldValue = this.IsDirty;
                if (this.SetProperty(ref field, value)) {
                    this.IsDirtyChanged?.Invoke(this, new() { OldValue = oldValue, NewValue = field });
                }
            }
        }

        /// <summary>
        /// 未保存チェックをするか
        /// </summary>
        public abstract bool EnableCheckDirty { get; set; }
        #endregion

        /// <summary>
        /// 保存済として確定する
        /// </summary>
        public abstract void AcceptChanges();
    }
}

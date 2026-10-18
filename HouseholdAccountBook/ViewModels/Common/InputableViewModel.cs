using HouseholdAccountBook.Models.Args;
using HouseholdAccountBook.ViewModels.Abstract;
using System;
using System.Collections.Generic;

namespace HouseholdAccountBook.ViewModels.Common
{
    /// <summary>
    /// ユーザ入力が可能なUI項目のVM
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InputableViewModel<T> : DirtyStateViewModelBase
    {
        #region イベント
        /// <summary>
        /// 入力値変更イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<T>> InputValueChanged;
        #endregion

        #region プロパティ
        /// <summary>
        /// 読込時の値
        /// </summary>
        public T LoadedValue {
            get;
            set {
                _ = this.SetProperty(ref field, value);

                // 読込値を設定した場合は、入力値も同じ値にする
                this.InputedValue = value;
                this.UpdateIsDirty();
            }
        }

        /// <summary>
        /// UI入力時の値
        /// </summary>
        public T InputedValue {
            get;
            set {
                T oldValue = field;
                if (this.SetProperty(ref field, value)) {
                    this.InputValueChanged?.Invoke(this, new ChangedEventArgs<T>() { OldValue = oldValue, NewValue = field });
                    this.UpdateIsDirty();
                }
            }
        }

        public override bool EnableCheckDirty { get; set; } = true;
        #endregion

        /// <summary>
        /// 未保存状態を更新する
        /// </summary>
        protected void UpdateIsDirty() => this.IsDirty = !EqualityComparer<T>.Default.Equals(this.LoadedValue, this.InputedValue);
        public override void AcceptChanges()
        {
            this.LoadedValue = this.InputedValue;
            this.UpdateIsDirty();
        }
    }
}

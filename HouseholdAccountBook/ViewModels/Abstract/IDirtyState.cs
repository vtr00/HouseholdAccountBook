using HouseholdAccountBook.Models.Args;
using System;

namespace HouseholdAccountBook.ViewModels.Abstract
{
    /// <summary>
    /// 保存状態を管理する ViewModel のためのインターフェイス
    /// </summary>
    public interface IDirtyState
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
        abstract bool IsDirty { get; }

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

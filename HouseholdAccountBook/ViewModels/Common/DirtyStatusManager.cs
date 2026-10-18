using HouseholdAccountBook.Infrastructure.Services;
using HouseholdAccountBook.Models.Args;
using HouseholdAccountBook.ViewModels.Abstract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace HouseholdAccountBook.ViewModels.Common
{
    /// <summary>
    /// 保存状態の管理クラス
    /// </summary>
    public class DirtyStatusManager : IDirtyState, INotifyPropertyChanged
    {
        #region イベント
        /// <summary>
        /// <see cref="IsDirty"/> 変更時に発火するイベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<bool>> IsDirtyChanged;

        /// <summary>
        /// プロパティ変更時に発火するイベント
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region フィールド
        /// <summary>
        /// 子 <see cref="IDirtyState"> リスト
        /// </summary>
        private readonly ObservableCollection<IDirtyState> mChildrenVM = [];
        /// <summary>
        /// 未保存の <see cref="IDirtyState"/> の個数
        /// </summary>
        private int mCount = 0;
        #endregion

        #region Bindプロパティ
        public bool IsDirty => this.EnableCheckDirty && 0 < this.mCount;

        public bool EnableCheckDirty {
            get;
            set {
                foreach (IDirtyState vm in this.mChildrenVM) {
                    vm.EnableCheckDirty = value;
                }
                field = value;
            }
        } = true;
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DirtyStatusManager()
        {
            this.mChildrenVM.CollectionChanged += (sender, e) => {
                using IDisposable _ = LockService.Enter();

                bool oldValue = this.IsDirty;

                if (e.OldItems != null) {
                    foreach (object obj in e.OldItems) {
                        if (obj is IDirtyState vm) {
                            if (vm.IsDirty) { --this.mCount; }
                            vm.IsDirtyChanged -= this.EditablePartViewModel_IsDirtyChanged;
                        }
                    }
                }
                if (e.NewItems != null) {
                    foreach (object obj in e.NewItems) {
                        if (obj is IDirtyState vm) {
                            if (vm.IsDirty) { ++this.mCount; }
                            vm.IsDirtyChanged += this.EditablePartViewModel_IsDirtyChanged;
                        }
                    }
                }

                if (oldValue != this.IsDirty) {
                    this.IsDirtyChanged?.Invoke(this, new() { OldValue = oldValue, NewValue = this.IsDirty });
                    this.RaisePropertyChanged(nameof(this.IsDirty));
                }
            };
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DirtyStatusManager(params List<IDirtyState> list) : this() => this.RegisterVMList(list);

        /// <summary>
        /// 未保存状態が変化したときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditablePartViewModel_IsDirtyChanged(object sender, ChangedEventArgs<bool> e)
        {
            using IDisposable _ = LockService.Enter();

            bool oldIsDirty = this.IsDirty;

            if (e.NewValue) { ++this.mCount; }
            if (!e.NewValue) { --this.mCount; }

            if (oldIsDirty != this.IsDirty) {
                this.IsDirtyChanged?.Invoke(this, new() { OldValue = oldIsDirty, NewValue = this.IsDirty });
                this.RaisePropertyChanged(nameof(this.IsDirty));
            }
        }

        /// <summary>
        /// 指定されたプロパティ名に対して変更通知を発行する
        /// </summary>
        /// <param name="propertyName">変更されたプロパティの名前</param>
        protected void RaisePropertyChanged(string propertyName = null) => this.PropertyChanged?.Invoke(this, new(propertyName));

        /// <summary>
        /// 子 <see cref="IDirtyState"/> をリストで登録する
        /// </summary>
        /// <param name="list"><see cref="IDirtyState"/> リスト</param>
        public void RegisterVMList(List<IDirtyState> list) => list.ForEach(this.mChildrenVM.Add);
        /// <summary>
        /// 子 <see cref="IDirtyState"/> をリストで登録解除する
        /// </summary>
        /// <param name="list"><see cref="IDirtyState"/> リスト</param>
        public void UnregisterVMList(List<IDirtyState> list) => list.ForEach((vm) => _ = this.mChildrenVM.Remove(vm));

        public void AcceptChanges()
        {
            foreach (IDirtyState vm in this.mChildrenVM) {
                vm.AcceptChanges();
            }
        }
    }
}

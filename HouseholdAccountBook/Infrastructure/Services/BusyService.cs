using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities;
using System;
using System.ComponentModel;

namespace HouseholdAccountBook.Infrastructure.Services
{
    /// <summary>
    /// 処理中かどうかを管理し、状態変化を通知するサービス
    /// </summary>
    public class BusyService : INotifyPropertyChanged
    {
        #region イベント
        /// <summary>
        /// プロパティ変更時に発火するイベント
        /// WPFのデータバインディングに必要
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region プロパティ
        /// <summary>
        /// 処理中状態を示すカウンタ
        /// </summary>
        private int Count {
            get;
            set {
                using IDisposable _ = LockService.Enter();
                bool oldIsBusy = this.IsBusy;
                if (field < value) {
                    field = value;
                    Log.Debug($"Increment to count:{field}");
                }
                if (value < field) {
                    field = value;
                    Log.Debug($"Decrement to count:{field}");
                }

                if (oldIsBusy != this.IsBusy) {
                    this.RaisePropertyChanged(nameof(this.IsBusy));
                }
            }
        }

        /// <summary>
        /// 現在処理中であるかどうか
        /// </summary>
        public bool IsBusy => 0 < this.Count;
        #endregion

        /// <summary>
        /// 指定されたプロパティ名に対して変更通知を発行する
        /// </summary>
        /// <param name="propertyName">変更されたプロパティの名前</param>
        protected void RaisePropertyChanged(string propertyName = null) => this.PropertyChanged?.Invoke(this, new(propertyName));

        /// <summary>
        /// 処理中状態を示すカウンタをインクリメントし、スコープから出たときにデクリメントする
        /// </summary>
        /// <returns>デクリメンタ</returns>
        public IDisposable Enter()
        {
            ++this.Count;

            return new ActionOnDispose(() => --this.Count);
        }
    }
}

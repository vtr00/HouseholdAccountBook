using System;
using System.ComponentModel;
using System.Threading;

namespace HouseholdAccountBook.Models.AppServices
{
    /// <summary>
    /// 処理中かどうかを管理し、状態変化を通知するサービス
    /// </summary>
    public class BusyService : INotifyPropertyChanged
    {
        /// <summary>
        /// 処理中状態を示すカウンタ
        /// </summary>
        private int mCount;

        /// <summary>
        /// 現在処理中であるかどうか
        /// </summary>
        public bool IsBusy => 0 < this.mCount;

        /// <summary>
        /// プロパティ変更時に発火するイベント
        /// WPFのデータバインディングに必要
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 指定されたプロパティ名に対して変更通知を発行する
        /// 通常はSetProperty経由で呼び出される
        /// </summary>
        /// <param name="propertyName">変更されたプロパティの名前。省略時は呼び出し元の名前を自動取得</param>
        protected void OnPropertyChanged(string propertyName = null) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// 処理中状態を示すカウンタをインクリメントし、スコープから出たときにデクリメントする
        /// </summary>
        /// <returns>デクリメンタ</returns>
        public IDisposable Enter()
        {
            _ = Interlocked.Increment(ref this.mCount);
            this.OnPropertyChanged(nameof(this.IsBusy));
            return new ActionOnDispose(() => {
                _ = Interlocked.Decrement(ref this.mCount);
                this.OnPropertyChanged(nameof(this.IsBusy));
            });
        }
    }
}

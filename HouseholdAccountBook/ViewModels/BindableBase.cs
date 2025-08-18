using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// <see cref="INotifyPropertyChanged"/> を実装したベースクラス
    /// </summary>
    public class BindableBase : INotifyPropertyChanged
    {
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
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// バッキングフィールドの値を変更し、必要に応じて変更通知を発行する
        /// PrismのSetPropertyと同様の動作を提供する
        /// </summary>
        /// <typeparam name="T">プロパティの型</typeparam>
        /// <param name="field">バッキングフィールドへの参照</param>
        /// <param name="value">新しい値</param>
        /// <param name="propertyName">変更されたプロパティの名前。省略時は呼び出し元の名前を自動取得</param>
        /// <returns>値が変更された場合はtrue、変更がなければfalse</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;

            field = value;
            this.RaisePropertyChanged(propertyName);

            return true;
        }
    }
}

using System.ComponentModel;

namespace HouseholdAccountBook.Extentions
{
    /// <summary>
    /// INotifyPropertyChanged の実装補助。
    /// </summary>
    public static partial class PropertyChangedEventHandleExtensions
    {
        /// <summary>
        /// PropertyChanged イベントを起こす。
        /// </summary>
        /// <param name="handler">イベントハンドラー。</param>
        /// <param name="sender">イベントの送信元。</param>
        /// <param name="propertyNames">変化したプロパティの名前。</param>
        public static void Raise(this PropertyChangedEventHandler handler, object sender, params string[] propertyNames)
        {
            if (handler == null) {
                return;
            }

            foreach (var name in propertyNames) {
                handler(sender, new PropertyChangedEventArgs(name));
            }
        }
    }
}

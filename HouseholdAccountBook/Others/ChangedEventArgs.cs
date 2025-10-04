#nullable enable

namespace HouseholdAccountBook.Others
{
    /// <summary>
    /// 値変更時のイベント引数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ChangedEventArgs<T>
    {
        /// <summary>
        /// 変更前の値
        /// </summary>
        public T? OldValue { get; set; }
        /// <summary>
        /// 変更後の値
        /// </summary>
        public T? NewValue { get; set; }
    }
}

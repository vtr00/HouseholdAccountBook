using System;

namespace HouseholdAccountBook.UserEventArgs
{
    /// <summary>
    /// イベントに使用する値を提供するクラスを表します。
    /// </summary>
    /// <typeparam name="TType">イベントデータの型</typeparam>
    public class EventArgs<TType> : EventArgs
    {
        /// <summary>
        /// 値
        /// </summary>
        public TType Value { get; set; }

        /// <summary>
        /// <see cref="EventArgs"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="value">イベントデータ</param>
        public EventArgs(TType value) : base()
        {
            this.Value = value;
        }
    }
}

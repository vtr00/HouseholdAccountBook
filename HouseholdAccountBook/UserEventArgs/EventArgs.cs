using System;

namespace HouseholdAccountBook.UserEventArgs
{
    /// <summary>
    /// イベントに使用する値を提供するクラスを表します。
    /// </summary>
    /// <typeparam name="TType">イベントデータの型</typeparam>
    /// <param name="value">イベントデータ</param>
    public class EventArgs<TType>(TType value) : EventArgs()
    {
        /// <summary>
        /// 値
        /// </summary>
        public TType Value { get; set; } = value;
    }
}

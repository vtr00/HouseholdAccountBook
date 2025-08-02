using System;

namespace HouseholdAccountBook.UserEventArgs
{
    /// <summary>
    /// イベントに使用する値を提供するクラスを表します。
    /// </summary>
    /// <typeparam name="TType1">イベントデータの型1</typeparam>
    /// <typeparam name="TType2">イベントデータの型2</typeparam>
    /// <param name="value1">イベントデータ1</param>
    /// <param name="value2">イベントデータ2</param>
    public class EventArgs<TType1, TType2>(TType1 value1, TType2 value2) : EventArgs()
    {
        /// <summary>
        /// 値1
        /// </summary>
        public TType1 Value1 { get; set; } = value1;
        /// <summary>
        /// 値2
        /// </summary>
        public TType2 Value2 { get; set; } = value2;
    }
}

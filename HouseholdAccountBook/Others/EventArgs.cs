using System;

namespace HouseholdAccountBook.Others
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

    /// <summary>
    /// イベントに使用する値を提供するクラスを表します。
    /// </summary>
    /// <typeparam name="TType1">イベントデータの型1</typeparam>
    /// <typeparam name="TType2">イベントデータの型2</typeparam>
    /// <typeparam name="TType3">イベントデータの型3</typeparam>
    /// <param name="value1">イベントデータ1</param>
    /// <param name="value2">イベントデータ2</param>
    /// <param name="value3">イベントデータ3</param>
    public class EventArgs<TType1, TType2, TType3>(TType1 value1, TType2 value2, TType3 value3) : EventArgs()
    {
        /// <summary>
        /// 値1
        /// </summary>
        public TType1 Value1 { get; set; } = value1;
        /// <summary>
        /// 値2
        /// </summary>
        public TType2 Value2 { get; set; } = value2;
        /// <summary>
        /// 値3
        /// </summary>
        public TType3 Value3 { get; set; } = value3;
    }
}

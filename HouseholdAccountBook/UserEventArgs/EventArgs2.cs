using System;

namespace HouseholdAccountBook.UserEventArgs
{
    /// <summary>
    /// イベントのデータを含み、イベント データを含むイベントに使用する値を提供するクラスを表します。
    /// </summary>
    /// <typeparam name="TType1">イベントデータの型1</typeparam>
    /// <typeparam name="TType2">イベントデータの型2</typeparam>
    public class EventArgs<TType1, TType2> : EventArgs
    {
        /// <summary>
        /// 値1
        /// </summary>
        public TType1 Value1 { get; set; }
        /// <summary>
        /// 値1
        /// </summary>
        public TType2 Value2 { get; set; }

        /// <summary>
        /// <see cref="EventArgs"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="value1">イベントデータ1</param>
        /// <param name="value2">イベントデータ2</param>
        public EventArgs(TType1 value1, TType2 value2) : base() {
            this.Value1 = value1;
            this.Value2 = value2;
        }
    }
}

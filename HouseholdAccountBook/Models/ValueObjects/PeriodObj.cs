namespace HouseholdAccountBook.Models.ValueObjects
{
    /// <summary>
    /// 期間VO
    /// </summary>
    /// <param name="start">開始</param>
    /// <param name="end">終了</param>
    public class PeriodObj<T>(T start, T end) where T : struct
    {
        /// <summary>
        /// 開始
        /// </summary>
        public T Start { get; init; } = start;
        /// <summary>
        /// 終了
        /// </summary>
        public T End { get; init; } = end;
    }
}

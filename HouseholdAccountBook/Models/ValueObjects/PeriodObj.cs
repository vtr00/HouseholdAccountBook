using System;

namespace HouseholdAccountBook.Models.ValueObjects
{
    /// <summary>
    /// 期間VO
    /// </summary>
    /// <param name="Start">開始</param>
    /// <param name="End">終了</param>
    public record class PeriodObj<T>(T Start, T End) where T : struct
    {
        public PeriodObj<TResult> Convert<TResult>(Func<T, TResult> converter) where TResult : struct => new(converter(this.Start), converter(this.End));
    }
}

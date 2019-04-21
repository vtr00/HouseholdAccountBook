using System.Threading.Tasks;

namespace HouseholdAccountBook.Extensions
{
    public static class ValueTupleExtensions
    {
        /// <summary>
        /// 複数のタスクを同時に非同期で実行する
        /// </summary>
        /// <typeparam name="T1">タスク実行結果の型1</typeparam>
        /// <typeparam name="T2">タスク実行結果の型2</typeparam>
        /// <param name="tasks">タスクのタプル</param>
        /// <returns>タスク実行結果</returns>
        public static async Task<(T1, T2)> WhenAll<T1, T2>(this (Task<T1>, Task<T2>) tasks)
        {
            await Task.WhenAll(tasks.Item1, tasks.Item2).ConfigureAwait(false);
            return (tasks.Item1.Result, tasks.Item2.Result);
        }

        /// <summary>
        /// 複数のタスクを同時に非同期で実行する
        /// </summary>
        /// <typeparam name="T1">タスク実行結果の型1</typeparam>
        /// <typeparam name="T2">タスク実行結果の型2</typeparam>
        /// <typeparam name="T3">タスク実行結果の型3</typeparam>
        /// <param name="tasks">タスクのタプル</param>
        /// <returns>タスク実行結果</returns>
        public static async Task<(T1, T2, T3)> WhenAll<T1, T2, T3>(this (Task<T1>, Task<T2>, Task<T3>) tasks)
        {
            await Task.WhenAll(tasks.Item1, tasks.Item2, tasks.Item3).ConfigureAwait(false);
            return (tasks.Item1.Result, tasks.Item2.Result, tasks.Item3.Result);
        }

        /// <summary>
        /// 複数のタスクを同時に非同期で実行する
        /// </summary>
        /// <typeparam name="T1">タスク実行結果の型1</typeparam>
        /// <typeparam name="T2">タスク実行結果の型2</typeparam>
        /// <typeparam name="T3">タスク実行結果の型3</typeparam>
        /// <typeparam name="T4">タスク実行結果の型4</typeparam>
        /// <param name="tasks">タスクのタプル</param>
        /// <returns>タスク実行結果</returns>
        public static async Task<(T1, T2, T3, T4)> WhenAll<T1, T2, T3, T4>(this (Task<T1>, Task<T2>, Task<T3>, Task<T4>) tasks)
        {
            await Task.WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4).ConfigureAwait(false);
            return (tasks.Item1.Result, tasks.Item2.Result, tasks.Item3.Result, tasks.Item4.Result);
        }
    }
}

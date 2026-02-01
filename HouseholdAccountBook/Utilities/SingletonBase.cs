using System;
using System.Runtime.CompilerServices;

#nullable enable

namespace HouseholdAccountBook.Utilities
{
    /// <summary>
    /// シングルトンの基本クラス
    /// </summary>
    /// <typeparam name="T">Singletonの対象</typeparam>
    public abstract class SingletonBase<T> where T : class
    {
        /// <summary>
        /// 遅延初期化
        /// </summary>
        private static Lazy<T>? mInstance;

        /// <summary>
        /// インスタンス取得
        /// </summary>
        /// <remarks>初めて呼び出したタイミングで子クラスのコンストラクタが呼び出される</remarks>
        public static T Instance => mInstance?.Value ?? throw new InvalidOperationException("Uninitialized singleton called");

        /// <summary>
        /// スタティックコンストラクタ
        /// </summary>
        /// <remarks>子クラスのstaticコンストラクタを強制呼び出し</remarks>
        static SingletonBase() => RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);

        /// <summary>
        /// ファクトリを登録する
        /// </summary>
        /// <param name="factory">生成ロジック</param>
        protected static void Register(Func<T> factory) => mInstance = new Lazy<T>(factory);
    }
}

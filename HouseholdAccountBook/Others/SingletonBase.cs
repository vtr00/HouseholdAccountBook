using System;
using System.Runtime.CompilerServices;

#nullable enable

namespace HouseholdAccountBook.Others
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
        private static Lazy<T>? _instance;

        /// <summary>
        /// インスタンス取得
        /// </summary>
        public static T Instance => _instance?.Value ?? throw new InvalidOperationException("Uninitialized singleton called");

        /// <summary>
        /// スタティックコンストラクタ
        /// </summary>
        static SingletonBase()
        {
            // 子クラスのstaticコンストラクタを強制呼び出し
            RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
        }

        /// <summary>
        /// ファクトリを登録
        /// </summary>
        /// <param name="factory">生成ロジック</param>
        protected static void Register(Func<T> factory) => _instance = new Lazy<T>(factory);
    }
}

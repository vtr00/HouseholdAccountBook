namespace HouseholdAccountBook.Adapters
{
    /// <summary>
    /// DB関連の定数定義
    /// </summary>
    /// <remarks>DB制御に用いる定数</remarks>
    public static class DbConstants
    {
        /// <summary>
        /// 更新者
        /// </summary>
        public static string Updater { get; } = string.Empty;

        /// <summary>
        /// 挿入者
        /// </summary>
        public static string Inserter { get; } = string.Empty;

        /// <summary>
        /// Accessプロバイダヘッダ
        /// </summary>
        public static string AccessProviderHeader { get; } = "Microsoft.ACE.OLEDB";
    }
}

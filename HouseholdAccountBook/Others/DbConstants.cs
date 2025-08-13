namespace HouseholdAccountBook.Others
{
    /// <summary>
    /// DB関連の定数定義
    /// </summary>
    /// <remarks>DB制御に用いる定数およびDBに保存する定数</remarks>
    public static class DbConstants
    {
        /// <summary>
        /// SQL DB種別
        /// </summary>
        public enum DBKind
        {
            /// <summary>
            /// 未定義
            /// </summary>
            Undefined = -1,
            /// <summary>
            /// PostgreSQL
            /// </summary>
            PostgreSQL = 0,
            /// <summary>
            /// Access
            /// </summary>
            Access,
            /// <summary>
            /// SQLite
            /// </summary>
            SQLite
        }

        /// <summary>
        /// SQL DBライブラリ種別
        /// </summary>
        public enum DBLibraryKind
        {
            /// <summary>
            /// 未定義
            /// </summary>
            Undefined = -1,
            /// <summary>
            /// PostgreSQL
            /// </summary>
            PostgreSQL = 0,
            /// <summary>
            /// Ole DB
            /// </summary>
            OleDb,
            /// <summary>
            /// SQLite
            /// </summary>
            SQLite
        }

        /// <summary>
        /// PostgreSQL パスワード入力方法
        /// </summary>
        public enum PostgresPasswordInput
        {
            /// <summary>
            /// InputWindowによる入力
            /// </summary>
            InputWindow = 0,
            /// <summary>
            /// pgpass.confによる入力
            /// </summary>
            PgPassConf = 1
        }

        /// <summary>
        /// PostgreSQL ダンプ/リストアフォーマット
        /// </summary>
        public enum PostgresFormat
        {
            Plain = 0,
            Custom = 1,
            Dictionary = 2,
            Tar = 3
        }

        /// <summary>
        /// 帳簿種別
        /// </summary>
        public enum BookKind
        {
            /// <summary>
            /// 未分類
            /// </summary>
            Uncategorized = 0,
            /// <summary>
            /// 財布
            /// </summary>
            Wallet = 1,
            /// <summary>
            /// 銀行口座
            /// </summary>
            BankAccount = 2,
            /// <summary>
            /// クレジットカード
            /// </summary>
            CreditCard = 3,
            /// <summary>
            /// 株式口座
            /// </summary>
            BrokerageAccount = 4
        }

        /// <summary>
        /// 収支種別
        /// </summary>
        public enum BalanceKind
        {
            /// <summary>
            /// 収入
            /// </summary>
            Income = 0,
            /// <summary>
            /// 支出
            /// </summary>
            Expenses = 1,
            /// <summary>
            /// その他(残高、差引損益)
            /// </summary>
            Others = -1
        }

        /// <summary>
        /// グループ種別
        /// </summary>
        public enum GroupKind
        {
            /// <summary>
            /// 移動
            /// </summary>
            Move = 0,
            /// <summary>
            /// 繰返し
            /// </summary>
            Repeat = 1,
            /// <summary>
            /// リスト登録
            /// </summary>
            ListReg = 2
        }

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

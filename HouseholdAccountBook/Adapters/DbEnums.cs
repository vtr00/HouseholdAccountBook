namespace HouseholdAccountBook.Adapters
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
        /// <remarks>Access, Excel, CSV, SQLServer, Oracle, PostgreSQL, MySQLなどに接続可能(ファイルに限定されない)</remarks>
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
}

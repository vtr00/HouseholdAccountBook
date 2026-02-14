using System.Runtime.CompilerServices;

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
    /// SQL DB種別マスク
    /// </summary>
    public enum DBKindMask
    {
        /// <summary>
        /// PostgreSQL
        /// </summary>
        PostgreSQL = 1 << DBKind.PostgreSQL,
        /// <summary>
        /// Access
        /// </summary>
        Access = 1 << DBKind.Access,
        /// <summary>
        /// SQLite
        /// </summary>
        SQLite = 1 << DBKind.SQLite,
        /// <summary>
        /// All DB
        /// </summary>
        All = PostgreSQL | Access | SQLite
    }

    /// <summary>
    /// SQL DB種別マスクの拡張メソッド
    /// </summary>
    public static class DBKindMaskExtension
    {
        /// <summary>
        /// マスクの対象かどうかを判定する
        /// </summary>
        /// <param name="dbKindMask">マスク</param>
        /// <param name="dbKind">判定対象のDB種別</param>
        /// <returns>マスクの対象</returns>
        public static bool Check(this DBKindMask dbKindMask, DBKind dbKind) => (dbKindMask & (DBKindMask)(1 << (int)dbKind)) != 0;
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

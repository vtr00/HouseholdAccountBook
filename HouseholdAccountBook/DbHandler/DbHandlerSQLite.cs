using HouseholdAccountBook.DbHandler.Abstract;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace HouseholdAccountBook.DbHandler
{
    /// <summary>
    /// SQLite DB Hander
    /// </summary>
    public partial class DbHanderSQLite : DbHandlerBase
    {
        /// <summary>
        /// 接続文字列
        /// </summary>
        private const string stringFormat = @"Data Source={0}";

        /// <summary>
        /// <see cref="DbHanderSQLite"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="info">接続情報</param>
        public DbHanderSQLite(ConnectInfo info) : this(info.FilePath) { }

        /// <summary>
        /// <see cref="DbHanderSQLite"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        public DbHanderSQLite(string filePath) : base(new SQLiteConnection(string.Format(stringFormat, filePath))) { }

        /// <summary>
        /// [非同期]クエリを実行する
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="objects">SQLパラメータ</param>
        /// <returns>DBデータリーダ</returns>
        protected async override Task<DbDataReader> ExecuteReaderAsync(string sql, params object[] objects)
        {
            SQLiteCommand command = this.CreateCommand(sql, objects) as SQLiteCommand;
            return await command.ExecuteReaderAsync();
        }

        /// <summary>
        /// コマンドを生成する
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="objects">引数リスト</param>
        /// <returns>SQLコマンド</returns>
        protected override DbCommand CreateCommand(string sql, params object[] objects)
        {
            SQLiteCommand command = ((SQLiteConnection)this.connection).CreateCommand();

            sql = sql.Replace("{", "_").Replace("}", "_");
            command.Transaction = this.dbTransaction as SQLiteTransaction;
            command.CommandText = sql;

            int cnt = 0;
            foreach (object obj in objects) {
                command.Parameters.AddWithValue("@_" + cnt + "_", obj);
                ++cnt;
            }
            return command;
        }
    }
}

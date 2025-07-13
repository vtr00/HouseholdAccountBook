using HouseholdAccountBook.DbHandler.Abstract;
using Npgsql;
using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace HouseholdAccountBook.DbHandler
{
    public partial class DbHandlerNpgsql : DbHandlerBase
    {
        /// <summary>
        /// 接続文字列
        /// </summary>
        private const string stringFormat = @"Server={0};Port={1};User Id={2};Password={3};Database={4}";

        /// <summary>
        /// <see cref="DbHandlerNpgsql"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="info">接続情報</param>
        public DbHandlerNpgsql(ConnectInfo info) : this(info.Host, info.Port, info.UserName, info.Password, info.DatabaseName) { }

        /// <summary>
        /// <see cref="DbHandlerNpgsql"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="uri">URI</param>
        /// <param name="port">ポート番号</param>
        /// <param name="userName">ユーザー名</param>
        /// <param name="password">パスワード</param>
        /// <param name="databaseName">データベース名</param>
        public DbHandlerNpgsql(string uri, int port, string userName, string password, string databaseName)
            : base(new NpgsqlConnection(string.Format(stringFormat, uri, port, userName, password, databaseName))) { }

        /// <summary>
        /// [非同期]クエリを実行する
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="objects">SQLパラメータ</param>
        /// <returns>DBデータリーダ</returns>
        protected async override Task<DbDataReader> ExecuteReaderAsync(string sql, params object[] objects)
        {
            NpgsqlCommand command = this.CreateCommand(sql, objects) as NpgsqlCommand;
            return await command.ExecuteReaderAsync();
        }

        /// <summary>
        /// コマンドを生成する
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="objects">SQLパラメータ</param>
        /// <returns>SQLコマンド</returns>
        protected override DbCommand CreateCommand(string sql, params object[] objects)
        {
            NpgsqlCommand command = ((NpgsqlConnection)this.connection).CreateCommand();

            sql = sql.Replace("{", "_").Replace("}", "_");
            command.Transaction = this.dbTransaction as NpgsqlTransaction;
            command.CommandText = sql;

            int cnt = 0;
            foreach (object obj in objects) {
                object tmpObj = obj ?? DBNull.Value;
                command.Parameters.AddWithValue("@_" + cnt + "_", tmpObj);
                ++cnt;
            }
            return command;
        }
    }
}

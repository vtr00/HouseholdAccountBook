﻿using HouseholdAccountBook.DbHandler.Abstract;
using System.Data.Common;
using System.Data.OleDb;
using System.Threading.Tasks;

namespace HouseholdAccountBook.DbHandler
{
    /// <summary>
    /// Ole Db Handler
    /// </summary>
    public partial class DbHandlerOle : DbHandlerBase
    {
        /// <summary>
        /// 接続文字列
        /// </summary>
        private const string stringFormat = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}";

        /// <summary>
        /// <see cref="DbHandlerOle"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="info">接続情報</param>
        public DbHandlerOle(ConnectInfo info) : this(info.FilePath) { }

        /// <summary>
        /// <see cref="DbHandlerOle"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        public DbHandlerOle(string filePath) : base(new OleDbConnection(string.Format(stringFormat, filePath))) { }

        /// <summary>
        /// [非同期]クエリを実行する
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="objects">SQLパラメータ</param>
        /// <returns>DBデータリーダ</returns>
        protected async override Task<DbDataReader> ExecuteReaderAsync(string sql, params object[] objects)
        {
            OleDbCommand command = this.CreateCommand(sql, objects) as OleDbCommand;
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
            OleDbCommand command = ((OleDbConnection)this.connection).CreateCommand();

            sql = sql.Replace("{", "_").Replace("}", "_");
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

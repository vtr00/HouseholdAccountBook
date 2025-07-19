using HouseholdAccountBook.DbHandler.Abstract;
using Npgsql;
using System;
using System.Data.Common;
using System.Threading.Tasks;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.DbHandler
{
    public partial class NpgsqlDbHandler : DbHandlerBase
    {
        /// <summary>
        /// 接続文字列
        /// </summary>
        private const string stringFormat = @"Server={0};Port={1};User Id={2};Password={3};Database={4}";

        /// <summary>
        /// <see cref="NpgsqlDbHandler"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="info">接続情報</param>
        public NpgsqlDbHandler(ConnectInfo info) : this(info.Host, info.Port, info.UserName, info.Password, info.DatabaseName) {
            this.Type = DatabaseType.PostgreSQL;
        }

        /// <summary>
        /// <see cref="NpgsqlDbHandler"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="uri">URI</param>
        /// <param name="port">ポート番号</param>
        /// <param name="userName">ユーザー名</param>
        /// <param name="password">パスワード</param>
        /// <param name="databaseName">データベース名</param>
        public NpgsqlDbHandler(string uri, int port, string userName, string password, string databaseName)
            : base(new NpgsqlConnection(string.Format(stringFormat, uri, port, userName, password, databaseName))) { }
    }
}

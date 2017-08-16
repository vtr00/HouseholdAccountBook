using System;
using System.Collections.Generic;
using Npgsql;

namespace HouseholdAccountBook.Dao
{
    public partial class DaoNpgsql : DaoBase
    {
        /// <summary>
        /// 接続文字列
        /// </summary>
        private const string daoStringFormat = @"Server={0};Port={1};User Id={2};Password={3};Database={4}";

        /// <summary>
        /// <see cref="DaoNpgsql"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="info">接続情報</param>
        public DaoNpgsql(ConnectInfo info) : this(info.Host, info.Port, info.UserName, info.Password, info.DatabaseName) { }

        /// <summary>
        /// <see cref="DaoNpgsql"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="uri">URI</param>
        /// <param name="port">ポート番号</param>
        /// <param name="userName">ユーザー名</param>
        /// <param name="password">パスワード</param>
        /// <param name="databaseName">データベース名</param>
        public DaoNpgsql(string uri, int port, string userName, string password, string databaseName)
            : base(new NpgsqlConnection(string.Format(daoStringFormat, uri, port, userName, password, databaseName))) { }
        
        /// <summary>
        /// 非クエリの実行
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="objects">SQLパラメータ</param>
        /// <returns>処理結果</returns>
        public override int ExecNonQuery(string sql, params object[] objects)
        {
            try {
                return CreateCommand(sql, objects).ExecuteNonQuery();
            }
            catch (Exception e) {
                throw e;
            }
        }

        /// <summary>
        /// クエリの実行
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="objects">SQLパラメータ</param>
        /// <returns>DAOリーダ</returns>
        public override DaoReader ExecQuery(string sql, params object[] objects)
        {
            try {
                LinkedList<Dictionary<string, object>> resultSet = new LinkedList<Dictionary<string, object>>();

                NpgsqlCommand command = CreateCommand(sql, objects);
                using (NpgsqlDataReader reader = command.ExecuteReader()) {
                    // フィールド名の取得
                    List<string> fieldList = new List<string>();
                    for (int i = 0; i < reader.FieldCount; ++i) {
                        fieldList.Add(reader.GetName(i));
                    }

                    // レコードの取得
                    while (reader.Read()) {
                        Dictionary<string, object> result = new Dictionary<string, object>();
                        for (int i = 0; i < fieldList.Count; ++i) {
                            result.Add(fieldList[i], reader[fieldList[i]]);
                        }
                        resultSet.AddLast(result);
                    }
                }
                return new DaoReader(sql, resultSet);
            }
            catch (NpgsqlException e) {
                throw e;
            }
            catch (Exception e) {
                throw e;
            }
        }

        /// <summary>
        /// コマンドの生成
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="objects">SQLパラメータ</param>
        /// <returns>SQLコマンド</returns>
        private NpgsqlCommand CreateCommand(string sql, params object[] objects)
        {
            NpgsqlCommand command = ((NpgsqlConnection)connection).CreateCommand();

            sql = sql.Replace("{", "_").Replace("}", "_");
            command.CommandText = sql;

            int cnt = 0;
            foreach (object obj in objects) {
                object tmpObj = obj;
                if (tmpObj == null) tmpObj = DBNull.Value;

                command.Parameters.AddWithValue("@_" + cnt + "_", tmpObj);
                ++cnt;
            }
            return command;
        }
    }
}

using System;
using System.Collections.Generic;
using Npgsql;

namespace HouseholdAccountBook.Dao
{
    public class DaoNpgsql : DaoBase
    {
        private const String daoStringFormat = @"Server={0};Port={1};User Id={2};Password={3};Database={4}";

        /// <summary>
        /// 接続情報
        /// </summary>
        public new class ConnectInfo : DaoBase.ConnectInfo
        {
            /// <summary>
            /// URI
            /// </summary>
            public String Host { get; set; }
            /// <summary>
            /// ポート番号
            /// </summary>
            public int Port { get; set; }
            /// <summary>
            /// ユーザー名
            /// </summary>
            public String UserName { get; set; }
            /// <summary>
            /// パスワード
            /// </summary>
            public String Password { get; set; }
            /// <summary>
            /// データベース名
            /// </summary>
            public String DatabaseName { get; set; }
            /// <summary>
            /// ロール名
            /// </summary>
            public String Role { get; set; }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="info">接続情報</param>
        public DaoNpgsql(ConnectInfo info) : this(info.Host, info.Port, info.UserName, info.Password, info.DatabaseName) { }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="uri">URI</param>
        /// <param name="port">ポート番号</param>
        /// <param name="userName">ユーザー名</param>
        /// <param name="password">パスワード</param>
        /// <param name="databaseName">データベース名</param>
        public DaoNpgsql(String uri, int port, String userName, String password, String databaseName)
            : base(new NpgsqlConnection(String.Format(daoStringFormat, uri, port, userName, password, databaseName))) { }
        
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
                LinkedList<Dictionary<String, Object>> resultSet = new LinkedList<Dictionary<String, Object>>();

                NpgsqlCommand command = CreateCommand(sql, objects);
                using (NpgsqlDataReader reader = command.ExecuteReader()) {
                    // フィールド名の取得
                    List<String> fieldList = new List<String>();
                    for (int i = 0; i < reader.FieldCount; ++i) {
                        fieldList.Add(reader.GetName(i));
                    }

                    // レコードの取得
                    while (reader.Read()) {
                        Dictionary<String, Object> result = new Dictionary<String, Object>();
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
        private NpgsqlCommand CreateCommand(String sql, params Object[] objects)
        {
            NpgsqlCommand command = ((NpgsqlConnection)connection).CreateCommand();

            sql = sql.Replace("{", "_").Replace("}", "_");
            command.CommandText = sql;

            int cnt = 0;
            foreach (Object obj in objects) {
                Object tmpObj = obj;
                if (tmpObj == null) tmpObj = DBNull.Value;

                command.Parameters.AddWithValue("@_" + cnt + "_", tmpObj);
                ++cnt;
            }
            return command;
        }
    }
}

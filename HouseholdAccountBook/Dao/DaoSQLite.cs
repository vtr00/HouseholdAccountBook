using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace HouseholdAccountBook.Dao {
    /// <summary>
    /// SQLite Data Access Object
    /// </summary>
    public class DaoSQLite : DaoBase {
        private const string DaoStringFormat = @"Data Source={0}";

        /// <summary>
        /// 接続情報
        /// </summary>
        public new class ConnectInfo : DaoBase.ConnectInfo
        {
            public string filePath { get; set; }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="info">接続情報</param>
        public DaoSQLite(ConnectInfo info) : this(info.filePath) { }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        public DaoSQLite(String filePath) : base (new SQLiteConnection(string.Format(DaoStringFormat, filePath))) { }

        public override int ExecNonQuery(string sql, params Object[] objects)
        {
            return CreateCommand(sql, objects).ExecuteNonQuery();
        }

        public override DaoReader ExecQuery(String sql, params Object[] objects) {
            LinkedList<Dictionary<String, Object>> resultSet = new LinkedList<Dictionary<String, Object>>();

            SQLiteCommand command = CreateCommand(sql, objects);
            using (SQLiteDataReader reader = command.ExecuteReader()) {
                // フィールド名の取得
                List<String> fieldList = new List<String>();
                for (int i = 0; i < reader.FieldCount; ++i) {
                    fieldList.Add(reader.GetName(i));
                }

                // レコードの取得
                while (reader.NextResult()) {
                    Dictionary<String, Object> result = new Dictionary<String, Object>();
                    for (int i = 0; i < fieldList.Count; ++i) {
                        result.Add(fieldList[i], reader[fieldList[i]]);
                    }
                    resultSet.AddLast(result);
                }
            }
            return new DaoReader(sql, resultSet);
        }

        /// <summary>
        /// コマンドを生成する
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="objects">引数リスト</param>
        /// <returns>SQLコマンド</returns>
        private SQLiteCommand CreateCommand(String sql, params Object[] objects) {
            SQLiteCommand command = ((SQLiteConnection)connection).CreateCommand();

            sql = sql.Replace("{", "_").Replace("}", "_");
            command.CommandText = sql;

            int cnt = 0;
            foreach (Object obj in objects) {
                command.Parameters.AddWithValue("@_" + cnt + "_", obj);
                ++cnt;
            }
            return command;
        }
    }
}

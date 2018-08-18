using System.Collections.Generic;
using System.Data.SQLite;

namespace HouseholdAccountBook.Dao
{
    /// <summary>
    /// SQLite Data Access Object
    /// </summary>
    public partial class DaoSQLite : DaoBase
    {
        /// <summary>
        /// 接続文字列
        /// </summary>
        private const string DaoStringFormat = @"Data Source={0}";

        /// <summary>
        /// <see cref="DaoSQLite"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="info">接続情報</param>
        public DaoSQLite(ConnectInfo info) : this(info.FilePath) { }

        /// <summary>
        /// <see cref="DaoSQLite"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        public DaoSQLite(string filePath) : base(new SQLiteConnection(string.Format(DaoStringFormat, filePath))) { }

        public override int ExecNonQuery(string sql, params object[] objects)
        {
            return this.CreateCommand(sql, objects).ExecuteNonQuery();
        }

        public override DaoReader ExecQuery(string sql, params object[] objects)
        {
            LinkedList<Dictionary<string, object>> resultSet = new LinkedList<Dictionary<string, object>>();

            SQLiteCommand command = this.CreateCommand(sql, objects);
            using (SQLiteDataReader reader = command.ExecuteReader()) {
                // フィールド名の取得
                List<string> fieldList = new List<string>();
                for (int i = 0; i < reader.FieldCount; ++i) {
                    fieldList.Add(reader.GetName(i));
                }

                // レコードの取得
                while (reader.NextResult()) {
                    Dictionary<string, object> result = new Dictionary<string, object>();
                    foreach (string fieldName in fieldList) {
                        result.Add(fieldName, reader[fieldName]);
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
        private SQLiteCommand CreateCommand(string sql, params object[] objects)
        {
            SQLiteCommand command = ((SQLiteConnection)this.connection).CreateCommand();

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

using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading.Tasks;

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

        public async override Task<int> ExecNonQueryAsync(string sql, params object[] objects)
        {
            return await this.CreateCommand(sql, objects).ExecuteNonQueryAsync();
        }

        public async override Task<DaoReader> ExecQueryAsync(string sql, params object[] objects)
        {
            LinkedList<Dictionary<string, object>> resultSet = new LinkedList<Dictionary<string, object>>();

            SQLiteCommand command = this.CreateCommand(sql, objects);
            using (DbDataReader reader = await command.ExecuteReaderAsync()) {
                // フィールド名の取得
                List<string> fieldNameList = new List<string>();
                Parallel.For(0, reader.FieldCount - 1, (i) => {
                    string tmp = reader.GetName(i);
                    lock (this) {
                        fieldNameList.Add(tmp);
                    }
                });

                // レコードの取得
                while (reader.NextResult()) {
                    Dictionary<string, object> result = new Dictionary<string, object>();
                    Parallel.ForEach(fieldNameList, (fieldName) => {
                        object tmp = reader[fieldName];
                        lock (this) {
                            result.Add(fieldName, tmp);
                        }
                    });
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

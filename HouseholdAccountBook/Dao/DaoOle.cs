using System;
using System.Collections.Generic;
using System.Data.OleDb;

namespace HouseholdAccountBook.Dao
{
    /// <summary>
    /// Ole Db Data Access Object
    /// </summary>
    public class DaoOle : DaoBase
    {
        private const string DaoStringFormat = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}";

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
        public DaoOle(ConnectInfo info) : this(info.filePath) { }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        public DaoOle(string filePath) : base(new OleDbConnection(string.Format(DaoStringFormat, filePath))) { }

        public override int ExecNonQuery(string sql, params object[] objects)
        {
            return CreateCommand(sql, objects).ExecuteNonQuery();
        }

        public override DaoReader ExecQuery(string sql, params object[] objects)
        {
            LinkedList<Dictionary<string, object>> resultSet = new LinkedList<Dictionary<string, object>>();

            OleDbCommand command = CreateCommand(sql, objects);
            using (OleDbDataReader reader = command.ExecuteReader()) {
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

        /// <summary>
        /// コマンドの生成
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="objects">引数リスト</param>
        /// <returns>SQLコマンド</returns>
        private OleDbCommand CreateCommand(string sql, params object[] objects)
        {
            OleDbCommand command = ((OleDbConnection)connection).CreateCommand();

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

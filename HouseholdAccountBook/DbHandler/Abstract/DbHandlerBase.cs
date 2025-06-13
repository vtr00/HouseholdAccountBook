using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace HouseholdAccountBook.DbHandler.Abstract
{
    /// <summary>
    /// Db Handler Base クラス
    /// </summary>
    public abstract class DbHandlerBase : IDisposable
    {
        /// <summary>
        /// 接続情報
        /// </summary>
        public abstract class ConnectInfo { }

        /// <summary>
        /// DB接続
        /// </summary>
        protected DbConnection connection;

        /// <summary>
        /// <see cref="DbHandlerBase"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="connection">DB接続</param>
        public DbHandlerBase(DbConnection connection)
        {
            this.connection = connection;
            this.Open();
        }

        /// <summary>
        /// 接続を開始する
        /// </summary>
        /// <returns>接続結果</returns>
        private bool Open()
        {
            try {
                this.connection.Open();
                while (this.connection.State == System.Data.ConnectionState.Connecting) {
                    Thread.Sleep(100);
                }

                return this.connection.State == System.Data.ConnectionState.Open;
            }
            catch (Npgsql.PostgresException) {
                return false;
            }
            catch (System.Net.Sockets.SocketException) {
                return false;
            }
        }

        /// <summary>
        /// 接続状態を取得する
        /// </summary>
        /// <returns>接続状態</returns>
        public bool IsOpen => this.connection != null && this.connection.State == System.Data.ConnectionState.Open;

        /// <summary>
        /// 接続を終了する
        /// </summary>
        public void Dispose()
        {
            if (this.connection != null) {
                this.connection.Close();
                this.connection.Dispose();
            }
            this.connection = null;
        }

        /// <summary>
        /// [非同期]クエリを実行する
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="objects">SQLパラメータ</param>
        /// <returns>DBデータリーダ</returns>
        protected abstract Task<DbDataReader> ExecuteReaderAsync(string sql, params object[] objects);

        /// <summary>
        /// [非同期]非クエリを実行する
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="objects">引数リスト</param>
        /// <returns>更新レコード数</returns>
        public async Task<int> ExecNonQueryAsync(string sql, params object[] objects)
        {
            try {
                return await this.CreateCommand(sql, objects).ExecuteNonQueryAsync();
            }
            catch (Exception e) {
                throw e;
            }
        }

        /// <summary>
        /// [非同期]クエリを実行する
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="objects">SQLパラメータ</param>
        /// <returns>DAOリーダ</returns>
        public async Task<DbReader> ExecQueryAsync(string sql, params object[] objects)
        {
            try {
                LinkedList<Dictionary<string, object>> resultSet = new LinkedList<Dictionary<string, object>>();

                using (DbDataReader reader = await this.ExecuteReaderAsync(sql, objects)) {
                    // フィールド名の取得
                    List<string> fieldList = new List<string>();
                    for (int i = 0; i < reader.FieldCount; ++i) {
                        fieldList.Add(reader.GetName(i));
                    }

                    // レコードの取得
                    while (reader.Read()) {
                        Dictionary<string, object> result = new Dictionary<string, object>();
                        foreach (string fieldName in fieldList) {
                            result.Add(fieldName, reader[fieldName]);
                        }
                        resultSet.AddLast(result);
                    }
                }
                return new DbReader(sql, resultSet);
            }
            catch (Exception e) {
                throw e;
            }
        }

        /// <summary>
        /// DBコマンドを生成する
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="objects">引数リスト</param>
        /// <returns>DBコマンド</returns>
        protected abstract DbCommand CreateCommand(string sql, params object[] objects);

        /// <summary>
        /// [同期]トランザクション内の処理
        /// </summary>
        public delegate void Transaction();

        /// <summary>
        /// [非同期]トランザクション内の処理
        /// </summary>
        public delegate Task TransactionAsync();

        /// <summary>
        /// [同期]トランザクション処理を行う
        /// </summary>
        /// <param name="transaction">トランザクション内の処理</param>
        public void ExecTransaction(Transaction transaction)
        {
            DbTransaction dbTransaction = null;
            try {
                dbTransaction = this.connection.BeginTransaction();

                transaction();

                dbTransaction.Commit();
            }
            catch (DbException e) {
                Console.WriteLine(e.Message);
                dbTransaction?.Rollback();
                throw;
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                dbTransaction?.Rollback();
                throw;
            }
        }

        /// <summary>
        /// [非同期]トランザクション処理を行う
        /// </summary>
        /// <param name="transaction">トランザクション内の処理</param>
        public async Task ExecTransactionAsync(TransactionAsync transaction)
        {
            DbTransaction dbTransaction = null;
            try {
                dbTransaction = this.connection.BeginTransaction();

                await transaction();

                await Task.Run(() => dbTransaction.Commit());
            }
            catch (DbException e) {
                Console.WriteLine(e.Message);
                await Task.Run(() => dbTransaction?.Rollback());
                throw;
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                await Task.Run(() => dbTransaction?.Rollback());
                throw;
            }
        }
    }
}

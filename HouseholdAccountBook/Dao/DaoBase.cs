using System;
using System.Data.Common;

namespace HouseholdAccountBook.Dao
{
    /// <summary>
    /// Data Access Object
    /// </summary>
    public abstract class DaoBase : IDisposable
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
        /// コンストラクタ
        /// </summary>
        /// <param name="connection">DB接続</param>
        public DaoBase(DbConnection connection)
        {
            this.connection = connection;
            Open();
        }

        /// <summary>
        /// 接続を開始する
        /// </summary>
        /// <returns>接続結果</returns>
        private bool Open()
        {
            connection.Open();
            while (connection.State == System.Data.ConnectionState.Connecting) { ; }

            return connection.State == System.Data.ConnectionState.Open;
        }

        /// <summary>
        /// 接続状態を取得する
        /// </summary>
        /// <returns>接続状態</returns>
        public bool IsOpen()
        {
            return connection != null && connection.State == System.Data.ConnectionState.Open;
        }

        /// <summary>
        /// 接続を終了する
        /// </summary>
        public void Dispose()
        {
            if (connection != null) {
                connection.Close();
                connection.Dispose();
            }
            connection = null;
        }

        /// <summary>
        /// 非クエリを実行する
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="objects">引数リスト</param>
        /// <returns>更新レコード数</returns>
        public abstract int ExecNonQuery(string sql, params object[] objects);

        /// <summary>
        /// クエリを実行する
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="objects">引数リスト</param>
        /// <returns>リーダー</returns>
        public abstract DaoReader ExecQuery(string sql, params object[] objects);

        /// <summary>
        /// トランザクション内の処理
        /// </summary>
        public delegate void Transaction();
        /// <summary>
        /// トランザクション処理
        /// </summary>
        /// <param name="transaction">トランザクション内の処理</param>
        public void ExecTransaction(Transaction transaction)
        {
            DbTransaction dbTransaction = null;
            try {
                dbTransaction = connection.BeginTransaction();

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
    }
}

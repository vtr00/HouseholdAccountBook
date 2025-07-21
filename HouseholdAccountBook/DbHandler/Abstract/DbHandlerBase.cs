using Dapper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.DbHandler.Abstract
{
    /// <summary>
    /// Db Handler Base クラス
    /// </summary>
    public abstract class DbHandlerBase : IDisposable
    {
        /// <summary>
        /// 対象データベース
        /// </summary>
        public DBLibraryKind LibKind { get; protected set; } = DBLibraryKind.Undefined;

        /// <summary>
        /// 接続情報
        /// </summary>
        public abstract class ConnectInfo { }

        /// <summary>
        /// DB接続
        /// </summary>
        protected DbConnection connection = null;
        /// <summary>
        /// DBトランザクション
        /// </summary>
        protected DbTransaction dbTransaction = null;

        /// <summary>
        /// <see cref="DbHandlerBase"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="connection">DB接続</param>
        public DbHandlerBase(DbConnection connection)
        {
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

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
        /// <typeparam name="T">DTO</typeparam>
        /// <param name="sql">SQL</param>
        /// <param name="param">SQLパラメータ</param>
        /// <returns>クエリ結果リスト</returns>
        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null)
        {
            try {
                return await this.connection.QueryAsync<T>(sql, param, this.dbTransaction);
            }
            catch (Exception e) {
                throw e;
            }
        }

        /// <summary>
        /// [非同期]クエリを実行し、最初の1行だけ返す。結果がない場合はデフォルト値を返す
        /// </summary>
        /// <typeparam name="T">DTO</typeparam>
        /// <param name="sql">SQL</param>
        /// <param name="param">SQLパラメータ</param>
        /// <returns>クエリ結果 または デフォルト値</returns>
        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null)
        {
            try {
                return await this.connection.QueryFirstOrDefaultAsync<T>(sql, param, this.dbTransaction);
            }
            catch (Exception e) {
                throw e;
            }
        }

        /// <summary>
        /// [非同期]クエリを実行し、1行だけ返す
        /// </summary>
        /// <typeparam name="T">DTO</typeparam>
        /// <param name="sql">SQL</param>
        /// <param name="param">SQLパラメータ</param>
        /// <returns>クエリ結果</returns>
        /// <exception cref="InvalidOperationException">結果が1行以外の場合にスローされる</exception>
        public async Task<T> QuerySingleAsync<T>(string sql, object param = null)
        {
            try {
                return await this.connection.QuerySingleAsync<T>(sql, param, this.dbTransaction);
            }
            catch (Exception e) {
                throw e;
            }
        }

        /// <summary>
        /// [非同期]クエリを実行し、1行だけ返す。結果がない場合はデフォルト値を返す
        /// </summary>
        /// <typeparam name="T">DTO</typeparam>
        /// <param name="sql">SQL</param>
        /// <param name="param">SQLパラメータ</param>
        /// <returns>クエリ結果 または デフォルト値</returns>
        /// <exception cref="InvalidOperationException">結果が2行以上の場合にスローされる</exception>
        public async Task<T> QuerySingleOrDefaultAsync<T>(string sql, object param = null)
        {
            try {
                return await this.connection.QuerySingleOrDefaultAsync<T>(sql, param, this.dbTransaction);
            }
            catch (Exception e) {
                throw e;
            }
        }

        /// <summary>
        /// [非同期]非クエリを実行する
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="param">SQLパラメータ</param>
        /// <returns>変更行数</returns>
        public async Task<int> ExecuteAsync(string sql, object param = null)
        {
            try {
                return await this.connection.ExecuteAsync(sql, param, this.dbTransaction);
            }
            catch (Exception e) {
                throw e;
            }
        }

        /// <summary>
        /// [同期]トランザクション内の処理
        /// </summary>
        public delegate void TxAction();

        /// <summary>
        /// [非同期]トランザクション内の処理
        /// </summary>
        public delegate Task AxActionAsync();

        /// <summary>
        /// [同期]トランザクション処理を行う
        /// </summary>
        /// <param name="action">トランザクション内の処理</param>
        public void ExecTransaction(TxAction action)
        {
            this.dbTransaction = this.connection.BeginTransaction();
            try {
                action();

                dbTransaction.Commit();
            }
            catch (DbException e) {
                Console.WriteLine(e.Message);
                this.dbTransaction.Rollback();
                throw;
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                this.dbTransaction.Rollback();
                throw;
            }
            finally {
                this.dbTransaction.Dispose();
                this.dbTransaction = null;
            }
        }

        /// <summary>
        /// [非同期]トランザクション処理を行う
        /// </summary>
        /// <param name="actionAsync">トランザクション内の処理</param>
        public async Task ExecTransactionAsync(AxActionAsync actionAsync)
        {
            this.dbTransaction = this.connection.BeginTransaction();
            try {
                await actionAsync();

                await Task.Run(() => this.dbTransaction.Commit());
            }
            catch (DbException e) {
                Console.WriteLine(e.Message);
                await Task.Run(() => this.dbTransaction.Rollback());
                throw;
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                await Task.Run(() => this.dbTransaction.Rollback());
                throw;
            }
            finally {
                await Task.Run(() => this.dbTransaction.Dispose());
                this.dbTransaction = null;
            }
        }
    }
}

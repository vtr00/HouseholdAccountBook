using Dapper;
using HouseholdAccountBook.Enums;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.DbHandlers.Abstract
{
    /// <summary>
    /// Db Handler Base クラス
    /// </summary>
    public abstract class DbHandlerBase : IAsyncDisposable
    {
        /// <summary>
        /// 対象データベースライブラリ
        /// </summary>
        public DBLibraryKind DBLibKind { get; protected set; } = DBLibraryKind.Undefined;
        /// <summary>
        /// 対象データベース
        /// </summary>
        public DBKind DBKind { get; protected set; } = DBKind.Undefined;
        /// <summary>
        /// 接続可能か
        /// </summary>
        public bool CanOpen { get; protected set; } = true;
        /// <summary>
        /// 接続状態を取得する
        /// </summary>
        /// <returns>接続状態</returns>
        public bool IsOpen => this.mConnection != null && this.mConnection.State == System.Data.ConnectionState.Open;

        /// <summary>
        /// 接続情報
        /// </summary>
        public abstract class ConnectInfo { }

        /// <summary>
        /// DB接続
        /// </summary>
        protected DbConnection mConnection;

        /// <summary>
        /// DBトランザクション
        /// </summary>
        protected DbTransaction mDbTransaction;

        /// <summary>
        /// <see cref="DbHandlerBase"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="connection">DB接続</param>
        public DbHandlerBase(DbConnection connection)
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            this.mConnection = connection;
        }

        /// <summary>
        /// [非同期]接続を開始する
        /// </summary>
        /// <param name="timeoutMs">タイムアウト時間(ms)。0以下の場合は無制限</param>
        /// <returns>接続結果</returns>
        /// <exception cref="TimeoutException">接続タイムアウトが発生した場合</exception>
        public async Task<bool> OpenAsync(int timeoutMs = 0)
        {
            // 接続不可の場合、接続しない
            if (this.CanOpen == false) {
                return false;
            }

            try {
                await this.mConnection.OpenAsync();
                Stopwatch sw = Stopwatch.StartNew();
                while (this.mConnection.State == System.Data.ConnectionState.Connecting) {
                    Thread.Sleep(10);
                    if (0 < timeoutMs && timeoutMs < sw.ElapsedMilliseconds) {
                        this.mConnection.Close();
                        throw new TimeoutException();
                    }
                }

                return this.mConnection.State == System.Data.ConnectionState.Open;
            }
            catch (Npgsql.PostgresException) {
                return false;
            }
            catch (System.Net.Sockets.SocketException) {
                return false;
            }
        }

        /// <summary>
        /// [非同期]接続を終了する
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);

            if (this.mConnection != null) {
                await this.mConnection.CloseAsync();
                await this.mConnection.DisposeAsync();
            }
            this.mConnection = null;
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
                return await this.mConnection.QueryAsync<T>(sql, param, this.mDbTransaction);
            }
            catch (Exception) {
                throw;
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
                return await this.mConnection.QueryFirstOrDefaultAsync<T>(sql, param, this.mDbTransaction);
            }
            catch (Exception) {
                throw;
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
                return await this.mConnection.QuerySingleAsync<T>(sql, param, this.mDbTransaction);
            }
            catch (Exception) {
                throw;
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
                return await this.mConnection.QuerySingleOrDefaultAsync<T>(sql, param, this.mDbTransaction);
            }
            catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// [非同期]非クエリを実行する
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="param">SQLパラメータ</param>
        /// <returns>変更件数</returns>
        public async Task<int> ExecuteAsync(string sql, object param = null)
        {
            try {
                return await this.mConnection.ExecuteAsync(sql, param, this.mDbTransaction);
            }
            catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// [非同期]トランザクション内の処理
        /// </summary>
        public delegate Task AxActionAsync();

        /// <summary>
        /// [非同期]トランザクション処理を行う
        /// </summary>
        /// <param name="actionAsync">トランザクション内の処理</param>
        public async Task ExecTransactionAsync(AxActionAsync actionAsync)
        {
            try {
                this.mDbTransaction = await this.mConnection.BeginTransactionAsync();

                await actionAsync?.Invoke();

                await this.mDbTransaction.CommitAsync();
            }
            catch (DbException e) {
                Console.WriteLine(e.Message);
                await this.mDbTransaction.RollbackAsync();
                throw;
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                await this.mDbTransaction.RollbackAsync();
                throw;
            }
            finally {
                await this.mDbTransaction.DisposeAsync();
                this.mDbTransaction = null;
            }
        }

        /// <summary>
        /// トランザクション中か
        /// </summary>
        /// <returns>トランザクション中か</returns>
        public bool InTransaction() => this.mDbTransaction != null;
    }
}

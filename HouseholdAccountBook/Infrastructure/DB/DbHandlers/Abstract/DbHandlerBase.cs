using Dapper;
using HouseholdAccountBook.Infrastructure.Logger;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract
{
    /// <summary>
    /// Db Handler Base クラス
    /// </summary>
    public abstract class DbHandlerBase : IAsyncDisposable
    {
        /// <summary>
        /// 接続情報
        /// </summary>
        public abstract class ConnectInfoBase {
            /// <summary>
            /// DB種別
            /// </summary>
            /// <remarks>接続情報からどのDBに接続するのかが明らかになるのでここで保持する</remarks>
            public DBKind Kind { get; init; }
        }

        #region フィールド
        /// <summary>
        /// 接続情報
        /// </summary>
        protected ConnectInfoBase mConnectInfoBase;
        /// <summary>
        /// DB接続
        /// </summary>
        protected DbConnection mConnection;
        /// <summary>
        /// DBトランザクション
        /// </summary>
        protected DbTransaction mDbTransaction;
        #endregion

        #region プロパティ
        /// <summary>
        /// 対象データベースライブラリ
        /// </summary>
        public DBLibraryKind LibKind { get; protected init; } = DBLibraryKind.Undefined;
        /// <summary>
        /// 対象データベース
        /// </summary>
        public DBKind Kind => this.mConnectInfoBase.Kind;
        /// <summary>
        /// 接続可能か
        /// </summary>
        public bool CanOpen { get; protected set; } = true;
        /// <summary>
        /// 接続状態を取得する
        /// </summary>
        /// <returns>接続状態</returns>
        public bool IsOpen => this.mConnection != null && this.mConnection.State == System.Data.ConnectionState.Open;
        #endregion

        static DbHandlerBase()
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;
            SqlMapper.AddTypeHandler(new DateOnlyHandler());
        }

        /// <summary>
        /// <see cref="DbHandlerBase"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="connection">DB接続</param>
        public DbHandlerBase(DbConnection connection)
        {
            using FuncLog funcLog = new(new { }, Log.LogLevel.Trace);

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
            using FuncLog funcLog = new(new { timeoutMs }, Log.LogLevel.Trace);

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
            using FuncLog funcLog = new(new { }, Log.LogLevel.Trace);

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
        /// <param name="target">対象データベース</param>
        /// <returns>クエリ結果リスト。対象外の場合はデフォルト値</returns>
        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null, DBKindMask target = DBKindMask.All)
        {
            using FuncLog funcLog = new(new { sql, param, target }, Log.LogLevel.Trace);

            if (!target.Check(this.Kind)) { return default; }

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
        /// <param name="target">対象データベース</param>
        /// <returns>クエリ結果 または デフォルト値。対象外の場合はデフォルト値</returns>
        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null, DBKindMask target = DBKindMask.All)
        {
            using FuncLog funcLog = new(new { sql, param, target }, Log.LogLevel.Trace);

            if (!target.Check(this.Kind)) { return default; }

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
        /// <param name="target">対象データベース</param>
        /// <returns>クエリ結果。対象外の場合はデフォルト値</returns>
        /// <exception cref="InvalidOperationException">結果が1行以外の場合にスローされる</exception>
        public async Task<T> QuerySingleAsync<T>(string sql, object param = null, DBKindMask target = DBKindMask.All)
        {
            using FuncLog funcLog = new(new { sql, param, target }, Log.LogLevel.Trace);

            if (!target.Check(this.Kind)) { return default; }

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
        /// <param name="target">対象データベース</param>
        /// <returns>クエリ結果 または デフォルト値。対象外の場合はデフォルト値</returns>
        /// <exception cref="InvalidOperationException">結果が2行以上の場合にスローされる</exception>
        public async Task<T> QuerySingleOrDefaultAsync<T>(string sql, object param = null, DBKindMask target = DBKindMask.All)
        {
            using FuncLog funcLog = new(new { sql, param, target }, Log.LogLevel.Trace);

            if (!target.Check(this.Kind)) { return default; }

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
        /// <param name="target">対象データベース</param>
        /// <returns>変更件数。対象外の場合は0</returns>
        public async Task<int> ExecuteAsync(string sql, object param = null, DBKindMask target = DBKindMask.All)
        {
            using FuncLog funcLog = new(new { sql, param, target }, Log.LogLevel.Trace);

            if (!target.Check(this.Kind)) { return default; }

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
        public delegate Task ActionAsync();
        /// <summary>
        /// ロールバック後の処理
        /// </summary>
        public delegate void OnRollbacked();

        /// <summary>
        /// [非同期]トランザクション処理を行う
        /// </summary>
        /// <param name="target">対象データベース</param>
        /// <param name="actionAsync">トランザクション内の処理</param>
        /// <exception cref="DbException">DB関連の例外</exception>
        /// <exception cref="Exception">DB以外の例外</exception>
        public async Task ExecTransactionAsync(ActionAsync actionAsync, OnRollbacked onRollbacked = null, DBKindMask target = DBKindMask.All)
        {
            using FuncLog funcLog = new(new { target }, Log.LogLevel.Trace);

            if (!target.Check(this.Kind)) { return; }

            try {
                this.mDbTransaction = await this.mConnection.BeginTransactionAsync();

                await actionAsync?.Invoke();

                await this.mDbTransaction.CommitAsync();
            }
            catch (DbException e) {
                Console.WriteLine(e.Message);
                await this.mDbTransaction.RollbackAsync();
                onRollbacked?.Invoke();
                throw;
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                await this.mDbTransaction.RollbackAsync();
                onRollbacked?.Invoke();
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

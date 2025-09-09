using HouseholdAccountBook.DbHandler;
using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Models.DbHandler.Abstract;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Models.DbHandler
{
    /// <summary>
    /// DBハンドラファクトリ
    /// </summary>
    /// <remarks>接続情報に応じた <see cref="DbHandlerBase"/> の派生クラスを生成する</remarks>
    public class DbHandlerFactory
    {
        /// <summary>
        /// DBライブラリ種別
        /// </summary>
        private DBLibraryKind DBLibKind { get; set; } = DBLibraryKind.Undefined;
        /// <summary>
        /// 接続情報
        /// </summary>
        private readonly DbHandlerBase.ConnectInfo info;

        /// <summary>
        /// <see cref="DbHandlerFactory"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="info">接続情報</param>
        public DbHandlerFactory(DbHandlerBase.ConnectInfo info)
        {
            if (info is NpgsqlDbHandler.ConnectInfo) {
                this.DBLibKind = DBLibraryKind.PostgreSQL;
                this.info = info;
                return;
            }

            if (info is SQLiteDbHandler.ConnectInfo) {
                this.DBLibKind = DBLibraryKind.SQLite;
                this.info = info;
                return;
            }

            if (info is OleDbHandler.ConnectInfo) {
                this.DBLibKind = DBLibraryKind.OleDb;
                this.info = info;
                return;
            }

            this.DBLibKind = DBLibraryKind.Undefined;
            this.info = null;
        }

        /// <summary>
        /// [非同期] <see cref="DbHandlerBase"/> 生成
        /// </summary>
        /// <returns>DbHandler</returns>
        public async Task<DbHandlerBase> CreateAsync()
        {
            try {
                DbHandlerBase dbHandler = this.DBLibKind switch {
                    DBLibraryKind.SQLite => new SQLiteDbHandler(this.info as SQLiteDbHandler.ConnectInfo),
                    DBLibraryKind.PostgreSQL => new NpgsqlDbHandler(this.info as NpgsqlDbHandler.ConnectInfo),
                    DBLibraryKind.OleDb => new OleDbHandler(this.info as OleDbHandler.ConnectInfo),
                    _ => null,
                };

                _ = await dbHandler.OpenAsync();
                ; return dbHandler;
            }
            catch (System.TimeoutException) {
                throw;
            }
        }
    }
}

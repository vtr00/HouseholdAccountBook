using HouseholdAccountBook.Infrastructure.DB;
using HouseholdAccountBook.Infrastructure.DB.DbDao.Abstract;
using HouseholdAccountBook.Infrastructure.DB.DbDao.DbTable;
using HouseholdAccountBook.Infrastructure.DB.DbDao.KHDbTable;
using HouseholdAccountBook.Infrastructure.DB.DbDto.Abstract;
using HouseholdAccountBook.Infrastructure.DB.DbDto.DbTable;
using HouseholdAccountBook.Infrastructure.DB.DbDto.KHDbTable;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Models.DbHandlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Models.AppServices
{
    /// <summary>
    /// DBインポートサービス
    /// </summary>
    /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
    public class DbImportService(DbHandlerFactory dbHandlerFactory)
    {
        /// <summary>
        /// DBハンドラファクトリ
        /// </summary>
        private readonly DbHandlerFactory mDbHandlerFactory = dbHandlerFactory;

        /// <summary>
        /// 移行元から全行を読み込んで移行先に挿入する
        /// </summary>
        /// <typeparam name="DTO1">移行元型</typeparam>
        /// <typeparam name="DTO2">移行先型</typeparam>
        /// <param name="src">移行元DTO</param>
        /// <param name="dest">移行先DTO</param>
        /// <param name="converter">型変換関数</param>
        /// <returns>件数差</returns>
        private static async Task<int> Mapping<DTO1, DTO2>(IReadTableDao<DTO1> src, IWriteTableDao<DTO2> dest,
                                                           Func<IEnumerable<DTO1>, IEnumerable<DTO2>> converter) where DTO1 : DtoBase where DTO2 : DtoBase
        {
            ArgumentNullException.ThrowIfNull(src);
            ArgumentNullException.ThrowIfNull(dest);
            ArgumentNullException.ThrowIfNull(converter);

            IEnumerable<DTO1> srcDtoList = await src.FindAllAsync();
            _ = await dest.DeleteAllAsync();
            if (srcDtoList is IEnumerable<ISequentialIDDto> srcSeqDtoList && dest is ISequentialIDDao<ISequentialIDDto> destSeq) {
                // シーケンスを更新する
                await destSeq.SetIdSequenceAsync(srcSeqDtoList);
            }
            int srcCount = srcDtoList.Count();
            int dstCount = await dest.BulkInsertAsync(converter(srcDtoList));
            return srcCount - dstCount;
        }

        /// <summary>
        /// 移行元から全行を読み込んで移行先に挿入する
        /// </summary>
        /// <typeparam name="DTO">型</typeparam>
        /// <param name="src">移行元DTO</param>
        /// <param name="dest">移行先DTO</param>
        /// <returns>件数差</returns>
        private static async Task<int> Mapping<DTO>(IReadTableDao<DTO> src, IWriteTableDao<DTO> dest) where DTO : DtoBase
        {
            ArgumentNullException.ThrowIfNull(src);
            ArgumentNullException.ThrowIfNull(dest);

            IEnumerable<DTO> srcDtoList = await src.FindAllAsync();
            _ = await dest.DeleteAllAsync();
            if (srcDtoList is IEnumerable<ISequentialIDDto> srcSeqDtoList && dest is ISequentialIDDao<ISequentialIDDto> destSeq) {
                // シーケンスを更新する
                await destSeq.SetIdSequenceAsync(srcSeqDtoList);
            }
            int srcCount = srcDtoList.Count();
            int dstCount = await dest.BulkInsertAsync(srcDtoList);
            return srcCount - dstCount;
        }

        /// <summary>
        /// 記帳風月からインポートする
        /// </summary>
        /// <param name="info">Dle Db接続情報</param>
        /// <returns>(成功/失敗, 列数の差分)</returns>
        public async Task<(bool, int)> ImportKichoFugetsuDbAsync(OleDbHandler.ConnectInfo info)
        {
            using FuncLog funcLog = new(new { info });

            bool result = false;
            int actRowsDiff = 0;

            await using OleDbHandler dbHandlerOle = await new DbHandlerFactory(info).CreateAsync() as OleDbHandler;
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            if (dbHandlerOle.IsOpen) {
                await dbHandler.ExecTransactionAsync(async () => {
                    CbmBookDao cbmBookDao = new(dbHandlerOle);
                    MstBookDao mstBookDao = new(dbHandler);
                    _ = await Mapping(cbmBookDao, mstBookDao, src => src.Select(dto => new MstBookDto(dto)));

                    CbmCategoryDao cbmCategoryDao = new(dbHandlerOle);
                    MstCategoryDao mstCategoryDao = new(dbHandler);
                    _ = await Mapping(cbmCategoryDao, mstCategoryDao, src => src.Select(dto => new MstCategoryDto(dto)));

                    CbmItemDao cbmItemDao = new(dbHandlerOle);
                    MstItemDao mstItemDao = new(dbHandler);
                    _ = await Mapping(cbmItemDao, mstItemDao, src => src.Select(dto => new MstItemDto(dto)));

                    CbtActDao cbtActDao = new(dbHandlerOle);
                    HstActionDao hstActionDao = new(dbHandler);
                    actRowsDiff = await Mapping(cbtActDao, hstActionDao, src => src.Where(dto => !(dto.INCOME == 0 && dto.EXPENSE == 0)).Select(dto => new HstActionDto(dto)));

                    HstGroupDao hstGroupDao = new(dbHandler);
                    var cbtActDtoList = await cbtActDao.FindAllAsync();
                    _ = await hstGroupDao.DeleteAllAsync();
                    int maxGroupId = 0; // グループIDの最大値
                    IEnumerable<HstGroupDto> hstGroupDtoList = [];
                    foreach (CbtActDto cbtActDto in cbtActDtoList) {
                        // groupId が存在しないなら次のレコードへ
                        if (cbtActDto.GROUP_ID == 0) { continue; }
                        int groupId = cbtActDto.GROUP_ID;

                        // groupId のレコードが存在しないとき
                        if (!hstGroupDtoList.Any(dto => dto.GroupId == groupId)) {
                            // グループIDの最大値を更新する
                            if (maxGroupId < groupId) { maxGroupId = groupId; }

                            // グループの種類を調べる
                            var cbtActDtoList2 = cbtActDtoList.Where(dto => dto.GROUP_ID == groupId);
                            GroupKind groupKind = GroupKind.Repeat;
                            int? tmpBookId = null;
                            foreach (CbtActDto cbtActDto2 in cbtActDtoList2) {
                                if (tmpBookId == null) { // 1つ目のレコードの帳簿IDを記録する
                                    tmpBookId = cbtActDto2.BOOK_ID;
                                }
                                else { // 2つ目のレコードの帳簿IDが1つ目と一致するか
                                    if (tmpBookId != cbtActDto2.BOOK_ID) {
                                        // 帳簿が一致しない場合は移動
                                        groupKind = GroupKind.Move;
                                    }
                                    else {
                                        // 帳簿が一致する場合は繰り返し
                                        groupKind = GroupKind.Repeat;
                                    }
                                    break;
                                }
                            }

                            // グループテーブルのレコードを追加する
                            hstGroupDtoList = hstGroupDtoList.Append(new HstGroupDto {
                                GroupId = groupId,
                                GroupKind = (int)groupKind
                            });
                        }
                    }
                    if (0 < maxGroupId) {
                        await hstGroupDao.SetIdSequenceAsync(maxGroupId);
                        _ = await hstGroupDao.BulkInsertAsync(hstGroupDtoList);
                    }

                    HstShopDao hstShopDao = new(dbHandler);
                    _ = await hstShopDao.DeleteAllAsync();

                    CbtNoteDao cbtNoteDao = new(dbHandlerOle);
                    HstRemarkDao hstRemarkDao = new(dbHandler);
                    _ = await Mapping(cbtNoteDao, hstRemarkDao, src => src.Select(dto => new HstRemarkDto(dto)));

                    CbrBookDao cbrBookDao = new(dbHandlerOle);
                    RelBookItemDao relBookItemDao = new(dbHandler);
                    _ = await Mapping(cbrBookDao, relBookItemDao, src => src.Select(dto => new RelBookItemDto(dto)));

                    result = true;
                }, () => result = false);
            }
            else {
                result = false;
            }

            return (result, actRowsDiff);
        }

        /// <summary>
        /// PostgreSQLからインポートする
        /// </summary>
        /// <param name="info">PostgreSQL接続情報</param>
        /// <returns>成功/失敗</returns>
        public async Task<bool> ImportPostgreSQLAsync(NpgsqlDbHandler.ConnectInfo info)
        {
            using FuncLog funcLog = new(new { info });

            bool result = false;

            await using NpgsqlDbHandler dbHandlerNpgsql = await new DbHandlerFactory(info).CreateAsync() as NpgsqlDbHandler;
            await using DbHandlerBase dbHandlerSQLite = await this.mDbHandlerFactory.CreateAsync();

            if (dbHandlerNpgsql.IsOpen) {
                await dbHandlerSQLite.ExecTransactionAsync(async () => {
                    MstBookDao mstBookDao1 = new(dbHandlerNpgsql);
                    MstBookDao mstBookDao2 = new(dbHandlerSQLite);
                    _ = await Mapping(mstBookDao1, mstBookDao2);

                    MstCategoryDao mstCategoryDao1 = new(dbHandlerNpgsql);
                    MstCategoryDao mstCategoryDao2 = new(dbHandlerSQLite);
                    _ = await Mapping(mstCategoryDao1, mstCategoryDao2);

                    MstItemDao mstItemDao1 = new(dbHandlerNpgsql);
                    MstItemDao mstItemDao2 = new(dbHandlerSQLite);
                    _ = await Mapping(mstItemDao1, mstItemDao2);

                    HstActionDao hstActionDao1 = new(dbHandlerNpgsql);
                    HstActionDao hstActionDao2 = new(dbHandlerSQLite);
                    _ = await Mapping(hstActionDao1, hstActionDao2);

                    HstGroupDao hstGroupDao1 = new(dbHandlerNpgsql);
                    HstGroupDao hstGroupDao2 = new(dbHandlerSQLite);
                    _ = await Mapping(hstGroupDao1, hstGroupDao2);

                    HstShopDao hstShopDao1 = new(dbHandlerNpgsql);
                    HstShopDao hstShopDao2 = new(dbHandlerSQLite);
                    _ = await Mapping(hstShopDao1, hstShopDao2);

                    HstRemarkDao hstRemarkDao1 = new(dbHandlerNpgsql);
                    HstRemarkDao hstRemarkDao2 = new(dbHandlerSQLite);
                    _ = await Mapping(hstRemarkDao1, hstRemarkDao2);

                    RelBookItemDao relBookItemDao1 = new(dbHandlerNpgsql);
                    RelBookItemDao relBookItemDao2 = new(dbHandlerSQLite);
                    _ = await Mapping(relBookItemDao1, relBookItemDao2);

                    // スキーマバージョンは移行しない

                    result = true;
                }, () => result = false);
            }

            return result;
        }

        /// <summary>
        /// カスタムファイルからインポートする
        /// </summary>
        /// <param name="filePath">カスタムファイルパス</param>
        /// <returns>成功/失敗</returns>
        public async Task<bool> ImportCustomFileAsync(string filePath)
        {
            using FuncLog funcLog = new(new { filePath });

            bool result = true;
            int schemaVersion = 0;

            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                await dbHandler.ExecTransactionAsync(async () => {
                    MstBookDao mstBookDao = new(dbHandler);
                    MstCategoryDao mstCategoryDao = new(dbHandler);
                    MstItemDao mstItemDao = new(dbHandler);
                    HstActionDao hstActionDao = new(dbHandler);
                    HstGroupDao hstGroupDao = new(dbHandler);
                    HstShopDao hstShopDao = new(dbHandler);
                    HstRemarkDao hstRemarkDao = new(dbHandler);
                    RelBookItemDao relBookItemDao = new(dbHandler);
                    MtdSchemaVersionDao mtdSchemaVersionDao = new(dbHandler);

                    // 現在のスキーマバージョンを取得しておく
                    schemaVersion = await mtdSchemaVersionDao.SelectSchemaVersionAsync();

                    // 既存のデータを削除する
                    _ = await mstBookDao.DeleteAllAsync();
                    _ = await mstCategoryDao.DeleteAllAsync();
                    _ = await mstItemDao.DeleteAllAsync();
                    _ = await hstActionDao.DeleteAllAsync();
                    _ = await hstGroupDao.DeleteAllAsync();
                    _ = await hstShopDao.DeleteAllAsync();
                    _ = await hstRemarkDao.DeleteAllAsync();
                    _ = await relBookItemDao.DeleteAllAsync();
                    _ = await mtdSchemaVersionDao.DeleteAllAsync(); // リストア時にレコードが増えるためスキーマバージョンを削除する

                    result = true;
                }, () => result = false);
            }

            result = await DbBackUpManager.Instance.ExecuteRestorePostgreSQLAsync(filePath);

            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                // スキーマバージョンを元に戻す
                MtdSchemaVersionDao mtdSchemaVersionDao = new(dbHandler);
                _ = await mtdSchemaVersionDao.UpsertSchemaVersionAsync(schemaVersion);
            }

            return result;
        }

        /// <summary>
        /// SQLiteからインポートする
        /// </summary>
        /// <param name="dbKind">インポート先のDB種別</param>
        /// <param name="fromFilePath">インポート元のSQLiteファイルパス</param>
        /// <param name="destFilePath">インポート先のSQLiteファイルパス</param>
        /// <returns></returns>
        public async Task<bool> ImportSQLiteAsync(DBKind dbKind, string fromFilePath, string destFilePath = "")
        {
            using FuncLog funcLog = new(new { dbKind, fromFilePath, destFilePath });

            bool result = false;

            switch (dbKind) {
                case DBKind.SQLite: {
                    try {
                        // ファイルをコピーするだけ
                        File.Copy(fromFilePath, destFilePath, true);
                        result = true;
                    }
                    catch (Exception) {
                        Log.Error("Failed to copy SQLite file.");
                        throw;
                    }
                    break;
                }
                case DBKind.PostgreSQL: {
                    SQLiteDbHandler.ConnectInfo info = new() {
                        FilePath = fromFilePath
                    };

                    await using SQLiteDbHandler dbHandlerSqlite = await new DbHandlerFactory(info).CreateAsync() as SQLiteDbHandler;
                    await using DbHandlerBase dbHandlerNpgsql = await this.mDbHandlerFactory.CreateAsync();

                    if (dbHandlerSqlite.IsOpen) {
                        await dbHandlerNpgsql.ExecTransactionAsync(async () => {
                            MstBookDao mstBookDao1 = new(dbHandlerSqlite);
                            MstBookDao mstBookDao2 = new(dbHandlerNpgsql);
                            _ = await Mapping(mstBookDao1, mstBookDao2);

                            MstCategoryDao mstCategoryDao1 = new(dbHandlerSqlite);
                            MstCategoryDao mstCategoryDao2 = new(dbHandlerNpgsql);
                            _ = await Mapping(mstCategoryDao1, mstCategoryDao2);

                            MstItemDao mstItemDao1 = new(dbHandlerSqlite);
                            MstItemDao mstItemDao2 = new(dbHandlerNpgsql);
                            _ = await Mapping(mstItemDao1, mstItemDao2);

                            HstActionDao hstActionDao1 = new(dbHandlerSqlite);
                            HstActionDao hstActionDao2 = new(dbHandlerNpgsql);
                            _ = await Mapping(hstActionDao1, hstActionDao2);

                            HstGroupDao hstGroupDao1 = new(dbHandlerSqlite);
                            HstGroupDao hstGroupDao2 = new(dbHandlerNpgsql);
                            _ = await Mapping(hstGroupDao1, hstGroupDao2);

                            HstShopDao hstShopDao1 = new(dbHandlerSqlite);
                            HstShopDao hstShopDao2 = new(dbHandlerNpgsql);
                            _ = await Mapping(hstShopDao1, hstShopDao2);

                            HstRemarkDao hstRemarkDao1 = new(dbHandlerSqlite);
                            HstRemarkDao hstRemarkDao2 = new(dbHandlerNpgsql);
                            _ = await Mapping(hstRemarkDao1, hstRemarkDao2);

                            RelBookItemDao relBookItemDao1 = new(dbHandlerSqlite);
                            RelBookItemDao relBookItemDao2 = new(dbHandlerNpgsql);
                            _ = await Mapping(relBookItemDao1, relBookItemDao2);

                            // スキーマバージョンは移行しない

                            result = true;
                        }, () => result = false);
                    }
                    break;
                }
                default:
                    break;
            }

            return result;
        }
    }
}

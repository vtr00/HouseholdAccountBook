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
using System.Linq;
using System.Threading;
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
        private static async Task<int> MappingAsync<DTO1, DTO2>(IReadTableDao<DTO1> src, IWriteTableDao<DTO2> dest,
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
        private static async Task<int> MappingAsync<DTO>(IReadTableDao<DTO> src, IWriteTableDao<DTO> dest) where DTO : DtoBase
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
        /// <param name="token">キャンセル用トークン</param>
        /// <param name="progress">進捗</param>
        /// <exception cref="OperationCanceledException">キャンセル例外</exception>
        /// <returns>(成功/失敗, 列数の差分)</returns>
        public async Task<(bool, int)> ImportKichoFugetsuDbAsync(OleDbHandler.ConnectInfo info, CancellationToken token, IProgress<int> progress)
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
                    _ = await MappingAsync(cbmBookDao, mstBookDao, src => src.Select(dto => new MstBookDto(dto)));
                    token.ThrowIfCancellationRequested();
                    progress.Report(10);

                    CbmCategoryDao cbmCategoryDao = new(dbHandlerOle);
                    MstCategoryDao mstCategoryDao = new(dbHandler);
                    _ = await MappingAsync(cbmCategoryDao, mstCategoryDao, src => src.Select(dto => new MstCategoryDto(dto)));
                    token.ThrowIfCancellationRequested();
                    progress.Report(20);

                    CbmItemDao cbmItemDao = new(dbHandlerOle);
                    MstItemDao mstItemDao = new(dbHandler);
                    _ = await MappingAsync(cbmItemDao, mstItemDao, src => src.Select(dto => new MstItemDto(dto)));
                    token.ThrowIfCancellationRequested();
                    progress.Report(30);

                    CbtActDao cbtActDao = new(dbHandlerOle);
                    HstActionDao hstActionDao = new(dbHandler);
                    actRowsDiff = await MappingAsync(cbtActDao, hstActionDao, src => src.Where(dto => !(dto.INCOME == 0 && dto.EXPENSE == 0)).Select(dto => new HstActionDto(dto)));
                    token.ThrowIfCancellationRequested();
                    progress.Report(50);

                    HstGroupDao hstGroupDao = new(dbHandler);
                    IEnumerable<CbtActDto> cbtActDtoList = await cbtActDao.FindAllAsync();
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
                            IEnumerable<CbtActDto> cbtActDtoList2 = cbtActDtoList.Where(dto => dto.GROUP_ID == groupId);
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
                    token.ThrowIfCancellationRequested();
                    progress.Report(60);

                    HstShopDao hstShopDao = new(dbHandler);
                    _ = await hstShopDao.DeleteAllAsync();
                    token.ThrowIfCancellationRequested();
                    progress.Report(70);

                    CbtNoteDao cbtNoteDao = new(dbHandlerOle);
                    HstRemarkDao hstRemarkDao = new(dbHandler);
                    _ = await MappingAsync(cbtNoteDao, hstRemarkDao, src => src.Select(dto => new HstRemarkDto(dto)));
                    token.ThrowIfCancellationRequested();
                    progress.Report(80);

                    CbrBookDao cbrBookDao = new(dbHandlerOle);
                    RelBookItemDao relBookItemDao = new(dbHandler);
                    _ = await MappingAsync(cbrBookDao, relBookItemDao, src => src.Select(dto => new RelBookItemDto(dto)));
                    token.ThrowIfCancellationRequested();
                    progress.Report(90);

                    // アセットマスタはインポート対象外のため、このタイミングでデータを作成する
                    MstAssetDao mstAssetDao = new(dbHandler);
                    _ = await mstAssetDao.DeleteAllAsync();
                    // デフォルトアセットを設定する
                    await SetDefaultAssetIdAsync(mstAssetDao);
                    progress.Report(100);

                    result = true;
                }, () => result = false);

                if (result) {
                    // アセットリストを更新する
                    await AssetService.Instance.UpdateAssets(this.mDbHandlerFactory);
                }
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
        /// <param name="token">キャンセル用トークン</param>
        /// <param name="progress">進捗</param>
        /// <exception cref="OperationCanceledException">キャンセル例外</exception>
        /// <returns>成功/失敗</returns>
        public async Task<bool> ImportPostgreSQLAsync(NpgsqlDbHandler.ConnectInfo info, CancellationToken token, IProgress<int> progress)
        {
            using FuncLog funcLog = new(new { info });

            bool result = false;

            // インポート元のスキーマバージョンを取得する
            DbHandlerFactory npgsqlDbHandlerFactory = new(info);
            DbMigrationService migService = new(npgsqlDbHandlerFactory);
            int srcVersion = await migService.GetSchemaVersionAsync();

            await using NpgsqlDbHandler dbHandlerNpgsql = await npgsqlDbHandlerFactory.CreateAsync() as NpgsqlDbHandler;
            await using DbHandlerBase dbHandlerSQLite = await this.mDbHandlerFactory.CreateAsync();

            if (dbHandlerNpgsql.IsOpen) {
                await dbHandlerSQLite.ExecTransactionAsync(async () => {
                    MstBookDao mstBookDao1 = new(dbHandlerNpgsql);
                    MstBookDao mstBookDao2 = new(dbHandlerSQLite);
                    _ = await MappingAsync(mstBookDao1, mstBookDao2);
                    token.ThrowIfCancellationRequested();
                    progress.Report(10);

                    MstCategoryDao mstCategoryDao1 = new(dbHandlerNpgsql);
                    MstCategoryDao mstCategoryDao2 = new(dbHandlerSQLite);
                    _ = await MappingAsync(mstCategoryDao1, mstCategoryDao2);
                    token.ThrowIfCancellationRequested();
                    progress.Report(20);

                    MstItemDao mstItemDao1 = new(dbHandlerNpgsql);
                    MstItemDao mstItemDao2 = new(dbHandlerSQLite);
                    _ = await MappingAsync(mstItemDao1, mstItemDao2);
                    token.ThrowIfCancellationRequested();
                    progress.Report(30);

                    HstActionDao hstActionDao1 = new(dbHandlerNpgsql);
                    HstActionDao hstActionDao2 = new(dbHandlerSQLite);
                    _ = await MappingAsync(hstActionDao1, hstActionDao2);
                    token.ThrowIfCancellationRequested();
                    progress.Report(50);

                    HstGroupDao hstGroupDao1 = new(dbHandlerNpgsql);
                    HstGroupDao hstGroupDao2 = new(dbHandlerSQLite);
                    _ = await MappingAsync(hstGroupDao1, hstGroupDao2);
                    token.ThrowIfCancellationRequested();
                    progress.Report(60);

                    HstShopDao hstShopDao1 = new(dbHandlerNpgsql);
                    HstShopDao hstShopDao2 = new(dbHandlerSQLite);
                    _ = await MappingAsync(hstShopDao1, hstShopDao2);
                    token.ThrowIfCancellationRequested();
                    progress.Report(70);

                    HstRemarkDao hstRemarkDao1 = new(dbHandlerNpgsql);
                    HstRemarkDao hstRemarkDao2 = new(dbHandlerSQLite);
                    _ = await MappingAsync(hstRemarkDao1, hstRemarkDao2);
                    token.ThrowIfCancellationRequested();
                    progress.Report(80);

                    RelBookItemDao relBookItemDao1 = new(dbHandlerNpgsql);
                    RelBookItemDao relBookItemDao2 = new(dbHandlerSQLite);
                    _ = await MappingAsync(relBookItemDao1, relBookItemDao2);
                    token.ThrowIfCancellationRequested();
                    progress.Report(90);

                    MstAssetDao mstAssetDao1 = new(dbHandlerNpgsql);
                    MstAssetDao mstAssetDao2 = new(dbHandlerSQLite);
                    if (1 <= srcVersion) {
                        _ = await MappingAsync(mstAssetDao1, mstAssetDao2);
                    }
                    else {
                        _ = await mstAssetDao2.DeleteAllAsync();
                    }
                    // デフォルトアセットを設定する
                    await SetDefaultAssetIdAsync(mstAssetDao2);
                    token.ThrowIfCancellationRequested();
                    progress.Report(100);

                    // スキーマバージョンは移行しない

                    result = true;
                }, () => result = false);
            }

            if (result) {
                // アセットリストを更新する
                await AssetService.Instance.UpdateAssets(this.mDbHandlerFactory);
            }

            return result;
        }

        /// <summary>
        /// SQLiteからインポートする
        /// </summary>
        /// <param name="fromFilePath">インポート元のSQLiteファイルパス</param>
        /// <param name="token">キャンセル用トークン</param>
        /// <param name="progress">進捗</param>
        /// <exception cref="OperationCanceledException">キャンセル例外</exception>
        /// <returns>成功/失敗</returns>
        public async Task<bool> ImportSQLiteAsync(string fromFilePath, CancellationToken token, IProgress<int> progress)
        {
            using FuncLog funcLog = new(new { fromFilePath });

            bool result = false;

            // インポート元のスキーマバージョンを取得する
            DbHandlerFactory sqliteDbHandlerFactory = new(new SQLiteDbHandler.ConnectInfo() { FilePath = fromFilePath });
            DbMigrationService manager = new(sqliteDbHandlerFactory);
            int srcVersion = await manager.GetSchemaVersionAsync();

            await using SQLiteDbHandler dbHandlerSQLite = await sqliteDbHandlerFactory.CreateAsync(false) as SQLiteDbHandler;
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            if (dbHandlerSQLite.IsOpen) {
                await dbHandler.ExecTransactionAsync(async () => {
                    MstBookDao mstBookDao1 = new(dbHandlerSQLite);
                    MstBookDao mstBookDao2 = new(dbHandler);
                    _ = await MappingAsync(mstBookDao1, mstBookDao2);
                    token.ThrowIfCancellationRequested();
                    progress.Report(10);

                    MstCategoryDao mstCategoryDao1 = new(dbHandlerSQLite);
                    MstCategoryDao mstCategoryDao2 = new(dbHandler);
                    _ = await MappingAsync(mstCategoryDao1, mstCategoryDao2);
                    token.ThrowIfCancellationRequested();
                    progress.Report(20);

                    MstItemDao mstItemDao1 = new(dbHandlerSQLite);
                    MstItemDao mstItemDao2 = new(dbHandler);
                    _ = await MappingAsync(mstItemDao1, mstItemDao2);
                    token.ThrowIfCancellationRequested();
                    progress.Report(30);

                    HstActionDao hstActionDao1 = new(dbHandlerSQLite);
                    HstActionDao hstActionDao2 = new(dbHandler);
                    _ = await MappingAsync(hstActionDao1, hstActionDao2);
                    token.ThrowIfCancellationRequested();
                    progress.Report(60);

                    HstGroupDao hstGroupDao1 = new(dbHandlerSQLite);
                    HstGroupDao hstGroupDao2 = new(dbHandler);
                    _ = await MappingAsync(hstGroupDao1, hstGroupDao2);
                    token.ThrowIfCancellationRequested();
                    progress.Report(70);

                    HstShopDao hstShopDao1 = new(dbHandlerSQLite);
                    HstShopDao hstShopDao2 = new(dbHandler);
                    _ = await MappingAsync(hstShopDao1, hstShopDao2);
                    token.ThrowIfCancellationRequested();
                    progress.Report(80);

                    HstRemarkDao hstRemarkDao1 = new(dbHandlerSQLite);
                    HstRemarkDao hstRemarkDao2 = new(dbHandler);
                    _ = await MappingAsync(hstRemarkDao1, hstRemarkDao2);
                    token.ThrowIfCancellationRequested();
                    progress.Report(90);

                    RelBookItemDao relBookItemDao1 = new(dbHandlerSQLite);
                    RelBookItemDao relBookItemDao2 = new(dbHandler);
                    _ = await MappingAsync(relBookItemDao1, relBookItemDao2);
                    token.ThrowIfCancellationRequested();
                    progress.Report(100);

                    MstAssetDao mstAssetDao1 = new(dbHandlerSQLite);
                    MstAssetDao mstAssetDao2 = new(dbHandler);
                    if (1 <= srcVersion) {
                        _ = await MappingAsync(mstAssetDao1, mstAssetDao2);
                    }
                    else {
                        _ = await mstAssetDao2.DeleteAllAsync();
                    }
                    // デフォルトアセットを設定する
                    await SetDefaultAssetIdAsync(mstAssetDao2);
                    token.ThrowIfCancellationRequested();
                    progress.Report(100);

                    // スキーマバージョンは移行しない

                    result = true;
                }, () => result = false);
            }

            if (result) {
                // アセットリストを更新する
                await AssetService.Instance.UpdateAssets(this.mDbHandlerFactory);
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
                    MstAssetDao mstAssetDao = new(dbHandler);

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
                    _ = await mstAssetDao.DeleteAllAsync();

                    result = true;
                }, () => result = false);
            }

            if (result) {
                result = await DbBackUpService.Instance.ExecuteRestorePostgreSQLAsync(filePath);

                if (result) {
                    await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                        // デフォルトアセットを設定する
                        MstAssetDao mstAssetDao = new(dbHandler);
                        await SetDefaultAssetIdAsync(mstAssetDao);

                        // スキーマバージョンを元に戻す
                        MtdSchemaVersionDao mtdSchemaVersionDao = new(dbHandler);
                        _ = await mtdSchemaVersionDao.UpsertSchemaVersionAsync(schemaVersion);
                    }

                    // アセットリストを更新する
                    await AssetService.Instance.UpdateAssets(this.mDbHandlerFactory);
                }
            }

            return result;
        }

        /// <summary>
        /// デフォルトアセットIDを登録する
        /// </summary>
        /// <param name="dao">アセットテーブルDAO</param>
        /// <returns></returns>
        private static async Task SetDefaultAssetIdAsync(MstAssetDao dao)
        {
            int assetId = 0;
            IEnumerable<MstAssetDto> dtoList = await dao.FindAllAsync();
            if (dtoList.Any()) {
                decimal baseRate = decimal.MaxValue;
                foreach (MstAssetDto dto in dtoList) {
                    // BaseRate が1に近いアセットを探す
                    if (Math.Abs(dto.BaseRate - 1) < Math.Abs(baseRate - 1)) {
                        assetId = dto.AssetId;
                        baseRate = dto.BaseRate;
                    }
                }
            }
            else {
                assetId = await dao.InsertDefaultReturningIdAsync();
            }
            UserSettingService.Instance.DefaultAssetId = assetId;
        }
    }
}

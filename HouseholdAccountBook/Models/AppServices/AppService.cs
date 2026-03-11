using HouseholdAccountBook.Infrastructure.DB.DbDao.Compositions;
using HouseholdAccountBook.Infrastructure.DB.DbDao.DbTable;
using HouseholdAccountBook.Infrastructure.DB.DbDto.DbTable;
using HouseholdAccountBook.Infrastructure.DB.DbDto.Others;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities.Extensions;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Models.AppServices
{
    /// <summary>
    /// アプリサービス
    /// </summary>
    /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
    public class AppService(DbHandlerFactory dbHandlerFactory)
    {
        /// <summary>
        /// DBハンドラファクトリ
        /// </summary>
        private readonly DbHandlerFactory mDbHandlerFactory = dbHandlerFactory;

        /// <summary>
        /// 帳簿Modelリストを取得する
        /// </summary>
        /// <param name="period">期間</param>
        /// <param name="initialName">1番目に追加する項目の名称(空文字の場合は追加しない)</param>
        /// <returns>帳簿Modelリスト</returns>
        public async Task<IEnumerable<BookModel>> LoadBookListAsync(PeriodObj<DateOnly> period = null, string initialName = "")
        {
            using FuncLog funcLog = new(new { period, initialName });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            List<BookModel> bmList = [];

            // 1番目に表示する項目を追加する
            if (initialName != string.Empty) {
                bmList.Add(new(null, initialName));
            }

            MstBookDao mstBookDao = new(dbHandler);
            var dtoList = await mstBookDao.FindAllAsync();
            foreach (MstBookDto dto in dtoList) {
                MstBookDto.JsonDto jsonObj = dto.JsonCode == null ? null : new(dto.JsonCode);

                if (DateOnlyExtensions.IsWithIn(period?.Start, period?.End, jsonObj?.StartDate?.ToDateOnly(), jsonObj?.EndDate?.ToDateOnly())) {
                    BookModel vm = new(dto.BookId, dto.BookName) {
                        Remark = jsonObj?.Remark ?? string.Empty,
                        BookKind = (BookKind)dto.BookKind,
                        DebitBookId = dto.DebitBookId,
                        PayDay = dto.PayDay
                    };
                    bmList.Add(vm);
                }
            }

            return bmList;
        }

        /// <summary>
        /// 分類Modelリストを取得する
        /// </summary>
        /// <param name="bookId">絞り込み対象の帳簿ID</param>
        /// <param name="balanceKind">絞り込み対象の収支種別</param>
        /// <returns>分類Modelリスト</returns>
        public async Task<IEnumerable<CategoryModel>> LoadCategoryListAsync(BookIdObj bookId, BalanceKind balanceKind, string initialName = "")
        {
            using FuncLog funcLog = new(new { bookId, balanceKind });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            List<CategoryModel> cmList = [];

            if (initialName != string.Empty) {
                cmList.Add(new CategoryModel(-1, initialName, BalanceKind.Others));
            }

            MstCategoryWithinBookDao mstCategoryWithinBookDao = new(dbHandler);
            IEnumerable<MstCategoryDto> dtoList = await mstCategoryWithinBookDao.FindByBookIdAndBalanceKindAsync((int)bookId, (int)balanceKind);
            foreach (MstCategoryDto dto in dtoList) {
                cmList.Add(new(dto.CategoryId, dto.CategoryName, (BalanceKind)dto.BalanceKind));
            }

            return cmList;
        }

        /// <summary>
        /// 項目Modelリストを取得する
        /// </summary>
        /// <param name="bookId">絞り込み対象の帳簿ID</param>
        /// <param name="balanceKind">絞り込み対象の収支種別</param>
        /// <param name="categoryId">絞り込み対象の分類ID</param>
        /// <returns>項目Modelリスト</returns>
        public async Task<IEnumerable<ItemModel>> LoadItemListAsync(BookIdObj bookId, BalanceKind balanceKind, CategoryIdObj categoryId)
        {
            using FuncLog funcLog = new(new { bookId, balanceKind, categoryId });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            List<ItemModel> imList = [];

            CategoryItemInfoDao categoryItemInfoDao = new(dbHandler);
            IEnumerable<CategoryItemInfoDto> dtoList = (categoryId is null || categoryId == -1)
                ? await categoryItemInfoDao.FindByBookIdAndBalanceKindAsync((int)bookId, (int)balanceKind)
                : await categoryItemInfoDao.FindByBookIdAndCategoryIdAsync((int)bookId, (int)categoryId);
            foreach (CategoryItemInfoDto dto in dtoList) {
                ItemModel vm = new(dto.ItemId, dto.ItemName) {
                    CategoryName = (int)categoryId == -1 ? dto.CategoryName : ""
                };
                imList.Add(vm);
            }

            return imList;
        }

        /// <summary>
        /// 店舗Modelリストを取得する
        /// </summary>
        /// <param name="itemId">絞り込み対象の項目ID</param>
        /// <param name="insertEmpty">空のデータを追加するか</param>
        /// <returns>店舗Modelリスト</returns>
        public async Task<IEnumerable<ShopModel>> LoadShopListAsync(ItemIdObj itemId, bool insertEmpty = false)
        {
            using FuncLog funcLog = new(new { itemId, insertEmpty });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            List<ShopModel> smList = [];

            if (insertEmpty) {
                smList.Add(new ShopModel(string.Empty));
            }

            ShopInfoDao shopInfoDao = new(dbHandler);
            IEnumerable<ShopInfoDto> dtoList = await shopInfoDao.FindByItemIdAsync((int)itemId);
            smList.AddRange(dtoList.Select(static dto => new ShopModel(dto.ShopName) {
                UsedCount = dto.Count,
                UsedTime = dto.UsedTime
            }));

            return smList;
        }

        /// <summary>
        /// 備考Modelリストを取得する
        /// </summary>
        /// <param name="itemId">絞り込み対象の項目ID</param>
        /// <param name="insertEmpty">空のデータを追加するか</param>
        /// <returns>備考Modelリスト</returns>
        public async Task<IEnumerable<RemarkModel>> LoadRemarkListAsync(ItemIdObj itemId, bool insertEmpty = false)
        {
            using FuncLog funcLog = new(new { itemId, insertEmpty });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            List<RemarkModel> rmList = [];

            if (insertEmpty) {
                rmList.Add(new RemarkModel(string.Empty));
            }

            RemarkInfoDao remarkInfoDao = new(dbHandler);
            IEnumerable<RemarkInfoDto> dtoList = await remarkInfoDao.FindByItemIdAsync((int)itemId);
            rmList.AddRange(dtoList.Select(static dto => new RemarkModel(dto.Remark) {
                UsedCount = dto.Count,
                UsedTime = dto.UsedTime
            }));

            return rmList;
        }

        /// <summary>
        /// グループ種別を取得する
        /// </summary>
        /// <param name="actionId">帳簿項目ID</param>
        /// <returns>グループ種別</returns>
        public async Task<GroupKind> LoadGroupKind(ActionIdObj actionId)
        {
            using FuncLog funcLog = new(new { actionId });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            GroupInfoDao groupInfoDao = new(dbHandler);
            var dto = await groupInfoDao.FindByActionId((int)actionId);
            int groupKind = dto.GroupKind ?? -1;

            return (GroupKind)groupKind;
        }

        /// <summary>
        /// 一致フラグを保存する
        /// </summary>
        /// <param name="actionId">帳簿項目ID</param>
        /// <param name="isMatch">一致フラグ</param>
        /// <returns></returns>
        public async Task SaveIsMatchAsync(ActionIdObj actionId, bool isMatch)
        {
            using FuncLog funcLog = new(new { actionId, isMatch });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            HstActionDao hstActionDao = new(dbHandler);
            _ = await hstActionDao.UpdateIsMatchByIdAsync((int)actionId, isMatch ? 1 : 0);
        }

        /// <summary>
        /// 全データの期間を取得する
        /// </summary>
        /// <returns>期間</returns>
        public async Task<PeriodObj<DateTime>> LoadPeriodOfAll()
        {
            using FuncLog funcLog = new();
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            PeriodInfoDao periodInfoDao = new(dbHandler);
            var dto = await periodInfoDao.Find();
            PeriodObj<DateTime> result = new(dto.FirstTime, dto.LastTime);

            return result;
        }
    }
}

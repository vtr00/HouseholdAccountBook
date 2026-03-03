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
        /// <param name="initialName">1番目に追加する項目の名称(空文字の場合は追加しない)</param>
        /// <param name="period">期間</param>
        /// <returns>帳簿Modelリスト</returns>
        public async Task<List<BookModel>> LoadBookListAsync(string initialName = "", PeriodObj<DateOnly> period = null)
        {
            using FuncLog funcLog = new(new { initialName, period });

            List<BookModel> bmList = [];

            // 1番目に表示する項目を追加する
            if (initialName != string.Empty) {
                bmList.Add(new(null, initialName));
            }

            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                MstBookDao mstBookDao = new(dbHandler);
                var dtoList = await mstBookDao.FindAllAsync();
                foreach (var dto in dtoList) {
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
            }

            return bmList;
        }

        /// <summary>
        /// 分類Modelリストを取得する
        /// </summary>
        /// <param name="bookId">絞り込み対象の帳簿ID</param>
        /// <param name="balanceKind">絞り込み対象の収支種別</param>
        /// <returns>分類Modelリスト</returns>
        public async Task<List<CategoryModel>> LoadCategoryListAsync(BookIdObj bookId, BalanceKind balanceKind)
        {
            using FuncLog funcLog = new(new { bookId, balanceKind });

            List<CategoryModel> cmList = [
                new CategoryModel(-1, Properties.Resources.ListName_NoSpecification, BalanceKind.Others)
            ];

            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                MstCategoryWithinBookDao mstCategoryWithinBookDao = new(dbHandler);
                IEnumerable<MstCategoryDto> dtoList = await mstCategoryWithinBookDao.FindByBookIdAndBalanceKindAsync((int)bookId, (int)balanceKind);
                foreach (MstCategoryDto dto in dtoList) {
                    cmList.Add(new(dto.CategoryId, dto.CategoryName, (BalanceKind)dto.BalanceKind));
                }
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
        public async Task<List<ItemModel>> LoadItemListAsync(BookIdObj bookId, BalanceKind balanceKind, CategoryIdObj categoryId)
        {
            using FuncLog funcLog = new(new { bookId, balanceKind, categoryId });

            List<ItemModel> imList = [];

            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                CategoryItemInfoDao categoryItemInfoDao = new(dbHandler);
                IEnumerable<CategoryItemInfoDto> dtoList = (int)categoryId == -1
                    ? await categoryItemInfoDao.FindByBookIdAndBalanceKindAsync((int)bookId, (int)balanceKind)
                    : await categoryItemInfoDao.FindByBookIdAndCategoryIdAsync((int)bookId, (int)categoryId);
                foreach (CategoryItemInfoDto dto in dtoList) {
                    ItemModel vm = new(dto.ItemId, dto.ItemName) {
                        CategoryName = (int)categoryId == -1 ? dto.CategoryName : ""
                    };
                    imList.Add(vm);
                }
            }

            return imList;
        }

        /// <summary>
        /// 店舗Modelリストを取得する
        /// </summary>
        /// <param name="itemId">絞り込み対象の項目ID</param>
        /// <returns>店舗Modelリスト</returns>
        public async Task<List<ShopModel>> LoadShopListAsync(ItemIdObj itemId)
        {
            using FuncLog funcLog = new(new { itemId });

            List<ShopModel> smList = [
                new ShopModel(string.Empty)
            ];

            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                ShopInfoDao shopInfoDao = new(dbHandler);
                IEnumerable<ShopInfoDto> dtoList = await shopInfoDao.FindByItemIdAsync((int)itemId);
                foreach (ShopInfoDto dto in dtoList) {
                    ShopModel vm = new(dto.ShopName) {
                        UsedCount = dto.Count,
                        UsedTime = dto.UsedTime
                    };
                    smList.Add(vm);
                }
            }

            return smList;
        }

        /// <summary>
        /// 備考Modelリストを取得する
        /// </summary>
        /// <param name="itemId">絞り込み対象の項目ID</param>
        /// <returns>備考Modelリスト</returns>
        public async Task<List<RemarkModel>> LoadRemarkListAsync(ItemIdObj itemId)
        {
            using FuncLog funcLog = new(new { itemId });

            List<RemarkModel> rmList = [
                new RemarkModel(string.Empty)
            ];

            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                RemarkInfoDao remarkInfoDao = new(dbHandler);
                IEnumerable<RemarkInfoDto> dtoList = await remarkInfoDao.FindByItemIdAsync((int)itemId);
                foreach (RemarkInfoDto dto in dtoList) {
                    RemarkModel vm = new(dto.Remark) {
                        UsedCount = dto.Count,
                        UsedTime = dto.UsedTime
                    };
                    rmList.Add(vm);
                }
            }

            return rmList;
        }
    }
}

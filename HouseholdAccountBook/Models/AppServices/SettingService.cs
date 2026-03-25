using HouseholdAccountBook.Infrastructure.DB.DbDao.Compositions;
using HouseholdAccountBook.Infrastructure.DB.DbDao.DbTable;
using HouseholdAccountBook.Infrastructure.DB.DbDto.DbTable;
using HouseholdAccountBook.Infrastructure.DB.DbDto.Others;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities;
using HouseholdAccountBook.Infrastructure.Utilities.Extensions;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HouseholdAccountBook.ViewModels.UiConstants;

namespace HouseholdAccountBook.Models.AppServices
{
    /// <summary>
    /// 設定サービス
    /// </summary>
    /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
    /// <remarks>主に設定ウィンドウで使用する</remarks>
    public class SettingService(DbHandlerFactory dbHandlerFactory)
    {
        /// <summary>
        /// DBハンドラファクトリ
        /// </summary>
        private readonly DbHandlerFactory mDbHandlerFactory = dbHandlerFactory;

        #region 帳簿設定
        /// <summary>
        /// 帳簿Modelを取得する
        /// </summary>
        /// <param name="bookId">対象の帳簿ID</param>
        /// <returns>帳簿Model</returns>
        public async Task<BookModel> LoadBookAsync(BookIdObj bookId)
        {
            using FuncLog funcLog = new(new { bookId });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            BookInfoDao bookInfoDao = new(dbHandler);
            BookInfoDto dto = await bookInfoDao.FindByBookId((int)bookId);

            MstBookDto.JsonDto jsonObj = dto.JsonCode == null ? null : new(dto.JsonCode);

            BookModel bm = new(bookId, dto.BookName) {
                SortOrder = dto.SortOrder,
                BookKind = (BookKind)dto.BookKind,
                Remark = jsonObj?.Remark ?? string.Empty,
                InitialValue = dto.InitialValue,
                StartDateExists = jsonObj?.StartDate != null,
                EndDateExists = jsonObj?.EndDate != null,
                Period = new(jsonObj?.StartDate?.ToDateOnly() ?? dto.StartDate?.ToDateOnly() ?? DateOnlyExtensions.Today,
                                jsonObj?.EndDate?.ToDateOnly() ?? dto.EndDate?.ToDateOnly() ?? DateOnlyExtensions.Today),
                DebitBookId = dto.DebitBookId ?? BookIdObj.System,
                PayDay = dto.PayDay,
                CsvFolderPath = jsonObj == null ? "" : PathUtil.GetSmartPath(App.GetCurrentDir(), jsonObj.CsvFolderPath),
                TextEncoding = jsonObj?.TextEncoding ?? Encoding.UTF8.CodePage,
                ActDateIndex = jsonObj?.CsvActDateIndex + 1,
                ExpensesIndex = jsonObj?.CsvOutgoIndex + 1,
                ItemNameIndex = jsonObj?.CsvItemNameIndex + 1
            };

            return bm;
        }

        /// <summary>
        /// 帳簿を追加する
        /// </summary>
        /// <returns>帳簿ID</returns>
        public async Task<BookIdObj> AddBookAsync()
        {
            using FuncLog funcLog = new();
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            MstBookDao mstBookDao = new(dbHandler);
            BookIdObj bookId = await mstBookDao.InsertReturningIdAsync(new MstBookDto { });

            return bookId;
        }

        /// <summary>
        /// 帳簿に紐づく帳簿項目が存在しなければ帳簿を削除する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <returns>削除したか</returns>
        public async Task<bool> DeleteBookAsync(BookIdObj bookId)
        {
            using FuncLog funcLog = new(new { bookId });

            bool result = false;
            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                await dbHandler.ExecTransactionAsync(async () => {
                    HstActionDao hstActionDao = new(dbHandler);
                    IEnumerable<HstActionDto> dtoList = await hstActionDao.FindByBookIdAsync((int)bookId);
                    if (dtoList.Any()) {
                        result = false;
                    }
                    else {
                        MstBookDao hstBookDao = new(dbHandler);
                        _ = await hstBookDao.DeleteByIdAsync((int)bookId);
                        result = true;
                    }
                });
            }
            return result;
        }

        /// <summary>
        /// 帳簿のソート順を入れ替える
        /// </summary>
        /// <param name="bookId1">帳簿ID1</param>
        /// <param name="bookId2">帳簿ID2</param>
        /// <returns></returns>
        public async Task SwapBookSortOrderAsync(BookIdObj bookId1, BookIdObj bookId2)
        {
            using FuncLog funcLog = new(new { bookId1, bookId2 });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            MstBookDao mstBookDao = new(dbHandler);
            _ = await mstBookDao.SwapSortOrderAsync((int)bookId1, (int)bookId2);
        }

        /// <summary>
        /// 帳簿Modelを保存する
        /// </summary>
        /// <param name="book">帳簿Model</param>
        /// <returns></returns>
        public async Task SaveBookAsync(BookModel book)
        {
            using FuncLog funcLog = new(new { book });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            MstBookDto.JsonDto jsonObj = new() {
                Remark = book.Remark,
                StartDate = book.StartDateExists ? book.Period.Start.ToDateTime(TimeOnly.MinValue) : null,
                EndDate = book.EndDateExists ? book.Period.End.ToDateTime(TimeOnly.MinValue) : null,
                CsvFolderPath = book.CsvFolderPath,
                TextEncoding = book.TextEncoding,
                CsvActDateIndex = book.ActDateIndex - 1,
                CsvOutgoIndex = book.ExpensesIndex - 1,
                CsvItemNameIndex = book.ItemNameIndex - 1
            };
            string jsonCode = jsonObj.ToJson();

            MstBookDao mstBookDao = new(dbHandler);
            _ = await mstBookDao.UpdateSetableAsync(new() {
                BookName = book.Name,
                BookKind = (int)book.BookKind,
                InitialValue = (int)book.InitialValue,
                DebitBookId = book.DebitBookId == BookIdObj.System ? null : (int)book.DebitBookId,
                PayDay = book.PayDay,
                JsonCode = jsonCode,
                BookId = (int)book.Id
            });
        }

        /// <summary>
        /// 関連Model(帳簿主体)を取得する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <returns>関連Model</returns>
        public async Task<IEnumerable<RelationModel>> LoadRelationListAsync(BookIdObj bookId)
        {
            using FuncLog funcLog = new(new { bookId });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            List<RelationModel> rvmList = [];
            ItemRelFromBookInfoDao itemRelFromBookInfoDao = new(dbHandler);
            IEnumerable<ItemRelFromBookInfoDto> dtoList = await itemRelFromBookInfoDao.FindByBookIdAsync((int)bookId);

            foreach (ItemRelFromBookInfoDto dto in dtoList) {
                RelationModel rvm = new() {
                    Id = dto.ItemId,
                    Name = $"{BalanceKindStr[(BalanceKind)dto.BalanceKind]} > {dto.CategoryName} > {dto.ItemName}",
                    IsRelated = dto.IsRelated
                };
                rvmList.Add(rvm);
            }

            return rvmList;
        }
        #endregion

        #region 項目設定
        /// <summary>
        /// 収支内の分類Modelリストを取得する
        /// </summary>
        /// <param name="kind">終始種別</param>
        /// <returns>分類Modelリスト</returns>
        public async Task<IEnumerable<CategoryModel>> LoadCategoryListAsync(BalanceKind kind)
        {
            using FuncLog funcLog = new(new { kind });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            MstCategoryDao mstCategoryDao = new(dbHandler);
            IEnumerable<MstCategoryDto> cDtoList = await mstCategoryDao.FindByBalanceKindAsync((int)kind);
            IEnumerable<CategoryModel> categoryList = cDtoList.Select(dto => new CategoryModel(dto.CategoryId, dto.CategoryName, kind));

            return categoryList;
        }

        /// <summary>
        /// 分類内の項目Modelリストを取得する
        /// </summary>
        /// <param name="categoryId">分類ID</param>
        /// <returns>項目Modelリスト</returns>
        public async Task<IEnumerable<ItemModel>> LoadItemListAsync(CategoryIdObj categoryId)
        {
            using FuncLog funcLog = new(new { categoryId });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            MstItemDao mstItemDao = new(dbHandler);
            IEnumerable<MstItemDto> iDtoList = await mstItemDao.FindByCategoryIdAsync((int)categoryId);
            IEnumerable<ItemModel> itemList = iDtoList.Select(static dto => new ItemModel(dto.ItemId, dto.ItemName));

            return itemList;
        }

        /// <summary>
        /// 分類を追加する
        /// </summary>
        /// <param name="kind">収支種別</param>
        /// <returns>分類ID</returns>
        public async Task<CategoryIdObj> AddCategoryAsync(BalanceKind kind)
        {
            using FuncLog funcLog = new(new { kind });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            MstCategoryDao dao = new(dbHandler);
            CategoryIdObj categoryId = await dao.InsertReturningIdAsync(new MstCategoryDto { BalanceKind = (int)kind });

            return categoryId;
        }

        /// <summary>
        /// 項目を追加する
        /// </summary>
        /// <param name="categoryId">分類ID</param>
        /// <returns>項目ID</returns>
        public async Task<ItemIdObj> AddItemAsync(CategoryIdObj categoryId)
        {
            using FuncLog funcLog = new(new { categoryId });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            MstItemDao mstItemDao = new(dbHandler);
            ItemIdObj itemId = await mstItemDao.InsertReturningIdAsync(new MstItemDto { CategoryId = (int)categoryId });

            return itemId;
        }

        /// <summary>
        /// 分類に紐づく帳簿項目が存在しなければ分類を削除する
        /// </summary>
        /// <param name="categoryId">分類ID</param>
        /// <returns>削除したか</returns>
        public async Task<bool> DeleteCategoryAsync(CategoryIdObj categoryId)
        {
            using FuncLog funcLog = new(new { categoryId });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            bool result = false;
            await dbHandler.ExecTransactionAsync(async () => {
                HstActionWithHstItemDao hstActionWithHstItemDao = new(dbHandler);
                IEnumerable<HstActionDto> dtoList = await hstActionWithHstItemDao.FindByCategoryIdAsync((int)categoryId);

                if (dtoList.Any()) {
                    result = false;
                }
                else {
                    MstCategoryDao mstCategoryDao = new(dbHandler);
                    _ = await mstCategoryDao.DeleteByIdAsync((int)categoryId);
                    result = true;
                }
            });

            return result;
        }

        /// <summary>
        /// 項目に紐づく帳簿項目が存在しなければ項目を削除する
        /// </summary>
        /// <param name="itemId">項目ID</param>
        /// <returns>削除したか</returns>
        public async Task<bool> DeleteItemAsync(ItemIdObj itemId)
        {
            using FuncLog funcLog = new(new { itemId });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            bool result = false;
            await dbHandler.ExecTransactionAsync(async () => {
                HstActionDao hstActionDao = new(dbHandler);
                IEnumerable<HstActionDto> dtoList = await hstActionDao.FindByItemIdAsync((int)itemId);

                if (dtoList.Any()) {
                    result = false;
                }
                else {
                    MstItemDao mstItemDao = new(dbHandler);
                    _ = await mstItemDao.DeleteByIdAsync((int)itemId);
                    result = true;
                }
            });

            return result;
        }

        /// <summary>
        /// 分類のソート順を入れ替える
        /// </summary>
        /// <param name="categoryId1">分類ID1</param>
        /// <param name="categoryId2">分類ID2</param>
        /// <returns></returns>
        public async Task SwapCategorySortOrderAsync(CategoryIdObj categoryId1, CategoryIdObj categoryId2)
        {
            using FuncLog funcLog = new(new { categoryId1, categoryId2 });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            MstCategoryDao mstCategoryDao = new(dbHandler);
            _ = await mstCategoryDao.SwapSortOrderAsync((int)categoryId1, (int)categoryId2);
        }

        /// <summary>
        /// 項目のソート順を入れ替える
        /// </summary>
        /// <param name="itemId1">項目ID1</param>
        /// <param name="itemId2">項目ID2</param>
        /// <returns></returns>
        public async Task SwapItemSortOrderAsync(ItemIdObj itemId1, ItemIdObj itemId2)
        {
            using FuncLog funcLog = new(new { itemId1, itemId2 });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            MstItemDao mstItemDao = new(dbHandler);
            _ = await mstItemDao.SwapSortOrderAsync((int)itemId1, (int)itemId2);
        }

        /// <summary>
        /// 項目のソート順を分類内で最大となる値に変更する
        /// </summary>
        /// <param name="categoryId">分類ID</param>
        /// <param name="itemId">項目ID</param>
        /// <returns></returns>
        public async Task UpdateItemSortOrderToMaximumAsync(CategoryIdObj categoryId, ItemIdObj itemId)
        {
            using FuncLog funcLog = new(new { categoryId, itemId });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            MstItemDao mstItemDao = new(dbHandler);
            _ = await mstItemDao.UpdateSortOrderToMaximumAsync(categoryId.Id, itemId.Id);
        }

        /// <summary>
        /// 項目のソート順を分類内で最小となる値に変更する
        /// </summary>
        /// <param name="categoryId">分類ID</param>
        /// <param name="itemId">項目ID</param>
        /// <returns></returns>
        public async Task UpdateItemSortOrderToMinimumAsync(CategoryIdObj categoryId, ItemIdObj itemId)
        {
            using FuncLog funcLog = new(new { categoryId, itemId });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            MstItemDao mstItemDao = new(dbHandler);
            _ = await mstItemDao.UpdateSortOrderToMinimumAsync(categoryId.Id, itemId.Id);
        }

        /// <summary>
        /// 分類Modelを取得する
        /// </summary>
        /// <param name="categoryId">分類ID</param>
        /// <returns>分類Model</returns>
        public async Task<CategoryModel> LoadCategoryAsync(CategoryIdObj categoryId)
        {
            using FuncLog funcLog = new(new { categoryId });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            MstCategoryDao mstCategoryDao = new(dbHandler);
            var dto = await mstCategoryDao.FindByIdAsync((int)categoryId);

            CategoryModel category = new(categoryId, dto.CategoryName, (BalanceKind)dto.BalanceKind) {
                SortOrder = dto.SortOrder
            };

            return category;
        }

        /// <summary>
        /// 項目Modelを取得する
        /// </summary>
        /// <param name="itemId">項目ID</param>
        /// <returns>項目Model</returns>
        public async Task<ItemModel> LoadItemAsync(ItemIdObj itemId)
        {
            using FuncLog funcLog = new(new { itemId });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            MstItemDao mstItemDao = new(dbHandler);
            var dto = await mstItemDao.FindByIdAsync((int)itemId);

            ItemModel item = new(itemId, dto.ItemName) {
                SortOrder = dto.SortOrder
            };

            return item;
        }

        /// <summary>
        /// 分類Modelを保存する
        /// </summary>
        /// <param name="category">分類Model</param>
        /// <returns></returns>
        public async Task SaveCategoryAsync(CategoryModel category)
        {
            using FuncLog funcLog = new(new { category });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            MstCategoryDao mstCategoryDao = new(dbHandler);
            _ = await mstCategoryDao.UpdateSetableAsync(new MstCategoryDto { CategoryName = category.Name, CategoryId = (int)category.Id });
        }

        /// <summary>
        /// 項目Modelを保存する
        /// </summary>
        /// <param name="item">項目Model</param>
        /// <returns></returns>
        public async Task SaveItemAsync(ItemModel item)
        {
            using FuncLog funcLog = new(new { item });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            MstItemDao mstItemDao = new(dbHandler);
            _ = await mstItemDao.UpdateSetableAsync(new MstItemDto { ItemName = item.Name, ItemId = (int)item.Id });
        }

        /// <summary>
        /// 関連Model(項目主体)を取得する
        /// </summary>
        /// <param name="itemId">項目ID</param>
        /// <returns>関連Model</returns>
        public async Task<IEnumerable<RelationModel>> LoadRelationListAsync(ItemIdObj itemId)
        {
            using FuncLog funcLog = new(new { itemId });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            BookRelFromItemInfoDao bookRelFromItemInfoDao = new(dbHandler);
            IEnumerable<BookRelFromItemInfoDto> dtoList = await bookRelFromItemInfoDao.FindByItemIdAsync((int)itemId);

            List<RelationModel> rvmList = [];
            foreach (BookRelFromItemInfoDto dto in dtoList) {
                RelationModel rvm = new() {
                    Id = dto.BookId,
                    Name = dto.BookName,
                    IsRelated = dto.IsRelated
                };
                rvmList.Add(rvm);
            }

            return rvmList;
        }

        /// <summary>
        /// 店舗を削除する
        /// </summary>
        /// <param name="shopName">店舗名</param>
        /// <param name="itemId">項目ID</param>
        /// <returns></returns>
        public async Task DeleteShopAsync(string shopName, ItemIdObj itemId)
        {
            using FuncLog funcLog = new(new { shopName, itemId });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            HstShopDao hstShopDao = new(dbHandler);
            _ = await hstShopDao.DeleteAsync(new HstShopDto {
                ShopName = shopName,
                ItemId = (int)itemId
            });
        }

        /// <summary>
        /// 備考を削除する
        /// </summary>
        /// <param name="remark">備考</param>
        /// <param name="itemId">項目ID</param>
        /// <returns></returns>
        public async Task DeleteRemarkAsync(string remark, ItemIdObj itemId)
        {
            using FuncLog funcLog = new(new { remark, itemId });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            HstRemarkDao hstRemarkDao = new(dbHandler);
            _ = await hstRemarkDao.DeleteAsync(new HstRemarkDto {
                Remark = remark,
                ItemId = (int)itemId
            });
        }
        #endregion

        /// <summary>
        /// 帳簿と項目に紐づく帳簿項目が存在しなければ関連付けを変更する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="itemId">項目ID</param>
        /// <param name="isRelated">更新後の関連付け</param>
        /// <returns>変更したか</returns>
        public async Task<bool> SaveBookItemRemationAsync(BookIdObj bookId, ItemIdObj itemId, bool isRelated)
        {
            using FuncLog funcLog = new(new { bookId, itemId, isRelated });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            bool result = false;
            await dbHandler.ExecTransactionAsync(async () => {
                HstActionDao hstActionDao = new(dbHandler);
                IEnumerable<HstActionDto> hstActionDtoList = await hstActionDao.FindByBookIdAndItemIdAsync((int)bookId, (int)itemId);

                if (hstActionDtoList.Any()) {
                    result = false;
                }
                else {
                    RelBookItemDao relBookItemDao = new(dbHandler);
                    _ = await relBookItemDao.UpsertAsync(new RelBookItemDto {
                        BookId = (int)bookId,
                        ItemId = (int)itemId,
                        DelFlg = isRelated ? 0 : 1
                    });
                    result = true;
                }
            });

            return result;
        }
    }
}

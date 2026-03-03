using HouseholdAccountBook.Infrastructure;
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HouseholdAccountBook.ViewModels.UiConstants;

namespace HouseholdAccountBook.Models.AppServices
{
    public class SettingService(DbHandlerFactory dbHandlerFactory)
    {
        /// <summary>
        /// DBハンドラファクトリ
        /// </summary>
        private readonly DbHandlerFactory mDbHandlerFactory = dbHandlerFactory;

        /// <summary>
        /// 帳簿Modelを取得する
        /// </summary>
        /// <param name="bookId">対象の帳簿ID</param>
        /// <returns>帳簿Model</returns>
        public async Task<BookModel> LoadBookAsync(BookIdObj bookId)
        {
            using FuncLog funcLog = new(new { bookId });

            BookModel bm = null;
            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                BookInfoDao bookInfoDao = new(dbHandler);
                var dto = await bookInfoDao.FindByBookId((int)bookId);

                MstBookDto.JsonDto jsonObj = dto.JsonCode == null ? null : new(dto.JsonCode);

                bm = new(bookId, dto.BookName) {
                    SortOrder = dto.SortOrder,
                    BookKind = (BookKind)dto.BookKind,
                    Remark = jsonObj?.Remark ?? string.Empty,
                    InitialValue = dto.InitialValue,
                    StartDateExists = jsonObj?.StartDate != null,
                    EndDateExists = jsonObj?.EndDate != null,
                    Period = new(jsonObj?.StartDate?.ToDateOnly() ?? dto.StartDate?.ToDateOnly() ?? DateOnlyExtensions.Today,
                                 jsonObj?.EndDate?.ToDateOnly() ?? dto.EndDate?.ToDateOnly() ?? DateOnlyExtensions.Today),
                    DebitBookId = dto.DebitBookId,
                    PayDay = dto.PayDay,
                    CsvFolderPath = jsonObj == null ? "" : PathUtil.GetSmartPath(App.GetCurrentDir(), jsonObj.CsvFolderPath),
                    TextEncoding = jsonObj?.TextEncoding ?? Encoding.UTF8.CodePage,
                    ActDateIndex = jsonObj?.CsvActDateIndex + 1,
                    ExpensesIndex = jsonObj?.CsvOutgoIndex + 1,
                    ItemNameIndex = jsonObj?.CsvItemNameIndex + 1
                };
            }

            return bm;
        }

        /// <summary>
        /// 収支内の分類Modelリストを取得する
        /// </summary>
        /// <param name="kind">終始種別</param>
        /// <returns>分類Modelリスト</returns>
        public async Task<List<CategoryModel>> LoadCategoryListAsync(BalanceKind kind)
        {
            List<CategoryModel> categoryList = null;
            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                MstCategoryDao mstCategoryDao = new(dbHandler);
                IEnumerable<MstCategoryDto> cDtoList = await mstCategoryDao.FindByBalanceKindAsync((int)kind);
                categoryList = [.. cDtoList.Select(dto => new CategoryModel(dto.CategoryId, dto.CategoryName, kind))];
            }
            return categoryList;
        }

        /// <summary>
        /// 分類内の項目Modelリストを取得する
        /// </summary>
        /// <param name="categoryId">分類ID</param>
        /// <returns>項目Modelリスト</returns>
        public async Task<List<ItemModel>> LoadItemListAsync(CategoryIdObj categoryId)
        {
            List<ItemModel> itemList = null;
            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                MstItemDao mstItemDao = new(dbHandler);
                IEnumerable<MstItemDto> iDtoList = await mstItemDao.FindByCategoryIdAsync((int)categoryId);
                itemList = [.. iDtoList.Select(static dto => new ItemModel(dto.ItemId, dto.ItemName))];
            }
            return itemList;
        }

        /// <summary>
        /// 分類Modelを取得する
        /// </summary>
        /// <param name="categoryId">分類ID</param>
        /// <returns>分類Model</returns>
        public async Task<CategoryModel> LoadCategoryAsync(CategoryIdObj categoryId)
        {
            CategoryModel category = null;
            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                MstCategoryDao mstCategoryDao = new(dbHandler);
                var dto = await mstCategoryDao.FindByIdAsync((int)categoryId);

                category = new(categoryId, dto.CategoryName, (BalanceKind)dto.BalanceKind) {
                    SortOrder = dto.SortOrder
                };
            }
            return category;
        }

        /// <summary>
        /// 項目Modelを取得する
        /// </summary>
        /// <param name="itemId">項目ID</param>
        /// <returns>項目Model</returns>
        public async Task<ItemModel> LoadItemAsync(ItemIdObj itemId)
        {
            ItemModel item = null;
            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                MstItemDao mstItemDao = new(dbHandler);
                var dto = await mstItemDao.FindByIdAsync((int)itemId);

                item = new(itemId, dto.ItemName) {
                    SortOrder = dto.SortOrder
                };
            }
            return item;
        }

        /// <summary>
        /// 関連Model(帳簿主体)を取得する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <returns>関連Model</returns>
        public async Task<List<RelationModel>> LoadRelationAsync(BookIdObj bookId)
        {
            using FuncLog funcLog = new(new { bookId });

            List<RelationModel> rvmList = [];
            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                ItemRelFromBookInfoDao itemRelFromBookInfoDao = new(dbHandler);
                var dtoList = await itemRelFromBookInfoDao.FindByBookIdAsync((int)bookId);

                foreach (ItemRelFromBookInfoDto dto in dtoList) {
                    RelationModel rvm = new() {
                        Id = dto.ItemId,
                        Name = $"{BalanceKindStr[(BalanceKind)dto.BalanceKind]} > {dto.CategoryName} > {dto.ItemName}",
                        IsRelated = dto.IsRelated
                    };
                    rvmList.Add(rvm);
                }
            }
            return rvmList;
        }

        /// <summary>
        /// 関連Model(項目主体)を取得する
        /// </summary>
        /// <param name="itemId">項目ID</param>
        /// <returns>関連Model</returns>
        public async Task<List<RelationModel>> LoadRelationListAsync(ItemIdObj itemId)
        {
            using FuncLog funcLog = new(new { itemId });

            List<RelationModel> rvmList = [];
            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                BookRelFromItemInfoDao bookRelFromItemInfoDao = new(dbHandler);
                var dtoList = await bookRelFromItemInfoDao.FindByItemIdAsync((int)itemId);

                foreach (BookRelFromItemInfoDto dto in dtoList) {
                    RelationModel rvm = new() {
                        Id = dto.BookId,
                        Name = dto.BookName,
                        IsRelated = dto.IsRelated
                    };
                    rvmList.Add(rvm);
                }
            }
            return rvmList;
        }
    }
}

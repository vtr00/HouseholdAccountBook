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
using HouseholdAccountBook.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HouseholdAccountBook.Infrastructure.EncodingUtil;
using static HouseholdAccountBook.ViewModels.UiConstants;

namespace HouseholdAccountBook.Models.AppServices
{
    /// <summary>
    /// ViewModelサービス
    /// </summary>
    public class AppService(DbHandlerFactory dbHandlerFactory)
    {
        private readonly DbHandlerFactory mDbHandlerFactory = dbHandlerFactory;

        #region RegistrationWindow
        /// <summary>
        /// 帳簿リストを取得する
        /// </summary>
        /// <param name="initialName">1番目に追加する項目の名称(空文字の場合は追加しない)</param>
        /// <param name="start">開始日</param>
        /// <param name="end">終了日</param>
        /// <returns>帳簿リスト</returns>
        public async Task<List<BookModel>> LoadBookListAsync(string initialName = "", PeriodObj<DateOnly> period = null)
        {
            using FuncLog funcLog = new(new { initialName, period });

            List<BookModel> bookList = [];

            // 1番目に表示する項目を追加する
            if (initialName != string.Empty) {
                bookList.Add(new(null, initialName));
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
                        bookList.Add(vm);
                    }
                }
            }

            return bookList;
        }

        /// <summary>
        /// 分類リストを取得する
        /// </summary>
        /// <param name="bookId">絞り込み対象の帳簿ID</param>
        /// <param name="balanceKind">絞り込み対象の収支種別</param>
        /// <returns>分類リスト</returns>
        public async Task<List<CategoryModel>> LoadCategoryListAsync(BookIdObj bookId, BalanceKind balanceKind)
        {
            using FuncLog funcLog = new(new { bookId, balanceKind });

            List<CategoryModel> categoryList = [
                new CategoryModel(-1, Properties.Resources.ListName_NoSpecification, BalanceKind.Others)
            ];

            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                MstCategoryWithinBookDao mstCategoryWithinBookDao = new(dbHandler);
                IEnumerable<MstCategoryDto> dtoList = await mstCategoryWithinBookDao.FindByBookIdAndBalanceKindAsync((int)bookId, (int)balanceKind);
                foreach (MstCategoryDto dto in dtoList) {
                    categoryList.Add(new(dto.CategoryId, dto.CategoryName, (BalanceKind)dto.BalanceKind));
                }
            }

            return categoryList;
        }

        /// <summary>
        /// 項目リストを取得する
        /// </summary>
        /// <param name="bookId">絞り込み対象の帳簿ID</param>
        /// <param name="balanceKind">絞り込み対象の収支種別</param>
        /// <param name="categoryId">絞り込み対象の分類ID</param>
        /// <returns>項目リスト</returns>
        public async Task<List<ItemModel>> LoadItemListAsync(BookIdObj bookId, BalanceKind balanceKind, CategoryIdObj categoryId)
        {
            using FuncLog funcLog = new(new { bookId, balanceKind, categoryId });

            List<ItemModel> itemList = [];

            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                CategoryItemInfoDao categoryItemInfoDao = new(dbHandler);
                IEnumerable<CategoryItemInfoDto> dtoList = (int)categoryId == -1
                    ? await categoryItemInfoDao.FindByBookIdAndBalanceKindAsync((int)bookId, (int)balanceKind)
                    : await categoryItemInfoDao.FindByBookIdAndCategoryIdAsync((int)bookId, (int)categoryId);
                foreach (CategoryItemInfoDto dto in dtoList) {
                    ItemModel vm = new(dto.ItemId, dto.ItemName) {
                        CategoryName = (int)categoryId == -1 ? dto.CategoryName : ""
                    };
                    itemList.Add(vm);
                }
            }

            return itemList;
        }

        /// <summary>
        /// 店舗リストを取得する
        /// </summary>
        /// <param name="itemId">絞り込み対象の項目ID</param>
        /// <returns>店舗リスト</returns>
        public async Task<List<ShopModel>> LoadShopListAsync(ItemIdObj itemId)
        {
            using FuncLog funcLog = new(new { itemId });

            List<ShopModel> shopList = [
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
                    shopList.Add(vm);
                }
            }

            return shopList;
        }

        /// <summary>
        /// 店舗VMリストを取得する
        /// </summary>
        /// <param name="dbHandler">DBハンドラ</param>
        /// <param name="itemId">項目ID</param>
        /// <returns>店舗VMリスト</returns>
        private static async Task<List<ShopModel>> LoadShopListAsync(DbHandlerBase dbHandler, ItemIdObj itemId)
        {
            using FuncLog funcLog = new(new { itemId });

            List<ShopModel> svmList = [];
            ShopInfoDao shopInfoDao = new(dbHandler);
            var dtoList = await shopInfoDao.FindByItemIdAsync((int)itemId);

            foreach (ShopInfoDto dto in dtoList) {
                ShopModel svm = new(dto.ShopName) {
                    UsedCount = dto.Count,
                    UsedTime = dto.UsedTime
                };
                svmList.Add(svm);
            }
            return svmList;
        }

        /// <summary>
        /// 備考リストを取得する
        /// </summary>
        /// <param name="itemId">絞り込み対象の項目ID</param>
        /// <returns>備考リスト</returns>
        public async Task<List<RemarkModel>> LoadRemarkListAsync(ItemIdObj itemId)
        {
            using FuncLog funcLog = new(new { itemId });

            List<RemarkModel> remarkList = [
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
                    remarkList.Add(vm);
                }
            }

            return remarkList;
        }

        /// <summary>
        /// 備考VMリストを取得する
        /// </summary>
        /// <param name="dbHandler">DBハンドラ</param>
        /// <param name="itemId">項目ID</param>
        /// <returns>備考VMリスト</returns>
        private static async Task<List<RemarkModel>> LoadRemarkListAsync(DbHandlerBase dbHandler, ItemIdObj itemId)
        {
            using FuncLog funcLog = new(new { itemId });

            List<RemarkModel> rvmList = [];
            RemarkInfoDao remarkInfoDao = new(dbHandler);
            var dtoList = await remarkInfoDao.FindByItemIdAsync((int)itemId);

            foreach (RemarkInfoDto dto in dtoList) {
                RemarkModel rvm = new(dto.Remark) {
                    UsedCount = dto.Count,
                    UsedTime = dto.UsedTime
                };
                rvmList.Add(rvm);
            }
            return rvmList;
        }
        #endregion

        #region MainWindow
        /// <summary>
        /// 繰越残高を取得する
        /// </summary>
        /// <param name="targetBookId">帳簿ID</param>
        /// <param name="startDate">開始日付</param>
        /// <returns>繰越残高</returns>
        public async Task<decimal> LoadEndingBalance(BookIdObj targetBookId, DateOnly startDate)
        {
            decimal balance;
            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                EndingBalanceInfoDao endingBalanceInfoDao = new(dbHandler);
                EndingBalanceInfoDto dto = targetBookId == null
                    ? await endingBalanceInfoDao.Find(startDate) // 全帳簿の繰越残高
                    : await endingBalanceInfoDao.FindByBookId(targetBookId.Value, startDate); // 各帳簿の繰越残高
                balance = dto.EndingBalance;
            }
            return balance;
        }

        /// <summary>
        /// 月内帳簿項目VMリストを取得する(帳簿タブ)
        /// </summary>
        /// <param name="targetBookId">帳簿ID</param>
        /// <param name="includedTime">月内の時刻</param>
        /// <returns>帳簿項目VMリスト</returns>
        public async Task<List<ActionWithBalanceModel>> LoadActionListAsync(BookIdObj targetBookId, DateOnly includedTime)
        {
            using FuncLog funcLog = new(new { targetBookId, includedTime });

            DateOnly startTime = includedTime.GetFirstDateOfMonth();
            DateOnly endTime = startTime.GetLastDateOfMonth();
            return await this.LoadActionListAsync(targetBookId, (PeriodObj<DateOnly>)new (startTime, endTime));
        }

        /// <summary>
        /// 期間内帳簿項目VMリストを取得する(帳簿タブ)
        /// </summary>
        /// <param name="targetBookId">帳簿ID</param>
        /// <param name="period">期間</param>
        /// <returns>帳簿項目VMリスト</returns>
        public async Task<List<ActionWithBalanceModel>> LoadActionListAsync(BookIdObj targetBookId, PeriodObj<DateOnly> period)
        {
            using FuncLog funcLog = new(new { targetBookId, period });

            List<ActionWithBalanceModel> amList = [];
            decimal balance = await this.LoadEndingBalance(targetBookId, period.Start);

            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                    // 繰越残高を追加
                    {
                    ActionWithBalanceModel am = new() {
                        Action = new() {
                            Book = new(-1, string.Empty),
                            Category = new(-1, string.Empty, BalanceKind.Others),
                            Item = new(-1, Properties.Resources.ListName_CarryForward),
                            Base = new(-1, period.Start.ToDateTime(TimeOnly.MinValue), 0),
                            Shop = null,
                            Remark = null,
                            IsMatch = false
                        },
                        Balance = balance
                    };
                    amList.Add(am);
                }

                ActionInfoDao actionInfoDao = new(dbHandler);
                IEnumerable<ActionInfoDto> dtoList = targetBookId == null
                    ? await actionInfoDao.FindAllWithinTerm(period.Start, period.End) // 全帳簿項目
                    : await actionInfoDao.FindByBookIdWithinTerm(targetBookId.Value, period.Start, period.End); // 各帳簿項目

                foreach (ActionInfoDto aDto in dtoList) {
                    balance += aDto.ActValue;

                    ActionWithBalanceModel am = new() {
                        Action = new() {
                            GroupId = aDto.GroupId,
                            Book = new(aDto.BookId, aDto.BookName),
                            Category = new(aDto.CategoryId, aDto.CategoryName, aDto.ActValue < 0 ? BalanceKind.Expenses : BalanceKind.Income),
                            Item = new(aDto.ItemId, aDto.ItemName),
                            Base = new(aDto.ActionId, aDto.ActTime, aDto.ActValue),
                            Shop = new(aDto.ShopName),
                            Remark = new(aDto.Remark),
                            IsMatch = aDto.IsMatch == 1
                        },
                        Balance = balance
                    };
                    amList.Add(am);
                }
            }

            return amList;
        }

        /// <summary>
        /// 月内概要VMリストを取得する(帳簿タブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="includedTime">月内の時間</param>
        /// <returns>概要VMリスト</returns>
        public async Task<List<SummaryModel>> LoadSummaryListAsync(BookIdObj bookId, DateOnly includedTime)
        {
            using FuncLog funcLog = new(new { bookId, includedTime });

            DateOnly startTime = includedTime.GetFirstDateOfMonth();
            DateOnly endTime = startTime.GetLastDateOfMonth();
            return await this.LoadSummaryListAsync(bookId, (PeriodObj<DateOnly>)new(startTime, endTime));
        }

        /// <summary>
        /// 期間内概要VMリストを取得する(帳簿タブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="startTime">開始時刻</param>
        /// <param name="endTime">終了時刻</param>
        /// <returns>概要VMリスト</returns>
        public async Task<List<SummaryModel>> LoadSummaryListAsync(BookIdObj bookId, PeriodObj<DateOnly> period)
        {
            using FuncLog funcLog = new(new { bookId, period });

            List<SummaryModel> smList = [];
            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                SummaryInfoDao summaryInfoDao = new(dbHandler);
                IEnumerable<SummaryInfoDto> dtoList = bookId == null
                    ? await summaryInfoDao.FindAllWithinPeriod(period.Start, period.End)
                    : await summaryInfoDao.FindByBookIdWithinPeriod(bookId.Value, period.Start, period.End);

                foreach (SummaryInfoDto dto in dtoList) {
                    smList.Add(new() {
                        Category = new(dto.CategoryId, dto.CategoryName, (BalanceKind)dto.BalanceKind),
                        Item = new(dto.ItemId, dto.ItemName),
                        Total = dto.Total
                    });
                }
            }

            // 差引損益
            decimal total = smList.Sum(obj => obj.Total);
            // 収入/支出
            List<SummaryModel> totalAsBalanceKindList = [];
            // 分類小計
            List<SummaryModel> totalAsCategoryList = [];

            // 収支別に計算する
            foreach (var g1 in smList.GroupBy(obj => obj.Category.BalanceKind)) {
                // 収入/支出の小計を計算する
                totalAsBalanceKindList.Add(new() {
                    Category = new(-1, string.Empty, g1.Key),
                    Total = g1.Sum(obj => obj.Total)
                });
                // 分類別の小計を計算する
                foreach (var g2 in g1.GroupBy(obj => (int)obj.Category.Id)) {
                    totalAsCategoryList.Add(new() {
                        Category = new(g2.Key, g2.First().Category.Name, g1.Key),
                        Total = g2.Sum(obj => obj.Total)
                    });
                }
            }

            // 差引損益を追加する
            smList.Insert(0, new() {
                OtherName = Properties.Resources.ListName_profitAndLoss,
                Total = total
            });
            // 収入/支出の小計を追加する
            foreach (SummaryModel svm in totalAsBalanceKindList) {
                smList.Insert(smList.IndexOf(smList.First(obj => obj.Category.BalanceKind == svm.Category.BalanceKind)), svm);
            }
            // 分類別の小計を追加する
            foreach (SummaryModel svm in totalAsCategoryList) {
                smList.Insert(smList.IndexOf(smList.First(obj => obj.Category.Id == svm.Category.Id)), svm);
            }

            return smList;
        }
        #endregion

        #region CsvComparisonWindow
        /// <summary>
        /// 帳簿VM(比較用)を取得する
        /// </summary>
        public async Task<ObservableCollection<BookModel>> UpdateBookCompListAsync()
        {
            using FuncLog funcLog = new();

            ObservableCollection<BookModel> bmList = [];

            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                MstBookDao mstBookDao = new(dbHandler);
                var dtoList = await mstBookDao.FindIfJsonCodeExistsAsync();
                foreach (MstBookDto dto in dtoList) {
                    MstBookDto.JsonDto jsonObj = dto.JsonCode == null ? null : new(dto.JsonCode);
                    if (jsonObj is null) { continue; }

                    BookModel vm = new(dto.BookId, dto.BookName) {
                        CsvFolderPath = jsonObj.CsvFolderPath == string.Empty ? null : jsonObj.CsvFolderPath,
                        TextEncoding = jsonObj.TextEncoding,
                        ActDateIndex = jsonObj.CsvActDateIndex + 1,
                        ExpensesIndex = jsonObj.CsvOutgoIndex + 1,
                        ItemNameIndex = jsonObj.CsvItemNameIndex + 1
                    };
                    if (vm.CsvFolderPath == null || vm.ActDateIndex == null || vm.ExpensesIndex == null || vm.ItemNameIndex == null) { continue; }

                    bmList.Add(vm);
                }
            }

            return bmList;
        }
        #endregion

        #region SettingsWindow
        /// <summary>
        /// 帳簿設定VMを取得する
        /// </summary>
        /// <param name="bookId">表示対象の帳簿ID</param>
        /// <returns>帳簿設定VM</returns>
        public async Task<BookSettingViewModel> LoadBookSettingViewModelAsync(BookIdObj bookId)
        {
            using FuncLog funcLog = new(new { bookId });

            BookSettingViewModel vm = null;

            AppService service = new(this.mDbHandlerFactory);
            List<BookModel> vmList = await service.LoadBookListAsync(Properties.Resources.ListName_None);

            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                // 帳簿一覧を取得する
                BookInfoDao bookInfoDao = new(dbHandler);
                var dto = await bookInfoDao.FindByBookId((int)bookId);

                MstBookDto.JsonDto jsonObj = dto.JsonCode == null ? null : new(dto.JsonCode);

                vm = new BookSettingViewModel() {
                    Id = bookId,
                    SortOrder = dto.SortOrder,
                    InputedName = dto.BookName,
                    SelectedBookKind = (BookKind)dto.BookKind,
                    InputedRemark = jsonObj?.Remark ?? string.Empty,
                    InputedInitialValue = dto.InitialValue,
                    SelectedIfStartDateExists = jsonObj?.StartDate != null,
                    SelectedIfEndDateExists = jsonObj?.EndDate != null,
                    InputedPeriod = new(jsonObj?.StartDate?.ToDateOnly() ?? dto.StartDate?.ToDateOnly() ?? DateOnlyExtensions.Today, 
                                        jsonObj?.EndDate?.ToDateOnly() ?? dto.EndDate?.ToDateOnly() ?? DateOnlyExtensions.Today),
                    DebitBookVMList = new ObservableCollection<BookModel>(vmList.Where(tmpVM => tmpVM.Id != bookId)),
                    InputedPayDay = dto.PayDay,
                    InputedCsvFolderPath = jsonObj is null ? "" : PathUtil.GetSmartPath(App.GetCurrentDir(), jsonObj.CsvFolderPath),
                    TextEncodingList = GetTextEncodingList(),
                    SelectedTextEncoding = jsonObj?.TextEncoding ?? Encoding.UTF8.CodePage,
                    InputedActDateIndex = jsonObj?.CsvActDateIndex + 1,
                    InputedExpensesIndex = jsonObj?.CsvOutgoIndex + 1,
                    InputedItemNameIndex = jsonObj?.CsvItemNameIndex + 1,
                    RelationVMList = [.. await LoadRelationAsync(dbHandler, bookId)]
                };
                vm.SelectedDebitBookVM = vm.DebitBookVMList.FirstOrElementAtOrDefault(tmpVM => (int?)tmpVM.Id == dto.DebitBookId, 0);
            }

            return vm;
        }

        /// <summary>
        /// 項目ツリーVMリストを取得する
        /// </summary>
        /// <returns>階層構造項目VMリスト</returns>
        public async Task<ObservableCollection<ItemTreeViewModel>> LoadItemTreeVMListAsync()
        {
            using FuncLog funcLog = new();

            ObservableCollection<ItemTreeViewModel> vmList = [
                new () {
                    Depth = (int)HierarchicalKind.Balance,
                    Id = (int)BalanceKind.Income,
                    SortOrder = -1,
                    Name = Properties.Resources.BalanceKind_Income,
                    ParentVM = null,
                    ChildrenVMList = []
                },
                new () {
                    Depth = (int)HierarchicalKind.Balance,
                    Id = (int)BalanceKind.Expenses,
                    SortOrder = -1,
                    Name = Properties.Resources.BalanceKind_Expenses,
                    ParentVM = null,
                    ChildrenVMList = []
                }
            ];

            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                foreach (ItemTreeViewModel balanceVM in vmList) {
                    // 分類
                    MstCategoryDao mstCategoryDao = new(dbHandler);
                    var cDtoList = await mstCategoryDao.FindByBalanceKindAsync((int)balanceVM.Id);

                    foreach (MstCategoryDto dto in cDtoList) {
                        balanceVM.ChildrenVMList.Add(new ItemTreeViewModel() {
                            Depth = (int)HierarchicalKind.Category,
                            Id = dto.CategoryId,
                            SortOrder = dto.SortOrder,
                            Name = dto.CategoryName,
                            ParentVM = balanceVM,
                            ChildrenVMList = []
                        });
                    }

                    // 項目
                    MstItemDao mstItemDao = new(dbHandler);
                    foreach (ItemTreeViewModel categoryVM in balanceVM.ChildrenVMList) {
                        var iDtoList = await mstItemDao.FindByCategoryIdAsync((int)categoryVM.Id);

                        foreach (MstItemDto dto in iDtoList) {
                            categoryVM.ChildrenVMList.Add(new ItemTreeViewModel() {
                                Depth = (int)HierarchicalKind.Item,
                                Id = dto.ItemId,
                                SortOrder = dto.SortOrder,
                                Name = dto.ItemName,
                                ParentVM = categoryVM
                            });
                        }
                    }
                }
            }

            return vmList;
        }

        /// <summary>
        /// 分類/項目設定VMを取得する
        /// </summary>
        /// <param name="kind">表示対象の階層種別</param>
        /// <param name="id">表示対象のID</param>
        /// <returns>分類/項目設定VM</returns>
        public async Task<ItemSettingViewModel> LoadItemSettingVMAsync(HierarchicalKind kind, IdObj id)
        {
            using FuncLog funcLog = new(new { kind, id });

            ItemSettingViewModel vm = null;

            switch (kind) {
                case HierarchicalKind.Balance: {
                    vm = new ItemSettingViewModel() {
                        Kind = HierarchicalKind.Balance,
                        Id = -1,
                        SortOrder = -1,
                        InputedName = string.Empty
                    };
                    break;
                }
                case HierarchicalKind.Category: {
                    // 分類
                    CategoryIdObj tmpId = new((int)id);
                    await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                        MstCategoryDao mstCategoryDao = new(dbHandler);
                        var dto = await mstCategoryDao.FindByIdAsync((int)tmpId);

                        vm = new ItemSettingViewModel() {
                            Kind = HierarchicalKind.Category,
                            Id = id,
                            SortOrder = dto.SortOrder,
                            InputedName = dto.CategoryName
                        };
                    }
                    break;
                }
                case HierarchicalKind.Item: {
                    // 項目
                    ItemIdObj tmpId = new((int)id);
                    await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                        MstItemDao mstItemDao = new(dbHandler);
                        var dto = await mstItemDao.FindByIdAsync((int)tmpId);

                        vm = new ItemSettingViewModel {
                            Kind = HierarchicalKind.Item,
                            Id = id,
                            SortOrder = dto.SortOrder,
                            InputedName = dto.ItemName,
                            RelationVMList = [.. await LoadRelationListAsync(dbHandler, tmpId)],
                            ShopVMList = [.. await LoadShopListAsync(dbHandler, tmpId)],
                            RemarkVMList = [.. await LoadRemarkListAsync(dbHandler, tmpId)]
                        };
                    }
                    break;
                }
            }

            return vm;
        }

        /// <summary>
        /// 関連VMリスト(帳簿主体)を取得する
        /// </summary>
        /// <param name="dbHandler">DBハンドラ</param>
        /// <param name="bookId">帳簿ID</param>
        /// <returns>関連VMリスト</returns>
        private static async Task<List<RelationModel>> LoadRelationAsync(DbHandlerBase dbHandler, BookIdObj bookId)
        {
            using FuncLog funcLog = new(new { bookId });

            ItemRelFromBookInfoDao itemRelFromBookInfoDao = new(dbHandler);
            var dtoList = await itemRelFromBookInfoDao.FindByBookIdAsync((int)bookId);

            List<RelationModel> rvmList = [];
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

        /// <summary>
        /// 関連VMリスト(項目主体)を取得する
        /// </summary>
        /// <param name="dbHandler">DBハンドラ</param>
        /// <param name="itemId">項目ID</param>
        /// <returns>関連VMリスト</returns>
        private static async Task<List<RelationModel>> LoadRelationListAsync(DbHandlerBase dbHandler, ItemIdObj itemId)
        {
            using FuncLog funcLog = new(new { itemId });

            BookRelFromItemInfoDao bookRelFromItemInfoDao = new(dbHandler);
            var dtoList = await bookRelFromItemInfoDao.FindByItemIdAsync((int)itemId);

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
        #endregion
    }
}

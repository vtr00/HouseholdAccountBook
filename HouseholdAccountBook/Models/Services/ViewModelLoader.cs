using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Models.Dao.Compositions;
using HouseholdAccountBook.Models.DbHandler;
using HouseholdAccountBook.Models.DbHandler.Abstract;
using HouseholdAccountBook.Models.Dto.Others;
using HouseholdAccountBook.Models.Logger;
using HouseholdAccountBook.ViewModels.Component;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static HouseholdAccountBook.Views.UiConstants;

namespace HouseholdAccountBook.Models.Services
{
    /// <summary>
    /// ViewModel読込サービス
    /// </summary>
    public class ViewModelLoader(DbHandlerFactory dbHandlerFactory)
    {
        private DbHandlerFactory DbHandlerFactory { get; } = dbHandlerFactory;

        #region MainWindow
        /// <summary>
        /// 月内帳簿項目VMリストを取得する(帳簿タブ)
        /// </summary>
        /// <param name="targetBookId">帳簿ID</param>
        /// <param name="includedTime">月内の時刻</param>
        /// <returns>帳簿項目VMリスト</returns>
        public async Task<ObservableCollection<ActionViewModel>> LoadActionViewModelListWithinMonthAsync(int? targetBookId, DateTime includedTime)
        {
            Log.Info($"targetBookId:{targetBookId}, includedTime:{includedTime:yyyy-MM-dd}");

            DateTime startTime = includedTime.GetFirstDateOfMonth();
            DateTime endTime = startTime.GetLastDateOfMonth();
            return await this.LoadActionViewModelListAsync(targetBookId, startTime, endTime);
        }

        /// <summary>
        /// 期間内帳簿項目VMリストを取得する(帳簿タブ)
        /// </summary>
        /// <param name="targetBookId">帳簿ID</param>
        /// <param name="startTime">開始時刻</param>
        /// <param name="endTime">終了時刻</param>
        /// <returns>帳簿項目VMリスト</returns>
        public async Task<ObservableCollection<ActionViewModel>> LoadActionViewModelListAsync(int? targetBookId, DateTime startTime, DateTime endTime)
        {
            Log.Info($"targetBookId:{targetBookId} startTime:{startTime:yyyy-MM-dd} endTime:{endTime:yyyy-MM-dd}");

            ObservableCollection<ActionViewModel> actionVMList = [];
            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                EndingBalanceInfoDao endingBalanceInfoDao = new(dbHandler);
                EndingBalanceInfoDto dto = targetBookId == null
                    ? await endingBalanceInfoDao.Find(startTime) // 全帳簿の繰越残高
                    : await endingBalanceInfoDao.FindByBookId(targetBookId.Value, startTime); // 各帳簿の繰越残高

                // 繰越残高を追加
                int balance = dto.EndingBalance;
                {
                    ActionViewModel avm = new() {
                        ActionId = -1,
                        ActTime = startTime,
                        BookId = -1,
                        CategoryId = -1,
                        ItemId = -1,
                        BookName = "",
                        CategoryName = "",
                        ItemName = Properties.Resources.ListName_CarryForward,
                        BalanceKind = BalanceKind.Others,
                        Income = null,
                        Expenses = null,
                        Balance = balance,
                        ShopName = null,
                        GroupId = null,
                        Remark = null,
                        IsMatch = false
                    };
                    actionVMList.Add(avm);
                }

                ActionInfoDao actionInfoDao = new(dbHandler);
                IEnumerable<ActionInfoDto> dtoList = targetBookId == null
                    ? await actionInfoDao.FindAllWithinTerm(startTime, endTime) // 全帳簿項目
                    : await actionInfoDao.FindByBookIdWithinTerm(targetBookId.Value, startTime, endTime); // 各帳簿項目

                foreach (ActionInfoDto aDto in dtoList) {
                    balance += aDto.ActValue;

                    ActionViewModel avm = new() {
                        ActionId = aDto.ActionId,
                        ActTime = aDto.ActTime,
                        BookId = aDto.BookId,
                        CategoryId = aDto.CategoryId,
                        ItemId = aDto.ItemId,
                        BookName = aDto.BookName,
                        CategoryName = aDto.CategoryName,
                        ItemName = aDto.ItemName,
                        BalanceKind = aDto.ActValue < 0 ? BalanceKind.Expenses : BalanceKind.Income,
                        Income = aDto.ActValue < 0 ? (int?)null : aDto.ActValue,
                        Expenses = aDto.ActValue < 0 ? -aDto.ActValue : (int?)null,
                        Balance = balance,
                        ShopName = aDto.ShopName,
                        GroupId = aDto.GroupId,
                        Remark = aDto.Remark,
                        IsMatch = aDto.IsMatch == 1
                    };
                    actionVMList.Add(avm);
                }
            }

            return actionVMList;
        }

        /// <summary>
        /// 月内概要VMリストを取得する(帳簿タブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="includedTime">月内の時間</param>
        /// <returns>概要VMリスト</returns>
        public async Task<ObservableCollection<SummaryViewModel>> LoadSummaryViewModelListWithinMonthAsync(int? bookId, DateTime includedTime)
        {
            Log.Info($"bookId:{bookId} includedTime:{includedTime:yyyy-MM-dd}");

            DateTime startTime = includedTime.GetFirstDateOfMonth();
            DateTime endTime = startTime.GetLastDateOfMonth();
            return await this.LoadSummaryViewModelListAsync(bookId, startTime, endTime);
        }

        /// <summary>
        /// 期間内概要VMリストを取得する(帳簿タブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="startTime">開始時刻</param>
        /// <param name="endTime">終了時刻</param>
        /// <returns>概要VMリスト</returns>
        public async Task<ObservableCollection<SummaryViewModel>> LoadSummaryViewModelListAsync(int? bookId, DateTime startTime, DateTime endTime)
        {
            Log.Info($"bookId:{bookId} startTime:{startTime:yyyy-MM-dd} endTime:{endTime:yyyy-MM-dd}");

            ObservableCollection<SummaryViewModel> summaryVMList = [];
            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                SummaryInfoDao summaryInfoDao = new(dbHandler);
                IEnumerable<SummaryInfoDto> dtoList = bookId == null
                    ? await summaryInfoDao.FindAllWithinTerm(startTime, endTime)
                    : await summaryInfoDao.FindByBookIdWithinTerm(bookId.Value, startTime, endTime);

                foreach (SummaryInfoDto dto in dtoList) {
                    int balanceKind = dto.BalanceKind;
                    int categoryId = dto.CategoryId;
                    string categoryName = dto.CategoryName;
                    int itemId = dto.ItemId;
                    string itemName = dto.ItemName;
                    int summary = dto.Total;
                    summaryVMList.Add(new SummaryViewModel() {
                        BalanceKind = dto.BalanceKind,
                        BalanceName = BalanceKindStr[(BalanceKind)dto.BalanceKind],
                        CategoryId = dto.CategoryId,
                        CategoryName = dto.CategoryName,
                        ItemId = dto.ItemId,
                        ItemName = dto.ItemName,
                        Total = dto.Total
                    });
                }
            }

            // 差引損益
            int total = summaryVMList.Sum((obj) => obj.Total);
            // 収入/支出
            List<SummaryViewModel> totalAsBalanceKindList = [];
            // 分類小計
            List<SummaryViewModel> totalAsCategoryList = [];

            // 収支別に計算する
            foreach (var g1 in summaryVMList.GroupBy((obj) => obj.BalanceKind)) {
                // 収入/支出の小計を計算する
                totalAsBalanceKindList.Add(new SummaryViewModel() {
                    BalanceKind = g1.Key,
                    BalanceName = BalanceKindStr[(BalanceKind)g1.Key],
                    Total = g1.Sum((obj) => obj.Total)
                });
                // 分類別の小計を計算する
                foreach (var g2 in g1.GroupBy((obj) => obj.CategoryId)) {
                    totalAsCategoryList.Add(new SummaryViewModel() {
                        BalanceKind = g1.Key,
                        BalanceName = BalanceKindStr[(BalanceKind)g1.Key],
                        CategoryId = g2.Key,
                        CategoryName = g2.First().CategoryName,
                        Total = g2.Sum((obj) => obj.Total)
                    });
                }
            }

            // 差引損益を追加する
            summaryVMList.Insert(0, new SummaryViewModel() {
                OtherName = Properties.Resources.ListName_profitAndLoss,
                Total = total
            });
            // 収入/支出の小計を追加する
            foreach (SummaryViewModel svm in totalAsBalanceKindList) {
                summaryVMList.Insert(summaryVMList.IndexOf(summaryVMList.First(obj => obj.BalanceKind == svm.BalanceKind)), svm);
            }
            // 分類別の小計を追加する
            foreach (SummaryViewModel svm in totalAsCategoryList) {
                summaryVMList.Insert(summaryVMList.IndexOf(summaryVMList.First(obj => obj.CategoryId == svm.CategoryId)), svm);
            }

            return summaryVMList;
        }

        /// <summary>
        /// 月内日別系列VMリストを取得する(日別グラフタブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="includedTime">月内の時間</param>
        /// <returns>月内日別系列VMリスト</returns>
        public async Task<ObservableCollection<SeriesViewModel>> LoadDailySeriesViewModelListWithinMonthAsync(int? bookId, DateTime includedTime)
        {
            Log.Info($"bookId:{bookId} includedTime:{includedTime:yyyy-MM-dd}");

            DateTime startTime = includedTime.GetFirstDateOfMonth();
            DateTime endTime = startTime.GetLastDateOfMonth();

            return await this.LoadDailySeriesViewModelListAsync(bookId, startTime, endTime);
        }

        /// <summary>
        /// 期間内日別系列VMリストを取得する(日別グラフタブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="startTime">開始時刻</param>
        /// <param name="endTime">終了時刻</param>
        /// <returns>日別系列VMリスト</returns>
        public async Task<ObservableCollection<SeriesViewModel>> LoadDailySeriesViewModelListAsync(int? bookId, DateTime startTime, DateTime endTime)
        {
            Log.Info($"bookId:{bookId} startTime:{startTime:yyyy-MM-dd} endTime:{endTime:yyyy-MM-dd}");

            // 開始日までの収支を取得する
            int balance = 0;
            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                EndingBalanceInfoDao endingBalanceInfoDao = new(dbHandler);
                EndingBalanceInfoDto dto = bookId == null
                    ? await endingBalanceInfoDao.Find(startTime) // 全帳簿
                    : await endingBalanceInfoDao.FindByBookId(bookId.Value, startTime); // 各帳簿
                balance = dto.EndingBalance;
            }

            // 系列データ
            ObservableCollection<SeriesViewModel> vmList = [
                new SeriesViewModel() {
                    OtherName = Properties.Resources.ListName_Balance,
                    Values = []
                }
            ];
            int averageCount = 0; // 平均値計算に使用する月数(先月まで)

            // 最初の日の分を取得する
            DateTime tmpStartTime = startTime;
            DateTime tmpEndTime = tmpStartTime.AddDays(1).AddMilliseconds(-1);
            ObservableCollection<SummaryViewModel> summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpStartTime, tmpEndTime);
            balance += summaryVMList[0].Total;
            vmList[0].Values.Add(balance); // 残高

            foreach (SummaryViewModel summaryVM in summaryVMList) {
                int value = summaryVM.Total;
                SeriesViewModel vm = new(summaryVM) {
                    Values = [],
                    Total = value,
                    Average = endTime < DateTime.Now ? value : 0 // 平均値は過去のデータのみで計算する
                };
                vm.Values.Add(value);
                vmList.Add(vm);
            }
            if (endTime < DateTime.Now) {
                ++averageCount;
            }

            // 最初以外の日の分を取得する
            int days = (endTime - startTime.AddDays(-1)).Days;
            for (int i = 1; i < days; ++i) {
                tmpStartTime = tmpStartTime.AddDays(1);
                tmpEndTime = tmpStartTime.AddDays(1).AddMilliseconds(-1);

                summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpStartTime, tmpEndTime);
                balance += summaryVMList[0].Total;
                vmList[0].Values.Add(balance); // 残高
                for (int j = 0; j < summaryVMList.Count; ++j) {
                    int value = summaryVMList[j].Total;

                    vmList[j + 1].Values.Add(value);

                    if (tmpEndTime < DateTime.Now) {
                        vmList[j + 1].Average += value;
                    }
                    vmList[j + 1].Total += value;
                }
                if (tmpEndTime < DateTime.Now) {
                    ++averageCount;
                }
            }

            // 平均値を計算する
            foreach (SeriesViewModel vm in vmList) {
                if (vm.Average != null) {
                    if (averageCount != 0) {
                        vm.Average /= averageCount;
                    }
                    else {
                        vm.Average = 0;
                    }
                }
            }

            return vmList;
        }

        /// <summary>
        /// 年度内月別系列VMリストを取得する(月別一覧/月別グラフタブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="includedTime">年度内の日付</param>
        /// <returns>年度内月別系列VMリスト</returns>
        public async Task<ObservableCollection<SeriesViewModel>> LoadMonthlySeriesViewModelListWithinYearAsync(int? bookId, DateTime includedTime, int startMonth)
        {
            Log.Info($"bookId:{bookId} includedTime:{includedTime:yyyy-MM-dd}");

            DateTime startTime = includedTime.GetFirstDateOfFiscalYear(startMonth);
            DateTime endTime = startTime.GetLastDateOfFiscalYear(startMonth);

            // 開始日までの収支を取得する
            int balance = 0;
            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                EndingBalanceInfoDao endingBalanceInfoDao = new(dbHandler);
                EndingBalanceInfoDto dto = bookId == null
                    ? await endingBalanceInfoDao.Find(startTime) // 全帳簿
                    : await endingBalanceInfoDao.FindByBookId(bookId.Value, startTime); // 各帳簿
                balance = dto.EndingBalance;
            }

            ObservableCollection<SeriesViewModel> vmList = [
                new SeriesViewModel() {
                    OtherName = Properties.Resources.ListName_Balance,
                    Values = []
                }
            ];
            int averageCount = 0; // 平均値計算に使用する月数(先月まで)

            // 最初の月の分を取得する
            DateTime tmpStartTime = startTime;
            DateTime tmpEndTime = tmpStartTime.GetLastDateOfMonth();
            ObservableCollection<SummaryViewModel> summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpStartTime, tmpEndTime);
            balance += summaryVMList[0].Total;
            vmList[0].Values.Add(balance); // 残高
            foreach (SummaryViewModel summaryVM in summaryVMList) {
                int value = summaryVM.Total;
                SeriesViewModel vm = new(summaryVM) {
                    Values = [],
                    Total = value,
                    Average = tmpEndTime < DateTime.Now ? value : 0
                };
                vm.Values.Add(value);
                vmList.Add(vm);
            }
            if (tmpEndTime < DateTime.Now) {
                ++averageCount;
            }

            // 最初以外の月の分を取得する
            int monthes = (endTime.Year * 12) + endTime.Month - ((startTime.Year * 12) + startTime.Month - 1);
            for (int i = 1; i < monthes; ++i) {
                tmpStartTime = tmpStartTime.AddMonths(1);
                tmpEndTime = tmpStartTime.GetLastDateOfMonth();

                summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpStartTime, tmpEndTime);
                balance += summaryVMList[0].Total;
                vmList[0].Values.Add(balance); // 残高
                for (int j = 0; j < summaryVMList.Count; ++j) {
                    int value = summaryVMList[j].Total;

                    vmList[j + 1].Values.Add(value);

                    if (tmpEndTime < DateTime.Now) {
                        vmList[j + 1].Average += value;
                    }
                    vmList[j + 1].Total += value;
                }
                if (tmpEndTime < DateTime.Now) {
                    ++averageCount;
                }
            }

            // 平均値を計算する
            foreach (SeriesViewModel vm in vmList) {
                if (vm.Average != null) {
                    if (averageCount != 0) {
                        vm.Average /= averageCount;
                    }
                    else {
                        vm.Average = 0;
                    }
                }
            }

            return vmList;
        }

        /// <summary>
        /// 10年内年別系列VMリストを取得する(年別一覧/年別グラフタブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <returns>年別系列VMリスト</returns>
        public async Task<ObservableCollection<SeriesViewModel>> LoadYearlySeriesViewModelListWithinDecadeAsync(int? bookId, DateTime startYear, int startMonth)
        {
            Log.Info($"bookId:{bookId}");

            DateTime startTime = startYear;

            // 開始日までの収支を取得する
            int balance = 0;
            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                EndingBalanceInfoDao endingBalanceInfoDao = new(dbHandler);
                EndingBalanceInfoDto dto = bookId == null
                    ? await endingBalanceInfoDao.Find(startTime) // 全帳簿
                    : await endingBalanceInfoDao.FindByBookId(bookId.Value, startTime); // 各帳簿
                balance = dto.EndingBalance;
            }

            ObservableCollection<SeriesViewModel> vmList = [
                new SeriesViewModel() {
                    OtherName = Properties.Resources.ListName_Balance,
                    Values = []
                }
            ];
            int averageCount = 0; // 平均値計算に使用する年数(去年まで)

            // 最初の年の分を取得する
            DateTime tmpStartTime = startTime;
            DateTime tmpEndTime = tmpStartTime.GetLastDateOfFiscalYear(startMonth);
            ObservableCollection<SummaryViewModel> summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpStartTime, tmpEndTime);
            balance += summaryVMList[0].Total;
            vmList[0].Values.Add(balance); // 残高
            foreach (SummaryViewModel summaryVM in summaryVMList) {
                int value = summaryVM.Total;
                SeriesViewModel vm = new(summaryVM) {
                    Values = [],
                    Total = value,
                    Average = tmpEndTime < DateTime.Now ? value : 0
                };
                vm.Values.Add(value);
                vmList.Add(vm);
            }
            if (tmpEndTime < DateTime.Now) {
                ++averageCount;
            }

            // 最初以外の年の分を取得する
            int years = 10;
            for (int i = 1; i < years; ++i) {
                tmpStartTime = tmpStartTime.AddYears(1);
                tmpEndTime = tmpStartTime.GetLastDateOfFiscalYear(startMonth);

                summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpStartTime, tmpEndTime);
                balance += summaryVMList[0].Total;
                vmList[0].Values.Add(balance); // 残高
                for (int j = 0; j < summaryVMList.Count; ++j) {
                    int value = summaryVMList[j].Total;

                    vmList[j + 1].Values.Add(value);

                    if (tmpEndTime < DateTime.Now) {
                        vmList[j + 1].Average += value;
                    }
                    vmList[j + 1].Total += value;
                }
                if (tmpEndTime < DateTime.Now) {
                    ++averageCount;
                }
            }

            // 平均値を計算する
            foreach (SeriesViewModel vm in vmList) {
                if (vm.Average != null) {
                    if (averageCount != 0) {
                        vm.Average /= averageCount;
                    }
                    else {
                        vm.Average = 0;
                    }
                }
            }

            return vmList;
        }
        #endregion
    }
}

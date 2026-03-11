using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities.Extensions;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Component;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace HouseholdAccountBook.ViewModels.Loaders
{
    /// <summary>
    /// 系列VMローダ
    /// </summary>
    /// <param name="appService">アプリサービス</param>
    public class SeriesViewModelLoader(MainService appService)
    {
        /// <summary>
        /// アプリサービス
        /// </summary>
        private readonly MainService mAppService = appService;

        /// <summary>
        /// 月内日別系列VMリストを取得する(日別グラフタブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="includedTime">月内の時間</param>
        /// <returns>月内日別系列VMリスト</returns>
        public async Task<ObservableCollection<SeriesViewModel>> LoadDailySeriesViewModelListWithinMonthAsync(BookIdObj bookId, DateOnly includedTime)
        {
            using FuncLog funcLog = new(new { bookId, includedTime });

            DateOnly startTime = includedTime.GetFirstDateOfMonth();
            DateOnly endTime = startTime.GetLastDateOfMonth();

            return await this.LoadDailySeriesViewModelListAsync(bookId, new(startTime, endTime));
        }

        /// <summary>
        /// 期間内日別系列VMリストを取得する(日別グラフタブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="startTime">開始時刻</param>
        /// <param name="endTime">終了時刻</param>
        /// <returns>日別系列VMリスト</returns>
        public async Task<ObservableCollection<SeriesViewModel>> LoadDailySeriesViewModelListAsync(BookIdObj bookId, PeriodObj<DateOnly> period)
        {
            using FuncLog funcLog = new(new { bookId, period });

            // 開始日までの収支を取得する
            decimal balance = await this.mAppService.LoadEndingBalance(bookId, period.Start);

            // 系列データ
            ObservableCollection<SeriesViewModel> vmList = [
                new SeriesViewModel() {
                    OtherName = Properties.Resources.ListName_Balance,
                    Values = [],
                    Periods = []
                }
            ];
            int averageCount = 0; // 平均値計算に使用する月数(先月まで)

            // 最初の日の分を取得する
            PeriodObj<DateOnly> tmpPeriod;
            {
                DateOnly tmpStartTime = period.Start;
                DateOnly tmpEndTime = tmpStartTime;
                tmpPeriod = new(tmpStartTime, tmpEndTime);
            }
            ObservableCollection<SummaryModel> summaryVMList = [.. await this.mAppService.LoadSummaryListAsync(bookId, tmpPeriod)];
            balance += summaryVMList[0].Total;
            vmList[0].Values.Add(balance); // 残高
            vmList[0].Periods.Add(tmpPeriod);

            foreach (SummaryModel summaryVM in summaryVMList) {
                decimal value = summaryVM.Total;
                SeriesViewModel vm = new(summaryVM) {
                    Values = [value],
                    Periods = [tmpPeriod],
                    Total = value,
                    Average = period.End < DateOnlyExtensions.Today ? value : 0 // 平均値は過去のデータのみで計算する
                };
                vmList.Add(vm);
            }
            if (period.End < DateOnly.FromDateTime(DateTime.Now)) {
                ++averageCount;
            }

            // 最初以外の日の分を取得する
            int days = period.End.DayNumber - period.Start.AddDays(-1).DayNumber;
            for (int i = 1; i < days; ++i) {
                {
                    DateOnly tmpStartTime = tmpPeriod.Start.AddDays(1);
                    DateOnly tmpEndTime = tmpStartTime;
                    tmpPeriod = new(tmpStartTime, tmpEndTime);
                }
                summaryVMList = [.. await this.mAppService.LoadSummaryListAsync(bookId, tmpPeriod)];
                balance += summaryVMList[0].Total;
                vmList[0].Values.Add(balance); // 残高
                vmList[0].Periods.Add(tmpPeriod);
                for (int j = 0; j < summaryVMList.Count; ++j) {
                    decimal value = summaryVMList[j].Total;

                    vmList[j + 1].Values.Add(value);
                    vmList[j + 1].Periods.Add(tmpPeriod);

                    if (tmpPeriod.End.IsPost()) {
                        vmList[j + 1].Average += value;
                    }
                    vmList[j + 1].Total += value;
                }
                if (tmpPeriod.End.IsPost()) {
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
        /// <param name="startMonth">年度開始月</param>
        /// <returns>年度内月別系列VMリスト</returns>
        public async Task<ObservableCollection<SeriesViewModel>> LoadMonthlySeriesViewModelListWithinYearAsync(BookIdObj bookId, DateOnly includedTime, int startMonth)
        {
            using FuncLog funcLog = new(new { bookId, includedTime, startMonth });

            PeriodObj<DateOnly> period;
            {
                DateOnly startTime = includedTime.GetFirstDateOfFiscalYear(startMonth);
                DateOnly endTime = startTime.GetLastDateOfFiscalYear(startMonth);
                period = new(startTime, endTime);
            }

            // 開始日までの収支を取得する
            decimal balance = await this.mAppService.LoadEndingBalance(bookId, period.Start);

            ObservableCollection<SeriesViewModel> vmList = [
                new SeriesViewModel() {
                    OtherName = Properties.Resources.ListName_Balance,
                    Values = [],
                    Periods = []
                }
            ];
            int averageCount = 0; // 平均値計算に使用する月数(先月まで)

            // 最初の月の分を取得する
            PeriodObj<DateOnly> tmpPeriod;
            {
                DateOnly tmpStartTime = period.Start;
                DateOnly tmpEndTime = tmpStartTime.GetLastDateOfMonth();
                tmpPeriod = new(tmpStartTime, tmpEndTime);
            }
            List<SummaryModel> summaryVMList = [.. await this.mAppService.LoadSummaryListAsync(bookId, tmpPeriod)];
            balance += summaryVMList[0].Total;
            vmList[0].Values.Add(balance); // 残高
            vmList[0].Periods.Add(tmpPeriod);
            foreach (SummaryModel summaryVM in summaryVMList) {
                decimal value = summaryVM.Total;
                SeriesViewModel vm = new(summaryVM) {
                    Values = [value],
                    Periods = [tmpPeriod],
                    Total = value,
                    Average = tmpPeriod.End.IsPost() ? value : 0
                };
                vmList.Add(vm);
            }
            if (tmpPeriod.End.IsPost()) {
                ++averageCount;
            }

            // 最初以外の月の分を取得する
            int monthes = (period.End.Year * 12) + period.End.Month - ((period.Start.Year * 12) + period.Start.Month - 1);
            for (int i = 1; i < monthes; ++i) {
                {
                    DateOnly tmpStartTime = tmpPeriod.Start.AddMonths(1);
                    DateOnly tmpEndTime = tmpStartTime.GetLastDateOfMonth();
                    tmpPeriod = new(tmpStartTime, tmpEndTime);
                }
                summaryVMList = [.. await this.mAppService.LoadSummaryListAsync(bookId, tmpPeriod)];
                balance += summaryVMList[0].Total;
                vmList[0].Values.Add(balance); // 残高
                vmList[0].Periods.Add(tmpPeriod);
                for (int j = 0; j < summaryVMList.Count; ++j) {
                    decimal value = summaryVMList[j].Total;

                    vmList[j + 1].Values.Add(value);
                    vmList[j + 1].Periods.Add(tmpPeriod);

                    if (tmpPeriod.End.IsPost()) {
                        vmList[j + 1].Average += value;
                    }
                    vmList[j + 1].Total += value;
                }
                if (tmpPeriod.End.IsPost()) {
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
        public async Task<ObservableCollection<SeriesViewModel>> LoadYearlySeriesViewModelListWithinDecadeAsync(BookIdObj bookId, DateOnly startYear, int startMonth)
        {
            using FuncLog funcLog = new(new { bookId, startYear, startMonth });

            DateOnly startTime = startYear;

            // 開始日までの収支を取得する
            decimal balance = await this.mAppService.LoadEndingBalance(bookId, startTime);

            ObservableCollection<SeriesViewModel> vmList = [
                new SeriesViewModel() {
                    OtherName = Properties.Resources.ListName_Balance,
                    Values = [],
                    Periods = []
                }
            ];
            int averageCount = 0; // 平均値計算に使用する年数(去年まで)

            // 最初の年の分を取得する
            PeriodObj<DateOnly> tmpPeriod;
            {
                DateOnly tmpStartTime = startTime;
                DateOnly tmpEndTime = tmpStartTime.GetLastDateOfFiscalYear(startMonth);
                tmpPeriod = new(tmpStartTime, tmpEndTime);
            }
            ObservableCollection<SummaryModel> summaryVMList = [.. await this.mAppService.LoadSummaryListAsync(bookId, tmpPeriod)];
            balance += summaryVMList[0].Total;
            vmList[0].Values.Add(balance); // 残高
            vmList[0].Periods.Add(tmpPeriod);
            foreach (SummaryModel summaryVM in summaryVMList) {
                decimal value = summaryVM.Total;
                SeriesViewModel vm = new(summaryVM) {
                    Values = [],
                    Periods = [],
                    Total = value,
                    Average = tmpPeriod.End.IsPost() ? value : 0
                };
                vm.Values.Add(value);
                vm.Periods.Add(tmpPeriod);
                vmList.Add(vm);
            }
            if (tmpPeriod.End.IsPost()) {
                ++averageCount;
            }

            // 最初以外の年の分を取得する
            int years = 10;
            for (int i = 1; i < years; ++i) {
                {
                    DateOnly tmpStartTime = tmpPeriod.Start.AddYears(1);
                    DateOnly tmpEndTime = tmpStartTime.GetLastDateOfFiscalYear(startMonth);
                    tmpPeriod = new(tmpStartTime, tmpEndTime);
                }

                summaryVMList = [.. await this.mAppService.LoadSummaryListAsync(bookId, tmpPeriod)];
                balance += summaryVMList[0].Total;
                vmList[0].Values.Add(balance); // 残高
                vmList[0].Periods.Add(tmpPeriod);
                for (int j = 0; j < summaryVMList.Count; ++j) {
                    decimal value = summaryVMList[j].Total;

                    vmList[j + 1].Values.Add(value);
                    vmList[j + 1].Periods.Add(tmpPeriod);

                    if (tmpPeriod.End.IsPost()) {
                        vmList[j + 1].Average += value;
                    }
                    vmList[j + 1].Total += value;
                }
                if (tmpPeriod.End.IsPost()) {
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

    }
}

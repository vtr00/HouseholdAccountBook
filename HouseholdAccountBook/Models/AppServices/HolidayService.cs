using HouseholdAccountBook.Infrastructure;
using HouseholdAccountBook.Infrastructure.CSV;
using HouseholdAccountBook.Infrastructure.Logger;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Models.AppServices
{
    /// <summary>
    /// 祝日サービス
    /// </summary>
    public class HolidayService : SingletonBase<HolidayService>
    {
        /// <summary>
        /// スタティックコンストラクタ
        /// </summary>
        static HolidayService() => Register(static () => new HolidayService());
        /// <summary>
        /// プライベートコンストラクタ
        /// </summary>
        private HolidayService() { }

        /// <summary>
        /// 国民の祝日リスト
        /// </summary>
        private List<DateOnly> mHolidayList = [];

        /// <summary>
        /// 国民の祝日リストを取得する
        /// </summary>
        /// <param name="config">CSV設定</param>
        /// <returns>成功/失敗</returns>
        public async Task<bool> DownloadHolidayListAsync(CSVFileDao.HolidayCSVConfigulation config)
        {
            using FuncLog funcLog = new(new { config });

            this.mHolidayList = [.. await CSVFileDao.DownloadHolidayListAsync(config)];

            return this.mHolidayList != null && this.mHolidayList.Count != 0;
        }

        /// <summary>
        /// 国民の祝日かどうかを取得する
        /// </summary>
        /// <param name="date">対象の日付</param>
        /// <returns>国民の祝日かどうか</returns>
        public bool IsNationalHoliday(DateOnly date)
        {
            bool ans = this.mHolidayList?.Contains(date) ?? false;
            return ans;
        }
    }
}

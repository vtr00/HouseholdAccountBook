﻿using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Models.Dao.Compositions;
using HouseholdAccountBook.Models.DbHandler.Abstract;
using HouseholdAccountBook.ViewModels.Abstract;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels.Windows
{
    /// <summary>
    /// 期間指定ウィンドウVM
    /// </summary>
    public class TermWindowViewModel : WindowViewModelBase
    {
        #region Bindingプロパティ
        /// <summary>
        /// 開始日
        /// </summary>
        #region StartDate
        public DateTime StartDate
        {
            get => this._StartDate;
            set => this.SetProperty(ref this._StartDate, value);
        }
        private DateTime _StartDate = DateTime.Now;
        #endregion

        /// <summary>
        /// 終了日
        /// </summary>
        #region EndDate
        public DateTime EndDate
        {
            get => this._EndDate;
            set => this.SetProperty(ref this._EndDate, value);
        }
        private DateTime _EndDate = DateTime.Now;
        #endregion

        #region コマンド
        /// <summary>
        /// 今月コマンド
        /// </summary>
        public ICommand ThisMonthCommand => new RelayCommand(this.ThisMonthCommand_Executed);
        /// <summary>
        /// 全期間コマンド
        /// </summary>
        public ICommand AllTermCommand => new RelayCommand(this.AllTermCommand_Executed);
        #endregion
        #endregion

        #region コマンドイベントハンドラ
        /// <summary>
        /// 今月コマンド処理
        /// </summary>
        void ThisMonthCommand_Executed()
        {
            this.StartDate = DateTime.Today.GetFirstDateOfMonth();
            this.EndDate = DateTime.Today.GetLastDateOfMonth();
        }

        /// <summary>
        /// 全期間コマンド処理
        /// </summary>
        async void AllTermCommand_Executed()
        {
            var firstLastPair = await this.LoadFirstLastDate();
            this.StartDate = firstLastPair.Item1;
            this.EndDate = firstLastPair.Item2;
        }
        #endregion

        #region ウィンドウ設定プロパティ
        public override Point WindowPointSetting
        {
            get {
                Properties.Settings settings = Properties.Settings.Default;
                return new Point(settings.TermWindow_Left, settings.TermWindow_Top);
            }
            set {
                Properties.Settings settings = Properties.Settings.Default;
                settings.TermWindow_Left = value.X;
                settings.TermWindow_Top = value.Y;
                settings.Save();
            }
        }
        #endregion

        /// <summary>
        /// 帳簿項目の初日/最終日を取得する
        /// </summary>
        /// <returns>初日/最終日のペア</returns>
        private async Task<Tuple<DateTime, DateTime>> LoadFirstLastDate()
        {
            DateTime firstTime = DateTime.Today;
            DateTime lastTime = DateTime.Today;
            await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                TermInfoDao termInfoDao = new(dbHandler);
                var dto = await termInfoDao.Find();
                firstTime = dto.FirstTime;
                lastTime = dto.LastTime;
            }
            return new Tuple<DateTime, DateTime>(firstTime, lastTime);
        }
    }
}

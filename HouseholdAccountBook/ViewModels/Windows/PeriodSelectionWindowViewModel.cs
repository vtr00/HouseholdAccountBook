using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities.Extensions;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.ValueObjects;
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
    public class PeriodSelectionWindowViewModel : WindowViewModelBase
    {
        #region Bindingプロパティ
        /// <summary>
        /// 選択された月
        /// </summary>
        public DateTime SelectedMonth {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    this.SelectedStart = value.GetFirstDateOfMonth().ToDateOnly();
                }
            }
        }
        /// <summary>
        /// 選択された開始日
        /// </summary>
        public DateOnly SelectedStart {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    this.SelectedMonth = value.ToDateTime(TimeOnly.MinValue);
                    // TODO: SelectedStartよりも後になった場合の処理
                }
            }
        }
        /// <summary>
        /// 選択された終了日
        /// </summary>
        public DateOnly SelectedEnd {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    // TODO: SelectedStartよりも前になった場合の処理
                }
            }
        }
        /// <summary>
        /// 選択された期間
        /// </summary>
        public PeriodObj<DateOnly> SelectedPeriod {
            get => new(this.SelectedStart, this.SelectedEnd);
            set {
                this.SelectedStart = value.Start;
                this.SelectedEnd = value.End;
            }
        }

        #region コマンド
        /// <summary>
        /// 今月コマンド
        /// </summary>
        public ICommand ThisMonthCommand => new RelayCommand(this.ThisMonthCommand_Execute);
        /// <summary>
        /// 全期間コマンド
        /// </summary>
        public ICommand AllPeriodCommand => new AsyncRelayCommand(this.AllPeriodCommand_ExecuteAsync);
        #endregion
        #endregion

        #region コマンドイベントハンドラ
        /// <summary>
        /// 今月コマンド処理
        /// </summary>
        void ThisMonthCommand_Execute() => this.SelectedPeriod = new PeriodObj<DateOnly>(DateOnlyExtensions.Today.GetFirstDateOfMonth(), DateOnlyExtensions.Today.GetLastDateOfMonth());

        /// <summary>
        /// 全期間コマンド処理
        /// </summary>
        async Task AllPeriodCommand_ExecuteAsync() => this.SelectedPeriod = await this.LoadFirstLastDate();
        #endregion

        #region ウィンドウ設定プロパティ
        public override Point WindowPointSetting {
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

        public override Task LoadAsync() => throw new NotImplementedException();

        public override void AddEventHandlers()
        {
            // NOP
        }

        /// <summary>
        /// 帳簿項目の初日/最終日を取得する
        /// </summary>
        /// <returns>初日/最終日のペア</returns>
        private async Task<PeriodObj<DateOnly>> LoadFirstLastDate()
        {
            using FuncLog funcLog = new();

            AppCommonService service = new(this.mDbHandlerFactory);
            return (await service.LoadPeriodOfAll()).Convert(DateOnly.FromDateTime);
        }
    }
}

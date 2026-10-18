using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Common;
using HouseholdAccountBook.ViewModels.Component;
using System;
using System.Collections.Generic;

namespace HouseholdAccountBook.ViewModels.Settings
{
    /// <summary>
    /// 帳簿VM(設定用)
    /// </summary>
    public class AccountSettingViewModel : ViewModelBase
    {
        #region プロパティ
        /// <summary>
        /// 帳簿モデル
        /// </summary>
        public AccountModel Account { get; init; }

        /// <summary>
        /// 帳簿ID
        /// </summary>
        public AccountIdObj Id => this.Account.Id;

        /// <summary>
        /// ソート順
        /// </summary>
        public int SortOrder => this.Account.SortOrder;

        #region 一般情報
        /// <summary>
        /// 帳簿名
        /// </summary>
        public InputableViewModel<string> NameVM { get; } = new();
        /// <summary>
        /// 帳簿種別リストVM
        /// </summary>
        public SelectableViewModel<KeyValuePair<AccountKind, string>, AccountKind> AccountKindListVM => field ??= new(static p => p.Key);
        /// <summary>
        /// 備考
        /// </summary>
        public InputableViewModel<string> RemarkVM { get; } = new();

        /// <summary>
        /// アセットリストVM
        /// </summary>
        public SelectableViewModel<AssetModel, AssetIdObj> AssetListVM => field ??= new(static vm => vm?.Id);

        /// <summary>
        /// 初期残高
        /// </summary>
        public InputableViewModel<decimal> InitialValueVM { get; } = new();
        /// <summary>
        /// 初期残高(表示値)
        /// </summary>
        public string InputedInitialValueStr => AssetService.Instance.ToAssetString(this.InitialValueVM.InputedValue, this.AssetListVM.SelectedKey, UnitKind.MainUnit, UnitKind.MainUnit);
        /// <summary>
        /// 初期残高スケール
        /// </summary>
        public int InitialValueScale {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 一般情報未保存状態マネージャ
        /// </summary>
        public DirtyStatusManager CommonDirtyState => field ??= new(this.NameVM, this.AccountKindListVM, this.RemarkVM, this.AssetListVM, this.InitialValueVM);
        #endregion

        #region 期間情報
        /// <summary>
        /// 開始日の有無
        /// </summary>
        public InputableViewModel<bool> StartDateExistsVM { get; } = new();
        /// <summary>
        /// 終了日の有無
        /// </summary>
        public InputableViewModel<bool> EndDateExistsVM { get; } = new();
        /// <summary>
        /// 開始日
        /// </summary>
        public InputableViewModel<DateOnly> PeriodStartVM { get; } = new();
        /// <summary>
        /// 終了日
        /// </summary>
        public InputableViewModel<DateOnly> PeriodEndVM { get; } = new();

        /// <summary>
        /// 期間情報未保存状態マネージャ
        /// </summary>
        public DirtyStatusManager PeriodDirtyState => field ??= new(this.StartDateExistsVM, this.EndDateExistsVM, this.PeriodStartVM, this.PeriodEndVM);
        #endregion

        #region 支払情報
        /// <summary>
        /// 支払の必要があるか
        /// </summary>
        public bool NeedToPay => this.AccountKindListVM.SelectedKey == AccountKind.CreditCard;

        /// <summary>
        /// 支払日
        /// </summary>
        public InputableViewModel<int?> PayDayVM { get; } = new();

        /// <summary>
        /// 支払元帳簿リストVM
        /// </summary>
        public SelectableViewModel<AccountModel, AccountIdObj> DebitAccountListVM => field ??= new(static vm => vm?.Id);

        /// <summary>
        /// 支払情報未保存状態マネージャ
        /// </summary>
        public DirtyStatusManager PayDirtyState => field ??= new(this.PayDayVM, this.DebitAccountListVM);
        #endregion

        #region CSV情報
        /// <summary>
        /// CSVデータがあるか
        /// </summary>
        public bool CsvDataExists => this.AccountKindListVM.SelectedKey != AccountKind.Wallet;

        /// <summary>
        /// CSVフォルダパス
        /// </summary>
        public InputableViewModel<string> CsvFolderPathVM { get; } = new();

        /// <summary>
        /// 文字エンコーディング リストVM
        /// </summary>
        public SelectableViewModel<KeyValuePair<int, string>, int> TextEncodingListVM => field ??= new(static p => p.Key);

        /// <summary>
        /// 日付 位置(1開始)
        /// </summary>
        public InputableViewModel<int?> ActDateIndexVM { get; } = new();
        /// <summary>
        /// 支出 位置(1開始)
        /// </summary>
        public InputableViewModel<int?> ExpensesIndexVM { get; } = new();
        /// <summary>
        /// 項目名 位置(1開始)
        /// </summary>
        public InputableViewModel<int?> ItemNameIndexVM { get; } = new();

        /// <summary>
        /// CSV情報未保存状態マネージャ
        /// </summary>
        public DirtyStatusManager CSVDirtyState => field ??= new(this.CsvFolderPathVM, this.TextEncodingListVM, this.ActDateIndexVM, this.ExpensesIndexVM, this.ItemNameIndexVM);
        #endregion

        /// <summary>
        /// 未保存状態マネージャ
        /// </summary>
        public DirtyStatusManager DirtyState => field ??= new(this.CommonDirtyState, this.PeriodDirtyState, this.PayDirtyState, this.CSVDirtyState);

        /// <summary>
        /// 関連性リストVM
        /// </summary>
        public SelectableViewModel<RelationViewModel, ItemIdObj> RelationListVM => field ??= new(static vm => vm?.Id);
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="account">帳簿Model</param>
        public AccountSettingViewModel(AccountModel account)
        {
            this.Account = account;
            this.NameVM.LoadedValue = account.Name;
            this.RemarkVM.LoadedValue = account.Remark;
            this.InitialValueVM.LoadedValue = account.InitialValue.MainValue;
            this.InitialValueScale = account.InitialValue.Scale;
            this.PayDayVM.LoadedValue = account.PayDay;
            this.StartDateExistsVM.LoadedValue = account.StartDateExists;
            this.EndDateExistsVM.LoadedValue = account.EndDateExists;
            this.PeriodStartVM.LoadedValue = account.Period.Start;
            this.PeriodEndVM.LoadedValue = account.Period.End;
            this.CsvFolderPathVM.LoadedValue = account.CsvFolderPath;
            this.ItemNameIndexVM.LoadedValue = account.ItemNameIndex;
            this.ActDateIndexVM.LoadedValue = account.ActDateIndex;
            this.ExpensesIndexVM.LoadedValue = account.ExpensesIndex;

            this.InitialValueVM.InputValueChanged += (sender, e) => this.RaisePropertyChanged(nameof(this.InputedInitialValueStr));

            this.AccountKindListVM.SelectionChanged += (sender, e) => {
                this.RaisePropertyChanged(nameof(this.NeedToPay));
                this.RaisePropertyChanged(nameof(this.CsvDataExists));
            };
            this.AssetListVM.SelectionChanged += (sender, e) => {
                this.RaisePropertyChanged(nameof(this.InputedInitialValueStr));

                AssetModel asset = AssetService.Instance.GetAssetModel(e.NewValue);
                this.InitialValueScale = asset.Scale;
            };
        }
    }
}

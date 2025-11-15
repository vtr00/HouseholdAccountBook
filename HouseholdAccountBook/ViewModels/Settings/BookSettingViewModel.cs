using HouseholdAccountBook.Enums;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Component;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static HouseholdAccountBook.Views.UiConstants;

namespace HouseholdAccountBook.ViewModels.Settings
{
    /// <summary>
    /// 帳簿VM(設定用)
    /// </summary>
    public class BookSettingViewModel : BindableBase
    {
        #region プロパティ
        /// <summary>
        /// 帳簿ID
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// ソート順
        /// </summary>
        #region SortOrder
        public int SortOrder {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 帳簿名
        /// </summary>
        #region Name
        public string Name {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 帳簿種別辞書
        /// </summary>
        public Dictionary<BookKind, string> BookKindDic { get; } = BookKindStr;
        /// <summary>
        /// 選択された帳簿種別
        /// </summary>
        #region SelectedBookKind
        public BookKind SelectedBookKind {
            get;
            set {
                _ = this.SetProperty(ref field, value);
                this.RaisePropertyChanged(nameof(this.NeedToPay));
                this.RaisePropertyChanged(nameof(this.CsvDataExists));
            }
        } = BookKind.Uncategorized;
        #endregion

        /// <summary>
        /// 初期値
        /// </summary>
        #region InitialValue
        public int InitialValue {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        #region 期間情報
        /// <summary>
        /// 開始日の有無
        /// </summary>
        #region StartDateExists
        public bool StartDateExists {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 開始日
        /// </summary>
        #region StartDate
        public DateTime StartDate {
            get;
            set => this.SetProperty(ref field, value);
        } = DateTime.Now;
        #endregion

        /// <summary>
        /// 終了日の有無
        /// </summary>
        #region EndDateExists
        public bool EndDateExists {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 終了日
        /// </summary>
        #region EndDate
        public DateTime EndDate {
            get;
            set => this.SetProperty(ref field, value);
        } = DateTime.Now;
        #endregion
        #endregion

        #region 支払情報
        /// <summary>
        /// 支払の必要があるか
        /// </summary>
        #region NeedToPay
        public bool NeedToPay => this.SelectedBookKind == BookKind.CreditCard;
        #endregion

        /// <summary>
        /// 支払元帳簿VMリスト
        /// </summary>
        #region DebitBookVMList
        public ObservableCollection<BookViewModel> DebitBookVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 選択された支払元帳簿VM
        /// </summary>
        #region SelectedDebitBookVM
        public BookViewModel SelectedDebitBookVM {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 支払日
        /// </summary>
        #region PayDay
        public int? PayDay {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        #endregion

        #region CSV情報
        /// <summary>
        /// CSVデータがあるか
        /// </summary>
        #region CsvDataExists
        public bool CsvDataExists => this.SelectedBookKind != BookKind.Wallet;
        #endregion

        /// <summary>
        /// CSVフォルダパス
        /// </summary>
        #region CsvFolderPath
        public string CsvFolderPath {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 文字エンコーディング
        /// </summary>
        #region TextEncodingList
        public ObservableCollection<KeyValuePair<int, string>> TextEncodingList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 選択された文字エンコーディング
        /// </summary>
        #region SelectedTextEncoding
        public int SelectedTextEncoding {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 日付 位置(1開始)
        /// </summary>
        #region ActDateIndex
        public int? ActDateIndex {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 支出 位置(1開始)
        /// </summary>
        #region ExpensesIndex
        public int? ExpensesIndex {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 項目名 位置(1開始)
        /// </summary>
        #region ItemNameIndex
        public int? ItemNameIndex {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        #endregion

        /// <summary>
        /// 関係性VMリスト
        /// </summary>
        #region RelationVMList
        public ObservableCollection<RelationViewModel> RelationVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 選択された関係性VM
        /// </summary>
        #region SelectedRelationVM
        public RelationViewModel SelectedRelationVM {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        #endregion
    }
}

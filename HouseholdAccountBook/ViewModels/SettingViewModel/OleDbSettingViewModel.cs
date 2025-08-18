using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// OleDb設定VM
    /// </summary>
    public class OleDbSettingViewModel : BindableBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public OleDbSettingViewModel() { }

        /// <summary>
        /// プロバイダ名
        /// </summary>
        /// <remarks>動的に指定するため<see cref="ObservableCollection{T}"/>を用いる</remarks>
        public ObservableCollection<KeyValuePair<string, string>> ProviderNameDic { get; } = [];
        /// <summary>
        /// 選択されたプロバイダ名
        /// </summary>
        #region SelectedProviderName
        public string SelectedProviderName
        {
            get => this._SelectedProviderName;
            set => this.SetProperty(ref this._SelectedProviderName, value);
        }
        private string _SelectedProviderName = default;
        #endregion

        /// <summary>
        /// DBファイルパス
        /// </summary>
        #region DBFilePath
        public string DBFilePath
        {
            get => this._DBFilePath;
            set => this.SetProperty(ref this._DBFilePath, value);
        }
        private string _DBFilePath = default;
        #endregion

    }
}

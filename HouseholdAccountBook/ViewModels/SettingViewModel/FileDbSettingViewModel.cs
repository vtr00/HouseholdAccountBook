namespace HouseholdAccountBook.ViewModels
{
    public class FileDbSettingViewModel : BindableBase
    {
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

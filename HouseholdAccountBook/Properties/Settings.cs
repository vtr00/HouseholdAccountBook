using System;

namespace HouseholdAccountBook.Properties
{
    // このクラスでは設定クラスでの特定のイベントを処理することができます:
    //  SettingChanging イベントは、設定値が変更される前に発生します。
    //  PropertyChanged イベントは、設定値が変更された後に発生します。
    //  SettingsLoaded イベントは、設定値が読み込まれた後に発生します。
    //  SettingsSaving イベントは、設定値が保存される前に発生します。
    public sealed partial class Settings
    {

        public Settings()
        {
            // // 設定の保存と変更のイベント ハンドラーを追加するには、以下の行のコメントを解除します:
            //
            // this.SettingChanging += this.SettingChangingEventHandler;
            //
            // this.SettingsSaving += this.SettingsSavingEventHandler;
            //
        }

        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            // SettingChangingEvent イベントを処理するコードをここに追加してください。
        }

        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // SettingsSaving イベントを処理するコードをここに追加してください。
        }

        /// <summary>
        /// 設定を最新版に更新する
        /// </summary>
        public override void Upgrade()
        {
            #region 設定の変更を引き継ぐ
            try {
                // 前回保存時のバージョンを取得する
                string versionText = (string)this.GetPreviousVersion(nameof(this.App_Version));
                if (!Version.TryParse(versionText, out Version preVer)) {
                    // 初回起動時、またはバージョン番号保存以前の場合
                }
                else {
                    // 削除された設定はここで取得する
                }

                base.Upgrade();

                // 削除された設定を新しい設定に反映する
            }
            catch (Exception) {
                base.Upgrade();
            }
            #endregion

            // Upgrade時のバージョン番号を保存する
            Version assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            this.App_Version = assemblyVersion.ToString();
            this.Save();
        }
    }
}

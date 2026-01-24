using HouseholdAccountBook.Adapters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;

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
            this.SettingsLoaded += this.Settings_SettingsLoaded;
            this.SettingsSaving += this.Settings_SettingsSaving;
        }

        /// <summary>
        /// app.config から設定を読み込んだあとに、JSON ファイルから設定を読み込む
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Settings_SettingsLoaded(object sender, SettingsLoadedEventArgs e)
        {
            // JSON ファイルが存在する場合、JSON ファイルから設定を読み込む
            if (File.Exists(FileConstants.SettingsJsonFilePath)) {
                string jsonCode = File.ReadAllText(FileConstants.SettingsJsonFilePath);
                JObject jObj = JObject.Parse(jsonCode);

                foreach (SettingsProperty prop in this.Properties) {
                    string name = prop.Name;

                    // JSON に設定が存在しない場合はスキップ
                    if (!jObj.TryGetValue(name, out JToken token)) { continue; }

                    // Nullable対応
                    Type type = prop.PropertyType;
                    Type targetType = Nullable.GetUnderlyingType(type) ?? type;

                    try {
                        object value = targetType == typeof(DateTime)
                            ? token.Type == JTokenType.String
                                ? DateTime.Parse(token.ToString())
                                : token.ToObject<DateTime>()
                            : token.ToObject(targetType);
                        this[name] = value;
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"設定 '{name}' の読み込みに失敗: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// app.config に設定を保存する前に、JSON ファイルに設定を保存する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Settings_SettingsSaving(object sender, CancelEventArgs e)
        {
            // 全設定を辞書に格納する
            SortedDictionary<string, object> dict = [];
            foreach (SettingsProperty prop in this.Properties) {
                object value = this[prop.Name];
                dict[prop.Name] = value;
            }

            // 辞書を JSON ファイルに保存する
            string jsonCode = JsonConvert.SerializeObject(dict, Formatting.Indented, new JsonSerializerSettings { DateFormatString = "yyyy-MM-dd HH:mm:ss" });
            File.WriteAllText(FileConstants.SettingsJsonFilePath, jsonCode);

            e.Cancel = true; // app.config への保存はキャンセルする
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
            Version assemblyVersion = App.GetAssemblyVersion();
            this.App_Version = assemblyVersion.ToString();
            this.Save();
        }
    }
}

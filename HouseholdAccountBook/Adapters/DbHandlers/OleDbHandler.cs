using HouseholdAccountBook.Adapters.DbHandlers.Abstract;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace HouseholdAccountBook.Adapters.DbHandlers
{
    /// <summary>
    /// Ole Db Handler
    /// </summary>
    public partial class OleDbHandler : DbHandlerBase
    {
        #region フィールド
        /// <summary>
        /// 接続文字列
        /// </summary>
        private const string mStringFormat = @"Provider={0};Data Source={1}";
        #endregion

        #region プロパティ
        /// <summary>
        /// Accessプロバイダヘッダ
        /// </summary>
        public static string AccessProviderHeader => "Microsoft.ACE.OLEDB";
        #endregion

        /// <summary>
        /// <see cref="OleDbHandler"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="info">接続情報</param>
        public OleDbHandler(ConnectInfo info) : this(info.Provider, info.FilePath) { }

        /// <summary>
        /// <see cref="OleDbHandler"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        public OleDbHandler(string provider, string filePath) : base(new OleDbConnection(string.Format(mStringFormat, provider, filePath)))
        {
            this.DBLibKind = DBLibraryKind.OleDb;
            this.DBKind = provider.Contains(AccessProviderHeader) ? DBKind.Access : DBKind.Undefined;
        }

        /// <summary>
        /// Ole DBプロバイダの一覧を取得する
        /// </summary>
        /// <returns></returns>
        public static List<KeyValuePair<string, string>> GetOleDbProvider()
        {
            List<KeyValuePair<string, string>> list = [];

            OleDbEnumerator enumerator = new();
            DataTable table = enumerator.GetElements();
            foreach (DataRow row in table.Rows) {
                string sourcesName = row["SOURCES_NAME"].ToString();
                string sourcesDescription = row["SOURCES_DESCRIPTION"].ToString();

                list.Add(new KeyValuePair<string, string>(sourcesName, $"{sourcesName} ({sourcesDescription})"));
            }
            return list;
        }
    }
}

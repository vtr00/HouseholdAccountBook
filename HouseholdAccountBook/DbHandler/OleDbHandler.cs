using HouseholdAccountBook.DbHandler.Abstract;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using static HouseholdAccountBook.Others.DbConstants;

namespace HouseholdAccountBook.DbHandler
{
    /// <summary>
    /// Ole Db Handler
    /// </summary>
    public partial class OleDbHandler : DbHandlerBase
    {
        /// <summary>
        /// 接続文字列
        /// </summary>
        private const string stringFormat = @"Provider={0};Data Source={1}";

        /// <summary>
        /// <see cref="OleDbHandler"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="info">接続情報</param>
        public OleDbHandler(ConnectInfo info) : this(info.Provider, info.FilePath) { }

        /// <summary>
        /// <see cref="OleDbHandler"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        public OleDbHandler(string provider, string filePath) : base(new OleDbConnection(string.Format(stringFormat, provider, filePath)))
        {
            this.DBLibKind = DBLibraryKind.OleDb;
            if (provider.Contains(AccessProviderHeader)) {
                this.DBKind = DBKind.Access;

                _ = this.Open();
            }
            else {
                this.DBKind = DBKind.Undefined;
            }
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

using HouseholdAccountBook.Infrastructure.DB;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;

namespace HouseholdAccountBook.Models.DbHandlers
{
    public partial class OleDbHandler : DbHandlerBase
    {
        /// <summary>
        /// 接続情報
        /// </summary>
        public class ConnectInfo : ConnectInfoBase
        {
            public ConnectInfo(string provider)
            {
                this.Kind = provider.Contains(AccessProviderHeader) ? DBKind.Access : DBKind.Undefined;

                this.Provider = provider;
            }

            /// <summary>
            /// プロバイダ
            /// </summary>
            public string Provider { get; set; }
            /// <summary>
            /// データソース
            /// </summary>
            public string DataSource { get; set; }

            /// <summary>
            /// Accessプロバイダヘッダ
            /// </summary>
            public static string AccessProviderHeader => "Microsoft.ACE.OLEDB";
        }
    }
}

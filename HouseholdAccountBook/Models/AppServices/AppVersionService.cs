using HouseholdAccountBook.Infrastructure;
using HouseholdAccountBook.Infrastructure.GitHub;
using HouseholdAccountBook.Infrastructure.GitHub.JsonDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Models.AppServices
{
    public class AppVersionService : SingletonBase<AppVersionService>
    {
        /// <summary>
        /// 最新版の情報
        /// </summary>
        private Release mLatestInfo;

        /// <summary>
        /// スタティックコンストラクタ
        /// </summary>
        static AppVersionService() => Register(static () => new AppVersionService());
        /// <summary>
        /// プライベートコンストラクタ
        /// </summary>
        private AppVersionService() { }

        /// <summary>
        /// 最新版の情報を取得する
        /// </summary>
        /// <returns>取得の成否</returns>
        public async Task<bool> GetLatestInfoAsync()
        {
            this.mLatestInfo = null;
            List<Release> releaseList = [.. await GitHubDao.GetReleaseInfo("vtr00", "HouseholdAccountBook")];
            if (0 < releaseList.Count) {
                this.mLatestInfo = releaseList.Where(static release => !release.Prerelease && !release.Draft).OrderBy(static release => release.PublishedAt).Reverse().FirstOrDefault();
            }

            return this.mLatestInfo != null;
        }
        /// <summary>
        /// 最新バージョン番号を取得する
        /// </summary>
        /// <returns>最新バージョン番号</returns>
        public Version GetLatestVersion()
        {
            if (!Version.TryParse(this.mLatestInfo?.TagName?.Replace("v", ""), out Version version)) {
                version = null;
            }
            return version;
        }
        /// <summary>
        /// 最新バージョンのHTMLアドレスを取得する
        /// </summary>
        /// <returns>最新バージョンアドレス</returns>
        public string GetLatestHtmlUrl() => this.mLatestInfo?.HtmlUrl;
    }
}

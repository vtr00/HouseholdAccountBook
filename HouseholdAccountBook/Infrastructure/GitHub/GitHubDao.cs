using HouseholdAccountBook.Infrastructure.GitHub.JsonDto;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Infrastructure.GitHub
{
    /// <summary>
    /// GitHub関連のDAO
    /// </summary>
    public static class GitHubDao
    {
        // Jsonシリアライズオプション
        private static readonly JsonSerializerOptions mOptions = new() {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// GitHubからリリース情報を取得する
        /// </summary>
        /// <param name="owner">オーナー名</param>
        /// <param name="repo">リポジトリ名</param>
        /// <returns>リリース情報</returns>
        public static async Task<IEnumerable<Release>> GetReleaseInfo(string owner, string repo)
        {
            HttpClient http = new();
            http.DefaultRequestHeaders.UserAgent.ParseAdd("app"); // 必須

            string json = await http.GetStringAsync($"https://api.github.com/repos/{owner}/{repo}/releases");

            List<Release> releases = JsonSerializer.Deserialize<List<Release>>(json, mOptions);
            return releases;
        }
    }
}

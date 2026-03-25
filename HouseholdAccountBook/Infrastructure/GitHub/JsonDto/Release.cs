using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HouseholdAccountBook.Infrastructure.GitHub.JsonDto
{
    /// <summary>
    /// GitHub Releases API の1件のリリース情報を表す
    /// </summary>
    public sealed class Release
    {
        /// <summary>
        /// API上のリソースURL
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; init; }

        /// <summary>
        /// アセット一覧取得用URL
        /// </summary>
        [JsonPropertyName("assets_url")]
        public string AssetsUrl { get; init; }

        /// <summary>
        /// アセットアップロード用URL（テンプレート付き）
        /// </summary>
        [JsonPropertyName("upload_url")]
        public string UploadUrl { get; init; }

        /// <summary>
        /// ブラウザでの表示URL
        /// </summary>
        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; init; }

        /// <summary>
        /// リリースID
        /// </summary>
        [JsonPropertyName("id")]
        public long Id { get; init; }

        /// <summary>
        /// 作成者ユーザー情報
        /// </summary>
        [JsonPropertyName("author")]
        public User Author { get; init; }

        /// <summary>
        /// GraphQL用ノードID
        /// </summary>
        [JsonPropertyName("node_id")]
        public string NodeId { get; init; }

        /// <summary>
        /// タグ名（例: v1.0.0）
        /// </summary>
        [JsonPropertyName("tag_name")]
        public string TagName { get; init; }

        /// <summary>
        /// 対象ブランチまたはコミット
        /// </summary>
        [JsonPropertyName("target_commitish")]
        public string TargetCommitish { get; init; }

        /// <summary>
        /// リリース名
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; init; }

        /// <summary>
        /// ドラフト状態かどうか
        /// </summary>
        [JsonPropertyName("draft")]
        public bool Draft { get; init; }

        /// <summary>
        /// プレリリースかどうか
        /// </summary>
        [JsonPropertyName("prerelease")]
        public bool Prerelease { get; init; }

        /// <summary>
        /// 作成日時（UTC）
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; init; }

        /// <summary>
        /// 公開日時（未公開の場合はnull）
        /// </summary>
        [JsonPropertyName("published_at")]
        public DateTimeOffset? PublishedAt { get; init; }

        /// <summary>
        /// 添付されたアセット一覧
        /// </summary>
        [JsonPropertyName("assets")]
        public List<ReleaseAsset> Assets { get; init; } = new();

        /// <summary>
        /// tar.gz形式のソースコードダウンロードURL
        /// </summary>
        [JsonPropertyName("tarball_url")]
        public string TarballUrl { get; init; }

        /// <summary>
        /// zip形式のソースコードダウンロードURL
        /// </summary>
        [JsonPropertyName("zipball_url")]
        public string ZipballUrl { get; init; }

        /// <summary>
        /// リリース本文（Markdown）
        /// </summary>
        [JsonPropertyName("body")]
        public string Body { get; init; }

        /// <summary>
        /// 関連ディスカッションURL（存在する場合）
        /// </summary>
        [JsonPropertyName("discussion_url")]
        public string DiscussionUrl { get; init; }

        /// <summary>
        /// リアクション情報（存在する場合）
        /// </summary>
        [JsonPropertyName("reactions")]
        public Reactions Reactions { get; init; }
    }

    /// <summary>
    /// リリースに紐づく添付ファイル（アセット）
    /// </summary>
    public sealed class ReleaseAsset
    {
        /// <summary>
        /// API上のリソースURL
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; init; }

        /// <summary>
        /// アセットID
        /// </summary>
        [JsonPropertyName("id")]
        public long Id { get; init; }

        /// <summary>
        /// GraphQL用ノードID
        /// </summary>
        [JsonPropertyName("node_id")]
        public string NodeId { get; init; }

        /// <summary>
        /// ファイル名
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; init; }

        /// <summary>
        /// 表示用ラベル（任意）
        /// </summary>
        [JsonPropertyName("label")]
        public string Label { get; init; }

        /// <summary>
        /// アップロードユーザー
        /// </summary>
        [JsonPropertyName("uploader")]
        public User Uploader { get; init; }

        /// <summary>
        /// MIMEタイプ
        /// </summary>
        [JsonPropertyName("content_type")]
        public string ContentType { get; init; }

        /// <summary>
        /// 状態（uploaded など）
        /// </summary>
        [JsonPropertyName("state")]
        public string State { get; init; }

        /// <summary>
        /// ファイルサイズ（バイト）
        /// </summary>
        [JsonPropertyName("size")]
        public long Size { get; init; }

        /// <summary>
        /// ダウンロード回数
        /// </summary>
        [JsonPropertyName("download_count")]
        public int DownloadCount { get; init; }

        /// <summary>
        /// 作成日時
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; init; }

        /// <summary>
        /// 更新日時
        /// </summary>
        [JsonPropertyName("updated_at")]
        public DateTimeOffset UpdatedAt { get; init; }

        /// <summary>
        /// ブラウザ用ダウンロードURL
        /// </summary>
        [JsonPropertyName("browser_download_url")]
        public string BrowserDownloadUrl { get; init; }
    }

    /// <summary>
    /// GitHubユーザー情報（簡易）
    /// </summary>
    public sealed class User
    {
        /// <summary>
        /// ユーザー名（ログインID）
        /// </summary>
        [JsonPropertyName("login")]
        public string Login { get; init; }

        /// <summary>
        /// ユーザーID
        /// </summary>
        [JsonPropertyName("id")]
        public long Id { get; init; }

        /// <summary>
        /// GraphQL用ノードID
        /// </summary>
        [JsonPropertyName("node_id")]
        public string NodeId { get; init; }

        /// <summary>
        /// アバター画像URL
        /// </summary>
        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; init; }

        /// <summary>
        /// Gravatar ID（通常は空）
        /// </summary>
        [JsonPropertyName("gravatar_id")]
        public string GravatarId { get; init; }

        /// <summary>
        /// API URL
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; init; }

        /// <summary>
        /// プロフィールページURL
        /// </summary>
        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; init; }

        /// <summary>
        /// 種別（User / Organization）
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; init; }

        /// <summary>
        /// サイト管理者かどうか
        /// </summary>
        [JsonPropertyName("site_admin")]
        public bool SiteAdmin { get; init; }
    }

    /// <summary>
    /// リアクション集計情報
    /// </summary>
    public sealed class Reactions
    {
        /// <summary>
        /// API URL
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; init; }

        /// <summary>
        /// 合計リアクション数
        /// </summary>
        [JsonPropertyName("total_count")]
        public int TotalCount { get; init; }

        /// <summary>
        /// +1 の数
        /// </summary>
        [JsonPropertyName("+1")]
        public int PlusOne { get; init; }

        /// <summary>
        /// -1 の数
        /// </summary>
        [JsonPropertyName("-1")]
        public int MinusOne { get; init; }

        /// <summary>
        /// 笑いのリアクション数
        /// </summary>
        [JsonPropertyName("laugh")]
        public int Laugh { get; init; }

        /// <summary>
        /// 祝福リアクション数
        /// </summary>
        [JsonPropertyName("hooray")]
        public int Hooray { get; init; }

        /// <summary>
        /// 困惑リアクション数
        /// </summary>
        [JsonPropertyName("confused")]
        public int Confused { get; init; }

        /// <summary>
        /// ハートリアクション数
        /// </summary>
        [JsonPropertyName("heart")]
        public int Heart { get; init; }

        /// <summary>
        /// ロケットリアクション数
        /// </summary>
        [JsonPropertyName("rocket")]
        public int Rocket { get; init; }

        /// <summary>
        /// 注目リアクション数
        /// </summary>
        [JsonPropertyName("eyes")]
        public int Eyes { get; init; }
    }
}

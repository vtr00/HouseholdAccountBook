using HouseholdAccountBook.Adapters.Logger;
using System;
using System.IO;

namespace HouseholdAccountBook.Extensions
{
    public class PathExtensions
    {
        /// <summary>
        /// 基準とするフォルダからの相対パスを取得する
        /// </summary>
        /// <param name="fullRootPath">相対パスの基準とするフォルダパス</param>
        /// <param name="targetPath">相対パスを取得したいファイル/フォルダパス</param>
        /// <param name="fullBasePath">絶対パスの基準とするフォルダパス</param>
        /// <returns>fullRootPath から targetPath への相対パス。targetPath が fullRootPath 配下にない場合は targetPath のフルパス</returns>
        public static string GetSmartPath(string fullRootPath, string targetPath, string fullBasePath = "")
        {
            using FuncLog funcLog = new(new { fullRootPath, targetPath, fullBasePath }, Log.LogLevel.Trace);

            if (string.IsNullOrEmpty(targetPath)) {
                return string.Empty;
            }
            if (!Path.IsPathFullyQualified(fullRootPath)) {
                throw new ArgumentException("fullRootPath must be an absolute path.");
            }
            if (string.IsNullOrEmpty(fullBasePath)) {
                fullBasePath = fullRootPath;
            }
            if (!Path.IsPathFullyQualified(fullBasePath)) {
                throw new ArgumentException("fullBasePath must be an absolute path.");
            }

            // 絶対パスに変換
            string fullTargetPath = Path.GetFullPath(targetPath, fullBasePath);

            // targetPath が fullRootPath の下にあるか確認
            return fullTargetPath.StartsWith(fullRootPath, StringComparison.OrdinalIgnoreCase)
                ? Path.GetRelativePath(fullRootPath, fullTargetPath)
                : fullTargetPath;
        }

        /// <summary>
        /// フォルダパスとファイル名/フォルダ名に分割する
        /// </summary>
        /// <param name="path">対象のファイル/フォルダパス</param>
        /// <param name="fullBasePath">絶対パスの基準とするフォルダパス</param>
        /// <returns>フォルダパスとファイル名/フォルダ名のタプル</returns>
        public static (string, string) GetSeparatedPath(string path, string fullBasePath)
        {
            using FuncLog funcLog = new(new { path, fullBasePath }, Log.LogLevel.Trace);

            if (string.IsNullOrEmpty(path)) {
                return (fullBasePath, string.Empty);
            }
            if (!Path.IsPathFullyQualified(fullBasePath)) {
                throw new ArgumentException("fullBasePath must be an absolute path.");
            }

            // 絶対パスに変換
            string fullPath = Path.GetFullPath(path, fullBasePath);

            // パスを分割
            string directory = Path.GetDirectoryName(fullPath) ?? string.Empty;
            string fileName = Path.GetFileName(fullPath) ?? string.Empty;

            // 分割したパスを返す
            return (directory, fileName);
        }

    }
}

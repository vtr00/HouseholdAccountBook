using System;
using System.IO;

namespace HouseholdAccountBook.Extensions
{
    public class PathExtensions
    {
        /// <summary>
        /// 基準とするフォルダからの相対パスを取得する
        /// </summary>
        /// <param name="basePath">基準とするフォルダパス</param>
        /// <param name="targetPath">相対パスを取得したいファイル/フォルダパス</param>
        /// <returns>basePath から targetPath への相対パス。targetPath が basePath 配下にない場合は targetPath のフルパス</returns>
        public static string GetSmartPath(string basePath, string targetPath)
        {
            // 絶対パスに変換
            string fullBasePath = Path.GetFullPath(basePath);
            string fullTargetPath = Path.GetFullPath(targetPath);

            // targetPath が basePath の下にあるか確認
            if (fullTargetPath.StartsWith(fullBasePath, StringComparison.OrdinalIgnoreCase)) {
                return Path.GetRelativePath(fullBasePath, fullTargetPath);
            }
            else {
                return fullTargetPath;
            }
        }

        /// <summary>
        /// フォルダパスとファイル名/フォルダ名に分割する
        /// </summary>
        /// <param name="path">対象のファイル/フォルダパス</param>
        /// <returns>フォルダパスとファイル名/フォルダ名のタプル</returns>
        public static (string, string) GetSeparatedPath(string path, string basePath)
        {
            // 絶対パスに変換
            string fullPath = Path.GetFullPath(path, basePath);

            // パスを分割
            string directory = Path.GetDirectoryName(fullPath) ?? string.Empty;
            string fileName = Path.GetFileName(fullPath) ?? string.Empty;

            // 分割したパスを返す
            return (directory, fileName);
        }

    }
}

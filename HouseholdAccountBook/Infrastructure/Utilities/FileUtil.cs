using HouseholdAccountBook.Infrastructure.Logger;
using System.Collections.Generic;
using System.IO;

namespace HouseholdAccountBook.Infrastructure.Utilities
{
    /// <summary>
    /// ファイルユーティリティ
    /// </summary>
    public static class FileUtil
    {
        /// <summary>
        /// ファイル名を昇順にソートして指定数を残して残りを削除する
        /// </summary>
        /// <param name="directoryPath">対象となるディレクトリのパス</param>
        /// <param name="fileNamePattern">ファイル名のパターン</param>
        /// <param name="fileNum">残すファイル数</param>
        public static void DeleteOldFiles(string directoryPath, string fileNamePattern, int fileNum)
        {
            using FuncLog funcLog = new(new { directoryPath, fileNamePattern, fileNum });

            if (Directory.Exists(directoryPath)) {
                // サイズ0のファイルを削除する
                int deletedZeroSizeFilesCount = 0;
                List<string> filePathList = [.. Directory.GetFiles(directoryPath, fileNamePattern, SearchOption.TopDirectoryOnly)];
                foreach (string filePath in filePathList) {
                    FileInfo fileInfo = new(filePath);
                    if (fileInfo.Length == 0) {
                        fileInfo.Delete();
                        deletedZeroSizeFilesCount++;
                    }
                }
                if (deletedZeroSizeFilesCount != 0) {
                    Log.Debug($"Deleted size 0 file(s): {deletedZeroSizeFilesCount}");
                }

                // 古いファイルを削除する
                int deletedOldFilesCount = 0;
                filePathList = [.. Directory.GetFiles(directoryPath, fileNamePattern, SearchOption.TopDirectoryOnly)];
                if (filePathList.Count > fileNum) {
                    filePathList.Sort();

                    for (int i = 0; i < filePathList.Count - fileNum; ++i) {
                        File.Delete(filePathList[i]);
                        deletedOldFilesCount++;
                    }
                }
                if (deletedOldFilesCount != 0) {
                    Log.Debug($"Deleted old file(s): {deletedOldFilesCount}");
                }
            }
        }

        /// <summary>
        /// 指定フォルダ内のファイル一覧を取得する
        /// </summary>
        /// <param name="directoryPath">探索するフォルダ</param>
        /// <param name="searchPattern">ファイル名のパターン</param>
        /// <param name="searchOption"></param>
        /// <returns>ファイルパス一覧</returns>
        public static IEnumerable<string> GetFiles(string directoryPath, string searchPattern, SearchOption searchOption)
        {
            using FuncLog funcLog = new(new { directoryPath, searchPattern, searchOption });

            List<string> filePathList = [];
            if (Directory.Exists(directoryPath)) {
                filePathList = [.. Directory.GetFiles(directoryPath, searchPattern, searchOption)];
            }
            return filePathList;
        }
    }
}

using HouseholdAccountBook.Adapters.Logger;
using System.Collections.Generic;
using System.IO;

namespace HouseholdAccountBook.Adapters
{
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
                bool deletedZeroSizeFiles = false;
                List<string> filePathList = [.. Directory.GetFiles(directoryPath, fileNamePattern, SearchOption.TopDirectoryOnly)];
                foreach (string filePath in filePathList) {
                    FileInfo fileInfo = new(filePath);
                    if (fileInfo.Length == 0) {
                        fileInfo.Delete();
                        deletedZeroSizeFiles = true;
                    }
                }
                if (deletedZeroSizeFiles) {
                    Log.Debug("Deleted size 0 files.");
                }

                // 古いファイルを削除する
                bool deletedOldFiles = false;
                filePathList = [.. Directory.GetFiles(directoryPath, fileNamePattern, SearchOption.TopDirectoryOnly)];
                if (filePathList.Count > fileNum) {
                    filePathList.Sort();

                    for (int i = 0; i < filePathList.Count - fileNum; ++i) {
                        File.Delete(filePathList[i]);
                        deletedOldFiles = true;
                    }
                }
                if (deletedOldFiles) {
                    Log.Debug("Deleted old files.");
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
        public static List<string> GetFiles(string directoryPath, string searchPattern, SearchOption searchOption)
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

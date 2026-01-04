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
            if (Directory.Exists(directoryPath)) {
                // サイズ0のファイルを削除する
                Log.Debug("Delete size 0 files.");
                List<string> filePathList = [.. Directory.GetFiles(directoryPath, fileNamePattern, SearchOption.TopDirectoryOnly)];
                foreach (string filePath in filePathList) {
                    FileInfo fileInfo = new(filePath);
                    if (fileInfo.Length == 0) {
                        fileInfo.Delete();
                    }
                }

                // 古いファイルを削除する
                Log.Debug("Delete old files.");
                filePathList = [.. Directory.GetFiles(directoryPath, fileNamePattern, SearchOption.TopDirectoryOnly)];
                if (filePathList.Count > fileNum) {
                    filePathList.Sort();

                    for (int i = 0; i < filePathList.Count - fileNum; ++i) {
                        File.Delete(filePathList[i]);
                    }
                }
            }
        }
    }
}

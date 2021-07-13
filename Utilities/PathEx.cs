using System;
using System.IO;
using System.Collections.Generic;

namespace Utilities
{
    public static class PathEx
    {
        private static string mPlatformTempFolder = Path.GetTempPath();

        /// <summary>
        /// Временная папка нашей платформы
        /// </summary>
        public static string PlatformTempFolder
        {
            get { return mPlatformTempFolder; }
            set { mPlatformTempFolder = value; }
        }
        /// <summary>
        /// Убедиться в существовании директории (если нет - создать)
        /// </summary>
        /// <returns>false, если создание директории невозможно</returns>
        public static bool EnsureDirectoryExists(string DirName)
        {
            try
            {
                if (Directory.Exists(DirName)) return true;
                Directory.CreateDirectory(DirName);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool TryDeleteDirectory(string DirName)
        {
            try
            {
                if (Directory.Exists(DirName))
                    Directory.Delete(DirName, true);
            }
            catch
            {
                return false;
            }

            return true;
        }
        public static bool IsDirectoryEmpty(string DirName)
        {
            return (Directory.GetFiles(DirName).Length == 0 && Directory.GetDirectories(DirName).Length == 0);
        }

        public static bool TryDeleteFile(string FileName)
        {
            if (FileName == null) return true;
            if (File.Exists(FileName))
            {
                try
                {
                    File.Delete(FileName);
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }
        public static bool TryDeleteFiles(string FromFolder, string FileMask)
        {
            bool bSuccess = true;
            string[] filesToDelete;
            try
            {
                if (!Directory.Exists(FromFolder)) return false;
                filesToDelete = Directory.GetFiles(FromFolder, FileMask);
            }
            catch (Exception)
            {
                return false;
            }
            foreach (string fname in filesToDelete)
            {
                try
                {
                    File.Delete(fname);
                }
                catch
                {
                    bSuccess = false;
                }
            }
            return bSuccess;
        }
        public static bool TryMoveFiles(string FromFolder, string ToFolder, string FileMask)
        {
            bool bSuccess = true;
            foreach (string srcFile in Directory.GetFiles(FromFolder, FileMask))
            {
                try
                {
                    if (!Directory.Exists(ToFolder))
                        Directory.CreateDirectory(ToFolder);

                    string dstFile = Path.Combine(ToFolder, Path.GetFileName(srcFile));
                    if (File.Exists(dstFile))
                        File.Delete(dstFile);
                    File.Move(srcFile, dstFile);
                }
                catch
                {
                    bSuccess = false;
                }
            }
            return bSuccess;
        }

        public static bool CleanDirectory(string DirName)
        {
            if (!Directory.Exists(DirName)) return true;
            bool res = true;
            foreach (string fname in Directory.GetFiles(DirName))
                if (!TryDeleteFile(fname))
                    res = false;
            foreach (string subDir in Directory.GetDirectories(DirName))
                if (!TryDeleteDirectory(subDir))
                    res = false;
            return res;
        }

        /// <summary>
        /// Вернуть имя нового, несуществующего файла по маске
        /// </summary>
        /// <param name="FileMaskWithSubstitution">Маска должна содержать полный путь со вставкой '{0}'</param>
        public static string GetFileNameByMask(string FileMaskWithSubstitution)
        {
            int i = 1;
            if (!FileMaskWithSubstitution.Contains("{0}")) throw new Exception("Assert. File mask should contain {0} substring");
            while (true)
            {
                string fname = string.Format(FileMaskWithSubstitution, i++);
                if (!File.Exists(fname))
                    return fname;
            }
        }

        public static string MakeRelativePath(string FileName, string fromFileName, bool bForContainingFilesOnly)
        {
            FileName = Path.GetFullPath(FileName);
            string FromDir = Path.GetFullPath(fromFileName);
            if (bForContainingFilesOnly)
            {
                if (!FileName.ToLower().StartsWith(FromDir.ToLower())) return FileName;
            }

            string[] pathMy = FileName.Split(Path.DirectorySeparatorChar);
            string[] pathOwner = FromDir.Split(Path.DirectorySeparatorChar);
            int ixFirstDif = Math.Min(pathMy.Length, pathOwner.Length);
            for (int i = 0; i < pathMy.Length && i < pathOwner.Length; ++i)
            {
                if (pathMy[i].ToLower() != pathOwner[i].ToLower())
                {
                    ixFirstDif = i;
                    break;
                }
            }
            if (ixFirstDif == 0) return FileName;
            List<string> PathItems = new List<string>();
            for (int i = ixFirstDif; i < pathOwner.Length; ++i)
                PathItems.Add("..");
            for (int i = ixFirstDif; i < pathMy.Length; ++i)
            {
                PathItems.Add(pathMy[i]);
            }

            return string.Join(new string(Path.DirectorySeparatorChar, 1), PathItems.ToArray());
        }
        public static string RestoreFullPath(string FileName, string fromFileName)
        {
            string FromDir = Path.GetDirectoryName(Path.GetFullPath(fromFileName));
            if (!Directory.Exists(FromDir))
                return FileName;

            string curDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(FromDir);
            string res = Path.GetFullPath(FileName);
            Directory.SetCurrentDirectory(curDir);
            return res;
        }
        public static bool CompareFileNames(string fname1, string fname2)
        {
            if (string.IsNullOrEmpty(fname1))
                return string.IsNullOrEmpty(fname2);
            if (string.IsNullOrEmpty(fname2))
                return false;
            //return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fname1.ToLower()) == Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fname2.ToLower());

            return Path.GetFullPath(fname1.ToLower()) == Path.GetFullPath(fname2.ToLower());
        }

        /// <summary>
        /// Попробовать для известного имени подкаталога найти другой подкаталог в том же каталоге.
        /// например для c:\ffmgmt\Distributors попробовать найти c:\ffmgmt\FxStrategies
        /// </summary>
        /// <param name="postfixFrom">имя известного существующего каталога</param>
        /// <param name="postfixTo">имя известного искомого каталога</param>
        /// <param name="PathFrom">введённый пользователем путь</param>
        /// <param name="PathTo">результат</param>
        /// <returns>true если подкаталог существует</returns>
        public static bool GetSopath(string postfixFrom, string postfixTo, string PathFrom, out string PathTo)
        {
            PathTo = null;
            PathFrom = PathFrom.TrimEnd('\\', '/');
            postfixFrom = postfixFrom.TrimEnd('\\', '/');
            postfixTo = postfixTo.TrimEnd('\\', '/');

            // 1) если папки PathFrom нет, не применяем логику "плагины по аналогии"
            if (!Directory.Exists(PathFrom)) return false;
            // 2) проверяем, что структура каталогов "правильная" 
            if (!PathFrom.ToLower().EndsWith(postfixFrom.ToLower())) return false;

            // 3) проверяем что postfixFrom это полное имя каталога (прямо перед ним разделитель)
            int posReplace = PathFrom.Length - postfixFrom.Length;
            if (posReplace <= 0) posReplace = 1; // на самом деле тут может быть только ноль.
            char separ = PathFrom[posReplace - 1];
            if (Path.DirectorySeparatorChar != separ && Path.AltDirectorySeparatorChar != separ) return false;

            // 4) заменяем postfixFrom на postfixTo, проверяем, что такой каталог существует.
            PathTo = PathFrom.Substring(0, posReplace) + postfixTo;
            return Directory.Exists(PathTo);
        }
    }
}

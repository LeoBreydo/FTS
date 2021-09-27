using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Configurator
{
    public static class Serializer<T>
    {
        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(T));

        /// <summary>
        /// serialize data to xml file
        /// </summary>
        /// <param name="item">the item to save</param>
        /// <param name="FileName">filename to save to</param>
        /// <param name="Rethrow">defines behaviour when exception: false(by default)=do return false, true=do rethrow exception</param>
        /// <returns>true if save succeeded</returns>
        public static bool Save(T item, string FileName, bool Rethrow = false)
        {
            try
            {
                //string dirName = Path.GetDirectoryName(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName));
                string dirName = Path.GetDirectoryName(Path.GetFullPath(FileName));
                if (dirName != null && !Directory.Exists(dirName)) Directory.CreateDirectory(dirName);

                using (TextWriter writer = new StreamWriter(FileName))
                {
                    serializer.Serialize(writer, item);
                }
            }
            catch
            {
                if (Rethrow) throw;
                return false;
            }
            return true;
        }

        /// <summary>
        /// restore xml-serialized data from file 
        /// </summary>
        /// <param name="FileName">filename</param>
        /// <param name="Rethrow">defines behaviour when exception: false(by default)=return def.value (null), true=do rethrow exception</param>
        /// <returns>returns default(T) when error occured</returns>
        public static T Open(string FileName, bool Rethrow = false)
        {
            T res;
            try
            {
                if (!File.Exists(FileName))
                {
                    if (Rethrow)
                        throw new Exception(string.Format("File not found '{0}'", FileName));
                    return default(T);
                }

                using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    res = (T)serializer.Deserialize(fs);
                }
            }
            catch (Exception excp)
            {
                if (Rethrow) throw; res = default(T);
            }
            return res;
        }
        public static string Open(string FileName, out T res)
        {
            res = default(T);
            try
            {
                if (!File.Exists(FileName))
                    return string.Format("File not found '{0}'", FileName);

                using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    res = (T)serializer.Deserialize(fs);
                }

                return null;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

    }
}

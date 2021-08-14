using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Utilities
{
    /// <summary>
    /// Save/restore xml-serializable data to the file
    /// </summary>
    /// <remarks>
    /// Make sure class T and all serializable subitems has default public constructors
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public static class Serializer<T>
    {
        private static readonly XmlSerializer serializer = new(typeof(T));

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

                using TextWriter writer = new StreamWriter(FileName);
                serializer.Serialize(writer, item);
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
                    return default;
                }

                using var fs = new FileStream(FileName, FileMode.Open,FileAccess.Read,FileShare.ReadWrite);
                res = (T)serializer.Deserialize(fs);
            }
            catch
            {
                if (Rethrow) throw; res = default;
            }
            return res;
        }
        public static string Open(string FileName, out T res)
        {
            res = default;
            try
            {
                if (!File.Exists(FileName))
                    return string.Format("File not found '{0}'", FileName);

                using var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                res = (T)serializer.Deserialize(fs);

                return null;
            }
            catch(Exception exception)
            {
                return exception.Message;
            }
        }

    }

    /// <summary>
    /// Implements the Xml-serialization for the data which should to save often and/or to be restorable if failed during the save operation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SerializerInstance<T> where T : new()
    {
        /// <summary>
        /// serializer instance
        /// </summary>
        readonly XmlSerializer serializer = new(typeof(T));
        /// <summary>
        /// true=improved reliability when overwrite the data
        /// </summary>
        private readonly bool mbCarefull;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="bCarefull">Improved reliability when overwriting an existing file (uses the filename+".bak" for temporary store previous data when overwrite the file)</param>
        public SerializerInstance(bool bCarefull = false)
        {
            mbCarefull = bCarefull;
        }
        /// <summary>
        /// Save data to file 
        /// </summary>
        /// <param name="item">data to save (should be xml-serializable)</param>
        /// <param name="FileName">filename to save to</param>
        /// <param name="Rethrow">true=rethrow an exception; false=return false</param>
        /// <returns>true if save succeeded</returns>
        public bool Save(T item, string FileName, bool Rethrow = false)
        {
            try
            {
                if (string.IsNullOrEmpty(FileName))
                {
                    if (Rethrow) throw new Exception("Invalid file name");
                    return false;
                }
                //string dirName = Path.GetDirectoryName(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName));
                string dirName = Path.GetDirectoryName(Path.GetFullPath(FileName));
                if (dirName != null && !Directory.Exists(dirName)) Directory.CreateDirectory(dirName);

                string bakFile = null;
                if (mbCarefull && File.Exists(FileName))
                {
                    bakFile = FileName + ".bak";
                    File.Copy(FileName, bakFile, true);
                }

                using (TextWriter writer = new StreamWriter(FileName))
                    serializer.Serialize(writer, item);

                if (bakFile != null)
                    File.Delete(bakFile);
            }
            catch
            {
                if (Rethrow) throw;
                return false;
            }
            return true;
        }
        /// <summary>
        /// return true if file exists
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public bool FileExists(string FileName)
        {
            if (File.Exists(FileName)) return true;
            if (!mbCarefull) return false;
            return File.Exists(FileName + ".bak");
        }
        /// <summary>
        /// restores xml-serializaed item from file
        /// </summary>
        /// <param name="FileName">file to restore from</param>
        /// <param name="throwIfProblems">true=throw an exception if can't restore data; false=return null if can't restore data</param>
        /// <returns>restored data or null</returns>
        /// <remarks>
        /// Serializer with try to restore data from specified file. If data restore is impossible (file not exists or 
        /// </remarks>
        public T Open(string FileName, bool throwIfProblems = true)
        {
            T res;
            if (!mbCarefull)
                return (OpenImpl(FileName, out res, throwIfProblems)) ? res : default;

            bool bFileExists = File.Exists(FileName);
            string bakFileName = FileName + ".bak";
            bool bBackFileExists = File.Exists(bakFileName);

            // no data found on the disk
            if (!bFileExists && !bBackFileExists) return default;

            if (!bBackFileExists)
            {
                // the backuped file not exists (the last save was succeeded)
                return (OpenImpl(FileName, out res, throwIfProblems)) ? res : default;
            }

            // Note! we not delete the bakup file before explicit Save call
            // both files exists
            // read main file
            if (OpenImpl(FileName, out res, false))
                return res; // if ok, then suppose the error occured when tried to copy main file to the backup or delete the backup file
            // if main file is invalid restore data from backup file
            return (OpenImpl(bakFileName, out res, throwIfProblems)) ? res : default;
        }

        private bool OpenImpl(string FileName, out T res, bool Rethrow)
        {
            try
            {
                if (!File.Exists(FileName))
                {
                    res = default;
                    return false;
                }

                using var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                res = (T)serializer.Deserialize(fs);

                return true;
            }
            catch
            {
                if (Rethrow) throw;

                res = default;
                return false;
            }
        }
    }

    public static class Serializer_ToString<T>
    {
        public static string Save(T item)
        {
            if (typeof(T) == typeof(String)) return item as String;

            try
            {
                // The idea
                // http://ashish.tonse.com/2008/04/serializing-xml-in-net-utf-8-and-utf-16/

                var memStream = new MemoryStream();
                var xmlWriter = new System.Xml.XmlTextWriter(memStream, Encoding.UTF8);
                var serializer = new XmlSerializer(typeof(T));

                serializer.Serialize(xmlWriter, item);
                memStream.Flush();

                return Encoding.UTF8.GetString(memStream.ToArray());
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static T Open(string savedString)
        {
            if (typeof(T) == typeof(String)) return (T)(object)savedString;
            try
            {
                var sr = new MemoryStream(Encoding.UTF8.GetBytes(savedString));
                var XMLReader = new System.Xml.XmlTextReader(sr);
                var deserializer = new XmlSerializer(typeof(T));
                object deserialized = deserializer.Deserialize(XMLReader);
                return (T)deserialized;
            }
            catch (Exception)
            {
                return default;
            }
        }
    }

}

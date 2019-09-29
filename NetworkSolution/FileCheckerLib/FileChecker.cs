using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileCheckerLib
{
    public static class FileChecker
    {
        public static event EventHandler<GetFilesDictionaryProgressEventArgs> GetFilesDictionaryProgress = delegate { };
        private static void OnGetFilesDictionaryProgress(GetFilesDictionaryProgressEventArgs e)
        {
            var handler = GetFilesDictionaryProgress;
            handler(null, e);
        }

        //public FileChecker()
        //{
        //    string currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        //    string correctArchiveFolder = Path.Combine(currentDirectory, "correct");
        //    string invalidArchiveFolder = Path.Combine(currentDirectory, "invalid");
        //    string[] correctFiles = Directory.GetFiles(correctArchiveFolder, "*.*", SearchOption.AllDirectories);
        //    string[] invalidFiles = Directory.GetFiles(invalidArchiveFolder, "*.*", SearchOption.AllDirectories);


        //    //Directory.GetFiles()
        //    //Dictionary<string, string> allFilesDictionary = GetFilesDictionary();
        //    //string[] checkSums = GetChecksums(files);

        //    Thread t = new Thread(GetFilesDictionary);
        //    t.IsBackground = true;
        //    t.Start();

        //    //Dictionary<string, string> myValidFiles = GetFilesDictionary(correctFiles, correctArchiveFolder);
        //    //Dictionary<string, string> myInvalidFiles = GetFilesDictionary(invalidFiles, invalidArchiveFolder);
        //    Console.ReadKey();
        //    //List<string> r = CompareFileDictionaries(myValidFiles, myInvalidFiles);
        //}


        /// <summary>
        /// Returns a list of paths if there are any different or missing files from the master
        /// </summary>
        /// <param name="masterFiles">The dictionary containing the valid checksums</param>
        /// <param name="filesToCompare">The dictionary to be checked against the master</param>
        /// <returns></returns>
        public static List<string> CompareFileDictionaries(Dictionary<string, string> masterFiles, Dictionary<string, string> filesToCompare)
        {
            List<string> result = new List<string>();

            foreach (var item in masterFiles)
            {
                string value;
                if (filesToCompare.TryGetValue(item.Key, out value))
                {
                    if (value != item.Value)
                        result.Add(item.Key);
                }
                else
                    result.Add(item.Key);
            }

            return result;
        }

        public static void GetFilesDictionary()
        {
            string currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string[] files = Directory.GetFiles(currentDirectory, "*.*", SearchOption.AllDirectories);

            //filesFound = files.Length;

            Dictionary<string, string> validFiles = new Dictionary<string, string>();

            for (int i = 0; i < files.Length; i++)
            {
                validFiles.Add(GetRelativePath(files[i], currentDirectory), GetChecksum(files[i]));
                //checksumsGenerated++;
                //Console.Clear();
                //Console.WriteLine("Checksums generated: {0}/{1}", checksumsGenerated, filesFound);
            }
        }

        //public Dictionary<string, string> GetFilesDictionary()
        //{
        //    string currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        //    string[] files = Directory.GetFiles(currentDirectory, "*.*", SearchOption.AllDirectories);


        //    Dictionary<string, string> validFiles = new Dictionary<string, string>();

        //    for (int i = 0; i < files.Length; i++)
        //    {
        //        validFiles.Add(GetRelativePath(files[i], currentDirectory), GetChecksum(files[i]));
        //    }

        //    return validFiles;
        //}

        /// <summary>
        /// Generate a dictionary from a collection of file paths and a base directory
        /// Progress can be fetched from the OnGetFilesDictionaryProgress event
        /// </summary>
        /// <param name="files"></param>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static void GetFilesDictionary(out Dictionary<string, string> result, string path = "")
        {
            GetFilesDictionaryProgressEventArgs args = new GetFilesDictionaryProgressEventArgs();
            result = null;
            string currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            if(path != "")
            {
                if (currentDirectory[currentDirectory.Length - 1] != '\\')
                    currentDirectory += "\\";

                currentDirectory += path;
            }
            string[] files = Directory.GetFiles(currentDirectory, "*.*", SearchOption.AllDirectories);
            args.FilesFound = files.Length;
            args.ChecksumsGenerated = 0;
            OnGetFilesDictionaryProgress(args);
            Dictionary<string, string> validFilesDictionary = new Dictionary<string, string>();
            for (int i = 0; i < files.Length; i++)
            {
                validFilesDictionary.Add(GetRelativePath(files[i], currentDirectory), GetChecksum(files[i]));
                args.ChecksumsGenerated++;
                OnGetFilesDictionaryProgress(args);
            }
            result = validFilesDictionary;
        }

        /// <summary>
        /// Generate a dictionary from a collection of file paths and a base directory
        /// </summary>
        /// <param name="files"></param>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetFilesDictionary(string[] files, string directory)
        {
            Dictionary<string, string> validFiles = new Dictionary<string, string>();

            for (int i = 0; i < files.Length; i++)
            {
                validFiles.Add(GetRelativePath(files[i], directory), GetChecksum(files[i]));
            }

            return validFiles;
        }

        /// <summary>
        /// Returns the relative path to a file, based on a directory
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="directory"></param>
        /// <returns></returns>
        private static string GetRelativePath(string filePath, string directory)
        {
            System.Uri uri1 = new Uri(filePath);

            System.Uri uri2 = new Uri(directory+"\\");

            //return Path.GetFileName(filePath);
            string t = uri2.MakeRelativeUri(uri1).ToString();
            return Uri.UnescapeDataString(uri2.MakeRelativeUri(uri1).ToString());
        }

        /// <summary>
        /// Generate checksums for a collection of file paths
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        private static string[] GetChecksums(string[] files)
        {
            string[] checkSums = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                using (FileStream stream = File.OpenRead(files[i]))
                {
                    var sha = new SHA256Managed();
                    byte[] checksum = sha.ComputeHash(stream);
                    checkSums[i] = BitConverter.ToString(checksum).Replace("-", String.Empty);
                }
            }
            return checkSums;
        }

        /// <summary>
        /// Generate checksum from a single file path
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static string GetChecksum(string filePath)
        {
            using (FileStream stream = File.OpenRead(filePath))
            {
                var sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }
    }
}

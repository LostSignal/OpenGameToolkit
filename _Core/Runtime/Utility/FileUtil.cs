//-----------------------------------------------------------------------
// <copyright file="FileUtil.cs" company="DefaultCompany">
//     Copyright (c) DefaultCompany. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY_EDITOR

namespace OGT
{
    using System.IO;
    using System.Text;
    using UnityEditor;
    using UnityEditor.VersionControl;

    public static class FileUtil
    {
        private static readonly OGTLogger Logger = OGTLogger.OGTEditor;

        public static bool CreateOrUpdateFile(string contents, string path, bool useSourceControl = true)
        {
            return CreateOrUpdateFile(contents, path, useSourceControl, EditorSettings.lineEndingsForNewScripts);
        }

        public static void CreateFile(string contents, string destinationFile, bool sourceControlAdd, LineEndingsMode lineEndings)
        {
            var parentDirectory = Path.GetDirectoryName(destinationFile);

            if (Directory.Exists(parentDirectory) == false)
            {
                Directory.CreateDirectory(parentDirectory);
            }

            string fileContents = ConvertLineEndings(contents, lineEndings);

            // Actually writing out the contents
            File.WriteAllText(destinationFile, fileContents);

            // Telling source control to add the file
            if (sourceControlAdd && Provider.enabled && Provider.isActive)
            {
                AssetDatabase.Refresh();
                Provider.Add(new Asset(destinationFile), false).Wait();
            }
        }

        public static void CreateFile(string contents, string destinationFile, bool sourceControlAdd)
        {
            CreateFile(contents, destinationFile, sourceControlAdd, EditorSettings.lineEndingsForNewScripts);
        }

        public static bool UpdateFile(string contents, string path, bool useSourceControl, LineEndingsMode lineEndings)
        {
            string fileContents = ConvertLineEndings(contents, lineEndings);

            // Early out if nothing has changed
            if (File.ReadAllText(path) == fileContents)
            {
                return false;
            }

            // Checking out the file
            if (useSourceControl && Provider.enabled && Provider.isActive)
            {
                Provider.Checkout(path, CheckoutMode.Asset).Wait();
            }

            File.WriteAllText(path, fileContents);
            return true;
        }

        public static bool UpdateFile(string contents, string path, bool useSourceControl)
        {
            return UpdateFile(contents, path, useSourceControl, EditorSettings.lineEndingsForNewScripts);
        }

        public static void CopyFile(string sourceFilePath, string destinationFilePath, bool sourceControlCheckout, LineEndingsMode lineEndings)
        {
            if (File.Exists(sourceFilePath) == false)
            {
                Logger.LogErrorFormat("Unable to copy file {0} to {1}.  Source file does not exist!", (object)sourceFilePath, (object)destinationFilePath);
            }

            string fileContents = ConvertLineEndings(File.ReadAllText(sourceFilePath), lineEndings);

            if (fileContents != File.ReadAllText(destinationFilePath))
            {
                // Checking out the file
                if (sourceControlCheckout && Provider.enabled && Provider.isActive)
                {
                    Provider.Checkout(destinationFilePath, CheckoutMode.Asset).Wait();
                }

                // Actually writing out the contents
                File.WriteAllText(destinationFilePath, fileContents);
            }
        }

        public static void CopyFile(string sourceFile, string destinationFile, bool sourceControlCheckout)
        {
            CopyFile(sourceFile, destinationFile, sourceControlCheckout, EditorSettings.lineEndingsForNewScripts);
        }

        public static string ConvertLineEndings(string inputText, LineEndingsMode lineEndings)
        {
            // Checking for a really messed up situation that happens when mixing max/pc sometimes
            if (inputText.Contains("\r\r\n"))
            {
                inputText = inputText.Replace("\r\r\n", "\n");
            }

            // If it has windows line escaping, then convert everything to Unix
            if (inputText.Contains("\r\n"))
            {
                inputText = inputText.Replace("\r\n", "\n");
            }

            if (lineEndings == LineEndingsMode.Unix)
            {
                // Do nothing, already in Unix
            }
            else if (lineEndings == LineEndingsMode.Windows)
            {
                // Convert all unix to windows
                inputText = inputText.Replace("\n", "\r\n");
            }
            else if (lineEndings == LineEndingsMode.OSNative)
            {
                // Convert all os native to windows
                inputText = inputText.Replace("\n", System.Environment.NewLine);
            }
            else
            {
                Logger.LogErrorFormat("Unable to convert line endings, unknown line ending type found: {0}", lineEndings);
            }

            return inputText;
        }

        public static string TrimTrailingWhitespace(string fileContents, char newlineCharacter)
        {
            var newFileContents = new StringBuilder();

            foreach (var line in fileContents.Split(newlineCharacter))
            {
                newFileContents.Append(line.TrimEnd());
                newFileContents.Append(newlineCharacter);
            }

            return newFileContents.ToString();
        }

        public static string ReplaceHardTabsWithSoftTabs(string fileContents)
        {
            return fileContents.Replace("\t", "    ");
        }

        public static string InsertFinalNewLine(string fileContents, char newlineCharacter)
        {
            return fileContents.TrimEnd() + newlineCharacter;
        }

        public static string ConvertLineEndings(string inputText)
        {
            return ConvertLineEndings(inputText, EditorSettings.lineEndingsForNewScripts);
        }

        public static byte[] GetUtf8Bytes(string fileContents)
        {
            byte[] contents = new UTF8Encoding().GetBytes(fileContents);
            var finalFileContents = new MemoryStream();
            finalFileContents.Write(contents, 0, contents.Length);
            return finalFileContents.ToArray();
        }

        public static void RemoveEmptyDirectories(string directory)
        {
            directory = directory.Replace("\\", "/");

            foreach (var childDirectory in Directory.GetDirectories(directory))
            {
                RemoveEmptyDirectories(childDirectory.Replace("\\", "/"));
            }

            if (Directory.GetDirectories(directory).Length == 0 && Directory.GetFiles(directory).Length == 0)
            {
                // Source control check
                if (directory.Contains("/.git/") || directory.Contains("/.plastic/") || directory.Contains("/.vs/"))
                {
                    return;
                }

                if (directory.Contains("/Assets/") && Provider.isActive)
                {
                    Provider.Delete(directory).Wait();
                }
                else
                {
                    // Manually deleting the directory meta file first
                    var directoryMetaFile = directory + ".meta";

                    if (File.Exists(directoryMetaFile))
                    {
                        Logger.LogFormat("Removing Directory Meta: {0}", directoryMetaFile);
                        EnsureNotReadOnly(directoryMetaFile);
                        File.Delete(directoryMetaFile);
                    }

                    // Manually deleting the directory
                    Logger.LogFormat("Deleting Directory: {0}", directory);
                    Directory.Delete(directory);
                }
            }
        }

        private static bool CreateOrUpdateFile(string contents, string path, bool useSourceControl, LineEndingsMode lineEndings)
        {
            bool updated;

            if (File.Exists(path))
            {
                updated = UpdateFile(contents, path, useSourceControl, lineEndings);
            }
            else
            {
                CreateFile(contents, path, useSourceControl, lineEndings);
                updated = true;
            }

            if (updated)
            {
                AssetDatabase.ImportAsset(path);
                AssetDatabase.Refresh();
            }

            return updated;
        }

        private static void EnsureNotReadOnly(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var fileAttributes = fileInfo.Attributes;

            if ((fileAttributes & FileAttributes.ReadOnly) > 0)
            {
                fileInfo.Attributes &= ~FileAttributes.ReadOnly;
            }
        }
    }
}

#endif

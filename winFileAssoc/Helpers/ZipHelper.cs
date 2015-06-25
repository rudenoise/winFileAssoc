using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Foundation;

using System.IO;
using System.IO.Compression;

using System.Diagnostics;

namespace Helpers
{
    public sealed class ZipHelper
    {
        /// <summary>
        /// Unzips the specified zip file to the specified destination folder.
        /// </summary>
        /// <param name="zipFile">The zip file</param>
        /// <param name="destinationFolder">The destination folder</param>
        /// <returns></returns>
        public static IAsyncAction UnZipFileAsync(StorageFile zipFile, StorageFolder destinationFolder)
        {
            Debug.WriteLine("Hello");
            return UnZipFileHelper(zipFile, destinationFolder).AsAsyncAction();
        }
        public static IAsyncAction AddFileToZip(StorageFile zipFile, StorageFile file)
        {
            return AddFileToZipHelper(zipFile, file).AsAsyncAction();
        }
        public static IAsyncAction RemoveFileFromZip(StorageFile zipFile, StorageFile file)
        {
            return RemoveFileFromZipHelper(zipFile, file).AsAsyncAction();
        }
        #region private helper functions
        private static async Task UnZipFileHelper(StorageFile zipFile, StorageFolder destinationFolder)
        {
            if (zipFile == null || destinationFolder == null ||
                //!Path.GetExtension(zipFile.Name).Equals(".zip", StringComparison.OrdinalIgnoreCase) ||
                !Path.GetExtension(zipFile.Name).Equals(".epub", StringComparison.OrdinalIgnoreCase)
                )
            {
                throw new ArgumentException("Invalid argument...");
            }

            Stream zipMemoryStream = await zipFile.OpenStreamForReadAsync();

            // Create zip archive to access compressed files in memory stream
            using (ZipArchive zipArchive = new ZipArchive(zipMemoryStream, ZipArchiveMode.Read))
            {
                // Unzip compressed file iteratively.
                foreach (ZipArchiveEntry entry in zipArchive.Entries)
                {
                    Debug.WriteLine("Entry");
                    Debug.WriteLine(entry.FullName);
                    await UnzipZipArchiveEntryAsync(entry, entry.FullName, destinationFolder);
                }
            }
        }

        /// <summary>
        /// It checks if the specified path contains directory.
        /// </summary>
        /// <param name="entryPath">The specified path</param>
        /// <returns></returns>
        private static bool IfPathContainDirectory(string entryPath)
        {
            if (string.IsNullOrEmpty(entryPath))
            {
                return false;
            }

            return entryPath.Contains("/");
        }

        /// <summary>
        /// It checks if the specified folder exists.
        /// </summary>
        /// <param name="storageFolder">The container folder</param>
        /// <param name="subFolderName">The sub folder name</param>
        /// <returns></returns>
        private static async Task<bool> IfFolderExistsAsync(StorageFolder storageFolder, string subFolderName)
        {
            try
            {
                IStorageItem item = await storageFolder.GetItemAsync(subFolderName);
                return (item != null);
            }
            catch
            {
                // Should never get here
                return false;
            }
        }

        /// <summary>
        /// Unzips ZipArchiveEntry asynchronously.
        /// </summary>
        /// <param name="entry">The entry which needs to be unzipped</param>
        /// <param name="filePath">The entry's full name</param>
        /// <param name="unzipFolder">The unzip folder</param>
        /// <returns></returns>
        private static async Task UnzipZipArchiveEntryAsync(ZipArchiveEntry entry, string filePath, StorageFolder unzipFolder)
        {
            if (IfPathContainDirectory(filePath))
            {
                // Create sub folder
                string subFolderName = Path.GetDirectoryName(filePath);

                bool isSubFolderExist = await IfFolderExistsAsync(unzipFolder, subFolderName);

                StorageFolder subFolder;

                if (!isSubFolderExist)
                {
                    // Create the sub folder.
                    subFolder =
                        await unzipFolder.CreateFolderAsync(subFolderName, CreationCollisionOption.ReplaceExisting);
                }
                else
                {
                    // Just get the folder.
                    subFolder =
                        await unzipFolder.GetFolderAsync(subFolderName);
                }

                // All sub folders have been created yet. Just pass the file name to the Unzip function.
                string newFilePath = Path.GetFileName(filePath);

                if (!string.IsNullOrEmpty(newFilePath))
                {
                    // Unzip file iteratively.
                    await UnzipZipArchiveEntryAsync(entry, newFilePath, subFolder);
                }
            }
            else
            {

                // Read uncompressed contents
                using (Stream entryStream = entry.Open())
                {
                    byte[] buffer = new byte[entry.Length];
                    entryStream.Read(buffer, 0, buffer.Length);

                    // Create a file to store the contents
                    StorageFile uncompressedFile = await unzipFolder.CreateFileAsync
                    (entry.Name, CreationCollisionOption.ReplaceExisting);

                    // Store the contents
                    using (IRandomAccessStream uncompressedFileStream =
                    await uncompressedFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        using (Stream outstream = uncompressedFileStream.AsStreamForWrite())
                        {
                            outstream.Write(buffer, 0, buffer.Length);
                            outstream.Flush();
                        }
                    }
                }
            }
        }
        #endregion
        #region more

        private static async Task RemoveFileFromZipHelper(StorageFile zipFile, StorageFile file)
        {
            if (
                    zipFile == null ||
                    !Path.GetExtension(zipFile.Name)
                        .Equals(".epub", StringComparison.OrdinalIgnoreCase)
                    )
            {
                throw new ArgumentException("Invalid argument...");
            }

            Stream zipMemoryStream = await zipFile.OpenStreamForWriteAsync();

            using (ZipArchive zipArchive = new ZipArchive(zipMemoryStream, ZipArchiveMode.Update))
            {
                ZipArchiveEntry currentEntry = null;
                String fileName = "OEBPS/" + file.Name;

                // Unzip compressed file iteratively.
                foreach (ZipArchiveEntry entry in zipArchive.Entries)
                {
                    if (entry.FullName == fileName)
                    {
                        currentEntry = entry;
                    }
                }
                
                if (currentEntry != null) {
                    currentEntry.Delete();
                }
                zipArchive.Dispose();
            }
            zipMemoryStream.Dispose();
        }
        private static async Task AddFileToZipHelper(StorageFile zipFile, StorageFile file)
        {
            if (
                zipFile == null ||
                !Path.GetExtension(zipFile.Name)
                    .Equals(".epub", StringComparison.OrdinalIgnoreCase)
                )
            {
                throw new ArgumentException("Invalid argument...");
            }

            Stream zipMemoryStream = await zipFile.OpenStreamForWriteAsync();

            // Create zip archive to access compressed files in memory stream
            using (ZipArchive zipArchive = new ZipArchive(zipMemoryStream, ZipArchiveMode.Update))
            {
                ZipArchiveEntry currentEntry = null;
                var fileName = "OEBPS/" + file.Name;
                
                // Unzip compressed file iteratively.
                foreach (ZipArchiveEntry entry in zipArchive.Entries)
                {
                    Debug.WriteLine(entry.FullName);
                    Debug.WriteLine(fileName);
                    if (entry.FullName == fileName)
                    {
                        currentEntry = entry;
                    }
                }

                if (currentEntry == null)
                {
                    // make a new entry
                    currentEntry = zipArchive.CreateEntry(fileName);
                }

                var contentType = file.ContentType;

                // TODO: catch other file types!
                using (var entryStream = currentEntry.Open())
                using (BinaryWriter writer = new BinaryWriter(entryStream))
                {
                    IBuffer buffer = await FileIO.ReadBufferAsync(file);
                    DataReader reader = DataReader.FromBuffer(buffer);
                    byte[] fileContent = new byte[reader.UnconsumedBufferLength];
                    reader.ReadBytes(fileContent);
                    writer.Write(fileContent);
                    
                    writer.Dispose();
                    reader.Dispose();
                }
                
                zipArchive.Dispose();
            }
            zipMemoryStream.Dispose();
        }
        #endregion
    }
}

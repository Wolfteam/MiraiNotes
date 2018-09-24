using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Streams;

namespace MiraiNotes.UWP.Utils
{
    public class MiscellaneousUtils
    {
        /// <summary>
        /// Gets the app version duh!
        /// </summary>
        /// <returns>App version (e.g: 1.0.2.0)</returns>
        public static string GetAppVersion()
        {
            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;
            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        /// <summary>
        /// Gets the path to the app local folder
        /// </summary>
        /// <returns>The app local folder path</returns>
        public static string GetApplicationPath()
            => ApplicationData.Current.LocalFolder.Path;

        /// <summary>
        /// Removes the indicated file from the app local folder
        /// </summary>
        /// <param name="filename"></param>
        /// <returns><see cref="Task"/></returns>
        public static async Task RemoveFile(string filename)
        {
            try
            {
                await (await ApplicationData.Current.LocalFolder.GetFileAsync(filename))
                    .DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch (Exception)
            {
                //no file
            }
        }

        /// <summary>
        /// Saves the file to the app local folder
        /// </summary>
        /// <param name="filename">Filename (e.g: yolo.png)</param>
        /// <param name="fileBytes">Bytes to save</param>
        /// <returns><see cref="Task"/></returns>
        public static async Task SaveFile(string filename, byte[] fileBytes)
        {
            try
            {
                var folder = ApplicationData.Current.LocalFolder;
                var thumb = await folder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

                var fs = await thumb.OpenStreamForWriteAsync(); //get stream
                var writer = new DataWriter(fs.AsOutputStream());

                writer.WriteBytes(fileBytes); //write
                await writer.StoreAsync();
                await writer.FlushAsync();

                writer.Dispose();
            }
            catch (Exception)
            {
                //Could save the file
            }
            await Task.Delay(2000);
        }
    }
}

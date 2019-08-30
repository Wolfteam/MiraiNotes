using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using AndroidGraphics = Android.Graphics;

namespace MiraiNotes.Android.Common.Utils
{
    public class MiscellaneousUtils
    {
        //TODO: IMPROVE THE IMAGEDIR, MAYBE JUST KEEP ALL THE FILES IN ONE FOLDER

        private const string UserImageFileName = "user_image.png";

        public static string GetAppVersion()
        {
            return Application.Context.PackageManager
                .GetPackageInfo(Application.Context.PackageName, 0)
                .VersionName;
        }

        public static async Task DownloadProfileImage(string url, string googleUserId)
        {
            string filename = BuildImageFilename(googleUserId);
            try
            {
                using (var client = new HttpClient())
                {
                    var imageBytes = await client.GetByteArrayAsync(url);
                    await SaveFile(filename, imageBytes);
                }
            }
            catch (Exception)
            {
                //Http excep..
            }
        }


        public static async Task SaveFile(string filename, byte[] fileBytes)
        {
            try
            {
                using (var cw = new ContextWrapper(Application.Context))
                {
                    // path to /data/data/yourapp/app_data/imageDir
                    var directory = cw.GetDir("imageDir", FileCreationMode.Private);
                    // Create imageDir
                    //                var mypath = new File(directory, $"{filename}.jpg");
                    string fullPath = Path.Combine(directory.Path, filename);
                    await File.WriteAllBytesAsync(fullPath, fileBytes);
                }
            }
            catch (Exception)
            {
                //Could save the file
            }

            await Task.Delay(1000);
        }

        public static async Task RemoveFile(string filename)
        {
            try
            {
                using (var cw = new ContextWrapper(Application.Context))
                {
                    // path to /data/data/yourapp/app_data/imageDir
                    var directory = cw.GetDir("imageDir", FileCreationMode.Private);
                    string fullPath = Path.Combine(directory.Path, filename);
                    File.Delete(fullPath);
                }
            }
            catch (Exception)
            {
                //no file
            }
            await Task.Delay(1000);
        }

        //This generates something like = 123_user_image.png
        public static string BuildImageFilename(string id)
            => $"{id}_{UserImageFileName}";


        //This generates something like = /data/data/yourapp/app_data/imageDir/123_user_image.png
        public static string GetUserProfileImagePath(string id)
        {
            using (var cw = new ContextWrapper(Application.Context))
            {
                // path to /data/data/yourapp/app_data/imageDir
                var directory = cw.GetDir("imageDir", FileCreationMode.Private);
                return Path.Combine(directory.Path, BuildImageFilename(id));
            }
        }

        public static async Task<AndroidGraphics.Bitmap> GetImageBitmapAsync(string fullPath)
        {
            return await AndroidGraphics.BitmapFactory.DecodeFileAsync(fullPath);
        }

        public static AndroidGraphics.Bitmap GetImageBitmap(string fullPath)
        {
            return AndroidGraphics.BitmapFactory.DecodeFile(fullPath);
        }
    }
}
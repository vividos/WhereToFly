using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using System;
using System.Diagnostics;
using System.IO;
using WhereToFly.App.Core;
using Xamarin.Forms;

[assembly: Dependency(typeof(WhereToFly.App.Android.AndroidAppManager))]

namespace WhereToFly.App.Android
{
    /// <summary>
    /// Android app manager implementation that provides operations on external Android apps.
    /// </summary>
    public class AndroidAppManager : IAppManager
    {
        /// <summary>
        /// Starts app's launch intent for app with given package name. See:
        /// https://stackoverflow.com/questions/2780102/open-another-application-from-your-own-intent
        /// </summary>
        /// <param name="packageName">android package name</param>
        /// <returns>true when start was successful, false when not</returns>
        public bool OpenApp(string packageName)
        {
            var context = AndroidPlatform.CurrentContext;

            var pm = context.PackageManager;
            try
            {
                var intent = pm.GetLaunchIntentForPackage(packageName);

                if (intent == null)
                {
                    return false;
                }

                context.StartActivity(intent);

                return true;
            }
            catch (ActivityNotFoundException ex)
            {
                Debug.WriteLine($"activity for package {packageName} not found: {ex.ToString()}");
                return false;
            }
        }

        /// <summary>
        /// Retrieves an icon for an Android app
        /// </summary>
        /// <param name="packageName">package name of app to get icon</param>
        /// <returns>image data bytes, or null when no image could be retrieved</returns>
        public byte[] GetAppIcon(string packageName)
        {
            var stream = this.LoadAppIcon(packageName);
            return stream.ToArray();
        }

        /// <summary>
        /// Loads app icon for package name and returns a stream
        /// </summary>
        /// <param name="packageName">package name of app icon to load</param>
        /// <returns>stream containing a PNG image, or null when no bitmap could be loaded</returns>
        private MemoryStream LoadAppIcon(string packageName)
        {
            Drawable drawable = null;
            try
            {
                var context = AndroidPlatform.CurrentContext;
                var pm = context.PackageManager;

                drawable = pm.GetApplicationIcon(packageName);
            }
            catch (Exception)
            {
                // ignore exception
            }

            if (drawable == null)
            {
                return null;
            }

            Bitmap bitmap = BitmapFromDrawable(drawable);
            if (bitmap == null)
            {
                return null;
            }

            var stream = new MemoryStream();
            bitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
            stream.Seek(0L, SeekOrigin.Begin);

            return stream;
        }

        /// <summary>
        /// Converts a Drawable to a Bitmap. See:
        /// https://stackoverflow.com/questions/3035692/how-to-convert-a-drawable-to-a-bitmap
        /// When it's a BitmapDrawable, return the internal bitmap, or else paint drawable on a
        /// canvas and return canvas bitmap.
        /// </summary>
        /// <param name="drawable">drawable to convert</param>
        /// <returns>bitmap to convert to</returns>
        private static Bitmap BitmapFromDrawable(Drawable drawable)
        {
            if (drawable is BitmapDrawable bitmapDrawable)
            {
                return bitmapDrawable.Bitmap;
            }

            Bitmap bitmap;
            if (drawable.IntrinsicWidth <= 0 || drawable.IntrinsicHeight <= 0)
            {
                bitmap = Bitmap.CreateBitmap(1, 1, Bitmap.Config.Argb8888); // Single color bitmap will be created of 1x1 pixel
            }
            else
            {
                bitmap = Bitmap.CreateBitmap(drawable.IntrinsicWidth, drawable.IntrinsicHeight, Bitmap.Config.Argb8888);
            }

            var canvas = new Canvas(bitmap);
            drawable.SetBounds(0, 0, canvas.Width, canvas.Height);
            drawable.Draw(canvas);

            return bitmap;
        }
    }
}

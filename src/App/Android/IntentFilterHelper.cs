using Android.Content;
using Android.Database;
using Android.Net;
using System.IO;
using System.Net.Http;

namespace WhereToFly.App.Android
{
    /// <summary>
    /// Helper for intent filter functions
    /// See https://stackoverflow.com/questions/3760276/android-intent-filter-associate-app-with-file-extension
    /// </summary>
    internal class IntentFilterHelper
    {
        /// <summary>
        /// Content resolver used to resolve file:// and content:// intents
        /// </summary>
        private readonly ContentResolver resolver;

        /// <summary>
        /// Creates a new IntentFilter helper object
        /// </summary>
        /// <param name="resolver">content resolver to use</param>
        internal IntentFilterHelper(ContentResolver resolver)
        {
            this.resolver = resolver;
        }

        /// <summary>
        /// Returns the filename-only part of the file transported in the intent object
        /// </summary>
        /// <param name="intent">intent object</param>
        /// <returns>filename or null when filename couldn't be retrieved</returns>
        internal string GetFilenameFromIntent(Intent intent)
        {
            if (intent.Action == Intent.ActionView ||
                intent.Action == Intent.ActionOpenDocument)
            {
                switch (intent.Scheme)
                {
                    case ContentResolver.SchemeContent:
                        return this.GetContentNameFromContentUri(intent.Data);

                    case ContentResolver.SchemeFile:
                        return intent.Data.LastPathSegment;

                    case "http":
                    case "https":
                        return intent.Data.LastPathSegment;

                    case WhereToFly.Shared.Model.AppResourceUri.DefaultScheme:
                        return intent.Data.ToString();

                    default:
                        System.Diagnostics.Debug.Assert(false, "invalid scheme");
                        break;
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves content filename from content:// URI
        /// </summary>
        /// <param name="uri">content URI</param>
        /// <returns>filename-only part</returns>
        private string GetContentNameFromContentUri(Uri uri)
        {
            ICursor cursor = this.resolver.Query(uri, null, null, null, null);
            if (cursor == null)
            {
                return null;
            }

            cursor.MoveToFirst();

            int nameIndex = cursor.GetColumnIndex(global::Android.Provider.MediaStore.IMediaColumns.DisplayName);
            return nameIndex >= 0 ? cursor.GetString(nameIndex) : null;
        }

        /// <summary>
        /// Returns a data stream from given intent
        /// </summary>
        /// <param name="intent">intent object</param>
        /// <returns>file stream, or null when none could be retrieved</returns>
        internal Stream GetStreamFromIntent(Intent intent)
        {
            if (intent.Action == Intent.ActionView ||
                intent.Action == Intent.ActionOpenDocument)
            {
                switch (intent.Scheme)
                {
                    case ContentResolver.SchemeContent:
                    case ContentResolver.SchemeFile:
                        return this.resolver.OpenInputStream(intent.Data);

                    case "http":
                    case "https":
                        return this.GetStreamFromInternetLink(intent.Data.ToString());

                    default:
                        System.Diagnostics.Debug.Assert(false, "invalid scheme");
                        break;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns a stream from given internet link, http:// or https://
        /// </summary>
        /// <param name="url">internet link</param>
        /// <returns>stream object, or null when download is not possible</returns>
        private Stream GetStreamFromInternetLink(string url)
        {
            var client = new HttpClient();

            return client.GetStreamAsync(url).Result;
        }
    }
}

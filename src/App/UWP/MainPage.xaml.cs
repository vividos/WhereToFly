using System;
using System.IO;
using WhereToFly.App.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace WhereToFly.App.UWP
{
    /// <summary>
    /// Main page of the UWP app
    /// </summary>
    public sealed partial class MainPage
    {
        /// <summary>
        /// Creates new main page
        /// </summary>
        public MainPage()
        {
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = titleBar.ButtonBackgroundColor =
                Windows.UI.Color.FromArgb(0xFF, 0x2F, 0x29, 0x9E);

            this.InitializeComponent();

            this.AllowDrop = true;
            this.DragOver += this.OnDragOver;
            this.Drop += this.OnDrop;

            this.LoadApplication(new Core.App());
        }

        /// <summary>
        /// Called when a file is dragged over the main page
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnDragOver(object sender, DragEventArgs args)
        {
            args.AcceptedOperation = DataPackageOperation.Copy;
        }

        /// <summary>
        /// Called when a file is dropped onto the main page
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private async void OnDrop(object sender, DragEventArgs args)
        {
            if (args.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await args.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    var file = items[0] as StorageFile;

                    Core.App.RunOnUiThread(async () =>
                    {
                        using var stream = await file.OpenStreamForReadAsync();
                        await OpenFileHelper.OpenFileAsync(stream, file.Name);
                    });
                }
            }
        }
    }
}

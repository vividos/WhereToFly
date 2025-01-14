using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using WhereToFly.App.Svg.UnitTest;

namespace WhereToFly.App.Svg.Sample
{
    /// <summary>
    /// View model for the sample page, providing some image sources to reference from XAML.
    /// </summary>
    public class SamplePageViewModel : ObservableObject
    {
        /// <summary>
        /// Backing store for dynamic image size
        /// </summary>
        private double dynamicImageSize = 64.0;

        #region Binding properties
        /// <summary>
        /// Image source from stream coming from the app package's Assets folder
        /// </summary>
        public ImageSource ImageFromPlatformAssets { get; }

        /// <summary>
        /// Image source from a Forms based assmbly, integrated with EmbeddedResource
        /// </summary>
        public ImageSource ImageFromFormsAssets { get; }

        /// <summary>
        /// Data URL containing base64 encoded SVG image
        /// </summary>
        public string SvgImageDataUrlBase64Encoded { get; } =
            SvgConstants.DataUriBase64Prefix +
            SvgTestImages.EncodeBase64(SvgTestImages.TestSvgImageText);

        /// <summary>
        /// Data URL containing unencoded SVG image
        /// </summary>
        public string SvgImageDataUrlUnencoded { get; }
            = SvgConstants.DataUriPlainPrefix + SvgTestImages.TestSvgImageText;

        /// <summary>
        /// Plain SVG image data
        /// </summary>
        public string SvgImagePlainData { get; } = SvgTestImages.TestSvgImageText;

        /// <summary>
        /// Indicates if dark mode is currently enabled
        /// </summary>
        public bool IsDarkModeOn
        {
            get =>
                Application.Current != null &&
                (Application.Current.UserAppTheme == AppTheme.Dark ||
                (Application.Current.UserAppTheme == AppTheme.Unspecified && Application.Current.RequestedTheme == AppTheme.Dark));

            set => Application.Current!.UserAppTheme = value ? AppTheme.Dark : AppTheme.Light;
        }

        /// <summary>
        /// Command to execute when tapping on an image using a gesture recognizer
        /// </summary>
        public ICommand TappedImageCommand { get; }

        /// <summary>
        /// Command to execute when user clicked on the "Pick SVG image" button
        /// </summary>
        public ICommand PickSvgImageCommand { get; }

        /// <summary>
        /// Image source of picked image
        /// </summary>
        public ImageSource? PickedImage { get; private set; }

        /// <summary>
        /// Current image size for dynamic image resizing
        /// </summary>
        public double DynamicImageSize
        {
            get => this.dynamicImageSize;
            set => this.SetProperty(ref this.dynamicImageSize, value);
        }
        #endregion

        /// <summary>
        /// Creates a new view model object and initializes all binding properties
        /// </summary>
        public SamplePageViewModel()
        {
            this.ImageFromPlatformAssets = ImageSource.FromStream(
                () => GetPlatformStream());

            this.ImageFromFormsAssets = ImageSource.FromResource(
                "WhereToFly.App.Svg.Sample.Assets.colibri.svg",
                typeof(SvgTestImages).Assembly);

            this.TappedImageCommand = new AsyncRelayCommand(this.TappedImage);

            this.PickSvgImageCommand = new AsyncRelayCommand(this.PickSvgImage);
        }

        /// <summary>
        /// Returns a stream from the platform's assets folder
        /// </summary>
        /// <returns>platform file stream</returns>
        private static Stream GetPlatformStream()
        {
            string filename = "Assets/toucan.svg";

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                filename = filename.Replace("Assets/", string.Empty);
            }

            return FileSystem.OpenAppPackageFileAsync(filename).Result;
        }

        /// <summary>
        /// Called when an SVG image is tapped
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task TappedImage()
        {
            var currentPage = Application.Current!.Windows[0].Page!;

            await currentPage.DisplayAlert(
               "SvgImage sample app",
               "Image was tapped",
               "Close");
        }

        /// <summary>
        /// Lets the user pick an SVG image from storage and tries to display it
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task PickSvgImage()
        {
            try
            {
                var options = new PickOptions
                {
                    FileTypes = new FilePickerFileType(
                        new Dictionary<DevicePlatform, IEnumerable<string>>
                        {
                            { DevicePlatform.Android, new string[] { "image/svg+xml" } },
                            { DevicePlatform.WinUI, new string[] { ".svg" } },
                        }),
                    PickerTitle = "Select an SVG image to display",
                };

                var result = await FilePicker.PickAsync(options);

                if (result == null ||
                    string.IsNullOrEmpty(result.FullPath))
                {
                    return;
                }

                var stream = await result.OpenReadAsync();
                this.PickedImage = ImageSource.FromStream(() => stream);

                this.OnPropertyChanged(nameof(this.PickedImage));
            }
            catch (Exception ex)
            {
                var currentPage = Application.Current!.Windows[0].Page!;

                await currentPage.DisplayAlert(
                   "SvgImage sample app",
                   "Error while picking an SVG image file: " + ex.Message,
                   "Close");
            }
        }
    }
}

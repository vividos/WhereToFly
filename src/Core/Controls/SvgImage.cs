using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Threading;
using Xamarin.Forms;

namespace WhereToFly.Core.Controls
{
    /// <summary>
    /// SVG image for Xamarin.Forms, using the SkiaSharp.Extended.Svg NuGet package. See also:
    /// https://www.pshul.com/2018/01/25/xamarin-forms-using-svg-images-with-skiasharp/
    /// </summary>
    public class SvgImage : Frame
    {
        /// <summary>
        /// SkiaSharp canvas view, to draw SVG image to
        /// </summary>
        private readonly SKCanvasView canvasView = new SKCanvasView();

        /// <summary>
        /// Lazy-initialized SVG image instance
        /// </summary>
        private readonly Lazy<SkiaSharp.Extended.Svg.SKSvg> svgImage;

        /// <summary>
        /// Creates a new SVG image control
        /// </summary>
        public SvgImage()
        {
            this.Padding = new Thickness(0);

            // Thanks to TheMax for pointing out that on mobile, the icon will have a shadow by default.
            // Also it has a white background, which we might not want.
            this.HasShadow = false;
            this.BackgroundColor = Color.Transparent;

            this.Content = canvasView;
            this.canvasView.PaintSurface += this.CanvasViewOnPaintSurface;

            this.svgImage = new Lazy<SkiaSharp.Extended.Svg.SKSvg>(() => this.LoadImage());
        }

        #region Bindable properties
        /// <summary>
        /// Source property, storing an ImageSource instance holding the SVG image
        /// </summary>
        public static readonly BindableProperty SourceProperty = BindableProperty.Create(
            nameof(Source),
            typeof(ImageSource),
            typeof(SvgImage),
            default(ImageSource),
            propertyChanged: OnSourcePropertyChanged);

        /// <summary>
        /// Image source for SVG image
        /// </summary>
        public ImageSource Source
        {
            get => (ImageSource)base.GetValue(SourceProperty);
            set => base.SetValue(SourceProperty, value);
        }
        #endregion

        /// <summary>
        /// Called when the Source property has been changed.
        /// </summary>
        /// <param name="bindable">bindable object</param>
        /// <param name="oldvalue">old bound value</param>
        /// <param name="newvalue">newly bound value</param>
        private static void OnSourcePropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            SvgImage image = bindable as SvgImage;
            image?.canvasView.InvalidateSurface();
        }

        /// <summary>
        /// Called when binding context has changed
        /// </summary>
        protected override void OnBindingContextChanged()
        {
            if (this.Source != null)
            {
                BindableObject.SetInheritedBindingContext(this.Source, base.BindingContext);
            }

            base.OnBindingContextChanged();
        }

        /// <summary>
        /// Called in order to paint on the surface of the SkiaSharp canvas.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void CanvasViewOnPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKCanvas canvas = args.Surface.Canvas;
            canvas.Clear();

            SkiaSharp.Extended.Svg.SKSvg svg = this.svgImage.Value;
            if (svg == null)
            {
                return;
            }

            SKImageInfo info = args.Info;
            canvas.Translate(info.Width / 2f, info.Height / 2f);

            SKRect bounds = svg.ViewBox;
            float xRatio = info.Width / bounds.Width;
            float yRatio = info.Height / bounds.Height;

            float ratio = Math.Min(xRatio, yRatio);

            canvas.Scale(ratio);
            canvas.Translate(-bounds.MidX, -bounds.MidY);

            canvas.DrawPicture(svg.Picture);
        }

        /// <summary>
        /// Loads SVG image from Source property and returns it.
        /// </summary>
        /// <returns>loaded SVG image</returns>
        private SkiaSharp.Extended.Svg.SKSvg LoadImage()
        {
            var streamSource = this.Source as StreamImageSource;

            if (streamSource != null &&
                streamSource.Stream != null)
            {
                var stream = streamSource.Stream.Invoke(CancellationToken.None).Result;

                if (stream != null)
                {
                    var svg = new SkiaSharp.Extended.Svg.SKSvg();
                    svg.Load(stream);

                    return svg;
                }
            }

            return null;
        }
    }
}

﻿using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System.ComponentModel;
using System.Diagnostics;

// make SvgImage internals visible to unit tests
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("WhereToFly.App.Svg.UnitTest")]

namespace WhereToFly.App.Svg
{
    /// <summary>
    /// SVG image control, using the SkiaSharp.Extended.Svg classes (formerly NuGet package).
    /// </summary>
    public class SvgImage : SKCanvasView
    {
        /// <summary>
        /// Lazy-initialized SVG image instance
        /// </summary>
        private SkiaSharp.Extended.Svg.SKSvg? svgImage;

        /// <summary>
        /// Creates a new SVG image control
        /// </summary>
        public SvgImage()
        {
            this.BackgroundColor = Colors.Transparent;
            this.PaintSurface += this.CanvasViewOnPaintSurface;
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
        /// Image source for SVG image. The image source can be any image source created by
        /// ImageSource factory methods (e.g. FromStream()), but can also be a string that is
        /// converted by the SvgImageSourceTypeConverter converter.
        /// </summary>
        [TypeConverter(typeof(SvgImageSourceTypeConverter))]
        public ImageSource Source
        {
            get => (ImageSource)this.GetValue(SourceProperty);
            set => this.SetValue(SourceProperty, value);
        }

        /// <summary>
        /// TintColor property, storing a color to tint the image
        /// </summary>
        public static readonly BindableProperty TintColorProperty = BindableProperty.Create(
            nameof(TintColor),
            typeof(Color),
            typeof(SvgImage),
            defaultValue: null,
            propertyChanged: OnTintColorPropertyChanged);

        /// <summary>
        /// Tint color for the image. When set to Color.Default, the color isn't tinted when
        /// drawn, and all colors of the SVG images are used. When the tint color is set, the
        /// non-transparent parts of the images are drawn with the tint color. This is useful for
        /// coloring mono-colored images in light and dark app themes.
        /// </summary>
        public Color TintColor
        {
            get => (Color)this.GetValue(TintColorProperty);
            set => this.SetValue(TintColorProperty, value);
        }
        #endregion

        /// <summary>
        /// Called when the Source property has been changed.
        /// </summary>
        /// <param name="bindable">bindable object</param>
        /// <param name="oldValue">old bound value</param>
        /// <param name="newValue">newly bound value</param>
        private static void OnSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is not SvgImage image)
            {
                return;
            }

            bindable.Dispatcher.Dispatch(async () =>
            {
                try
                {
                    image.svgImage = await SvgDataResolver.LoadSvgImage(image.Source);
                    image.InvalidateSurface();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("SvgImage.Source: Error while loading image: " + ex.ToString());
                }
            });
        }

        /// <summary>
        /// Called when the TintColor property has changed. Invalidates surface to redraw the
        /// image with a new color.
        /// </summary>
        /// <param name="bindable">bindable object</param>
        /// <param name="oldValue">old bound value</param>
        /// <param name="newValue">newly bound value</param>
        private static void OnTintColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is not SvgImage image)
            {
                return;
            }

            bindable.Dispatcher.Dispatch(image.InvalidateSurface);
        }

        /// <summary>
        /// Called when binding context has changed
        /// </summary>
        protected override void OnBindingContextChanged()
        {
            if (this.Source != null)
            {
                SetInheritedBindingContext(this.Source, this.BindingContext);
            }

            base.OnBindingContextChanged();
        }

        /// <summary>
        /// Called in order to paint on the surface of the SkiaSharp canvas.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void CanvasViewOnPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            var canvas = args.Surface.Canvas;

            if (this.Background != null)
            {
                if (this.Background is SolidColorBrush solidColorBrush)
                {
                    if (solidColorBrush.Color != null &&
                        solidColorBrush.Color != Colors.Transparent)
                    {
                        canvas.Clear(solidColorBrush.Color.ToSKColor());
                    }
                }
                else
                {
                    Debug.WriteLine("SvgImage doesn't support Background property yet");
                }
            }

            if (this.BackgroundColor != null &&
                this.BackgroundColor != Colors.Transparent)
            {
                canvas.Clear(this.BackgroundColor.ToSKColor());
            }
            else
            {
                canvas.Clear();
            }

            if (this.svgImage == null ||
                this.svgImage.Picture == null)
            {
                return;
            }

            var info = args.Info;
            canvas.Translate(info.Width / 2f, info.Height / 2f);

            var bounds = this.svgImage.Picture.CullRect;
            float xRatio = info.Width / bounds.Width;
            float yRatio = info.Height / bounds.Height;

            float ratio = Math.Min(xRatio, yRatio);

            canvas.Scale(ratio);
            canvas.Translate(-bounds.MidX, -bounds.MidY);

            if (this.TintColor == null)
            {
                canvas.DrawPicture(this.svgImage.Picture);
            }
            else
            {
                using var paint = new SKPaint();

                paint.ColorFilter = SKColorFilter.CreateBlendMode(
                    this.TintColor.ToSKColor(),
                    SKBlendMode.SrcIn);

                canvas.DrawPicture(this.svgImage.Picture, paint);
            }
        }
    }
}

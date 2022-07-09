using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Diagnostics;
using System.Windows.Input;
using WhereToFly.Geo.Model;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Controls
{
    /// <summary>
    /// View that displays a compass with all takeoff directions specified in the Source
    /// attribute. When the control is editable, the user can change the takeoff directions by
    /// tapping on one or more compass directions to activate or deactivate it. This needs a
    /// TwoWay binding to the source. When the control is read-only, tapping on the view triggers
    /// a TouchCommand.
    /// The following properties can be used by this control:
    /// - Source: TakeoffDirections value
    /// - IsReadOnly: bool value that determines if the user can modify the takeoff directions
    ///   value by tapping on one of the takeoff directions; requires a TwoWay binding of Source
    /// - TouchCommand: ICommand that is triggered when the user taps on the control; only in
    ///   read-only mode
    /// - BackgroundColor: Color property (inherited from VisualElement) that determines the
    ///   control's background color
    /// - CompassColor: Color value specifying the color of the compass segments that can be used
    ///   as takeoff directions
    /// - CompassBorderColor: Color value specifying the color of the border and the dashed lines
    ///   of the compass segments
    /// - CompassBackgroundColor: Color value specifying the background of the compass
    /// - ShowLabels: bool value that determines if each segment also contains a direction label
    /// See also:
    /// https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/
    /// https://medium.com/@k.l.mueller/the-basics-to-create-custom-xamarin-forms-controls-using-skiasharp-be24bdda879e
    /// </summary>
    public class TakeoffDirectionsView : SKCanvasView
    {
        /// <summary>
        /// Angle of each segment
        /// </summary>
        private const float SegmentAngle = 45.0f;

        /// <summary>
        /// Indicates if a touch operation is ongoing
        /// </summary>
        private bool isTouchDown;

        /// <summary>
        /// Stores the last toggled segment index; needed to remember which segment was last
        /// toggled during a touch operation
        /// </summary>
        private int lastToggledSegmentIndex = -1;

        #region Bindable properties
        /// <summary>
        /// Binding property for the source, containing the takeoff directions flags enum
        /// </summary>
        public static readonly BindableProperty SourceProperty =
            BindableProperty.Create(
                propertyName: nameof(Source),
                returnType: typeof(TakeoffDirections),
                declaringType: typeof(TakeoffDirectionsView),
                defaultValue: TakeoffDirections.None,
                propertyChanged: InvalidateSurfaceOnChange);

        /// <summary>
        /// Binding property specifying if the view is non-modifyable
        /// </summary>
        public static readonly BindableProperty IsReadOnlyProperty =
            BindableProperty.Create(
                propertyName: nameof(IsReadOnly),
                returnType: typeof(bool),
                declaringType: typeof(TakeoffDirectionsView),
                defaultValue: true);

        /// <summary>
        /// Binding property specifying a touch command object
        /// </summary>
        public static readonly BindableProperty TouchCommandProperty =
            BindableProperty.Create(
                propertyName: nameof(TouchCommand),
                returnType: typeof(ICommand),
                declaringType: typeof(TakeoffDirectionsView),
                defaultValue: null);

        /// <summary>
        /// Binding property specifying the compass color
        /// </summary>
        public static readonly BindableProperty CompassColorProperty =
            BindableProperty.Create(
                propertyName: nameof(CompassColor),
                returnType: typeof(Color),
                declaringType: typeof(TakeoffDirectionsView),
                defaultValue: Color.Green,
                validateValue: (_, value) => value != null,
                propertyChanged: InvalidateSurfaceOnChange);

        /// <summary>
        /// Binding property specifying the compass border color
        /// </summary>
        public static readonly BindableProperty CompassBorderColorProperty =
            BindableProperty.Create(
                propertyName: nameof(CompassBorderColor),
                returnType: typeof(Color),
                declaringType: typeof(TakeoffDirectionsView),
                defaultValue: Color.Accent,
                validateValue: (_, value) => value != null,
                propertyChanged: InvalidateSurfaceOnChange);

        /// <summary>
        /// Binding property specifying the compass background color
        /// </summary>
        public static readonly BindableProperty CompassBackgroundColorProperty =
            BindableProperty.Create(
                propertyName: nameof(CompassBackgroundColor),
                returnType: typeof(Color),
                declaringType: typeof(TakeoffDirectionsView),
                defaultValue: Color.Transparent,
                validateValue: (_, value) => value != null,
                propertyChanged: InvalidateSurfaceOnChange);

        /// <summary>
        /// Binding property specifying if direction labels are shown
        /// </summary>
        public static readonly BindableProperty ShowLabelsProperty =
            BindableProperty.Create(
                propertyName: nameof(ShowLabels),
                returnType: typeof(bool),
                declaringType: typeof(TakeoffDirectionsView),
                defaultValue: true);
        #endregion

        #region View properties
        /// <summary>
        /// Takeoff directions source value
        /// </summary>
        public TakeoffDirections Source
        {
            get => (TakeoffDirections)this.GetValue(SourceProperty);
            set => this.SetValue(SourceProperty, value);
        }

        /// <summary>
        /// Indicates if the view is read-only and the source value can't be modified
        /// </summary>
        public bool IsReadOnly
        {
            get => (bool)this.GetValue(IsReadOnlyProperty);
            set => this.SetValue(IsReadOnlyProperty, value);
        }

        /// <summary>
        /// Command that is executed when the view is in read-only mode and the user touched the
        /// view.
        /// </summary>
        public ICommand TouchCommand
        {
            get => (ICommand)this.GetValue(TouchCommandProperty);
            set => this.SetValue(TouchCommandProperty, value);
        }

        /// <summary>
        /// Color for the highlighted compass segments based on the takeoff directions
        /// </summary>
        public Color CompassColor
        {
            get => (Color)this.GetValue(CompassColorProperty);
            set => this.SetValue(CompassColorProperty, value);
        }

        /// <summary>
        /// Border color for the compass, including segment divider lines
        /// </summary>
        public Color CompassBorderColor
        {
            get => (Color)this.GetValue(CompassBorderColorProperty);
            set => this.SetValue(CompassBorderColorProperty, value);
        }

        /// <summary>
        /// Background color for the compass
        /// </summary>
        public Color CompassBackgroundColor
        {
            get => (Color)this.GetValue(CompassBackgroundColorProperty);
            set => this.SetValue(CompassBackgroundColorProperty, value);
        }

        /// <summary>
        /// Indicates if takeoff direction labels should be shown
        /// </summary>
        public bool ShowLabels
        {
            get => (bool)this.GetValue(ShowLabelsProperty);
            set => this.SetValue(ShowLabelsProperty, value);
        }
        #endregion

        /// <summary>
        /// Creates a new view to display takeoff directions
        /// </summary>
        public TakeoffDirectionsView()
        {
            this.EnableTouchEvents = true;
        }

        #region Drawing
        /// <summary>
        /// Called when the surface has to be painted
        /// </summary>
        /// <param name="e">event args</param>
        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;

            canvas.Clear();

            float xy = Math.Min(e.Info.Width, e.Info.Height) / 2.0f;
            float strokeWidth = xy / 20.0f;

            float margin = xy * 0.1f;
            float radius = xy - margin - (strokeWidth / 2.0f);

            var center = new SKPoint(xy, xy);

            this.DrawCompassCircle(canvas, center, radius, strokeWidth);
            this.DrawFilledSegments(canvas, center, radius);
            this.DrawDividerLines(canvas, center, radius);

            if (this.ShowLabels)
            {
                this.DrawDirectionLabels(canvas, center, radius);
            }
        }

        /// <summary>
        /// Draws compass circle, including border and filled background
        /// </summary>
        /// <param name="canvas">canvas to draw on</param>
        /// <param name="center">center point of compass</param>
        /// <param name="radius">radius of compass</param>
        /// <param name="strokeWidth">stroke width of circle border</param>
        private void DrawCompassCircle(SKCanvas canvas, SKPoint center, float radius, float strokeWidth)
        {
            using (var borderCircle = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = strokeWidth,
                Color = this.CompassBorderColor.ToSKColor(),
            })
            {
                canvas.DrawCircle(center, radius, borderCircle);
            }

            using var fillCircle = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = this.CompassBackgroundColor.ToSKColor(),
            };
            canvas.DrawCircle(center, radius, fillCircle);
        }

        /// <summary>
        /// Draws all filled segments, based on the Source property
        /// </summary>
        /// <param name="canvas">canvas to draw on</param>
        /// <param name="center">center point of compass</param>
        /// <param name="radius">radius of compass</param>
        private void DrawFilledSegments(SKCanvas canvas, SKPoint center, float radius)
        {
            using var path = new SKPath();
            using var fillPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = this.CompassColor.ToSKColor(),
            };
            var rect = new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius);

            for (int segmentIndex = 0; segmentIndex < 8; segmentIndex++)
            {
                if (!IsSegmentFilled(this.Source, segmentIndex))
                {
                    continue;
                }

                float startAngle = (segmentIndex * SegmentAngle) - (0.5f * SegmentAngle) - 90.0f;

                path.MoveTo(center);
                path.ArcTo(rect, startAngle, SegmentAngle, false);
                path.Close();
            }

            canvas.DrawPath(path, fillPaint);
        }

        /// <summary>
        /// Draw divider lines between each segment, using the background color
        /// </summary>
        /// <param name="canvas">canvas to draw on</param>
        /// <param name="center">center point of compass</param>
        /// <param name="radius">radius of compass</param>
        private void DrawDividerLines(SKCanvas canvas, SKPoint center, float radius)
        {
            float dash = radius < 32.0 ? radius / 8.0f : 3.0f;
            var dashPathEffect = SKPathEffect.CreateDash(new float[] { dash, dash }, 0.0f);

            using var linePaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = this.CompassBorderColor.ToSKColor(),
                PathEffect = dashPathEffect,
            };
            for (int segmentIndex = 0; segmentIndex < 8; segmentIndex++)
            {
                float angle = (segmentIndex * SegmentAngle) + (0.5f * SegmentAngle) - 90.0f;
                double angleInRad = (angle * Math.PI) / 180.0;

                var endPoint = new SKPoint(
                    (float)(center.X + (radius * Math.Cos(angleInRad))),
                    (float)(center.Y + (radius * Math.Sin(angleInRad))));

                canvas.DrawLine(center, endPoint, linePaint);
            }
        }

        /// <summary>
        /// Text strings for all segment directions
        /// </summary>
        private static readonly string[] DirectionText = new string[8]
        {
            "N", "NE", "E", "SE", "S", "SW", "W", "NW",
        };

        /// <summary>
        /// Draws direction labels for every segment
        /// </summary>
        /// <param name="canvas">canvas to draw on</param>
        /// <param name="center">center point of compass</param>
        /// <param name="radius">radius of compass</param>
        private void DrawDirectionLabels(SKCanvas canvas, SKPoint center, float radius)
        {
            for (int segmentIndex = 0; segmentIndex < 8; segmentIndex++)
            {
                bool isFilled = IsSegmentFilled(this.Source, segmentIndex);
                Color textColor = isFilled ? this.CompassBackgroundColor : this.CompassBorderColor;

                float angle = (segmentIndex * SegmentAngle) - 90.0f;
                double angleInRad = (angle * Math.PI) / 180.0;

                var textPos = new SKPoint(
                    (float)(center.X + (0.8 * radius * Math.Cos(angleInRad))),
                    (float)(center.Y + (0.8 * radius * Math.Sin(angleInRad))));

                using var textPaint = new SKPaint
                {
                    Color = textColor.ToSKColor(),
                    TextAlign = SKTextAlign.Center,
                    TextSize = radius / 5.0f,
                };
                string text = DirectionText[segmentIndex];

                var textBounds = default(SKRect);
                textPaint.MeasureText(text, ref textBounds);

                textPos.Y += textBounds.Height / 2.0f;

                canvas.DrawText(text, textPos, textPaint);
            }
        }
        #endregion

        #region Touch handling
        /// <summary>
        /// Called when the view receives a touch event
        /// </summary>
        /// <param name="e">event args</param>
        protected override void OnTouch(SKTouchEventArgs e)
        {
            e.Handled = true;

            if (this.IsReadOnly)
            {
                // check read-only TouchCommand handling
                if (e.ActionType == SKTouchAction.Pressed &&
                    this.TouchCommand != null &&
                    this.TouchCommand.CanExecute(null))
                {
                    this.TouchCommand.Execute(null);
                }

                return;
            }

            switch (e.ActionType)
            {
                case SKTouchAction.Pressed:
                    this.isTouchDown = true;
                    this.lastToggledSegmentIndex = -1;
                    this.ToggleSegment(e.Location);
                    break;

                case SKTouchAction.Exited:
                case SKTouchAction.Released:
                case SKTouchAction.Cancelled:
                    this.isTouchDown = false;
                    break;

                case SKTouchAction.Moved:
                    if (this.isTouchDown)
                    {
                        this.ToggleSegment(e.Location);
                    }

                    break;

                default:
                    // ignore other actions
                    break;
            }
        }

        /// <summary>
        /// Toggles a segment's takeoff directions value
        /// </summary>
        /// <param name="location">location of touch event</param>
        private void ToggleSegment(SKPoint location)
        {
            var size = this.CanvasSize;

            float xy = Math.Min(size.Width, size.Height) / 2.0f;

            double deltaX = location.X - xy;
            double deltaY = xy - location.Y;
            double distance = Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
            if (distance > xy)
            {
                return;
            }

            double angleSkiaCoord = Math.Atan2(deltaY, deltaX) * (180.0f / Math.PI);
            double angle = (90.0 - angleSkiaCoord + 360.0) % 360.0;

            int segmentIndex = (int)(((angle + 22.5) % 360.0) / 45.0);

            if (segmentIndex == this.lastToggledSegmentIndex)
            {
                return;
            }

            this.lastToggledSegmentIndex = segmentIndex;

            TakeoffDirections direction = DirectionFromSegmentIndex(segmentIndex);
            TakeoffDirections directionPlus = segmentIndex == 0 ? TakeoffDirections.Last : (TakeoffDirections)((int)direction >> 1);
            TakeoffDirections directionMinus = segmentIndex == 7 ? TakeoffDirections.Last : (TakeoffDirections)((int)direction << 1);

            TakeoffDirections directionInclusive = direction;
            directionInclusive |= directionPlus;
            directionInclusive |= directionMinus;

            if (this.Source.HasFlag(direction) ||
                this.Source.HasFlag(directionPlus) ||
                this.Source.HasFlag(directionMinus))
            {
                this.Source &= ~directionInclusive;
            }
            else
            {
                this.Source |= direction;
            }

            this.InvalidateSurface();
        }
        #endregion

        /// <summary>
        /// Returns if a segment with the given segment index should be filled. As the enum
        /// TakeoffDirections has 16 directions, but the view only draws 8 directions, each
        /// segment is visible when the matching cardinal direction is set, or one of its
        /// neighbours. Example: Draw segment 0 when N, NNE or NNW is set.
        /// </summary>
        /// <param name="takeoffDirectionsToCheck">takeoff directions</param>
        /// <param name="segmentIndex">segment index, from 0 to 7</param>
        /// <returns>true when segment should be filled, false when not</returns>
        private static bool IsSegmentFilled(TakeoffDirections takeoffDirectionsToCheck, int segmentIndex)
        {
            Debug.Assert(segmentIndex >= 0 && segmentIndex < 8, "segment index must be in range");

            TakeoffDirections direction = DirectionFromSegmentIndex(segmentIndex);
            TakeoffDirections directionPlus = segmentIndex == 0 ? TakeoffDirections.Last : (TakeoffDirections)((int)direction >> 1);
            TakeoffDirections directionMinus = segmentIndex == 7 ? TakeoffDirections.Last : (TakeoffDirections)((int)direction << 1);

            return takeoffDirectionsToCheck.HasFlag(direction) ||
                takeoffDirectionsToCheck.HasFlag(directionPlus) ||
                takeoffDirectionsToCheck.HasFlag(directionMinus);
        }

        /// <summary>
        /// Determines the takeoff direction value from a given segment index, in the range of 0
        /// to 7.
        /// </summary>
        /// <param name="segmentIndex">segment index</param>
        /// <returns>takeoff direction</returns>
        private static TakeoffDirections DirectionFromSegmentIndex(int segmentIndex)
        {
            Debug.Assert(segmentIndex >= 0 && segmentIndex < 8, "segment index must be in range");

            return (TakeoffDirections)(1 << (segmentIndex * 2));
        }

        /// <summary>
        /// Called by the binding properties when a new value is set; invalidates surface and
        /// forces a redraw.
        /// </summary>
        /// <param name="bindable">bindable object</param>
        /// <param name="oldValue">old property value</param>
        /// <param name="newValue">new property value</param>
        private static void InvalidateSurfaceOnChange(BindableObject bindable, object oldValue, object newValue)
        {
            var view = (TakeoffDirectionsView)bindable;

            // avoid unnecessary invalidation
            if (oldValue != newValue)
            {
                view.InvalidateSurface();
            }
        }
    }
}

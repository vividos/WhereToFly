using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using WhereToFly.Geo;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Controls
{
    /// <summary>
    /// Compass view, showing a compass rose, cardinal directions, sunrise and sunset points and
    /// direction to a selected target. The compass view rotates with the user's heading.
    /// </summary>
    public class CompassView : SKCanvasView
    {
        #region Bindable properties
        /// <summary>
        /// Binding property for the user's current heading, in degrees
        /// </summary>
        public static readonly BindableProperty HeadingProperty =
            BindableProperty.Create(
                propertyName: nameof(Heading),
                returnType: typeof(int?),
                declaringType: typeof(CompassView),
                defaultValue: null,
                propertyChanged: InvalidateSurfaceOnChange);

        /// <summary>
        /// Binding property for the set target direction, in degrees
        /// </summary>
        public static readonly BindableProperty TargetDirectionProperty =
            BindableProperty.Create(
                propertyName: nameof(TargetDirection),
                returnType: typeof(int?),
                declaringType: typeof(CompassView),
                defaultValue: null,
                propertyChanged: InvalidateSurfaceOnChange);

        /// <summary>
        /// Binding property for the direction angle of today's sunrise, in degrees
        /// </summary>
        public static readonly BindableProperty SunriseDirectionProperty =
            BindableProperty.Create(
                propertyName: nameof(SunriseDirection),
                returnType: typeof(int?),
                declaringType: typeof(CompassView),
                defaultValue: null,
                propertyChanged: InvalidateSurfaceOnChange);

        /// <summary>
        /// Binding property for the direction angle of today's sunset, in degrees
        /// </summary>
        public static readonly BindableProperty SunsetDirectionProperty =
            BindableProperty.Create(
                propertyName: nameof(SunsetDirection),
                returnType: typeof(int?),
                declaringType: typeof(CompassView),
                defaultValue: null,
                propertyChanged: InvalidateSurfaceOnChange);

        /// <summary>
        /// Binding property specifying the compass color
        /// </summary>
        public static readonly BindableProperty CompassColorProperty =
            BindableProperty.Create(
                propertyName: nameof(CompassColor),
                returnType: typeof(Color),
                declaringType: typeof(CompassView),
                defaultValue: Color.White,
                validateValue: (_, value) => value != null,
                propertyChanged: InvalidateSurfaceOnChange);

        /// <summary>
        /// Binding property specifying the target direction color
        /// </summary>
        public static readonly BindableProperty TargetDirectionColorProperty =
            BindableProperty.Create(
                propertyName: nameof(TargetDirectionColor),
                returnType: typeof(Color),
                declaringType: typeof(CompassView),
                defaultValue: Color.Red,
                validateValue: (_, value) => value != null,
                propertyChanged: InvalidateSurfaceOnChange);

        /// <summary>
        /// Binding property specifying the sun direction color
        /// </summary>
        public static readonly BindableProperty SunDirectionColorProperty =
            BindableProperty.Create(
                propertyName: nameof(SunDirectionColor),
                returnType: typeof(Color),
                declaringType: typeof(CompassView),
                defaultValue: Color.Yellow,
                validateValue: (_, value) => value != null,
                propertyChanged: InvalidateSurfaceOnChange);
        #endregion

        #region View properties
        /// <summary>
        /// Heading angle, in degrees; when not set, the compass uses North as heading.
        /// </summary>
        public int? Heading
        {
            get => (int?)this.GetValue(HeadingProperty);
            set => this.SetValue(HeadingProperty, value);
        }

        /// <summary>
        /// Target direction angle, in degrees; when not set, the compass doesn't show a target
        /// direction.
        /// </summary>
        public int? TargetDirection
        {
            get => (int?)this.GetValue(TargetDirectionProperty);
            set => this.SetValue(TargetDirectionProperty, value);
        }

        /// <summary>
        /// Direction angle for today's sunrise, in degrees; when not set, no sunrise icon is
        /// shown.
        /// </summary>
        public int? SunriseDirection
        {
            get => (int?)this.GetValue(SunriseDirectionProperty);
            set => this.SetValue(SunriseDirectionProperty, value);
        }

        /// <summary>
        /// Direction angle for today's sunset, in degrees; when not set, no sunset icon is shown.
        /// </summary>
        public int? SunsetDirection
        {
            get => (int?)this.GetValue(SunsetDirectionProperty);
            set => this.SetValue(SunsetDirectionProperty, value);
        }

        /// <summary>
        /// Color for the compass lines
        /// </summary>
        public Color CompassColor
        {
            get => (Color)this.GetValue(CompassColorProperty);
            set => this.SetValue(CompassColorProperty, value);
        }

        /// <summary>
        /// Color for the target direction line
        /// </summary>
        public Color TargetDirectionColor
        {
            get => (Color)this.GetValue(TargetDirectionColorProperty);
            set => this.SetValue(TargetDirectionColorProperty, value);
        }

        /// <summary>
        /// Color for the sun direction line
        /// </summary>
        public Color SunDirectionColor
        {
            get => (Color)this.GetValue(SunDirectionColorProperty);
            set => this.SetValue(SunDirectionColorProperty, value);
        }
        #endregion

        #region Drawing
        /// <summary>
        /// Called when the surface has to be painted
        /// </summary>
        /// <param name="e">event args</param>
        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            SKCanvas canvas = e.Surface.Canvas;

            canvas.Clear();

            float xy = Math.Min(e.Info.Width, e.Info.Height) / 2.0f;
            float strokeWidth = xy / 20.0f;

            float margin = xy * 0.1f;
            float radius = xy - margin - (strokeWidth / 2.0f);

            var center = new SKPoint(xy, xy);

            if (this.Heading.HasValue)
            {
                canvas.RotateDegrees(-this.Heading.Value, xy, xy);
            }

            this.DrawDegreesCircle(canvas, center, radius);

            this.DrawCompassRose(canvas, center, radius, strokeWidth);

            if (this.SunriseDirection.HasValue || this.SunsetDirection.HasValue)
            {
                this.DrawSunPositions(canvas, center, radius);
            }

            if (this.TargetDirection.HasValue)
            {
                this.DrawTargetDirection(canvas, center, radius);
            }
        }

        /// <summary>
        /// Draws the outer "degrees" circle, with lines every degree, longer lines every 5
        /// degrees and thick lines every 90 degrees.
        /// </summary>
        /// <param name="canvas">canvas to draw on</param>
        /// <param name="center">compass center point</param>
        /// <param name="radius">compass radius</param>
        private void DrawDegreesCircle(SKCanvas canvas, SKPoint center, float radius)
        {
            using var outlineThickPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3.0f,
                Color = this.CompassColor.ToSKColor(),
            };

            var filledDegreesPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                StrokeWidth = 1.0f,
                Color = this.CompassColor.ToSKColor(),
            };

            var filledThickDegreesPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                StrokeWidth = 3.0f,
                Color = this.CompassColor.ToSKColor(),
            };

            var textDegreesPaint = new SKPaint
            {
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                TextSize = radius * 0.06f,
                Style = SKPaintStyle.Fill,
                Color = this.CompassColor.ToSKColor(),
            };

            canvas.DrawCircle(center, 0.93f * radius, outlineThickPaint);

            float arcRadius = radius * 0.70f;
            var arcRect = new SKRect(
                center.X - arcRadius,
                center.Y - arcRadius,
                center.X + arcRadius,
                center.Y + arcRadius);

            for (int angle = 0; angle < 360; angle++)
            {
                float endRadius = ((angle % 5) == 0 ? 0.83f : 0.86f) * radius;

                SKPaint linePaint = (angle % 90) != 0
                    ? filledDegreesPaint
                    : filledThickDegreesPaint;

                canvas.DrawLine(
                    center.X,
                    center.Y - (0.93f * radius),
                    center.X,
                    center.Y - endRadius,
                    linePaint);

                if ((angle % 10) == 0 && (angle % 90) != 0)
                {
                    canvas.DrawText(
                        $"{angle}",
                        center.X,
                        center.Y - (0.76f * radius),
                        textDegreesPaint);
                }

                if ((angle % 45) == 0)
                {
                    canvas.DrawArc(arcRect, 5.0f, 35.0f, false, outlineThickPaint);
                }

                canvas.RotateDegrees(1.0f, center.X, center.Y);
            }
        }

        /// <summary>
        /// Text strings for all main wind directions
        /// </summary>
        private static readonly string[] MainDirectionNames = new string[4]
        {
            "N", "E", "S", "W",
        };

        /// <summary>
        /// Text strings for all "between" wind directions
        /// </summary>
        private static readonly string[] BetweenDirectionNames = new string[4]
        {
            "NE", "SE", "SW", "NW",
        };

        /// <summary>
        /// Draws compass circle, including border and filled background
        /// </summary>
        /// <param name="canvas">canvas to draw on</param>
        /// <param name="center">center point of compass</param>
        /// <param name="radius">radius of compass</param>
        /// <param name="strokeWidth">stroke width of circle border</param>
        private void DrawCompassRose(SKCanvas canvas, SKPoint center, float radius, float strokeWidth)
        {
            using var filledPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                StrokeWidth = 3.0f,
                Color = this.CompassColor.ToSKColor(),
            };

            using var outlinePaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3.0f,
                Color = this.CompassColor.ToSKColor(),
            };

            using var textLargeSpirePaint = new SKPaint
            {
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                TextSize = radius * 0.2f,
                Style = SKPaintStyle.StrokeAndFill,
                Color = this.CompassColor.ToSKColor(),
            };

            using var textSmallSpirePaint = new SKPaint
            {
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                TextSize = radius * 0.12f,
                Style = SKPaintStyle.Fill,
                Color = this.CompassColor.ToSKColor(),
            };

            // first draw the "between" direction spires and names, at 45 degrees rotated
            float spireRadius = 0.8f * radius;

            // This angle is determined by the triangle that spans the large spire.
            // Calculated with https://www.calculator.net/triangle-calculator.html
            // angle A = 45°, side b = outerRadius, side c = innerRadius
            // solution is angle C
            const double smallSpireAngle = 12.119;

            canvas.RotateDegrees(45.0f, center.X, center.Y);

            this.DrawCompassRoseSpires(
                canvas,
                center,
                filledPaint,
                outlinePaint,
                textSmallSpirePaint,
                0.2f * spireRadius,
                0.1f * spireRadius,
                0.9f * spireRadius,
                0.95f * radius,
                45 - smallSpireAngle,
                BetweenDirectionNames);

            canvas.RotateDegrees(-45.0f, center.X, center.Y);

            // and then draw the main direction spires and names
            this.DrawCompassRoseSpires(
                canvas,
                center,
                filledPaint,
                outlinePaint,
                textLargeSpirePaint,
                0.0f,
                0.2f * spireRadius,
                spireRadius,
                0.95f * radius,
                45,
                MainDirectionNames);

            canvas.RotateDegrees(90.0f, center.X, center.Y);
        }

        /// <summary>
        /// Draws four compass spires on the canvas. The spires consist of a filled and an outline
        /// part. On top of each spire, a direction name letter is drawn.
        /// </summary>
        /// <param name="canvas">canvas to draw on</param>
        /// <param name="center">compass center point</param>
        /// <param name="filledPaint">paint for the filled part of the spire</param>
        /// <param name="outlinePaint">paint for the outline part of the spire</param>
        /// <param name="textPaint">paint for the text of direction names</param>
        /// <param name="startRadius">radius where the spire starts</param>
        /// <param name="innerRadius">radius of the obtuse angle part of the spire</param>
        /// <param name="outerRadius">radius where the spire ends</param>
        /// <param name="textRadius">radius where direction letter text is drawn</param>
        /// <param name="sideAngleDegrees">angle of the side of the obtuse angle part</param>
        /// <param name="directionNames">four direction names</param>
        private void DrawCompassRoseSpires(
            SKCanvas canvas,
            SKPoint center,
            SKPaint filledPaint,
            SKPaint outlinePaint,
            SKPaint textPaint,
            float startRadius,
            float innerRadius,
            float outerRadius,
            float textRadius,
            double sideAngleDegrees,
            string[] directionNames)
        {
            double sideAngleRadians = sideAngleDegrees.ToRadians();
            float deltaSideX = (float)Math.Cos(sideAngleRadians) * innerRadius;
            float deltaSideY = (float)Math.Sin(sideAngleRadians) * innerRadius;

            var filledPath = new SKPath();
            filledPath.MoveTo(center.X, center.Y - startRadius);
            filledPath.LineTo(center.X, center.Y - outerRadius);
            filledPath.LineTo(center.X + deltaSideX, center.Y - startRadius - deltaSideY);
            filledPath.Close();

            var outlinePath = new SKPath();
            outlinePath.MoveTo(center.X, center.Y - startRadius);
            outlinePath.LineTo(center.X, center.Y - outerRadius);
            outlinePath.LineTo(center.X - deltaSideX, center.Y - startRadius - deltaSideY);
            outlinePath.Close();

            for (int angle = 0; angle < 360; angle += 90)
            {
                canvas.DrawPath(filledPath, filledPaint);
                canvas.DrawPath(outlinePath, outlinePaint);

                canvas.DrawText(
                    directionNames[angle / 90],
                    center.X,
                    center.Y - textRadius,
                    textPaint);

                canvas.RotateDegrees(90.0f, center.X, center.Y);
            }
        }

        /// <summary>
        /// Draws the sun positions when set
        /// </summary>
        /// <param name="canvas">canvas to draw on</param>
        /// <param name="center">compass center point</param>
        /// <param name="radius">compass radius</param>
        private void DrawSunPositions(SKCanvas canvas, SKPoint center, float radius)
        {
            var sunPositionPaint = new SKPaint
            {
                IsAntialias = true,
                PathEffect = SKPathEffect.CreateDash(
                    new float[2]
                    {
                        0.02f * radius,
                        0.02f * radius,
                    },
                    0),
                Style = SKPaintStyle.Fill,
                Color = this.SunDirectionColor.ToSKColor(),
                StrokeWidth = 4.0f,
                StrokeCap = SKStrokeCap.Round,
            };

            if (this.SunriseDirection.HasValue)
            {
                DrawPolarLine(
                    canvas,
                    center,
                    radius,
                    this.SunriseDirection.Value,
                    sunPositionPaint);
            }

            if (this.SunsetDirection.HasValue)
            {
                DrawPolarLine(
                    canvas,
                    center,
                    radius,
                    this.SunsetDirection.Value,
                    sunPositionPaint);
            }
        }

        /// <summary>
        /// Draws the target direction when set
        /// </summary>
        /// <param name="canvas">canvas to draw on</param>
        /// <param name="center">compass center point</param>
        /// <param name="radius">compass radius</param>
        private void DrawTargetDirection(SKCanvas canvas, SKPoint center, float radius)
        {
            var targetDirectionPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = this.TargetDirectionColor.ToSKColor(),
                StrokeWidth = 4.0f,
                StrokeCap = SKStrokeCap.Round,
            };

            DrawPolarLine(
                canvas,
                center,
                radius,
                this.TargetDirection.Value,
                targetDirectionPaint);
        }

        /// <summary>
        /// Draws a line defined from compass center using polar coordinates
        /// </summary>
        /// <param name="canvas">canvas to draw on</param>
        /// <param name="center">compass center point</param>
        /// <param name="radius">compass radius</param>
        /// <param name="angleInDegrees">direction angle, in degrees</param>
        /// <param name="paint">paint to use</param>
        private static void DrawPolarLine(
            SKCanvas canvas,
            SKPoint center,
            float radius,
            double angleInDegrees,
            SKPaint paint)
        {
            double angleInRadians = (180.0 - angleInDegrees).ToRadians();

            var endPoint = new SKPoint(
                center.X + (radius * (float)Math.Cos(angleInRadians)),
                center.Y - (radius * (float)Math.Sin(angleInRadians)));

            canvas.DrawLine(center, endPoint, paint);

            canvas.DrawCircle(endPoint, radius * 0.02f, paint);
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
            var view = (CompassView)bindable;

            // avoid unnecessary invalidation
            if (oldValue != newValue)
            {
                view.InvalidateSurface();
            }
        }
        #endregion
    }
}

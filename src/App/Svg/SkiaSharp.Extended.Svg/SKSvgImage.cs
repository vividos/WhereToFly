﻿namespace SkiaSharp.Extended.Svg
{
	internal struct SKSvgImage
	{
		public SKSvgImage(SKRect rect, string uri, byte[] bytes = null)
		{
			Rect = rect;
			Uri = uri;
			Bytes = bytes;
		}

		public SKRect Rect { get; }

		public string Uri { get; }

		public byte[] Bytes { get; }
	}
}

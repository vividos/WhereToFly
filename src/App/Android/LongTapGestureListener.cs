using Android.Runtime;
using Android.Views;
using System;

namespace WhereToFly.App.Android
{
    /// <summary>
    /// A gesture listener class that only listens on long taps
    /// </summary>
    internal class LongTapGestureListener : GestureDetector.SimpleOnGestureListener
    {
        /// <summary>
        /// Event handler that is called when a long tap occured
        /// </summary>
        public event EventHandler<GestureDetector.LongPressEventArgs> LongTap;

        /// <summary>
        /// Default ctor
        /// </summary>
        public LongTapGestureListener()
        {
        }

        /// <summary>
        /// Java copy ctor
        /// </summary>
        /// <param name="javaReference">java reference</param>
        /// <param name="transfer">transfer ownership</param>
        protected LongTapGestureListener(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        /// <summary>
        /// Called when a long tap (or long press) is detected
        /// </summary>
        /// <param name="e">motion event arg</param>
        public override void OnLongPress(MotionEvent e)
        {
            base.OnLongPress(e);
            this.LongTap?.Invoke(this, new GestureDetector.LongPressEventArgs(e));
        }
    }
}

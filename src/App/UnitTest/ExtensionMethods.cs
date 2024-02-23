using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// Extension methods for unit tests
    /// </summary>
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Waits for a PropertyChanged event from view model implementing INotifyPropertyChanged.
        /// Waits maximum time span before returning.
        /// </summary>
        /// <param name="viewModel">view model to wait on</param>
        /// <param name="propertyName">name of property that has to change</param>
        /// <param name="maximumWaitTime">maximum wait time for property to change</param>
        /// <returns>true when property was changed, or false when not</returns>
        internal static bool WaitForPropertyChange(
            this INotifyPropertyChanged viewModel,
            string propertyName,
            TimeSpan maximumWaitTime)
        {
            var tcs = new TaskCompletionSource<bool>();
            var cts = new CancellationTokenSource();

            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == propertyName)
                {
                    tcs.TrySetResult(true);
                    cts.Cancel();
                }
            };

            Task.WhenAny(
                tcs.Task,
                Task.Run(async () =>
                {
                    await Task.Delay(maximumWaitTime, cts.Token);

                    if (!cts.IsCancellationRequested)
                    {
                        tcs.TrySetResult(false);
                    }
                }));

            bool result = tcs.Task.Result;

            cts.Dispose();

            return result;
        }
    }
}

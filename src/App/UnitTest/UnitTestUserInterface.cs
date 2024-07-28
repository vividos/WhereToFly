using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhereToFly.App.Services;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// User interface implementation for unit tests
    /// </summary>
    internal class UnitTestUserInterface : IUserInterface
    {
        /// <inheritdoc />
        public AppTheme UserAppTheme { get; set; } = AppTheme.Unspecified;

        /// <inheritdoc />
        public bool IsDarkTheme => this.UserAppTheme == AppTheme.Dark;

        /// <inheritdoc />
        public NavigationService NavigationService
            => throw new NotImplementedException();

        /// <summary>
        /// Result returned by call to <see cref="DisplayAlert(string, string, string)"/>
        /// </summary>
        public bool AlertResult { get; set; } = true;

        /// <summary>
        /// Result returned by call to <see cref="DisplayActionSheet"/>. Wen set to -1, returns the
        /// 'cancel' action text, and for -2 returns the 'destruction' action text.
        /// </summary>
        public int ActionSheetResult { get; set; } = -1;

        /// <inheritdoc />
        public void DisplayToast(string message)
        {
            Debug.WriteLine($"Displaying toast: {message}");
        }

        /// <inheritdoc />
        public Task DisplayAlert(string message, string cancel)
        {
            Debug.WriteLine($"Displaying alert: {message} [{cancel}]");
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<bool> DisplayAlert(string message, string accept, string cancel)
        {
            Debug.WriteLine($"Displaying alert: {message} [{accept}] [{cancel}]");
            Debug.WriteLine($"Pressing button [{(this.AlertResult ? accept : cancel)}]");

            return Task.FromResult(this.AlertResult);
        }

        /// <inheritdoc />
        public Task<string> DisplayActionSheet(
            string title,
            string? cancel,
            string? destruction,
            params string[] buttons)
        {
            if (cancel == null &&
                this.ActionSheetResult == -1)
            {
                throw new ArgumentException(
                    "no 'cancel' text was provided!",
                    nameof(cancel));
            }

            if (destruction == null &&
                this.ActionSheetResult == -2)
            {
                throw new ArgumentException(
                    "no 'destruction' text was provided!",
                    nameof(destruction));
            }

            string choices =
                string.Join(
                    ", ",
                    buttons.Select(
                        text => $"{Array.IndexOf(buttons, text)}. {text}"));

            if (cancel != null)
            {
                choices += $", -1. {cancel}";
            }

            if (destruction != null)
            {
                choices += $", -2. {destruction}";
            }

            Debug.WriteLine($"Displaying action sheet: {title} {choices}");

#pragma warning disable S3358 // Ternary operators should not be nested
            string result =
                this.ActionSheetResult == -2
                ? destruction!
                : this.ActionSheetResult == -1 || buttons.Length >= this.ActionSheetResult
                ? cancel!
                : buttons[this.ActionSheetResult];
#pragma warning restore S3358 // Ternary operators should not be nested

            Debug.WriteLine($"Choosing: {result}");

            return Task.FromResult(result);
        }
    }
}

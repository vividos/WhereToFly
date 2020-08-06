using System.Collections.Generic;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Styles
{
    /// <summary>
    /// Dark theme style
    /// </summary>
    public partial class DarkTheme : ResourceDictionary
    {
        /// <summary>
        /// Creates a new dark theme resource dictionary
        /// </summary>
        public DarkTheme()
        {
            this.InitializeComponent();

            this["SvgImageFillDark"] = new Dictionary<string, string>
            {
                { "fill=\"#000000", "fill=\"#ffffff" }
            };
        }
    }
}

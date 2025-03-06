using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Services;

namespace WhereToFly.WebApi.UnitTest
{
    /// <summary>
    /// Tests for the web API controllers using the <see cref="BackendDataService"/> from the app.
    /// </summary>
    [TestClass]
    public class BackendDataServiceTest
    {
        /// <summary>
        /// Tests GetAppConfigAsync() method
        /// </summary>
        /// <returns>task to wait on</returns>
        [TestMethod]
        public async Task TestGetAppConfigAsync()
        {
            // set up
            var factory = new WebApplicationFactory<Program>();
            HttpClient client = factory.CreateClient();

            var service = new BackendDataService(client);

            // run
            var appConfig = await service.GetAppConfigAsync(
                "1.234.5");

            // check
            Assert.AreNotEqual(
                DateTimeOffset.MinValue,
                appConfig.ExpiryDate,
                "expiry date must not be min value");
        }
    }
}

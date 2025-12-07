using TaskManagement.E2E.Tests.Pages;

namespace TaskManagement.E2E.Tests
{
    [TestClass]
    public class HomePageTests : PageTest
    {
        // Base URL for the application
        private const string BaseUrl = "http://localhost:5244";

        [TestMethod]
        public async Task HomePageLoadsSuccessfully()
        {
            // Create a HomePage object using the Playwright Page instance
            var homePage = new HomePage(Page);

            // Navigate to the home page
            await homePage.GoToAsync(BaseUrl);

            // Wait until the page and main elements are fully loaded
            await homePage.WaitForLoadAsync();

            // Verify that the page title matches expected
            Assert.AreEqual("Task Manager", await homePage.GetTitleAsync());

            // Verify that the login button is visible
            Assert.IsTrue(await homePage.LoginButton.IsVisibleAsync());
        }
    }
}

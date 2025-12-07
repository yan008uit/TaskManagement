using Microsoft.Playwright;
using TaskManagement.E2E.Tests.Pages;

namespace TaskManagement.E2E.Tests
{
    [TestClass]
    public class LoginPageTests : PageTest
    {
        // URL of the login page
        private const string LoginUrl = "http://localhost:5244/login";

        [TestMethod]
        public async Task CanLoginSuccessfully()
        {
            // Initialize page objects
            var loginPage = new LoginPage(Page);
            var dashboardPage = new DashboardPage(Page);

            // Navigate to login page
            await loginPage.GoToAsync(LoginUrl);

            // Perform login with valid credentials
            await loginPage.LoginAsync("Yuri", "Pass123!");

            // Wait for the dashboard page to fully load
            await dashboardPage.WaitForLoadAsync();

            // Verify the heading is "Dashboard"
            var heading = await dashboardPage.GetHeadingTextAsync();
            Assert.IsTrue(heading.Equals("Dashboard", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public async Task CannotLoginWithInvalidCredentials()
        {
            var loginPage = new LoginPage(Page);

            // Navigate to login page
            await loginPage.GoToAsync(LoginUrl);

            // Attempt login with empty fields (no navigation expected)
            await loginPage.LoginAsync("", "", expectNavigation: false);

            // Wait for error message to appear
            var errorLocator = Page.Locator(".alert-danger");
            await errorLocator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 5000
            });

            // Get and verify error message
            var error = await errorLocator.InnerTextAsync();
            Assert.IsTrue(
                error.Contains("Please fill in all fields.", StringComparison.OrdinalIgnoreCase),
                $"Expected error message but got: '{error}'"
            );
        }

        [TestMethod]
        public async Task CannotLoginWithWrongCredentials()
        {
            var loginPage = new LoginPage(Page);

            // Navigate to login page
            await loginPage.GoToAsync(LoginUrl);

            // Attempt login with incorrect credentials
            await loginPage.LoginAsync("WrongUser", "WrongPass", expectNavigation: false);

            // Wait for error message to appear
            var errorLocator = Page.Locator(".alert.alert-danger");
            await errorLocator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 5000
            });

            // Get and verify error message
            var errorText = await errorLocator.InnerTextAsync();
            Assert.IsFalse(string.IsNullOrEmpty(errorText), "Error message not found.");
            Assert.IsTrue(
                errorText.Contains("Invalid username or password", StringComparison.OrdinalIgnoreCase),
                $"Expected error message, but got: '{errorText}'"
            );
        }
    }
}
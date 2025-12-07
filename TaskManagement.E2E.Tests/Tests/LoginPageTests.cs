using Microsoft.Playwright;
using TaskManagement.E2E.Tests.Pages;

namespace TaskManagement.E2E.Tests
{
    [TestClass]
    public class LoginPageTests : PageTest
    {
        private const string LoginUrl = "http://localhost:5244/login";

        [TestMethod]
        public async Task CanLoginSuccessfully()
        {
            var loginPage = new LoginPage(Page);
            var dashboardPage = new DashboardPage(Page);

            await loginPage.GoToAsync(LoginUrl);
            await loginPage.LoginAsync("Yuri", "Pass123!");

            await dashboardPage.WaitForLoadAsync();

            var heading = await dashboardPage.GetHeadingTextAsync();
            Assert.IsTrue(heading.Equals("Dashboard", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public async Task CannotLoginWithInvalidCredentials()
        {
            var loginPage = new LoginPage(Page);

            await loginPage.GoToAsync(LoginUrl);
            await loginPage.LoginAsync("", "", expectNavigation: false);

            var error = await loginPage.GetErrorMessageAsync();
            Assert.Contains("Please fill in all fields.", error!);
        }

        [TestMethod]
        public async Task CannotLoginWithWrongCredentials()
        {
            var loginPage = new LoginPage(Page);

            await loginPage.GoToAsync(LoginUrl);
            await loginPage.LoginAsync("WrongUser", "WrongPass", expectNavigation: false);

            var errorLocator = Page.Locator(".alert.alert-danger");

            await errorLocator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 5000
            });

            var errorText = await errorLocator.InnerTextAsync();

            Assert.IsFalse(string.IsNullOrEmpty(errorText), "Error message not found.");
            Assert.IsTrue(
                errorText.Contains("Invalid username or password", StringComparison.OrdinalIgnoreCase),
                $"Expected error message, but got: '{errorText}'"
            );
        }
    }
}
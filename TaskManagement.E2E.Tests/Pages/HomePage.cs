using Microsoft.Playwright;

namespace TaskManagement.E2E.Tests.Pages
{
    // Page object representing the Home page in the E2E tests.
    // Inherits from BasePage and provides locators and actions specific to the home page.
    public class HomePage : BasePage
    {
        // Constructor to initialize the page object with an IPage instance.
        public HomePage(IPage page) : base(page) { }

        // Login button on the home page
        public ILocator LoginButton => Page.GetByRole(AriaRole.Button, new() { Name = "Login" });

        /// <summary>
        /// Waits until the home page is fully loaded.
        /// Checks that the page title is correct and the login button is visible.
        /// </summary>
        public async Task WaitForLoadAsync()
        {
            // Wait until the document title matches "Task Manager"
            await Page.WaitForFunctionAsync(@"() => document.title === 'Task Manager'");

            // Wait until the login button is visible
            await LoginButton.WaitForAsync();
        }
    }
}
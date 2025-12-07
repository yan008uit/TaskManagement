using Microsoft.Playwright;

namespace TaskManagement.E2E.Tests.Pages
{
    // Page object representing the Login page in the E2E tests.
    // Provides locators and actions for username/password login functionality.
    public class LoginPage
    {
        // Playwright IPage instance for interacting with the browser page.
        private readonly IPage _page;

        // Centralized selectors for the login form
        private readonly ILocator _usernameField;
        private readonly ILocator _passwordField;
        private readonly ILocator _submitButton;
        private readonly ILocator _errorMessage;

        // Constructor to initialize the page object with an IPage instance and locators
        public LoginPage(IPage page)
        {
            _page = page;

            _usernameField = _page.Locator("input[placeholder='Username']");
            _passwordField = _page.Locator("input[placeholder='Password']");
            _submitButton = _page.Locator("button:has-text('Login')");
            _errorMessage = _page.Locator(".alert.alert-danger");
        }

        /// <summary>
        /// Navigates to the login page and waits until the username field is visible.
        /// </summary>
        /// <param name="url">The URL of the login page.</param>
        public async Task GoToAsync(string url)
        {
            await _page.GotoAsync(url);

            // Ensure login page is fully loaded
            await _usernameField.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 30000
            });
        }

        /// <summary>
        /// Performs a login action by filling username/password and clicking submit.
        /// Optionally waits for navigation to the projects page.
        /// </summary>
        /// <param name="username">Username to enter.</param>
        /// <param name="password">Password to enter.</param>
        /// <param name="expectNavigation">Whether a redirect is expected after login.</param>
        public async Task LoginAsync(string username, string password, bool expectNavigation = true)
        {
            await _usernameField.FillAsync(username);
            await _passwordField.FillAsync(password);

            if (expectNavigation)
            {
                // Click the submit button and wait for redirect to /projects
                await Task.WhenAll(
                    _page.WaitForURLAsync("**/projects", new PageWaitForURLOptions { Timeout = 60000 }),
                    _submitButton.ClickAsync()
                );
            }
            else
            {
                // Just click without waiting for navigation
                await _submitButton.ClickAsync();
            }
        }

        // Exposes the error message locator publicly
        public ILocator ErrorMessage => _errorMessage;

        /// <summary>
        /// Returns the error message text if visible, otherwise null.
        /// </summary>
        public async Task<string?> GetErrorMessageAsync()
        {
            if (!await _errorMessage.IsVisibleAsync())
                return null;

            return await _errorMessage.InnerTextAsync();
        }

        /// <summary>
        /// Checks whether an error message is currently displayed.
        /// </summary>
        public async Task<bool> IsErrorDisplayedAsync()
        {
            return await _errorMessage.IsVisibleAsync();
        }
    }
}
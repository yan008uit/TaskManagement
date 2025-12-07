using Microsoft.Playwright;

namespace TaskManagement.E2E.Tests.Pages
{
    public class LoginPage
    {
        private readonly IPage _page;

        // Centralized selectors
        private readonly ILocator _usernameField;
        private readonly ILocator _passwordField;
        private readonly ILocator _submitButton;
        private readonly ILocator _errorMessage;

        public LoginPage(IPage page)
        {
            _page = page;

            _usernameField = _page.Locator("input[placeholder='Username']");
            _passwordField = _page.Locator("input[placeholder='Password']");
            _submitButton = _page.Locator("button:has-text('Login')");
            _errorMessage = _page.Locator(".alert.alert-danger");
        }

        public async Task GoToAsync(string url)
        {
            await _page.GotoAsync(url);

            // Ensure login page is loaded
            await _usernameField.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 30000
            });
        }

        public async Task LoginAsync(string username, string password, bool expectNavigation = true)
        {
            await _usernameField.FillAsync(username);
            await _passwordField.FillAsync(password);

            if (expectNavigation)
            {
                // Wait for success redirect to /projects
                await Task.WhenAll(
                    _page.WaitForURLAsync("**/projects", new PageWaitForURLOptions { Timeout = 60000 }),
                    _submitButton.ClickAsync()
                );
            }
            else
            {
                // Just click, no redirect expected
                await _submitButton.ClickAsync();
            }
        }

        public ILocator ErrorMessage => _errorMessage;

        public async Task<string?> GetErrorMessageAsync()
        {
            if (!await _errorMessage.IsVisibleAsync())
                return null;

            return await _errorMessage.InnerTextAsync();
        }

        public async Task<bool> IsErrorDisplayedAsync()
        {
            return await _errorMessage.IsVisibleAsync();
        }
    }
}
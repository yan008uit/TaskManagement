using Microsoft.Playwright;

namespace TaskManagement.E2E.Tests.Pages
{
    // Abstract base class for all page objects in the E2E tests.
    // Provides common functionality like navigation and title retrieval.
    public abstract class BasePage
    {
        // Playwright IPage instance for interacting with the browser page.
        protected readonly IPage Page;

        // Constructor to initialize the page object with an IPage instance.
        protected BasePage(IPage page) => Page = page;

        /// <summary>
        /// Navigates to the specified URL and waits until network activity is idle.
        /// </summary>
        /// <param name="url">The URL to navigate to.</param>
        public virtual async Task GoToAsync(string url)
        {
            await Page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        }

        /// <summary>
        /// Retrieves the title of the current page.
        /// </summary>
        /// <returns>The page title as a string.</returns>
        public async Task<string> GetTitleAsync() => await Page.TitleAsync();
    }
}
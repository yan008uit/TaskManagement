using Microsoft.Playwright;
using TaskManagement.E2E.Tests.Pages;

namespace TaskManagement.E2E.Tests
{
    [TestClass]
    public class CreateTasksPageTests
    {
        // URLs and credentials
        private const string LoginUrl = "http://localhost:5244/login";
        private const string CreateTaskUrl = "http://localhost:5244/tasks/create";
        private const string Username = "Yuri";
        private const string Password = "Pass123!";

        // Playwright browser, context, and page
        private IBrowser? _browser;
        private IBrowserContext? _context;
        private IPage? _page;

        [TestInitialize]
        public async Task Init()
        {
            // Launch Playwright and create browser/context/page
            var playwright = await Playwright.CreateAsync();
            _browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            _context = await _browser.NewContextAsync();
            _page = await _context.NewPageAsync();

            // Perform login
            var loginPage = new LoginPage(_page);
            await loginPage.GoToAsync(LoginUrl);
            await loginPage.LoginAsync(Username, Password);

            // Wait for redirect to projects dashboard
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            // Close context and browser after tests
            if (_context != null) await _context.CloseAsync();
            if (_browser != null) await _browser.CloseAsync();
        }

        [TestMethod]
        public async Task PreselectedValuesAreCorrect()
        {
            // Navigate to Add Task page
            await _page!.GotoAsync(CreateTaskUrl);
            await _page.Locator("h3.page-title:has-text('Add Task')").WaitForAsync();

            // Verify default selected user
            var assignedUser = await _page.Locator("#assignedUser").InputValueAsync();
            Assert.IsFalse(string.IsNullOrEmpty(assignedUser), "Expected a preselected user.");

            // Verify default selected project
            var assignedProject = await _page.Locator("#assignedProject").InputValueAsync();
            Assert.IsFalse(string.IsNullOrEmpty(assignedProject), "Expected a preselected project.");

            // Verify default status
            var statusValue = await _page.Locator("select[name='editModel.Status']").InputValueAsync();
            Assert.AreEqual("ToDo", statusValue, "Expected default status to be 'ToDo'.");
        }

        [TestMethod]
        public async Task CanCreateTaskWithCustomSelections()
        {
            // Navigate to Add Task page
            await _page!.GotoAsync(CreateTaskUrl);
            await _page.Locator("h3.page-title:has-text('Add Task')").WaitForAsync();

            // Fill title and description
            await _page.FillAsync("input[placeholder='Enter task title']", "Custom Task");
            await _page.FillAsync("textarea[placeholder='Enter task description']", "Custom description.");

            // Select 2nd user
            var userSelect = _page.Locator("#assignedUser");
            var secondUserValue = await userSelect.Locator("option").Nth(1).GetAttributeAsync("value");
            await userSelect.SelectOptionAsync(new[] { secondUserValue! });

            // Select last project
            var projectSelect = _page.Locator("#assignedProject");
            var lastProjectValue = await projectSelect.Locator("option").Last.GetAttributeAsync("value");
            await projectSelect.SelectOptionAsync(new[] { lastProjectValue! });

            // Select last status
            var statusSelect = _page.Locator("select[name='editModel.Status']");
            var lastStatusValue = await statusSelect.Locator("option").Last.GetAttributeAsync("value");
            await statusSelect.SelectOptionAsync(new[] { lastStatusValue! });

            // Set due date
            await _page.FillAsync("#dueDate", DateTime.Today.AddDays(5).ToString("yyyy-MM-dd"));

            // Submit the form
            await _page.ClickAsync("button.btn-primary[type='submit']");

            // Wait for Blazor routing to complete (UI rerender)
            await _page.WaitForURLAsync("**/projects", new() { Timeout = 5000 });

            // Confirm UI update by waiting for a project card
            await _page.Locator(".project-card").First.WaitForAsync(new() { Timeout = 5000 });

            // Assert redirected URL
            Assert.Contains("/projects", _page.Url);
        }

        [TestMethod]
        public async Task CannotCreateTaskWithoutTitle()
        {
            // Navigate to Add Task page
            await _page!.GotoAsync(CreateTaskUrl);
            await _page.Locator("h3.page-title:has-text('Add Task')").WaitForAsync();

            // Fill only description
            await _page.FillAsync("textarea[placeholder='Enter task description']", "No title task.");

            // Select last user
            var userSelect = _page.Locator("#assignedUser");
            var lastUserValue = await userSelect.Locator("option").Last.GetAttributeAsync("value");
            await userSelect.SelectOptionAsync(new[] { lastUserValue! });

            // Select last project
            var projectSelect = _page.Locator("#assignedProject");
            var lastProjectValue = await projectSelect.Locator("option").Last.GetAttributeAsync("value");
            await projectSelect.SelectOptionAsync(new[] { lastProjectValue! });

            // Select 2nd status
            var statusSelect = _page.Locator("select[name='editModel.Status']");
            var secondStatusValue = await statusSelect.Locator("option").Nth(1).GetAttributeAsync("value");
            await statusSelect.SelectOptionAsync(new[] { secondStatusValue! });

            // Submit without title
            await _page.ClickAsync("button.btn-primary[type='submit']");

            // Verify validation error appears
            var errorLocator = _page.Locator("li.validation-message:has-text('Title is required')");
            await errorLocator.WaitForAsync();

            var errorMsg = await errorLocator.TextContentAsync();
            Assert.IsFalse(string.IsNullOrEmpty(errorMsg), "Expected 'Title is required.' validation message.");
        }

        [TestMethod]
        public async Task TaskCancelButtonRedirectsToProjects()
        {
            // Navigate to Add Task page
            await _page!.GotoAsync(CreateTaskUrl);
            await _page.Locator("h3.page-title:has-text('Add Task')").WaitForAsync();

            // Click cancel button
            await _page.ClickAsync("button.btn-outline-secondary[type='button']");

            // Wait for redirect to projects
            await _page.WaitForURLAsync("**/projects");

            // Assert redirected URL
            Assert.Contains("/projects", _page.Url);
        }
    }
}
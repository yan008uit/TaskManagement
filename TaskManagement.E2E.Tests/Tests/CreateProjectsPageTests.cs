using Microsoft.Playwright;
using TaskManagement.E2E.Tests.Pages;

namespace TaskManagement.E2E.Tests
{
    [TestClass]
    public class CreateProjectsPageTests
    {
        // URLs and credentials
        private const string LoginUrl = "http://localhost:5244/login";
        private const string CreateProjectUrl = "http://localhost:5244/projects/create";
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
            _browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

            _context = await _browser.NewContextAsync();
            _page = await _context.NewPageAsync();

            // Perform login
            var loginPage = new LoginPage(_page);
            await loginPage.GoToAsync(LoginUrl);
            await loginPage.LoginAsync(Username, Password);

            // Wait for redirect to projects dashboard
            await _page.WaitForURLAsync("**/projects", new PageWaitForURLOptions { Timeout = 15000 });
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            // Close context and browser after tests
            if (_context != null) await _context.CloseAsync();
            if (_browser != null) await _browser.CloseAsync();
        }

        [TestMethod]
        public async Task CanCreateProjectWithCustomText()
        {
            // Navigate to Create Project page
            await _page!.GotoAsync(CreateProjectUrl);

            // Wait until the form heading is visible
            await _page.Locator("h3.page-title:has-text('Create New Project')")
                       .WaitForAsync(new() { Timeout = 30000 });

            // Fill project name and description
            await _page.FillAsync("#projectName", "Custom Project");
            await _page.FillAsync("#projectDescription", "Custom project description.");

            // Click the create project button
            await _page.ClickAsync("#createProjectBtn");

            // Wait for redirect to dashboard
            await _page.Locator("h3.page-title:has-text('Dashboard')")
                       .WaitForAsync(new() { Timeout = 30000 });

            // Wait for first project card to confirm UI refresh
            await _page.Locator(".project-card").First.WaitForAsync(new() { Timeout = 5000 });

            // Assert the URL contains /projects
            Assert.Contains("/projects",
                _page.Url, $"Expected URL to contain '/projects' but was '{_page.Url}'");
        }

        [TestMethod]
        public async Task CannotCreateProjectWithoutTitle()
        {
            // Navigate to Create Project page
            await _page!.GotoAsync(CreateProjectUrl);

            // Wait for heading to ensure form is loaded
            await _page.Locator("h3.page-title:has-text('Create New Project')").WaitForAsync();

            // Leave project title empty, only fill description
            await _page.FillAsync("#projectDescription", "Custom project description.");

            // Click create button
            await _page.ClickAsync("#createProjectBtn");

            // Wait for validation message to appear
            var validation = _page.Locator(".validation-message:has-text('Project Name is required')");
            await validation.WaitForAsync();

            // Assert validation message is visible
            Assert.IsTrue(await validation.IsVisibleAsync(),
                "Expected validation message 'Project Name is required' not found");
        }

        [TestMethod]
        public async Task ProjectCancelButtonRedirectsToProjects()
        {
            // Navigate to Create Project page
            await _page!.GotoAsync(CreateProjectUrl);

            // Ensure form container loaded
            await _page.Locator(".card.p-4").WaitForAsync();

            var cancelBtn = _page.Locator("#cancelProjectBtn");
            await cancelBtn.WaitForAsync();

            // Click cancel button
            await cancelBtn.ClickAsync();

            // Wait until dashboard heading is visible to confirm redirect
            await _page.Locator("h3.page-title:has-text('Dashboard')")
                       .WaitForAsync(new LocatorWaitForOptions { Timeout = 15000 });

            // Assert redirect URL contains /projects
            Assert.Contains("/projects", _page.Url, $"Expected redirect to '/projects', but URL was {_page.Url}");
        }
    }
}
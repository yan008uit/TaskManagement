using Microsoft.Playwright;
using TaskManagement.E2E.Tests.Pages;

[TestClass]
public class CreateProjectsPageTests
{
    private const string LoginUrl = "http://localhost:5244/login";
    private const string CreateProjectUrl = "http://localhost:5244/projects/create";
    private const string Username = "Yuri";
    private const string Password = "Pass123!";

    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;

    [TestInitialize]
    public async Task Init()
    {
        var playwright = await Playwright.CreateAsync();
        _browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        _context = await _browser.NewContextAsync();
        _page = await _context.NewPageAsync();

        // Login
        var loginPage = new LoginPage(_page);
        await loginPage.GoToAsync(LoginUrl);
        await loginPage.LoginAsync(Username, Password);

        await _page.WaitForURLAsync("**/projects", new PageWaitForURLOptions { Timeout = 15000 });
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        if (_context != null) await _context.CloseAsync();
        if (_browser != null) await _browser.CloseAsync();
    }

    [TestMethod]
    public async Task CanCreateProjectWithCustomText()
    {
        await _page!.GotoAsync(CreateProjectUrl);

        await _page.Locator("h3.page-title:has-text('Create New Project')")
                   .WaitForAsync(new LocatorWaitForOptions { Timeout = 30000 });

        // Fill fields
        await _page.FillAsync("#projectName", "Custom Project");
        await _page.FillAsync("#projectDescription", "Custom project description.");

        // Click create button
        await _page.ClickAsync("#createProjectBtn");

        // Expect redirect
        await _page.WaitForURLAsync("**/projects");
        Assert.Contains("/projects", _page.Url);
    }

    [TestMethod]
    public async Task CannotCreateProjectWithoutTitle()
    {
        await _page!.GotoAsync(CreateProjectUrl);
        await _page.Locator("h3.page-title:has-text('Create New Project')").WaitForAsync();

        // Leave title empty
        await _page.FillAsync("#projectDescription", "Custom project description.");

        // Click create
        await _page.ClickAsync("#createProjectBtn");

        // Check validation message
        var error = await _page
            .Locator(".validation-message:has-text('Project Name is required')")
            .TextContentAsync();

        Assert.IsFalse(string.IsNullOrEmpty(error),
            "Expected validation message 'Project Name is required' not found");
    }

    [TestMethod]
    public async Task ProjectCancelButtonRedirectsToProjects()
    {
        await _page!.GotoAsync(CreateProjectUrl);

        // Ensure form loaded
        await _page.Locator(".card.p-4").WaitForAsync();

        var cancelBtn = _page.Locator("#cancelProjectBtn");
        await cancelBtn.WaitForAsync();

        // Click
        await cancelBtn.ClickAsync();

        // Expect redirect
        await _page.WaitForURLAsync("**/projects", new PageWaitForURLOptions { Timeout = 15000 });

        Assert.Contains("/projects",
_page.Url, $"Expected redirect to '/projects', but URL was {_page.Url}");
    }
}
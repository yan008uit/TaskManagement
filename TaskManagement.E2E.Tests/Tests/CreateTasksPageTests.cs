using Microsoft.Playwright;
using TaskManagement.E2E.Tests.Pages;

[TestClass]
public class CreateTasksPageTests
{
    private const string LoginUrl = "http://localhost:5244/login";
    private const string CreateTaskUrl = "http://localhost:5244/tasks/create";
    private const string Username = "Yuri";
    private const string Password = "Pass123!";

    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;

    [TestInitialize]
    public async Task Init()
    {
        var playwright = await Playwright.CreateAsync();
        _browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        _context = await _browser.NewContextAsync();
        _page = await _context.NewPageAsync();

        // Login
        var loginPage = new LoginPage(_page);
        await loginPage.GoToAsync(LoginUrl);
        await loginPage.LoginAsync(Username, Password);

        // Wait for dashboard or projects page
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        if (_context != null) await _context.CloseAsync();
        if (_browser != null) await _browser.CloseAsync();
    }

    [TestMethod]
    public async Task PreselectedValuesAreCorrect()
    {
        await _page!.GotoAsync(CreateTaskUrl);
        await _page.Locator("h3.page-title:has-text('Add Task')").WaitForAsync();

        var assignedUser = await _page.Locator("#assignedUser").InputValueAsync();
        Assert.IsFalse(string.IsNullOrEmpty(assignedUser), "Expected a preselected user.");

        var assignedProject = await _page.Locator("#assignedProject").InputValueAsync();
        Assert.IsFalse(string.IsNullOrEmpty(assignedProject), "Expected a preselected project.");

        var statusValue = await _page.Locator("select[name='editModel.Status']").InputValueAsync();
        Assert.AreEqual("ToDo", statusValue, "Expected default status to be 'ToDo'.");
    }

    [TestMethod]
    public async Task CanCreateTaskWithCustomSelections()
    {
        await _page!.GotoAsync(CreateTaskUrl);
        await _page.Locator("h3.page-title:has-text('Add Task')").WaitForAsync();

        // Title & Description
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

        // Select 2nd status
        var statusSelect = _page.Locator("select[name='editModel.Status']");
        var secondStatusValue = await statusSelect.Locator("option").Nth(1).GetAttributeAsync("value");
        await statusSelect.SelectOptionAsync(new[] { secondStatusValue! });

        // Due date
        await _page.FillAsync("#dueDate", DateTime.Today.AddDays(5).ToString("yyyy-MM-dd"));

        // Save
        await _page.ClickAsync("button.btn-primary[type='submit']");
        await _page.WaitForURLAsync("**/projects", new PageWaitForURLOptions { Timeout = 10000 });

        Assert.Contains("/projects", _page.Url);
    }

    [TestMethod]
    public async Task CannotCreateTaskWithoutTitle()
    {
        await _page!.GotoAsync(CreateTaskUrl);
        await _page.Locator("h3.page-title:has-text('Add Task')").WaitForAsync();

        // Only description filled
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

        // Check validation message
        var errorLocator = _page.Locator("li.validation-message:has-text('Title is required')");
        await errorLocator.WaitForAsync();

        var errorMsg = await errorLocator.TextContentAsync();
        Assert.IsFalse(string.IsNullOrEmpty(errorMsg), "Expected 'Title is required.' validation message.");
    }

    [TestMethod]
    public async Task TaskCancelButtonRedirectsToProjects()
    {
        await _page!.GotoAsync(CreateTaskUrl);
        await _page.Locator("h3.page-title:has-text('Add Task')").WaitForAsync();

        await _page.ClickAsync("button.btn-outline-secondary[type='button']");
        await _page.WaitForURLAsync("**/projects");

        Assert.Contains("/projects", _page.Url);
    }
}
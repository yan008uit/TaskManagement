using Microsoft.Playwright;
using TaskManagement.E2E.Tests.Pages;

[TestClass]
public class DashboardPageTests
{
    private const string LoginUrl = "http://localhost:5244/login";
    private const string DashboardUrl = "http://localhost:5244/projects";
    private const string Username = "Yuri";
    private const string Password = "Pass123!";

    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;

    [TestInitialize]
    public async Task TestInitialize()
    {
        var playwright = await Playwright.CreateAsync();
        _browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });

        _context = await _browser.NewContextAsync();
        _page = await _context.NewPageAsync();

        // Login
        var loginPage = new LoginPage(_page);
        await loginPage.GoToAsync(LoginUrl);
        await loginPage.LoginAsync(Username, Password);

        await _page.WaitForURLAsync("**/projects", new PageWaitForURLOptions { Timeout = 10000 });
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
        if (_context != null) await _context.CloseAsync();
        if (_browser != null) await _browser.CloseAsync();
    }

    private async Task<DashboardPage> GoToDashboardAsync()
    {
        // Only navigate if not already on dashboard
        if (!_page!.Url.Contains("/projects"))
        {
            await _page.GotoAsync(DashboardUrl);
        }

        var dashboardPage = new DashboardPage(_page);
        await dashboardPage.WaitForLoadAsync();
        return dashboardPage;
    }

    [TestMethod]
    public async Task DashboardLoadsCorrectlyAfterLogin()
    {
        var dashboardPage = await GoToDashboardAsync();
        var heading = await dashboardPage.GetHeadingTextAsync();
        Assert.IsTrue(heading.Contains("Dashboard", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task CanClickCreateTaskButton()
    {
        var dashboardPage = await GoToDashboardAsync();
        await dashboardPage.CreateTaskButton.WaitForAsync();

        await dashboardPage.CreateTaskButton.ClickAsync();

        await _page!.WaitForURLAsync("**/tasks/create");
        Assert.Contains("/tasks/create", _page.Url);
    }

    [TestMethod]
    public async Task CanClickCreateProjectButton()
    {
        var dashboardPage = await GoToDashboardAsync();
        await dashboardPage.CreateProjectButton.WaitForAsync();

        await dashboardPage.CreateProjectButton.ClickAsync();

        await _page!.WaitForURLAsync("**/projects/create");
        Assert.Contains("/projects/create", _page.Url);
    }

    [TestMethod]
    public async Task CanOpenTaskFromDashboard()
    {
        var dashboardPage = await GoToDashboardAsync();

        var firstTask = dashboardPage.GetFirstTask();
        if (firstTask == null)
            Assert.Inconclusive("No tasks available on dashboard.");

        var taskTitle = await firstTask!.Locator(".task-title").InnerTextAsync();

        await firstTask.ClickAsync();

        await _page!.WaitForURLAsync(url => url.Contains("/tasks/"));

        await _page.Locator("div.task-details-card").WaitForAsync();

        Assert.Contains("/tasks/", _page.Url);
    }

    [TestMethod]
    public async Task CanOpenProjectFromDashboard()
    {
        var dashboardPage = await GoToDashboardAsync();

        var firstProject = dashboardPage.GetFirstProject();
        if (firstProject == null)
            Assert.Inconclusive("No projects available on dashboard.");

        var projectName = await firstProject!.Locator(".project-name").InnerTextAsync();

        await firstProject.ClickAsync();

        await _page!.WaitForURLAsync(url => url.Contains("/projects/"));

        await _page.Locator("p.project-description").WaitForAsync();

        Assert.Contains("/projects/", _page.Url);
    }

    [TestMethod]
    public async Task CanChangeTaskStatusSuccessfully()
    {
        await _page!.GotoAsync(DashboardUrl);

        // Ensure at least one task exists
        var taskCards = _page.Locator(".task-card");
        int count = await taskCards.CountAsync();
        Assert.IsGreaterThan(0, count, "There must be at least one task to test status update.");

        // Use the first task card
        var firstCard = taskCards.First;

        // Locate status dropdown inside the card
        var statusSelect = firstCard.Locator("select.form-select-sm").First;

        await statusSelect.ScrollIntoViewIfNeededAsync();
        await statusSelect.WaitForAsync(new() { Timeout = 5000 });

        // Change status to Done
        await statusSelect.SelectOptionAsync("Done");

        // Wait a bit for Blazor server update + UI refresh
        await _page.WaitForTimeoutAsync(500);

        // Verify dropdown now has value="Done"
        string value = await statusSelect.InputValueAsync();

        Assert.AreEqual("Done", value, "Task status did not update to Done.");
    }

    [TestMethod]
    public async Task CanDeleteProjectSuccessfully()
    {
        const string testProjectName = "AutoDeleteTestProject";

        await _page!.GotoAsync(DashboardUrl);

        // Create project if not exists
        bool exists = await _page.Locator($"h5.project-name:has-text('{testProjectName}')").CountAsync() > 0;

        if (!exists)
        {
            await _page.GotoAsync("http://localhost:5244/projects/create");
            await _page.FillAsync("#projectName", testProjectName);
            await _page.FillAsync("#projectDescription", "Temporary project for delete test");
            await _page.ClickAsync("#createProjectBtn");
            await _page.WaitForURLAsync("**/projects");
        }

        // Locate the project card
        var targetCard = _page.Locator(".project-card")
            .Filter(new LocatorFilterOptions
            {
                Has = _page.Locator($"h5.project-name:has-text('{testProjectName}')")
            });

        // Wait for card to appear
        await targetCard.First.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });

        await targetCard.First.ScrollIntoViewIfNeededAsync();

        // Locate delete button and scroll also
        var deleteBtn = targetCard.Locator(".delete-project-btn").First;

        await deleteBtn.ScrollIntoViewIfNeededAsync();
        await deleteBtn.ClickAsync();

        // Verify deletion
        await _page.WaitForTimeoutAsync(500);

        int remaining = await _page.Locator($"h5.project-name:has-text('{testProjectName}')").CountAsync();

        Assert.AreEqual(0, remaining, $"Project '{testProjectName}' should have been deleted.");
    }
}
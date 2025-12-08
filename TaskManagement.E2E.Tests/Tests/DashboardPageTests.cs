using Microsoft.Playwright;
using TaskManagement.E2E.Tests.Pages;

namespace TaskManagement.E2E.Tests
{
    [TestClass]
    public class DashboardPageTests
    {
        // URLs and credentials
        private const string LoginUrl = "http://localhost:5244/login";
        private const string DashboardUrl = "http://localhost:5244/projects";
        private const string Username = "Yuri";
        private const string Password = "Pass123!";

        // Playwright browser, context, and page
        private IBrowser? _browser;
        private IBrowserContext? _context;
        private IPage? _page;

        [TestInitialize]
        public async Task TestInitialize()
        {
            // Launch browser and create context/page
            var playwright = await Playwright.CreateAsync();
            _browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            _context = await _browser.NewContextAsync();
            _page = await _context.NewPageAsync();

            // Perform login
            var loginPage = new LoginPage(_page);
            await loginPage.GoToAsync(LoginUrl);
            await loginPage.LoginAsync(Username, Password);

            // Wait for redirect to projects dashboard
            await _page.WaitForURLAsync("**/projects", new PageWaitForURLOptions { Timeout = 10000 });
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            // Close context and browser after tests
            if (_context != null) await _context.CloseAsync();
            if (_browser != null) await _browser.CloseAsync();
        }

        // Helper: Navigate to dashboard and wait until fully loaded
        private async Task<DashboardPage> GoToDashboardAsync()
        {
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

            // Click button and verify navigation to Add Task page
            await dashboardPage.CreateTaskButton.ClickAsync();
            await _page!.WaitForURLAsync("**/tasks/create");
            Assert.Contains("/tasks/create", _page.Url);
        }

        [TestMethod]
        public async Task CanClickCreateProjectButton()
        {
            var dashboardPage = await GoToDashboardAsync();
            await dashboardPage.CreateProjectButton.WaitForAsync();

            // Click button and verify navigation to Create Project page
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
                Assert.Inconclusive("No task cards available on dashboard.");

            // Get task name for verification
            var taskName = await firstTask!.Locator(".task-title").InnerTextAsync();

            // Click first task card and wait for details page
            await firstTask.ClickAsync();
            await _page!.WaitForURLAsync(url => url.Contains("/tasks/"));
            await _page.Locator(".task-details-card").WaitForAsync();

            Assert.Contains("/tasks/", _page.Url);
        }

        [TestMethod]
        public async Task CanOpenProjectFromDashboard()
        {
            var dashboardPage = await GoToDashboardAsync();

            var secondProject = dashboardPage.Get2ndProject();
            if (secondProject == null)
                Assert.Inconclusive("No projects available on dashboard.");

            // Get project name for verification
            var projectName = await secondProject!.Locator(".project-name").InnerTextAsync();

            // Click project card and wait for project details
            await secondProject.ClickAsync();
            await _page!.WaitForURLAsync(url => url.Contains("/projects/"));
            await _page.Locator("p.project-description").WaitForAsync();

            Assert.Contains("/projects/", _page.Url);
        }

        [TestMethod]
        public async Task ChangeTaskStatusSuccessfully()
        {
            await _page!.GotoAsync(DashboardUrl);

            // Wait for first task card to appear
            await _page.WaitForSelectorAsync(".task-card");
            var firstCard = _page.Locator(".task-card").First;

            // Change status
            var statusSelect = firstCard.Locator("select[name='task.Status']");
            await statusSelect.WaitForAsync();
            await statusSelect.SelectOptionAsync("Done");
            await statusSelect.DispatchEventAsync("change");

            // Wait for UI update
            await Assertions.Expect(statusSelect).ToHaveValueAsync("Done", new() { Timeout = 5000 });

            // Verify selection
            string selected = await statusSelect.InputValueAsync();
            Assert.AreEqual("Done", selected);
        }

        [TestMethod]
        public async Task CanEditAndDeleteProjectSuccessfully()
        {
            const string testProjectName = "AutoTestProject";

            await _page!.GotoAsync(DashboardUrl);

            // Create project if it doesn't exist
            if (await _page.Locator($"h5.project-name:has-text('{testProjectName}')").CountAsync() == 0)
            {
                await _page.GotoAsync("http://localhost:5244/projects/create");
                await _page.FillAsync("#projectName", testProjectName);
                await _page.FillAsync("#projectDescription", "Temporary project for test");
                await _page.ClickAsync("#createProjectBtn");
                await _page.WaitForURLAsync("**/projects");
            }

            // Locate project card
            var targetCard = _page.Locator(".project-card")
                .Filter(new LocatorFilterOptions { Has = _page.Locator($"h5.project-name:has-text('{testProjectName}')") })
                .First;
            await targetCard.WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });

            // Edit project description
            var editBtn = targetCard.Locator(".edit-project-btn").First;
            await editBtn.ScrollIntoViewIfNeededAsync();
            await editBtn.ClickAsync();
            await _page.WaitForURLAsync(url => url.Contains("/projects/edit/"), new PageWaitForURLOptions { Timeout = 10000 });

            var descriptionTextarea = _page.Locator("textarea[name='editModel.Description']");
            await descriptionTextarea.WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
            await descriptionTextarea.ScrollIntoViewIfNeededAsync();
            await descriptionTextarea.FillAsync("Updated description for test project");

            // Save changes
            var saveButton = _page.Locator("button:has-text('Save Project')");
            await saveButton.ScrollIntoViewIfNeededAsync();
            await saveButton.ClickAsync();

            // Wait for dashboard reload
            await _page.WaitForURLAsync("**/projects", new PageWaitForURLOptions { Timeout = 10000 });

            // Verify updated description
            targetCard = _page.Locator(".project-card")
                .Filter(new LocatorFilterOptions { Has = _page.Locator($"h5.project-name:has-text('{testProjectName}')") })
                .First;
            await targetCard.WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });

            var description = await targetCard.Locator(".project-desc").InnerTextAsync();
            Assert.AreEqual("Updated description for test project", description);

            // Delete project
            var deleteBtn = targetCard.Locator(".delete-project-btn").First;
            await deleteBtn.ScrollIntoViewIfNeededAsync();
            await deleteBtn.ClickAsync();

            // Wait until project card is removed
            await _page.WaitForSelectorAsync(
                $"h5.project-name:has-text('{testProjectName}')",
                new() { State = WaitForSelectorState.Detached, Timeout = 10000 }
            );

            // Final verification
            int remaining = await _page.Locator($"h5.project-name:has-text('{testProjectName}')").CountAsync();
            Assert.AreEqual(0, remaining, $"Project '{testProjectName}' should have been deleted.");
        }
    }
}
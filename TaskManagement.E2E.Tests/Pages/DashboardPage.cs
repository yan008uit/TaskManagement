using Microsoft.Playwright;

namespace TaskManagement.E2E.Tests.Pages
{
    // Page object representing the Dashboard page in the E2E tests.
    // Provides locators and actions for tasks, projects, and dashboard elements.
    public class DashboardPage
    {
        // Playwright IPage instance for interacting with the browser page.
        private readonly IPage _page;

        // Constructor to initialize the page object with an IPage instance.
        public DashboardPage(IPage page)
        {
            _page = page;
        }

        // Heading element at the top of the dashboard
        private ILocator Heading => _page.Locator("h3.page-title");

        // Containers for tasks and projects
        private ILocator TasksContainer => _page.Locator(".projects-container").First;
        private ILocator ProjectsContainer => _page.Locator(".projects-container").Nth(1);

        // Individual task and project cards within their respective containers
        public ILocator TaskCards => TasksContainer.Locator(".task-card");
        public ILocator ProjectCards => ProjectsContainer.Locator(".project-card");

        // Buttons for creating tasks and projects
        public ILocator CreateTaskButton => _page.Locator("div.card:has-text('Add Task')");
        public ILocator CreateProjectButton => _page.Locator("div.card:has-text('Create Project')");

        /// <summary>
        /// Waits until the dashboard heading, tasks, and projects containers are fully loaded and visible.
        /// </summary>
        public async Task WaitForLoadAsync()
        {
            await Task.WhenAll(
                Heading.WaitForAsync(new LocatorWaitForOptions { Timeout = 30000, State = WaitForSelectorState.Visible }),
                TasksContainer.WaitForAsync(new LocatorWaitForOptions { Timeout = 30000, State = WaitForSelectorState.Visible }),
                ProjectsContainer.WaitForAsync(new LocatorWaitForOptions { Timeout = 30000, State = WaitForSelectorState.Visible })
            );
        }

        // Dynamic accessors for task and project cards by index
        public ILocator GetTaskByIndex(int index) => TaskCards.Nth(index);
        public ILocator GetProjectByIndex(int index) => ProjectCards.Nth(index);

        // Convenience methods for first task/project
        public ILocator GetFirstTask() => GetTaskByIndex(0);
        public ILocator GetFirstProject() => GetProjectByIndex(0);

        // Get counts for tasks and projects
        public async Task<int> GetTaskCountAsync() => await TaskCards.CountAsync();
        public async Task<int> GetProjectCountAsync() => await ProjectCards.CountAsync();

        // Get the dashboard heading text
        public async Task<string> GetHeadingTextAsync() => await Heading.InnerTextAsync();
    }
}
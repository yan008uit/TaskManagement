using Microsoft.Playwright;

namespace TaskManagement.E2E.Tests.Pages
{
    public class DashboardPage
    {
        private readonly IPage _page;

        public DashboardPage(IPage page)
        {
            _page = page;
        }

        // Heading
        private ILocator Heading => _page.Locator("h3.page-title");

        // Containers
        private ILocator TasksContainer => _page.Locator(".projects-container").First;
        private ILocator ProjectsContainer => _page.Locator(".projects-container").Nth(1);

        // Task & Project cards
        public ILocator TaskCards => TasksContainer.Locator(".task-card");
        public ILocator ProjectCards => ProjectsContainer.Locator(".project-card");

        // Buttons
        public ILocator CreateTaskButton => _page.Locator("div.card:has-text('Add Task')");

        public ILocator CreateProjectButton => _page.Locator("div.card:has-text('Create Project')");

        // Wait until the dashboard is fully loaded
        public async Task WaitForLoadAsync()
        {
            await Task.WhenAll(
                Heading.WaitForAsync(new LocatorWaitForOptions { Timeout = 30000, State = WaitForSelectorState.Visible }),
                TasksContainer.WaitForAsync(new LocatorWaitForOptions { Timeout = 30000, State = WaitForSelectorState.Visible }),
                ProjectsContainer.WaitForAsync(new LocatorWaitForOptions { Timeout = 30000, State = WaitForSelectorState.Visible })
            );
        }

        // Dynamic accessors
        public ILocator GetTaskByIndex(int index) => TaskCards.Nth(index);
        public ILocator GetProjectByIndex(int index) => ProjectCards.Nth(index);

        public ILocator GetFirstTask() => GetTaskByIndex(0);
        public ILocator GetFirstProject() => GetProjectByIndex(0);

        public async Task<int> GetTaskCountAsync() => await TaskCards.CountAsync();
        public async Task<int> GetProjectCountAsync() => await ProjectCards.CountAsync();
        public async Task<string> GetHeadingTextAsync() => await Heading.InnerTextAsync();
    }
}
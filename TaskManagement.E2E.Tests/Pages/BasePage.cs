using Microsoft.Playwright;
using System.Threading.Tasks;

namespace TaskManagement.E2E.Tests.Pages
{
    public abstract class BasePage
    {
        protected readonly IPage Page;
        protected BasePage(IPage page) => Page = page;

        public virtual async Task GoToAsync(string url)
        {
            await Page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        }

        public async Task<string> GetTitleAsync() => await Page.TitleAsync();
    }
}
using Microsoft.Playwright;
using System.Threading.Tasks;

namespace TaskManagement.E2E.Tests.Pages
{
    public class HomePage : BasePage
    {
        public HomePage(IPage page) : base(page) { }

        public ILocator LoginButton => Page.GetByRole(AriaRole.Button, new() { Name = "Login" });

        public async Task WaitForLoadAsync()
        {
            await Page.WaitForFunctionAsync(@"() => document.title === 'Task Manager'");
            await LoginButton.WaitForAsync();
        }
    }
}
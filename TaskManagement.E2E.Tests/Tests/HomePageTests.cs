using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using TaskManagement.E2E.Tests.Pages;

namespace TaskManagement.E2E.Tests
{
    [TestClass]
    public class HomePageTests : PageTest
    {
        private const string BaseUrl = "http://localhost:5244";

        [TestMethod]
        public async Task HomePageLoadsSuccessfully()
        {
            var homePage = new HomePage(Page);
            await homePage.GoToAsync(BaseUrl);
            await homePage.WaitForLoadAsync();

            Assert.AreEqual("Task Manager", await homePage.GetTitleAsync());
            Assert.IsTrue(await homePage.LoginButton.IsVisibleAsync());
        }
    }
}
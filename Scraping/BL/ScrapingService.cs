using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using PuppeteerSharp;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Scraping.Global;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics.Metrics;
using System.Composition;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Scraping.Models;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.Collections.Generic;

namespace Scraping.BL
{
    public class ScrapingService
    {
        public async Task<List<BankData>> GetBankHapoalimData(IWebHostEnvironment _environment)
        {
            string screenshotPath = Globals.screenshotPath,
                   screenshotFile = Path.Combine(_environment.ContentRootPath, screenshotPath);

            using var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            });

            //Login
            PuppeteerSharp.Page page = (PuppeteerSharp.Page)await browser.NewPageAsync();
            var viewport = new ViewPortOptions();
            viewport.Width = 1200;
            viewport.Height = 800;
            await page.SetViewportAsync(viewport);
            await page.GoToAsync(Globals.bankHapoalimPath);

            await page.ClickAsync("a.login-button.login-desktop");

            await page.WaitForNavigationAsync();
            await page.Keyboard.PressAsync("Escape");

            await page.TypeAsync("#userCode", Creds.hapoalimUsername);
            await page.TypeAsync("#password", Creds.hapoalimPassword);

            //For debugging
            //await loginPage.ScreenshotAsync(screenshotFile);

            await page.Keyboard.PressAsync("Enter");

            await page.WaitForNavigationAsync();

            //For debugging
            //await loginPage.ScreenshotAsync(screenshotFile);

            await page.WaitForSelectorAsync("ul.mega-menu #mega-menu-1");
            await page.ClickAsync("ul.mega-menu #mega-menu-1");

            //For debugging
            //await page.ScreenshotAsync(screenshotFile);

            await page.WaitForSelectorAsync("#period-filter-button-0");
            await page.ClickAsync("#period-filter-button-0");

            await page.WaitForSelectorAsync("label[for=\"period-filter-last-calendar-month-00\"]");
            await page.ClickAsync("label[for=\"period-filter-last-calendar-month-00\"]");

            //For debugging
            await page.ScreenshotAsync(screenshotFile);

            await page.WaitForSelectorAsync(".dropdown-footer.divider button[type=\"submit\"]");
            await page.ClickAsync(".dropdown-footer.divider button[type=\"submit\"]");

            //Thread.Sleep(2000);
            await page.ScreenshotAsync(screenshotFile);

            await page.WaitForSelectorAsync(".icon-export-file.export-navigation-item");
            await page.ClickAsync(".icon-export-file.export-navigation-item");

            Thread.Sleep(2000);


            var tableHead = await page.EvaluateFunctionAsync<string[]>(@"() => {
                var elements = [];
                document.querySelectorAll('table.table tr th').forEach(e => elements.push(e.innerText));
                return elements;
            }");

            var tableCells = await page.EvaluateFunctionAsync<string[]>(@"() => {
                var elements = [];
                document.querySelectorAll('table.table tr td').forEach(e => elements.push(e.innerText));
                return elements;
            }");

            List<BankData> bankData = new List<BankData>();

            for (int i = 0; i < tableCells.Count(); i += 5)
            {
                BankData bd = new BankData();

                bd.ActionDate = tableCells[i];
                bd.ActionAmount = tableCells[i + 2];
                bd.ActionDetailes = FillByX(tableCells[i + 1]);
                bankData.Add(bd);
            }

            WriteToJsonFile(_environment, bankData);

            return bankData;
        }

        private string FillByX(string str)
        {
            string outStr = str;
            try
            {
                string s = str.Trim(),
                       x = new string('X', str.Length - 2);
                outStr = $"{s.Substring(0, 1)}{x}{s.Substring(s.Length - 1, 1)}";
            }
            catch(Exception ex)
            {
                string error = ex.Message;
            }

            return outStr;
        }

        private void WriteToJsonFile(IWebHostEnvironment _environment, List<BankData> bankData)
        {
            string bankDataPath = Globals.bankHapoalimData,
                   bankDataFile = Path.Combine(_environment.ContentRootPath, bankDataPath);

            string jsoFile = JsonConvert.SerializeObject(bankData);

            System.IO.File.WriteAllText(bankDataFile, jsoFile);
        }

        private async Task TypeText(PuppeteerSharp.Page page, string elementName, string text)
        {
            await page.ClickAsync(elementName);
            foreach (char letter in text)
            {
                await page.Keyboard.PressAsync(letter.ToString());
            }
        }
    }
}

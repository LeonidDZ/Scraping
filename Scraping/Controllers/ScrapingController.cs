using Microsoft.AspNetCore.Mvc;
using Scraping.BL;
using Scraping.Global;
using Scraping.Models;
using System.Security.Policy;

namespace Scraping.Controllers
{
    public class ScrapingController : Controller
    {
        private readonly IWebHostEnvironment _environment;

        public ScrapingController(IWebHostEnvironment IWebHostEnvironment) {
            _environment = IWebHostEnvironment;
        }

        ScrapingService service = new ScrapingService();
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public async Task<List<BankData>> GetBankHapoalimData()
        {
            List<BankData> bankData = await service.GetBankHapoalimData(_environment);

            return bankData;
        }
    }
}

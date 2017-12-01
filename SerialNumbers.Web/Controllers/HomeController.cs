using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SerialNumbers.Business;
using SerialNumbers.Web.Models;
using SerialNumbers.Web.Settings;

namespace SerialNumbers.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISerialNumberService _serialNumberService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HomeController> _logger;
        private readonly IOptions<AppSettings> _settings;

        public HomeController(ILogger<HomeController> logger, IOptions<AppSettings> settings, IConfiguration configuration, ISerialNumberService serialNumberService)
        {
            _serialNumberService = serialNumberService ?? throw new ArgumentNullException(nameof(serialNumberService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public IActionResult Index()
        {
            _logger.LogInformation("Index page was shown.");
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = $"Your application about page. {Environment.NewLine} Value from config: {_settings.Value.BaseDomain} {Environment.NewLine} Value from custom configuration file: {_configuration.GetValue<string>("Timeouts:ApplicationTimeout")}";

            _logger.LogInformation("About page was shown.");
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            _logger.LogInformation("Contact page was shown.");
            return View();
        }

        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            _logger.LogWarning($"Error page was shown, RequestId = {requestId}.");
            return View(new ErrorViewModel { RequestId = requestId });
        }
    }
}
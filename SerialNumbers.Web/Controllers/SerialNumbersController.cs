using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SerialNumbers.Business;
using SerialNumbers.Web.Models;
using SerialNumbers.Web.Models.SerialNumbersViewModels;
using SerialNumbers.Web.Settings;

namespace SerialNumbers.Web.Controllers
{
    [Route("[controller]/[action]")]
    public class SerialNumbersController : Controller
    {
        private readonly ISerialNumberService _serialNumberService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SerialNumbersController> _logger;
        private readonly IOptions<AppSettings> _settings;

        public SerialNumbersController(ILogger<SerialNumbersController> logger, IOptions<AppSettings> settings, IConfiguration configuration, ISerialNumberService serialNumberService)
        {
            _serialNumberService = serialNumberService ?? throw new ArgumentNullException(nameof(serialNumberService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult CreateSchema(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult CreateSchema(CreateSchemaViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = _serialNumberService.CreateSchema(model.Schema, model.Customer, model.Mask, model.Seed ?? 0, model.Increment ?? 1);
                if (result != null)
                {
                    _logger.LogInformation($"Schema was successfully created, Schema: {result.Schema}.");
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Create schema failed.");
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            _logger.LogWarning($"Error page was shown, RequestId = {requestId}.");
            return View(new ErrorViewModel { RequestId = requestId });
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}
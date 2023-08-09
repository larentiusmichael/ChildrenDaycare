using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ChildrenDaycare.Models;
using System.Security.Claims;
using System.Text;

namespace ChildrenDaycare.Controllers
{
    public class HomeController : Controller
    {
    private readonly ILogger<HomeController> _logger;
    private readonly HttpClient _client;

    public HomeController(ILogger<HomeController> logger, HttpClient client)
    {
        _logger = logger;
        _client = client;
    }

    public async Task<IActionResult> Index()
    {
        System.Security.Claims.ClaimsPrincipal currentUser = User;

        var userClaims = new Dictionary<string, string>();
        foreach (var claim in currentUser.Claims)
        {
            userClaims[claim.Type] = claim.Value;
        }

        var request = new HttpRequestMessage(HttpMethod.Post, "https://6y6yqe9s73.execute-api.us-east-1.amazonaws.com/Development");
        request.Content = new StringContent(JsonConvert.SerializeObject(userClaims), Encoding.UTF8, "application/json");

        var response = await _client.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

            var actionControllerPair = responseObject["body"].Split('-');
            var actionName = actionControllerPair[0];
            var controllerName = actionControllerPair.Length > 1 ? actionControllerPair[1] : "Home";

            if (actionName == "Home" && controllerName == "Home")
            {
                return View();
            }

            return RedirectToAction(actionName, controllerName);
        }

        return View();
    }

        public IActionResult Waiting()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

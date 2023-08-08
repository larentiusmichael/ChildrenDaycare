// using ChildrenDaycare.Models;
// using Microsoft.AspNetCore.Mvc;
// using System.Diagnostics;

// namespace ChildrenDaycare.Controllers
// {
//     public class HomeController : Controller
//     {
//         private readonly ILogger<HomeController> _logger;

//         public HomeController(ILogger<HomeController> logger)
//         {
//             _logger = logger;
//         }

//         public IActionResult Index()
//         {
//             System.Security.Claims.ClaimsPrincipal currentUser = User;
//             if (currentUser.IsInRole("Admin"))
//             {
//                 return RedirectToAction("Index", "Slot");
//             }
//             else if (currentUser.IsInRole("Takecare Giver"))
//             {
//                 return RedirectToAction("TakecareGiverDisplay", "Slot");
//             }
//             else if (currentUser.IsInRole("Public"))
//             {
//                 return RedirectToAction("PublicDisplay", "Slot"); 
//             }
//             return View();
//         }

//         public IActionResult Waiting()
//         {
//             return View();
//         }

//         public IActionResult Privacy()
//         {
//             return View();
//         }

//         [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
//         public IActionResult Error()
//         {
//             return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
//         }
//     }
// }

using Amazon.Lambda;
using Amazon.Lambda.Model;
using ChildrenDaycare.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChildrenDaycare.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAmazonLambda _lambdaClient;

        public HomeController(ILogger<HomeController> logger, IAmazonLambda lambdaClient)
        {
            _logger = logger;
            _lambdaClient = lambdaClient;
        }

        public async Task<IActionResult> Index()
        {
            System.Security.Claims.ClaimsPrincipal currentUser = User;

            var request = new InvokeRequest
            {
                FunctionName = "YourLambdaFunctionName",
                InvocationType = InvocationType.RequestResponse,
                Payload = JsonSerializer.Serialize(new { role = currentUser.Identity.Name }) 
            };

            var response = await _lambdaClient.InvokeAsync(request);

            var payload = JsonSerializer.Deserialize<Dictionary<string, string>>(response.Payload);

            var actionControllerPair = payload["role"].Split('-');

            var actionName = actionControllerPair[0];
            var controllerName = actionControllerPair.Length > 1 ? actionControllerPair[1] : "Home";

            if (actionName == "Home" && controllerName == "Home")
            {
                return View();
            }

            return RedirectToAction(actionName, controllerName);
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

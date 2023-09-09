using DBLayer.EntitiesManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SmartChatWebApp.Models;
using System.Diagnostics;

namespace SmartChatWebApp.Controllers
{
    [Authorize(Roles ="Member")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserManager _userManager;
        private readonly IChatManager _chatManager;
        private readonly IMessageManager _messageManager;

        public HomeController(ILogger<HomeController> logger, IUserManager userManager, 
            IChatManager chatManager, IMessageManager messageManager)
        {
            _logger = logger;
            _userManager = userManager;
            _chatManager = chatManager;
            _messageManager = messageManager;
        }

        public IActionResult Index()
        {
            HubCallerContext context;
            ViewBag.GetAllUsers = _userManager.GetAll();
            //ViewBag.GetMessagesFromTo = _messageManager.GetMessagesFromSenderToReceiver();

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
using System.Buffers;
using Microsoft.AspNetCore.Mvc;
using MsgBroker.Models;
using System.Diagnostics;
using MsgBroker.Models.AuthorizationScope;
using MsgBroker.Models.Common;

namespace MsgBroker.Controllers
{
    public class HomeController : Controller
    {
        public static MemoryRepository Repository { get; set; }

        public HomeController()
        {
            
        }

        public IActionResult Index()
        {
            return View();
        }

        [ActionName("Show")]
        [HttpPost]
        [SelectAction(Name = "selector", Value = "Login as trader")]
        public ActionResult ShowGlobal(AuthorizationInfo ainfo)
        {
            var success = LoginVerificator.VerifyCredentials(ainfo, Repository);
            if (!success) return new RedirectResult("http://www.google.com");
            var ut = new UserTicket(ainfo.Login, ainfo.Password);
            ViewBag.UserId = ainfo.Login;
            ViewBag.UserTicket = ut.Encrypt(UserTicketValidator.__key);
            ViewBag.AccountId = "Global";
            return View("Global");
        }

        [ActionName("Show")]
        [HttpPost]
        [SelectAction(Name = "selector", Value = "Login as admin")]
        public ActionResult ShowAdminTools(AuthorizationInfo ainfo)
        {
            var success = LoginVerificator.VerifyAdminCredentials(ainfo, Repository);
            if (!success) return new RedirectResult("http://www.google.com");
            var ut = new UserTicket(ainfo.Login, ainfo.Password);
            //ViewBag.UserTicket = ut.Encrypt(UserTicketValidator.__key);
            ModelState.Clear();
            return View("AdminTools");
        }

        [ActionName("Show")]
        [HttpPost]
        [SelectAction(Name = "selector", Value = "Change admin credentials")]
        public ActionResult ChangeAdminCredentials(AuthorizationInfo ainfo)
        {
            if (!UserTicket.VerifyId(ainfo.Login.Trim()))
            {
                ViewBag.Message = "Bad login";
                return View("BadInput");
            }
            if (!UserTicket.VerifyId(ainfo.Password.Trim()))
            {
                ViewBag.Message = "Bad password";
                return View("BadInput");
            }

            Repository.ChangeAdminCredentials(ainfo);
            ModelState.Clear();
            return View("Index");
        }

        [ActionName("Show")]
        [HttpPost]
        [SelectAction(Name = "selector", Value = "Add new user")]
        public ActionResult AddNewUser(AuthorizationInfo ainfo)
        {
            if (!UserTicket.VerifyId(ainfo.Login.Trim()))
            {
                ViewBag.Message = "Bad login";
                return View("BadInput");
            }
            if (!UserTicket.VerifyId(ainfo.Password.Trim()))
            {
                ViewBag.Message = "Bad password";
                return View("BadInput");
            }

            if (!Repository.Add(ainfo))
            {
                ViewBag.Message = "Please, change login or password!";
                return View("BadInput");
            }

            ModelState.Clear();
            return View("Index");
        }

        [ActionName("Show")]
        [HttpPost]
        [SelectAction(Name = "selector", Value = "Exchange")]
        public ActionResult ShowDetails(string exName, string ut)
        {
            ActionResult actionResult = null;
            if (ticketProcessing(ut, ref actionResult)) return actionResult;
            ViewBag.ExchangeNameToDisplay = exName.Replace(' ', '\u00a0');
            ViewBag.ExchangeName = exName;
            ViewBag.UserTicket = ut;
            return View("Details");
        }

        private bool ticketProcessing(string ut, ref ActionResult actionResult)
        {
            var ticket = UserTicket.Decrypt(ut, UserTicketValidator.__key);
            var ainfo = new AuthorizationInfo { Login = ticket.Login, Password = ticket.Password };
            var success = LoginVerificator.VerifyCredentials(ainfo, Repository);
            if (!success)
            {
                actionResult = new RedirectResult("http://www.google.com");
                return true;
            }
            ViewBag.UserId = ainfo.Login;
            return false;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

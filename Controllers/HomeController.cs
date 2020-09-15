using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TestSessionWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.SessionId = Session.SessionID;
            HttpContext.Cache.Insert("admin", Session.SessionID, null, DateTime.Now.AddSeconds(10), TimeSpan.Zero);
            Session["admin"] = "admin";//当使用Session保存一些信息的时候才会在浏览器中生成cookie保存sessionId, 可以取消此处尝试
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            ViewBag.SessionId = Session.SessionID;
            var _cache = HttpContext.Cache.Get("admin");
            ViewBag.Admin = _cache;
            ViewBag.SessionInfoAdmin = Session["admin"];
            return View();
        }
    }
}
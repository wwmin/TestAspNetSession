using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;

namespace TestSessionWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.SessionId = Session.SessionID;
            ViewBag.SessionName = Session["name"];
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            ViewBag.OldSessionId = Session.SessionID;            
            ViewBag.NewSessionId = ReGenerateSessionId();
            Session["name"] = "SessionName @ " + DateTime.Now;
            return View();
        }

        //https://stackoverflow.com/questions/15241464/after-change-sessionid-data-in-session-variables-is-lost
        private string ReGenerateSessionId() {
            var Context = ((HttpApplication)HttpContext.GetService(typeof(HttpApplication))).Context;
            SessionIDManager manager = new SessionIDManager();
            string oldId = manager.GetSessionID(Context);
            string newId = manager.CreateSessionID(Context);
            bool isAdd = false, isRedir = false;
            manager.RemoveSessionID(Context);
            manager.SaveSessionID(Context, newId, out isRedir, out isAdd);

            HttpApplication ctx = (HttpApplication)HttpContext.ApplicationInstance;
            HttpModuleCollection mods = ctx.Modules;
            System.Web.SessionState.SessionStateModule ssm = (SessionStateModule)mods.Get("Session");
            System.Reflection.FieldInfo[] fields = ssm.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            SessionStateStoreProviderBase store = null;
            System.Reflection.FieldInfo rqIdField = null, rqLockIdField = null, rqStateNotFoundField = null;

            SessionStateStoreData rqItem = null;
            foreach (System.Reflection.FieldInfo field in fields) {
                if (field.Name.Equals("_store")) store = (SessionStateStoreProviderBase)field.GetValue(ssm);
                if (field.Name.Equals("_rqId")) rqIdField = field;
                if (field.Name.Equals("_rqLockId")) rqLockIdField = field;
                if (field.Name.Equals("_rqSessionStateNotFound")) rqStateNotFoundField = field;

                if ((field.Name.Equals("_rqItem"))) {
                    rqItem = (SessionStateStoreData)field.GetValue(ssm);
                }
            }
            object lockId = rqLockIdField.GetValue(ssm);

            if ((lockId != null) && (oldId != null)) {
                store.RemoveItem(Context, oldId, lockId, rqItem);
            }

            rqStateNotFoundField.SetValue(ssm, true);
            rqIdField.SetValue(ssm, newId);

            return newId;
        }

    }
}
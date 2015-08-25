using OrderSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace OrderSystem.Controllers {
	public class WaiterController : Controller {
		// GET: Waiter
		public ActionResult Index() {
			return View();
		}
		public ActionResult Partial(string id) {
			return View(id);
		}
		public JsonResult IsAuthenticated() {
			using(MrCyContext ctx = new MrCyContext()) {
				if(User.Identity.IsAuthenticated) {
					return Json(new JsonSucceedObj(ctx.ClerkInfo.FirstOrDefault(p => p.ClerkId == User.Identity.Name)));
				}
				return Json(new JsonErrorObj());
			}

			
		}
		public ActionResult Signin() {
			using(MrCyContext ctx = new MrCyContext()) {
				string username = Request.Form["username"].ToString();
				string password = Request.Form["password"].ToString();
                ClerkInfo clerk = ctx.ClerkInfo.Where(p => p.LoginName == username && p.LoginPwd == password).FirstOrDefault();
				if(clerk == null) {
					return Json(new JsonErrorObj("用户名或密码不正确"));
				}
				FormSignin(clerk);
				return Json(new JsonSucceedObj(clerk));
			}
		}
		private void FormSignin(ClerkInfo c) {
			FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(c.ClerkId, true, 60);
			string authTicket = FormsAuthentication.Encrypt(ticket);
			HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName) {
				Value = authTicket,
				Expires = DateTime.Now.AddDays(1)
			};
			Response.SetCookie(cookie);
		}

		public ActionResult GetTables() {
			using(MrCyContext ctx = new MrCyContext()) {
				var list = ctx.DeskInfo.Where(p => p.Usable).ToList();
				return Json(list);
			}
		}
	}
}
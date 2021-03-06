﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OrderSystem.Models;
using System.Threading.Tasks;
using System.Web.Security;
using OrderSystem.Utility;

namespace OrderSystem.Controllers {
	public class AccountController : Controller {

		public JsonResult IsAuthenticated() {
			if(User.Identity.IsAuthenticated) {
				return Json(new JsonSucceedObj(new {
					Name = User.Identity.Name
				}));
			}
			return Json(new JsonErrorObj());
		}

		public FileContentResult CodeImage() {
			string code = Util.CreateRandomCode();
			Session["CodeImg"] = code.ToLower();
			return File(Util.CreateCheckCodeImage(code), "image/jpeg");
		}

		public JsonResult Signin(SigninViewModel model) {
			if(model.CodeImg.ToLower() != (string)Session["CodeImg"]) {
				return Json(new JsonErrorObj("验证码不正确"));
			}
			using(MrCyContext ctx = new MrCyContext()) {
				ClientInfo client = ctx.ClientInfo.Where(p => p.LoginName == model.Mobile && p.LoginPwd == model.Password).FirstOrDefault();
				if(client == null) {
					return Json(new JsonErrorObj("手机或密码不正确"));
				}
				FormSignin(client);
				return Json(new JsonSucceedObj());
			}
		}

		public ActionResult SendForgetSMS(SMSSendViewModel model) {
			using(MrCyContext ctx = new MrCyContext()) {
				int count = ctx.ClientInfo.Where(p => p.LoginName == model.Mobile).ToList().Count;
				if(count == 0) {
					return Json(new JsonErrorObj("此号码未注册"), "Mobile");
				}
			}
			Random rand = new Random(unchecked((int)DateTime.Now.Ticks));
			string code = "";
			for(int i = 0; i < 6; i++) {
				code += rand.Next(10);
			}
			Session["SMSForgetCode"] = code;
			if(SMS.SMSSender.Send(model.Mobile, code)) {
				return Json(new JsonSucceedObj());
			}
			return Json(new JsonErrorObj());
		}
		public async Task<JsonResult> Forget(SignupViewModel model) {
			if(Session["SMSForgetCode"].ToString() != model.Code) {
				return Json(new JsonErrorObj("验证码不正确", "code"));
			}
			using(MrCyContext ctx = new MrCyContext()) {
				ClientInfo client = ctx.ClientInfo.Where(p => p.LoginName == model.Mobile).FirstOrDefault();
				client.LoginPwd = model.Password;
				ctx.Entry(client).Property(p => p.LoginPwd).IsModified = true;
				await ctx.SaveChangesAsync();
				return Json(new JsonSucceedObj());
			}
		}

		public ActionResult SendSMS(SMSSendViewModel model) {
			using(MrCyContext ctx = new MrCyContext()) {
				int count = ctx.ClientInfo.Where(p => p.LoginName == model.Mobile).ToList().Count;
				if(count > 0) {
					return Json(new JsonErrorObj("此号码已注册"), "Mobile");
				}
			}

			Random rand = new Random(unchecked((int)DateTime.Now.Ticks));
			string code = "";
			for(int i = 0; i < 6; i++) {
				code += rand.Next(10);
			}
			Session["SMSCode"] = code;
			if(SMS.SMSSender.Send(model.Mobile, code)) {
				return Json(new JsonSucceedObj());
			}
			return Json(new JsonErrorObj());
		}
		public async Task<JsonResult> Signup(SignupViewModel model) {
			if(Session["SMSCode"].ToString() != model.Code) {
				return Json(new JsonErrorObj("验证码不正确", "code"));
			}
			using(MrCyContext ctx = new MrCyContext()) {
				ClientInfo client = new ClientInfo() {
					ClientId = (DateTime.Now.Year%100).ToString() + model.Mobile,
					ClientName = model.Mobile,
					LoginName = model.Mobile,
					LoginPwd = model.Password
				};
				ctx.ClientInfo.Add(client);
				await ctx.SaveChangesAsync();
				FormSignin(client);
				return Json(new JsonSucceedObj());
			}
		}
		public JsonResult Signout() {
			FormsAuthentication.SignOut();
			return Json(new JsonSucceedObj());
		}
		private void FormSignin(ClientInfo c) {
			FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(c.ClientId, true, 60);
			string authTicket = FormsAuthentication.Encrypt(ticket);
			HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName) {
				Value = authTicket,
				Expires = DateTime.Now.AddDays(1)
			};
			Response.SetCookie(cookie);
		}
	}
}
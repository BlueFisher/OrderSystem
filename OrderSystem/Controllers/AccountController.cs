using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OrderSystem.Controllers {
	public class AccountController : Controller {
		// GET: Account
		public ActionResult SendSMS(string id) {
			Random rand = new Random(unchecked((int)DateTime.Now.Ticks));
			string code = "";
			for(int i = 0; i < 6; i++) {
				code += rand.Next(10);
			}
			Session["SMSCode"] = code;
			if(SMS.SMSSender.Send(id, code)) {
				return Json(new {
					status = 1
				});
			}
			return Json(new {
				status = 0
			});
		}
		
	}
}
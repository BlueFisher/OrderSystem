using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OrderSystem.Models;
using System.Data.Entity;

namespace OrderSystem.Controllers {
	public class PayController : Controller {
		// GET: Pay

		public ActionResult WeixinCompleted() {
			if(Request.QueryString["id"] == null) {
				return Redirect("/#/");
			}
			if(Request.QueryString["cancel"].ToString() == "1") {
				using(MrCyContext ctx = new MrCyContext()) {
					int id = Convert.ToInt32(Request.QueryString["id"]);
					DineTempInfo info = ctx.DineTempInfo.Where(p => p.AutoID == id).FirstOrDefault();
					info.IsPaid = 1;
					ctx.Entry<DineTempInfo>(info).Property(p => p.IsPaid).IsModified = true;
					ctx.SaveChanges();
				}
				return Redirect("/#/onlinepaysuccess");
			}
			else {
				return Redirect("/#/onlinepayfail");
			}
		}
	}
}
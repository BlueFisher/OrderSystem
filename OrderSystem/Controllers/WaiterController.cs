using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OrderSystem.Controllers {
	public class WaiterController : Controller {
		// GET: Waiter
		public ActionResult Index() {
			return View();
		}
		public ActionResult Partial(string id) {
			return View(id);
		}
	}
}
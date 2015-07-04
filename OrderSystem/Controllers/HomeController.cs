using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Data.Entity;
using OrderSystem.Models;

namespace OrderSystem.Controllers {
	public class HomeController : Controller {
		// GET: Home
		public ActionResult Index() {
			return View();
		}

		public async Task<JsonResult> GetMenuSubClass() {
			using(MrCyContext ctx = new MrCyContext()) {
				List<MenuSubClass> list = await ctx.MenuSubClass.ToListAsync();
				return Json(list, JsonRequestBehavior.AllowGet);
			}
		}
		public async Task<JsonResult> GetMenuDetail() {
			using(MrCyContext ctx = new MrCyContext()) {
				List<MenuDetail> list = await ctx.MenuDetail.ToListAsync();
				return Json(list, JsonRequestBehavior.AllowGet);
			}
		}
		public async Task<JsonResult> GetNote() {
			using(MrCyContext ctx = new MrCyContext()) {
				List<Note> list = await ctx.Note.ToListAsync();
				return Json(list, JsonRequestBehavior.AllowGet);
			}
		}
	}
}
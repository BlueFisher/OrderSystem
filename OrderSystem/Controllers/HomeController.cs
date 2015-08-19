using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Data.Entity;
using OrderSystem.Models;
using System.Configuration;

namespace OrderSystem.Controllers {
	public class HomeController : Controller {
		// GET: Home
		public ActionResult Index() {
			return View();
		}
		public ActionResult Partial(string id) {
			return View(id);
		}

		public JsonResult GetPayMethod() {
			return Json(new {
				canOfflinePay = ConfigurationManager.AppSettings["canOfflinePay"].ToString() == "True",
				canOnlinePay = ConfigurationManager.AppSettings["canOnlinePay"].ToString() == "True",
			});
		}

		public async Task<JsonResult> GetTable(GetTableViewModel model) {
			using(MrCyContext ctx = new MrCyContext()) {
				DeskInfo desk = await ctx.DeskInfo.Where(p => p.QRCode == model.qrCode).FirstOrDefaultAsync();
				Session["qrCode"] = desk.QRCode;
				return Json(desk);
			}
		}

		public async Task<JsonResult> GetMenuSubClass() {
			using(MrCyContext ctx = new MrCyContext()) {
				List<MenuSubClass> list = await ctx.MenuSubClass.ToListAsync();
				return Json(list);
			}
		}
		public async Task<JsonResult> GetMenuDetail() {
			using(MrCyContext ctx = new MrCyContext()) {
				List<MenuDetail> list = await ctx.MenuDetail.Where(p=>p.Usable == true && p.CanSelect == true).ToListAsync();
				return Json(list);
			}
		}
		public async Task<JsonResult> GetNote() {
			using(MrCyContext ctx = new MrCyContext()) {
				List<Note> list = await ctx.Note.ToListAsync();
				return Json(list);
			}
		}
		public JsonResult GetSetMeal() {
			using(MrCyContext ctx = new MrCyContext()) {
				List<MenuSet> setList = ctx.MenuSet.ToList();
				return Json(setList);
			}
		}
	}
}
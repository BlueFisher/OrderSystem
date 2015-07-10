using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OrderSystem.Models;
using System.Data.Entity;
using System.Threading.Tasks;

namespace OrderSystem.Controllers {
	public class CartController : Controller {

		public async Task<JsonResult> Submit(SubmitViewModel model) {
			DineTempInfo dti;
			using(MrCyContext ctx = new MrCyContext()) {
				string client = null;
				if(User.Identity.IsAuthenticated) {
					client = User.Identity.Name;
				}
				dti = new DineTempInfo() {
					ClientID = client,
					DeskID = model.Table.DeskId,
					Roomid = model.Table.RoomId,
					peoplecount = (short)model.Customer,
					IsPaid = model.IsPaid,
					PayKind = model.PayKind
				};
				ctx.DineTempInfo.Add(dti);
				await ctx.SaveChangesAsync();

				foreach(SubmitMenuDetail menu in model.Results) {
					string note = "";
					if(menu.Additional.Notes != null) {
						foreach(Note n in menu.Additional.Notes) {
							note += (n.Note1 + " ");
						}
					}

					DineTempDetail dtd = new DineTempDetail() {
						AutoID = dti.AutoID,
						DisherID = menu.DisherId,
						DisherNum = menu.Additional.Ordered,
						DisherPrice = (decimal)menu.DisherPrice,
						Note = note,
						SalesDiscount = menu.DisherDiscount
					};
					ctx.DineTempDetail.Add(dtd);
				}
				await ctx.SaveChangesAsync();

				DeskInfo d = await ctx.DeskInfo.Where(p => p.DeskId == model.Table.DeskId).FirstOrDefaultAsync();
				d.Status = 1;
				ctx.Entry<DeskInfo>(d).Property(p => p.Status).IsModified = true;
				await ctx.SaveChangesAsync();

			}
			Session["savedMenu"] = model;

			return Json(new JsonSucceedObj());
		}

		public async Task<JsonResult> GetPayName() {
			using(MrCyContext ctx = new MrCyContext()) {
				List<PayKind> list = await ctx.PayKind.Where(p => p.Usable == true && p.IsNetwork == true).ToListAsync();
				return Json(list);
			}
		}
		public JsonResult GetSavedMenu() {
			return Json((SubmitViewModel)Session["savedMenu"]);
		}

		public async Task<JsonResult> GetHistoryDineInfo() {
			if(!User.Identity.IsAuthenticated) {
				return Json(new JsonErrorObj());
			}
			string clientId = User.Identity.Name;
			using(MrCyContext ctx = new MrCyContext()) {
				List<DineInfoHistory> list = await ctx.DineInfoHistory.Where(p => p.ClientID == clientId).ToListAsync();
				return Json(list);
			}
		}
	}
}
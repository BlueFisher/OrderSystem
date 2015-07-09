using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OrderSystem.Models;
using System.Threading.Tasks;

namespace OrderSystem.Controllers {
	public class CartController : Controller {
		// GET: Cart
		public async Task<JsonResult> Submit(SubmitViewModel model) {
			DineTempInfo dti;
			using(MrCyContext ctx = new MrCyContext()) {
				dti = new DineTempInfo(){
					DeskID = model.table.DeskId,
					Roomid = model.table.RoomId,
					peoplecount = (short)model.customer
				};
				ctx.DineTempInfo.Add(dti);
				await ctx.SaveChangesAsync();

				foreach(SubmitMenuDetail menu in model.results){
					string note = "";
					if(menu.cart.notes != null) {
						foreach(Note n in menu.cart.notes) {
							note += (n.Note1 + " ");
						}
					}
					
					DineTempDetail dtd = new DineTempDetail() {
						AutoID = dti.AutoID,
						DisherID = menu.DisherId,
						DisherNum = menu.cart.ordered,
						DisherPrice = (decimal)menu.DisherPrice,
						Note = note,
						SalesDiscount = menu.DisherDiscount
					};
					ctx.DineTempDetail.Add(dtd);
				}
				await ctx.SaveChangesAsync();
			}
			return Json(null);
		}
	}
}
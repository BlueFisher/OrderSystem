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
				string client = null;
				if(User.Identity.IsAuthenticated) {
					client = User.Identity.Name;
				}
				dti = new DineTempInfo(){
					ClientID = client,
					DeskID = model.Table.DeskId,
					Roomid = model.Table.RoomId,
					peoplecount = (short)model.Customer
				};
				ctx.DineTempInfo.Add(dti);
				await ctx.SaveChangesAsync();

				foreach(SubmitMenuDetail menu in model.Results){
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
			}
			return Json(new JsonSucceedObj());
		}
	}
}
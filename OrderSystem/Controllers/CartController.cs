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
		public async Task<JsonResult> GetTablewareFeeFee() {
			using(MrCyContext ctx = new MrCyContext()) {
				BaseInfo info = await ctx.BaseInfo.Where(p => p.InfoName == "TablewareFee").FirstOrDefaultAsync();
				return Json(new {
					TablewareFee = info.InfoContent
				});
			}
		}

		public JsonResult GetSavedMenu() {
			SubmitViewModel tempMenu = (SubmitViewModel)Session["savedMenu"];
			if(tempMenu == null) {
				return Json(null);
			}
			HistoryMenuModel menuModel = new HistoryMenuModel() {
				PriceAll = (double)tempMenu.PriceAll,
				SizeAll = tempMenu.SizeAll,
				Table = tempMenu.Table,
				Customer = tempMenu.Customer,
				PayKind = tempMenu.PayKind,
				Results = new List<HistoryMenuDetail>()
			};
			foreach(SubmitMenuDetail menu in tempMenu.Results) {
				string note = "";
				if(menu.Additional.Notes != null) {
					foreach(Note n in menu.Additional.Notes) {
						note += (n.Note1 + " ");
					}
				}
				menuModel.Results.Add(new HistoryMenuDetail() {
					DisherId = menu.DisherId,
					DisherName = menu.DisherName,
					DisherDiscount = menu.DisherDiscount,
					DisherPrice = menu.DisherPrice,
					Note = note,
					Ordered = menu.Additional.Ordered
				});
			}
			return Json(menuModel);
		}

		public async Task<JsonResult> GetHistoryDineInfo() {
			if(!User.Identity.IsAuthenticated) {
				return Json(null);
			}
			string clientId = User.Identity.Name;
			using(MrCyContext ctx = new MrCyContext()) {
				List<DineInfoHistory> list = await ctx.DineInfoHistory.Where(p => p.ClientID == clientId).ToListAsync();
				List<HistoryDineInfoModel> infoList = new List<HistoryDineInfoModel>();
				foreach(DineInfoHistory info in list) {
					infoList.Add(new HistoryDineInfoModel() {
						CheckID = info.CheckID,
						BeginTime = info.BeginTime.ToString()
					});
				}
				return Json(infoList);
			}
		}
		public async Task<JsonResult> GetHistoryMenu(GetHistoryMenuViewModel model) {
			using(MrCyContext ctx = new MrCyContext()) {
				DineInfoHistory dineInfo = await ctx.DineInfoHistory.Where(p => p.CheckID == model.CheckID).FirstOrDefaultAsync();
				DeskInfo desk = await ctx.DeskInfo.Where(p => p.DeskId == dineInfo.DeskID).FirstOrDefaultAsync();
				HistoryMenuModel menuModel = new HistoryMenuModel() {
					CheckID = dineInfo.CheckID,
					BeginTime = dineInfo.BeginTime.ToString(),
					PriceAll = (double)dineInfo.Subtotal,
					Table = desk,
					Customer = (int)dineInfo.ClientNum,
					PayKind = dineInfo.PayKind,
					Results = new List<HistoryMenuDetail>()
				};
				List<DineDetailHistory> list = await ctx.DineDetailHistory.Where(p => p.CheckID == dineInfo.CheckID).ToListAsync();
				foreach(DineDetailHistory menu in list) {
					MenuDetail md = await ctx.MenuDetail.Where(p => p.DisherId == menu.DisherID).FirstOrDefaultAsync();
					HistoryMenuDetail newDetail = new HistoryMenuDetail() {
						DisherName = md.DisherName,
						DisherId = menu.DisherID,
						DisherPrice = menu.SalesPrice == null ? 0 : (double)menu.SalesPrice,
						DisherDiscount = (double)menu.SalesDiscount,
						Note = menu.Note,
						Ordered = (int)menu.DisherNum
					};
					menuModel.Results.Add(newDetail);
				}
				return Json(menuModel);
			}
		}
	}
}
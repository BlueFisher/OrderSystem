using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OrderSystem.Models;
using System.Data.Entity;
using System.Threading.Tasks;
using Com.Alipay;
using WeiPay;
using Newtonsoft.Json;
using System.Net;
using System.Text;

using System.Configuration;


namespace OrderSystem.Controllers {
	public class CartController : Controller {

		public ContentResult Submit(SubmitViewModel model) {
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
					IsPaid = 0,
					PayKind = model.PayKind,
					Subtotal = (decimal)model.PriceAll,
					Invoice = model.Bill
				};
				ctx.DineTempInfo.Add(dti);
				ctx.SaveChanges();

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
					// 增加已点人数
					MenuDetail m = ctx.MenuDetail.FirstOrDefault(p => p.DisherId == menu.DisherId);
					if(m.DisherPoint == null) {
						m.DisherPoint = 1;
					}
					else {
						m.DisherPoint++;
					}
					ctx.Entry<MenuDetail>(m).Property(p => p.DisherPoint).IsModified = true;
				}
				ctx.SaveChanges();

				DeskInfo d = ctx.DeskInfo.Where(p => p.DeskId == model.Table.DeskId).FirstOrDefault();
				d.Status = 1;
				ctx.Entry<DeskInfo>(d).Property(p => p.Status).IsModified = true;
				ctx.SaveChanges();

			}
			Session["savedMenu"] = model;
			Session["qrCode"] = model.Table.QRCode;

			string returnContent = "";
			if(model.PayKind == "支付宝") {

			}
			else if(model.PayKind == "微信支付") {
				int id = dti.AutoID;
				string hotelid = "";
				using(MrCyContext ctx = new MrCyContext()) {
					hotelid = ctx.BaseInfo.Where(p => p.InfoName == "HotelID").FirstOrDefault().InfoContent;
				}
				returnContent = String.Format(
					"http://www.choice.shu.edu.cn/weixin/Send.aspx?price={0}&hotelid={1}&id={2}&rediret={3}",
					Convert.ToInt32(Convert.ToDouble(dti.Subtotal.ToString()) * 100),
					hotelid,
					id,
					ConfigurationManager.AppSettings["WeixinRedirectUrl"].ToString()
				);
				PayController.StartTimer(id, hotelid);


			}

			return Content(returnContent);
		}



		public async Task<JsonResult> GetPayName() {
			using(MrCyContext ctx = new MrCyContext()) {
				List<PayKind> list = await ctx.PayKind.Where(p => p.Usable == true && p.IsNetwork == true).ToListAsync();
				return Json(list);
			}
		}
		public async Task<JsonResult> GetTablewareFee() {
			using(MrCyContext ctx = new MrCyContext()) {
				BaseInfo info = await ctx.BaseInfo.Where(p => p.InfoName == "HasTablewareFee").FirstOrDefaultAsync();
				if(info.InfoContent == "false") {
					return Json(new {
						TablewareFee = 0
					});
				}
				info = await ctx.BaseInfo.Where(p => p.InfoName == "TablewareFee").FirstOrDefaultAsync();
				return Json(new {
					TablewareFee = info.InfoContent
				});
			}
		}

		public async Task<JsonResult> GetSavedMenu(GetTableViewModel model) {
			SubmitViewModel tempMenu = (SubmitViewModel)Session["savedMenu"];
			if(tempMenu == null) {
				using(MrCyContext ctx = new MrCyContext()) {
					DeskInfo desk = await ctx.DeskInfo.Where(p => p.QRCode == model.qrCode).FirstOrDefaultAsync();
					DineTempInfo dineInfo = await ctx.DineTempInfo.Where(p => p.DeskID == desk.DeskId).FirstOrDefaultAsync();
					if(dineInfo == null) {
						return Json(null);
					}
					HistoryMenuModel menuModel = new HistoryMenuModel() {
						BeginTime = dineInfo.BeginTime.ToString(),
						PriceAll = (double)dineInfo.Subtotal,
						Table = desk,
						Customer = (int)dineInfo.peoplecount,
						PayKind = dineInfo.PayKind,
						Results = new List<HistoryMenuDetail>()
					};
					List<DineTempDetail> list = await ctx.DineTempDetail.Where(p => p.AutoID == dineInfo.AutoID).ToListAsync();
					foreach(DineTempDetail menu in list) {
						MenuDetail md = await ctx.MenuDetail.Where(p => p.DisherId == menu.DisherID).FirstOrDefaultAsync();
						HistoryMenuDetail newDetail = new HistoryMenuDetail() {
							DisherName = md.DisherName,
							DisherId = menu.DisherID,
							DisherPrice = menu.DisherPrice == null ? 0 : (double)menu.DisherPrice,
							DisherDiscount = (double)menu.SalesDiscount,
							Note = menu.Note,
							Ordered = (int)menu.DisherNum
						};
						menuModel.Results.Add(newDetail);
					}
					return Json(menuModel);
				}
			}
			HistoryMenuModel menuModelSession = new HistoryMenuModel() {
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
				menuModelSession.Results.Add(new HistoryMenuDetail() {
					DisherId = menu.DisherId,
					DisherName = menu.DisherName,
					DisherDiscount = menu.DisherDiscount,
					DisherPrice = menu.DisherPrice,
					Note = note,
					Ordered = menu.Additional.Ordered
				});
			}
			return Json(menuModelSession);
		}

		public async Task<JsonResult> GetHistoryDineInfo() {
			if(!User.Identity.IsAuthenticated) {
				return Json(null);
			}
			string clientId = User.Identity.Name;
			using(MrCyContext ctx = new MrCyContext()) {
				List<HistoryDineInfoModel> infoList = new List<HistoryDineInfoModel>();

				List<DineInfoHistory> list = await ctx.DineInfoHistory.Where(p => p.ClientID == clientId).ToListAsync();
				List<DineInfo> list2 = await ctx.DineInfo.Where(p => p.ClientID == clientId).ToListAsync();

				foreach(DineInfo info in list2) {
					infoList.Add(new HistoryDineInfoModel() {
						CheckID = info.CheckID,
						BeginTime = info.BeginTime.ToString()
					});
				}

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
				HistoryMenuModel menuModel;

				DineInfoHistory dineInfo = await ctx.DineInfoHistory.Where(p => p.CheckID == model.CheckID).FirstOrDefaultAsync();
				DineInfo dineInfo2 = await ctx.DineInfo.Where(p => p.CheckID == model.CheckID).FirstOrDefaultAsync();
				if(dineInfo != null) {
					DeskInfo desk = await ctx.DeskInfo.Where(p => p.DeskId == dineInfo.DeskID).FirstOrDefaultAsync();
					menuModel = new HistoryMenuModel() {
						CheckID = dineInfo.CheckID,
						BeginTime = dineInfo.BeginTime.ToString(),
						PriceAll = (double)dineInfo.Subtotal,
						Table = desk,
						Customer = (int)dineInfo.ClientNum,
						PayKind = dineInfo.PayKind,
						Results = new List<HistoryMenuDetail>()
					};
				}
				else {
					if(dineInfo2 == null) {
						return Json(null);
					}
					DeskInfo desk = await ctx.DeskInfo.Where(p => p.DeskId == dineInfo2.DeskID).FirstOrDefaultAsync();
					menuModel = new HistoryMenuModel() {
						CheckID = dineInfo2.CheckID,
						BeginTime = dineInfo2.BeginTime.ToString(),
						PriceAll = (double)dineInfo2.Subtotal,
						Table = desk,
						Customer = (int)dineInfo2.ClientNum,
						PayKind = dineInfo2.PayKind,
						Results = new List<HistoryMenuDetail>()
					};
				}



				if(dineInfo != null) {
					List<DineDetailHistory> list = await ctx.DineDetailHistory.Where(p => p.CheckID == dineInfo.CheckID).ToListAsync();
					foreach(DineDetailHistory menu in list) {
						MenuDetail md = await ctx.MenuDetail.Where(p => p.DisherId == menu.DisherID).FirstOrDefaultAsync();
						HistoryMenuDetail newDetail = new HistoryMenuDetail() {
							DisherName = md.DisherName,
							DisherId = menu.DisherID,
							DisherPrice = menu.DisherPrice == null ? 0 : (double)menu.DisherPrice,
							DisherDiscount = (double)menu.SalesDiscount,
							Note = menu.Note,
							Ordered = (int)menu.DisherNum
						};
						menuModel.Results.Add(newDetail);
					}
				}
				else {
					List<DineDetail> list2 = await ctx.DineDetail.Where(p => p.CheckID == dineInfo2.CheckID).ToListAsync();
					foreach(DineDetail menu in list2) {
						MenuDetail md = await ctx.MenuDetail.Where(p => p.DisherId == menu.DisherID).FirstOrDefaultAsync();
						HistoryMenuDetail newDetail = new HistoryMenuDetail() {
							DisherName = md.DisherName,
							DisherId = menu.DisherID,
							DisherPrice = menu.DisherPrice == null ? 0 : (double)menu.DisherPrice,
							DisherDiscount = (double)menu.SalesDiscount,
							Note = menu.Note,
							Ordered = (int)menu.DisherNum
						};
						menuModel.Results.Add(newDetail);
					}
				}

				return Json(menuModel);
			}
		}
	}
}
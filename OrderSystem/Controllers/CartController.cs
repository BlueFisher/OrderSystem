﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OrderSystem.Models;
using System.Data.Entity;
using System.Threading.Tasks;
using Com.Alipay;

namespace OrderSystem.Controllers {
	public class CartController : Controller {

		public async Task<ContentResult> Submit(SubmitViewModel model) {
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
					Subtotal = (decimal)model.PriceAll
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


			string payment_type = "1";
			//必填，不能修改
			//服务器异步通知页面路径
			string notify_url = "http://10.1.131.37/Pay/Completed";
			//需http://格式的完整路径，不能加?id=123这类自定义参数

			//页面跳转同步通知页面路径
			string return_url = "http://10.1.131.37/Pay/Indexx";
			//需http://格式的完整路径，不能加?id=123这类自定义参数，不能写成http://localhost/

			//商户订单号
			string out_trade_no = dti.AutoID.ToString();
			//商户网站订单系统中唯一订单号，必填

			//订单名称
			string subject = "上海乔曦信息技术有限";
			//必填

			//付款金额
			string price = dti.Subtotal.ToString();
			//必填

			//商品数量
			string quantity = "1";
			//必填，建议默认为1，不改变值，把一次交易看成是一次下订单而非购买一件商品
			//物流费用
			string logistics_fee = "0.00";
			//必填，即运费
			//物流类型
			string logistics_type = "EXPRESS";
			//必填，三个值可选：EXPRESS（快递）、POST（平邮）、EMS（EMS）
			//物流支付方式
			string logistics_payment = "SELLER_PAY";
			//必填，两个值可选：SELLER_PAY（卖家承担运费）、BUYER_PAY（买家承担运费）
			//订单描述

			string body = "";
			//商品展示地址
			string show_url = "";
			//需以http://开头的完整路径，如：http://www.商户网站.com/myorder.html

			//收货人姓名
			string receive_name = "";
			//如：张三

			//收货人地址
			string receive_address = "";
			//如：XX省XXX市XXX区XXX路XXX小区XXX栋XXX单元XXX号

			//收货人邮编
			string receive_zip = "";
			//如：123456

			//收货人电话号码
			string receive_phone = "";
			//如：0571-88158090

			//收货人手机号码
			string receive_mobile = "";
			//如：13312341234


			////////////////////////////////////////////////////////////////////////////////////////////////

			//把请求参数打包成数组
			SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
			sParaTemp.Add("partner", Config.Partner);
			sParaTemp.Add("seller_email", Config.Seller_email);
			sParaTemp.Add("_input_charset", Config.Input_charset.ToLower());
			sParaTemp.Add("service", "create_partner_trade_by_buyer");
			sParaTemp.Add("payment_type", payment_type);
			sParaTemp.Add("notify_url", notify_url);
			sParaTemp.Add("return_url", return_url);
			sParaTemp.Add("out_trade_no", out_trade_no);
			sParaTemp.Add("subject", subject);
			sParaTemp.Add("price", price);
			sParaTemp.Add("quantity", quantity);
			sParaTemp.Add("logistics_fee", logistics_fee);
			sParaTemp.Add("logistics_type", logistics_type);
			sParaTemp.Add("logistics_payment", logistics_payment);
			sParaTemp.Add("body", body);
			sParaTemp.Add("show_url", show_url);
			sParaTemp.Add("receive_name", receive_name);
			sParaTemp.Add("receive_address", receive_address);
			sParaTemp.Add("receive_zip", receive_zip);
			sParaTemp.Add("receive_phone", receive_phone);
			sParaTemp.Add("receive_mobile", receive_mobile);

			//建立请求

			string sHtmlText = Com.Alipay.Submit.BuildRequest(sParaTemp, "get", "确认");
			Response.Write(sHtmlText);


			return Content(sHtmlText);
		}



		public async Task<JsonResult> GetPayName() {
			using(MrCyContext ctx = new MrCyContext()) {
				List<PayKind> list = await ctx.PayKind.Where(p => p.Usable == true && p.IsNetwork == true).ToListAsync();
				return Json(list);
			}
		}
		public async Task<JsonResult> GetTablewareFee() {
			using(MrCyContext ctx = new MrCyContext()) {
				BaseInfo info = await ctx.BaseInfo.Where(p => p.InfoName == "TablewareFee").FirstOrDefaultAsync();
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
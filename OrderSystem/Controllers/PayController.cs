using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Com.Alipay;
using System.Collections.Specialized;
using OrderSystem.Models;
using System.Data.Entity;
using System.Diagnostics;

namespace OrderSystem.Controllers {
	public class PayController : Controller {
		// GET: Pay

		public ContentResult Completed() {
			System.IO.FileStream fs = new System.IO.FileStream("d:/temp.txt", System.IO.FileMode.Append);
			System.IO.StreamWriter sw = new System.IO.StreamWriter(fs);
			SortedDictionary<string, string> sPara = GetRequestPost();

			if(sPara.Count > 0) {//判断是否有带返回参数
				Notify aliNotify = new Notify();
				bool verifyResult = aliNotify.Verify(sPara, Request.Form["notify_id"], Request.Form["sign"]);

				if(verifyResult) {//验证成功
					/////////////////////////////////////////////////////////////////////////////////////////////////////////////
					//请在这里加上商户的业务逻辑程序代码


					//——请根据您的业务逻辑来编写程序（以下代码仅作参考）——
					//获取支付宝的通知返回参数，可参考技术文档中服务器异步通知参数列表

					//商户订单号

					string out_trade_no = Request.Form["out_trade_no"];

					//支付宝交易号

					string trade_no = Request.Form["trade_no"];

					//交易状态
					string trade_status = Request.Form["trade_status"];
					sw.WriteLine(trade_status);
					sw.WriteLine(out_trade_no);
					sw.Close();

					if(Request.Form["trade_status"] == "WAIT_BUYER_PAY") {//该判断表示买家已在支付宝交易管理中产生了交易记录，但没有付款

						//判断该笔订单是否在商户网站中已经做过处理
						//如果没有做过处理，根据订单号（out_trade_no）在商户网站的订单系统中查到该笔订单的详细，并执行商户的业务程序
						//如果有做过处理，不执行商户的业务程序
					
						return Content("success");
					}
					else if(Request.Form["trade_status"] == "WAIT_SELLER_SEND_GOODS") {//该判断示买家已在支付宝交易管理中产生了交易记录且付款成功，但卖家没有发货

						//判断该笔订单是否在商户网站中已经做过处理
						//如果没有做过处理，根据订单号（out_trade_no）在商户网站的订单系统中查到该笔订单的详细，并执行商户的业务程序
						//如果有做过处理，不执行商户的业务程序
						return Content("success");
					}
					else if(Request.Form["trade_status"] == "WAIT_BUYER_CONFIRM_GOODS") {//该判断表示卖家已经发了货，但买家还没有做确认收货的操作

						//判断该笔订单是否在商户网站中已经做过处理
						//如果没有做过处理，根据订单号（out_trade_no）在商户网站的订单系统中查到该笔订单的详细，并执行商户的业务程序
						//如果有做过处理，不执行商户的业务程序
						return Content("success");
					}
					else if(Request.Form["trade_status"] == "TRADE_FINISHED" || Request.Form["trade_status"] == "TRADE_SUCCESS") {//该判断表示买家已经确认收货，这笔交易完成

						//判断该笔订单是否在商户网站中已经做过处理
						//如果没有做过处理，根据订单号（out_trade_no）在商户网站的订单系统中查到该笔订单的详细，并执行商户的业务程序
						//如果有做过处理，不执行商户的业务程序

						

						using(MrCyContext ctx = new MrCyContext()) {
							DineTempInfo info = ctx.DineTempInfo.Where(p => p.AutoID == Convert.ToInt32(out_trade_no)).FirstOrDefault();
							info.IsPaid = 1;
							ctx.Entry<DineTempInfo>(info).Property(p => p.IsPaid).IsModified = true;
							ctx.SaveChanges();
						}

						return Content("success");
					}
					else {
						return Content("success");
					}

					//——请根据您的业务逻辑来编写程序（以上代码仅作参考）——

					/////////////////////////////////////////////////////////////////////////////////////////////////////////////
				}
				else {//验证失败
					
					sw.WriteLine("fail");
					sw.Close();
					return Content("success");
				}
			}
			else {
				
				sw.WriteLine("无通知参数");
				sw.Close();
				return Content("fail");
			}
			
		}

		public RedirectResult Index() {
			SortedDictionary<string, string> sPara = GetRequestGet();

			if(sPara.Count > 0) {//判断是否有带返回参数
				Notify aliNotify = new Notify();
				bool verifyResult = aliNotify.Verify(sPara, Request.QueryString["notify_id"], Request.QueryString["sign"]);

				if(verifyResult) {//验证成功
					/////////////////////////////////////////////////////////////////////////////////////////////////////////////
					//请在这里加上商户的业务逻辑程序代码


					//——请根据您的业务逻辑来编写程序（以下代码仅作参考）——
					//获取支付宝的通知返回参数，可参考技术文档中页面跳转同步通知参数列表

					//商户订单号

					string out_trade_no = Request.QueryString["out_trade_no"];

					//支付宝交易号

					string trade_no = Request.QueryString["trade_no"];

					//交易状态
					string trade_status = Request.QueryString["trade_status"];


					if(Request.QueryString["trade_status"] == "WAIT_SELLER_SEND_GOODS") {
						//判断该笔订单是否在商户网站中已经做过处理
						//如果没有做过处理，根据订单号（out_trade_no）在商户网站的订单系统中查到该笔订单的详细，并执行商户的业务程序
						//如果有做过处理，不执行商户的业务程序

						//支付宝交易号
						//string trade_no = Request.QueryString["trade_no"];
						//必填

						//物流公司名称
						string logistics_name = "test";
						//必填

						//物流发货单号
						string invoice_no = "test";
						//物流运输类型
						string transport_type = "EXPRESS";
						//三个值可选：POST（平邮）、EXPRESS（快递）、EMS（EMS）

						//把请求参数打包成数组
						SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
						sParaTemp.Add("partner", Config.Partner);
						sParaTemp.Add("_input_charset", Config.Input_charset.ToLower());
						sParaTemp.Add("service", "send_goods_confirm_by_platform");
						sParaTemp.Add("trade_no", trade_no);
						sParaTemp.Add("logistics_name", logistics_name);
						sParaTemp.Add("invoice_no", invoice_no);
						sParaTemp.Add("transport_type", transport_type);

						//建立请求
						string sHtmlText = Submit.BuildRequest(sParaTemp);
					}
					else {
						Response.Write("trade_status=" + Request.QueryString["trade_status"]);
					}

					//打印页面
					return Redirect("/#/onlinepaysuccess");

					//——请根据您的业务逻辑来编写程序（以上代码仅作参考）——

					/////////////////////////////////////////////////////////////////////////////////////////////////////////////
				}
				else {//验证失败
					return Redirect("/#/onlinepayfail");
				}
			}
			else {
				return Redirect("/#/onlinepayfail");
			}
		}


		#region 辅助函数
		public SortedDictionary<string, string> GetRequestPost() {
			int i = 0;
			SortedDictionary<string, string> sArray = new SortedDictionary<string, string>();
			NameValueCollection coll;
			//Load Form variables into NameValueCollection variable.
			coll = Request.Form;

			// Get names of all forms into a string array.
			String[] requestItem = coll.AllKeys;

			for(i = 0; i < requestItem.Length; i++) {
				sArray.Add(requestItem[i], Request.Form[requestItem[i]]);
			}
			return sArray;
		}
		public SortedDictionary<string, string> GetRequestGet() {
			int i = 0;
			SortedDictionary<string, string> sArray = new SortedDictionary<string, string>();
			NameValueCollection coll;
			//Load Form variables into NameValueCollection variable.
			coll = Request.QueryString;

			// Get names of all forms into a string array.
			String[] requestItem = coll.AllKeys;

			for(i = 0; i < requestItem.Length; i++) {
				sArray.Add(requestItem[i], Request.QueryString[requestItem[i]]);
			}

			return sArray;
		}
		#endregion
	}


}
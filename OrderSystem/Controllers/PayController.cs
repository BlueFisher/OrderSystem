using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Com.Alipay;
using System.Collections.Specialized;
using OrderSystem.Models;
using System.Data.Entity;
using WeiPay;
using Newtonsoft.Json;
using System.Xml;


namespace OrderSystem.Controllers {
	public class PayController : Controller {
		// GET: Pay

		public ContentResult Completed() {
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
							int id = Convert.ToInt32(out_trade_no);
							DineTempInfo info = ctx.DineTempInfo.Where(p => p.AutoID == id).FirstOrDefault();
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
					return Content("success");
				}
			}
			else {
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


		public static string Code = "";     //微信端传来的code
		public static string PrepayId = ""; //预支付ID
		public static string Sign = "";     //为了获取预支付ID的签名
		public static string PaySign = "";  //进行支付需要的签名
		public static string Package = "";  //进行支付需要的包
		public static string TimeStamp = ""; //时间戳 程序生成 无需填写
		public static string NonceStr = ""; //随机字符串  程序生成 无需填写

		protected string OrderSN = ""; //商户自己订单号
		protected string Body = ""; //商品描述
		protected string TotalFee = "";  //总支付金额，单位为：分，不能有小数
		protected string Attach = ""; //用户自定义参数，原样返回
		protected string UserOpenId = "";//微信用户openid

		public ActionResult Weixin() {
			this.OrderSN = this.Request.QueryString["OrderSN"];
			this.Body = this.Request.QueryString["Body"];
			this.TotalFee = this.Request.QueryString["TotalFee"];
			this.Attach = this.Request.QueryString["Attach"];
			this.UserOpenId = this.Request.QueryString["UserOpenId"];

			LogUtil.WriteLog("============ 单次支付开始 ===============");
			LogUtil.WriteLog(string.Format("传递支付参数：OrderSN={0}、Body={1}、TotalFee={2}、Attach={3}、UserOpenId={4}",
		   this.OrderSN, this.Body, this.TotalFee, this.Attach, this.UserOpenId));


			#region 支付操作============================


			#region 基本参数===========================
			//时间戳 
			TimeStamp = TenpayUtil.getTimestamp();
			//随机字符串 
			NonceStr = TenpayUtil.getNoncestr();

			//创建支付应答对象
			var packageReqHandler = new RequestHandler(HttpContext);
			//初始化
			packageReqHandler.init();

			//设置package订单参数  具体参数列表请参考官方pdf文档，请勿随意设置
			packageReqHandler.setParameter("body", this.Body); //商品信息 127字符
			packageReqHandler.setParameter("appid", PayConfig.AppId);
			packageReqHandler.setParameter("mch_id", PayConfig.MchId);
			packageReqHandler.setParameter("nonce_str", NonceStr.ToLower());
			packageReqHandler.setParameter("notify_url", PayConfig.NotifyUrl);
			packageReqHandler.setParameter("openid", this.UserOpenId);
			packageReqHandler.setParameter("out_trade_no", this.OrderSN); //商家订单号
			packageReqHandler.setParameter("spbill_create_ip", Request.UserHostAddress); //用户的公网ip，不是商户服务器IP
			packageReqHandler.setParameter("total_fee", this.TotalFee); //商品金额,以分为单位(money * 100).ToString()
			packageReqHandler.setParameter("trade_type", "JSAPI");
			if(!string.IsNullOrEmpty(this.Attach))
				packageReqHandler.setParameter("attach", this.Attach);//自定义参数 127字符

			#endregion

			#region sign===============================
			Sign = packageReqHandler.CreateMd5Sign("key", PayConfig.AppKey);
			LogUtil.WriteLog("WeiPay 页面  sign：" + Sign);
			#endregion

			#region 获取package包======================
			packageReqHandler.setParameter("sign", Sign);

			string data = packageReqHandler.parseXML();
			LogUtil.WriteLog("WeiPay 页面  package（XML）：" + data);

			string prepayXml = HttpUtil.Send(data, "https://api.mch.weixin.qq.com/pay/unifiedorder");
			LogUtil.WriteLog("WeiPay 页面  package（Back_XML）：" + prepayXml);

			//获取预支付ID
			var xdoc = new XmlDocument();
			xdoc.LoadXml(prepayXml);
			XmlNode xn = xdoc.SelectSingleNode("xml");
			XmlNodeList xnl = xn.ChildNodes;
			if(xnl.Count > 7) {
				PrepayId = xnl[7].InnerText;
				Package = string.Format("prepay_id={0}", PrepayId);
				LogUtil.WriteLog("WeiPay 页面  package：" + Package);
			}
			#endregion

			#region 设置支付参数 输出页面  该部分参数请勿随意修改 ==============
			var paySignReqHandler = new RequestHandler(HttpContext);
			paySignReqHandler.setParameter("appId", PayConfig.AppId);
			paySignReqHandler.setParameter("timeStamp", TimeStamp);
			paySignReqHandler.setParameter("nonceStr", NonceStr);
			paySignReqHandler.setParameter("package", Package);
			paySignReqHandler.setParameter("signType", "MD5");
			PaySign = paySignReqHandler.CreateMd5Sign("key", PayConfig.AppKey);

			LogUtil.WriteLog("WeiPay 页面  paySign：" + PaySign);
			#endregion
			#endregion

			ViewBag.AppId = WeiPay.PayConfig.AppId;
			ViewBag.TimeStamp = TimeStamp;
			ViewBag.NonceStr = NonceStr;
			ViewBag.Package = Package;
			ViewBag.PaySign = PaySign;
			return View();
		}
		public void WeixinCompleted() {
			//创建ResponseHandler实例
			ResponseHandler resHandler = new ResponseHandler(HttpContext);
			resHandler.setKey(PayConfig.AppKey);

			//判断签名
			try {
				string error = "";
				if(resHandler.isWXsign(out error)) {
					#region 协议参数=====================================
					//--------------协议参数--------------------------------------------------------
					//SUCCESS/FAIL此字段是通信标识，非交易标识，交易是否成功需要查
					string return_code = resHandler.getParameter("return_code");
					//返回信息，如非空，为错误原因签名失败参数格式校验错误
					string return_msg = resHandler.getParameter("return_msg");
					//微信分配的公众账号 ID
					string appid = resHandler.getParameter("appid");

					//以下字段在 return_code 为 SUCCESS 的时候有返回--------------------------------
					//微信支付分配的商户号
					string mch_id = resHandler.getParameter("mch_id");
					//微信支付分配的终端设备号
					string device_info = resHandler.getParameter("device_info");
					//微信分配的公众账号 ID
					string nonce_str = resHandler.getParameter("nonce_str");
					//业务结果 SUCCESS/FAIL
					string result_code = resHandler.getParameter("result_code");
					//错误代码 
					string err_code = resHandler.getParameter("err_code");
					//结果信息描述
					string err_code_des = resHandler.getParameter("err_code_des");

					//以下字段在 return_code 和 result_code 都为 SUCCESS 的时候有返回---------------
					//-------------业务参数---------------------------------------------------------
					//用户在商户 appid 下的唯一标识
					string openid = resHandler.getParameter("openid");
					//用户是否关注公众账号，Y-关注，N-未关注，仅在公众账号类型支付有效
					string is_subscribe = resHandler.getParameter("is_subscribe");
					//JSAPI、NATIVE、MICROPAY、APP
					string trade_type = resHandler.getParameter("trade_type");
					//银行类型，采用字符串类型的银行标识
					string bank_type = resHandler.getParameter("bank_type");
					//订单总金额，单位为分
					string total_fee = resHandler.getParameter("total_fee");
					//货币类型，符合 ISO 4217 标准的三位字母代码，默认人民币：CNY
					string fee_type = resHandler.getParameter("fee_type");
					//微信支付订单号
					string transaction_id = resHandler.getParameter("transaction_id");
					//商户系统的订单号，与请求一致。
					string out_trade_no = resHandler.getParameter("out_trade_no");
					//商家数据包，原样返回
					string attach = resHandler.getParameter("attach");
					//支 付 完 成 时 间 ， 格 式 为yyyyMMddhhmmss，如 2009 年12 月27日 9点 10分 10 秒表示为 20091227091010。时区为 GMT+8 beijing。该时间取自微信支付服务器
					string time_end = resHandler.getParameter("time_end");

					#endregion
					//支付成功
					if(!out_trade_no.Equals("") && return_code.Equals("SUCCESS") && result_code.Equals("SUCCESS")) {
						LogUtil.WriteLog("Notify 页面  支付成功，支付信息：商家订单号：" + out_trade_no + "、支付金额(分)：" + total_fee + "、自定义参数：" + attach);

						/**
						 *  这里输入用户逻辑操作，比如更新订单的支付状态
						 * 
						 * **/

						LogUtil.WriteLog("============ 单次支付结束 ===============");
						using(MrCyContext ctx = new MrCyContext()) {
							int id = Convert.ToInt32(out_trade_no);
							DineTempInfo info = ctx.DineTempInfo.Where(p => p.AutoID == id).FirstOrDefault();
							info.IsPaid = 1;
							ctx.Entry<DineTempInfo>(info).Property(p => p.IsPaid).IsModified = true;
							ctx.SaveChanges();
						}
						Response.Write("success");
						return;
					}
					else {
						LogUtil.WriteLog("Notify 页面  支付失败，支付信息   total_fee= " + total_fee + "、err_code_des=" + err_code_des + "、result_code=" + result_code);
					}
				}
				else {
					LogUtil.WriteLog("Notify 页面  isWXsign= false ，错误信息：" + error);
				}


			}
			catch(Exception ee) {
				LogUtil.WriteLog("Notify 页面  发送异常错误：" + ee.Message);
			}

			Response.End();
		}
	}


}
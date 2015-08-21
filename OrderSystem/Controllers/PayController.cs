using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OrderSystem.Models;
using System.Data.Entity;
using System.Timers;
using System.IO;
using System.Net;
using System.Text;
using AutoPrint;

namespace OrderSystem.Controllers {
	public class PayController : Controller {
		// GET: Pay
        private static void autoPrint() {
            try {
                string conn = System.Configuration.ConfigurationManager.ConnectionStrings["sqlString"].ConnectionString;
                new PrintDineMenu(conn).Print();
            }
            catch(Exception e) {
                FileStream fs = new FileStream("d:/dll.txt", FileMode.Append);
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine(e);
                sw.Close();
            }
        }
		public ActionResult WeixinCompleted() {
			if(Request.QueryString["id"] == null) {
				return Redirect("/#/");
			}
			if(Request.QueryString["cancel"].ToString() == "1") {
				using(MrCyContext ctx = new MrCyContext()) {
					int id = Convert.ToInt32(Request.QueryString["id"]);
					log(id.ToString());
					DineTempInfo info = ctx.DineTempInfo.Where(p => p.AutoID == id).FirstOrDefault();
					if(info != null) {
						info.IsPaid = 1;
						ctx.Entry<DineTempInfo>(info).Property(p => p.IsPaid).IsModified = true;
						ctx.SaveChanges();
						log("点击返回修改1");
                        autoPrint();
					}
					else {
						log("点击返回未找到订单");
					}
				}
				return Redirect("/#/onlinepaysuccess?qrCode=" + Session["qrCode"]);
			}
			else {
				return Redirect("/#/onlinepayfail?qrCode=" + Session["qrCode"]);
			}
		}
		public static void StartTimer(int id, string hotelid) {
			Timer t = new Timer(1000 * 10);
			t.Elapsed += (object sender, ElapsedEventArgs e) => {
				log("订单号：" + id);
				log(DateTime.Now.ToLocalTime().ToString());
				using(MrCyContext ctx = new MrCyContext()) {
					DineTempInfo info = ctx.DineTempInfo.Where(p => p.AutoID == id).FirstOrDefault();
					if(info == null) {
						log("计时器 未找到订单");
						((Timer)sender).Stop();
						return;
					}
					else if(info.IsPaid == 1) {
						log("订单已经支付");
						((Timer)sender).Stop();
						return;
					}
				}
				try {
					string result = HttpGet(String.Format("http://www.choice.shu.edu.cn/weixin/NotifyLocal.aspx?id={0}&hotelid={1}",
						id,
						hotelid
					));
					
					
					log("接收到" + result);
					result = result.Trim();
					if(result == "1") {
						using(MrCyContext ctx = new MrCyContext()) {
							DineTempInfo info = ctx.DineTempInfo.Where(p => p.AutoID == id).FirstOrDefault();
							log(info.AutoID.ToString());
							info.IsPaid = 1;
							ctx.Entry<DineTempInfo>(info).Property(p => p.IsPaid).IsModified = true;
							ctx.SaveChanges();
						}
						log("计时器修改1");
                        autoPrint();
						log("支付成功");
						((Timer)sender).Stop();
					}
					else if(result == "0") {
						log("支付失败");
						((Timer)sender).Stop();
					}
					else {
						log("未支付");
					}
				}
				catch(Exception error) {
					log(error.ToString());
				}
			};
			t.Start();
		}
		private static string HttpGet(string Url) {
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
			request.Method = "GET";
			request.ContentType = "text/plain;charset=UTF-8";

			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			Stream myResponseStream = response.GetResponseStream();
			StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
			string retString = myStreamReader.ReadToEnd();
			myStreamReader.Close();
			myResponseStream.Close();

			return retString;
		}

		private static void log(string message) {
			FileStream fs = new FileStream("d:/log.txt", FileMode.Append);
			StreamWriter sw = new StreamWriter(fs);
			sw.WriteLine(message);
			sw.Close();
		}
	}
}
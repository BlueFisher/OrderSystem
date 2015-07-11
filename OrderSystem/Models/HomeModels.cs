using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OrderSystem.Models {
	public class GetTableViewModel {
		public string qrCode { get; set; }
	}
	public class GetHistoryMenuViewModel {
		public string CheckID { get; set; }
	}

	public class HistoryDineInfoModel {
		public string CheckID { get; set; }
		public string BeginTime { get; set; }
	}

	public class HistoryMenuModel {
		public string CheckID { get; set; }
		public string BeginTime { get; set; }
		public double PriceAll { get; set; }
		public int SizeAll { get; set; }
		public List<HistoryMenuDetail> Results { get; set; }
		public DeskInfo Table { get; set; }
		public int Customer { get; set; }
		public string PayKind { get; set; }
	}
	public class HistoryMenuDetail {
		public string DisherName { get; set; }
		public string DisherId { get; set; }
		public double DisherPrice { get; set; }
		public double DisherDiscount { get; set; }
		public string Note { get; set; }
		public int Ordered { get; set; }
	}
}
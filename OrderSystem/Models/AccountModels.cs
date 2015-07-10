using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OrderSystem.Models {
	public class SMSSendViewModel {
		public string Mobile { get; set; }
	}
	public class SignupViewModel {
		public string Mobile { get; set; }
		public string Code { get; set; }
		public string Password { get; set; }
	}

	public class SigninViewModel {
		public string Mobile { get; set; }
		public string Password { get; set; }
	}

	public class HistoryMenuModel {
		public string CheckID { get; set; }
		public string BeginTime { get; set; }
		public int SizeAll { get; set; }
		public double PriceAll { get; set; }
		public List<SubmitMenuDetail> Results { get; set; }
		public DeskInfo Table { get; set; }
		public int Customer { get; set; }
		public string Bill { get; set; }
		public string PayKind { get; set; }
	}
	public class HistoryMenuDetail {
		public string DisherName { get; set; }
		public string DisherId { get; set; }
		public double DisherPrice { get; set; }
		public double DisherDiscount { get; set; }
		public HistoryMenuAdditional Additional { get; set; }
	}
	public class HistoryMenuAdditional {
		public int Ordered { get; set; }
		public List<Note> Notes { get; set; }
	}
}
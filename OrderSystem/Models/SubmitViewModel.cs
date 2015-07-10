using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OrderSystem.Models {
	public class SubmitViewModel {
		public int SizeAll { get; set; }
		public double PriceAll { get; set; }
		public List<SubmitMenuDetail> Results { get; set; }
		public DeskInfo Table { get; set; }
		public int Customer { get; set; }
		public string Bill { get; set; }
		public int IsPaid { get; set; }
		public string PayKind { get; set; }
	}

	public class SubmitMenuDetail {
		public string DisherName { get; set; }
		public string DisherId { get; set; }
		public double DisherPrice { get; set; }
		public double DisherDiscount { get; set; }
		public SubmitMenuAdditional Additional { get; set; }
	}
	public class SubmitMenuAdditional {
		public int Ordered { get; set; }
		public List<Note> Notes { get; set; }
	}
}
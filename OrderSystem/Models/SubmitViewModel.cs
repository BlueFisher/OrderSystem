using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OrderSystem.Models {
	public class SubmitViewModel {
		public int sizeAll { get; set; }
		public double priceAll { get; set; }
		public List<SubmitMenuDetail> results { get; set; }
		public DeskInfo table { get; set; }
		public int customer { get; set; }
		public string bill { get; set; }
	}

	public class SubmitMenuDetail {
		public string DisherId { get; set; }
		public double DisherPrice { get; set; }
		public double DisherDiscount { get; set; }
		public SubmitMenuCart cart { get; set; }
	}
	public class SubmitMenuCart {
		public int ordered { get; set; }
		public List<Note> notes { get; set; }
	}
}
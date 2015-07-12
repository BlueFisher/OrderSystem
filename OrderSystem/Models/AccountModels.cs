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
		public string CodeImg { get; set; }
	}

	
}
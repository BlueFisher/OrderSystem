using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OrderSystem.Models {
	public class JsonErrorObj {
		public JsonErrorObj() { }
		public JsonErrorObj(string errorMessage) {
			ErrorMessage = errorMessage;
		}
		public JsonErrorObj(string errorMessage, string errorPosition) {
			ErrorMessage = errorMessage;
			ErrorPosition = errorPosition;
		}
		public JsonErrorObj(ModelStateDictionary model) {
			foreach(KeyValuePair<string, ModelState> item in model) {
				ModelErrorCollection errors = item.Value.Errors;
				if(errors.Count > 0) {
					ErrorMessage = errors[0].ErrorMessage;
					ErrorPosition = item.Key;
					break;
				}
			}
		}
		public bool IsSucceed = false;
		public string ErrorMessage;
		public string ErrorPosition;
	}
	public class JsonSucceedObj {
		public bool IsSucceed = true;
	}
}
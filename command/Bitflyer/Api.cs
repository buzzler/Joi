using System;
using LitJson;
using System.Net;

namespace Joi.Bitflyer
{
	public class Api
	{
		public Api ()
		{
		}

		public	JsonData GetMarketList()
		{
			return _GetCommonResponse ("getmarkets");
		}

		public	JsonData GetOrderBook(string product_code = null)
		{
			if (string.IsNullOrEmpty (product_code))
				return _GetCommonResponse ("getboard");
			else
				return _GetCommonResponse (string.Format ("getboard?product_code={0}", product_code));
		}

		public	JsonData GetTicker(string product_code = null)
		{
			if (string.IsNullOrEmpty (product_code))
				return _GetCommonResponse ("getticker");
			else
				return _GetCommonResponse (string.Format ("getticker?product_code={0}", product_code));
		}

		public	JsonData GetExecutionHistory(string product_code = null, int count = -1, int before = -1, int after = -1)
		{
			var sb = new System.Text.StringBuilder ();
			if (product_code != null) {
				sb.AppendFormat ("product_code={0}", product_code);
			}
			if (count > 0) {
				if (sb.Length > 0)
					sb.Append ("&");
				sb.AppendFormat ("count={0}", count);
			}
			if (before > 0) {
				if (sb.Length > 0)
					sb.Append ("&");
				sb.AppendFormat ("before={0}", before);
			}
			if (after > 0) {
				if (sb.Length > 0)
					sb.Append ("&");
				sb.AppendFormat ("after={0}", after);
			}
			return _GetCommonResponse (string.Format ("getexecutions?{0}", sb.ToString ()));
		}

		public	JsonData GetExchangeStatus(string product_code = null)
		{
			if (string.IsNullOrEmpty (product_code))
				return _GetCommonResponse ("gethealth");
			else
				return _GetCommonResponse (string.Format ("gethealth?product_code={0}", product_code));
		}

		public	JsonData GetChat(TimeSpan timespan)
		{
			return _GetCommonResponse (string.Format ("getchats?from_date={0}", DateTime.Now.Subtract (timespan).ToString ("yyyy-MM-ddTHH:mm:ss.fff")));
		}

		private	JsonData _GetCommonResponse(string uri)
		{
			var url = string.Format ("{0}/{1}", Key.URL_JP, uri);
			var request = WebRequest.Create (url);
			return Utility.GetResponse (request);
		}
	}
}


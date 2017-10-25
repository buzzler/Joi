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

		public	JsonData GetExecutionHistory(int count = 100, string product_code = null)
		{
			if (string.IsNullOrEmpty (product_code))
				return _GetCommonResponse (string.Format("getexecutions?count={0}", count));
			else
				return _GetCommonResponse (string.Format ("getexecutions?count={0}&product_code={1}", count, product_code));
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


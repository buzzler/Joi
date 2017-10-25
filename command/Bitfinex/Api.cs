using System;
using LitJson;
using System.Net;

namespace Joi.Bitfinex
{
	public class Api
	{
		public	Api()
		{
		}

		public	JsonData GetTicker(string symbol)
		{
			return _GetCommonResponse (string.Format("pubticker/{0}", symbol));
		}

		public	JsonData GetStats(string symbol)
		{
			return _GetCommonResponse (string.Format("stats/{0}", symbol));
		}

		public	JsonData GetFundingBook(string currency, int limit_bids = 50, int limit_asks = 50)
		{
			return _GetCommonResponse (string.Format("lendbook/{0}?limit_bids={1}&limit_asks={2}", currency, limit_bids, limit_asks));
		}

		public	JsonData GetOrderBook(string symbol, int limit_bids = 50, int limit_asks = 50, int group = 1)
		{
			return _GetCommonResponse (string.Format("book/{0}?limit_bids={1}&limit_asks={2}&group={2}", symbol, limit_bids, limit_asks, group));
		}

		public	JsonData GetTrades(string symbol, int limit_trades = 50)
		{
			return _GetCommonResponse (string.Format ("trades/{0}?limit_trades={1}", symbol, limit_trades));
		}

		public	JsonData GetTrades(string symbol, DateTime timestamp, int limit_trades = 50)
		{
			return _GetCommonResponse (string.Format ("trades/{0}?limit_trades={1}&timestamp={2}", symbol, limit_trades, Utility.Timestamp (timestamp)));
		}

		public	JsonData GetLends(string currency, int limit_lends = 50)
		{
			return _GetCommonResponse (string.Format ("lends/{0}?limit_lends={1}", currency, limit_lends));
		}

		public	JsonData GetLends(string currency, DateTime timestamp, int limit_lends = 50)
		{
			return _GetCommonResponse (string.Format ("lends/{0}?limit_lends={1}&timestamp={2}", currency, limit_lends, Utility.Timestamp (timestamp)));
		}

		public	JsonData GetSymbols()
		{
			return _GetCommonResponse ("symbols");
		}

		public	JsonData GetSymbolDetails()
		{
			return _GetCommonResponse ("symbols_details");
		}

		private	JsonData _GetCommonResponse(string uri)
		{
			var url = string.Format ("{0}/{1}", Key.URL, uri);
			var request = WebRequest.Create (url);
			return Utility.GetResponse (request);
		}
	}
}


using System;
using Joi.Bitfinex;
using LitJson;
using Joi.Data;
using WebSocketSharp;

namespace Joi.Brain
{
	public class CrawlerBitfinex : CrawlerLogic
	{
		private	Api _api;
		private	Market _market;
		private	string _symbol;

		public	CrawlerBitfinex(Symbol symbol, bool logging = true) : base ("Bitfinex", Joi.Bitfinex.Limit.QUERY_TIMEOUT, logging)
		{
			_api = new Api ();
			_market = new Market (name);
			switch (symbol) {
			case Symbol.BITCOIN:
				_symbol = "btcusd";
				break;
			case Symbol.ETHEREUM:
				_symbol = "ethusd";
				break;
			}
		}

		protected override void OnEntryInit ()
		{
		}

		protected override void OnLoopInit ()
		{
			Fire (TRIGGER_COMPLETE);
		}

		protected override void OnExitInit ()
		{
		}

		protected override void OnEntryGather ()
		{
			// for WebSocket
			_api.Connect (OnSocketError);
			_api.SubscribeTrade (_symbol, OnSubscribedTrade);
			_api.SubscribeOrderBook (_symbol, OnSubscribeOrderBook);
			_api.SubscribeTicker (_symbol, OnSubscribeTicker);
		}

		protected override void OnLoopGather ()
		{
			// for RestAPI
//			GetTrade (_market.GetLastTimestamp ());
//			var json2 = _api.GetOrderBook ("btcusd");
//			var json3 = _api.GetTicker ("btcusd");
		}

		protected override void OnExitGather ()
		{
			_api.UnsubscribeTrade ();
			_api.UnsubscribeTicker ();
			_api.UnsubscribeOrderBook ();
			_api.Disconnect ();
		}

		protected override void OnEntryStop ()
		{
		}

		protected override void OnLoopStop ()
		{
		}

		protected override void OnExitStop ()
		{
		}

		private	void GetTradeByWeb(int timestamp)
		{
			var trades = _api.GetTrades (_symbol, timestamp);
			if (trades == null) {
				Fire (TRIGGER_STOP);
				return;
			}
			if (!trades.IsArray)
				return;

			_market.BegineUpdate ();
			var count = trades.Count;
			for (int i = 0; i < count; i++) {
				var trade = trades [i];
				_market.UpdateTrade (
					int.Parse (trade ["tid"].ToString ()),
					double.Parse (trade ["price"].ToString ()),
					double.Parse (trade ["amount"].ToString ()),
					int.Parse (trade ["timestamp"].ToString ())
				);
			}
			_market.EndUpdate ();
		}

		private	void OnSocketError(string message)
		{
			Fire (TRIGGER_STOP);
		}

		private	void OnSubscribedTrade(JsonData json)
		{
			var len = json.Count;
			if (len < 4)
				return;

			var jsonId = json[len-4];
			var valueId = jsonId.ToString().Trim();
			int id = -1;
			if (jsonId.IsInt)
				id = int.Parse (valueId);
			else if (jsonId.IsString)
				return;
			if (id < 0)
				return;

			_market.BegineUpdate ();
			_market.UpdateTrade(
				id,
				float.Parse(json[len-2].ToString()),
				float.Parse(json[len-1].ToString()),
				int.Parse(json[len-3].ToString())
			);
			_market.EndUpdate ();
		}

		private	void OnSubscribeOrderBook(JsonData json)
		{
//			Console.WriteLine ("order: {0}", json.ToJson ());
		}

		private	void OnSubscribeTicker(JsonData json)
		{
//			Console.WriteLine ("ticker: {0}", json.ToJson ());
		}
	}
}


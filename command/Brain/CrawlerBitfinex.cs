using System;
using Joi.Bitfinex;
using LitJson;
using Joi.Data;

namespace Joi.Brain
{
	public class CrawlerBitfinex : CrawlerLogic
	{
		private	Api _api;
		private	Market _market;
		private	string _symbol;

		public	CrawlerBitfinex(Symbol symbol, bool logging = true) : base ("Bitfinex", Joi.Bitfinex.Limit.TRADES_TIMEOUT, logging)
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
			var yesterday = Utility.Timestamp (DateTime.Now.AddDays(1));
			GetTrade (yesterday);
			Fire (TRIGGER_COMPLETE);
		}

		protected override void OnExitInit ()
		{
		}

		protected override void OnEntryGather ()
		{
		}

		protected override void OnLoopGather ()
		{
			GetTrade (_market.GetLastTimestamp ());
//			var json2 = _api.GetOrderBook ("btcusd");
//			var json3 = _api.GetTicker ("btcusd");
		}

		protected override void OnExitGather ()
		{
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

		private	void GetTrade(int timestamp)
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
	}
}


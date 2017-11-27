using System;
using Joi.Bitflyer;
using LitJson;
using Joi.Data;

namespace Joi.Brain
{
	public class CrawlerBitflyer : CrawlerLogic
	{
		private	Api _api;
		private	string _productCode;

		public CrawlerBitflyer (Symbol symbol, bool logging = true) : base (BITFLYER, Joi.Bitflyer.Limit.QUERY_TIMEOUT, logging)
		{
			_api = new Api ();
			_market = new Market (name, TimeInterval.DAY_3);
			_market.SetIndicator (TimeInterval.MINUTE_1, TimeInterval.HOUR_5);
			_market.SetIndicator (TimeInterval.MINUTE_15, TimeInterval.DAY_3);

			switch (symbol) {
			case Symbol.BITCOIN:
				_productCode = "BTC_JPY";
				break;
			case Symbol.ETHEREUM:
				_productCode = "ETH_BTC";
				break;
			}
		}

		protected override void OnEntryInit ()
		{
		}

		protected override void OnLoopInit ()
		{
			GetTrade (10000);
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
			base.OnLoopGather ();
			GetTrade (-1, _market.GetLastId ());
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

		private	void GetTrade (int count = -1, int after = -1)
		{
			var trades = _api.GetExecutionHistory (_productCode, count, -1, after);
			if (trades == null)
				return;
			if (!trades.IsArray)
				return;

			var total = trades.Count;
			for (int i = 0; i < total; i++) {
				var trade = trades [i];
				_market.ReserveTrade (
					int.Parse (trade ["id"].ToString ()),
					double.Parse (trade ["price"].ToString ()),
					double.Parse (trade ["size"].ToString ()),
					Utility.Timestamp (DateTime.Parse (trade ["exec_date"].ToString ()))
				);
			}
			_market.FlushTrade ();
			_market.UpdateChart ();
		}

		protected override void GetTicker ()
		{
			var ticker = _api.GetTicker (_productCode);
			if (ticker == null)
				return;
			_market.ticker.Update (
				float.Parse (ticker ["best_bid"].ToString ()),
				float.Parse (ticker ["best_bid_size"].ToString ()),
				float.Parse (ticker ["best_ask"].ToString ()),
				float.Parse (ticker ["best_ask_size"].ToString ()),
				float.Parse (ticker ["volume"].ToString ())
			);
		}
	}
}


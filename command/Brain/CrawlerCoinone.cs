using System;
using Joi.Coinone;
using Joi.Data;
using LitJson;

namespace Joi.Brain
{
	public class CrawlerCoinone : CrawlerLogic
	{
		private	Api _api;
		private	string _currency;

		public CrawlerCoinone (Symbol symbol, bool logging = true) : base (COINONE, Joi.Coinone.Limit.QUERY_TIMEOUT, logging)
		{
			_api = new Api ();
			_market = new Market (name, TimeInterval.DAY_3);
			_market.SetIndicator (TimeInterval.MINUTE_1, TimeInterval.HOUR_5);
			_market.SetIndicator (TimeInterval.MINUTE_5, TimeInterval.HOUR_30);
			_market.SetIndicator (TimeInterval.MINUTE_15, TimeInterval.DAY_3);

			// convert symbol
			switch (symbol) {
			case Symbol.BITCOIN:
				_currency = "btc";
				break;
			case Symbol.ETHEREUM:
				_currency = "eth";
				break;
			}
		}

		protected override void OnEntryInit ()
		{
		}

		protected override void OnLoopInit ()
		{
			GetTrade ("day");
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
			GetTrade ();
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

		private	void GetTrade (string period = "hour")
		{
			var json = _api.GetCompleteOrders (_currency, period);
			if (json == null)
				return;
			var trades = json ["completeOrders"];
			if (trades == null)
				return;
			if (!trades.IsArray)
				return;

			var count = trades.Count;
			for (int i = 0; i < count; i++) {
				var trade = trades [i];
				_market.ReserveTrade (
					int.Parse (trade ["timestamp"].ToString ()),
					double.Parse (trade ["price"].ToString ()),
					double.Parse (trade ["qty"].ToString ()),
					int.Parse (trade ["timestamp"].ToString ())
				);
			}
			_market.FlushTrade ();
			_market.UpdateChart ();
		}

		protected override void GetTicker ()
		{
			var json = _api.GetTicker (_currency);
			if (json == null)
				return;

			_market.ticker.Update (
				float.Parse (json ["high"].ToString ()),
				0f,
				float.Parse (json ["low"].ToString ()),
				0f,
				float.Parse (json ["volume"].ToString ())
			);
		}

		protected override void GetBalance()
		{
			var json = _api.GetBalance ();
			if (json == null)
				return;
			var btc = json ["btc"];
			var eth = json ["eth"];
			var won = json ["krw"];
			_market.balance.SetValue (Symbol.BITCOIN, double.Parse (btc ["balance"].ToString ()), double.Parse (btc ["avail"].ToString ()));
			_market.balance.SetValue (Symbol.ETHEREUM, double.Parse (eth ["balance"].ToString ()), double.Parse (eth ["avail"].ToString ()));
			_market.balance.SetValue (Symbol.KR_WON, double.Parse (won ["balance"].ToString ()), double.Parse (won ["avail"].ToString ()));
		}
	}
}
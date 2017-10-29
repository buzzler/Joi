using System;
using Joi.Coinone;
using Joi.Data;
using LitJson;

namespace Joi.Brain
{
	public class CrawlerCoinone : CrawlerLogic
	{
		private	Api _api;
		private	Market _market;
		private	string _currency;

		public CrawlerCoinone (Symbol symbol, bool logging = true) : base("Coinone", Joi.Coinone.Limit.QUERY_TIMEOUT, logging)
		{
			_api = new Api ();
			_market = new Market (name);

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

		protected override void OnEntryInit()
		{
		}

		protected override void OnLoopInit ()
		{
			GetTradeDay ("day");
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
			GetTradeDay ();
//			var json = _api.GetOrderbook ("btc");
//			var json = _api.GetTicker ("btc");
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

		private	void GetTradeDay(string period = "hour")
		{
			var json = _api.GetCompleteOrders (_currency, period);
			if (json == null) {
				Fire (TRIGGER_STOP);
				return;
			}
			var trades = json ["completeOrders"];
			if (trades == null)
				return;
			if (!trades.IsArray)
				return;

			_market.BegineUpdate ();
			var count = trades.Count;
			for (int i = 0; i < count; i++) {
				var trade = trades [i];
				_market.UpdateTrade (
					int.Parse (trade ["timestamp"].ToString ()),
					double.Parse (trade ["price"].ToString ()),
					double.Parse (trade ["qty"].ToString ()),
					int.Parse (trade ["timestamp"].ToString ())
				);
			}
			_market.EndUpdate ();
		}
	}
}


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
		private	float _ratio;
		private	float _count;

		public CrawlerCoinone (Symbol symbol, bool logging = true) : base ("Coinone", Joi.Coinone.Limit.QUERY_TIMEOUT, logging)
		{
			_api = new Api ();
			_market = new Market (name, TimeInterval.DAY_1);
			_market.SetAnalyzer (TimeInterval.MINUTE_1, TimeInterval.HOUR_2);
			_market.SetAnalyzer (TimeInterval.MINUTE_15, TimeInterval.DAY_1);
			_ratio = 4;
			_count = 0;

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
			System.Threading.Thread.Sleep (Limit.QUERY_TIMEOUT);
			GetTicker ();
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
			_count = (_count + 1) % _ratio;
			if (_count < 1)
				GetTicker ();
			else
				GetTrade ();
//			var json = _api.GetOrderbook ("btc");
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

		public override void Dump ()
		{
			Console.WriteLine ("{0} was dumpped", name);
		}

		private	void GetTrade (string period = "hour")
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

		private	void GetTicker ()
		{
			try {
				var json = _api.GetTicker (_currency);
				_market.ticker.Update (
					float.Parse (json ["high"].ToString ()),
					0f,
					float.Parse (json ["low"].ToString ()),
					0f,
					float.Parse (json ["volume"].ToString ())
				);
			} catch (Exception e) {
				Console.Error.WriteLine (e.Message);
				Fire (TRIGGER_STOP);
			}
		}
	}
}


using System;
using Joi.Bitflyer;
using LitJson;
using Joi.Data;

namespace Joi.Brain
{
	public class CrawlerBitflyer : CrawlerLogic
	{
		private	Api _api;
		private	Market _market;
		private	string _productCode;
		private	float _ratio;
		private	float _count;

		public CrawlerBitflyer (Symbol symbol, bool logging = true) : base ("Bitflyer", Joi.Bitflyer.Limit.QUERY_TIMEOUT, logging)
		{
			_api = new Api ();
			_market = new Market (name);
			_ratio = 4;
			_count = 0;

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
				GetTrade (-1, _market.GetLastId ());
//			var json2 = _api.GetOrderBook ();
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

		private	void GetTrade(int count = -1, int after = -1)
		{
			var trades = _api.GetExecutionHistory (_productCode, count, -1, after);
			if (trades == null) {
				Fire (TRIGGER_STOP);
				return;
			}
			if (!trades.IsArray)
				return;

			var total = trades.Count;
			for (int i = 0; i < total; i++) {
				var trade = trades [i];
				_market.AddNewTrade (
					int.Parse (trade ["id"].ToString ()),
					double.Parse (trade ["price"].ToString ()),
					double.Parse (trade ["size"].ToString ()),
					Utility.Timestamp (DateTime.Parse (trade ["exec_date"].ToString ()))
				);
			}
		}

		private	void GetTicker()
		{
			try {
				var ticker = _api.GetTicker (_productCode);
				_market.ticker.Update (
					float.Parse (ticker ["best_bid"].ToString ()),
					float.Parse (ticker ["best_bid_size"].ToString ()),
					float.Parse (ticker ["best_ask"].ToString ()),
					float.Parse (ticker ["best_ask_size"].ToString ()),
					float.Parse (ticker ["volume"].ToString ())
				);
			} catch (Exception e) {
				Console.Error.WriteLine (e.Message);
				Fire (TRIGGER_STOP);
			}
		}
	}
}


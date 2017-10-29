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

		public CrawlerBitflyer (Symbol symbol, bool logging = true) : base ("Bitflyer", Joi.Bitflyer.Limit.QUERY_TIMEOUT, logging)
		{
			_api = new Api ();
			_market = new Market (name);

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
			GetTrade ();
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
			GetTrade (_market.GetLastId ());
//			var json2 = _api.GetOrderBook ();
//			var json3 = _api.GetTicker ();
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

		private	void GetTrade(int id = -1)
		{
			var trades = _api.GetExecutionHistory (_productCode, -1, -1, id);
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
					int.Parse (trade ["id"].ToString ()),
					double.Parse (trade ["price"].ToString ()),
					double.Parse (trade ["size"].ToString ()),
					Utility.Timestamp (DateTime.Parse (trade ["exec_date"].ToString ()))
				);
			}
			_market.EndUpdate ();
		}
	}
}


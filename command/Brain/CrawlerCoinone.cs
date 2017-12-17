using System;
using System.Collections.Generic;
using Joi.Coinone;
using Joi.Data;
using LitJson;

namespace Joi.Brain
{
	public class CrawlerCoinone : CrawlerLogic
	{
		private	Api _api;
		private	string _currency;
		private	double _buyFee;
		private double _sellFee;
		private bool _gatherTrade;
		private bool _gatherOrderBook;

		public Api api { get { return _api; } }
		public string currency { get { return _currency; } }
		public double buyingFee { get { return _buyFee; } }
		public double sellingFee { get { return _sellFee; } }

		public CrawlerCoinone (Symbol symbol, bool logging = true) : base (COINONE, Joi.Coinone.Limit.QUERY_TIMEOUT, logging)
		{
			_api = new Api ();
			_market = new Market (name, TimeInterval.DAY_3);
			_buyFee = 0f;
			_sellFee = 0f;
			_gatherTrade = true;
			_gatherOrderBook = false;

			_market.SetIndicator (TimeInterval.MINUTE_30, TimeInterval.DAY_3);
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

		public	void GatherTrade()
		{
			_gatherTrade = true;
			_gatherOrderBook = false;
		}

		public	void GatherOrderBook()
		{
			_gatherTrade = false;
			_gatherOrderBook = true;
		}

		protected override void OnEntryInit ()
		{
		}

		protected override void OnLoopInit ()
		{
			ConnectDatabase ();
			SetSpeed (3f);
			GetUserInfo ();
			Sleep ();
			GetBalance ();
			Sleep ();
			GetOrderBook ();
			Sleep ();
			GetTicker ();
			Sleep ();
			GetTrade ("day");
			SetSpeed (1f);
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
			if (_gatherTrade)
				GetTrade ();
			if (_gatherOrderBook)
				GetOrderBook ();
		}

		protected override void OnExitGather ()
		{
			DisconnectDatabase ();
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

		private	void GetUserInfo()
		{
			var json = _api.GetUserInfomation ();
			if (json == null)
				return;
			json = json ["userInfo"] ["feeRate"] [_currency];
			_buyFee = double.Parse (json ["maker"].ToString ());
			_sellFee = double.Parse (json ["taker"].ToString ());
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

				var timestamp = int.Parse (trade ["timestamp"].ToString ());
				var price = double.Parse (trade ["price"].ToString ());
				var amount = double.Parse (trade ["qty"].ToString ());

				_market.ReserveTrade (timestamp, price, amount, timestamp);
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
			
			var symbols = new Dictionary<Symbol, string> () { 
				{Symbol.BITCOIN, "btc"},
				{Symbol.ETHEREUM, "eth"},
				{Symbol.KR_WON, "krw"}
			};

			foreach (Symbol key in symbols.Keys) {
				var symbol = symbols [key];
				var obj = json [symbol];
				var balance = double.Parse (obj ["balance"].ToString ());
				var available = double.Parse (obj ["avail"].ToString ());
				_market.balance.SetValue (key, balance, available);
			}
		}

		protected override void GetOrderBook ()
		{
			var json = _api.GetOrderbook (_currency);
			if (json == null)
				return;

			var ob = _market.orderbook;
			var timestamp = int.Parse (json ["timestamp"].ToString ());
			var bids = json ["bid"];
			var asks = json ["ask"];

			ob.Clear (timestamp);
			for (int i = bids.Count - 1; i >= 0; i--) {
				var item = bids [i];
				ob.AddBid (double.Parse (item ["price"].ToString ()), double.Parse (item ["qty"].ToString ()));
			}
			for (int i = asks.Count - 1; i >= 0; i--) {
				var item = asks [i];
				ob.AddAsk (double.Parse (item ["price"].ToString ()), double.Parse (item ["qty"].ToString ()));
			}
		}
	}
}
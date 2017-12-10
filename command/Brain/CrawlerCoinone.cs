﻿using System;
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

		public CrawlerCoinone (Symbol symbol, bool logging = true) : base (COINONE, Joi.Coinone.Limit.QUERY_TIMEOUT, logging)
		{
			_api = new Api ();
			_market = new Market (name, TimeInterval.DAY_3);
			_market.SetIndicator (TimeInterval.MINUTE_1, TimeInterval.HOUR_5);
			_market.SetIndicator (TimeInterval.MINUTE_5, TimeInterval.HOUR_30);

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
			ConnectDatabase ();
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
				ExecuteQuery (string.Format("INSERT OR REPLACE INTO {0} VALUES({1}, {2}, {3}, {4});", name, timestamp, price, amount, timestamp));
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
				ConsoleIO.LogLine ("[balance] {0}:{1}({2})", symbol, balance, available);
			}
		}
	}
}
using System;
using System.Collections.Generic;
using Joi.Data.Chart;

namespace Joi.Data
{
	public class Market
	{
		private	string _name;
		private	TimeInterval _limit;
		private	List<Trade> _trades;
		private	List<Trade> _reserved;
		private	List<int> _ids;
		private	Ticker _ticker;
		private	Analyzer[] _analyzers;

		public	string name { get { return _name; } }

		public	Ticker ticker { get { return _ticker; } }

		public	Market (string name, TimeInterval limit = TimeInterval.DAY_3)
		{
			_name = name;
			_limit = limit;
			_trades = new List<Trade> ();
			_reserved = new List<Trade> ();
			_ids = new List<int> ();
			_ticker = new Ticker ();
			_analyzers = new Analyzer[] {
				new Analyzer (TimeInterval.MINUTE_1, TimeInterval.HOUR_3),
				new Analyzer (TimeInterval.MINUTE_3, TimeInterval.HOUR_9),
				new Analyzer (TimeInterval.MINUTE_5, TimeInterval.HOUR_15)
			};
		}

		public	void ReserveTrade (int id, double price, double amount, int timestamp)
		{
			if (_ids.Contains (id))
				return;

			var t = new Trade (id, price, amount, timestamp);
			_reserved.Add (t);
		}

		public	void FlushTrade()
		{
			SortTrade (_reserved);

			var count = _reserved.Count;
			for (int i = 0; i < count; i++) {
				var trade = _reserved [i];
				_trades.Add (trade);
				_ids.Add (trade.id);
				if (trade.timestamp < GetLastTimestamp ()) {
					SortTrade (_trades);
				}
			}
			_reserved.Clear ();
			RemoveTrade (GetLastTimestamp () - (int)_limit);
		}

		public	void AddTrade (int id, double price, double amount, int timestamp)
		{
			if (_ids.Contains (id))
				return;

			var t = new Trade (id, price, amount, timestamp);
			_trades.Add (t);
			_ids.Add (t.id);
			if (timestamp < GetLastTimestamp ()) {
				SortTrade (_trades);
			}
			RemoveTrade (timestamp - (int)_limit);
		}

		private	int RemoveTrade (int timestamp)
		{
			int removed = 0;
			int count = _trades.Count;
			for (int i = 0; i < count; i++) {
				var trade = _trades [i];
				if (trade.timestamp < timestamp) {
					_ids.Remove (trade.id);
					_trades.Remove (trade);
					i--;
					count--;
					removed++;
				} else {
					break;
				}
			}
			return removed;
		}

		private	void SortTrade (List<Trade> trades)
		{
			trades.Sort ((Trade x, Trade y) => {
				return x.timestamp - y.timestamp;
			});
		}

		public	void UpdateChart()
		{
			foreach (var analyzer in _analyzers) {
				analyzer.AssignCandle (_trades);
			}
		}

		public	int GetLastTimestamp ()
		{
			int count = _trades.Count;
			if (count > 0)
				return _trades [count - 1].timestamp;
			else
				return int.MinValue;
		}

		public	int GetLastId ()
		{
			int count = _trades.Count;
			if (count > 0)
				return _trades [count - 1].id;
			else
				return int.MinValue;
		}
	}
}


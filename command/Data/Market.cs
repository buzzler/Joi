using System;
using System.Collections.Generic;

namespace Joi.Data
{
	public class Market
	{
		private	string _name;
		private	List<Trade> _trades;
		private	List<int> _ids;
		private	Ticker _ticker;

		public	string name { get { return _name; } }

		public	Ticker ticker { get { return _ticker; } }

		public	Market (string name)
		{
			_name = name;
			_trades = new List<Trade> ();
			_ids = new List<int> ();
			_ticker = new Ticker ();
		}

		public	void AddNewTrade (int id, double price, double amount, int timestamp)
		{
			if (_ids.Contains (id))
				return;

			var t = new Trade (id, price, amount, timestamp);
			_trades.Add (t);
			if (timestamp < GetLastTimestamp ()) {
				AlignTrades ();
			}
			_ids.Add (t.id);
			Console.WriteLine ("{0} trade updated", name);
		}

		public	void AlignTrades ()
		{
			_trades.Sort ((Trade x, Trade y) => {
				return x.timestamp - y.timestamp;
			});
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


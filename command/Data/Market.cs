using System;
using System.Collections.Generic;

namespace Joi.Data
{
	public class Market
	{
		private	string _name;
		private	List<Trade> _trades;
		private	List<int> _ids;
		private List<Trade> _stashTrade;
		private	List<int> _stashId;

		public	string name { get { return _name; } }

		public	Market (string name) 
		{
			_name = name;
			_trades = new List<Trade> ();
			_ids = new List<int> ();
			_stashTrade = new List<Trade> ();
			_stashId = new List<int> ();
		}

		public	void BegineUpdate()
		{
			_stashTrade.Clear ();
		}

		public	void UpdateTrade(int id, double price, double amount, int timestamp)
		{
			if (_ids.Contains (id))
				return;
			_stashTrade.Add (new Trade (id, price, amount, timestamp));
			_stashId.Add (id);
		}

		public	void Align(List<Trade> list)
		{
			list.Sort ((Trade x, Trade y) => {
				return x.timestamp - y.timestamp;
			});
		}

		public	int EndUpdate()
		{
			int count = _stashId.Count;
			if (count > 0) {
				Align (_stashTrade);
				_trades.AddRange (_stashTrade);
				_ids.AddRange (_stashId);
				_stashTrade.Clear ();
				_stashId.Clear ();
			}
			return count;
		}

		public	int GetLastTimestamp()
		{
			int count = _trades.Count;
			if (count > 0)
				return _trades [count - 1].timestamp;
			else
				return int.MinValue;
		}

		public	int GetLastId()
		{
			int count = _trades.Count;
			if (count > 0)
				return _trades [count - 1].id;
			else
				return int.MinValue;
		}
	}
}


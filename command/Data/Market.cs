using System;
using System.Collections.Generic;

namespace Joi.Data
{
	public class Market
	{
		private	string _name;
		private	List<Trade> _trades;
		private	List<int> _ids;
		private	float _lastHighestBid;
		private	float _lastHighestSize;
		private	float _lastLowestAsk;
		private	float _lastLowestSize;
		private	float _volume;

		public	string name { get { return _name; } }

		public	List<Trade> trades { get { return _trades; } }

		public	float lastHighestBid { get { return _lastHighestBid; } }

		public	float lastHighestSize { get { return _lastHighestSize; } }

		public	float lastLowestAsk { get { return _lastLowestAsk; } }

		public	float lastLowestSize { get { return _lastLowestSize; } }

		public	float volume { get { return _volume; } }

		public	Market (string name)
		{
			_name = name;
			_trades = new List<Trade> ();
			_ids = new List<int> ();
			_lastHighestBid = 0f;
			_lastHighestSize = 0f;
			_lastLowestAsk = 0f;
			_lastLowestSize = 0f;
			_volume = 0f;
		}

		public	void AddNewTrade (int id, double price, double amount, int timestamp)
		{
			if (_ids.Contains (id))
				return;

			var last = GetLastTimestamp ();
			if (timestamp >= last) {
				_trades.Add (new Trade (id, price, amount, timestamp));
				_ids.Add (id);
			}
		}

		public	void AlignTrades (List<Trade> list)
		{
			list.Sort ((Trade x, Trade y) => {
				return x.timestamp - y.timestamp;
			});
		}

		public	void UpdateTicker (float highPrice = 0f, float highAmount = 0f, float lowPrice = 0f, float lowAmount = 0f, float volume = 0f)
		{
			_lastHighestBid = highPrice;
			_lastHighestSize = highAmount;
			_lastLowestAsk = lowPrice;
			_lastLowestSize = lowAmount;
			_volume = volume;
			Console.WriteLine ("{0} ticker updated", name);
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


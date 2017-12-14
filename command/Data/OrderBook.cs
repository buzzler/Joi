using System;
using System.Collections.Generic;

namespace Joi.Data
{
	public class OrderBook
	{
		private	int _timestamp;
		private	SortedList<double, double> _ask;
		private	SortedList<double, double> _bid;

		public	int timestamp { get { return _timestamp; } }

		public	OrderBook()
		{
			Clear ();
		}

		public	void Clear(int newtime = -1)
		{
			if (newtime > 0)
				_timestamp = newtime;
			else
				_timestamp = Utility.Timestamp (DateTime.Now);

			if (_ask == null)
				_ask = new SortedList<double, double> ();
			else
				_ask.Clear ();
			if (_bid == null)
				_bid = new SortedList<double, double> ();
			else
				_bid.Clear ();
		}

		public	void AddAsk(double price, double amount)
		{
			_ask.Add (price, amount);
		}

		public	void AddBid(double price, double amount)
		{
			_bid.Add (price, amount);
		}

		public	double GetLowestAsk()
		{
			return _ask.Keys [0];
		}

		public	double GetHighestBid()
		{
			return _bid.Keys [_bid.Count - 1];
		}
	}
}


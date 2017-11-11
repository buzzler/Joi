using System;

namespace Joi.Data.Chart
{
	public class Candlestick
	{
		private	double _high;
		private	double _low;
		private	double _open;
		private	double _close;
		private	int _openTime;
		private	int _closeTime;
		private	double _amount;
		private	int _count;

		public	double high { get { return _high; } }

		public	double low { get { return _low; } }

		public	double open { get { return _open; } }

		public	double close { get { return _close; } }

		public	bool increasing { get { return _close > _open; } }

		public	bool decreasing { get { return _open > _close; } }

		public	double amount { get { return _amount; } }

		public	double count { get { return _count; } }
		
		public	Candlestick ()
		{
			Reset ();
		}

		public	void Reset ()
		{
			_high = 0f;
			_low = 0f;
			_open = 0f;
			_close = 0f;
			_openTime = 0;
			_closeTime = 0;
			_amount = 0f;
			_count = 0;
		}

		public	void Assign (Trade trade)
		{
			var timestamp = trade.timestamp;
			var price = trade.price;

			if (_openTime == 0 || _openTime > timestamp) {
				_open = price;
				_openTime = timestamp;
			}
			if (_closeTime == 0 || _closeTime < timestamp) {
				_close = price;
				_closeTime = timestamp;
			}

			if (_high == 0 || _high < price)
				_high = price;
			if (_low == 0 || _low > price)
				_low = price;
			_amount += Math.Abs (trade.amount);
			_count++;
		}

		public	override string ToString()
		{
			return string.Format ("open:{0}, close:{1}, high:{2}, low: {3}, amount: {4}, inc:{5}, dec:{6}, total:{7}", _open, _close, _high, _low, _amount, increasing, decreasing, _count);
		}
	}
}


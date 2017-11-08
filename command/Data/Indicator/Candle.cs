using System;

namespace Joi.Data.Indicator
{
	public class Candle : IndicatorUnit
	{
		private	double _high;
		private	double _low;
		private	double _open;
		private	double _close;
		private	double _amount;

		public	double high { get { return _high; } }

		public	double low { get { return _low; } }

		public	double open { get { return _open; } }

		public	double close { get { return _close; } }

		public	double amount { get { return _amount; } }

		public	bool increasing { get { return _close > _open; } }

		public	bool decreasing { get { return _open > _close; } }

		public	Candle (int openTime, int closeTime) : base (openTime, closeTime)
		{
		}

		public	void Assign (Trade trade)
		{
			var time = trade.timestamp;
			if (time > closeTime) {
				if (next == null)
					next = new Candle (closeTime, closeTime + (closeTime - openTime));
				(next as Candle).Assign (trade);
			} else if (time <= openTime) {
				if (previous == null)
					previous = new Candle (openTime - (closeTime - openTime), openTime);
				(previous as Candle).Assign (trade);
			} else {
				var price = trade.absolute;
				if (_high == 0 || price > _high)
					_high = price;
				if (_low == 0 || price < _low)
					_low = price;
				if (_open == 0) {
					_open = price;
				}
				_close = price;
				_amount += trade.amount;
			}
		}

		public	void ResetValueBackward ()
		{
			_high = 0;
			_low = 0;
			_open = 0;
			_close = 0;
			_amount = 0;

			if (previous != null)
				(previous as Candle).ResetValueBackward ();
		}

		public	void ResetValueForward ()
		{
			_high = 0;
			_low = 0;
			_open = 0;
			_close = 0;
			_amount = 0;

			if (next != null)
				(next as Candle).ResetValueForward ();
		}
	}
}


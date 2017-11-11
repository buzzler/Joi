using System;
using System.Collections.Generic;

namespace Joi.Data.Chart
{
	public class MovingAverage
	{
		private	double _value;
		private	List<Candlestick> _candles;

		public	double value { get { return _value; } }

		public MovingAverage ()
		{
			_value = 0;
			_candles = new List<Candlestick> ();
		}

		public	void Begin ()
		{
			_value = 0;
			_candles.Clear ();
		}

		public	void Add (Candlestick candle)
		{
			_candles.Add(candle);
		}

		public	void End ()
		{
			int total = _candles.Count;
			double sum = 0;
			for (int i = total - 1; i >= 0; i--) {
				sum += _candles [i].close;
			}
			_value = sum / (double)total;
			_candles.Clear ();
		}
	}
}


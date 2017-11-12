using System;
using System.Collections.Generic;

namespace Joi.Data.Chart
{
	public class MA
	{
		private	double _value;
		private	List<Candle> _candles;

		public	double value { get { return _value; } }

		public MA ()
		{
			_value = 0;
			_candles = new List<Candle> ();
		}

		public	void Begin ()
		{
			_value = 0;
			_candles.Clear ();
		}

		public	void Add (Candle candle)
		{
			_candles.Add(candle);
		}

		public	void End ()
		{
			int validated = 0;
			int total = _candles.Count;
			double sum = 0;
			for (int i = total - 1; i >= 0; i--) {
				var candle = _candles [i];
				if (candle.valid) {
					sum += _candles [i].close;
					validated++;
				}
			}
			if (validated > 0)
				_value = sum / (double)validated;
			_candles.Clear ();
		}
	}
}


using System;
using System.Collections.Generic;

namespace Joi.Data.Chart
{
	public class EMA
	{
		protected	double _value;
		protected	List<Candle> _candles;

		public	double value { get { return _value; } }

		public EMA ()
		{
			_value = 0;
			_candles = new List<Candle> ();
		}

		public	virtual void Begin ()
		{
			_value = 0;
			_candles.Clear ();
		}

		public	virtual void Add (Candle candle)
		{
			_candles.Add(candle);
		}

		public	virtual void End ()
		{
			if (_candles.Count > 0) {
				_value = GetExponential (_candles.Count);
				_candles.Clear ();
			}
		}

		protected	double GetExponential(int scale)
		{
			var candle = _candles [scale - 1];
			if (scale == 1)
				return candle.close;

			var post = GetExponential (scale - 1);
			if (post == 0)
				return candle.close;
			
			var w = 2f / (double)(scale + 1);
			return candle.close * w + post * (1 - w);
		}
	}
}


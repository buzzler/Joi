using System;

namespace Joi.Data.Chart
{
	public class BollingerBand : EMA
	{
		private	double _deviation;
		private	double _highBand;
		private	double _lowBand;

		public	double deviation { get { return _deviation; } }

		public	double highband { get { return _highBand; } }

		public	double lowband { get { return _lowBand; } }

		public	BollingerBand () : base ()
		{
			_deviation = 0;
		}

		public override void Begin ()
		{
			base.Begin ();
			_deviation = 0;
		}

		public override void Add (Candle candle)
		{
			if (candle.valid)
				base.Add (candle);
		}

		public override void End ()
		{
			int scale = _candles.Count;
			if (scale > 1) {
				_value = GetExponential (scale);
				double sum = 0;
				for (int i = 0; i < scale; i++)
					sum += _candles [i].close;
				double average = sum / (double)scale;
				sum = 0;
				for (int i = 0; i < scale; i++)
					sum += Math.Pow (_candles [i].close - average, 2);
				_deviation = Math.Sqrt (sum / (double)(scale - 1));
				_candles.Clear ();
			} else if (scale > 0) {
				_value = _candles [0].close;
				_deviation = 0;
				_candles.Clear ();
			}
			double md = _deviation * 2;
			_highBand = _value + md;
			_lowBand = _value - md;
		}
	}
}


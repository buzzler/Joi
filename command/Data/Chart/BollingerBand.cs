using System;

namespace Joi.Data.Chart
{
	public class BollingerBand : EMA
	{
		private	double _deviation;
		private	double _highBand;
		private	double _lowBand;

		private	double _deviationRatio;
		private	bool _crossingAbove;
		private	bool _crossingBelow;

		public	double deviation { get { return _deviation; } }

		public	double highband { get { return _highBand; } }

		public	double lowband { get { return _lowBand; } }

		public	double deviationRatio { get { return _deviationRatio; } }

		public	bool crossingAbove { get { return _crossingAbove; } }

		public	bool crossingBelow { get { return _crossingBelow; } }

		public	BollingerBand () : base ()
		{
			_deviation = 0;
			_highBand = 0;
			_lowBand = 0;
		}

		public override void Begin ()
		{
			base.Begin ();
			_deviation = 0;
			_highBand = 0;
			_lowBand = 0;
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

		public	void SetDelta(BollingerBand beforeBB, Candle beforeCandle, Candle current)
		{
			if (this.deviation != 0) {
				var deviation = current.close - value;
				_deviationRatio = deviation / (this.deviation * 2f);
			} else
				_deviationRatio = 0;
			_crossingAbove = (beforeBB.value > beforeCandle.close) && (value <= current.close);
			_crossingBelow = (beforeBB.value < beforeCandle.close) && (value >= current.close);
		}
	}
}


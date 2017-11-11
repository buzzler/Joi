using System;

namespace Joi.Data.Indicator
{
	public class MovingAverage : IndicatorUnit
	{
		private	int _scale;
		private	double _value;
		private	Candle[] _candles;
		private	int _index;

		public	int scale { get { return _scale; } }

		public	double value { get { return _value; } }

		public MovingAverage (int scale)
		{
			_scale = scale;
			_value = 0;
			_candles = new Candle[scale];
			_index = 0;
		}

		public	void Begin ()
		{
			_index = 0;
		}

		public	bool AddCandle (Candle candle)
		{
			if (_index >= _scale)
				return false;
			
			_candles [_index] = candle;
			_index++;
			return true;
		}

		public	void End ()
		{
			if (_index <= 0)
				return;

			double sum = 0;
			for (int i = _index - 1; i >= 0; i--) {
				sum += _candles [i].close;
				_candles [i] = null;
			}
			_value = sum / (double)_index;
			_index = 0;
		}

		public	void ResetValue (int scale = -1)
		{
			if (scale > 0) {
				_scale = scale;
				_candles = new Candle[scale];
			}
			_value = 0;
			_index = 0;
		}
	}
}


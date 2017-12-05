using System;

namespace Joi.Data.Chart
{
	public class MACDOscillator
	{
		private	double _value;
		private	bool _valid;
		private	bool _crossingAbove;
		private	bool _crossingBelow;
		private	bool _increasing;
		private	bool _decreasing;
		private	double _delta;

		public	double value { get { return _value; } }

		public	bool valid { get { return _valid; } }

		public	bool crossingAbove { get { return _crossingAbove; } }

		public	bool crossingBelow { get { return _crossingBelow; } }

		public	bool increasing { get { return _increasing; } }

		public	bool decreasing { get { return _decreasing; } }

		public	double delta { get { return _delta; } }

		public	MACDOscillator()
		{
			_value = 0;
			_valid = false;
			_crossingAbove = false;
			_crossingBelow = false;
			_increasing = false;
			_decreasing = false;
		}

		public	void SetValue(MACD macd, Signal signal)
		{
			_value = macd.value - signal.value;
			_valid = (macd.value != 0) && (signal.value != 0);
		}

		public	void SetDelta(double max, MACDOscillator before)
		{
			_crossingAbove = (before.value < 0) && (value >= 0);
			_crossingBelow = (before.value > 0) && (value <= 0);
			_increasing = before.value < value;
			_decreasing = before.value >= value;
			_delta = (value - before.value) / max;
		}
	}
}


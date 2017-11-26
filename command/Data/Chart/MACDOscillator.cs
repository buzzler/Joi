using System;

namespace Joi.Data.Chart
{
	public class MACDOscillator
	{
		private	double _value;
		private	bool _valid;

		public	double value { get { return _value; } }

		public	bool valid { get { return _valid; } }

		public	MACDOscillator()
		{
			_value = 0;
			_valid = false;
		}

		public	void Calculate(MACD macd, Signal signal)
		{
			_value = macd.value - signal.value;
			_valid = (macd.value != 0) && (signal.value != 0);
		}
	}
}


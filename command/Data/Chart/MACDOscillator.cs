using System;

namespace Joi.Data.Chart
{
	public class MACDOscillator
	{
		private	double _value;

		public	double value { get { return _value; } }

		public	void Calculate(MACD macd, Signal signal)
		{
			_value = macd.value - signal.value;
		}
	}
}


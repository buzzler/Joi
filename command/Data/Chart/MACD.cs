using System;

namespace Joi.Data.Chart
{
	public class MACD
	{
		private	double _value;

		public	double value { get { return _value; } }

		public	MACD ()
		{
			_value = 0f;
		}

		public	void Calculate(EMA ema12, EMA ema26)
		{
			_value = ema12.value - ema26.value;
		}
	}
}


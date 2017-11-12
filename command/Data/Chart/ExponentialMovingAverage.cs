using System;
using System.Collections.Generic;

namespace Joi.Data.Chart
{
	public class ExponentialMovingAverage
	{
		private	double _value;

		public	double value { get { return _value; } }

		public ExponentialMovingAverage ()
		{
			Reset ();
		}

		public	void Reset ()
		{
			_value = 0;
		}

		public	void Calculate (Candlestick today, MovingAverage yesterday, int scale)
		{
			Calculate (today.close, yesterday.value, GetFactor (scale));
		}

		public	void Calculate (Candlestick today, ExponentialMovingAverage yesterday, int scale)
		{
			Calculate (today.close, yesterday.value, GetFactor (scale));
		}

		public	void Calculate (double close, double yesterday, double factor)
		{
			_value = yesterday + (factor * (close - yesterday));
		}

		private	float GetFactor (int scale)
		{
			return 2f / (float)(scale + 1);
		}
	}
}


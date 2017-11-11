using System;
using System.Collections.Generic;
using Joi.Data.Indicator;

namespace Joi.Data.Indicator
{
	public class Analyzer
	{
		private	TimeInterval _interval;
		private	Candle _lastest;

		public Analyzer (TimeInterval interval)
		{
			_interval = interval;
			_lastest = new Candle (_interval);
		}

		public	void Add(Trade trade)
		{
			_lastest.ResetTimeBackward (trade.timestamp, _interval);
			_lastest.AddTrade (trade);
			_lastest.AlignTrade ();
			_lastest.Calculate ();
		}
	}
}


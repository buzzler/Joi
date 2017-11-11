using System;

namespace Joi.Data.Chart
{
	public class Candlestick
	{
		private	double _high;
		private	double _low;
		private	double _open;
		private	double _close;
		private	double _amount;

		public	double high { get { return _high; } }

		public	double low { get { return _low; } }

		public	double open { get { return _open; } }

		public	double close { get { return _close; } }

		public	double amount { get { return _amount; } }

		public	bool increasing { get { return _close > _open; } }

		public	bool decreasing { get { return _open > _close; } }


	}
}


using System;
using System.Collections.Generic;

namespace Joi.Data.Chart
{
	public class Signal
	{
		private	double _value;
		private	List<MACD> _macds;

		public	double value { get { return _value; } }

		public	Signal ()
		{
			_value = 0f;
			_macds = new List<MACD> ();
		}

		public	void Begin()
		{
			_value = 0f;
			_macds.Clear ();
		}

		public	void Add(MACD macd)
		{
			_macds.Add (macd);
		}

		public	void End()
		{
			int validated = 0;
			int total = _macds.Count;
			double sum = 0;
			for (int i = total - 1; i >= 0; i--) {
				var macd = _macds [i];
				if (macd.value != 0) {
					sum += macd.value;
					validated++;
				}
			}
			if (validated > 0)
				_value = sum / (double)validated;
			_macds.Clear();
		}
	}
}


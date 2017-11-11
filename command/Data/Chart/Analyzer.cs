using System;
using System.Collections.Generic;
using System.Text;

namespace Joi.Data.Chart
{
	public class Analyzer
	{
		private	TimeInterval _unit;
		private	TimeInterval _limit;
		private	List<Candlestick> _candlesticks;
		private	List<MovingAverage> _movingAverrage15;
		private	List<MovingAverage> _movingAverrage50;
		private	int _count;

		public	Analyzer (TimeInterval unit, TimeInterval limit)
		{
			Resize (unit, limit);
		}

		public	void Resize(TimeInterval unit, TimeInterval limit)
		{
			if (_unit != unit || _limit != limit) {
				_unit = unit;
				_limit = limit;
				_count = (int)((float)limit / (float)unit);
				_candlesticks = new List<Candlestick> (_count);
				_movingAverrage15 = new List<MovingAverage> (_count);
				_movingAverrage50 = new List<MovingAverage> (_count);
				for (int i = 0; i < _count; i++) {
					_candlesticks.Add (new Candlestick ());
					_movingAverrage15.Add (new MovingAverage ());
					_movingAverrage50.Add (new MovingAverage ());
				}
			}
		}

		public	void AssignCandle (List<Trade> trades)
		{
			for (int i = 0; i < _count; i++)
				_candlesticks [i].Reset ();

			var totalTrade = trades.Count;
			var lastTrade = trades [totalTrade - 1];
			var startTime = lastTrade.timestamp - (int)_limit;
			for (int i = 0; i < totalTrade; i++) {
				var trade = trades [i];
				var timestamp = trade.timestamp;
				if (timestamp < startTime)
					continue;

				var index = (int)((float)(timestamp - startTime) / (float)_unit);
				if (index >= _count)
					index = _count - 1;
				_candlesticks [index].Assign (trade);
			}
		}

		private	void AssignMovingAverage()
		{
			
		}

		public	override string ToString ()
		{
			var sb = new StringBuilder ();
			sb.AppendFormat ("[analyzer] unit:{0}, limit:{1}", (int)_unit, (int)_limit);
			sb.AppendLine ();
			for (int i = 0; i < _count; i++)
				sb.AppendLine (_candlesticks [i].ToString ());

			return sb.ToString ();
		}
	}
}


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
		private	Dictionary<int, List<MovingAverage>> _movingAverrages;
		private	Dictionary<int, List<ExponentialMovingAverage>> _exponentialMovingAverages;
		private	int _count;

		public	Analyzer (TimeInterval unit, TimeInterval limit)
		{
			Resize (unit, limit);
		}

		public	void Resize (TimeInterval unit, TimeInterval limit)
		{
			if (_unit != unit || _limit != limit) {
				_unit = unit;
				_limit = limit;
				_count = (int)((float)limit / (float)unit);
				_candlesticks = new List<Candlestick> (_count);
				_movingAverrages = new Dictionary<int, List<MovingAverage>> () {
					{ 15, new List<MovingAverage> (_count) },
					{ 50, new List<MovingAverage> (_count) }
				};
				_exponentialMovingAverages = new Dictionary<int, List<ExponentialMovingAverage>> () {
					{ 12, new List<ExponentialMovingAverage> (_count) },
					{ 26, new List<ExponentialMovingAverage> (_count) }
				};

				for (int i = 0; i < _count; i++)
					_candlesticks.Add (new Candlestick ());
				var itr = _movingAverrages.GetEnumerator ();
				while (itr.MoveNext ()) {
					var list = itr.Current.Value;
					for (int i = 0; i < _count; i++)
						list.Add (new MovingAverage ());
				}
				var itr2 = _exponentialMovingAverages.GetEnumerator ();
				while (itr2.MoveNext ()) {
					var list = itr2.Current.Value;
					for (int i = 0; i < _count; i++)
						list.Add (new ExponentialMovingAverage ());
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

			AssignMovingAverage ();
			AssignExponentialMovingAverage ();
		}

		private	void AssignMovingAverage ()
		{
			var itr = _movingAverrages.GetEnumerator ();
			while (itr.MoveNext ()) {
				var scale = itr.Current.Key;
				var list = itr.Current.Value;
				for (int i = _count - 1; i >= 0; i--) {
					var ma = list [i];
					ma.Begin ();
					for (int k = 0; k < scale; k++) {
						int index = i - k;
						if (index >= 0)
							ma.Add (_candlesticks [index]);
					}
					ma.End ();
				}
			}
		}

		private	void AssignExponentialMovingAverage ()
		{
			var itr = _exponentialMovingAverages.GetEnumerator ();
			while (itr.MoveNext ()) {
				var scale = itr.Current.Key;
				var list = itr.Current.Value;
				for (int i = 0; i < _count; i++) {
					var candle = _candlesticks [i];
					var yesterday = list [i-1];
					var ema = list [i];
					ema.Reset ();
					ema.Calculate (candle, yesterday, scale);
				}
			}
		}

		//		public	override string ToString ()
		//		{
		//			var sb = new StringBuilder ();
		//			sb.AppendFormat ("[analyzer] unit:{0}, limit:{1}", (int)_unit, (int)_limit);
		//			sb.AppendLine ();
		//			for (int i = 0; i < _count; i++)
		//				sb.AppendLine (_candlesticks [i].ToString ());
		//
		//			return sb.ToString ();
		//		}
	}
}


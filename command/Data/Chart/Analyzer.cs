using System;
using System.Collections.Generic;
using System.Text;

namespace Joi.Data.Chart
{
	public class Analyzer
	{
		private	TimeInterval _unit;
		private	TimeInterval _limit;
		private	List<Candle> _candles;
		private	Dictionary<int, List<MA>> _mas;
		private	Dictionary<int, List<EMA>> _emas;
		private	List<MACD> _macds;
		private	List<Signal> _signals;
		private	List<MACDOscillator> _oscillators;

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
				_candles = new List<Candle> (_count);
				_mas = new Dictionary<int, List<MA>> () {
					{ 15, new List<MA> (_count) },
					{ 50, new List<MA> (_count) }
				};
				_emas = new Dictionary<int, List<EMA>> () {
					{ 12, new List<EMA> (_count) },
					{ 26, new List<EMA> (_count) }
				};
				_macds = new List<MACD> (_count);
				_signals = new List<Signal> (_count);
				_oscillators = new List<MACDOscillator> (_count);

				for (int i = 0; i < _count; i++) {
					_candles.Add (new Candle ());
					_macds.Add (new MACD ());
					_signals.Add (new Signal ());
					_oscillators.Add (new MACDOscillator ());
				}
				var itr = _mas.GetEnumerator ();
				while (itr.MoveNext ()) {
					var list = itr.Current.Value;
					for (int i = 0; i < _count; i++)
						list.Add (new MA ());
				}
				var itr2 = _emas.GetEnumerator ();
				while (itr2.MoveNext ()) {
					var list = itr2.Current.Value;
					for (int i = 0; i < _count; i++)
						list.Add (new EMA ());
				}
			}
		}

		public	void AssignCandle (List<Trade> trades)
		{
			for (int i = 0; i < _count; i++)
				_candles [i].Reset ();

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
				_candles [index].Assign (trade);
			}

			AssignMA ();
			AssignEMA ();
			AssignMACD ();
			AssignSignal ();
			AssignMACDOscillator ();
		}

		private	void AssignMA ()
		{
			var itr = _mas.GetEnumerator ();
			while (itr.MoveNext ()) {
				var scale = itr.Current.Key;
				var list = itr.Current.Value;
				for (int i = _count - 1; i >= 0; i--) {
					var ma = list [i];
					ma.Begin ();
					for (int k = scale - 1; k >= 0; k--) {
						int index = i - k;
						if (index >= 0)
							ma.Add (_candles [index]);
					}
					ma.End ();
				}
			}
		}

		private	void AssignEMA ()
		{
			var itr = _emas.GetEnumerator ();
			while (itr.MoveNext ()) {
				var scale = itr.Current.Key;
				var list = itr.Current.Value;
				for (int i = _count - 1; i >= 0; i--) {
					var ema = list [i];
					ema.Begin ();
					for (int k = scale - 1; k >= 0; k--) {
						int index = i - k;
						if (index >= 0)
							ema.Add (_candles [index]);
					}
					ema.End ();
				}
			}
		}

		private	void AssignMACD ()
		{
			var ema12 = _emas [12];
			var ema26 = _emas [26];
			for (int i = 0; i < _count; i++)
				_macds [i].Calculate (ema12 [i], ema26 [i]);
		}

		private	void AssignSignal ()
		{
			for (int i = _count - 1; i >= 0; i--) {
				var signal = _signals [i];
				signal.Begin ();
				for (int k = 8; k >= 0; k--) {
					int index = i - k;
					if (index >= 0)
						signal.Add (_macds [index]);
				}
				signal.End ();
			}
		}

		private	void AssignMACDOscillator ()
		{
			for (int i = _count - 1; i >= 0; i--)
				_oscillators [i].Calculate (_macds [i], _signals [i]);
		}
	}
}


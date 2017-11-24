using System;
using System.Collections.Generic;
using System.Text;
using Mono.Data.Sqlite;

namespace Joi.Data.Chart
{
	public class Indicator
	{
		private	string _name;
		private	TimeInterval _unit;
		private	TimeInterval _limit;
		private	int _count;
		private	List<Candle> _candles;
		private	Dictionary<int, List<MA>> _mas;
		private	Dictionary<int, List<EMA>> _emas;
		private	List<MACD> _macds;
		private	List<Signal> _signals;
		private	List<MACDOscillator> _oscillators;
		private	List<BollingerBand> _bollingerbands;

		public	string name { get { return _name; } }

		public	Indicator (string name, TimeInterval unit, TimeInterval limit)
		{
			_name = name;
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
				_bollingerbands = new List<BollingerBand> (_count);

				for (int i = 0; i < _count; i++) {
					_candles.Add (new Candle ());
					_macds.Add (new MACD ());
					_signals.Add (new Signal ());
					_oscillators.Add (new MACDOscillator ());
					_bollingerbands.Add (new BollingerBand ());
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

		public	void AssignTrades(List<Trade> trades)
		{
			AssignCandle (trades);
			AssignMA ();
			AssignEMA ();
			AssignMACD ();
			AssignSignal ();
			AssignMACDOscillator ();
			AssignBollingerBand ();
		}

		private	void AssignCandle (List<Trade> trades)
		{
			// resetting
			for (int i = 0; i < _count; i++)
				_candles [i].Reset ();

			// assigning
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

			// check empty candles (no trades)
			for (int i = 1; i < _count - 1; i++) {
				var current = _candles [i];
				if (current.valid)
					continue;

				var i_before = i - 1;
				var i_after = i + 1;
				while (i_before >= 0) {
					if (_candles [i_before].valid)
						break;
					i_before--;
				}
				if (i_before < 0)
					continue;
				var before = _candles [i_before];
				while (i_after < _count) {
					if (_candles [i_after].valid)
						break;
					i_after++;
				}
				if (i_after >= _count)
					continue;
				var after = _candles [i_after];
				current.Average (before, after);
			}
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

		private	void AssignBollingerBand ()
		{
			var scale = 20;
			for (int i = _count - 1; i >= 0; i--) {
				var bb = _bollingerbands [i];
				bb.Begin ();
				for (int k = scale - 1; k >= 0; k--) {
					int index = i - k;
					if (index >= 0)
						bb.Add (_candles [index]);
				}
				bb.End ();
			}
		}

		public	void Dump(SqliteCommand command)
		{
			var tablename = string.Format ("{0}_indicator_{1}", _name, (int)_unit);
			var sb = new StringBuilder ();
			sb.AppendFormat ("CREATE TABLE {0} (", tablename);
			sb.Append ("open REAL DEFAULT 0,");
			sb.Append ("close REAL DEFAULT 0,");
			sb.Append ("high REAL DEFAULT 0,");
			sb.Append ("low REAL DEFAULT 0,");
			sb.Append ("amount REAL DEFAULT 0,");
			sb.Append ("ema12 REAL DEFAULT 0,");
			sb.Append ("ema26 REAL DEFAULT 0,");
			sb.Append ("macd REAL DEFAULT 0,");
			sb.Append ("signal REAL DEFAULT 0,");
			sb.Append ("oscillator REAL DEFAULT 0,");
			sb.Append ("bollinger_high REAL DEFAULT 0,");
			sb.Append ("bollinger_low REAL DEFAULT 0);");
			command.CommandText = sb.ToString ();
			command.ExecuteNonQuery ();

			for (int i = 0; i < _count; i++) {
				var candle = _candles [i];
				sb.Clear ();
				sb.AppendFormat ("INSERT INTO {0} VALUES(", tablename);
				sb.AppendFormat ("{0},", candle.open);
				sb.AppendFormat ("{0},", candle.close);
				sb.AppendFormat ("{0},", candle.high);
				sb.AppendFormat ("{0},", candle.low);
				sb.AppendFormat ("{0},", candle.amount);
				sb.AppendFormat ("{0},", _emas[12][i].value);
				sb.AppendFormat ("{0},", _emas[26][i].value);
				sb.AppendFormat ("{0},", _macds[i].value);
				sb.AppendFormat ("{0},", _signals[i].value);
				sb.AppendFormat ("{0},", _oscillators[i].value);
				sb.AppendFormat ("{0},", _bollingerbands[i].highband);
				sb.AppendFormat ("{0});", _bollingerbands[i].lowband);
				command.CommandText = sb.ToString ();
				command.ExecuteNonQuery ();
			}
		}
	}
}


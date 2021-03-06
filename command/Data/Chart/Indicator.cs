﻿using System;
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
		private	int _lastest;
		private	List<Candle> _candles;
		private	Dictionary<int, List<MA>> _mas;
		private	Dictionary<int, List<EMA>> _emas;
		private	List<MACD> _macds;
		private	List<Signal> _signals;
		private	List<MACDOscillator> _oscillators;
		private	List<BollingerBand> _bollingerbands;
		private	double _avgAmount;
		private	double _curAmountRatio;

		public	int count { get { return _count; } }

		public	List<Candle> candles { get { return _candles; } }

		public	List<MACDOscillator> oscillators { get { return _oscillators; } }

		public	List<BollingerBand> bollingerbands { get { return _bollingerbands; } }

		public	string name { get { return _name; } }

		public	double averageAmount { get { return _avgAmount; } }

		public	double currentAmountRatio { get { return _curAmountRatio; } }

		public	Candle lastCandle { get { return _candles [_lastest]; } }

		public	MACDOscillator lastOscillator { get { return _oscillators [_lastest]; } }

		public	BollingerBand lastBollingerBand { get { return _bollingerbands [_lastest]; } }

		public	Indicator (string name, TimeInterval unit, TimeInterval limit)
		{
			_name = name;
			_avgAmount = 0;
			Resize (unit, limit);
		}

		public	void Resize (TimeInterval unit, TimeInterval limit)
		{
			if (_unit != unit || _limit != limit) {
				_unit = unit;
				_limit = limit;
				_count = (int)((float)limit / (float)unit);
				_lastest = _count - 1;
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

		public	void AssignTrades (List<Trade> trades)
		{
			AssignCandle (trades);
			AssignMA ();
			AssignEMA ();
			AssignMACD ();
			AssignSignal ();
			AssignMACDOscillator ();
			AssignBollingerBand ();

			CalculateVolume ();
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
					index = _lastest;
				_candles [index].Assign (trade);
			}

			// check empty candles (no trades)
			for (int i = 1; i < _lastest; i++) {
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
				current.Fillout (before, after);
			}
		}

		private	void AssignMA ()
		{
			var itr = _mas.GetEnumerator ();
			while (itr.MoveNext ()) {
				var scale = itr.Current.Key;
				var list = itr.Current.Value;
				for (int i = _lastest; i >= 0; i--) {
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
				for (int i = _lastest; i >= 0; i--) {
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
			for (int i = _lastest; i >= 0; i--) {
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
			double max = 0;
			for (int i = _lastest; i >= 0; i--) {
				var current = _oscillators [i];
				current.SetValue (_macds [i], _signals [i]);
				if (current.valid && max < current.value)
					max = Math.Abs(current.value);
			}

			for (int i = _lastest; i > 0; i--) {
				var current = _oscillators [i];
				var before = _oscillators [i - 1];
				current.SetDelta (max, before);
			}
		}

		private	void AssignBollingerBand ()
		{
			var scale = 20;
			for (int i = _lastest; i >= 0; i--) {
				var bb = _bollingerbands [i];
				bb.Begin ();
				for (int k = scale - 1; k >= 0; k--) {
					int index = i - k;
					if (index >= 0)
						bb.Add (_candles [index]);
				}
				bb.End ();
			}

			for (int i = _lastest; i > 0; i--)
				_bollingerbands [i].SetDelta (_bollingerbands [i - 1], _candles [i - 1], _candles [i]);
//
//			var beforeBB = _bollingerbands [_lastest - 1];
//			var lastBB = _bollingerbands [_lastest];
//			var beforeCandle = _candles [_lastest - 1];
//			var lastCandle = _candles [_lastest];
//
//			var deviation = lastCandle.close - lastBB.value;
//			_curDeviationRatio = deviation / (lastBB.deviation * 2f);
//			_crossingAboveBB = (beforeBB.value > beforeCandle.close) && (lastBB.value <= lastCandle.close);
//			_crossingBelowBB = (beforeBB.value < beforeCandle.close) && (lastBB.value >= lastCandle.close);
		}

		private	void CalculateVolume ()
		{
			double sum = 0;
			double count = 0;
			for (var i = 0; i < _count; i++) {
				var candle = _candles [i];
				if (!candle.valid)
					continue;
				sum += Math.Abs(candle.amount);
				count++;
			}
			_avgAmount = sum / count;
			_curAmountRatio = _candles [_lastest].amount / _avgAmount;
		}

		public	void Dump (SqliteCommand command)
		{
			var tablename = string.Format ("{0}_indicator_{1}", _name, (int)_unit);
			var sb = new StringBuilder ();
			sb.AppendFormat ("CREATE TABLE {0} (", tablename);
			sb.Append ("date TEXT,");
			sb.Append ("timestamp INTEGER DEFAULT 0,");
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

			var timestamp = _candles [_lastest].closeTime - (int)_limit;
			var startTime = Utility.DateTime (timestamp);
			for (int i = 0; i < _count; i++) {
				var candle = _candles [i];
				sb.Clear ();
				sb.AppendFormat ("INSERT INTO {0} VALUES(", tablename);
				sb.AppendFormat ("'{0}',", startTime.ToString ("T"));
				sb.AppendFormat ("{0},", timestamp);
				sb.AppendFormat ("{0},", candle.open);
				sb.AppendFormat ("{0},", candle.close);
				sb.AppendFormat ("{0},", candle.high);
				sb.AppendFormat ("{0},", candle.low);
				sb.AppendFormat ("{0},", candle.amount);
				sb.AppendFormat ("{0},", _emas [12] [i].value);
				sb.AppendFormat ("{0},", _emas [26] [i].value);
				sb.AppendFormat ("{0},", _macds [i].value);
				sb.AppendFormat ("{0},", _signals [i].value);
				sb.AppendFormat ("{0},", _oscillators [i].value);
				sb.AppendFormat ("{0},", _bollingerbands [i].highband);
				sb.AppendFormat ("{0});", _bollingerbands [i].lowband);
				command.CommandText = sb.ToString ();
				command.ExecuteNonQuery ();

				timestamp += (int)_unit;
				startTime = Utility.DateTime (timestamp);
			}
		}
	}
}


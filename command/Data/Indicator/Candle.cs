using System;
using System.Collections.Generic;

namespace Joi.Data.Indicator
{
	public class Candle : IndicatorUnit
	{
		private	double _high;
		private	double _low;
		private	double _open;
		private	double _close;
		private	double _amount;
		private	TimeInterval _interval;
		private	List<Trade> _trades;

		public	double high { get { return _high; } }

		public	double low { get { return _low; } }

		public	double open { get { return _open; } }

		public	double close { get { return _close; } }

		public	double amount { get { return _amount; } }

		public	bool increasing { get { return _close > _open; } }

		public	bool decreasing { get { return _open > _close; } }

		public	TimeInterval interval { get { return _interval; } }

		public	Candle (TimeInterval interval)
		{
			RemoveTradeAll ();
			ResetValue ();
			SetInterval (interval);
		}

		public	void SetInterval (TimeInterval interval)
		{
			_interval = interval;
		}

		public	void AddTrade (Trade trade)
		{
			var time = trade.timestamp;
			if (time > closeTime) {
				throw new Exception ("sort???");
				if (next == null) {
					next = new Candle (_interval);
					next.ResetTimeForward (closeTime, _interval);
				}
				(next as Candle).AddTrade (trade);
			} else if (time <= openTime) {
				if (previous == null) {
					previous = new Candle (_interval);
					previous.ResetTimeBackward (openTime, _interval);
				}
				(previous as Candle).AddTrade (trade);
			} else {
				_trades.Add (trade);
			}
		}

		public	void RemoveTradeAll ()
		{
			_trades = new List<Trade> ();
		}

		public	void Calculate ()
		{
			ResetValue ();
			int total = _trades.Count;
			int opened = 0;
			int closed = 0;

			for (int i = 0; i < total; i++) {
				var trade = _trades [i];
				var price = trade.price;
				var time = trade.timestamp;

				if (_high == 0 || price > _high)
					_high = price;
				if (_low == 0 || price < _low)
					_low = price;
				if (opened == 0 || time < opened) {
					opened = time;
					_open = price;
				}
				if (closed == 0 || time > closed) {
					closed = time;
					_close = price;
				}
				_amount += trade.amount;
			}
		}

		public	void CalculateBackward ()
		{
			Calculate ();
			if (previous != null)
				(previous as Candle).CalculateBackward ();
		}

		public	void CalculateForward ()
		{
			Calculate ();
			if (next != null)
				(next as Candle).CalculateForward ();
		}

		public	void AlignTrade ()
		{
			var total = _trades.Count;
			for (int i = 0; i < total; i++) {
				var trade = _trades [i];
				var time = trade.timestamp;

				if (time > closeTime) {
					if (next == null) {
						next = new Candle (_interval);
						next.ResetTimeForward (closeTime, _interval);
					}
					(next as Candle).AddTrade (trade);
				} else if (time <= openTime) {
					if (previous == null) {
						previous = new Candle (_interval);
						previous.ResetTimeBackward (openTime, _interval);
					}
					(previous as Candle).AddTrade (trade);
				} else
					continue;
				_trades.RemoveAt (i);
				total--;
				i--;
			}
		}

		public	void AlignTradeBackward ()
		{
			AlignTrade ();
			if (previous != null)
				(previous as Candle).AlignTradeBackward ();
		}

		public	void AlignTradeForward ()
		{
			AlignTrade ();
			if (next != null)
				(next as Candle).AlignTradeForward ();
		}

		public	void ResetValue ()
		{
			_high = 0;
			_low = 0;
			_open = 0;
			_close = 0;
			_amount = 0;
		}

		public	void ResetValueBackward ()
		{
			ResetValue ();
			if (previous != null)
				(previous as Candle).ResetValueBackward ();
		}

		public	void ResetValueForward ()
		{
			ResetValue ();
			if (next != null)
				(next as Candle).ResetValueForward ();
		}
	}
}


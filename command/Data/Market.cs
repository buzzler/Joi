using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using Joi.Data.Chart;
using System.Text;

namespace Joi.Data
{
	public class Market
	{
		private	string _name;
		private	TimeInterval _limit;
		private	List<Trade> _trades;
		private	List<Trade> _reserved;
		private	List<int> _ids;
		private	Ticker _ticker;
		private	Balance _balance;
		private	Dictionary <TimeInterval, Analyzer> _analyzers;

		public	string name { get { return _name; } }

		public	Ticker ticker { get { return _ticker; } }

		public	Balance balance { get { return _balance; } }

		public	Market (string name, TimeInterval limit = TimeInterval.DAY_3)
		{
			_name = name;
			_limit = limit;
			_trades = new List<Trade> ();
			_reserved = new List<Trade> ();
			_ids = new List<int> ();
			_ticker = new Ticker ();
			_balance = new Balance ();
			_analyzers = new Dictionary<TimeInterval, Analyzer> ();
		}

		public	void SetAnalyzer (TimeInterval interval, TimeInterval limit = TimeInterval.NONE)
		{
			if (_analyzers.ContainsKey (interval))
				return;

			if (limit == TimeInterval.NONE)
				limit = _limit;

			var analyzer = new Analyzer (_name, interval, limit);
			_analyzers.Add (interval, analyzer);
		}

		public	Analyzer GetAnalyzer (TimeInterval interval)
		{
			if (_analyzers.ContainsKey (interval))
				return _analyzers [interval];
			return null;
		}

		public	void ReserveTrade (int id, double price, double amount, int timestamp)
		{
			if (_ids.Contains (id))
				return;

			var t = new Trade (id, price, amount, timestamp);
			_reserved.Add (t);
		}

		public	void FlushTrade ()
		{
			SortTrade (_reserved);

			var count = _reserved.Count;
			for (int i = 0; i < count; i++) {
				var trade = _reserved [i];
				_trades.Add (trade);
				_ids.Add (trade.id);
				if (trade.timestamp < GetLastTimestamp ()) {
					SortTrade (_trades);
				}
			}
			_reserved.Clear ();
			RemoveTrade (GetLastTimestamp () - (int)_limit);
		}

		public	void AddTrade (int id, double price, double amount, int timestamp)
		{
			if (_ids.Contains (id))
				return;

			var t = new Trade (id, price, amount, timestamp);
			_trades.Add (t);
			_ids.Add (t.id);
			if (timestamp < GetLastTimestamp ()) {
				SortTrade (_trades);
			}
			RemoveTrade (timestamp - (int)_limit);
		}

		private	int RemoveTrade (int timestamp)
		{
			int removed = 0;
			int count = _trades.Count;
			for (int i = 0; i < count; i++) {
				var trade = _trades [i];
				if (trade.timestamp < timestamp) {
					_ids.Remove (trade.id);
					_trades.Remove (trade);
					i--;
					count--;
					removed++;
				} else {
					break;
				}
			}
			return removed;
		}

		private	void SortTrade (List<Trade> trades)
		{
			trades.Sort ((Trade x, Trade y) => {
				return x.timestamp - y.timestamp;
			});
		}

		public	void UpdateChart ()
		{
			foreach (var analyzer in _analyzers.Values)
				analyzer.AssignCandle (_trades);
		}

		public	int GetLastTimestamp ()
		{
			int count = _trades.Count;
			if (count > 0)
				return _trades [count - 1].timestamp;
			else
				return int.MinValue;
		}

		public	int GetLastId ()
		{
			int count = _trades.Count;
			if (count > 0)
				return _trades [count - 1].id;
			else
				return int.MinValue;
		}

		public	void Dump (SqliteCommand command)
		{
			var tablename = string.Format ("{0}_market", _name);
			var sb = new StringBuilder ();
			sb.AppendFormat ("CREATE TABLE {0} (", tablename);
			sb.Append ("id INTEGER,");
			sb.Append ("price REAL,");
			sb.Append ("amount REAL,");
			sb.Append ("timestamp INTEGER);");
			command.CommandText = sb.ToString ();
			command.ExecuteNonQuery ();
			var count = _trades.Count;
			for (int i = 0; i < count; i++) {
				var trade = _trades [i];
				sb.Clear ();
				sb.AppendFormat ("INSERT INTO {0} VALUES (", tablename);
				sb.AppendFormat ("{0},", trade.id);
				sb.AppendFormat ("{0},", trade.price);
				sb.AppendFormat ("{0},", trade.amount);
				sb.AppendFormat ("{0});", trade.timestamp);
				command.CommandText = sb.ToString ();
				command.ExecuteNonQuery ();
			}
			foreach (var analyzer in _analyzers.Values)
				analyzer.Dump (command);
		}

		public	string Status()
		{
			var sb = new StringBuilder ();
			sb.AppendFormat ("[{0}]", name);
			sb.AppendLine ();
			sb.AppendFormat ("-[balance]");
			sb.AppendLine ();
			sb.Append (_balance.Status ());
			sb.AppendFormat ("-[ticker]");
			sb.AppendLine ();
			sb.Append (_ticker.Status ());
			return sb.ToString ();
		}
	}
}


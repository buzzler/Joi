using System;
using Joi.FSM;
using Mono.Data.Sqlite;
using Joi.Data;

namespace Joi.Brain
{
	public abstract class CrawlerLogic : StateMachine
	{
		public	const string COINONE = "Coinone";
		public	const string BITFLYER = "Bitflyer";
		public	const string BITFINEX = "Bitfinex";
		protected const string STATE_INITIALIZING = "Initializing";
		protected const string STATE_GATHERING = "Gathering";
		protected const string STATE_STOPPED = "Stopped";
		protected const string TRIGGER_COMPLETE = "complete";
		protected const string TRIGGER_STOP = "stop";
		protected const string TRIGGER_START = "start";
		protected Market _market;
		protected string _dump;
		protected Action _onDump;
		protected Action _onTicker;
		protected Action _onBalance;
		private	object _lock;

		public	Market market { get { return _market; } }

		public CrawlerLogic (string name, int timeout, bool logging = true) : base(name, timeout, logging)
		{
			_lock = new object ();
			AnyState ()
				.ConnectTo (TRIGGER_STOP, STATE_STOPPED);
			SetFirstState (STATE_INITIALIZING)
				.SetupEntry (OnEntryInit)
				.SetupLoop (OnLoopInit)
				.SetupExit (OnExitInit)
				.ConnectTo (TRIGGER_COMPLETE, STATE_GATHERING);
			SetState (STATE_STOPPED)
				.SetupEntry (OnEntryStop)
				.SetupLoop (OnLoopStop)
				.SetupExit (OnExitStop)
				.ConnectTo (TRIGGER_START, STATE_INITIALIZING);
			SetState (STATE_GATHERING)
				.SetupEntry (OnEntryGather)
				.SetupLoop (OnLoopGather)
				.SetupExit (OnExitGather);
			Start ();
		}

		~CrawlerLogic()
		{
			End ();
		}

		#region 'Initialinzing' state

		protected abstract void OnEntryInit();

		protected abstract void OnLoopInit();

		protected abstract void OnExitInit();

		#endregion

		#region 'Gathering' state

		protected abstract void OnEntryGather();

		protected virtual void OnLoopGather()
		{
			lock (_lock) {
				if (!string.IsNullOrEmpty (_dump)) {
					Dump ();
					if (_onDump != null)
						_onDump ();
					_onDump = null;
					Sleep ();
				}
				if (_onTicker != null) {
					GetTicker ();
					_onTicker ();
					_onTicker = null;
					Sleep ();
				}
				if (_onBalance != null) {
					GetBalance ();
					_onBalance ();
					_onBalance = null;
					Sleep ();
				}
			}
		}

		protected abstract void OnExitGather();

		#endregion

		#region 'Stopped' state

		protected abstract void OnEntryStop();

		protected abstract void OnLoopStop();

		protected abstract void OnExitStop();

		#endregion

		#region 'Dump'

		public void DumpAsync(string filename, Action callback)
		{
			lock (_lock) {
				_dump = filename;
				_onDump = callback;
			}
		}

		public	void Dump()
		{
			var filename = _dump;
			_dump = null;

			try {
				if (_market != null && filename != null) {
					var connsb = new SqliteConnectionStringBuilder ();
					connsb.DataSource = filename;
					using (var conn = new SqliteConnection (connsb.ConnectionString)) {
						conn.Open ();
						var command = conn.CreateCommand ();
						_market.Dump (command);
						conn.Close ();
					}
				}
			} catch (Exception e) {
				ConsoleIO.Error (e.Message);
			}
		}

		#endregion

		#region 'Status'

		public	string Status()
		{
			return _market.Status ();
		}

		#endregion

		#region 'Async API'

		public	void GetTickerAsync(Action callback)
		{
			lock (_lock) {
				_onTicker = callback;
			}
		}

		protected virtual void GetTicker ()
		{
		}

		public	void GetBalanceAsync(Action callback)
		{
			lock (_lock) {
				_onBalance = callback;
			}
		}

		protected virtual void GetBalance()
		{
		}

		#endregion
	}
}


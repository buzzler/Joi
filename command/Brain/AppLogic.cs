using System;
using System.Threading;
using Joi.FSM;
using Joi.Data;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Joi.Brain
{
	public	class AppLogic : StateMachine
	{
		private	const string STATE_INITIALIZING	= "Initializing";
		private	const string STATE_STOPPED = "Stopped";
		private	const string STATE_RUNNING = "Running";
		private const string TRIGGER_COMPLETE	= "complete";
		private	const string TRIGGER_START = "start";
		private	const string TRIGGER_STOP = "stop";
		private	const string TRADE = "trade";

		private	Symbol _symbol = Symbol.BITCOIN;
		private	Dictionary<string, StateMachine> _stateMachines;
		private	Dictionary<string, Thread> _threads;

		public	AppLogic (bool logging = true) : base ("AppLogic", 1000, logging)
		{
			AnyState ()
				.ConnectTo (TRIGGER_STOP, STATE_STOPPED);
			SetFirstState (STATE_INITIALIZING)
				.SetupEntry (OnEntryInit)
				.SetupExit (OnExitInit)
				.SetupLoop (OnLoopInit)
				.ConnectTo (TRIGGER_COMPLETE, STATE_RUNNING);
			SetState (STATE_RUNNING)
				.SetupEntry (OnEntryRun)
				.SetupExit (OnExitRun)
				.SetupLoop (OnLoopRun);
			SetState (STATE_STOPPED)
				.SetupEntry (OnEntryStop)
				.SetupExit (OnExitStop)
				.SetupLoop (OnLoopStop);
			Start ();
		}

		~AppLogic ()
		{
			End ();
		}

		#region 'Initializing' state

		private	void OnEntryInit ()
		{
			_stateMachines = new Dictionary<string, StateMachine> () {
				{ TRADE, new TradeLogic (_symbol, logging) },
				{ CrawlerLogic.BITFINEX, new CrawlerBitfinex (_symbol, logging) },
				{ CrawlerLogic.BITFLYER, new CrawlerBitflyer (_symbol, logging) },
				{ CrawlerLogic.COINONE, new CrawlerCoinone (_symbol, logging) }
			};
			_threads = new Dictionary<string, Thread> () {
				{ TRADE, new Thread (_stateMachines [TRADE].Run) },
				{ CrawlerLogic.BITFINEX, new Thread (_stateMachines [CrawlerLogic.BITFINEX].Run) },
				{ CrawlerLogic.BITFLYER, new Thread (_stateMachines [CrawlerLogic.BITFLYER].Run) },
				{ CrawlerLogic.COINONE, new Thread (_stateMachines [CrawlerLogic.COINONE].Run) }
			};
			foreach (var thread in _threads.Values)
				thread.Start ();
		}

		private	void OnLoopInit ()
		{
			bool alive = true;
			foreach (var thread in _threads.Values)
				alive &= thread.IsAlive;
			if (alive)
				Fire (TRIGGER_COMPLETE);
		}

		private	void OnExitInit ()
		{
		}

		#endregion

		#region 'Stopped' state

		private	void OnEntryStop ()
		{
			foreach (var sm in _stateMachines.Values)
				sm.End ();
		}

		private	void OnLoopStop ()
		{
			bool alive = false;
			foreach (var thread in _threads.Values)
				alive |= thread.IsAlive;
			if (!alive)
				End ();
		}

		private	void OnExitStop ()
		{
			_stateMachines = null;
			_threads = null;
			System.GC.Collect ();
		}

		#endregion

		#region 'Running' state

		private	void OnEntryRun ()
		{
		}

		private	void OnLoopRun ()
		{
			PrintMainMenu ();
		}

		private	void OnExitRun ()
		{
		}

		#endregion

		#region commandline parser

		private	void PrintMainMenu ()
		{
			ConsoleIO.Clear ();
			ConsoleIO.WriteLine ("1. Backup to Database");
			ConsoleIO.WriteLine ("2. Status");
			ConsoleIO.WriteLine ("3. Log");
			ConsoleIO.WriteLine ("4. Exit");
			ConsoleIO.WriteLine ();
			ConsoleIO.Write ("> ");
			ConsoleIO.ShowCursor ();
			string input = ConsoleIO.ReadLine ();
			ConsoleIO.HideCursor ();

			switch (input) {
			case "1":
				OnSelectDumpToFiles ();
				break;
			case "2":
				OnSelectStatus ();
				break;
			case "3":
				OnSelectLog ();
				break;
			case "4":
				OnSelectExit ();
				break;
			}
		}

		private	bool _dumpped;

		private	void OnSelectDumpToFiles ()
		{
			var filename = string.Format ("{0}.db", DateTime.Now.ToString ("yyyy-MM-dd_hh.mm.ss"));
			var total = _stateMachines.Count;
			var current = 0;

			ConsoleIO.Clear ();
			if (File.Exists (filename)) {
				ConsoleIO.WriteLine ("delete a exist file: {0}", filename);
				File.Delete (filename);
			}
			ConsoleIO.WriteLine ("processing.. {0}", filename);
			foreach (var sm in _stateMachines.Values) {
				current++;
				if (sm is CrawlerLogic) {
					_dumpped = false;
					var crawler = sm as CrawlerLogic;
					ConsoleIO.WriteLine ("({1}/{2}) backup.. {0}", crawler.name, current, total);
					crawler.DumpAsync (filename, () => {
						_dumpped = true;
					});
					while (!_dumpped)
						Thread.Sleep (1000);
				} else {
					ConsoleIO.WriteLine ("({1}/{2}) pass.. {0}", sm.name, current, total);
				}
			}
			ConsoleIO.WriteLine ("done");
			Process.Start (filename);
			Thread.Sleep (2000);
		}

		private	void OnSelectStatus ()
		{
			ConsoleIO.Clear ();
			foreach (var sm in _stateMachines.Values) {
				if (sm is CrawlerLogic) {
					var crawler = sm as CrawlerLogic;
					ConsoleIO.WriteLine (crawler.Status ());
				}
			}
			ConsoleIO.WriteLine ("Press any key to return...");
			ConsoleIO.Read ();
		}

		private	void OnSelectLog ()
		{
			ConsoleIO.Clear ();
			ConsoleIO.WriteLine (ConsoleIO.GetLog ());
			ConsoleIO.WriteLine ("Press any key to return...");
			ConsoleIO.Read ();
		}

		private	void OnSelectExit ()
		{
			ConsoleIO.Clear ();
			ConsoleIO.WriteLine ("exiting..");
			ConsoleIO.ShowCursor ();
			Fire (TRIGGER_STOP);
		}

		#endregion
	}
}


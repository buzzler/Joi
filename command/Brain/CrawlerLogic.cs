using System;
using Joi.FSM;

namespace Joi.Brain
{
	public abstract class CrawlerLogic : StateMachine
	{
		protected const string STATE_INITIALIZING = "Initializing";
		protected const string STATE_GATHERING = "Gathering";
		protected const string STATE_STOPPED = "Stopped";

		protected const string TRIGGER_COMPLETE = "complete";
		protected const string TRIGGER_STOP = "stop";
		protected const string TRIGGER_START = "start";

		public CrawlerLogic (string name, int timeout, bool logging = true) : base(name, timeout, logging)
		{
			SetFirstState (STATE_INITIALIZING)
				.SetupEntry (OnEntryInit)
				.SetupLoop (OnLoopInit)
				.SetupExit (OnExitInit)
				.ConnectTo (TRIGGER_COMPLETE, STATE_GATHERING)
				.ConnectTo (TRIGGER_STOP, STATE_STOPPED);
			SetState (STATE_STOPPED)
				.SetupEntry (OnEntryStop)
				.SetupLoop (OnLoopStop)
				.SetupExit (OnExitStop)
				.ConnectTo (TRIGGER_START, STATE_INITIALIZING);
			SetState (STATE_GATHERING)
				.SetupEntry (OnEntryGather)
				.SetupLoop (OnLoopGather)
				.SetupExit (OnExitGather)
				.ConnectTo (TRIGGER_STOP, STATE_STOPPED);
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

		protected abstract void OnLoopGather();

		protected abstract void OnExitGather();

		#endregion

		#region 'Stopped' state

		protected abstract void OnEntryStop();

		protected abstract void OnLoopStop();

		protected abstract void OnExitStop();

		#endregion

		public abstract void Dump();
	}
}


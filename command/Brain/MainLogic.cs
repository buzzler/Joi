using System;
using Joi.FSM;

namespace Joi.Brain
{
	public	class MainLogic : StateMachine
	{
		private	const string STATE_INITIALIZING	= "Initializing";
		private	const string STATE_STOPPED		= "Stopped";
		private	const string STATE_RUNNING		= "Running";
		private	const string STATE_BLOCKED		= "Blocked";
		private	const string STATE_DISCONNECTED	= "Disconnected";
		private const string TRIGGER_COMPLETE	= "complete";
		private	const string TRIGGER_BLOCK		= "block";
		private	const string TRIGGER_UNBLOCK	= "unblock";
		private	const string TRIGGER_DISCONNECT	= "disconnect";
		private const string TRIGGER_CONNECT	= "connect";
		private	const string TRIGGER_START		= "start";
		private	const string TRIGGER_STOP		= "stop";

		public	MainLogic(bool logging = true) : base("MainLogic", logging)
		{
			this.SetFirstState (STATE_INITIALIZING)
				.SetupEntry (OnEntryInit)
				.SetupExit (OnExitInit)
				.SetupLoop (OnLoopInit)
				.ConnectTo (TRIGGER_COMPLETE, STATE_RUNNING);
			this.SetState (STATE_RUNNING)
				.SetupEntry (OnEntryRun)
				.SetupExit (OnExitRun)
				.SetupLoop (OnLoopRun)
				.ConnectTo (TRIGGER_DISCONNECT, STATE_DISCONNECTED)
				.ConnectTo (TRIGGER_BLOCK, STATE_BLOCKED)
				.ConnectTo (TRIGGER_STOP, STATE_STOPPED);
			this.SetState (STATE_BLOCKED)
				.SetupEntry (OnEntryBlock)
				.SetupExit (OnExitBlock)
				.SetupLoop (OnLoopBlock)
				.ConnectTo (TRIGGER_UNBLOCK, STATE_RUNNING)
				.ConnectTo (TRIGGER_STOP, STATE_STOPPED);
			this.SetState (STATE_DISCONNECTED)
				.SetupEntry (OnEntryDisconnect)
				.SetupExit (OnExitDisconnect)
				.SetupLoop (OnLoopDisconnect)
				.ConnectTo (TRIGGER_CONNECT, STATE_RUNNING)
				.ConnectTo (TRIGGER_STOP, STATE_STOPPED);
			this.SetState (STATE_STOPPED)
				.SetupEntry (OnEntryStop)
				.SetupExit (OnExitStop)
				.SetupLoop (OnLoopStop)
				.ConnectTo (TRIGGER_START, STATE_INITIALIZING);
			Start ();
		}

		~MainLogic()
		{
			End ();
		}

		#region 'Initializing' state

		private	void OnEntryInit()
		{
		}

		private	void OnLoopInit()
		{
		}

		private	void OnExitInit()
		{
		}

		#endregion

		#region 'Stopped' state

		private	void OnEntryStop()
		{
		}

		private	void OnLoopStop()
		{
		}

		private	void OnExitStop()
		{
		}

		#endregion

		#region 'Running' state

		private	void OnEntryRun()
		{
		}

		private	void OnLoopRun()
		{
		}

		private	void OnExitRun()
		{
		}

		#endregion

		#region 'Blocked' state

		private	void OnEntryBlock()
		{
		}

		private	void OnLoopBlock()
		{
		}

		private	void OnExitBlock()
		{
		}

		#endregion

		#region 'Disconnected' state

		private	void OnEntryDisconnect()
		{
		}

		private	void OnLoopDisconnect()
		{
		}

		private	void OnExitDisconnect()
		{
		}

		#endregion
	}
}


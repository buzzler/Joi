using System;
using System.Collections.Generic;

namespace Joi.FSM
{
	public	class State
	{
		private	StateMachine _stateMachine;
		private	string _name;
		private	Action _onEntry;
		private	Action _onLoop;
		private	Action _onExit;
		private	Dictionary<string, string> _triggerMap;

		/// <summary>
		/// Gets the state machine.
		/// </summary>
		/// <value>The state machine.</value>
		public	StateMachine stateMachine { get { return _stateMachine; } }
		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>The name.</value>
		public	string name { get { return _name; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="Joi.FSM.State"/> class.
		/// </summary>
		/// <param name="stateMachine">State machine.</param>
		/// <param name="name">Name.</param>
		public	State(StateMachine stateMachine, string name)
		{
			this._stateMachine = stateMachine;
			this._name = name;
			this._onEntry = null;
			this._onLoop = null;
			this._onExit = null;
			_triggerMap = new Dictionary<string, string> ();
		}

		/// <summary>
		/// Setups the entry.
		/// </summary>
		/// <returns>The entry.</returns>
		/// <param name="OnEntry">On entry.</param>
		public	State SetupEntry(Action OnEntry)
		{
			this._onEntry = OnEntry;
			return this;
		}

		/// <summary>
		/// Setups the loop.
		/// </summary>
		/// <returns>The loop.</returns>
		/// <param name="OnLoop">On loop.</param>
		public	State SetupLoop(Action OnLoop)
		{
			this._onLoop = OnLoop;
			return this;
		}

		/// <summary>
		/// Setups the exit.
		/// </summary>
		/// <returns>The exit.</returns>
		/// <param name="OnExit">On exit.</param>
		public	State SetupExit(Action OnExit)
		{
			this._onExit = OnExit;
			return this;
		}

		/// <summary>
		/// Connects to.
		/// </summary>
		/// <returns>The to.</returns>
		/// <param name="triggerName">Trigger name.</param>
		/// <param name="otherState">Other state.</param>
		public	State ConnectTo(string triggerName, string otherState)
		{
			if (_triggerMap.ContainsKey (triggerName))
				_triggerMap.Remove (triggerName);
			_triggerMap.Add (triggerName, otherState);
			return this;
		}

		/// <summary>
		/// Fires the on entry.
		/// </summary>
		public	void FireOnEntry()
		{
			if (_onEntry != null) {
				if (stateMachine.logging) Console.WriteLine ("{0}::{1}::OnEntry", stateMachine.name, name);
				_onEntry ();
			}
		}

		/// <summary>
		/// Fires the on loop.
		/// </summary>
		public	void FireOnLoop()
		{
//			if (_onLoop != null) {
//				if (stateMachine.logging) Console.WriteLine ("{0}::{1}::OnLoop", stateMachine.name, name);
				_onLoop ();
//			}
		}

		/// <summary>
		/// Fires the on exit.
		/// </summary>
		public	void FireOnExit()
		{
			if (_onExit != null) {
				if (stateMachine.logging) Console.WriteLine ("{0}::{1}::OnExit", stateMachine.name, name);
				_onExit ();
			}
		}

		/// <summary>
		/// Determines whether this instance has trigger the specified triggerName.
		/// </summary>
		/// <returns><c>true</c> if this instance has trigger the specified triggerName; otherwise, <c>false</c>.</returns>
		/// <param name="triggerName">Trigger name.</param>
		public	bool HasTrigger(string triggerName)
		{
			return _triggerMap.ContainsKey (triggerName);
		}

		/// <summary>
		/// Gets the state of the connected.
		/// </summary>
		/// <returns>The connected state.</returns>
		/// <param name="triggerName">Trigger name.</param>
		public	string GetConnectedState(string triggerName)
		{
			if (_triggerMap.ContainsKey (triggerName))
				return _triggerMap [triggerName];
			else
				return null;
		}
	}
}


using System;
using System.Collections.Generic;

namespace Joi.FSM
{
	public class StateMachine : IDisposable
	{
		private	bool _enable;
		private	bool _log;
		private	string _name;
		private State _first;
		private	State _prev;
		private	State _current;
		private	Dictionary<string, State> _states;

		/// <summary>
		/// Gets a value indicating whether this <see cref="Joi.FSM.StateMachine"/> is enable.
		/// </summary>
		/// <value><c>true</c> if enable; otherwise, <c>false</c>.</value>
		public	bool enable { get { return _enable; } }
		/// <summary>
		/// Gets a value indicating whether this <see cref="Joi.FSM.StateMachine"/> is logging.
		/// </summary>
		/// <value><c>true</c> if logging; otherwise, <c>false</c>.</value>
		public	bool logging { get { return _log; } }
		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>The name.</value>
		public	string name { get { return _name; } }
		/// <summary>
		/// Gets the first state.
		/// </summary>
		/// <value>The first state.</value>
		public	State firstState { get { return _first; } }
		/// <summary>
		/// Gets the state of the current.
		/// </summary>
		/// <value>The state of the current.</value>
		public	State currentState { get { return _current; } }
		/// <summary>
		/// Gets the state of the previous.
		/// </summary>
		/// <value>The state of the previous.</value>
		public	State prevState { get { return _prev; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="Joi.FSM.StateMachine"/> class.
		/// </summary>
		public StateMachine (string name = "", bool logging = true)
		{
			_enable = false;
			_log = logging;
			_name = name;
			_first = null;
			_prev = null;
			_current = null;
			_states = new Dictionary<string, State> ();
		}

		/// <summary>
		/// Sets the first state.
		/// </summary>
		/// <returns>The first state.</returns>
		/// <param name="stateName">State name.</param>
		public	State SetFirstState(string stateName)
		{
			if (_first != null && _states.ContainsKey (_first.name)) {
				_states.Remove (_first.name);
				_first = null;
			}
			_first = SetState (stateName);
			return _first;
		}

		/// <summary>
		/// Sets the state.
		/// </summary>
		/// <returns>The state.</returns>
		/// <param name="stateName">State name.</param>
		public	State SetState(string stateName)
		{
			if (_states.ContainsKey (stateName)) {
				return _states [stateName];
			}

			var state = new State (this, stateName);
			_states.Add (stateName, state);
			return state;
		}

		/// <summary>
		/// Gets the state.
		/// </summary>
		/// <returns>The state.</returns>
		/// <param name="stateName">State name.</param>
		public	State GetState(string stateName)
		{
			if (_states.ContainsKey (stateName))
				return _states [stateName];
			else
				return null;
		}

		/// <summary>
		/// Start this instance.
		/// </summary>
		public	StateMachine Start()
		{
			if (_first != null) {
				_current = _first;
				_current.FireOnEntry ();
			}
			_enable = true;
			return this;
		}

		/// <summary>
		/// Loop this instance.
		/// </summary>
		public	StateMachine Loop()
		{
			if (_current != null && _enable) {
				_current.FireOnLoop ();
			}
			return this;
		}

		/// <summary>
		/// Fire the specified triggerName.
		/// </summary>
		/// <param name="triggerName">Trigger name.</param>
		public	StateMachine Fire(string triggerName)
		{
			if (_current == null)
				return this;
			if (!_current.HasTrigger (triggerName))
				return this;

			State nextState = GetState (_current.GetConnectedState (triggerName));
			if (nextState != null) {
				_current.FireOnExit ();
				_prev = _current;
				_current = nextState;
				_current.FireOnEntry ();
			}
			return this;
		}

		/// <summary>
		/// End this instance.
		/// </summary>
		public	StateMachine End()
		{
			if (_current != null)
				_current.FireOnExit ();
			_prev = _current;
			_current = null;
			_enable = false;
			return this;
		}

		/// <summary>
		/// Releases all resource used by the <see cref="Joi.FSM.StateMachine"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="Joi.FSM.StateMachine"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="Joi.FSM.StateMachine"/> in an unusable state. After calling
		/// <see cref="Dispose"/>, you must release all references to the <see cref="Joi.FSM.StateMachine"/> so the garbage
		/// collector can reclaim the memory that the <see cref="Joi.FSM.StateMachine"/> was occupying.</remarks>
		public void Dispose()
		{
			End ();
			_first = null;
			_prev = null;
			_current = null;
			_states = null;
		}
	}
}


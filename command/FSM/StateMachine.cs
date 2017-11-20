using System;
using System.Collections.Generic;

namespace Joi.FSM
{
	public class StateMachine : IDisposable
	{
		private	static Dictionary <string, StateMachine> _stateMachines;

		public	static Dictionary <string, StateMachine> stateMachines { get { return _stateMachines; } }

		private	bool _enable;
		private	bool _log;
		private	int _timeout;
		private	string _name;
		private State _first;
		private	State _prev;
		private	State _current;
		private	State _any;
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
		public StateMachine (string name, int timeout = -1, bool logging = true)
		{
			if (_stateMachines == null)
				_stateMachines = new Dictionary<string, StateMachine> ();
			if (_stateMachines.ContainsKey (name))
				throw new Exception ("duplicated StateMachine: " + name);
			else
				_stateMachines.Add (name, this);

			_enable = false;
			_log = logging;
			_timeout = timeout;
			_name = name;
			_first = null;
			_prev = null;
			_current = null;
			_any = new State (this, "any");
			_states = new Dictionary<string, State> ();
		}

		~StateMachine ()
		{
			_stateMachines.Remove (name);
		}

		/// <summary>
		/// Sets the first state.
		/// </summary>
		/// <returns>The first state.</returns>
		/// <param name="stateName">State name.</param>
		public	State SetFirstState (string stateName)
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
		public	State SetState (string stateName)
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
		public	State GetState (string stateName)
		{
			if (_states.ContainsKey (stateName))
				return _states [stateName];
			else
				return null;
		}

		public	State AnyState ()
		{
			return _any;
		}

		/// <summary>
		/// Start this instance.
		/// </summary>
		public	void Start ()
		{
			if (_first != null) {
				_current = _first;
				_current.FireOnEntry ();
			}
			if (_any != null) {
				_any.FireOnEntry ();
			}
			_enable = true;
		}

		public	void Run ()
		{
			while (_enable) {
				Loop ();
				Sleep ();
			}
		}

		public	void Sleep (int time = -1)
		{
			if (time > 0)
				System.Threading.Thread.Sleep (time);
			else if (_timeout > 0)
				System.Threading.Thread.Sleep (_timeout);
		}

		/// <summary>
		/// Loop this instance.
		/// </summary>
		public	void Loop ()
		{
			if (!_enable)
				return;
			
			if (_current != null)
				_current.FireOnLoop ();
			if (_any != null)
				_any.FireOnLoop ();
		}

		/// <summary>
		/// Fire the specified triggerName.
		/// </summary>
		/// <param name="triggerName">Trigger name.</param>
		public	void Fire (string triggerName)
		{
			State nextState = null;
			if (_current != null && _current.HasTrigger (triggerName)) {
				nextState = GetState (_current.GetConnectedState (triggerName));
			} else if (_any != null && _any.HasTrigger (triggerName)) {
				nextState = GetState (_any.GetConnectedState (triggerName));
			}
			if (nextState != null && nextState != _current) {
				_current.FireOnExit ();
				_prev = _current;
				_current = nextState;
				_current.FireOnEntry ();
			}
		}

		/// <summary>
		/// End this instance.
		/// </summary>
		public	void End ()
		{
			if (_current != null)
				_current.FireOnExit ();
			if (_any != null)
				_any.FireOnExit ();
			_prev = _current;
			_current = null;
			_enable = false;
		}

		/// <summary>
		/// Releases all resource used by the <see cref="Joi.FSM.StateMachine"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="Joi.FSM.StateMachine"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="Joi.FSM.StateMachine"/> in an unusable state. After calling
		/// <see cref="Dispose"/>, you must release all references to the <see cref="Joi.FSM.StateMachine"/> so the garbage
		/// collector can reclaim the memory that the <see cref="Joi.FSM.StateMachine"/> was occupying.</remarks>
		public void Dispose ()
		{
			End ();
			_first = null;
			_prev = null;
			_current = null;
			_any = null;
			_states = null;
		}
	}
}


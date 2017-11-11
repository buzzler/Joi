using System;

namespace Joi.Data.Indicator
{
	public	class IndicatorUnit
	{
		private	int _openTime;
		private int _closeTime;
		private	IndicatorUnit _previous;
		private	IndicatorUnit _next;

		public	int openTime { get { return _openTime; } }

		public	int closeTime { get { return _closeTime; } }

		public	IndicatorUnit previous { 
			get { return _previous; }
			set { 
				if (_previous != null)
					_previous._next = null;
				_previous = value;
				if (_previous != null)
					_previous._next = this;
			}
		}

		public	IndicatorUnit next { 
			get { return _next; } 
			set {
				if (_next != null)
					_next._previous = null;
				_next = value;
				if (_next != null)
					_next._previous = this;
			}
		}

		public	void ResetTimeForward (int openTime, TimeInterval interval)
		{
			_openTime = openTime;
			_closeTime = openTime + (int)interval;
			if (_next != null)
				_next.ResetTimeForward (_closeTime, interval);
		}

		public	void ResetTimeBackward (int closeTime, TimeInterval interval)
		{
			_closeTime = closeTime;
			_openTime = closeTime - (int)interval;
			if (_previous != null)
				_previous.ResetTimeBackward (_openTime, interval);
		}

		public	bool IsValidTime(int timestamp)
		{
			return (_openTime < timestamp) && (_closeTime >= timestamp);
		}
	}
}


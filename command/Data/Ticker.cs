using System;

namespace Joi.Data
{
	public class Ticker
	{
		private	float _lastHighestBid;
		private	float _lastHighestSize;
		private	float _lastLowestAsk;
		private	float _lastLowestSize;
		private	float _volume;

		public	float lastHighestBid { get { return _lastHighestBid; } }

		public	float lastHighestSize { get { return _lastHighestSize; } }

		public	float lastLowestAsk { get { return _lastLowestAsk; } }

		public	float lastLowestSize { get { return _lastLowestSize; } }

		public	float volume { get { return _volume; } }

		public	Ticker ()
		{
			_lastHighestBid = 0f;
			_lastHighestSize = 0f;
			_lastLowestAsk = 0f;
			_lastLowestSize = 0f;
			_volume = 0f;
		}

		public	void Update (float highPrice = 0f, float highAmount = 0f, float lowPrice = 0f, float lowAmount = 0f, float volume = 0f)
		{
			_lastHighestBid = highPrice;
			_lastHighestSize = highAmount;
			_lastLowestAsk = lowPrice;
			_lastLowestSize = lowAmount;
			_volume = volume;
		}
	}
}


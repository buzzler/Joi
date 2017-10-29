using System;

namespace Joi.Bitfinex
{
	public class Limit
	{
		public	static TimeSpan query = new TimeSpan(0,0,0,0,6000);

		public	const int QUERY_TIMEOUT = 6000;			// 10 query per a minute
		public	const int TICKER_TIMEOUT = 1000;		// 60 ticker per a minute
		public	const int STATS_TIMEOUT = 6000;			// 10 stats per a minute
		public	const int FUNDINGBOOK_TIMEOUT = 1334;	// 45 fundingbook per a minute
		public	const int ORDERBOOK_TIMEOUT = 1000;		// 60 orderbook per a minute
		public	const int TRADES_TIMEOUT = 1334;		// 45 trades per a minute
		public	const int LENDS_TIMEOUT = 1000;			// 60 lends per a minute
		public	const int SYMBOLS_TIMEOUT = 12000;		// 5 symbol per a minute
		public	const int SYMBOLDETAIL_TIMEOUT = 12000;	// 5 symbol detail per a minute
	}
}


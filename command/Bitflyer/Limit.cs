using System;

namespace Joi.Bitflyer
{
	public class Limit
	{
		public	static TimeSpan query = new TimeSpan(0,0,0,0,120);
		public	const int QUERY_TIMEOUT = 120;		// 500 queries per a minute
	}
}


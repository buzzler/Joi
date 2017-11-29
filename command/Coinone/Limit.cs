using System;

namespace Joi.Coinone
{
	public class Limit
	{
		public	const int QUERY_TIMEOUT = 15000;		// 90 queries per a minute
		public	const int ORDER_TIMEOUT = 15000;		// 360 orders per a minute
	}
}


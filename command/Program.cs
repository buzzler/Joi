using System;
using Joi.Brain;

namespace Joi
{
	class Body
	{
		public static void Main (string[] args)
		{
			using (var ruvu = new MainLogic ()) {
				while (ruvu != null) {
					ruvu.Loop ();
					System.Threading.Thread.Sleep (1000);
				}
			}
		}
	}


}

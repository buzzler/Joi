namespace Joi
{
	class Body
	{
		public	static void Main (string[] args)
		{
			var app = new Joi.Brain.AppLogic (false);
			while (app.enable)
				app.Loop ();
		}
	}


}

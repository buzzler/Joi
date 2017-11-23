namespace Joi
{
	class Body
	{
		public	static void Main (string[] args)
		{
			var app = new Joi.Brain.AppLogic (true);
			while (app.enable)
				app.Loop ();
		}
	}
}

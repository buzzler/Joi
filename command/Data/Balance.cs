using System;
using System.Collections.Generic;

namespace Joi.Data
{
	public class Balance
	{
		private	Dictionary<Symbol, Tuple<double, double>> _wallet;

		public	Balance()
		{
			_wallet = new Dictionary<Symbol, Tuple<double, double>> ();
		}

		public	void Clear()
		{
			_wallet.Clear ();
		}

		public	void SetValue(Symbol symbol, double amount, double available)
		{
			if (_wallet.ContainsKey (symbol))
				_wallet.Remove (symbol);
			_wallet.Add (symbol, new Tuple<double, double> (amount, available));
		}

		public	double GetAvailable(Symbol symbol)
		{
			if (_wallet.ContainsKey (symbol))
				return _wallet [symbol].Item2;
			return 0;
		}

		public	double GetAmount(Symbol symbol)
		{
			if (_wallet.ContainsKey (symbol))
				return _wallet [symbol].Item1;
			return 0;
		}
	}
}


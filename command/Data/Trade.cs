using System;
using LitJson;

namespace Joi.Data
{
	public class Trade
	{
		private	int _id;
		private	double _price;
		private	double _amount;
		private	int _timestamp;

		public	int id { get { return _id; } }
		public	double price { get { return _price; } }
		public	double amount { get { return _amount; } }
		public	int timestamp { get { return _timestamp; } }

		public	Trade(int id, double price, double amount, int timestamp)
		{
			_id = id;
			_price = price;
			_amount = amount;
			_timestamp = timestamp;
		}

		public	Trade()
		{
			_id = -1;
			_price = 0f;
			_amount = 0f;
			_timestamp = -1;
		}

		public	static Trade CreateFromCoinone(JsonData json)
		{
			var trade = new Trade ();
			trade._id = int.Parse (json ["timestamp"].ToString ());
			trade._price = double.Parse (json ["price"].ToString ());
			trade._amount = double.Parse (json ["qty"].ToString ());
			trade._timestamp = trade._id;
			return trade;
		}

		public	static Trade CreateFromBitfinex(JsonData json)
		{
			var trade = new Trade ();
			trade._id = int.Parse (json ["tid"].ToString ());
			trade._price = double.Parse (json ["price"].ToString ());
			trade._amount = double.Parse (json ["amount"].ToString ());
			trade._timestamp = int.Parse (json ["timestamp"].ToString ());
			return trade;
		}

		public	static Trade CreateFromBitflyer(JsonData json)
		{
			var trade = new Trade ();
			trade._id = int.Parse (json ["id"].ToString ());
			trade._price = double.Parse (json ["price"].ToString ());
			trade._amount = double.Parse (json ["size"].ToString ());
			trade._timestamp = Utility.Timestamp (DateTime.Parse (json ["timestamp"].ToString ()));
			return trade;
		}
	}
}


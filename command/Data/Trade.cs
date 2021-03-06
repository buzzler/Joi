﻿using System;
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

		public	Trade (int id, double price, double amount, int timestamp)
		{
			_id = id;
			_price = Math.Abs(price);
			_amount = Math.Abs(amount);
			_timestamp = timestamp;
		}

		public	Trade ()
		{
			_id = -1;
			_price = 0f;
			_amount = 0f;
			_timestamp = -1;
		}
	}
}


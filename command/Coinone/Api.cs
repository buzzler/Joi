using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using LitJson;

/**
 * version 0.1.8
 * http://doc.coinone.co.kr
 */
namespace Joi.Coinone
{
	public class Api
	{
		private	Dictionary<int, string> _errorCode = new Dictionary<int, string> () {
			{ 0, "" },
			{ 4, "Blocked user access" },
			{ 11, "Access token is missing" },
			{ 12, "Invalid access token" },
			{ 40, "Invalid API permission" },
			{ 50, "Authenticate error" },
			{ 51, "Invalid API" },
			{ 52, "Deprecated API" },
			{ 53, "Two Factor Auth Fail" },
			{ 100, "Session expired" },
			{ 101, "Invalid format" },
			{ 102, "ID is not exist" },
			{ 103, "Lack of Balance" },
			{ 104, "Order id is not exist" },
			{ 105, "Price is not correct" },
			{ 106, "Locking error" },
			{ 107, "Parameter error" },
			{ 111, "Order id is not exist" },
			{ 112, "Cancel failed" },
			{ 113, "Quantity is too low(ETH, ETC > 0.01)" },
			{ 120, "V2 API payload is missing" },
			{ 121, "V2 API signature is missing" },
			{ 122, "V2 API nonce is missing" },
			{ 123, "V2 API signature is not correct" },
			{ 130, "V2 API Nonce value must be a positive integer" },
			{ 131, "V2 API Nonce is must be bigger then last nonce" },
			{ 132, "V2 API body is corrupted" },
			{ 141, "Too many limit orders" },
			{ 150, "It's V1 API. V2 Access token is not acceptable" },
			{ 151, "It's V2 API. V1 Access token is not acceptable" },
			{ 200, "Wallet Error" },
			{ 202, "Limitation error" },
			{ 210, "Limitation error" },
			{ 220, "Limitation error" },
			{ 221, "Limitation error" },
			{ 310, "Mobile auth error" },
			{ 311, "Need mobile auth" },
			{ 312, "Name is not correct" },
			{ 330, "Phone number error" },
			{ 404, "Page not found error" },
			{ 405, "Server error" },
			{ 444, "Locking error" },
			{ 500, "Email error" },
			{ 501, "Email error" },
			{ 777, "Mobile auth error" },
			{ 778, "Phone number error" },
			{ 1202, "App not found" },
			{ 1203, "Already registered" },
			{ 1204, "Invalid access" },
			{ 1205, "API Key error" },
			{ 1206, "User not found" },
			{ 1207, "User not found" },
			{ 1208, "User not found" },
			{ 1209, "User not found" }
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="Joi.Coinone.Api"/> class.
		/// </summary>
		public Api ()
		{
		}

		#region Public

		/// <summary>
		/// Gets the orderbook.
		/// </summary>
		/// <returns>The orderbook.</returns>
		/// <param name="currency">Allowed values: btc, bch, eth, etc, xrp, qtum.</param>
		public	JsonData GetOrderbook (string currency)
		{
			return _GetV1Response (string.Format ("orderbook?currency={0}", currency));
		}

		/// <summary>
		/// Gets the complete orders.
		/// </summary>
		/// <returns>The complete orders.</returns>
		/// <param name="currency">Allowed values: btc, bch, eth, etc, xrp, qtum.</param>
		/// <param name="period">Period.</param>
		public	JsonData GetCompleteOrders (string currency, string period)
		{
			return _GetV1Response (string.Format ("trades?currency={0}&period={1}", currency, period));
		}

		/// <summary>
		/// Gets the ticker.
		/// </summary>
		/// <returns>The ticker.</returns>
		/// <param name="currency">Allowed values: btc, bch, eth, etc, xrp, qtum.</param>
		public	JsonData GetTicker (string currency)
		{
			return _GetV1Response (string.Format ("ticker?currency={0}", currency));
		}

		#endregion

		#region Account

		/// <summary>
		/// Gets the balance.
		/// </summary>
		/// <returns>The balance.</returns>
		public	JsonData GetBalance ()
		{
			return _GetV2Response ("account/balance");
		}

		/// <summary>
		/// Gets the daily balance.
		/// </summary>
		/// <returns>The daily balance.</returns>
		public	JsonData GetDailyBalance ()
		{
			return _GetV2Response ("account/daily_balance");
		}

		/// <summary>
		/// Gets the deposit address.
		/// </summary>
		/// <returns>The deposit address.</returns>
		public	JsonData GetDepositAddress ()
		{
			return _GetV2Response ("account/deposit_address");
		}

		/// <summary>
		/// Gets the user infomation.
		/// </summary>
		/// <returns>The user infomation.</returns>
		public	JsonData GetUserInfomation ()
		{
			return _GetV2Response ("account/user_info");
		}

		/// <summary>
		/// Gets the virtual account.
		/// </summary>
		/// <returns>The virtual account.</returns>
		public	JsonData GetVirtualAccount ()
		{
			return _GetV2Response ("account/virtual_account");
		}

		#endregion

		#region Order

		/// <summary>
		/// Determines whether this instance cancel order the specified order_id price qty is_ask currency.
		/// </summary>
		/// <returns><c>true</c> if this instance cancel order the specified order_id price qty is_ask currency; otherwise, <c>false</c>.</returns>
		/// <param name="order_id">Order identifier.</param>
		/// <param name="price">KRW price.</param>
		/// <param name="qty">BTC/BCH/ETH/ETC/XRP/QTUM quantity.</param>
		/// <param name="is_ask">Is ask.</param>
		/// <param name="currency">Allowed values: btc, bch, eth, etc, xrp, qtum.</param>
		public	JsonData CancelOrder (string order_id, long price, double qty, int is_ask, string currency)
		{
			var payload = new JsonData ();
			payload ["order_id"] = order_id;
			payload ["price"] = price;
			payload ["qty"] = qty;
			payload ["is_ask"] = is_ask;
			payload ["currency"] = currency;
			return _GetV2Response ("order/cancel", payload);
		}

		/// <summary>
		/// Limits the buy.
		/// </summary>
		/// <returns>The buy.</returns>
		/// <param name="price">KRW price.</param>
		/// <param name="qty">BTC/BCH/ETH/ETC/XRP/QTUM quantity.</param>
		/// <param name="currency">Allowed values: btc, bch, eth, etc, xrp, qtum.</param>
		public	JsonData LimitBuy (long price, double qty, string currency)
		{
			var payload = new JsonData ();
			payload ["price"] = price;
			payload ["qty"] = qty;
			payload ["currency"] = currency;
			return _GetV2Response ("order/limit_buy", payload);
		}

		/// <summary>
		/// Limits the sell.
		/// </summary>
		/// <returns>The sell.</returns>
		/// <param name="price">KRW price.</param>
		/// <param name="qty">BTC/BCH/ETH/ETC/XRP/QTUM quantity.</param>
		/// <param name="currency">Allowed values: btc, bch, eth, etc, xrp, qtum.</param>
		public	JsonData LimitSell (long price, double qty, string currency)
		{
			var payload = new JsonData ();
			payload ["price"] = price;
			payload ["qty"] = qty;
			payload ["currency"] = currency;
			return _GetV2Response ("order/limit_sell", payload);
		}

		/// <summary>
		/// Gets my complete orders.
		/// </summary>
		/// <returns>The my complete orders.</returns>
		/// <param name="currency">Allowed values: btc, bch, eth, etc, xrp, qtum.</param>
		public	JsonData GetMyCompleteOrders (string currency)
		{
			var payload = new JsonData ();
			payload ["currency"] = currency;
			return _GetV2Response ("order/complete_orders", payload);
		}

		/// <summary>
		/// Gets my limit orders.
		/// </summary>
		/// <returns>The my limit orders.</returns>
		/// <param name="currency">Allowed values: btc, bch, eth, etc, xrp, qtum.</param>
		public	JsonData GetMyLimitOrders (string currency)
		{
			var payload = new JsonData ();
			payload ["currency"] = currency;
			return _GetV2Response ("order/limit_orders", payload);
		}

		/// <summary>
		/// Gets my order infomation.
		/// </summary>
		/// <returns>The my order infomation.</returns>
		/// <param name="order_id">Order identifier.</param>
		/// <param name="currency">Allowed values: btc, bch, eth, etc, xrp, qtum.</param>
		public	JsonData GetMyOrderInfomation (string order_id, string currency)
		{
			var payload = new JsonData ();
			payload ["order_id"] = order_id;
			payload ["currency"] = currency;
			return _GetV2Response ("order/order_info", payload);
		}

		#endregion

		#region Utility

		private	JsonData _GetV2Response (string uri, JsonData payload = null)
		{
			if (payload == null)
				payload = new JsonData ();

			payload ["access_token"] = Key.ACCESS_TOKEN;
			payload ["nonce"] = Utility.Timestamp (DateTime.Now);
			var payload_byte = UTF8Encoding.UTF8.GetBytes (payload.ToJson ());
			var payload_base64 = Convert.ToBase64String (payload_byte);

			var hmac = new HMACSHA512 (UTF8Encoding.UTF8.GetBytes (Key.SECRET_KEY));
			var hash = hmac.ComputeHash (UTF8Encoding.UTF8.GetBytes (payload_base64));
			var signature = Utility.HexStringFromBytes (hash);

			var request = HttpWebRequest.Create (string.Format("{0}/v2/{1}", Key.URL, uri));
			request.Method = "POST";
			request.Headers.Set ("X-COINONE-PAYLOAD", payload_base64);
			request.Headers.Set ("X-COINONE-SIGNATURE", signature);
			request.ContentType = "application/json";
			request.ContentLength = payload_byte.Length;

			using (var stream = request.GetRequestStream ())
				stream.Write (payload_byte, 0, payload_byte.Length);

			return _GetCommonResponse (request);
		}

		private	JsonData _GetV1Response (string uri)
		{
			return _GetCommonResponse (WebRequest.Create (string.Format("{0}/{1}", Key.URL, uri)));
		}

		private	JsonData _GetCommonResponse (WebRequest request)
		{
			var json = Utility.GetResponse (request);
			var errorCode = int.Parse (json ["errorCode"].ToString ());

			if (!_errorCode.ContainsKey (errorCode)) {
				ConsoleIO.Error("uknown errorCode: {0}", errorCode);
			} else {
				var errorMessage = _errorCode [errorCode];
				if (!string.IsNullOrEmpty (errorMessage))
					ConsoleIO.Error (errorMessage);
			}
			return json;
		}

		#endregion
	}
}


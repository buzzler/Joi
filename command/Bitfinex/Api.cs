using System;
using LitJson;
using System.Net;
using WebSocketSharp;

namespace Joi.Bitfinex
{
	public class Api
	{
		private WebSocket _socket;
		private	int _channelTrade;
		private	int _channelOrderBook;
		private	int _channelTicker;
		private	Action<string> _onError;
		private	Action<JsonData> _onTrade;
		private	Action<JsonData> _onOrderBook;
		private	Action<JsonData> _onTicker;

		public	Api()
		{
			_socket = new WebSocket (Key.SOCKET);
		}

		public	void Connect(Action<string> onError)
		{
			if (!_socket.IsAlive) {
				_onError = onError;
//				_socket.OnMessage += OnOpened;
				_socket.OnMessage += OnMessage;
				_socket.OnError += OnError;
				_socket.Connect ();
			}
		}

		public	void SubscribeTrade(string symbol, Action<JsonData> onMessage)
		{
			if (_socket.IsAlive) {
				var json = new JsonData ();
				json ["event"] = "subscribe";
				json ["channel"] = "trades";
				json ["symbol"] = symbol;
				_onTrade = onMessage;
//				_socket.OnMessage += OnTradeSubscribe;
				_socket.Send (json.ToJson ());
			}
		}

		public	void UnsubscribeTrade()
		{
			if (_socket.IsAlive && _channelTrade > 0) {
				var json = new JsonData ();
				json ["event"] = "unsubscribe";
				json ["chanId"] = _channelTrade;
				_onTrade = null;
//				_socket.OnMessage -= OnTradeMessage;
				_socket.Send (json.ToJson ());
			}
		}

		public	void SubscribeOrderBook(string symbol, Action<JsonData> onMessage)
		{
			if (_socket.IsAlive) {
				var json = new JsonData ();
				json ["event"] = "subscribe";
				json ["channel"] = "book";
				json ["symbol"] = symbol;
				json ["prec"] = "P0";
				json ["freq"] = "F0";
				_onOrderBook = onMessage;
				_socket.Send (json.ToJson ());
			}
		}

		public	void UnsubscribeOrderBook()
		{
			if (_socket.IsAlive && _channelOrderBook > 0) {
				var json = new JsonData ();
				json ["event"] = "unsubscribe";
				json ["chanId"] = _channelOrderBook;
				_onOrderBook = null;
				_socket.Send (json.ToJson ());
			}
		}

		public	void Disconnect()
		{
			if (_socket.IsAlive) {
				_onError = null;
				_socket.OnError -= OnError;
				_socket.OnMessage -= OnMessage;
				_socket.Close ();
			}
		}

		public	JsonData GetTicker(string symbol)
		{
			return _GetCommonResponse (string.Format("pubticker/{0}", symbol));
		}

		public	JsonData GetStats(string symbol)
		{
			return _GetCommonResponse (string.Format("stats/{0}", symbol));
		}

		public	JsonData GetFundingBook(string currency, int limit_bids = 50, int limit_asks = 50)
		{
			return _GetCommonResponse (string.Format("lendbook/{0}?limit_bids={1}&limit_asks={2}", currency, limit_bids, limit_asks));
		}

		public	JsonData GetOrderBook(string symbol, int limit_bids = 50, int limit_asks = 50, int group = 1)
		{
			return _GetCommonResponse (string.Format("book/{0}?limit_bids={1}&limit_asks={2}&group={2}", symbol, limit_bids, limit_asks, group));
		}

		public	JsonData GetTrades(string symbol, int timestamp)
		{
			return _GetCommonResponse (string.Format ("trades/{0}?timestamp={1}", symbol, timestamp));
		}

		public	JsonData GetTrades(string symbol, int timestamp, int limit_trades)
		{
			return _GetCommonResponse (string.Format ("trades/{0}?limit_trades={1}&timestamp={2}", symbol, limit_trades, timestamp));
		}

		public	JsonData GetLends(string currency, int limit_lends = 50)
		{
			return _GetCommonResponse (string.Format ("lends/{0}?limit_lends={1}", currency, limit_lends));
		}

		public	JsonData GetLends(string currency, int timestamp, int limit_lends = 50)
		{
			return _GetCommonResponse (string.Format ("lends/{0}?limit_lends={1}&timestamp={2}", currency, limit_lends, timestamp));
		}

		public	JsonData GetSymbols()
		{
			return _GetCommonResponse ("symbols");
		}

		public	JsonData GetSymbolDetails()
		{
			return _GetCommonResponse ("symbols_details");
		}

		private	JsonData _GetCommonResponse(string uri)
		{
			var url = string.Format ("{0}/{1}", Key.URL, uri);
			var request = WebRequest.Create (url);
			return Utility.GetResponse (request);
		}

		private	void OnOpened (object sender, MessageEventArgs e)
		{
			var json = JsonMapper.ToObject (e.Data);
			string response = json ["event"].ToString ();
			if (response != "info") {
				Console.Error.WriteLine (json.ToJson ());
			}
			_socket.OnMessage -= OnOpened;
		}

		private	void OnTradeSubscribe(object sender, MessageEventArgs e)
		{
			var json = JsonMapper.ToObject (e.Data);
			string response = json ["event"].ToString ();
			if (response == "subscribed") {
				_channelTrade = int.Parse (json ["chanId"].ToString ());
				_socket.OnMessage += OnTradeMessage;
			} else if (response == "error") {
				Console.Error.WriteLine ("error({0}): {1}", json ["code"].ToString (), json ["msg"].ToString ());
			}
			_socket.OnMessage -= OnTradeSubscribe;
		}

		private	void OnTradeMessage (object sender, MessageEventArgs e)
		{
			if (_onTrade == null)
				return;
			var json = JsonMapper.ToObject (e.Data);
			var len = json.Count;
			var chanId = int.Parse (json [0].ToString ());
			if (chanId == _channelTrade) {
				if (len == 2 && json [1].IsArray) {
					var trades = json [1];
					for (int i = 0; i < trades.Count; i++) {
						_onTrade (trades [i]);
					}
				} else if (len == 6) {
					_onTrade (json);
				}
			}
		}

		private	void OnMessage (object sender, MessageEventArgs e)
		{
			var json = JsonMapper.ToObject (e.Data);
			if (json.IsArray) {
				if (json.Count == 0)
					return;

				var chanId = int.Parse (json [0].ToString ());
				switch (chanId) {
				case _channelOrderBook:
					if (_onOrderBook != null)
						_onOrderBook (json);
					break;
				case _channelTicker:
					if (_onTicker != null)
						_onTicker (json);
					break;
				case _channelTrade:
					if (_onTrade != null)
						_onTrade (json);
					break;
				}
			} else if (json.IsObject) {
				var responce = json ["event"].ToString ();
				var protocol = json ["channel"].ToString ();
				var chanId = int.Parse (json ["chanId"].ToString ());

				if (responce != "subscribed")
					return;

				switch (protocol) {
				case "book":
					_channelOrderBook = chanId;
					break;
				case "trades":
					_channelTrade = chanId;
					break;
				case "ticker":
					_channelTicker = chanId;
					break;
				}
			}
		}

		private	void OnError (object sender, ErrorEventArgs e)
		{
			if (_onError != null)
				_onError (e.Message);
		}
	}
}


using System;
using LitJson;
using System.Net;
using WebSocketSharp;

namespace Joi.Bitfinex
{
	public class Api
	{
		public	Api()
		{
		}

		#region web-socket

		private WebSocket _socket;
		private	int _channelTrade;
		private	int _channelOrderBook;
		private	int _channelTicker;
		private	Action<string> _onError;
		private	Action<JsonData> _onTrade;
		private	Action<JsonData> _onOrderBook;
		private	Action<JsonData> _onTicker;

		public	void Connect(Action<string> onError)
		{
			if (_socket == null) {
				_onError = onError;
				_socket = new WebSocket (Key.SOCKET);
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

		public	void SubscribeTicker(string symbol, Action<JsonData> onMessage)
		{
			if (_socket.IsAlive) {
				var json = new JsonData ();
				json ["event"] = "subscribe";
				json ["channel"] = "ticker";
				json ["symbol"] = symbol;
				_onTicker = onMessage;
				_socket.Send (json.ToJson ());
			}
		}

		public	void UnsubscribeTicker()
		{
			if (_socket.IsAlive && _channelTicker > 0) {
				var json = new JsonData ();
				json ["event"] = "unsubscribe";
				json ["chanId"] = _channelTicker;
				_onTicker = null;
				_socket.Send (json.ToJson ());
			}
		}

		public	void Disconnect()
		{
			if (_socket != null && _socket.IsAlive) {
				_onError = null;
				_socket.OnError -= OnError;
				_socket.OnMessage -= OnMessage;
				_socket.Close ();
				_socket = null;
			}
		}

		#endregion

		#region Rest API

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

		public	JsonData GetTrades(string symbol, int timestamp, int limit_trades = 999)
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

		#endregion

		#region web-socket event handler

		private	void OnMessage (object sender, MessageEventArgs args)
		{
			try
			{
				var json = JsonMapper.ToObject (args.Data);

				if (json.IsArray) {
					var count = json.Count;
					if (count == 0)
						return;

					Action<JsonData> onCallback = null;
					var chanId = int.Parse (json [0].ToString ());
					if (chanId == _channelOrderBook && _onOrderBook != null)
						onCallback = _onOrderBook;
					else if (chanId == _channelTicker && _onTicker != null)
						onCallback = _onTicker;
					else if (chanId == _channelTrade && _onTrade != null)
						onCallback = _onTrade;
					else
						return;

					if (count == 2 && json [1].IsArray) {
						var ary = json [1];
						for (int i = 0; i < ary.Count; i++)
							onCallback (ary [i]);
					} else {
						onCallback (json);
					}
				} else if (json.IsObject) {
					var responce = json ["event"].ToString ();
					if (responce == "info") {
						return;
					} else if (responce != "subscribed")
						return;

					var protocol = json ["channel"].ToString ();
					var chanId = int.Parse (json ["chanId"].ToString ());
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
			} catch (Exception e) {
				if (_onError != null)
					_onError (e.Message);
			}
		}

		private	void OnError (object sender, ErrorEventArgs e)
		{
			if (_onError != null)
				_onError (e.Message);
		}

		#endregion
	}
}


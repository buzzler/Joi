using System;
using System.Timers;
using Joi.Bitfinex;
using LitJson;
using Joi.Data;
using WebSocketSharp;
using Mono.Data.Sqlite;

namespace Joi.Brain
{
    public class CrawlerBitfinex : CrawlerLogic
    {
        private Api _api;
        private string _symbol;
		private	Timer _timerTrade;
		private	Timer _timerOrderbook;
		private	Timer _timerTicker;

        public CrawlerBitfinex(Symbol symbol, bool logging = true) : base(BITFINEX, Joi.Bitfinex.Limit.QUERY_TIMEOUT, logging)
		{
			_api = new Api ();
			_market = new Market (name, TimeInterval.DAY_3);
			_market.SetIndicator (TimeInterval.MINUTE_1, TimeInterval.HOUR_5);
			_market.SetIndicator (TimeInterval.MINUTE_5, TimeInterval.HOUR_30);

			switch (symbol) {
			case Symbol.BITCOIN:
				_symbol = "btcusd";
				break;
			case Symbol.ETHEREUM:
				_symbol = "ethusd";
				break;
			}
		}

        protected override void OnEntryInit()
        {
        }

        protected override void OnLoopInit()
        {
			ConnectDatabase ();
            GetTradeByWeb(Utility.Timestamp(DateTime.Now) - (int)TimeInterval.DAY_3);
            Sleep();
            Fire(TRIGGER_COMPLETE);
        }

        protected override void OnExitInit()
        {
        }

        protected override void OnEntryGather()
        {
//			RestartTimer ();
//            _api.Connect(OnSocketError);
//            _api.SubscribeTrade(_symbol, OnSubscribedTrade);
//            _api.SubscribeOrderBook(_symbol, OnSubscribeOrderBook);
//            _api.SubscribeTicker(_symbol, OnSubscribeTicker);
        }

        protected override void OnLoopGather()
        {
            base.OnLoopGather();
			GetTradeByWeb (_market.GetLastTimestamp ());
        }

        protected override void OnExitGather()
        {
//            _api.UnsubscribeTrade();
//            _api.UnsubscribeTicker();
//			_api.UnsubscribeOrderBook();
//			_api.Disconnect();
//			StopTimer();
			DisconnectDatabase ();
        }

        protected override void OnEntryStop()
        {
        }

        protected override void OnLoopStop()
        {
			Fire (TRIGGER_START);
        }

        protected override void OnExitStop()
        {
        }

		private void RestartTimer(Timer timer = null)
		{
			if (timer != null) {
				timer.Stop ();
				timer.Start ();
				return;
			}

			if (_timerTrade == null) {
				_timerTrade = new Timer (10000);
				_timerTrade.AutoReset = false;
				_timerTrade.Elapsed += OnTimer;
			} else {
				_timerTrade.Stop ();
			}
			 
			if (_timerOrderbook == null) {
				_timerOrderbook = new Timer (10000);
				_timerOrderbook.AutoReset = false;
				_timerOrderbook.Elapsed += OnTimer;
			} else {
				_timerOrderbook.Stop ();
			}
			if (_timerTicker == null) {
				_timerTicker = new Timer (10000);
				_timerTicker.AutoReset = false;
				_timerTicker.Elapsed += OnTimer;
			} else {
				_timerTicker.Stop ();
			}

			_timerTrade.Start ();
			_timerOrderbook.Start ();
			_timerTicker.Start ();
		}

		private	void StopTimer()
		{
			if (_timerTrade != null) {
				_timerTrade.Elapsed -= OnTimer;
				_timerTrade.Stop ();
			}

			if (_timerOrderbook != null) {
				_timerOrderbook.Elapsed -= OnTimer;
				_timerOrderbook.Stop ();
			}

			if (_timerTicker != null) {
				_timerTicker.Elapsed -= OnTimer;
				_timerTicker.Stop ();
			}

			_timerTrade = null;
			_timerOrderbook = null;
			_timerTicker = null;
		}

		private	void OnTimer(object sender, ElapsedEventArgs e)
		{
			ConsoleIO.Error ("heart beat missing.. ({0} )", e.SignalTime.ToString("F"));
			Fire (TRIGGER_STOP);
		}

        private void GetTradeByWeb(int timestamp)
		{
			try {
				var trades = _api.GetTrades (_symbol, timestamp);
				if (trades == null)
					return;
				if (!trades.IsArray)
					return;

				var count = trades.Count;
				for (int i = 0; i < count; i++) {
					var trade = trades [i];
					var tid = int.Parse (trade ["tid"].ToString ());
					var price = double.Parse (trade ["price"].ToString ());
					var amount = double.Parse (trade ["amount"].ToString ());
					var ts = int.Parse (trade ["timestamp"].ToString ());

					_market.ReserveTrade (tid, price, amount, ts);
					ExecuteQuery (string.Format ("INSERT OR REPLACE INTO {0} VALUES({1}, {2}, {3}, {4});", name, tid, price, amount, ts));
				}
				_market.FlushTrade ();
				_market.UpdateChart ();
			} catch (Exception e) {
				ConsoleIO.Error (e.Message);
				Fire (TRIGGER_STOP);
			}
		}

        private void OnSocketError(string message)
        {
            Fire(TRIGGER_STOP);
        }

        private void OnSubscribedTrade(JsonData json)
        {
            try
            {
                var len = json.Count;
				if (len == 2) {
					RestartTimer(_timerTrade);
                    return;
				}

                var jsonId = json[len - 4];
                var valueId = jsonId.ToString().Trim();
                int id = -1;
                if (jsonId.IsInt)
                    id = int.Parse(valueId);
                else if (jsonId.IsString)
                    return;
                if (id < 0)
                    return;

				var price = float.Parse(json[len - 2].ToString());
				var amount = float.Parse(json[len - 1].ToString());
				var ts = int.Parse(json[len - 3].ToString());

				_market.AddTrade(id, price, amount, ts);
				ExecuteQuery (string.Format("INSERT OR REPLACE INTO {0} VALUES({1}, {2}, {3}, {4});", name, id, price, amount, ts));
                _market.UpdateChart();
            }
            catch (Exception e)
            {
                ConsoleIO.Error(e.Message);
                Fire(TRIGGER_STOP);
            }
        }

        private void OnSubscribeOrderBook(JsonData json)
        {
			var len = json.Count;
			if (len == 2) {
				RestartTimer(_timerOrderbook);
				return;
			}
        }

        private void OnSubscribeTicker(JsonData json)
        {
            try
            {
				var len = json.Count;
				if (len == 2) {
					RestartTimer(_timerTicker);
					return;
				}

                _market.ticker.Update(
                    float.Parse(json[1].ToString()),
                    float.Parse(json[2].ToString()),
                    float.Parse(json[3].ToString()),
                    float.Parse(json[4].ToString()),
                    float.Parse(json[8].ToString())
                );
            }
            catch (Exception e)
            {
                ConsoleIO.Error(e.Message);
                Fire(TRIGGER_STOP);
            }
        }
    }
}


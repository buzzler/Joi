using System;
using Joi.FSM;
using Joi.Data;
using Joi.Data.Chart;

namespace Joi.Brain
{
    public class TradeLogic : StateMachine
    {
        private const string STATE_INIT = "Init";
        private const string STATE_BALANCE = "Balance";
		private const string STATE_AGING = "Aging";
		private const string STATE_LONG_BUY = "LongTermBuy";
		private	const string STATE_SHORT_BUY = "ShortTermBuy";
		private const string STATE_LONG_SELL = "LongTermSell";
		private	const string STATE_SHORT_SELL = "ShortTermSell";
		private	const string STATE_IDLE = "Idle";
		private	const string STATE_BUYING = "Buying";
		private	const string STATE_SELLING = "Selling";

        private const string TRIGGER_COMPLETE = "complete";
		private const string TRIGGER_ERROR = "error";
		private	const string TRIGGER_LONGTERM = "longterm";
		private	const string TRIGGER_SHORTTERM = "shortterm";
		private	const string TRIGGER_BUY = "buy";
		private	const string TRIGGER_SELL = "sell";

		private	const TimeInterval _interval = TimeInterval.MINUTE_15;
		private CrawlerCoinone _kr;
		private	Indicator _indicator;
		private	OrderBook _orderbook;
		private	double _bought;	
		private double _earningrate;
		private	double _earning;
		private	bool _enableBuy;
		private bool _enableSell;
		private	bool _simulation;

		public	bool simulation { get { return _simulation; } }

		public TradeLogic(Symbol symbol, bool logging = true, bool simulation = true) : base("TradeLogic", 100, logging)
		{
			_simulation = simulation;
			_bought = 0;
			_earningrate = 0;
			_earning = 0;
			AnyState ()
				.ConnectTo (TRIGGER_ERROR, STATE_IDLE);
			SetFirstState (STATE_INIT)
				.SetupLoop (OnInitLoop)
				.ConnectTo (TRIGGER_COMPLETE, STATE_BALANCE);
			SetState (STATE_BALANCE)
				.SetupEntry (OnBalanceEntry)
				.ConnectTo (TRIGGER_COMPLETE, STATE_AGING);
			SetState (STATE_AGING)
				.SetupEntry (OnAgingEntry)
				.SetupLoop (OnAgingLoop)
				.ConnectTo (TRIGGER_BUY, STATE_LONG_BUY)
				.ConnectTo (TRIGGER_SELL, STATE_LONG_SELL);
			SetState (STATE_LONG_BUY)
				.SetupEntry (OnLongBuyEntry)
				.SetupLoop (OnLongBuyLoop)
				.ConnectTo (TRIGGER_BUY, STATE_BUYING)
				.ConnectTo (TRIGGER_SHORTTERM, STATE_SHORT_BUY);
			SetState (STATE_SHORT_BUY)
				.SetupEntry (OnShortBuyEntry)
				.SetupLoop (OnShortBuyLoop)
				.ConnectTo (TRIGGER_BUY, STATE_BUYING)
				.ConnectTo (TRIGGER_LONGTERM, STATE_LONG_BUY);
			SetState (STATE_LONG_SELL)
				.SetupEntry (OnLongSellEntry)
				.SetupLoop (OnLongSellLoop)
				.ConnectTo (TRIGGER_SELL, STATE_SELLING)
				.ConnectTo (TRIGGER_SHORTTERM, STATE_SHORT_SELL);
			SetState (STATE_SHORT_SELL)
				.SetupEntry (OnShortSellEntry)
				.SetupLoop (OnShortSellLoop)
				.ConnectTo (TRIGGER_SELL, STATE_SELLING)
				.ConnectTo (TRIGGER_LONGTERM, STATE_LONG_SELL);
			SetState (STATE_IDLE)
				.SetupEntry (OnIdleEntry)
				.SetupLoop (OnIdleLoop)
				.ConnectTo (TRIGGER_COMPLETE, STATE_INIT);
			SetState (STATE_BUYING)
				.SetupEntry (OnBuyingEntry)
				.SetupLoop (OnBuyingLoop)
				.SetupExit (OnBuyingExit)
				.ConnectTo (TRIGGER_BUY, STATE_LONG_SELL)
				.ConnectTo (TRIGGER_COMPLETE, STATE_BALANCE);
			SetState (STATE_SELLING)
				.SetupEntry (OnSellingEntry)
				.SetupLoop (OnSellingLoop)
				.SetupExit (OnSellingExit)
				.ConnectTo (TRIGGER_SELL, STATE_LONG_BUY)
				.ConnectTo (TRIGGER_COMPLETE, STATE_BALANCE);
			Start ();
		}

        ~TradeLogic()
        {
            End();
        }

		#region public interface

		public	bool EnableBuying { get { return _enableBuy; } }

		public	bool EnableSelling { get { return _enableSell; } }

		public	void StartBuying()
		{
			_enableBuy = true;
		}

		public	void StopBuying()
		{
			_enableBuy = false;
		}

		public	void StartSell()
		{
			_enableSell = true;
		}

		public	void StopSell()
		{
			_enableBuy = false;
		}

		#endregion

		#region Init state
		private void OnInitLoop()
		{
			if (stateMachines.ContainsKey (CrawlerLogic.COINONE)) {
				_kr = (CrawlerCoinone)stateMachines [CrawlerLogic.COINONE];
				_indicator = _kr.market.GetIndicator (_interval);
				_orderbook = _kr.market.orderbook;
				_bought = 0;
				_earningrate = 0;
				_earning = 0;
				_enableBuy = true;
				_enableSell = true;
				Fire (TRIGGER_COMPLETE);
			}
		}
		#endregion

		#region balance state
		private void OnBalanceEntry()
		{
			_kr.GetBalanceAsync (() => {
				Fire (TRIGGER_COMPLETE);
			});
		}
		#endregion

		#region Aging state
		private void OnAgingEntry()
		{
		}

		private void OnAgingLoop()
		{
			if (simulation) {
				Fire (TRIGGER_BUY);
			} else {
				var balance = _kr.market.balance;
//				var krw = balance.GetAvailable (Symbol.KR_WON);
				var btc = balance.GetAvailable (Symbol.BITCOIN);

				if (btc > 0.0001)
					Fire (TRIGGER_SELL);
				else
					Fire (TRIGGER_BUY);
			}
		}
		#endregion

		#region Long-Term Buy state
		private void OnLongBuyEntry()
		{
		}

		private	void OnLongBuyLoop()
		{
//			if (_indicator.lastOscillator.decreasing) {
//				Fire (TRIGGER_SHORTTERM);
//				return;
//			}

			if (_enableBuy) {
//				if (_indicator.lastBollingerBand.crossingBelow || _indicator.lastBollingerBand.deviationRatio <= -1)
//					Fire (TRIGGER_BUY);

				if (_indicator.lastBollingerBand.deviationRatio <= -0.9 ||
					(_indicator.lastCandle.low < _indicator.lastBollingerBand.lowband && _indicator.lastCandle.close > _indicator.lastBollingerBand.lowband))
					Fire (TRIGGER_BUY);
			}
		}
		#endregion

		#region Shot-Term Buy state
		private void OnShortBuyEntry()
		{
		}

		private void OnShortBuyLoop()
		{
			if (_indicator.lastOscillator.increasing) {
				Fire (TRIGGER_LONGTERM);
				return;
			}
		}
		#endregion

		#region Long-Term Sell state
		private	void OnLongSellEntry()
		{
		}

		private	void OnLongSellLoop()
		{
//			if (_indicator.lastOscillator.decreasing) {
//				Fire (TRIGGER_SHORTTERM);
//				return;
//			}

			if (_enableSell) {
//				if (_indicator.lastBollingerBand.crossingAbove || _indicator.lastBollingerBand.deviationRatio >= 1)
//					Fire (TRIGGER_SELL);

				if (_indicator.lastBollingerBand.deviationRatio >= 0.9 || 
					(_indicator.lastCandle.high > _indicator.lastBollingerBand.highband && _indicator.lastCandle.close < _indicator.lastBollingerBand.highband))
					Fire (TRIGGER_SELL);
			}
		}
		#endregion

		#region Short-Term Sell state
		private	void OnShortSellEntry()
		{
		}

		private void OnShortSellLoop()
		{
			if (_indicator.lastOscillator.increasing) {
				Fire (TRIGGER_LONGTERM);
				return;
			}
		}

		#endregion

		#region Buying state
		private	void OnBuyingEntry()
		{
			_kr.SetSpeed (3f);
			_kr.GatherOrderBook ();
		}

		private void OnBuyingLoop()
		{
			var now = Utility.Timestamp (DateTime.UtcNow);
			var past = now - _orderbook.timestamp;
			if (past > 10)
				return;

			var price = _orderbook.GetLowestAsk();
			var amount = CalculateBuyingAmount (price);
			if (!simulation) {
				if (amount > 0)
					_kr.api.LimitBuy ((long)price, amount, _kr.currency);
				else {
					Fire (TRIGGER_ERROR);
					return;
				}
			}

			LogBuy ("LONG", price, amount);
			Sleep ((int)_interval * 500);

			if (simulation)
				Fire (TRIGGER_BUY);
			else
				Fire (TRIGGER_COMPLETE);
		}

		private	void OnBuyingExit()
		{
			_kr.GatherTrade ();
			_kr.SetSpeed (1f);
		}
		#endregion

		#region Selling state
		private	void OnSellingEntry()
		{
			_kr.SetSpeed (3f);
			_kr.GatherOrderBook ();
		}

		private	void OnSellingLoop()
		{
			var now = Utility.Timestamp (DateTime.UtcNow);
			var past = now - _orderbook.timestamp;
			if (past > 10)
				return;

			var price = _orderbook.GetHighestBid();
			var amount = CalculateSellingAmount ();
			if (!simulation) {
				if (amount > 0)
					_kr.api.LimitSell ((long)price, amount, _kr.currency);
				else {
					Fire (TRIGGER_ERROR);
					return;
				}
			}

			LogSell ("LONG", price, amount);
			Sleep ((int)_interval * 500);

			if (simulation)
				Fire (TRIGGER_SELL);
			else
				Fire (TRIGGER_COMPLETE);
		}

		private	void OnSellingExit()
		{
			_kr.GatherTrade ();
			_kr.SetSpeed (1f);
		}
		#endregion

		#region Idle state
		private void OnIdleEntry()
		{
		}

		private void OnIdleLoop()
		{
			Fire (TRIGGER_COMPLETE);
		}
		#endregion

		private	void LogBuy(string term, double price, double amount)
		{
			_bought = price;
			var benefit = -(_bought * _kr.buyingFee);
			_earning += benefit;
			_earningrate += -_kr.buyingFee;
			ConsoleIO.LogLine ("{0}-term buy: {1} won ({2})", term, price.ToString ("N"), DateTime.Now.ToString ("T"));
			ConsoleIO.LogLine ("{0}-term amount: {1}", term, amount);
			ConsoleIO.LogLine ("{0}-term earning: {1} won (total {2} won)", term, benefit.ToString ("N"), _earning.ToString ("N"));
			ConsoleIO.LogLine ("{0}-term earning rate: -{1} % (total {2} %)", term, _kr.buyingFee.ToString ("##.000"), _earningrate.ToString ("##.000"));
			ConsoleIO.LogLine ("bollingerband: high {0}, low {1}, ratio {2}", 
				_indicator.lastBollingerBand.highband.ToString ("##.000"),
				_indicator.lastBollingerBand.lowband.ToString ("##.000"),
				_indicator.lastBollingerBand.deviationRatio
			);
		}

		private	void LogSell(string term, double price, double amount)
		{
			var benefit = CalculateEarning (price);
			var rate = CalculateEarningRate (price);
			_earning += benefit;
			_earningrate += rate;
			ConsoleIO.LogLine ("{0}-term sell: {1} won ({2})", term, price.ToString ("N"), DateTime.Now.ToString ("T"));
			ConsoleIO.LogLine ("{0}-term amount: {1}", term, amount);
			ConsoleIO.LogLine ("{0}-term earning: {1} won (total {2} won)", term, benefit, _earning.ToString("N"));
			ConsoleIO.LogLine ("{0}-term earning rate: {1} % (total {2} %)", term, rate.ToString("##.000"), _earningrate.ToString ("##.000"));
			ConsoleIO.LogLine ("bollingerband: high {0}, low {1}, ratio {2}", 
				_indicator.lastBollingerBand.highband.ToString ("##.000"),
				_indicator.lastBollingerBand.lowband.ToString ("##.000"),
				_indicator.lastBollingerBand.deviationRatio
			);
		}

		private double CalculateEarning(double price)
		{
			if (_bought == 0)
				return 0;

			var gap = price - _bought;
			return gap - (price * _kr.sellingFee);
		}

		private	double CalculateEarningRate(double price)
		{
			if (_bought == 0)
				return 0;
			var gap = price - _bought;
			var rate = gap / _bought;
			return rate - _kr.sellingFee;
		}

		private	double CalculateSellingAmount()
		{
			var btc = _kr.market.balance.GetAvailable (Symbol.BITCOIN);
			return double.Parse (btc.ToString ("##.0000"));
		}

		private	double CalculateBuyingAmount(double price)
		{
			if (price == 0)
				return 0;

			var krw = _kr.market.balance.GetAvailable (Symbol.KR_WON);
			var amount = krw / price;
			return double.Parse (amount.ToString ("##.0000"));
		}
    }
}


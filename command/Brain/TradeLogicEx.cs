using System;
using Joi.FSM;
using Joi.Data;
using Joi.Data.Chart;

namespace Joi.Brain
{
    public class TradeLogicEx : StateMachine
    {
        private const string STATE_INIT = "Init";
        private const string STATE_BALANCE = "Balance";
		private const string STATE_AGING = "Aging";
		private const string STATE_LONG_BUY = "LongTermBuy";
		private const string STATE_MID_BUY = "MidTermBuy";
		private	const string STATE_SHORT_BUY = "ShortTermBuy";
		private const string STATE_LONG_SELL = "LongTermSell";
		private const string STATE_MID_SELL = "MidTermSell";
		private	const string STATE_SHORT_SELL = "ShortTermSell";
		private	const string STATE_IDLE = "Idle";

        private const string TRIGGER_COMPLETE = "complete";
		private const string TRIGGER_ERROR = "error";
		private	const string TRIGGER_LONGTERM = "longterm";
		private	const string TRIGGER_MIDTERM = "midterm";
		private	const string TRIGGER_SHORTTERM = "shortterm";
		private	const string TRIGGER_BUY = "buy";
		private	const string TRIGGER_SELL = "sell";

//        private Symbol _symbol;
		private CrawlerCoinone _kr;
		private	Indicator _longest;
		private	Indicator _longterm;
		private	Indicator _midterm;
		private Indicator _shorterm;
		private Indicator _shortest;
		private	double _bought;
		private double _earningrate;
		private	double _earning;

		public TradeLogicEx(Symbol symbol, bool logging = true) : base("TradeLogicEx", 100, logging)
		{
//			_symbol = symbol;
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
				.SetupEntry(OnLongBuyEntry)
				.SetupLoop (OnLongBuyLoop)
				.ConnectTo (TRIGGER_BUY, STATE_LONG_SELL)
				.ConnectTo (TRIGGER_SELL, STATE_LONG_SELL)
				.ConnectTo (TRIGGER_MIDTERM, STATE_MID_BUY)
				.ConnectTo (TRIGGER_SHORTTERM, STATE_SHORT_BUY);
			SetState (STATE_MID_BUY)
				.SetupEntry(OnMidBuyEntry)
				.SetupLoop (OnMidBuyLoop)
				.ConnectTo (TRIGGER_BUY, STATE_MID_SELL)
				.ConnectTo (TRIGGER_SELL, STATE_MID_SELL)
				.ConnectTo (TRIGGER_LONGTERM, STATE_LONG_BUY)
				.ConnectTo (TRIGGER_SHORTTERM, STATE_SHORT_BUY);
			SetState (STATE_SHORT_BUY)
				.SetupEntry(OnShortBuyEntry)
				.SetupLoop (OnShortBuyLoop)
				.ConnectTo (TRIGGER_BUY, STATE_SHORT_SELL)
				.ConnectTo (TRIGGER_SELL, STATE_SHORT_SELL)
				.ConnectTo (TRIGGER_LONGTERM, STATE_LONG_BUY)
				.ConnectTo (TRIGGER_MIDTERM, STATE_MID_BUY);
			SetState (STATE_LONG_SELL)
				.SetupEntry(OnLongSellEntry)
				.SetupLoop (OnLongSellLoop)
				.ConnectTo (TRIGGER_BUY, STATE_LONG_BUY)
				.ConnectTo (TRIGGER_SELL, STATE_LONG_BUY)
				.ConnectTo (TRIGGER_MIDTERM, STATE_MID_SELL)
				.ConnectTo (TRIGGER_SHORTTERM, STATE_SHORT_SELL);
			SetState (STATE_MID_SELL)
				.SetupEntry(OnMidSellEntry)
				.SetupLoop (OnMidSellLoop)
				.ConnectTo (TRIGGER_BUY, STATE_MID_BUY)
				.ConnectTo (TRIGGER_SELL, STATE_MID_BUY)
				.ConnectTo (TRIGGER_LONGTERM, STATE_LONG_SELL)
				.ConnectTo (TRIGGER_SHORTTERM, STATE_SHORT_SELL);
			SetState (STATE_SHORT_SELL)
				.SetupEntry(OnShortSellEntry)
				.SetupLoop (OnShortSellLoop)
				.ConnectTo (TRIGGER_BUY, STATE_SHORT_BUY)
				.ConnectTo (TRIGGER_SELL, STATE_SHORT_BUY)
				.ConnectTo (TRIGGER_LONGTERM, STATE_LONG_SELL)
				.ConnectTo (TRIGGER_MIDTERM, STATE_MID_SELL);
			SetState (STATE_IDLE)
				.SetupEntry (OnIdleEntry)
				.SetupLoop (OnIdleLoop)
				.ConnectTo (TRIGGER_COMPLETE, STATE_INIT);
			Start ();
		}

        ~TradeLogicEx()
        {
            End();
        }

		#region Init state
		private void OnInitLoop()
		{
			if (stateMachines.ContainsKey (CrawlerLogic.COINONE)) {
				_kr = (CrawlerCoinone)stateMachines [CrawlerLogic.COINONE];
				_shortest = _kr.market.GetIndicator (TimeInterval.MINUTE_1);
				_shorterm = _kr.market.GetIndicator (TimeInterval.MINUTE_3);
				_midterm = _kr.market.GetIndicator (TimeInterval.MINUTE_5);
				_longterm = _kr.market.GetIndicator (TimeInterval.MINUTE_15);
				_longest = _kr.market.GetIndicator (TimeInterval.HOUR_1);
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
			double btc = _kr.market.balance.GetAvailable (Symbol.BITCOIN);
			double krw = _kr.market.balance.GetAvailable (Symbol.KR_WON);

			if (btc > 0) {
				Fire (TRIGGER_SELL);
			} else if (krw > 0) {
				Fire (TRIGGER_BUY);
			} else {
				Fire (TRIGGER_ERROR);
			}
		}
		#endregion

		#region Long-Term Buy state
		private void OnLongBuyEntry()
		{
		}

		private	void OnLongBuyLoop()
		{
			if (_longterm.lastOscillator.decreasing) {
				Fire (TRIGGER_MIDTERM);
				return;
			}

			/**
			 * long-term buying method
			 */
			if (_longterm.lastCandle.increasing && 
				_midterm.lastOscillator.increasing && _midterm.lastCandle.increasing) {
				LogBuy ("long", _longterm.lastCandle.close);
			}
		}
		#endregion

		#region Mid-Term Buy state
		private	void OnMidBuyEntry()
		{
		}

		private void OnMidBuyLoop()
		{
			if (_longterm.lastOscillator.increasing) {
				Fire (TRIGGER_LONGTERM);
				return;
			} else if (_midterm.lastOscillator.decreasing) {
				Fire (TRIGGER_SHORTTERM);
				return;
			}

			/**
			 * mid-term buying method
			 */
			if (_midterm.lastCandle.increasing && 
				_shorterm.lastOscillator.increasing && _shorterm.lastCandle.increasing) {
				LogBuy ("mid", _midterm.lastCandle.close);
			}
		}
		#endregion

		#region Shot-Term Buy state
		private void OnShortBuyEntry()
		{
		}

		private void OnShortBuyLoop()
		{
			if (_longterm.lastOscillator.increasing) {
				Fire (TRIGGER_LONGTERM);
				return;
			} else if (_midterm.lastOscillator.increasing) {
				Fire (TRIGGER_MIDTERM);
				return;
			}

			/**
			 * short-term buying method
			 */
			if (_shorterm.lastOscillator.decreasing && _shorterm.lastCandle.increasing && 
				_shortest.lastOscillator.increasing && _shortest.lastCandle.increasing) {
				LogBuy ("short", _shorterm.lastCandle.close);
			}
		}
		#endregion

		#region Long-Term Sell state
		private	void OnLongSellEntry()
		{
		}

		private	void OnLongSellLoop()
		{
			if (_longterm.lastOscillator.decreasing) {
				Fire (TRIGGER_MIDTERM);
				return;
			}

			/**
			 * long-term sell method
			 */
			if (_longterm.lastCandle.decreasing &&
			    _midterm.lastOscillator.decreasing && _midterm.lastCandle.decreasing)
				LogSell ("long", _longterm.lastCandle.close);
		}
		#endregion

		#region Mid-Term Sell state
		private void OnMidSellEntry()
		{
		}

		private void OnMidSellLoop()
		{
			if (_longterm.lastOscillator.increasing) {
				Fire (TRIGGER_LONGTERM);
				return;
			} else if (_midterm.lastOscillator.decreasing) {
				Fire (TRIGGER_SHORTTERM);
				return;
			}

			/**
			 * mid-term sell method
			 */
			if (_midterm.lastCandle.decreasing && 
				_shorterm.lastOscillator.decreasing && _shorterm.lastCandle.decreasing)
				LogSell ("mid", _midterm.lastCandle.close);
		}
		#endregion

		#region Shot-Term Sell state
		private	void OnShortSellEntry()
		{
		}

		private void OnShortSellLoop()
		{
			if (_longterm.lastOscillator.increasing) {
				Fire (TRIGGER_LONGTERM);
				return;
			} else if (_midterm.lastOscillator.increasing) {
				Fire (TRIGGER_MIDTERM);
				return;
			}

			/**
			 * short-term sell method
			 */
			if (_shorterm.lastCandle.decreasing)
				LogSell ("short", _shorterm.lastCandle.close);
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

		private	void LogBuy(string term, double price)
		{
			_bought = price;
			_earning -= _bought * _kr.buyingFee;
			_earningrate -= _kr.buyingFee;
			ConsoleIO.LogLine ("{0}-term buy: {1} won ({2})", term, price.ToString ("N"), DateTime.Now.ToString ("T"));
			ConsoleIO.LogLine ("{0}-term earning: {1} won", term, _earning.ToString("N"));
			ConsoleIO.LogLine ("{0}-term earning rate: {1} %", term, _earningrate.ToString ("##.000"));
			Fire (TRIGGER_BUY);
		}

		private	void LogSell(string term, double price)
		{
			var gap = price - _bought;
			var rate = gap / _bought;
			_earning += gap - (price * _kr.sellingFee);
			_earningrate += rate - _kr.sellingFee;
			ConsoleIO.LogLine ("{0}-term sell: {1} won ({2})", term, price.ToString ("N"), DateTime.Now.ToString ("T"));
			ConsoleIO.LogLine ("{0}-term earning: {1} won", term, _earning.ToString("N"));
			ConsoleIO.LogLine ("{0}-term earning rate: {1} %", term, _earningrate.ToString ("##.000"));
			Fire (TRIGGER_SELL);
		}
    }
}


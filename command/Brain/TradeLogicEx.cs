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
        
        private const string TRIGGER_COMPLETE = "complete";
		private const string TRIGGER_ERROR = "error";
		private	const string TRIGGER_LONGTERM = "longterm";
		private	const string TRIGGER_MIDTERM = "midterm";
		private	const string TRIGGER_SHORTTERM = "shortterm";
		private	const string TRIGGER_BUY = "buy";
		private	const string TRIGGER_SELL = "sell";

        private Symbol _symbol;
		private CrawlerCoinone _kr;
		private	Indicator _longest;
		private	Indicator _longterm;
		private	Indicator _midterm;
		private Indicator _shorterm;
		private Indicator _shortest;

		public TradeLogicEx(Symbol symbol, bool logging = true) : base("TradeLogicEx", 100, logging)
		{
			_symbol = symbol;

			SetFirstState (STATE_INIT)
				.SetupLoop (OnInitLoop)
				.ConnectTo (TRIGGER_COMPLETE, STATE_BALANCE);
			SetState (STATE_BALANCE)
				.SetupEntry (OnBalanceEntry)
				.ConnectTo (TRIGGER_COMPLETE, STATE_AGING);
			SetState (STATE_AGING)
				.SetupEntry (OnAgingEntry)
				.ConnectTo (TRIGGER_LONGTERM, STATE_LONG_BUY)
				.ConnectTo (TRIGGER_MIDTERM, STATE_MID_BUY)
				.ConnectTo (TRIGGER_SHORTTERM, STATE_SHORT_BUY)
				.ConnectTo (TRIGGER_ERROR, STATE_BALANCE);
			SetState (STATE_LONG_BUY)
				.SetupLoop (OnLongBuyLoop)
				.ConnectTo (TRIGGER_BUY, STATE_LONG_SELL)
				.ConnectTo (TRIGGER_SELL, STATE_LONG_SELL)
				.ConnectTo (TRIGGER_MIDTERM, STATE_MID_BUY)
				.ConnectTo (TRIGGER_SHORTTERM, STATE_SHORT_BUY);
			SetState (STATE_MID_BUY)
				.SetupLoop (OnMidBuyLoop)
				.ConnectTo (TRIGGER_BUY, STATE_MID_SELL)
				.ConnectTo (TRIGGER_SELL, STATE_MID_SELL)
				.ConnectTo (TRIGGER_LONGTERM, STATE_LONG_BUY)
				.ConnectTo (TRIGGER_SHORTTERM, STATE_SHORT_BUY);
			SetState (STATE_SHORT_BUY)
				.SetupLoop (OnShortBuyLoop)
				.ConnectTo (TRIGGER_BUY, STATE_SHORT_SELL)
				.ConnectTo (TRIGGER_SELL, STATE_SHORT_SELL)
				.ConnectTo (TRIGGER_LONGTERM, STATE_LONG_BUY)
				.ConnectTo (TRIGGER_MIDTERM, STATE_MID_BUY);
			SetState (STATE_LONG_SELL)
				.SetupLoop (OnLongSellLoop)
				.ConnectTo (TRIGGER_BUY, STATE_LONG_BUY)
				.ConnectTo (TRIGGER_SELL, STATE_LONG_BUY)
				.ConnectTo (TRIGGER_MIDTERM, STATE_MID_SELL)
				.ConnectTo (TRIGGER_SHORTTERM, STATE_SHORT_SELL);
			SetState (STATE_MID_SELL)
				.SetupLoop (OnMidSellLoop)
				.ConnectTo (TRIGGER_BUY, STATE_MID_BUY)
				.ConnectTo (TRIGGER_SELL, STATE_MID_BUY)
				.ConnectTo (TRIGGER_LONGTERM, STATE_LONG_SELL)
				.ConnectTo (TRIGGER_SHORTTERM, STATE_SHORT_SELL);
			SetState (STATE_SHORT_SELL)
				.SetupLoop (OnShortSellLoop)
				.ConnectTo (TRIGGER_BUY, STATE_SHORT_BUY)
				.ConnectTo (TRIGGER_SELL, STATE_SHORT_BUY)
				.ConnectTo (TRIGGER_LONGTERM, STATE_LONG_SELL)
				.ConnectTo (TRIGGER_MIDTERM, STATE_MID_SELL);
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
			double btc = _kr.market.balance.GetAvailable (Symbol.BITCOIN);
			double krw = _kr.market.balance.GetAvailable (Symbol.KR_WON);

			if (btc > 0) {
				Fire (TRIGGER_LONGTERM);
				Fire (TRIGGER_SELL);
			} else if (krw > 0) {
				Fire (TRIGGER_LONGTERM);
				Fire (TRIGGER_BUY);
			} else {
				Fire (TRIGGER_ERROR);
			}
		}
		#endregion

		#region Long-Term Buy state
		private	void OnLongBuyLoop()
		{
			if (_longterm.lastOscillator.decreasing) {
				Fire (TRIGGER_MIDTERM);
				return;
			}

			/**
			 * long-term buying method
			 */
			Fire (TRIGGER_BUY);
		}
		#endregion

		#region Mid-Term Buy state
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
			Fire (TRIGGER_BUY);
		}
		#endregion

		#region Shot-Term Buy state
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
			if (_shorterm.lastOscillator.increasing)
				Fire (TRIGGER_BUY);
		}
		#endregion

		#region Long-Term Sell state
		private	void OnLongSellLoop()
		{
		}
		#endregion

		#region Mid-Term Sell state
		private void OnMidSellLoop()
		{
		}
		#endregion

		#region Shot-Term Sell state
		private void OnShortSellLoop()
		{
		}
		#endregion
    }
}


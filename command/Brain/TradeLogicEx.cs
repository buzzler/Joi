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
		private	Indicator _longterm;
		private	Indicator _midterm;
		private Indicator _shorterm;

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
				.ConnectTo (TRIGGER_SHORTTERM, STATE_SHORT_BUY);
			SetState (STATE_LONG_BUY)
				.SetupLoop (OnLongBuyLoop)
				.ConnectTo (TRIGGER_SELL, STATE_LONG_SELL)
				.ConnectTo (TRIGGER_MIDTERM, STATE_MID_BUY)
				.ConnectTo (TRIGGER_SHORTTERM, STATE_SHORT_BUY);
			SetState (STATE_MID_BUY)
				.SetupLoop (OnMidBuyLoop)
				.ConnectTo (TRIGGER_SELL, STATE_MID_SELL)
				.ConnectTo (TRIGGER_LONGTERM, STATE_LONG_BUY)
				.ConnectTo (TRIGGER_SHORTTERM, STATE_SHORT_BUY);
			SetState (STATE_SHORT_BUY)
				.SetupLoop (OnShortBuyLoop)
				.ConnectTo (TRIGGER_SELL, STATE_SHORT_SELL)
				.ConnectTo (TRIGGER_LONGTERM, STATE_LONG_BUY)
				.ConnectTo (TRIGGER_MIDTERM, STATE_MID_BUY);
			SetState (STATE_LONG_SELL)
				.SetupLoop (OnLongSellLoop)
				.ConnectTo (TRIGGER_BUY, STATE_LONG_BUY)
				.ConnectTo (TRIGGER_MIDTERM, STATE_MID_SELL)
				.ConnectTo (TRIGGER_SHORTTERM, STATE_SHORT_SELL);
			SetState (STATE_MID_SELL)
				.SetupLoop (OnMidSellLoop)
				.ConnectTo (TRIGGER_BUY, STATE_MID_BUY)
				.ConnectTo (TRIGGER_LONGTERM, STATE_LONG_SELL)
				.ConnectTo (TRIGGER_SHORTTERM, STATE_SHORT_SELL);
			SetState (STATE_SHORT_SELL)
				.SetupLoop (OnShortSellLoop)
				.ConnectTo (TRIGGER_BUY, STATE_SHORT_BUY)
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
		#endregion

		#region Long-Term Buy state
		private	void OnLongBuyLoop()
		{
		}
		#endregion

		#region Mid-Term Buy state
		private void OnMidBuyLoop()
		{
		}
		#endregion

		#region Shot-Term Buy state
		private void OnShortBuyLoop()
		{
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


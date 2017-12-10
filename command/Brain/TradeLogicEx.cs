using System;
using Joi.FSM;
using Joi.Data;

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

        private Symbol _symbol;
		private CrawlerCoinone _kr;

		public TradeLogicEx(Symbol symbol, bool logging = true) : base("TradeLogicEx", 100, logging)
		{
			_symbol = symbol;

			SetFirstState (STATE_INIT)
				.SetupLoop (OnInitLoop)
				.ConnectTo (TRIGGER_COMPLETE, STATE_BALANCE);
			SetState (STATE_BALANCE)
				.SetupEntry (OnInitBalance)
				.ConnectTo (TRIGGER_COMPLETE, STATE_AGING);
			Start ();
		}

        ~TradeLogicEx()
        {
            End();
        }

		private void OnInitLoop()
		{
			if (stateMachines.ContainsKey (CrawlerLogic.COINONE)) {
				_kr = (CrawlerCoinone)stateMachines [CrawlerLogic.COINONE];
				Fire (TRIGGER_COMPLETE);
			}
		}

		private void OnInitBalance()
		{
			_kr.GetBalanceAsync (() => {
				Fire (TRIGGER_COMPLETE);
			});
		}
    }
}


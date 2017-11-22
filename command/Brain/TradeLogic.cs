using System;
using Joi.FSM;
using Joi.Data;

namespace Joi.Brain
{
	public class TradeLogic : StateMachine
	{
		private	const string STATE_BALANCE		= "Balance";
		private	const string STATE_HISTORY		= "History";
		private	const string STATE_READY2BUY	= "Ready2Buy";
		private	const string STATE_READY2SELL	= "Ready2Sell";
		private	const string STATE_BUYING		= "Buying";
		private	const string STATE_SELLING		= "Selling";
		private	const string STATE_IDLE			= "Idle";

		private	const string TRIGGER_NEED_SELL	= "needSell";
		private	const string TRIGGER_NEED_BUY	= "needBuy";
		private	const string TRIGGER_COMPLETE	= "complete";
		private	const string TRIGGER_ERROR		= "error";

		private	Symbol _symbol;

		public TradeLogic (Symbol symbol, bool logging = true) : base ("TradeLogic", 100, logging)
		{
			_symbol = symbol;
			SetFirstState (STATE_BALANCE)
				.SetupEntry (OnEntryBalance)
				.SetupExit (OnExitBalance)
				.SetupLoop (OnLoopBalance)
				.ConnectTo (TRIGGER_COMPLETE, STATE_HISTORY);
			AnyState ()
				.ConnectTo (TRIGGER_ERROR, STATE_IDLE);
			SetState (STATE_HISTORY)
				.SetupEntry (OnEntryHistory)
				.SetupExit (OnExitHistory)
				.SetupLoop (OnLoopHistory)
				.ConnectTo (TRIGGER_NEED_BUY, STATE_READY2BUY)
				.ConnectTo (TRIGGER_NEED_SELL, STATE_READY2SELL);
			SetState (STATE_READY2BUY)
				.SetupEntry (OnEntryR2B)
				.SetupExit (OnExitR2B)
				.SetupLoop (OnLoopR2B)
				.ConnectTo (TRIGGER_COMPLETE, STATE_BUYING);
			SetState (STATE_BUYING)
				.SetupEntry (OnEntryBuy)
				.SetupExit (OnExitBuy)
				.SetupLoop (OnLoopBuy)
				.ConnectTo (TRIGGER_COMPLETE, STATE_BALANCE);
			SetState (STATE_READY2SELL)
				.SetupEntry (OnEntryR2S)
				.SetupExit (OnExitR2S)
				.SetupLoop (OnLoopR2S)
				.ConnectTo (TRIGGER_COMPLETE, STATE_SELLING);
			SetState (STATE_SELLING)
				.SetupEntry (OnEntrySell)
				.SetupExit (OnExitSell)
				.SetupLoop (OnLoopSell)
				.ConnectTo (TRIGGER_COMPLETE, STATE_BALANCE);
			SetState (STATE_IDLE)
				.SetupEntry (OnEntryIdle)
				.SetupExit (OnExitIdle)
				.SetupLoop (OnLoopIdle)
				.ConnectTo (TRIGGER_COMPLETE, STATE_BALANCE);
			Start ();
		}

		~TradeLogic ()
		{
			End ();
		}

		#region 'Getting Balance' state

		private	void OnEntryBalance ()
		{
		}

		private	void OnLoopBalance ()
		{
			var cl = stateMachines [CrawlerLogic.COINONE] as CrawlerCoinone;
			cl.GetBalanceAsync (() => {
				Fire (TRIGGER_COMPLETE);
			});
		}

		private	void OnExitBalance ()
		{
		}

		#endregion

		#region 'Getting History' state

		private	void OnEntryHistory ()
		{
		}

		private	void OnLoopHistory ()
		{
			var crawler = stateMachines [CrawlerLogic.COINONE] as CrawlerCoinone;
			var balance = crawler.market.balance;
			var available = balance.GetAvailable (_symbol);
			var cash = balance.GetAvailable (Symbol.KR_WON);

			if (available > 0)
				Fire (TRIGGER_NEED_SELL);
			else if (cash > 0)
				Fire (TRIGGER_NEED_BUY);
			else
				Fire (TRIGGER_ERROR);
		}

		private	void OnExitHistory ()
		{
		}

		#endregion

		#region 'Ready2Buy' state

		private	void OnEntryR2B ()
		{
			Console.WriteLine ("Ready to BUY");
		}

		private	void OnLoopR2B ()
		{
		}

		private	void OnExitR2B ()
		{
		}

		#endregion

		#region 'Buying' state

		private	void OnEntryBuy ()
		{
		}

		private	void OnLoopBuy ()
		{
		}

		private	void OnExitBuy ()
		{
		}

		#endregion

		#region 'Ready2Sell' state

		private	void OnEntryR2S ()
		{
			Console.WriteLine ("Ready to SELL");
		}

		private void OnLoopR2S ()
		{
		}

		private void OnExitR2S ()
		{
		}

		#endregion

		#region 'Selling' state

		private	void OnEntrySell ()
		{
		}

		private	void OnLoopSell ()
		{
		}

		private	void OnExitSell ()
		{
		}

		#endregion

		#region 'Idle' state

		private	void OnEntryIdle ()
		{
		}

		private	void OnLoopIdle ()
		{
		}

		private	void OnExitIdle ()
		{
		}

		#endregion
	}
}


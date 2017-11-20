using System;
using Joi.FSM;

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

		private	const string TRIGGER_NEED_SELL	= "needSell";
		private	const string TRIGGER_NEED_BUY	= "needBuy";
		private	const string TRIGGER_COMPLETE	= "complete";

		public TradeLogic (bool logging = true) : base("TradeLogic", 100, logging)
		{
			SetFirstState (STATE_BALANCE)
				.SetupEntry (OnEntryBalance)
				.SetupExit (OnExitBalance)
				.SetupLoop (OnLoopBalance)
				.ConnectTo (TRIGGER_COMPLETE, STATE_HISTORY);
			SetState(STATE_HISTORY)
				.SetupEntry(OnEntryHistory)
				.SetupExit(OnExitHistory)
				.SetupLoop(OnLoopHistory)
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
			Start ();
		}

		~TradeLogic()
		{
			End ();
		}

		#region 'Getting Balance' state

		private	void OnEntryBalance()
		{
		}

		private	void OnLoopBalance()
		{
		}

		private	void OnExitBalance()
		{
		}

		#endregion

		#region 'Getting History' state

		private	void OnEntryHistory()
		{
		}

		private	void OnLoopHistory()
		{
		}

		private	void OnExitHistory()
		{
		}

		#endregion

		#region 'Ready2Buy' state

		private	void OnEntryR2B()
		{
		}

		private	void OnLoopR2B()
		{
		}

		private	void OnExitR2B()
		{
		}

		#endregion

		#region 'Buying' state

		private	void OnEntryBuy()
		{
		}

		private	void OnLoopBuy()
		{
		}

		private	void OnExitBuy()
		{
		}

		#endregion

		#region 'Ready2Sell' state

		private	void OnEntryR2S()
		{
		}

		private void OnLoopR2S()
		{
		}

		private void OnExitR2S()
		{
		}

		#endregion

		#region 'Selling' state

		private	void OnEntrySell()
		{
		}

		private	void OnLoopSell()
		{
		}

		private	void OnExitSell()
		{
		}

		#endregion
	}
}


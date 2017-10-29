using System;
using Joi.FSM;

namespace Joi.Brain
{
	public class TradeLogic : StateMachine
	{
		private	const string STATE_BALANCING	= "Balancing";
		private	const string STATE_SEARCHING	= "Searching";
		private	const string STATE_READY2BUY	= "Ready2Buy";
		private	const string STATE_READY2SELL	= "Ready2Sell";
		private	const string STATE_FALLING		= "Falling";
		private	const string STATE_RISING		= "Rising";
		private	const string STATE_BUYING		= "Buying";
		private	const string STATE_SELLING		= "Selling";

		private	const string TRIGGER_SEARCH		= "search";
		private	const string TRIGGER_READY		= "ready";
		private	const string TRIGGER_CHECKOUT	= "checkout";
		private	const string TRIGGER_RISE		= "rise";
		private	const string TRIGGER_FALL		= "fall";
		private	const string TRIGGER_COMPLETE	= "complete";

		public TradeLogic (bool logging = true) : base("TradeLogic", 100, logging)
		{
			SetFirstState (STATE_BALANCING)
				.SetupEntry(OnEntryBalance)
				.SetupExit(OnExitBalance)
				.SetupLoop(OnLoopBalance)
				.ConnectTo (TRIGGER_SEARCH, STATE_SEARCHING)
				.ConnectTo (TRIGGER_READY, STATE_READY2SELL);
			SetState (STATE_SEARCHING)
				.SetupEntry(OnEntrySearch)
				.SetupExit(OnExitSearch)
				.SetupLoop(OnLoopSearch)
				.ConnectTo (TRIGGER_READY, STATE_READY2BUY);
			SetState (STATE_READY2BUY)
				.SetupEntry(OnEntryR2B)
				.SetupExit(OnExitR2B)
				.SetupLoop(OnLoopR2B)
				.ConnectTo (TRIGGER_CHECKOUT, STATE_BUYING);
			SetState (STATE_BUYING)
				.SetupEntry(OnEntryBuy)
				.SetupExit(OnExitBuy)
				.SetupLoop(OnLoopBuy)
				.ConnectTo (TRIGGER_COMPLETE, STATE_BALANCING);
			SetState (STATE_READY2SELL)
				.SetupEntry(OnEntryR2S)
				.SetupExit(OnExitR2S)
				.SetupLoop(OnLoopR2S)
				.ConnectTo (TRIGGER_FALL, STATE_FALLING)
				.ConnectTo (TRIGGER_RISE, STATE_RISING);
			SetState (STATE_FALLING)
				.SetupEntry(OnEntryFall)
				.SetupExit(OnExitFall)
				.SetupLoop(OnLoopFall)
				.ConnectTo (TRIGGER_RISE, STATE_RISING)
				.ConnectTo (TRIGGER_CHECKOUT, STATE_SELLING);
			SetState (STATE_RISING)
				.SetupEntry(OnEntryRise)
				.SetupExit(OnExitRise)
				.SetupLoop(OnLoopRise)
				.ConnectTo (TRIGGER_FALL, STATE_FALLING)
				.ConnectTo (TRIGGER_CHECKOUT, STATE_SELLING);
			SetState (STATE_SELLING)
				.SetupEntry(OnEntrySell)
				.SetupExit(OnExitSell)
				.SetupLoop(OnLoopSell)
				.ConnectTo (TRIGGER_COMPLETE, STATE_BALANCING);
			Start ();
		}

		~TradeLogic()
		{
			End ();
		}

		#region 'Balancing' state

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

		#region 'Searching' state

		private void OnEntrySearch()
		{
		}

		private	void OnLoopSearch()
		{
		}

		private	void OnExitSearch()
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

		#region 'Rising' state

		private	void OnEntryRise()
		{
		}

		private	void OnLoopRise()
		{
		}

		private	void OnExitRise()
		{
		}

		#endregion

		#region 'Falling' state

		private	void OnEntryFall()
		{
		}

		private	void OnLoopFall()
		{
		}

		private	void OnExitFall()
		{
		}

		#endregion
	}
}


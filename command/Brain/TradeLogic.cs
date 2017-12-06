using System;
using Joi.FSM;
using Joi.Data;

namespace Joi.Brain
{
    public class TradeLogic : StateMachine
    {
        private const string STATE_INIT = "Init";
        private const string STATE_BALANCE = "Balance";
        private const string STATE_HISTORY = "History";
        private const string STATE_READY2BUY = "Ready2Buy";
        private const string STATE_READY2SELL = "Ready2Sell";
        private const string STATE_BUYING = "Buying";
        private const string STATE_SELLING = "Selling";
        private const string STATE_IDLE = "Idle";

        private const string TRIGGER_NEED_SELL = "needSell";
        private const string TRIGGER_NEED_BUY = "needBuy";
        private const string TRIGGER_COMPLETE = "complete";
		private const string TRIGGER_SIMULATE = "simulate";
        private const string TRIGGER_ERROR = "error";

        private Symbol _symbol;
		private	double _buying;
		private	double _benefit;
		private	bool _tradable;

		public	bool IsTradable { get { return _tradable; } }

        public TradeLogic(Symbol symbol, bool logging = true) : base("TradeLogic", 100, logging)
		{
			_symbol = symbol;
			_buying = 0;
			_benefit = 0;
			_tradable = true;
			SetFirstState (STATE_INIT)
                .SetupEntry (OnEntryInit)
                .SetupLoop (OnLoopInit)
                .SetupExit (OnExitInit)
                .ConnectTo (TRIGGER_COMPLETE, STATE_BALANCE);
			SetState (STATE_BALANCE)
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
				.ConnectTo (TRIGGER_SIMULATE, STATE_READY2SELL)
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
				.ConnectTo (TRIGGER_SIMULATE, STATE_READY2BUY)
                .ConnectTo (TRIGGER_COMPLETE, STATE_BALANCE);
			SetState (STATE_IDLE)
                .SetupEntry (OnEntryIdle)
                .SetupExit (OnExitIdle)
                .SetupLoop (OnLoopIdle)
                .ConnectTo (TRIGGER_COMPLETE, STATE_BALANCE);
			Start ();
		}

        ~TradeLogic()
        {
            End();
        }

		public	void TradeOn()
		{
			_tradable = true;
		}

		public	void TradeOff()
		{
			_tradable = false;
		}

        #region 'Initialize' state

        private void OnEntryInit()
        {
        }

        private void OnLoopInit()
        {
            var cl = stateMachines[CrawlerLogic.COINONE] as CrawlerCoinone;
            cl.GetBalanceAsync(() =>
            {
                Fire(TRIGGER_COMPLETE);
            });
            Fire(TRIGGER_COMPLETE);
        }

        private void OnExitInit()
        {
        }

        #endregion

        #region 'Getting Balance' state

        private void OnEntryBalance()
        {
        }

        private void OnLoopBalance()
        {
        }

        private void OnExitBalance()
        {
        }

        #endregion

        #region 'Getting History' state

        private void OnEntryHistory()
        {
        }

        private void OnLoopHistory()
        {
            var crawler = stateMachines[CrawlerLogic.COINONE] as CrawlerCoinone;
            var balance = crawler.market.balance;
            var available = balance.GetAvailable(_symbol);
            var cash = balance.GetAvailable(Symbol.KR_WON);

            if (available > 0)
                Fire(TRIGGER_NEED_SELL);
            else if (cash > 0)
                Fire(TRIGGER_NEED_BUY);
            else
                Fire(TRIGGER_ERROR);
        }

        private void OnExitHistory()
        {
        }

        #endregion

        #region 'Ready2Buy' state

        private void OnEntryR2B()
        {
        }

        private void OnLoopR2B()
		{
			if (!_tradable)
				return;

			var us = (stateMachines [CrawlerLogic.BITFINEX] as CrawlerBitfinex).market.GetIndicator (TimeInterval.MINUTE_1);
			if (Utility.IsTimeToReadyBuying (us.lastBollingerBand))
				Fire (TRIGGER_COMPLETE);
		}

        private void OnExitR2B()
        {
        }

        #endregion

        #region 'Buying' state

        private void OnEntryBuy()
        {
        }

        private void OnLoopBuy()
        {
			if (!_tradable)
				return;

//			var kr = stateMachines [CrawlerLogic.COINONE] as CrawlerCoinone;
//			var krInd = kr.market.GetIndicator (TimeInterval.MINUTE_1);
//			var krLast = krInd.lastCandle;
//			_buying = krLast.close;

			var us = stateMachines [CrawlerLogic.BITFINEX] as CrawlerBitfinex;
			var usInd = us.market.GetIndicator (TimeInterval.MINUTE_1);
			var usLast = usInd.lastCandle;

			if (!Utility.IsTimeToBuying (usInd.lastBollingerBand, usInd.lastOscillator))
				return;

			// for test
			_buying = usLast.close;

			ConsoleIO.LogLine ("{1} BUY: {0}", _buying, DateTime.Now.ToString("F"));
			Fire (TRIGGER_SIMULATE);
        }

        private void OnExitBuy()
        {
        }

        #endregion

        #region 'Ready2Sell' state

        private void OnEntryR2S()
        {
        }

        private void OnLoopR2S()
        {
			if (!_tradable)
				return;

			var us1Ind = (stateMachines [CrawlerLogic.BITFINEX] as CrawlerBitfinex).market.GetIndicator (TimeInterval.MINUTE_1);
			if (Utility.IsTimeToSell (us1Ind.lastCandle, us1Ind.lastOscillator, _buying))
				Fire (TRIGGER_COMPLETE);
        }

        private void OnExitR2S()
        {
        }

        #endregion

        #region 'Selling' state

        private void OnEntrySell()
        {
        }

        private void OnLoopSell()
        {
//			var kr = stateMachines [CrawlerLogic.COINONE] as CrawlerCoinone;
//			var krInd = kr.market.GetIndicator (TimeInterval.MINUTE_1);
//			var krLast = krInd.lastCandle;
//			var cur = krLast.close;
//			var benefit = cur * 0.999 - _buying;
//			_benefit += benefit;

			var us = stateMachines [CrawlerLogic.BITFINEX] as CrawlerBitfinex;
			var usInd = us.market.GetIndicator (TimeInterval.MINUTE_1);
			var usLast = usInd.lastCandle;


			// for test
			var cur = usLast.close;
			var benefit = usLast.close * 0.999 - _buying;
			_benefit += benefit;

			ConsoleIO.LogLine ("{3} SELL: {0} ({1}, total {2})", (int)cur, (int)benefit, (int)_benefit, DateTime.Now.ToString("F"));
			Fire (TRIGGER_SIMULATE);
        }

        private void OnExitSell()
        {
        }

        #endregion

        #region 'Idle' state

        private void OnEntryIdle()
        {
        }

        private void OnLoopIdle()
        {
        }

        private void OnExitIdle()
        {
        }

        #endregion
    }
}


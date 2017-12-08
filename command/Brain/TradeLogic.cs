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
		private const string STATE_AGING = "Aging";
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
		private	double _buyingUS;
		private	double _buyingKR;
		private	double _benefitUS;
		private	double _benefitKR;
		private	bool _tradable;

		public	bool IsTradable { get { return _tradable; } }

        public TradeLogic(Symbol symbol, bool logging = true) : base("TradeLogic", 100, logging)
		{
			_symbol = symbol;
			_buyingUS = 0;
			_benefitUS = 0;
			_buyingKR = 0;
			_benefitKR = 0;
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
				.ConnectTo (TRIGGER_SIMULATE, STATE_AGING)
				.ConnectTo (TRIGGER_COMPLETE, STATE_AGING);
			SetState (STATE_AGING)
				.SetupEntry(OnEntryAging)
				.SetupExit(OnExitAging)
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

			var kr_1m = (stateMachines [CrawlerLogic.COINONE] as CrawlerCoinone).market.GetIndicator (TimeInterval.MINUTE_1);
			var kr_5m = (stateMachines [CrawlerLogic.COINONE] as CrawlerCoinone).market.GetIndicator (TimeInterval.MINUTE_5);
			if (!Utility.IsTimeToReadyBuying (kr_1m.lastBollingerBand, kr_1m.lastOscillator, kr_5m.lastOscillator))
				return;

			ConsoleIO.LogLine ("{1} READY: W{0}", kr_1m.lastCandle.close, DateTime.Now.ToString("F"));
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
			
			var usInd_1m = (stateMachines [CrawlerLogic.BITFINEX] as CrawlerBitfinex).market.GetIndicator (TimeInterval.MINUTE_1);
			var usInd_5m = (stateMachines [CrawlerLogic.BITFINEX] as CrawlerBitfinex).market.GetIndicator (TimeInterval.MINUTE_5);
			var usLast_1m = usInd_1m.lastCandle;
			var krInd_1m = (stateMachines [CrawlerLogic.COINONE] as CrawlerCoinone).market.GetIndicator (TimeInterval.MINUTE_1);
			var krInd_5m = (stateMachines [CrawlerLogic.COINONE] as CrawlerCoinone).market.GetIndicator (TimeInterval.MINUTE_5);
			var krLast_1m = krInd_1m.lastCandle;

			if (!Utility.IsTimeToBuying (krInd_1m.lastBollingerBand, krInd_1m.lastOscillator, krInd_5m.lastOscillator))
				return;

			// for test
			_buyingUS = usLast_1m.close;
			_buyingKR = krLast_1m.close;

			ConsoleIO.LogLine ("{1} BUY: W{0}", (int)_buyingKR, DateTime.Now.ToString("F"));
			Fire (TRIGGER_SIMULATE);
        }

        private void OnExitBuy()
        {
        }

        #endregion

		#region 'Aging' state

		System.Timers.Timer _timer;

		private	void OnEntryAging()
		{
			_timer = new System.Timers.Timer ((int)TimeInterval.MINUTE_1 * 1000);
			_timer.AutoReset = false;
			_timer.Elapsed += OnTimerAging;
			_timer.Start ();
		}

		private void OnTimerAging (object sender, System.Timers.ElapsedEventArgs e)
		{
			Fire (TRIGGER_SIMULATE);
		}

		private void OnExitAging()
		{
			_timer.Elapsed -= OnTimerAging;
			_timer.Stop ();
			_timer = null;
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

			var kr1Ind = (stateMachines [CrawlerLogic.COINONE] as CrawlerCoinone).market.GetIndicator (TimeInterval.MINUTE_1);
			var kr5Ind = (stateMachines [CrawlerLogic.COINONE] as CrawlerCoinone).market.GetIndicator (TimeInterval.MINUTE_5);
			if (Utility.IsTimeToSell (kr1Ind.lastCandle, kr1Ind.lastOscillator, kr5Ind.lastOscillator, _buyingKR))
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
			var krInd = (stateMachines [CrawlerLogic.COINONE] as CrawlerCoinone).market.GetIndicator (TimeInterval.MINUTE_1);
			var krLast = krInd.lastCandle;

			// for test
			var curKR = krLast.close;
			var benefitKR = krLast.close * 0.999 - _buyingKR;
			_benefitKR += benefitKR;

			ConsoleIO.LogLine ("{3} SELL: W{0} ({1}, total {2})", (int)curKR, (int)benefitKR, (int)_benefitKR, DateTime.Now.ToString("F"));
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


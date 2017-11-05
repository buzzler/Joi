using System;
using System.Threading;
using Joi.Brain;
using Joi.Data;
/// <summary>
/// test
/// </summary>
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Text;

namespace Joi
{
	class Body
	{
//		public static void Main (string[] args)
//		{
//			using (var socket = new WebSocketSharp.WebSocket ("wss://api.bitfinex.com/ws")) {
//				socket.OnMessage += (object sender, WebSocketSharp.MessageEventArgs e) => {
//					Console.WriteLine ("onmessage: {0}", e.Data);
//				};
//				socket.Connect ();
//				socket.Send ("{\"event\":\"ping\"}");
//				socket.Send ("{\"event\":\"subscribe\",\"channel\":\"trades\",\"symbol\":\"btcusd\"}");
//				Console.ReadKey (true);
//			}
//		}

		public static void Main (string[] args)
		{
			Symbol symbol = Symbol.BITCOIN;
			bool logging = false;

			Thread[] threads = new Thread[] {
				new Thread (new AppLogic (logging).Run),
				new Thread (new TradeLogic (logging).Run),
				new Thread (new CrawlerCoinone (symbol, logging).Run),
				new Thread (new CrawlerBitfinex (symbol, logging).Run),
				new Thread (new CrawlerBitflyer (symbol, logging).Run)
			};
			for (int i = 0; i < threads.Length; i++)
				threads [i].Start ();
			bool alive = true;
			while (alive) {
				for (int i = 0; i < threads.Length; i++) {
					alive &= threads [i].IsAlive;
				}
			}
		}
	}


}

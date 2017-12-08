using System;
using LitJson;
using System.Net;
using System.IO;
using System.Text;
using Joi.Data.Chart;

namespace Joi
{
	public class Utility
	{
		/// <summary>
		/// Gets the response.
		/// </summary>
		/// <returns>The response.</returns>
		/// <param name="request">Request.</param>
		public	static JsonData GetResponse (WebRequest request)
		{
			JsonData result = null;
			try {
				request.Credentials = CredentialCache.DefaultCredentials;
				using (var response = request.GetResponse ()) {
					using (var stream = response.GetResponseStream ()) {
						using (var reader = new StreamReader (stream)) {
							result = JsonMapper.ToObject (reader.ReadToEnd ());
						}
					}
				}
			} catch (Exception e) {
				ConsoleIO.Error (e.Message);
			}
			return result;
		}

		/// <summary>
		/// Hexs the string from bytes.
		/// </summary>
		/// <returns>The string from bytes.</returns>
		/// <param name="bytes">Bytes.</param>
		public	static string HexStringFromBytes (byte[] bytes)
		{
			var sb = new StringBuilder ();
			int total = bytes.Length;
			for (int i = 0; i < total; i++) {
				var hex = bytes [i].ToString ("x2");
				sb.Append (hex);
			}
			return sb.ToString ();
		}

		/// <summary>
		/// Timestamp the specified dateTime.
		/// </summary>
		/// <param name="dateTime">Date time.</param>
		public	static int Timestamp(DateTime dateTime)
		{
			return (Int32)(dateTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
		}

		public	static DateTime DateTime(int unixTimeStamp)
		{
			System.DateTime dtDateTime = new DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);
			dtDateTime = dtDateTime.AddSeconds( (double)unixTimeStamp ).ToLocalTime();
			return dtDateTime;
		}

		public	static bool IsTimeToSell(Candle candle_1m, MACDOscillator oscillator_1m, MACDOscillator oscillator_5m, double bought = 0)
		{
//			if ((bought > 0) && (bought * 1.001 > candle_1m.close))
//				return false;

			if (bought > 0) {
				var fee = bought * 0.001;
				var benefit = Math.Abs(candle_1m.close - bought);
				if (fee >= benefit)
					return false;
			}

			if (oscillator_1m.decreasing) {
				if (oscillator_5m.decreasing)
					return true;
			}

			return false;
		}

		public	static bool IsTimeToReadyBuying(BollingerBand bb_1m, MACDOscillator oscillator_1m, MACDOscillator oscillator_5m)
		{
//			var result = (bb_1m.deviationRatio < -0.7);
//			if (result) {
//				ConsoleIO.LogLine ("[BollingerBand] high:{0}, value:{1}, low:{2}, ratio:{3}", bb_1m.highband, bb_1m.value, bb_1m.lowband, bb_1m.deviationRatio);
//				return true;
//			}

			var result = (oscillator_1m.increasing && oscillator_5m.increasing);
			if (result) {
				ConsoleIO.LogLine ("[Oscillator] increasing");
				return true;
			}

			return false;
		}

		public	static bool IsTimeToBuying(BollingerBand bb_1m, MACDOscillator oscillator_1m, MACDOscillator oscillator_5m)
		{
			var result = (bb_1m.deviationRatio > -0.6 && oscillator_1m.increasing);
			if (result) {
				ConsoleIO.LogLine ("[BollingerBand] high:{0}, value:{1}, low:{2}, ratio:{3}", bb_1m.highband, bb_1m.value, bb_1m.lowband, bb_1m.deviationRatio);
				return true;
			}

//			result = (oscillator_1m.increasing && oscillator_5m.increasing);
//			if (result) {
//				ConsoleIO.LogLine ("[Oscillator] increasing");
//				return true;
//			}

			return false;
		}
	}
}


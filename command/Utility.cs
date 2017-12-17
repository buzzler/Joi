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
							reader.Close();
							reader.Dispose();
						}
						stream.Close();
						stream.Dispose();
					}
					response.Close();
					response.Dispose();
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

		public	static bool IsTimeToBuy(Indicator indicator, Candle candle, BollingerBand bb, MACDOscillator os)
		{
//			if (os.decreasing) {
//				return false;
//			}
//			if (bb.crossingBelow || bb.deviationRatio <= -1)
//				return true;
//			return false;

			// test

			if (os.decreasing) {
				return false;
			}
			if (bb.crossingBelow) {
				crossingBuy = true;
				return true;
			} else if (bb.deviationRatio <= -0.9) {
				crossingBuy = false;
				return true;
			}
			return false;
		}

		public	static bool crossingBuy = false;

		public	static bool IsTimeToSell(Indicator indicator, Candle candle, BollingerBand bb, MACDOscillator os, double bought, double feerate)
		{
//			if (os.decreasing) {
//				if ((candle.close * (1 - feerate)) < bought)
//					return true;
//				else
//					return false;
//			}
//			if (bb.crossingAbove || bb.deviationRatio >= 1) {
//				return true;
//			}
//			return false;

			// test

			if (os.decreasing) {
				if ((candle.close * (1 - feerate)) < bought)
					return true;
				else
					return false;
			}

			if (crossingBuy && bb.deviationRatio >= 0.9)
				return true;
			if (!crossingBuy && bb.crossingAbove)
				return true;
			return false;
		}
	}
}


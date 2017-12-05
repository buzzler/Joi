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

		public	static bool IsTimeToSell(Candle candle, MACDOscillator oscillator, double bought = 0)
		{
			if ((bought > 0) && (bought * 1.001 > candle.close))
				return false;
			else if (oscillator.decreasing)
				return true;

			return false;
		}

		public	static bool IsTimeToBuy(BollingerBand bb, MACDOscillator oscillator)
		{
			if (bb.deviationRatio < -0.9 && oscillator.increasing)
				return true;
			else if (oscillator.delta > 0.5)
				return true;

			return false;
		}
	}
}


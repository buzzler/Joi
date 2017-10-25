using System;
using LitJson;
using System.Net;
using System.IO;
using System.Text;

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
			request.Credentials = CredentialCache.DefaultCredentials;
			using (var response = request.GetResponse ()) {
				using (var stream = response.GetResponseStream ()) {
					using (var reader = new StreamReader (stream)) {
						return JsonMapper.ToObject (reader.ReadToEnd ());
					}
				}
			}
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
	}
}


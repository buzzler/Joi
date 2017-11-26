using System;
using System.Text;
using System.Collections.Generic;

namespace Joi
{
	public class ConsoleIO
	{
		private	static ConsoleIO _instance;

		private static ConsoleIO instance { 
			get {
				if (_instance == null)
					_instance = new ConsoleIO ();
				return _instance;
			}
		}

		private	const int CAPACITY = 65535;
		private	StringBuilder _log;
		private	Stack<Tuple<int, int>> _cursor;

		public	ConsoleIO()
		{
			if (_instance != null)
				throw new Exception ("Already a instance exist");

			_log = new StringBuilder ();
			_cursor = new Stack<Tuple<int, int>> ();
			_instance = this;
		}

		public	static void Write(string message)
		{
			Console.Write (message);
		}

		public	static void Write(string format, params object[] args)
		{
			Console.Write (format, args);
		}

		public	static void WriteLine()
		{
			Console.WriteLine ();
		}

		public	static void WriteLine(string message)
		{
			Console.WriteLine (message);
		}

		public	static void WriteLine(string format, params object[] args)
		{
			Console.WriteLine (format, args);
		}

		private	static string AppendPrefixLog(string message)
		{
			return string.Format ("[log]\t{0}", message);
		}

		private	static string AppendPrefixLog(string format, params object[] args)
		{
			return string.Format ("[log]\t{0}", string.Format(format, args));
		}

		private	static string AppendPrefixError(string message)
		{
			return string.Format ("[error]\t{0}", message);
		}

		private	static string AppendPrefixError(string format, params object[] args)
		{
			return string.Format ("[error]\t{0}", string.Format(format, args));
		}

		public	static void Log(string message)
		{
			instance._log.Append (AppendPrefixLog(message));
			Clip (instance._log);
		}

		public	static void Log(string format, params object[] args)
		{
			instance._log.Append (AppendPrefixLog(format, args));
			Clip (instance._log);
		}

		public	static void LogLine()
		{
			instance._log.AppendLine ();
			Clip (instance._log);
		}

		public	static void LogLine(string message)
		{
			instance._log.AppendLine (AppendPrefixLog(message));
			Clip (instance._log);
		}

		public	static void LogLine(string format, params object[] args)
		{
			instance._log.AppendLine (AppendPrefixLog(format, args));
			Clip (instance._log);
		}

		public	static void Error(string message)
		{
			instance._log.AppendLine (AppendPrefixError(message));
			Clip (instance._log);
		}

		public	static void Error(string format, params object[] args)
		{
			instance._log.AppendFormat (AppendPrefixError(format, args));
			instance._log.AppendLine ();
			Clip (instance._log);
		}

		public	static int Read()
		{
			return Console.Read ();
		}

		public	static string ReadLine()
		{
			return Console.ReadLine ();
		}

		public	static ConsoleKeyInfo ReadKey()
		{
			return Console.ReadKey ();
		}

		public	static void Clear()
		{
			Console.Clear ();
		}

		public	static void SetCursor(int x, int y)
		{
			Console.CursorLeft = x;
			Console.CursorTop = y;
		}

		public	static void PushCursor()
		{
			instance._cursor.Push (new Tuple<int, int> (Console.CursorLeft, Console.CursorTop));
		}

		public	static void PopCursor()
		{
			if (instance._cursor.Count == 0)
				return;
			
			var tuple = instance._cursor.Pop ();
			Console.CursorLeft = tuple.Item1;
			Console.CursorTop = tuple.Item2;
		}

		public	static void ShowCursor()
		{
			Console.CursorVisible = true;
		}

		public	static void HideCursor()
		{
			Console.CursorVisible = false;
		}

		private	static void Clip(StringBuilder sb)
		{
			var len = sb.Length;
			if (len > CAPACITY)
				sb.Remove (0, len - CAPACITY);
		}

		public	static string GetLog()
		{
			return instance._log.ToString ();
		}
	}
}


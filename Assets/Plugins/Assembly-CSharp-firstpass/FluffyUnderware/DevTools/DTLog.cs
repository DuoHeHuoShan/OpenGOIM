using System;
using UnityEngine;

namespace FluffyUnderware.DevTools
{
	public static class DTLog
	{
		public static void Log(object message)
		{
			Debug.Log(message);
		}

		public static void Log(object message, UnityEngine.Object context)
		{
			Debug.Log(message, context);
		}

		public static void LogError(object message)
		{
			Debug.LogError(message);
		}

		public static void LogError(object message, UnityEngine.Object context)
		{
			Debug.LogError(message, context);
		}

		public static void LogErrorFormat(string format, params object[] args)
		{
			Debug.LogErrorFormat(format, args);
		}

		public static void LogErrorFormat(UnityEngine.Object context, string format, params object[] args)
		{
			Debug.LogErrorFormat(context, format, args);
		}

		public static void LogException(Exception exception)
		{
			Debug.LogException(exception);
		}

		public static void LogException(Exception exception, UnityEngine.Object context)
		{
			Debug.LogException(exception, context);
		}

		public static void LogFormat(string format, params object[] args)
		{
			Debug.LogFormat(format, args);
		}

		public static void LogFormat(UnityEngine.Object context, string format, params object[] args)
		{
			Debug.LogFormat(context, format, args);
		}

		public static void LogWarning(object message)
		{
			Debug.LogWarning(message);
		}

		public static void LogWarning(object message, UnityEngine.Object context)
		{
			Debug.LogWarning(message, context);
		}

		public static void LogWarningFormat(string format, params object[] args)
		{
			Debug.LogWarningFormat(format, args);
		}

		public static void LogWarningFormat(UnityEngine.Object context, string format, params object[] args)
		{
			Debug.LogWarningFormat(context, format, args);
		}
	}
}

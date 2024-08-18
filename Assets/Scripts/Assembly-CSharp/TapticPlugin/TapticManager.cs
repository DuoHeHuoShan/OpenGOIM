namespace TapticPlugin
{
	public static class TapticManager
	{
		public static void Notification(NotificationFeedback feedback)
		{
			_unityTapticNotification((int)feedback);
		}

		public static void Impact(ImpactFeedback feedback)
		{
			_unityTapticImpact((int)feedback);
		}

		public static void Selection()
		{
			_unityTapticSelection();
		}

		public static bool IsSupport()
		{
			return _unityTapticIsSupport();
		}

		private static void _unityTapticNotification(int type)
		{
		}

		private static void _unityTapticSelection()
		{
		}

		private static void _unityTapticImpact(int style)
		{
		}

		private static bool _unityTapticIsSupport()
		{
			return false;
		}
	}
}

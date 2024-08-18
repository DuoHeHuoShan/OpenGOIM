using System;

namespace Prime31
{
	public class FlurryAndroidManager : AbstractManager
	{
		public static event Action<string> onFetchedEvent;

		public static event Action<string> onRenderedEvent;

		public static event Action<string> onDisplayEvent;

		public static event Action<string> onCloseEvent;

		public static event Action<string> onAppExitEvent;

		public static event Action<string> onClickedEvent;

		public static event Action<string> onVideoCompletedEvent;

		public static event Action<string> onRenderErrorEvent;

		public static event Action<string> onFetchErrorEvent;

		public static event Action<string> onShowFullscreenEvent;

		public static event Action<string> onCloseFullscreenEvent;

		static FlurryAndroidManager()
		{
			AbstractManager.initialize(typeof(FlurryAndroidManager));
		}

		private void onFetched(string adSpace)
		{
			FlurryAndroidManager.onFetchedEvent.fire(adSpace);
		}

		private void onRendered(string adSpace)
		{
			FlurryAndroidManager.onRenderedEvent.fire(adSpace);
		}

		private void onDisplay(string adSpace)
		{
			FlurryAndroidManager.onDisplayEvent.fire(adSpace);
		}

		private void onClose(string adSpace)
		{
			FlurryAndroidManager.onCloseEvent.fire(adSpace);
		}

		private void onAppExit(string adSpace)
		{
			FlurryAndroidManager.onAppExitEvent.fire(adSpace);
		}

		private void onClicked(string adSpace)
		{
			FlurryAndroidManager.onClickedEvent.fire(adSpace);
		}

		private void onVideoCompleted(string adSpace)
		{
			FlurryAndroidManager.onVideoCompletedEvent.fire(adSpace);
		}

		private void onRenderError(string adSpace)
		{
			FlurryAndroidManager.onRenderErrorEvent.fire(adSpace);
		}

		private void onFetchError(string adSpace)
		{
			FlurryAndroidManager.onFetchErrorEvent.fire(adSpace);
		}

		private void onShowFullscreen(string adSpace)
		{
			FlurryAndroidManager.onShowFullscreenEvent.fire(adSpace);
		}

		private void onCloseFullscreen(string adSpace)
		{
			FlurryAndroidManager.onCloseFullscreenEvent.fire(adSpace);
		}
	}
}

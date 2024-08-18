using UnityEngine;

namespace Prime31
{
	public class FlurryAndroidEventListener : MonoBehaviour
	{
		private void OnEnable()
		{
			FlurryAndroidManager.onFetchedEvent += onFetchedEvent;
			FlurryAndroidManager.onRenderedEvent += onRenderedEvent;
			FlurryAndroidManager.onDisplayEvent += onDisplayEvent;
			FlurryAndroidManager.onCloseEvent += onCloseEvent;
			FlurryAndroidManager.onAppExitEvent += onAppExitEvent;
			FlurryAndroidManager.onClickedEvent += onClickedEvent;
			FlurryAndroidManager.onVideoCompletedEvent += onVideoCompletedEvent;
			FlurryAndroidManager.onRenderErrorEvent += onRenderErrorEvent;
			FlurryAndroidManager.onFetchErrorEvent += onFetchErrorEvent;
			FlurryAndroidManager.onShowFullscreenEvent += onShowFullscreenEvent;
			FlurryAndroidManager.onCloseFullscreenEvent += onCloseFullscreenEvent;
		}

		private void OnDisable()
		{
			FlurryAndroidManager.onFetchedEvent -= onFetchedEvent;
			FlurryAndroidManager.onRenderedEvent -= onRenderedEvent;
			FlurryAndroidManager.onDisplayEvent -= onDisplayEvent;
			FlurryAndroidManager.onCloseEvent -= onCloseEvent;
			FlurryAndroidManager.onAppExitEvent -= onAppExitEvent;
			FlurryAndroidManager.onClickedEvent -= onClickedEvent;
			FlurryAndroidManager.onVideoCompletedEvent -= onVideoCompletedEvent;
			FlurryAndroidManager.onRenderErrorEvent -= onRenderErrorEvent;
			FlurryAndroidManager.onFetchErrorEvent -= onFetchErrorEvent;
			FlurryAndroidManager.onShowFullscreenEvent -= onShowFullscreenEvent;
			FlurryAndroidManager.onCloseFullscreenEvent -= onCloseFullscreenEvent;
		}

		private void onFetchedEvent(string adSpace)
		{
			Debug.Log("onFetchedEvent: " + adSpace);
		}

		private void onRenderedEvent(string adSpace)
		{
			Debug.Log("onRenderedEvent: " + adSpace);
		}

		private void onDisplayEvent(string adSpace)
		{
			Debug.Log("onDisplayEvent: " + adSpace);
		}

		private void onCloseEvent(string adSpace)
		{
			Debug.Log("onCloseEvent: " + adSpace);
		}

		private void onAppExitEvent(string adSpace)
		{
			Debug.Log("onAppExitEvent: " + adSpace);
		}

		private void onClickedEvent(string adSpace)
		{
			Debug.Log("onClickedEvent: " + adSpace);
		}

		private void onVideoCompletedEvent(string adSpace)
		{
			Debug.Log("onVideoCompletedEvent: " + adSpace);
		}

		private void onRenderErrorEvent(string adSpace)
		{
			Debug.Log("onRenderErrorEvent: " + adSpace);
		}

		private void onFetchErrorEvent(string adSpace)
		{
			Debug.Log("onFetchErrorEvent: " + adSpace);
		}

		private void onShowFullscreenEvent(string adSpace)
		{
			Debug.Log("onShowFullscreenEvent: " + adSpace);
		}

		private void onCloseFullscreenEvent(string adSpace)
		{
			Debug.Log("onCloseFullscreenEvent: " + adSpace);
		}
	}
}

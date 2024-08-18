using UnityEngine;

namespace FluffyUnderware.DevTools
{
	public static class DTTime
	{
		private static float _EditorDeltaTime;

		private static float _EditorLastTime;

		public static double TimeSinceStartup
		{
			get
			{
				return Time.timeSinceLevelLoad;
			}
		}

		public static float deltaTime
		{
			get
			{
				return (!Application.isPlaying) ? _EditorDeltaTime : Time.deltaTime;
			}
		}

		public static void InitializeEditorTime()
		{
			_EditorLastTime = Time.realtimeSinceStartup;
			_EditorDeltaTime = 0f;
		}

		public static void UpdateEditorTime()
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			float editorDeltaTime = realtimeSinceStartup - _EditorLastTime;
			_EditorDeltaTime = editorDeltaTime;
			_EditorLastTime = realtimeSinceStartup;
		}
	}
}

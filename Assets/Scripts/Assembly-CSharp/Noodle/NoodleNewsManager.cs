using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Noodle
{
	public class NoodleNewsManager : NoodleSingleton<NoodleNewsManager>
	{
		public bool DebugMode;

		public bool ShowCreatives = true;

		private List<Action> pendingMainThreadActions = new List<Action>();

		private float volumeToRestore = 1f;

		public void ShowMoreGames()
		{
			Application.OpenURL("https://www.noodlecake.com");
		}
	}
}

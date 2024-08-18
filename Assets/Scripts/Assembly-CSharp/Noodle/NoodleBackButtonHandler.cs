using UnityEngine;

namespace Noodle
{
	public class NoodleBackButtonHandler : MonoBehaviour
	{
		private static SettingsManager _settingsManager;

		private bool wasPaused;

		public static SettingsManager SettingsManager
		{
			get
			{
				if (_settingsManager == null)
				{
					_settingsManager = FindSettingsManager();
				}
				return _settingsManager;
			}
		}

		public static bool IsPaused
		{
			get
			{
				return SettingsManager != null && SettingsManager.IsMenuActive;
			}
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				if (SettingsManager.IsMenuActive && SettingsManager.IsQuitConfirmationWindowOpen)
				{
					SettingsManager.CloseQuitConfirmationWindow();
				}
				else if (SettingsManager.IsMenuActive && SettingsManager.IsResetGameConfirmationWindowOpen)
				{
					SettingsManager.CloseResetGameConfirmationWindow();
				}
				else if (SettingsManager.IsMenuActive && SettingsManager.IsRatePopupWindowOpen)
				{
					SettingsManager.CloseRatePopupWindow();
				}
				else
				{
					SettingsManager.ToggleMenu();
				}
			}
		}

		private void OnApplicationPause(bool pauseStatus)
		{
			if (pauseStatus)
			{
				wasPaused = true;
			}
			else if (wasPaused)
			{
				wasPaused = false;
				Pause();
			}
		}

		public static SettingsManager FindSettingsManager()
		{
			SettingsManager settingsManager = null;
			GameObject gameObject = GameObject.FindGameObjectWithTag("SettingsManager");
			if (gameObject != null)
			{
				settingsManager = gameObject.GetComponent<SettingsManager>();
			}
			if (settingsManager == null)
			{
				Debug.LogError("[NoodleManager] Failed to find SettingsManager");
			}
			return settingsManager;
		}

		public static void Pause()
		{
			SettingsManager settingsManager = SettingsManager;
			if (!(settingsManager == null) && !settingsManager.IsMenuActive)
			{
				settingsManager.ToggleMenu();
			}
		}

		public static void Unpause()
		{
			SettingsManager settingsManager = SettingsManager;
			if (!(settingsManager == null) && settingsManager.IsMenuActive)
			{
				if (settingsManager.IsQuitConfirmationWindowOpen)
				{
					settingsManager.CloseQuitConfirmationWindow();
				}
				if (settingsManager.IsResetGameConfirmationWindowOpen)
				{
					settingsManager.CloseResetGameConfirmationWindow();
				}
				if (settingsManager.IsRatePopupWindowOpen)
				{
					settingsManager.CloseRatePopupWindow();
				}
				settingsManager.ToggleMenu();
			}
		}
	}
}

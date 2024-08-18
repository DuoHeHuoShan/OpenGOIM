using UnityEngine;

namespace FluffyUnderware.DevTools
{
	public class DTVersionedMonoBehaviour : MonoBehaviour
	{
		[SerializeField]
		[HideInInspector]
		private string m_Version;

		private string mNewVersion;

		public string Version
		{
			get
			{
				return m_Version;
			}
		}

		protected void CheckForVersionUpgrade()
		{
		}

		protected virtual bool UpgradeVersion(string oldVersion, string newVersion)
		{
			if (string.IsNullOrEmpty(oldVersion))
			{
				Debug.LogFormat("[{0}] Upgrading '{1}' to version {2}! PLEASE SAVE THE SCENE!", GetType().Name, base.name, newVersion);
			}
			else
			{
				Debug.LogFormat("[{0}] Upgrading '{1}' from version {2} to {3}! PLEASE SAVE THE SCENE!", GetType().Name, base.name, oldVersion, newVersion);
			}
			return true;
		}
	}
}

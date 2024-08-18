using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Noodle
{
	public class NoodleAntiPiracy : MonoBehaviour
	{
		public bool DebugMode;

		public bool VerifyGooglePlayLicense;

		public bool VerifyAndroidPackageId;

		[Tooltip("This must be disabled for Google NBO games but it's nice to have for non-NBO games.")]
		public bool ShowPopupIfFailed;

		public string AndroidPackageId;

		public string GoogleLicenseKeyModulus;

		public string GoogleLicenseKeyExponent;

		private static NoodleAntiPiracy instance;

		private bool didFailLicense;

		private void Awake()
		{
			if (instance == null)
			{
				instance = this;
				Object.DontDestroyOnLoad(base.gameObject);
			}
			else
			{
				Object.Destroy(base.gameObject);
			}
		}


	}
}

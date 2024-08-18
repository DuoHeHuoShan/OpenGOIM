using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Noodle
{
	public static class GooglePlayLicenseVerifier
	{
		public enum LicenseStates
		{
			UNKNOWN = 0,
			LICENSED = 1,
			NOT_LICENSED = 2
		}

		private static bool isInitialized = false;

		private static bool isDoneProcessing = false;

		private static bool isChecking = false;

		private const int LICENSED = 0;

		private const int NOT_LICENSED = 1;

		private const int LICENSED_OLD_KEY = 2;

		private const int ERROR_NOT_MARKET_MANAGED = 3;

		private const int ERROR_SERVER_FAILURE = 4;

		private const int ERROR_OVER_QUOTA = 5;

		private const int ERROR_CONTACTING_SERVER = 257;

		private const int ERROR_INVALID_PACKAGE_NAME = 258;

		private const int ERROR_NON_MATCHING_UID = 259;

		private static RSAParameters m_PublicKey = default(RSAParameters);

		private static AndroidJavaObject m_Activity;

		private static AndroidJavaObject m_LVLCheckType;

		private static AndroidJavaObject m_LVLCheck = null;

		private static string m_PackageName;

		private static int m_Nonce;

		private static bool m_LVL_Received = false;

		private static int m_ResponseCodeInt = -1;

		[NonSerialized]
		public static string m_ResponseCode_Received;

		[NonSerialized]
		public static string m_PackageName_Received;

		private static int m_Nonce_Received;

		private static int m_VersionCode_Received;

		private static string m_UserID_Received;

		private static string m_Timestamp_Received;

		private static int m_MaxRetry_Received;

		private static string m_LicenceValidityTimestamp_Received;

		private static string m_GracePeriodTimestamp_Received;

		private static string m_UpdateTimestamp_Received;

		private static string m_FileURL1_Received = string.Empty;

		private static string m_FileURL2_Received = string.Empty;

		private static string m_FileName1_Received;

		private static string m_FileName2_Received;

		private static int m_FileSize1_Received;

		private static int m_FileSize2_Received;

		[NonSerialized]
		public static string m_LicensingURL_Received = string.Empty;

		public static LicenseStates LicenseState { get; private set; }

		public static bool didLogInitFail { get; private set; }

		private static void Init(string publicKeyModulus, string publicKeyExponent)
		{
			if (!isInitialized)
			{
				isInitialized = true;
				m_PublicKey.Modulus = Convert.FromBase64String(publicKeyModulus);
				m_PublicKey.Exponent = Convert.FromBase64String(publicKeyExponent);
				LoadServiceBinder();
				new SHA1CryptoServiceProvider();
			}
		}

		private static void LoadServiceBinder()
		{
			TextAsset textAsset = Resources.Load("LVLNativeServiceBinder/classes_jar") as TextAsset;
			if (textAsset == null)
			{
				Debug.LogError("[GooglePlayLicenseVerifier] Failed to load native service binder");
				return;
			}
			byte[] bytes = textAsset.bytes;
			m_Activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
			m_PackageName = m_Activity.Call<string>("getPackageName", new object[0]);
			string text = Path.Combine(m_Activity.Call<AndroidJavaObject>("getCacheDir", new object[0]).Call<string>("getPath", new object[0]), m_PackageName);
			Directory.CreateDirectory(text);
			File.WriteAllBytes(text + "/classes.jar", bytes);
			Directory.CreateDirectory(text + "/odex");
			AndroidJavaObject androidJavaObject = new AndroidJavaObject("dalvik.system.DexClassLoader", text + "/classes.jar", text + "/odex", null, m_Activity.Call<AndroidJavaObject>("getClassLoader", new object[0]));
			m_LVLCheckType = androidJavaObject.Call<AndroidJavaObject>("findClass", new object[1] { "com.unity3d.plugin.lvl.ServiceBinder" });
			if (m_LVLCheckType == null)
			{
				Debug.LogError("[GooglePlayLicenseVerifier] Failed to find service binder class");
			}
			androidJavaObject.Dispose();
			Directory.Delete(text, true);
		}

		internal static Dictionary<string, string> DecodeExtras(string query)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			if (query.Length == 0)
			{
				return dictionary;
			}
			int length = query.Length;
			int num = 0;
			bool flag = true;
			while (num <= length)
			{
				int num2 = -1;
				int num3 = -1;
				for (int i = num; i < length; i++)
				{
					if (num2 == -1 && query[i] == '=')
					{
						num2 = i + 1;
					}
					else if (query[i] == '&')
					{
						num3 = i;
						break;
					}
				}
				if (flag)
				{
					flag = false;
					if (query[num] == '?')
					{
						num++;
					}
				}
				string key;
				if (num2 == -1)
				{
					key = null;
					num2 = num;
				}
				else
				{
					key = WWW.UnEscapeURL(query.Substring(num, num2 - num - 1));
				}
				if (num3 < 0)
				{
					num = -1;
					num3 = query.Length;
				}
				else
				{
					num = num3 + 1;
				}
				string value = WWW.UnEscapeURL(query.Substring(num2, num3 - num2));
				dictionary.Add(key, value);
				if (num == -1)
				{
					break;
				}
			}
			return dictionary;
		}

		private static long ConvertEpochSecondsToTicks(long secs)
		{
			DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			long num = 10000L;
			long num2 = (DateTime.MaxValue.Ticks - dateTime.Ticks) / num;
			if (secs < 0)
			{
				secs = 0L;
			}
			if (secs > num2)
			{
				secs = num2;
			}
			return dateTime.Ticks + secs * num;
		}

		private static void Process()
		{
			try
			{
				m_LVL_Received = true;
				if (m_LVLCheck == null)
				{
					return;
				}
				int num = m_LVLCheck.Get<int>("_arg0");
				string text = m_LVLCheck.Get<string>("_arg1");
				string text2 = m_LVLCheck.Get<string>("_arg2");
				m_LVLCheck = null;
				m_ResponseCode_Received = num.ToString();
				m_ResponseCodeInt = num;
				if (num < 0 || string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text2))
				{
					m_PackageName_Received = "<Failed>";
					return;
				}
				byte[] bytes = Encoding.UTF8.GetBytes(text);
				byte[] rgbSignature = Convert.FromBase64String(text2);
				RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider();
				rSACryptoServiceProvider.ImportParameters(m_PublicKey);
				SHA1Managed sHA1Managed = new SHA1Managed();
				if (!rSACryptoServiceProvider.VerifyHash(sHA1Managed.ComputeHash(bytes), CryptoConfig.MapNameToOID("SHA1"), rgbSignature))
				{
					m_ResponseCode_Received = "<Failed>";
					m_PackageName_Received = "<Invalid Signature>";
					return;
				}
				int num2 = text.IndexOf(':');
				string text3;
				string text4;
				if (num2 == -1)
				{
					text3 = text;
					text4 = string.Empty;
				}
				else
				{
					text3 = text.Substring(0, num2);
					text4 = ((num2 < text.Length) ? text.Substring(num2 + 1) : string.Empty);
				}
				string[] array = text3.Split('|');
				if (array[0].CompareTo(num.ToString()) != 0)
				{
					m_ResponseCode_Received = "<Failed>";
					m_PackageName_Received = "<Response Mismatch>";
					return;
				}
				m_ResponseCode_Received = array[0];
				int.TryParse(m_ResponseCode_Received, out m_ResponseCodeInt);
				m_Nonce_Received = Convert.ToInt32(array[1]);
				m_PackageName_Received = array[2];
				m_VersionCode_Received = Convert.ToInt32(array[3]);
				m_UserID_Received = array[4];
				long ticks = ConvertEpochSecondsToTicks(Convert.ToInt64(array[5]));
				m_Timestamp_Received = new DateTime(ticks).ToLocalTime().ToString();
				if (!string.IsNullOrEmpty(text4))
				{
					Dictionary<string, string> dictionary = DecodeExtras(text4);
					if (dictionary.ContainsKey("GR"))
					{
						m_MaxRetry_Received = Convert.ToInt32(dictionary["GR"]);
					}
					else
					{
						m_MaxRetry_Received = 0;
					}
					if (dictionary.ContainsKey("VT"))
					{
						ticks = ConvertEpochSecondsToTicks(Convert.ToInt64(dictionary["VT"]));
						m_LicenceValidityTimestamp_Received = new DateTime(ticks).ToLocalTime().ToString();
					}
					else
					{
						m_LicenceValidityTimestamp_Received = null;
					}
					if (dictionary.ContainsKey("GT"))
					{
						ticks = ConvertEpochSecondsToTicks(Convert.ToInt64(dictionary["GT"]));
						m_GracePeriodTimestamp_Received = new DateTime(ticks).ToLocalTime().ToString();
					}
					else
					{
						m_GracePeriodTimestamp_Received = null;
					}
					if (dictionary.ContainsKey("UT"))
					{
						ticks = ConvertEpochSecondsToTicks(Convert.ToInt64(dictionary["UT"]));
						m_UpdateTimestamp_Received = new DateTime(ticks).ToLocalTime().ToString();
					}
					else
					{
						m_UpdateTimestamp_Received = null;
					}
					if (dictionary.ContainsKey("FILE_URL1"))
					{
						m_FileURL1_Received = dictionary["FILE_URL1"];
					}
					else
					{
						m_FileURL1_Received = string.Empty;
					}
					if (dictionary.ContainsKey("FILE_URL2"))
					{
						m_FileURL2_Received = dictionary["FILE_URL2"];
					}
					else
					{
						m_FileURL2_Received = string.Empty;
					}
					if (dictionary.ContainsKey("FILE_NAME1"))
					{
						m_FileName1_Received = dictionary["FILE_NAME1"];
					}
					else
					{
						m_FileName1_Received = null;
					}
					if (dictionary.ContainsKey("FILE_NAME2"))
					{
						m_FileName2_Received = dictionary["FILE_NAME2"];
					}
					else
					{
						m_FileName2_Received = null;
					}
					if (dictionary.ContainsKey("FILE_SIZE1"))
					{
						m_FileSize1_Received = Convert.ToInt32(dictionary["FILE_SIZE1"]);
					}
					else
					{
						m_FileSize1_Received = 0;
					}
					if (dictionary.ContainsKey("FILE_SIZE2"))
					{
						m_FileSize2_Received = Convert.ToInt32(dictionary["FILE_SIZE2"]);
					}
					else
					{
						m_FileSize2_Received = 0;
					}
					if (dictionary.ContainsKey("LU"))
					{
						m_LicensingURL_Received = dictionary["LU"];
					}
					else
					{
						m_LicensingURL_Received = string.Empty;
					}
				}
			}
			finally
			{
				isDoneProcessing = true;
			}
		}

		public static IEnumerator CheckLicense(string publicKeyModulus, string publicKeyExponent)
		{
			if (Application.isEditor)
			{
				Debug.Log("LVL Returning LICENSED for Editor run");
				LicenseState = LicenseStates.LICENSED;
			}
			else
			{
				if (isChecking)
				{
					yield break;
				}
				isChecking = true;
				LicenseState = LicenseStates.UNKNOWN;
				isDoneProcessing = false;
				Init(publicKeyModulus, publicKeyExponent);
				m_Nonce = new System.Random().Next();
				if (m_LVLCheckType == null)
				{
					if (!didLogInitFail)
					{
						didLogInitFail = true;
					}
					throw new InvalidOperationException("GooglePlayLicenseVerifier is not initialized properly.");
				}
				object[] param = new object[1] { new AndroidJavaObject[1] { m_Activity } };
				AndroidJavaObject[] ctors = m_LVLCheckType.Call<AndroidJavaObject[]>("getConstructors", new object[0]);
				m_LVLCheck = ctors[0].Call<AndroidJavaObject>("newInstance", param);
				m_LVLCheck.Call("create", m_Nonce, new AndroidJavaRunnable(Process));
				yield return new WaitUntil(() => isDoneProcessing);
				if (m_ResponseCode_Received == "<Failed>" || m_ResponseCodeInt == 1 || m_ResponseCodeInt == 3)
				{
					LicenseState = LicenseStates.NOT_LICENSED;
				}
				else if (m_ResponseCodeInt == 257 || m_ResponseCodeInt == 4)
				{
					LicenseState = LicenseStates.UNKNOWN;
				}
				else
				{
					LicenseState = LicenseStates.LICENSED;
				}
				isChecking = false;
			}
		}

		public static void FollowLastLicensingUrl()
		{
			string text = m_LicensingURL_Received;
			if (string.IsNullOrEmpty(m_LicensingURL_Received))
			{
				text = "https://play.google.com/store/apps/details?id=" + Application.identifier;
			}
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			{
				using (AndroidJavaObject androidJavaObject3 = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
				{
					using (AndroidJavaClass androidJavaClass2 = new AndroidJavaClass("android.net.Uri"))
					{
						using (AndroidJavaObject androidJavaObject = androidJavaClass2.CallStatic<AndroidJavaObject>("parse", new object[1] { text }))
						{
							using (AndroidJavaObject androidJavaObject2 = new AndroidJavaObject("android.content.Intent", "android.intent.action.VIEW", androidJavaObject))
							{
								using (androidJavaObject2.Call<AndroidJavaObject>("setPackage", new object[1] { "com.android.vending" }))
								{
									androidJavaObject3.Call("startActivity", androidJavaObject2);
								}
							}
						}
					}
				}
			}
		}
	}
}

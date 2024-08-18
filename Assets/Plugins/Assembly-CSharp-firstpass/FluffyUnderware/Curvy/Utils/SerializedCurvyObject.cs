using UnityEngine;

namespace FluffyUnderware.Curvy.Utils
{
	public abstract class SerializedCurvyObject<T>
	{
		public string ToJson()
		{
			return JsonUtility.ToJson(this);
		}

		public static T FromJson(string json)
		{
			return JsonUtility.FromJson<T>(json);
		}
	}
}

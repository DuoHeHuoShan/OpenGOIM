using System;
using UnityEngine;

namespace FxProNS
{
	[Serializable]
	public class BloomHelperParams
	{
		public EffectsQuality Quality;

		public Color BloomTint = Color.white;

		[Range(0f, 0.99f)]
		public float BloomThreshold = 0.8f;

		[Range(0f, 3f)]
		public float BloomIntensity = 1.5f;

		[Range(0.01f, 3f)]
		public float BloomSoftness = 0.5f;
	}
}

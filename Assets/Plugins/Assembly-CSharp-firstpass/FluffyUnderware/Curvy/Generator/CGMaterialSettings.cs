using System;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[Serializable]
	public class CGMaterialSettings
	{
		public bool SwapUV;

		public CGKeepAspectMode KeepAspect;

		public float UVRotation;

		public Vector2 UVOffset = Vector2.zero;

		public Vector2 UVScale = Vector2.one;
	}
}

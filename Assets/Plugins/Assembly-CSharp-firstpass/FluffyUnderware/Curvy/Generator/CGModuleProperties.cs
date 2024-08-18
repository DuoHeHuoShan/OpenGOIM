using System;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[Serializable]
	public class CGModuleProperties
	{
		public Rect Dimensions;

		public float MinWidth = 250f;

		public float LabelWidth;

		public Color BackgroundColor = Color.black;
	}
}

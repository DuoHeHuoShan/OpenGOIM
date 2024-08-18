using System;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	public class CGDataInfoAttribute : Attribute
	{
		public readonly Color Color;

		public CGDataInfoAttribute(Color color)
		{
			Color = color;
		}

		public CGDataInfoAttribute(float r, float g, float b, float a = 1f)
		{
			Color = new Color(r, g, b, a);
		}

		public CGDataInfoAttribute(string htmlColor)
		{
			Color = htmlColor.ColorFromHtml();
		}
	}
}

using System;

namespace FluffyUnderware.Curvy
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class CurvyShapeInfo : Attribute
	{
		public readonly string Name;

		public readonly bool Is2D;

		public CurvyShapeInfo(string name, bool is2D = true)
		{
			Name = name;
			Is2D = is2D;
		}
	}
}

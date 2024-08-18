using System;

namespace FluffyUnderware.Curvy
{
	[Flags]
	public enum CurvySplineGizmos
	{
		None = 0,
		Curve = 2,
		Approximation = 4,
		Tangents = 8,
		Orientation = 0x10,
		Labels = 0x20,
		Metadata = 0x40,
		Bounds = 0x80,
		All = 0xFFFF
	}
}

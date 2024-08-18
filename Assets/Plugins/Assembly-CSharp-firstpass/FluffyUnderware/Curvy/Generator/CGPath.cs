using System;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[CGDataInfo(0.13f, 0.59f, 0.95f, 1f)]
	public class CGPath : CGShape
	{
		public Vector3[] Direction = new Vector3[0];

		public CGPath()
		{
		}

		public CGPath(CGPath source)
			: base(source)
		{
			Direction = (Vector3[])source.Direction.Clone();
		}

		public override T Clone<T>()
		{
			return new CGPath(this) as T;
		}

		public static void Copy(CGPath dest, CGPath source)
		{
			CGShape.Copy(dest, source);
			Array.Resize(ref dest.Direction, source.Direction.Length);
			source.Direction.CopyTo(dest.Direction, 0);
			dest.Touch();
		}

		public void Interpolate(float f, out Vector3 pos, out Vector3 dir, out Vector3 up)
		{
			float frag;
			int fIndex = GetFIndex(f, out frag);
			pos = Vector3.Lerp(Position[fIndex], Position[fIndex + 1], frag);
			dir = Vector3.Lerp(Direction[fIndex], Direction[fIndex + 1], frag);
			up = Vector3.Lerp(Normal[fIndex], Normal[fIndex + 1], frag);
		}

		public void Interpolate(float f, float angleF, out Vector3 pos, out Vector3 dir, out Vector3 up)
		{
			Interpolate(f, out pos, out dir, out up);
			if (angleF != 0f)
			{
				Quaternion quaternion = Quaternion.AngleAxis(angleF * -360f, dir);
				up = quaternion * up;
			}
		}

		public Vector3 InterpolateDirection(float f)
		{
			float frag;
			int fIndex = GetFIndex(f, out frag);
			return Vector3.Lerp(Direction[fIndex], Direction[fIndex + 1], frag);
		}

		public Vector3 InterpolateUp(float f)
		{
			float frag;
			int fIndex = GetFIndex(f, out frag);
			return Vector3.Lerp(Normal[fIndex], Normal[fIndex + 1], frag);
		}

		public override void Recalculate()
		{
			base.Recalculate();
			for (int i = 1; i < Count; i++)
			{
				Direction[i] = (Position[i] - Position[i - 1]).normalized;
			}
		}
	}
}

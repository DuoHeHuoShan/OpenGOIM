using System;
using UnityEngine;

namespace FluffyUnderware.DevTools
{
	[Serializable]
	public class TTransform
	{
		public Vector3 localPosition;

		public Quaternion localRotation;

		public Vector3 localScale;

		public Matrix4x4 localToWorldMatrix;

		public Quaternion rotation;

		public Vector3 position;

		public Vector3 up
		{
			get
			{
				return rotation * Vector3.up;
			}
		}

		public TTransform(Transform t)
		{
			FromTransform(t);
		}

		public void FromTransform(Transform t)
		{
			position = t.position;
			localPosition = t.localPosition;
			localRotation = t.localRotation;
			localScale = t.localScale;
			localToWorldMatrix = t.localToWorldMatrix;
			rotation = t.rotation;
		}

		public void ToTransform(Transform t, Space space = Space.Self)
		{
			if (space == Space.World)
			{
				t.position = position;
				t.rotation = rotation;
				t.localScale = localScale;
			}
			else
			{
				t.localPosition = localPosition;
				t.localRotation = localRotation;
				t.localScale = localScale;
			}
		}

		public bool Changed(Transform transform)
		{
			if (this == transform)
			{
				return false;
			}
			FromTransform(transform);
			return true;
		}

		public override int GetHashCode()
		{
			return localPosition.GetHashCode() ^ (localRotation.GetHashCode() << 2) ^ (localScale.GetHashCode() >> 2) ^ (rotation.GetHashCode() >> 1);
		}

		public override bool Equals(object other)
		{
			if (!(other is TTransform))
			{
				return false;
			}
			TTransform tTransform = (TTransform)other;
			return localPosition.Equals(tTransform.localPosition) && localRotation.Equals(tTransform.localRotation) && localScale.Equals(tTransform.localScale) && rotation.Equals(tTransform.rotation);
		}

		public static bool operator ==(TTransform a, Transform b)
		{
			return a.localPosition == b.localPosition && a.localRotation == b.localRotation && a.localScale == b.localScale && a.rotation == b.rotation;
		}

		public static bool operator ==(TTransform a, TTransform b)
		{
			return a.localPosition == b.localPosition && a.localRotation == b.localRotation && a.localScale == b.localScale && a.rotation == b.rotation;
		}

		public static bool operator !=(TTransform a, Transform b)
		{
			return a.localPosition != b.localPosition || a.localRotation != b.localRotation || a.localScale != b.localScale || a.rotation != b.rotation;
		}

		public static bool operator !=(TTransform a, TTransform b)
		{
			return a.localPosition != b.localPosition || a.localRotation != b.localRotation || a.localScale != b.localScale || a.rotation != b.rotation;
		}

		public static implicit operator bool(TTransform a)
		{
			return !object.ReferenceEquals(a, null);
		}
	}
}

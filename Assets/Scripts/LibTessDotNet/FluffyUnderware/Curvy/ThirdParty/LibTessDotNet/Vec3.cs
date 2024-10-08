using System;

namespace FluffyUnderware.Curvy.ThirdParty.LibTessDotNet
{
	public struct Vec3
	{
		public static readonly Vec3 Zero = default(Vec3);

		public float X;

		public float Y;

		public float Z;

		public float this[int index]
		{
			get
			{
				switch (index)
				{
				case 0:
					return X;
				case 1:
					return Y;
				case 2:
					return Z;
				default:
					throw new IndexOutOfRangeException();
				}
			}
			set
			{
				switch (index)
				{
				case 0:
					X = value;
					break;
				case 1:
					Y = value;
					break;
				case 2:
					Z = value;
					break;
				default:
					throw new IndexOutOfRangeException();
				}
			}
		}

		public static void Sub(ref Vec3 lhs, ref Vec3 rhs, out Vec3 result)
		{
			result.X = lhs.X - rhs.X;
			result.Y = lhs.Y - rhs.Y;
			result.Z = lhs.Z - rhs.Z;
		}

		public static void Neg(ref Vec3 v)
		{
			v.X = 0f - v.X;
			v.Y = 0f - v.Y;
			v.Z = 0f - v.Z;
		}

		public static void Dot(ref Vec3 u, ref Vec3 v, out float dot)
		{
			dot = u.X * v.X + u.Y * v.Y + u.Z * v.Z;
		}

		public static void Normalize(ref Vec3 v)
		{
			float num = v.X * v.X + v.Y * v.Y + v.Z * v.Z;
			num = 1f / (float)Math.Sqrt(num);
			v.X *= num;
			v.Y *= num;
			v.Z *= num;
		}

		public static int LongAxis(ref Vec3 v)
		{
			int num = 0;
			if (Math.Abs(v.Y) > Math.Abs(v.X))
			{
				num = 1;
			}
			if (Math.Abs(v.Z) > Math.Abs((num != 0) ? v.Y : v.X))
			{
				num = 2;
			}
			return num;
		}

		public override string ToString()
		{
			return string.Format("{0}, {1}, {2}", X, Y, Z);
		}
	}
}

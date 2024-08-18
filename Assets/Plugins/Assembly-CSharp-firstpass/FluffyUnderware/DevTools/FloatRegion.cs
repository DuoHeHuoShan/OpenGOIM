using System;
using UnityEngine;

namespace FluffyUnderware.DevTools
{
	[Serializable]
	public struct FloatRegion
	{
		public float From;

		public float To;

		public bool SimpleValue;

		public static FloatRegion ZeroOne
		{
			get
			{
				return new FloatRegion(0f, 1f);
			}
		}

		public bool Positive
		{
			get
			{
				return From <= To;
			}
		}

		public float Low
		{
			get
			{
				return (!Positive) ? To : From;
			}
			set
			{
				if (Positive)
				{
					From = value;
				}
				else
				{
					To = value;
				}
			}
		}

		public float High
		{
			get
			{
				return (!Positive) ? From : To;
			}
			set
			{
				if (Positive)
				{
					To = value;
				}
				else
				{
					From = value;
				}
			}
		}

		public float Random
		{
			get
			{
				return UnityEngine.Random.Range(From, To);
			}
		}

		public float Next
		{
			get
			{
				if (SimpleValue)
				{
					return From;
				}
				return Random;
			}
		}

		public float Length
		{
			get
			{
				return To - From;
			}
		}

		public float LengthPositive
		{
			get
			{
				return (!Positive) ? (From - To) : (To - From);
			}
		}

		public FloatRegion(float value)
		{
			From = value;
			To = value;
			SimpleValue = true;
		}

		public FloatRegion(float A, float B)
		{
			From = A;
			To = B;
			SimpleValue = false;
		}

		public void MakePositive()
		{
			if (To < From)
			{
				float to = To;
				To = From;
				From = to;
			}
		}

		public void Clamp(float low, float high)
		{
			Low = Mathf.Clamp(Low, low, high);
			High = Mathf.Clamp(High, low, high);
		}

		public override string ToString()
		{
			return string.Format("({0:F1}-{1:F1})", From, To);
		}

		public override int GetHashCode()
		{
			return From.GetHashCode() ^ (To.GetHashCode() << 2);
		}

		public override bool Equals(object other)
		{
			if (!(other is FloatRegion))
			{
				return false;
			}
			FloatRegion floatRegion = (FloatRegion)other;
			return From.Equals(floatRegion.From) && To.Equals(floatRegion.To);
		}

		public static FloatRegion operator +(FloatRegion a, FloatRegion b)
		{
			return new FloatRegion(a.From + b.From, a.To + b.To);
		}

		public static FloatRegion operator -(FloatRegion a, FloatRegion b)
		{
			return new FloatRegion(a.From - b.From, a.To - b.To);
		}

		public static FloatRegion operator -(FloatRegion a)
		{
			return new FloatRegion(0f - a.From, 0f - a.To);
		}

		public static FloatRegion operator *(FloatRegion a, float v)
		{
			return new FloatRegion(a.From * v, a.To * v);
		}

		public static FloatRegion operator *(float v, FloatRegion a)
		{
			return new FloatRegion(a.From * v, a.To * v);
		}

		public static FloatRegion operator /(FloatRegion a, float v)
		{
			return new FloatRegion(a.From / v, a.To / v);
		}

		public static bool operator ==(FloatRegion lhs, FloatRegion rhs)
		{
			return lhs.SimpleValue == rhs.SimpleValue && Mathf.Approximately(lhs.From, rhs.From) && Mathf.Approximately(lhs.To, rhs.To);
		}

		public static bool operator !=(FloatRegion lhs, FloatRegion rhs)
		{
			return lhs.SimpleValue != rhs.SimpleValue || !Mathf.Approximately(lhs.From, rhs.From) || !Mathf.Approximately(lhs.To, rhs.To);
		}
	}
}

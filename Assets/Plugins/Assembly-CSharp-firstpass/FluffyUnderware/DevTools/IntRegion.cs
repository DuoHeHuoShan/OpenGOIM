using System;
using UnityEngine;

namespace FluffyUnderware.DevTools
{
	[Serializable]
	public struct IntRegion
	{
		public int From;

		public int To;

		public bool SimpleValue;

		public static IntRegion ZeroOne
		{
			get
			{
				return new IntRegion(0, 1);
			}
		}

		public bool Positive
		{
			get
			{
				return From <= To;
			}
		}

		public int Low
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

		public int High
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

		public int Random
		{
			get
			{
				return UnityEngine.Random.Range(From, To);
			}
		}

		public int Length
		{
			get
			{
				return To - From;
			}
		}

		public int LengthPositive
		{
			get
			{
				return (!Positive) ? (From - To) : (To - From);
			}
		}

		public IntRegion(int value)
		{
			From = value;
			To = value;
			SimpleValue = true;
		}

		public IntRegion(int A, int B)
		{
			From = A;
			To = B;
			SimpleValue = false;
		}

		public void MakePositive()
		{
			if (To < From)
			{
				int to = To;
				To = From;
				From = to;
			}
		}

		public void Clamp(int low, int high)
		{
			Low = Mathf.Clamp(Low, low, high);
			High = Mathf.Clamp(High, low, high);
		}

		public override string ToString()
		{
			return string.Format("({0}-{1})", From, To);
		}

		public override int GetHashCode()
		{
			return From.GetHashCode() ^ (To.GetHashCode() << 2);
		}

		public override bool Equals(object other)
		{
			if (!(other is IntRegion))
			{
				return false;
			}
			IntRegion intRegion = (IntRegion)other;
			return From.Equals(intRegion.From) && To.Equals(intRegion.To);
		}

		public static IntRegion operator +(IntRegion a, IntRegion b)
		{
			return new IntRegion(a.From + b.From, a.To + b.To);
		}

		public static IntRegion operator -(IntRegion a, IntRegion b)
		{
			return new IntRegion(a.From - b.From, a.To - b.To);
		}

		public static IntRegion operator -(IntRegion a)
		{
			return new IntRegion(-a.From, -a.To);
		}

		public static IntRegion operator *(IntRegion a, int v)
		{
			return new IntRegion(a.From * v, a.To * v);
		}

		public static IntRegion operator *(int v, IntRegion a)
		{
			return new IntRegion(a.From * v, a.To * v);
		}

		public static IntRegion operator /(IntRegion a, int v)
		{
			return new IntRegion(a.From / v, a.To / v);
		}

		public static bool operator ==(IntRegion lhs, IntRegion rhs)
		{
			return lhs.From == rhs.From && lhs.To == rhs.To && lhs.SimpleValue != rhs.SimpleValue;
		}

		public static bool operator !=(IntRegion lhs, IntRegion rhs)
		{
			return lhs.From != rhs.From || lhs.To != rhs.To || lhs.SimpleValue != rhs.SimpleValue;
		}
	}
}

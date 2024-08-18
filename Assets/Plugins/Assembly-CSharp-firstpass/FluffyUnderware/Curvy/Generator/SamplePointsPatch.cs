using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	public struct SamplePointsPatch
	{
		public int Start;

		public int Count;

		public int End
		{
			get
			{
				return Start + Count;
			}
			set
			{
				Count = Mathf.Max(0, value - Start);
			}
		}

		public int TriangleCount
		{
			get
			{
				return Count * 2;
			}
		}

		public SamplePointsPatch(int start)
		{
			Start = start;
			Count = 0;
		}

		public override string ToString()
		{
			return string.Format("Size={0} ({1}-{2}, {3} Tris)", Count, Start, End, TriangleCount);
		}
	}
}

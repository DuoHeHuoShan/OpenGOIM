using System.Collections.Generic;

namespace FluffyUnderware.Curvy.Generator
{
	[CGDataInfo(0.96f, 0.96f, 0.96f, 1f)]
	public class CGSpots : CGData
	{
		public CGSpot[] Points;

		public override int Count
		{
			get
			{
				return Points.Length;
			}
		}

		public CGSpots()
		{
			Points = new CGSpot[0];
		}

		public CGSpots(params CGSpot[] points)
		{
			Points = points;
		}

		public CGSpots(params List<CGSpot>[] lists)
		{
			int num = 0;
			for (int i = 0; i < lists.Length; i++)
			{
				num += lists[i].Count;
			}
			Points = new CGSpot[num];
			num = 0;
			for (int j = 0; j < lists.Length; j++)
			{
				lists[j].CopyTo(Points, num);
				num += lists[j].Count;
			}
		}

		public CGSpots(CGSpots source)
		{
			Points = source.Points;
		}

		public override T Clone<T>()
		{
			return new CGSpots(this) as T;
		}
	}
}

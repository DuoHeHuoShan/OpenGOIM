using System.Collections.Generic;

namespace FluffyUnderware.Curvy.Generator
{
	public class SamplePointsMaterialGroup
	{
		public int MaterialID;

		public List<SamplePointsPatch> Patches = new List<SamplePointsPatch>();

		public int TriangleCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < Patches.Count; i++)
				{
					num += Patches[i].TriangleCount;
				}
				return num;
			}
		}

		public int StartVertex
		{
			get
			{
				return Patches[0].Start;
			}
		}

		public int EndVertex
		{
			get
			{
				return Patches[Patches.Count - 1].End;
			}
		}

		public int VertexCount
		{
			get
			{
				return EndVertex - StartVertex + 1;
			}
		}

		public SamplePointsMaterialGroup(int materialID)
		{
			MaterialID = materialID;
		}

		public void GetLengths(CGVolume volume, out float worldLength, out float uLength)
		{
			worldLength = 0f;
			for (int i = StartVertex; i < EndVertex; i++)
			{
				worldLength += (volume.Vertex[i + 1] - volume.Vertex[i]).magnitude;
			}
			uLength = volume.CrossMap[EndVertex] - volume.CrossMap[StartVertex];
		}
	}
}

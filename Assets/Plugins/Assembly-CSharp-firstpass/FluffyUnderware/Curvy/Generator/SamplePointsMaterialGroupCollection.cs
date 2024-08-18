using System.Collections.Generic;

namespace FluffyUnderware.Curvy.Generator
{
	public class SamplePointsMaterialGroupCollection : List<SamplePointsMaterialGroup>
	{
		public int MaterialID;

		public float AspectCorrection = 1f;

		public int TriangleCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < base.Count; i++)
				{
					num += base[i].TriangleCount;
				}
				return num;
			}
		}

		public SamplePointsMaterialGroupCollection()
		{
		}

		public SamplePointsMaterialGroupCollection(int capacity)
			: base(capacity)
		{
		}

		public SamplePointsMaterialGroupCollection(IEnumerable<SamplePointsMaterialGroup> collection)
			: base(collection)
		{
		}

		public void CalculateAspectCorrection(CGVolume volume, CGMaterialSettingsEx matSettings)
		{
			if (matSettings.KeepAspect == CGKeepAspectMode.Off)
			{
				AspectCorrection = 1f;
				return;
			}
			float num = 0f;
			float num2 = 0f;
			for (int i = 0; i < base.Count; i++)
			{
				float worldLength;
				float uLength;
				base[i].GetLengths(volume, out worldLength, out uLength);
				num += worldLength;
				num2 += uLength;
			}
			AspectCorrection = volume.Length / (num / num2);
		}
	}
}

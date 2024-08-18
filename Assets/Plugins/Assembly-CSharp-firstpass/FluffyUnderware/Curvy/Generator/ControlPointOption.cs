namespace FluffyUnderware.Curvy.Generator
{
	public struct ControlPointOption
	{
		public float TF;

		public float Distance;

		public bool Include;

		public int MaterialID;

		public bool HardEdge;

		public float MaxStepDistance;

		public bool UVEdge;

		public bool UVShift;

		public float FirstU;

		public float SecondU;

		public ControlPointOption(float tf, float dist, bool includeAnyways, int materialID, bool hardEdge, float maxStepDistance, bool uvEdge, bool uvShift, float firstU, float secondU)
		{
			TF = tf;
			Distance = dist;
			Include = includeAnyways;
			MaterialID = materialID;
			HardEdge = hardEdge;
			if (maxStepDistance == 0f)
			{
				MaxStepDistance = float.MaxValue;
			}
			else
			{
				MaxStepDistance = maxStepDistance;
			}
			UVEdge = uvEdge;
			UVShift = uvShift;
			FirstU = firstU;
			SecondU = secondU;
		}
	}
}

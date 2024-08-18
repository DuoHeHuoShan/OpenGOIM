namespace FluffyUnderware.Curvy.Generator
{
	public struct SamplePointUData
	{
		public int Vertex;

		public bool UVEdge;

		public float FirstU;

		public float SecondU;

		public SamplePointUData(int vt, bool uvEdge, float uv0, float uv1)
		{
			Vertex = vt;
			UVEdge = uvEdge;
			FirstU = uv0;
			SecondU = uv1;
		}

		public override string ToString()
		{
			return string.Format("SamplePointUData (Vertex={0},Edge={1},FirstU={2},SecondU={3}", Vertex, UVEdge, FirstU, SecondU);
		}
	}
}

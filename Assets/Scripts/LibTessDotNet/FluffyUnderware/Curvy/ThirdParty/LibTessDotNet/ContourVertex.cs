namespace FluffyUnderware.Curvy.ThirdParty.LibTessDotNet
{
	public struct ContourVertex
	{
		public Vec3 Position;

		public object Data;

		public override string ToString()
		{
			return string.Format("{0}, {1}", Position, Data);
		}
	}
}

namespace FluffyUnderware.DevTools
{
	public class AsGroupAttribute : GroupAttribute, IDTFieldParsingAttribute, IDTFieldRenderAttribute
	{
		public AsGroupAttribute(string pathAndName = null)
			: base(pathAndName)
		{
			base.TypeSort = 10;
		}
	}
}

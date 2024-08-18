namespace FluffyUnderware.DevTools
{
	public class SortOrderAttribute : DTAttribute, IDTFieldParsingAttribute
	{
		public SortOrderAttribute(int sort = 100)
			: base(0)
		{
			Sort = sort;
		}
	}
}

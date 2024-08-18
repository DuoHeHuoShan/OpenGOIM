namespace FluffyUnderware.DevTools
{
	public class SectionAttribute : GroupAttribute
	{
		public bool Fixed;

		public SectionAttribute(string name, bool expanded = true, bool fix = false, int sort = 100)
			: base(name)
		{
			Expanded = expanded;
			base.TypeSort = 10;
			Sort = sort;
			Fixed = fix;
		}
	}
}

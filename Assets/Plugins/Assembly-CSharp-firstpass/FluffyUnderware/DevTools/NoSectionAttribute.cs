namespace FluffyUnderware.DevTools
{
	public class NoSectionAttribute : SectionAttribute
	{
		public NoSectionAttribute()
			: base(string.Empty)
		{
			base.TypeSort = 10;
		}
	}
}

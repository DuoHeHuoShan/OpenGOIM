namespace FluffyUnderware.DevTools
{
	public class DTRegionAttribute : DTPropertyAttribute
	{
		public bool RegionIsOptional;

		public string RegionOptionsPropertyName;

		public bool UseSlider = true;

		public DTRegionAttribute()
			: base(string.Empty, string.Empty)
		{
		}
	}
}

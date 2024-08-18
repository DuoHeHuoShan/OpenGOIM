namespace FluffyUnderware.DevTools
{
	public struct RegionOptions<T>
	{
		public string LabelFrom;

		public string LabelTo;

		public string OptionalTooltip;

		public DTValueClamping ClampFrom;

		public DTValueClamping ClampTo;

		public T FromMin;

		public T FromMax;

		public T ToMin;

		public T ToMax;

		public static RegionOptions<T> Default
		{
			get
			{
				RegionOptions<T> result = default(RegionOptions<T>);
				result.OptionalTooltip = "Range";
				result.LabelFrom = "From";
				result.LabelTo = "To";
				result.ClampFrom = DTValueClamping.None;
				result.ClampTo = DTValueClamping.None;
				return result;
			}
		}

		public static RegionOptions<T> MinMax(T min, T max)
		{
			RegionOptions<T> result = default(RegionOptions<T>);
			result.LabelFrom = "From";
			result.LabelTo = "To";
			result.ClampFrom = DTValueClamping.Range;
			result.ClampTo = DTValueClamping.Range;
			result.FromMin = min;
			result.FromMax = max;
			result.ToMin = min;
			result.ToMax = max;
			return result;
		}
	}
}

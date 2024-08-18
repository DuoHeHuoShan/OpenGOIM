namespace FluffyUnderware.Curvy
{
	public interface ICurvyInterpolatableMetadata : ICurvyMetadata
	{
		object Value { get; }

		object InterpolateObject(ICurvyMetadata b, float f);
	}
	public interface ICurvyInterpolatableMetadata<U> : ICurvyInterpolatableMetadata, ICurvyMetadata
	{
		U Interpolate(ICurvyMetadata b, float f);
	}
}
